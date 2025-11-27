using Microsoft.EntityFrameworkCore;
using ProsessPortal.Core.Entities;

namespace ProsessPortal.Infrastructure.Data;

public class ProsessPortalDbContext : DbContext
{
    public ProsessPortalDbContext(DbContextOptions<ProsessPortalDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    
    // Actor entities
    public DbSet<Actor> Actors { get; set; }
    public DbSet<ActorRole> ActorRoles { get; set; }
    public DbSet<ActorNote> ActorNotes { get; set; }
    
    // Process entities
    public DbSet<Prosess> Prosesser { get; set; }
    public DbSet<ProsessVersion> ProsessVersions { get; set; }
    public DbSet<ProsessStep> ProsessSteps { get; set; }
    public DbSet<StepConnection> StepConnections { get; set; }
    public DbSet<ProsessTag> ProsessTags { get; set; }
    
    // Approval entities
    public DbSet<ProsessApprovalRequest> ProsessApprovalRequests { get; set; }
    public DbSet<ProsessApprovalComment> ProsessApprovalComments { get; set; }
    public DbSet<ProsessApprovalHistory> ProsessApprovalHistory { get; set; }
    
    // Editing entities
    public DbSet<ProsessEditSession> ProsessEditSessions { get; set; }
    public DbSet<ProsessEditConflict> ProsessEditConflicts { get; set; }
    public DbSet<ProsessAutoSave> ProsessAutoSaves { get; set; }
    
    // Deletion entities
    public DbSet<ProsessDeletionHistory> ProsessDeletionHistory { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
        });
        
        // Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(200);
        });
        
        // Permission configuration
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Resource, e.Action }).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Resource).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
        });
        
        // UserRole many-to-many configuration
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.HasOne(e => e.User)
                .WithMany(e => e.UserRoles)
                .HasForeignKey(e => e.UserId);
            entity.HasOne(e => e.Role)
                .WithMany(e => e.UserRoles)
                .HasForeignKey(e => e.RoleId);
        });
        
        // RolePermission many-to-many configuration
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.PermissionId });
            entity.HasOne(e => e.Role)
                .WithMany(e => e.RolePermissions)
                .HasForeignKey(e => e.RoleId);
            entity.HasOne(e => e.Permission)
                .WithMany(e => e.RolePermissions)
                .HasForeignKey(e => e.PermissionId);
        });
        
        // Actor configuration
        modelBuilder.Entity<Actor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            
            // Personal information (nullable for organizations/units)
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            
            // Organization/Unit information
            entity.Property(e => e.OrganizationName).HasMaxLength(100);
            entity.Property(e => e.UnitName).HasMaxLength(100);
            entity.Property(e => e.UnitType).HasMaxLength(50);
            entity.Property(e => e.UnitCode).HasMaxLength(50);
            
            // Common fields
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.Position).HasMaxLength(100);
            entity.Property(e => e.ManagerName).HasMaxLength(100);
            entity.Property(e => e.ManagerEmail).HasMaxLength(100);
            entity.Property(e => e.GeographicLocation).HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.PreferredLanguage).HasMaxLength(10);
            entity.Property(e => e.ContractNumber).HasMaxLength(50);
            entity.Property(e => e.VendorId).HasMaxLength(50);
            
            // Organization-specific fields
            entity.Property(e => e.RegistrationNumber).HasMaxLength(50);
            entity.Property(e => e.ParentOrganization).HasMaxLength(100);
            
            // Unit-specific fields
            entity.Property(e => e.CommandStructure).HasMaxLength(100);
            entity.Property(e => e.UnitMission).HasMaxLength(500);
            
            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.UpdatedByUser)
                .WithMany()
                .HasForeignKey(e => e.UpdatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(e => e.ActorCategory);
            entity.HasIndex(e => e.ActorType);
            entity.HasIndex(e => e.OrganizationName);
            entity.HasIndex(e => e.UnitName);
            entity.HasIndex(e => e.SecurityClearance);
            entity.HasIndex(e => e.IsActive);
        });
        
        // ActorRole many-to-many configuration
        modelBuilder.Entity<ActorRole>(entity =>
        {
            entity.HasKey(e => new { e.ActorId, e.RoleId });
            entity.HasOne(e => e.Actor)
                .WithMany(e => e.ActorRoles)
                .HasForeignKey(e => e.ActorId);
            entity.HasOne(e => e.Role)
                .WithMany()
                .HasForeignKey(e => e.RoleId);
            entity.HasOne(e => e.AssignedByUser)
                .WithMany()
                .HasForeignKey(e => e.AssignedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.HasIndex(e => e.IsActive);
        });
        
        // ActorNote configuration
        modelBuilder.Entity<ActorNote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Note).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(50);
            
            entity.HasOne(e => e.Actor)
                .WithMany(e => e.Notes)
                .HasForeignKey(e => e.ActorId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(e => e.ActorId);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.CreatedAt);
        });
        
        // Seed default roles
        var fixedDateTime = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = RoleNames.Admin, Description = "Systemadministrator med full tilgang", CreatedAt = fixedDateTime },
            new Role { Id = 2, Name = RoleNames.ProsessEier, Description = "Eier av prosesser, kan redigere og godkjenne", CreatedAt = fixedDateTime },
            new Role { Id = 3, Name = RoleNames.QA, Description = "Kvalitetssikring, kan godkjenne endringer", CreatedAt = fixedDateTime },
            new Role { Id = 4, Name = RoleNames.SME, Description = "Fagekspert, kan foreslå endringer", CreatedAt = fixedDateTime },
            new Role { Id = 5, Name = RoleNames.Bruker, Description = "Vanlig bruker, kun lesetilgang", CreatedAt = fixedDateTime }
        );
        
        // Seed default permissions
        modelBuilder.Entity<Permission>().HasData(
            new Permission { Id = 1, Name = PermissionNames.ViewProsess, Description = "Se prosesser", Resource = "prosess", Action = "view" },
            new Permission { Id = 2, Name = PermissionNames.CreateProsess, Description = "Opprette prosesser", Resource = "prosess", Action = "create" },
            new Permission { Id = 3, Name = PermissionNames.EditProsess, Description = "Redigere prosesser", Resource = "prosess", Action = "edit" },
            new Permission { Id = 4, Name = PermissionNames.DeleteProsess, Description = "Slette prosesser", Resource = "prosess", Action = "delete" },
            new Permission { Id = 5, Name = PermissionNames.ApproveProsess, Description = "Godkjenne prosesser", Resource = "prosess", Action = "approve" },
            new Permission { Id = 6, Name = PermissionNames.ViewQAQueue, Description = "Se QA-kø", Resource = "qa", Action = "view" },
            new Permission { Id = 7, Name = PermissionNames.ApproveChanges, Description = "Godkjenne endringer", Resource = "qa", Action = "approve" },
            new Permission { Id = 8, Name = PermissionNames.RejectChanges, Description = "Avvise endringer", Resource = "qa", Action = "reject" },
            new Permission { Id = 9, Name = PermissionNames.ManageUsers, Description = "Administrere brukere", Resource = "user", Action = "manage" },
            new Permission { Id = 10, Name = PermissionNames.ManageRoles, Description = "Administrere roller", Resource = "role", Action = "manage" },
            new Permission { Id = 11, Name = PermissionNames.ViewAuditLog, Description = "Se audit-logg", Resource = "audit", Action = "view" }
        );
        
        // Prosess entity configuration
        modelBuilder.Entity<Prosess>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Category).HasMaxLength(50).IsRequired();
            entity.Property(e => e.GitRepository).HasMaxLength(500);
            entity.Property(e => e.GitPath).HasMaxLength(500);
            entity.Property(e => e.GitBranch).HasMaxLength(100);
            
            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.DeletedByUser)
                .WithMany()
                .HasForeignKey(e => e.DeletedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsDeleted);
        });
        
        // ProsessVersion configuration
        modelBuilder.Entity<ProsessVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VersionNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ChangeLog).HasMaxLength(2000);
            entity.Property(e => e.GitCommitHash).HasMaxLength(50);
            entity.Property(e => e.GitTag).HasMaxLength(100);
            
            entity.HasOne(e => e.Prosess)
                .WithMany(e => e.Versions)
                .HasForeignKey(e => e.ProsessId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.PublishedByUser)
                .WithMany()
                .HasForeignKey(e => e.PublishedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(e => new { e.ProsessId, e.VersionNumber }).IsUnique();
            entity.HasIndex(e => new { e.ProsessId, e.IsCurrent });
        });
        
        // ProsessStep configuration
        modelBuilder.Entity<ProsessStep>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ResponsibleRole).HasMaxLength(100);
            
            entity.HasOne(e => e.Prosess)
                .WithMany(e => e.Steps)
                .HasForeignKey(e => e.ProsessId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.ParentStep)
                .WithMany(e => e.SubSteps)
                .HasForeignKey(e => e.ParentStepId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(e => new { e.ProsessId, e.OrderIndex });
        });
        
        // StepConnection configuration
        modelBuilder.Entity<StepConnection>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Condition).HasMaxLength(500);
            
            entity.HasOne(e => e.FromStep)
                .WithMany(e => e.OutgoingConnections)
                .HasForeignKey(e => e.FromStepId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.ToStep)
                .WithMany(e => e.IncomingConnections)
                .HasForeignKey(e => e.ToStepId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // ProsessTag configuration
        modelBuilder.Entity<ProsessTag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Color).HasMaxLength(10);
            
            entity.HasOne(e => e.Prosess)
                .WithMany(e => e.Tags)
                .HasForeignKey(e => e.ProsessId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(e => new { e.ProsessId, e.Name }).IsUnique();
        });
        
        // Seed role-permission mappings
        modelBuilder.Entity<RolePermission>().HasData(
            // Admin - all permissions
            new RolePermission { RoleId = 1, PermissionId = 1 },   // view_prosess
            new RolePermission { RoleId = 1, PermissionId = 2 },   // create_prosess
            new RolePermission { RoleId = 1, PermissionId = 3 },   // edit_prosess
            new RolePermission { RoleId = 1, PermissionId = 4 },   // delete_prosess
            new RolePermission { RoleId = 1, PermissionId = 5 },   // approve_prosess
            new RolePermission { RoleId = 1, PermissionId = 6 },   // view_qa_queue
            new RolePermission { RoleId = 1, PermissionId = 7 },   // approve_changes
            new RolePermission { RoleId = 1, PermissionId = 8 },   // reject_changes
            new RolePermission { RoleId = 1, PermissionId = 9 },   // manage_users
            new RolePermission { RoleId = 1, PermissionId = 10 },  // manage_roles
            new RolePermission { RoleId = 1, PermissionId = 11 },  // view_audit_log
            
            // ProsessEier - create, edit, approve processes
            new RolePermission { RoleId = 2, PermissionId = 1 },   // view_prosess
            new RolePermission { RoleId = 2, PermissionId = 2 },   // create_prosess
            new RolePermission { RoleId = 2, PermissionId = 3 },   // edit_prosess
            new RolePermission { RoleId = 2, PermissionId = 5 },   // approve_prosess
            
            // QA - view, approve/reject changes
            new RolePermission { RoleId = 3, PermissionId = 1 },   // view_prosess
            new RolePermission { RoleId = 3, PermissionId = 6 },   // view_qa_queue
            new RolePermission { RoleId = 3, PermissionId = 7 },   // approve_changes
            new RolePermission { RoleId = 3, PermissionId = 8 },   // reject_changes
            
            // SME - view and basic editing
            new RolePermission { RoleId = 4, PermissionId = 1 },   // view_prosess
            new RolePermission { RoleId = 4, PermissionId = 3 },   // edit_prosess
            
            // Bruker - view only
            new RolePermission { RoleId = 5, PermissionId = 1 }    // view_prosess
        );
        
        // Seed admin user
        modelBuilder.Entity<User>().HasData(
            new User 
            { 
                Id = 1,
                Username = "admin",
                Email = "admin@prosessportal.no",
                PasswordHash = "$2a$11$N8gNZ4Hg/JQXS4PZH6YX6e3rR5Zf6KjV8W5Qf2S4h3G6Yq9Pb7Ed", // admin123
                FirstName = "System",
                LastName = "Administrator",
                IsActive = true,
                CreatedAt = new DateTime(2025, 11, 23, 17, 0, 0, DateTimeKind.Utc)
            }
        );
        
        // Seed admin user role assignment
        modelBuilder.Entity<UserRole>().HasData(
            new UserRole 
            { 
                UserId = 1,
                RoleId = 1, // Admin role
                AssignedAt = new DateTime(2025, 11, 23, 17, 0, 0, DateTimeKind.Utc)
            }
        );
        
        // ProsessApprovalRequest configuration
        modelBuilder.Entity<ProsessApprovalRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RequestComment).HasMaxLength(1000);
            entity.Property(e => e.ApprovalComment).HasMaxLength(1000);
            entity.Property(e => e.RejectionReason).HasMaxLength(1000);
            
            entity.HasOne(e => e.Prosess)
                .WithMany()
                .HasForeignKey(e => e.ProsessId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.RequestedByUser)
                .WithMany()
                .HasForeignKey(e => e.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.ApprovedByUser)
                .WithMany()
                .HasForeignKey(e => e.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(e => e.ProsessId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.RequestedAt);
        });
        
        // ProsessApprovalComment configuration
        modelBuilder.Entity<ProsessApprovalComment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comment).HasMaxLength(2000).IsRequired();
            
            entity.HasOne(e => e.ApprovalRequest)
                .WithMany(e => e.Comments)
                .HasForeignKey(e => e.ApprovalRequestId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(e => e.ApprovalRequestId);
            entity.HasIndex(e => e.CreatedAt);
        });
        
        // ProsessApprovalHistory configuration
        modelBuilder.Entity<ProsessApprovalHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comment).HasMaxLength(1000);
            entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
            
            entity.HasOne(e => e.Prosess)
                .WithMany()
                .HasForeignKey(e => e.ProsessId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(e => e.ProsessId);
            entity.HasIndex(e => e.ChangedAt);
        });
        
        // ProsessEditSession configuration
        modelBuilder.Entity<ProsessEditSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.StartComment).HasMaxLength(1000);
            entity.Property(e => e.CompletionComment).HasMaxLength(1000);
            entity.Property(e => e.DraftTitle).HasMaxLength(200);
            entity.Property(e => e.DraftDescription).HasMaxLength(1000);
            entity.Property(e => e.DraftCategory).HasMaxLength(50);
            entity.Property(e => e.DraftTags).HasMaxLength(2000);
            entity.Property(e => e.DraftSteps).HasMaxLength(10000);
            
            entity.HasOne(e => e.Prosess)
                .WithMany()
                .HasForeignKey(e => e.ProsessId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.CreatedVersion)
                .WithMany()
                .HasForeignKey(e => e.CreatedVersionId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasIndex(e => e.SessionId).IsUnique();
            entity.HasIndex(e => e.ProsessId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.LastActivity);
        });
        
        // ProsessEditConflict configuration
        modelBuilder.Entity<ProsessEditConflict>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId1).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SessionId2).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ConflictingFields).HasMaxLength(2000);
            entity.Property(e => e.ResolutionComment).HasMaxLength(1000);
            
            entity.HasOne(e => e.Prosess)
                .WithMany()
                .HasForeignKey(e => e.ProsessId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User1)
                .WithMany()
                .HasForeignKey(e => e.UserId1)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.User2)
                .WithMany()
                .HasForeignKey(e => e.UserId2)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.ResolvedByUser)
                .WithMany()
                .HasForeignKey(e => e.ResolvedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(e => e.ProsessId);
            entity.HasIndex(e => e.DetectedAt);
        });
        
        // ProsessAutoSave configuration
        modelBuilder.Entity<ProsessAutoSave>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Content).HasMaxLength(20000);
            
            entity.HasOne(e => e.Prosess)
                .WithMany()
                .HasForeignKey(e => e.ProsessId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.SavedAt);
        });
        
        // ProsessDeletionHistory configuration
        modelBuilder.Entity<ProsessDeletionHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Reason).HasMaxLength(1000);
            
            entity.HasOne(e => e.Prosess)
                .WithMany()
                .HasForeignKey(e => e.ProsessId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(e => e.ProsessId);
            entity.HasIndex(e => e.ActionAt);
        });
        
        // Seed sample processes
        SeedSampleProsesses(modelBuilder);
        
        // Seed sample actors
        SeedSampleActors(modelBuilder);
    }
    
    private void SeedSampleProsesses(ModelBuilder modelBuilder)
    {
        // Sample processes for demonstration
        var baseDate = new DateTime(2025, 11, 23, 17, 0, 0, DateTimeKind.Utc);
        
        modelBuilder.Entity<Prosess>().HasData(
            new Prosess 
            { 
                Id = 1, 
                Title = "Ny medarbeider onboarding",
                Description = "Komplett prosess for å ta imot nye medarbeidere",
                Category = ProsessCategories.HR,
                Status = ProsessStatus.Published,
                CreatedAt = baseDate.AddDays(-30),
                UpdatedAt = baseDate.AddDays(-5),
                CreatedByUserId = 1,
                IsActive = true,
                ViewCount = 25
            },
            new Prosess 
            { 
                Id = 2, 
                Title = "IT-utstyr bestilling",
                Description = "Prosess for bestilling av nytt IT-utstyr til medarbeidere",
                Category = ProsessCategories.IT,
                Status = ProsessStatus.Published,
                CreatedAt = baseDate.AddDays(-45),
                UpdatedAt = baseDate.AddDays(-10),
                CreatedByUserId = 1,
                IsActive = true,
                ViewCount = 18
            },
            new Prosess 
            { 
                Id = 3, 
                Title = "Fakturahåndtering",
                Description = "Standard prosess for håndtering og godkjenning av fakturaer",
                Category = ProsessCategories.Finance,
                Status = ProsessStatus.Published,
                CreatedAt = baseDate.AddDays(-20),
                UpdatedAt = baseDate.AddDays(-2),
                CreatedByUserId = 1,
                IsActive = true,
                ViewCount = 42
            },
            new Prosess 
            { 
                Id = 4, 
                Title = "Kundehenvendelser support",
                Description = "Håndtering av kundehenvendelser i support-system",
                Category = ProsessCategories.CustomerService,
                Status = ProsessStatus.InReview,
                CreatedAt = baseDate.AddDays(-7),
                UpdatedAt = baseDate.AddDays(-1),
                CreatedByUserId = 1,
                IsActive = true,
                ViewCount = 8
            }
        );
    }
    
    private void SeedSampleActors(ModelBuilder modelBuilder)
    {
        var baseDate = new DateTime(2025, 11, 23, 17, 0, 0, DateTimeKind.Utc);
        
        modelBuilder.Entity<Actor>().HasData(
            // Person - Internal
            new Actor 
            {
                Id = 1,
                ActorCategory = ActorCategory.Person,
                FirstName = "Lars",
                LastName = "Johansen",
                Email = "lars.johansen@forsvaret.no",
                Phone = "+47 98765432",
                ActorType = ActorType.Internal,
                SecurityClearance = SecurityClearance.Secret,
                OrganizationName = "Forsvaret",
                Department = "IT-avdelingen",
                Position = "IT-arkitekt",
                GeographicLocation = "Oslo",
                PreferredLanguage = "NO",
                IsActive = true,
                CreatedAt = baseDate.AddDays(-60),
                CreatedByUserId = 1
            },
            // Person - External
            new Actor 
            {
                Id = 2,
                ActorCategory = ActorCategory.Person,
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@techcorp.com",
                Phone = "+47 76543210",
                ActorType = ActorType.Vendor,
                SecurityClearance = SecurityClearance.Restricted,
                OrganizationName = "TechCorp AS",
                Department = "Support",
                Position = "Senior konsulent",
                GeographicLocation = "Oslo",
                PreferredLanguage = "EN",
                ContractNumber = "K-2025-001",
                ContractStartDate = baseDate.AddDays(-90),
                ContractEndDate = baseDate.AddDays(275),
                VendorId = "TECH001",
                IsActive = true,
                CreatedAt = baseDate.AddDays(-90),
                CreatedByUserId = 1
            },
            // Organization - External
            new Actor 
            {
                Id = 3,
                ActorCategory = ActorCategory.Organization,
                OrganizationName = "TechCorp AS",
                Email = "contact@techcorp.com",
                Phone = "+47 22 12 34 56",
                ActorType = ActorType.Vendor,
                SecurityClearance = SecurityClearance.Restricted,
                Department = "IT Services",
                GeographicLocation = "Oslo",
                Address = "Teknologigaten 15, 0150 Oslo",
                RegistrationNumber = "987654321",
                EmployeeCount = 150,
                ParentOrganization = "TechCorp International",
                ContractNumber = "K-2025-001",
                ContractStartDate = baseDate.AddDays(-90),
                ContractEndDate = baseDate.AddDays(275),
                VendorId = "TECH001",
                IsActive = true,
                CreatedAt = baseDate.AddDays(-90),
                CreatedByUserId = 1
            },
            // Unit - Internal
            new Actor 
            {
                Id = 4,
                ActorCategory = ActorCategory.Unit,
                UnitName = "Cyber Brigade",
                UnitType = "Brigade",
                UnitCode = "CYB-BDE",
                Email = "cyb.bde@forsvaret.no",
                Phone = "+47 23 09 50 00",
                ActorType = ActorType.Internal,
                SecurityClearance = SecurityClearance.Secret,
                OrganizationName = "Forsvaret",
                GeographicLocation = "Lillehammer",
                CommandStructure = "Hærstaben",
                UnitMission = "Ansvarlig for cyberoperasjoner og digitalt forsvar",
                PersonnelCount = 1200,
                IsActive = true,
                CreatedAt = baseDate.AddDays(-120),
                CreatedByUserId = 1
            },
            // Unit - Smaller unit
            new Actor 
            {
                Id = 5,
                ActorCategory = ActorCategory.Unit,
                UnitName = "2. Bataljon",
                UnitType = "Bataljon", 
                UnitCode = "2-BTN",
                Email = "2btn@forsvaret.no",
                Phone = "+47 75 50 30 00",
                ActorType = ActorType.Internal,
                SecurityClearance = SecurityClearance.Confidential,
                OrganizationName = "Forsvaret",
                Department = "Brigade Nord",
                GeographicLocation = "Setermoen",
                CommandStructure = "Brigade Nord",
                UnitMission = "Stridsklare styrker for forsvar av Nord-Norge",
                PersonnelCount = 600,
                IsActive = true,
                CreatedAt = baseDate.AddDays(-100),
                CreatedByUserId = 1
            }
        );
    }
}