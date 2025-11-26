using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProsessPortal.Core.Interfaces;
using ProsessPortal.Infrastructure.Data;
using ProsessPortal.Infrastructure.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/prosessportal-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger configuration with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProsessPortal API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database
builder.Services.AddDbContext<ProsessPortalDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Always use PostgreSQL
    options.UseNpgsql(connectionString);
});

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "default-secret-key-for-development-only-please-change-in-production";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "ProsessPortal",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "ProsessPortal",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireCreateProsess", policy => 
        policy.RequireClaim("permission", "create_prosess"));
    options.AddPolicy("RequireEditProsess", policy => 
        policy.RequireClaim("permission", "edit_prosess"));
    options.AddPolicy("RequireDeleteProsess", policy => 
        policy.RequireClaim("permission", "delete_prosess"));
    options.AddPolicy("RequireApproveProsess", policy => 
        policy.RequireClaim("permission", "approve_prosess"));
    options.AddPolicy("RequireViewQA", policy => 
        policy.RequireClaim("permission", "view_qa_queue"));
    options.AddPolicy("RequireAdminRole", policy => 
        policy.RequireRole("Admin"));
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder
            .WithOrigins("http://localhost:3000") // React dev server
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IProsessService, ProsessService>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<IEditingService, EditingService>();
builder.Services.AddScoped<IDeletionService, DeletionService>();
builder.Services.AddScoped<IActorService, ActorService>();

// HTTP client for agent service
builder.Services.AddHttpClient<IAgentService, AgentService>(client =>
{
    var agentServiceUrl = builder.Configuration["AgentService:BaseUrl"] ?? "http://localhost:8001";
    client.BaseAddress = new Uri(agentServiceUrl);
    client.Timeout = TimeSpan.FromMinutes(5); // Allow long-running AI operations
});
builder.Services.AddScoped<IAgentService, AgentService>();

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProsessPortalDbContext>();
    
    try 
    {
        // Create database schema - use EnsureCreated for now
        bool created = context.Database.EnsureCreated();
        if (created)
        {
            Log.Information("Database schema created successfully");
        }
        else
        {
            Log.Information("Database schema already exists");
        }
        
        // Add new deletion columns if they don't exist
        try
        {
            await context.Database.ExecuteSqlRawAsync(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                  WHERE table_name = 'Prosesser' AND column_name = 'IsDeleted') THEN
                        ALTER TABLE ""Prosesser"" ADD COLUMN ""IsDeleted"" boolean NOT NULL DEFAULT FALSE;
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                  WHERE table_name = 'Prosesser' AND column_name = 'DeletedAt') THEN
                        ALTER TABLE ""Prosesser"" ADD COLUMN ""DeletedAt"" timestamp with time zone NULL;
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                  WHERE table_name = 'Prosesser' AND column_name = 'DeletedByUserId') THEN
                        ALTER TABLE ""Prosesser"" ADD COLUMN ""DeletedByUserId"" integer NULL;
                    END IF;
                END $$;
            ");
            
            // Create ProsessDeletionHistory table if it doesn't exist
            await context.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""ProsessDeletionHistory"" (
                    ""Id"" serial PRIMARY KEY,
                    ""ProsessId"" integer NOT NULL,
                    ""UserId"" integer NOT NULL,
                    ""Action"" varchar(50) NOT NULL,
                    ""Reason"" varchar(1000),
                    ""ActionAt"" timestamp with time zone NOT NULL DEFAULT NOW(),
                    FOREIGN KEY (""ProsessId"") REFERENCES ""Prosesser""(""Id"") ON DELETE CASCADE,
                    FOREIGN KEY (""UserId"") REFERENCES ""Users""(""Id"") ON DELETE RESTRICT
                );
            ");
            
            Log.Information("Database schema updated with deletion fields");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Could not update database schema - this may be expected if columns already exist");
        }

        // Ensure admin user exists with correct password hash
        var adminUser = context.Users.FirstOrDefault(u => u.Username == "admin");
        if (adminUser == null)
        {
            var adminRole = context.Roles.First(r => r.Name == "Admin");
            adminUser = new ProsessPortal.Core.Entities.User
            {
                Username = "admin",
                Email = "admin@prosessportal.no",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                FirstName = "System",
                LastName = "Administrator",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            context.Users.Add(adminUser);
            context.SaveChanges();
            
            context.UserRoles.Add(new ProsessPortal.Core.Entities.UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id,
                AssignedAt = DateTime.UtcNow
            });
            context.SaveChanges();
            
            Log.Information("Created admin user with correct password hash");
        }
        else
        {
            // Update password hash if it's the old incorrect one
            var testPassword = "admin123";
            if (!BCrypt.Net.BCrypt.Verify(testPassword, adminUser.PasswordHash))
            {
                adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(testPassword);
                context.SaveChanges();
                Log.Information("Updated admin user password hash");
            }
            else
            {
                Log.Information("Admin user password hash is correct");
            }
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to initialize database");
        throw;
    }
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();