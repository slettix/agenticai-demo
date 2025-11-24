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
        
        // Seed default roles
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = RoleNames.Admin, Description = "Systemadministrator med full tilgang" },
            new Role { Id = 2, Name = RoleNames.ProsessEier, Description = "Eier av prosesser, kan redigere og godkjenne" },
            new Role { Id = 3, Name = RoleNames.QA, Description = "Kvalitetssikring, kan godkjenne endringer" },
            new Role { Id = 4, Name = RoleNames.SME, Description = "Fagekspert, kan foreslå endringer" },
            new Role { Id = 5, Name = RoleNames.Bruker, Description = "Vanlig bruker, kun lesetilgang" }
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
                
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Status);
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
        
        // Seed sample processes
        SeedSampleProsesses(modelBuilder);
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
}