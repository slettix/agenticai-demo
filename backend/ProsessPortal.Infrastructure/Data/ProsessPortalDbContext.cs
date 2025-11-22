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
    }
}