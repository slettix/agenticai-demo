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
    options.AddPolicy("RequireManageRoles", policy => 
        policy.RequireClaim("permission", "manage_roles"));
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder
            .WithOrigins(
                "http://localhost:3000", 
                "http://127.0.0.1:3000"
            ) // React dev server
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
builder.Services.AddScoped<IRoleService, RoleService>();

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
        // Check if database exists and ensure it's created if not
        if (!context.Database.CanConnect())
        {
            Log.Information("Database doesn't exist, creating it...");
            context.Database.EnsureCreated();
            Log.Information("Database created successfully");
        }
        else
        {
            // Apply pending migrations
            var pendingMigrations = context.Database.GetPendingMigrations();
            if (pendingMigrations.Any())
            {
                Log.Information("Applying {Count} pending migrations: {Migrations}", 
                    pendingMigrations.Count(), string.Join(", ", pendingMigrations));
                
                try 
                {
                    context.Database.Migrate();
                    Log.Information("Database schema updated successfully");
                }
                catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P07") // Table already exists
                {
                    Log.Warning("Some tables already exist, attempting to mark migrations as applied");
                    // If tables already exist but migrations aren't recorded, we'll use EnsureCreated approach
                    context.Database.EnsureCreated();
                    Log.Information("Database schema ensured");
                }
            }
            else
            {
                Log.Information("Database schema is up to date");
            }
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
            
            // Create Actors table if it doesn't exist
            await context.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""Actors"" (
                    ""Id"" serial PRIMARY KEY,
                    ""ActorCategory"" integer NOT NULL,
                    ""ActorType"" integer NOT NULL,
                    ""FirstName"" varchar(100),
                    ""LastName"" varchar(100),
                    ""Email"" varchar(255),
                    ""Phone"" varchar(20),
                    ""OrganizationName"" varchar(200),
                    ""RegistrationNumber"" varchar(50),
                    ""ParentOrganization"" varchar(200),
                    ""EmployeeCount"" integer,
                    ""UnitName"" varchar(200),
                    ""UnitType"" varchar(100),
                    ""UnitCode"" varchar(50),
                    ""CommandStructure"" varchar(100),
                    ""UnitMission"" varchar(500),
                    ""PersonnelCount"" integer,
                    ""Department"" varchar(200),
                    ""Position"" varchar(100),
                    ""ManagerName"" varchar(200),
                    ""ManagerEmail"" varchar(255),
                    ""GeographicLocation"" varchar(300),
                    ""Address"" varchar(300),
                    ""PreferredLanguage"" varchar(10) DEFAULT 'NO',
                    ""ContractNumber"" varchar(100),
                    ""ContractStartDate"" timestamp with time zone,
                    ""ContractEndDate"" timestamp with time zone,
                    ""VendorId"" varchar(100),
                    ""SecurityClearance"" integer NOT NULL DEFAULT 0,
                    ""IsActive"" boolean NOT NULL DEFAULT TRUE,
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT NOW(),
                    ""CreatedByUserId"" integer,
                    ""UpdatedAt"" timestamp with time zone,
                    ""UpdatedByUserId"" integer,
                    FOREIGN KEY (""CreatedByUserId"") REFERENCES ""Users""(""Id"") ON DELETE SET NULL,
                    FOREIGN KEY (""UpdatedByUserId"") REFERENCES ""Users""(""Id"") ON DELETE SET NULL
                );
            ");

            // Add missing columns to existing Actors table
            await context.Database.ExecuteSqlRawAsync(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                  WHERE table_name = 'Actors' AND column_name = 'ManagerName') THEN
                        ALTER TABLE ""Actors"" ADD COLUMN ""ManagerName"" varchar(200);
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                  WHERE table_name = 'Actors' AND column_name = 'ManagerEmail') THEN
                        ALTER TABLE ""Actors"" ADD COLUMN ""ManagerEmail"" varchar(255);
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                  WHERE table_name = 'Actors' AND column_name = 'GeographicLocation') THEN
                        ALTER TABLE ""Actors"" ADD COLUMN ""GeographicLocation"" varchar(300);
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                  WHERE table_name = 'Actors' AND column_name = 'Address') THEN
                        ALTER TABLE ""Actors"" ADD COLUMN ""Address"" varchar(300);
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                  WHERE table_name = 'Actors' AND column_name = 'PreferredLanguage') THEN
                        ALTER TABLE ""Actors"" ADD COLUMN ""PreferredLanguage"" varchar(10) DEFAULT 'NO';
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                  WHERE table_name = 'Actors' AND column_name = 'ContractNumber') THEN
                        ALTER TABLE ""Actors"" ADD COLUMN ""ContractNumber"" varchar(100);
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                  WHERE table_name = 'Actors' AND column_name = 'ContractStartDate') THEN
                        ALTER TABLE ""Actors"" ADD COLUMN ""ContractStartDate"" timestamp with time zone;
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                  WHERE table_name = 'Actors' AND column_name = 'ContractEndDate') THEN
                        ALTER TABLE ""Actors"" ADD COLUMN ""ContractEndDate"" timestamp with time zone;
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                  WHERE table_name = 'Actors' AND column_name = 'VendorId') THEN
                        ALTER TABLE ""Actors"" ADD COLUMN ""VendorId"" varchar(100);
                    END IF;
                END $$;
            ");

            // Create indexes for Actors table
            await context.Database.ExecuteSqlRawAsync(@"
                CREATE INDEX IF NOT EXISTS ""IX_Actors_ActorCategory"" ON ""Actors"" (""ActorCategory"");
                CREATE INDEX IF NOT EXISTS ""IX_Actors_ActorType"" ON ""Actors"" (""ActorType"");
                CREATE INDEX IF NOT EXISTS ""IX_Actors_Email"" ON ""Actors"" (""Email"");
                CREATE INDEX IF NOT EXISTS ""IX_Actors_IsActive"" ON ""Actors"" (""IsActive"");
                CREATE INDEX IF NOT EXISTS ""IX_Actors_OrganizationName"" ON ""Actors"" (""OrganizationName"");
                CREATE INDEX IF NOT EXISTS ""IX_Actors_UnitName"" ON ""Actors"" (""UnitName"");
                CREATE INDEX IF NOT EXISTS ""IX_Actors_SecurityClearance"" ON ""Actors"" (""SecurityClearance"");
            ");

            // Create ActorRoles table if it doesn't exist
            await context.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""ActorRoles"" (
                    ""Id"" serial PRIMARY KEY,
                    ""ActorId"" integer NOT NULL,
                    ""RoleId"" integer NOT NULL,
                    ""AssignedAt"" timestamp with time zone NOT NULL DEFAULT NOW(),
                    ""AssignedByUserId"" integer NOT NULL,
                    ""ValidFrom"" timestamp with time zone,
                    ""ValidTo"" timestamp with time zone,
                    ""IsActive"" boolean NOT NULL DEFAULT TRUE,
                    ""Notes"" text,
                    FOREIGN KEY (""ActorId"") REFERENCES ""Actors""(""Id"") ON DELETE CASCADE,
                    FOREIGN KEY (""RoleId"") REFERENCES ""Roles""(""Id"") ON DELETE CASCADE,
                    FOREIGN KEY (""AssignedByUserId"") REFERENCES ""Users""(""Id"") ON DELETE RESTRICT
                );
                CREATE INDEX IF NOT EXISTS ""IX_ActorRoles_ActorId"" ON ""ActorRoles"" (""ActorId"");
                CREATE INDEX IF NOT EXISTS ""IX_ActorRoles_RoleId"" ON ""ActorRoles"" (""RoleId"");
            ");

            // Add missing columns to existing ActorRoles table
            await context.Database.ExecuteSqlRawAsync(@"
                DO $$ 
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                  WHERE table_name = 'ActorRoles' AND column_name = 'AssignedByUserId') THEN
                        ALTER TABLE ""ActorRoles"" ADD COLUMN ""AssignedByUserId"" integer NOT NULL DEFAULT 1;
                        -- Add foreign key constraint
                        ALTER TABLE ""ActorRoles"" ADD CONSTRAINT ""FK_ActorRoles_Users_AssignedByUserId"" 
                            FOREIGN KEY (""AssignedByUserId"") REFERENCES ""Users""(""Id"") ON DELETE RESTRICT;
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                  WHERE table_name = 'ActorRoles' AND column_name = 'ValidFrom') THEN
                        ALTER TABLE ""ActorRoles"" ADD COLUMN ""ValidFrom"" timestamp with time zone;
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                  WHERE table_name = 'ActorRoles' AND column_name = 'ValidTo') THEN
                        ALTER TABLE ""ActorRoles"" ADD COLUMN ""ValidTo"" timestamp with time zone;
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                  WHERE table_name = 'ActorRoles' AND column_name = 'IsActive') THEN
                        ALTER TABLE ""ActorRoles"" ADD COLUMN ""IsActive"" boolean NOT NULL DEFAULT TRUE;
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                  WHERE table_name = 'ActorRoles' AND column_name = 'Notes') THEN
                        ALTER TABLE ""ActorRoles"" ADD COLUMN ""Notes"" text;
                    END IF;
                END $$;
            ");

            // Create ActorNotes table if it doesn't exist
            await context.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""ActorNotes"" (
                    ""Id"" serial PRIMARY KEY,
                    ""ActorId"" integer NOT NULL,
                    ""Note"" text NOT NULL,
                    ""Category"" varchar(100),
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT NOW(),
                    ""CreatedByUserId"" integer NOT NULL,
                    ""IsPrivate"" boolean NOT NULL DEFAULT FALSE,
                    FOREIGN KEY (""ActorId"") REFERENCES ""Actors""(""Id"") ON DELETE CASCADE,
                    FOREIGN KEY (""CreatedByUserId"") REFERENCES ""Users""(""Id"") ON DELETE RESTRICT
                );
                CREATE INDEX IF NOT EXISTS ""IX_ActorNotes_ActorId"" ON ""ActorNotes"" (""ActorId"");
                CREATE INDEX IF NOT EXISTS ""IX_ActorNotes_CreatedByUserId"" ON ""ActorNotes"" (""CreatedByUserId"");
            ");

            // Add missing columns to existing ActorNotes table
            await context.Database.ExecuteSqlRawAsync(@"
                DO $$ 
                BEGIN
                    -- Rename Content to Note if Content exists and Note doesn't
                    IF EXISTS (SELECT 1 FROM information_schema.columns 
                              WHERE table_name = 'ActorNotes' AND column_name = 'Content') 
                       AND NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                      WHERE table_name = 'ActorNotes' AND column_name = 'Note') THEN
                        ALTER TABLE ""ActorNotes"" RENAME COLUMN ""Content"" TO ""Note"";
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                  WHERE table_name = 'ActorNotes' AND column_name = 'Category') THEN
                        ALTER TABLE ""ActorNotes"" ADD COLUMN ""Category"" varchar(100);
                    END IF;
                    
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                  WHERE table_name = 'ActorNotes' AND column_name = 'IsPrivate') THEN
                        ALTER TABLE ""ActorNotes"" ADD COLUMN ""IsPrivate"" boolean NOT NULL DEFAULT FALSE;
                    END IF;
                END $$;
            ");
            
            Log.Information("Database schema updated with deletion fields and Actor tables");
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