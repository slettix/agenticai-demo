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
    if (string.IsNullOrEmpty(connectionString))
    {
        // Use in-memory database for development
        options.UseInMemoryDatabase("ProsessPortalDb");
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
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

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProsessPortalDbContext>();
    context.Database.EnsureCreated();
    
    // Seed admin user if not exists
    if (!context.Users.Any())
    {
        var adminRole = context.Roles.First(r => r.Name == "Admin");
        var adminUser = new ProsessPortal.Core.Entities.User
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
        
        Log.Information("Created default admin user: admin/admin123");
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