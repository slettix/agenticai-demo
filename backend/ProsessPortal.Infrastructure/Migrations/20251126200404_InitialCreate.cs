using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProsessPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Resource = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    PermissionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Actors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActorCategory = table.Column<int>(type: "integer", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OrganizationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UnitName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UnitType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UnitCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ActorType = table.Column<int>(type: "integer", nullable: false),
                    SecurityClearance = table.Column<int>(type: "integer", nullable: false),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Position = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ManagerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ManagerEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RegistrationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ParentOrganization = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EmployeeCount = table.Column<int>(type: "integer", nullable: true),
                    CommandStructure = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UnitMission = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PersonnelCount = table.Column<int>(type: "integer", nullable: true),
                    GeographicLocation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PreferredLanguage = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ContractNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ContractStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContractEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VendorId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Actors_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Actors_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Prosesser",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    GitRepository = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GitPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GitBranch = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<int>(type: "integer", nullable: true),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prosesser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prosesser_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prosesser_Users_DeletedByUserId",
                        column: x => x.DeletedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Prosesser_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActorNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    IsPrivate = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActorNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActorNotes_Actors_ActorId",
                        column: x => x.ActorId,
                        principalTable: "Actors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActorNotes_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActorRoles",
                columns: table => new
                {
                    ActorId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignedByUserId = table.Column<int>(type: "integer", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActorRoles", x => new { x.ActorId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_ActorRoles_Actors_ActorId",
                        column: x => x.ActorId,
                        principalTable: "Actors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActorRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActorRoles_Users_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProsessApprovalHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProsessId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: false),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProsessApprovalHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProsessApprovalHistory_Prosesser_ProsessId",
                        column: x => x.ProsessId,
                        principalTable: "Prosesser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProsessApprovalHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProsessApprovalRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProsessId = table.Column<int>(type: "integer", nullable: false),
                    RequestedByUserId = table.Column<int>(type: "integer", nullable: false),
                    RequestComment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ApprovedByUserId = table.Column<int>(type: "integer", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovalComment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProsessApprovalRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProsessApprovalRequests_Prosesser_ProsessId",
                        column: x => x.ProsessId,
                        principalTable: "Prosesser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProsessApprovalRequests_Users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProsessApprovalRequests_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProsessAutoSaves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProsessId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: false),
                    SavedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRestored = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProsessAutoSaves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProsessAutoSaves_Prosesser_ProsessId",
                        column: x => x.ProsessId,
                        principalTable: "Prosesser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProsessAutoSaves_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProsessDeletionHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProsessId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ActionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProsessDeletionHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProsessDeletionHistory_Prosesser_ProsessId",
                        column: x => x.ProsessId,
                        principalTable: "Prosesser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProsessDeletionHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProsessEditConflicts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProsessId = table.Column<int>(type: "integer", nullable: false),
                    SessionId1 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SessionId2 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserId1 = table.Column<int>(type: "integer", nullable: false),
                    UserId2 = table.Column<int>(type: "integer", nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConflictingFields = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Resolution = table.Column<int>(type: "integer", nullable: true),
                    ResolvedByUserId = table.Column<int>(type: "integer", nullable: true),
                    ResolutionComment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProsessEditConflicts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProsessEditConflicts_Prosesser_ProsessId",
                        column: x => x.ProsessId,
                        principalTable: "Prosesser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProsessEditConflicts_Users_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProsessEditConflicts_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProsessEditConflicts_Users_UserId2",
                        column: x => x.UserId2,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProsessSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProsessId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DetailedInstructions = table.Column<string>(type: "text", nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ResponsibleRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EstimatedDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    IsOptional = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ParentStepId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProsessSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProsessSteps_ProsessSteps_ParentStepId",
                        column: x => x.ParentStepId,
                        principalTable: "ProsessSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProsessSteps_Prosesser_ProsessId",
                        column: x => x.ProsessId,
                        principalTable: "Prosesser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProsessTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProsessId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Color = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProsessTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProsessTags_Prosesser_ProsessId",
                        column: x => x.ProsessId,
                        principalTable: "Prosesser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProsessVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProsessId = table.Column<int>(type: "integer", nullable: false),
                    VersionNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    ChangeLog = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    GitCommitHash = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    GitTag = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedByUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProsessVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProsessVersions_Prosesser_ProsessId",
                        column: x => x.ProsessId,
                        principalTable: "Prosesser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProsessVersions_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProsessVersions_Users_PublishedByUserId",
                        column: x => x.PublishedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProsessApprovalComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApprovalRequestId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProsessApprovalComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProsessApprovalComments_ProsessApprovalRequests_ApprovalReq~",
                        column: x => x.ApprovalRequestId,
                        principalTable: "ProsessApprovalRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProsessApprovalComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StepConnections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromStepId = table.Column<int>(type: "integer", nullable: false),
                    ToStepId = table.Column<int>(type: "integer", nullable: false),
                    Condition = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StepConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StepConnections_ProsessSteps_FromStepId",
                        column: x => x.FromStepId,
                        principalTable: "ProsessSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StepConnections_ProsessSteps_ToStepId",
                        column: x => x.ToStepId,
                        principalTable: "ProsessSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProsessEditSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProsessId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastActivity = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartComment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CompletionComment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    LockExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DraftTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DraftDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DraftCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DraftTags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DraftSteps = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    LastAutoSave = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedVersionId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProsessEditSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProsessEditSessions_ProsessVersions_CreatedVersionId",
                        column: x => x.CreatedVersionId,
                        principalTable: "ProsessVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProsessEditSessions_Prosesser_ProsessId",
                        column: x => x.ProsessId,
                        principalTable: "Prosesser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProsessEditSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Description", "Name", "Resource" },
                values: new object[,]
                {
                    { 1, "view", "Se prosesser", "view_prosess", "prosess" },
                    { 2, "create", "Opprette prosesser", "create_prosess", "prosess" },
                    { 3, "edit", "Redigere prosesser", "edit_prosess", "prosess" },
                    { 4, "delete", "Slette prosesser", "delete_prosess", "prosess" },
                    { 5, "approve", "Godkjenne prosesser", "approve_prosess", "prosess" },
                    { 6, "view", "Se QA-kø", "view_qa_queue", "qa" },
                    { 7, "approve", "Godkjenne endringer", "approve_changes", "qa" },
                    { 8, "reject", "Avvise endringer", "reject_changes", "qa" },
                    { 9, "manage", "Administrere brukere", "manage_users", "user" },
                    { 10, "manage", "Administrere roller", "manage_roles", "role" },
                    { 11, "view", "Se audit-logg", "view_audit_log", "audit" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "Description", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 11, 26, 20, 4, 4, 533, DateTimeKind.Utc).AddTicks(1810), "Systemadministrator med full tilgang", "Admin" },
                    { 2, new DateTime(2025, 11, 26, 20, 4, 4, 533, DateTimeKind.Utc).AddTicks(2130), "Eier av prosesser, kan redigere og godkjenne", "ProsessEier" },
                    { 3, new DateTime(2025, 11, 26, 20, 4, 4, 533, DateTimeKind.Utc).AddTicks(2130), "Kvalitetssikring, kan godkjenne endringer", "QA" },
                    { 4, new DateTime(2025, 11, 26, 20, 4, 4, 533, DateTimeKind.Utc).AddTicks(2140), "Fagekspert, kan foreslå endringer", "SME" },
                    { 5, new DateTime(2025, 11, 26, 20, 4, 4, 533, DateTimeKind.Utc).AddTicks(2140), "Vanlig bruker, kun lesetilgang", "Bruker" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IsActive", "LastLoginAt", "LastName", "PasswordHash", "Username" },
                values: new object[] { 1, new DateTime(2025, 11, 23, 17, 0, 0, 0, DateTimeKind.Utc), "admin@prosessportal.no", "System", true, null, "Administrator", "$2a$11$N8gNZ4Hg/JQXS4PZH6YX6e3rR5Zf6KjV8W5Qf2S4h3G6Yq9Pb7Ed", "admin" });

            migrationBuilder.InsertData(
                table: "Actors",
                columns: new[] { "Id", "ActorCategory", "ActorType", "Address", "CommandStructure", "ContractEndDate", "ContractNumber", "ContractStartDate", "CreatedAt", "CreatedByUserId", "Department", "Email", "EmployeeCount", "FirstName", "GeographicLocation", "IsActive", "LastName", "ManagerEmail", "ManagerName", "OrganizationName", "ParentOrganization", "PersonnelCount", "Phone", "Position", "PreferredLanguage", "RegistrationNumber", "SecurityClearance", "UnitCode", "UnitMission", "UnitName", "UnitType", "UpdatedAt", "UpdatedByUserId", "VendorId" },
                values: new object[,]
                {
                    { 1, 0, 0, null, null, null, null, null, new DateTime(2025, 9, 24, 17, 0, 0, 0, DateTimeKind.Utc), 1, "IT-avdelingen", "lars.johansen@forsvaret.no", null, "Lars", "Oslo", true, "Johansen", null, null, "Forsvaret", null, null, "+47 98765432", "IT-arkitekt", "NO", null, 3, null, null, null, null, null, null, null },
                    { 2, 0, 4, null, null, new DateTime(2026, 8, 25, 17, 0, 0, 0, DateTimeKind.Utc), "K-2025-001", new DateTime(2025, 8, 25, 17, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 25, 17, 0, 0, 0, DateTimeKind.Utc), 1, "Support", "john.smith@techcorp.com", null, "John", "Oslo", true, "Smith", null, null, "TechCorp AS", null, null, "+47 76543210", "Senior konsulent", "EN", null, 1, null, null, null, null, null, null, "TECH001" },
                    { 3, 1, 4, "Teknologigaten 15, 0150 Oslo", null, new DateTime(2026, 8, 25, 17, 0, 0, 0, DateTimeKind.Utc), "K-2025-001", new DateTime(2025, 8, 25, 17, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 25, 17, 0, 0, 0, DateTimeKind.Utc), 1, "IT Services", "contact@techcorp.com", 150, null, "Oslo", true, null, null, null, "TechCorp AS", "TechCorp International", null, "+47 22 12 34 56", null, "NO", "987654321", 1, null, null, null, null, null, null, "TECH001" },
                    { 4, 2, 0, null, "Hærstaben", null, null, null, new DateTime(2025, 7, 26, 17, 0, 0, 0, DateTimeKind.Utc), 1, null, "cyb.bde@forsvaret.no", null, null, "Lillehammer", true, null, null, null, "Forsvaret", null, 1200, "+47 23 09 50 00", null, "NO", null, 3, "CYB-BDE", "Ansvarlig for cyberoperasjoner og digitalt forsvar", "Cyber Brigade", "Brigade", null, null, null },
                    { 5, 2, 0, null, "Brigade Nord", null, null, null, new DateTime(2025, 8, 15, 17, 0, 0, 0, DateTimeKind.Utc), 1, "Brigade Nord", "2btn@forsvaret.no", null, null, "Setermoen", true, null, null, null, "Forsvaret", null, 600, "+47 75 50 30 00", null, "NO", null, 2, "2-BTN", "Stridsklare styrker for forsvar av Nord-Norge", "2. Bataljon", "Bataljon", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "Prosesser",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedByUserId", "DeletedAt", "DeletedByUserId", "Description", "GitBranch", "GitPath", "GitRepository", "IsActive", "IsDeleted", "LastAccessedAt", "OwnerId", "Status", "Title", "UpdatedAt", "ViewCount" },
                values: new object[,]
                {
                    { 1, "HR", new DateTime(2025, 10, 24, 17, 0, 0, 0, DateTimeKind.Utc), 1, null, null, "Komplett prosess for å ta imot nye medarbeidere", "main", null, null, true, false, null, null, 5, "Ny medarbeider onboarding", new DateTime(2025, 11, 18, 17, 0, 0, 0, DateTimeKind.Utc), 25 },
                    { 2, "IT", new DateTime(2025, 10, 9, 17, 0, 0, 0, DateTimeKind.Utc), 1, null, null, "Prosess for bestilling av nytt IT-utstyr til medarbeidere", "main", null, null, true, false, null, null, 5, "IT-utstyr bestilling", new DateTime(2025, 11, 13, 17, 0, 0, 0, DateTimeKind.Utc), 18 },
                    { 3, "Økonomi", new DateTime(2025, 11, 3, 17, 0, 0, 0, DateTimeKind.Utc), 1, null, null, "Standard prosess for håndtering og godkjenning av fakturaer", "main", null, null, true, false, null, null, 5, "Fakturahåndtering", new DateTime(2025, 11, 21, 17, 0, 0, 0, DateTimeKind.Utc), 42 },
                    { 4, "Kundeservice", new DateTime(2025, 11, 16, 17, 0, 0, 0, DateTimeKind.Utc), 1, null, null, "Håndtering av kundehenvendelser i support-system", "main", null, null, true, false, null, null, 2, "Kundehenvendelser support", new DateTime(2025, 11, 22, 17, 0, 0, 0, DateTimeKind.Utc), 8 }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 1 },
                    { 5, 1 },
                    { 6, 1 },
                    { 7, 1 },
                    { 8, 1 },
                    { 9, 1 },
                    { 10, 1 },
                    { 11, 1 },
                    { 1, 2 },
                    { 2, 2 },
                    { 3, 2 },
                    { 5, 2 },
                    { 1, 3 },
                    { 6, 3 },
                    { 7, 3 },
                    { 8, 3 },
                    { 1, 4 },
                    { 3, 4 },
                    { 1, 5 }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId", "AssignedAt", "AssignedBy" },
                values: new object[] { 1, 1, new DateTime(2025, 11, 23, 17, 0, 0, 0, DateTimeKind.Utc), null });

            migrationBuilder.CreateIndex(
                name: "IX_ActorNotes_ActorId",
                table: "ActorNotes",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_ActorNotes_Category",
                table: "ActorNotes",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ActorNotes_CreatedAt",
                table: "ActorNotes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ActorNotes_CreatedByUserId",
                table: "ActorNotes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ActorRoles_AssignedByUserId",
                table: "ActorRoles",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ActorRoles_IsActive",
                table: "ActorRoles",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ActorRoles_RoleId",
                table: "ActorRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Actors_ActorCategory",
                table: "Actors",
                column: "ActorCategory");

            migrationBuilder.CreateIndex(
                name: "IX_Actors_ActorType",
                table: "Actors",
                column: "ActorType");

            migrationBuilder.CreateIndex(
                name: "IX_Actors_CreatedByUserId",
                table: "Actors",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Actors_Email",
                table: "Actors",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Actors_IsActive",
                table: "Actors",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Actors_OrganizationName",
                table: "Actors",
                column: "OrganizationName");

            migrationBuilder.CreateIndex(
                name: "IX_Actors_SecurityClearance",
                table: "Actors",
                column: "SecurityClearance");

            migrationBuilder.CreateIndex(
                name: "IX_Actors_UnitName",
                table: "Actors",
                column: "UnitName");

            migrationBuilder.CreateIndex(
                name: "IX_Actors_UpdatedByUserId",
                table: "Actors",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Resource_Action",
                table: "Permissions",
                columns: new[] { "Resource", "Action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProsessApprovalComments_ApprovalRequestId",
                table: "ProsessApprovalComments",
                column: "ApprovalRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessApprovalComments_CreatedAt",
                table: "ProsessApprovalComments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessApprovalComments_UserId",
                table: "ProsessApprovalComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessApprovalHistory_ChangedAt",
                table: "ProsessApprovalHistory",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessApprovalHistory_ProsessId",
                table: "ProsessApprovalHistory",
                column: "ProsessId");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessApprovalHistory_UserId",
                table: "ProsessApprovalHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessApprovalRequests_ApprovedByUserId",
                table: "ProsessApprovalRequests",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessApprovalRequests_ProsessId",
                table: "ProsessApprovalRequests",
                column: "ProsessId");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessApprovalRequests_RequestedAt",
                table: "ProsessApprovalRequests",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessApprovalRequests_RequestedByUserId",
                table: "ProsessApprovalRequests",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessApprovalRequests_Status",
                table: "ProsessApprovalRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessAutoSaves_ProsessId",
                table: "ProsessAutoSaves",
                column: "ProsessId");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessAutoSaves_SavedAt",
                table: "ProsessAutoSaves",
                column: "SavedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessAutoSaves_SessionId",
                table: "ProsessAutoSaves",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessAutoSaves_UserId",
                table: "ProsessAutoSaves",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessDeletionHistory_ActionAt",
                table: "ProsessDeletionHistory",
                column: "ActionAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessDeletionHistory_ProsessId",
                table: "ProsessDeletionHistory",
                column: "ProsessId");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessDeletionHistory_UserId",
                table: "ProsessDeletionHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessEditConflicts_DetectedAt",
                table: "ProsessEditConflicts",
                column: "DetectedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessEditConflicts_ProsessId",
                table: "ProsessEditConflicts",
                column: "ProsessId");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessEditConflicts_ResolvedByUserId",
                table: "ProsessEditConflicts",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessEditConflicts_UserId1",
                table: "ProsessEditConflicts",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessEditConflicts_UserId2",
                table: "ProsessEditConflicts",
                column: "UserId2");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessEditSessions_CreatedVersionId",
                table: "ProsessEditSessions",
                column: "CreatedVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessEditSessions_LastActivity",
                table: "ProsessEditSessions",
                column: "LastActivity");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessEditSessions_ProsessId",
                table: "ProsessEditSessions",
                column: "ProsessId");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessEditSessions_SessionId",
                table: "ProsessEditSessions",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProsessEditSessions_Status",
                table: "ProsessEditSessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessEditSessions_UserId",
                table: "ProsessEditSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Prosesser_Category",
                table: "Prosesser",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Prosesser_CreatedByUserId",
                table: "Prosesser",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Prosesser_DeletedByUserId",
                table: "Prosesser",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Prosesser_IsDeleted",
                table: "Prosesser",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Prosesser_OwnerId",
                table: "Prosesser",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Prosesser_Status",
                table: "Prosesser",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Prosesser_Title",
                table: "Prosesser",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessSteps_ParentStepId",
                table: "ProsessSteps",
                column: "ParentStepId");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessSteps_ProsessId_OrderIndex",
                table: "ProsessSteps",
                columns: new[] { "ProsessId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_ProsessTags_ProsessId_Name",
                table: "ProsessTags",
                columns: new[] { "ProsessId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProsessVersions_CreatedByUserId",
                table: "ProsessVersions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProsessVersions_ProsessId_IsCurrent",
                table: "ProsessVersions",
                columns: new[] { "ProsessId", "IsCurrent" });

            migrationBuilder.CreateIndex(
                name: "IX_ProsessVersions_ProsessId_VersionNumber",
                table: "ProsessVersions",
                columns: new[] { "ProsessId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProsessVersions_PublishedByUserId",
                table: "ProsessVersions",
                column: "PublishedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StepConnections_FromStepId",
                table: "StepConnections",
                column: "FromStepId");

            migrationBuilder.CreateIndex(
                name: "IX_StepConnections_ToStepId",
                table: "StepConnections",
                column: "ToStepId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActorNotes");

            migrationBuilder.DropTable(
                name: "ActorRoles");

            migrationBuilder.DropTable(
                name: "ProsessApprovalComments");

            migrationBuilder.DropTable(
                name: "ProsessApprovalHistory");

            migrationBuilder.DropTable(
                name: "ProsessAutoSaves");

            migrationBuilder.DropTable(
                name: "ProsessDeletionHistory");

            migrationBuilder.DropTable(
                name: "ProsessEditConflicts");

            migrationBuilder.DropTable(
                name: "ProsessEditSessions");

            migrationBuilder.DropTable(
                name: "ProsessTags");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "StepConnections");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Actors");

            migrationBuilder.DropTable(
                name: "ProsessApprovalRequests");

            migrationBuilder.DropTable(
                name: "ProsessVersions");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "ProsessSteps");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Prosesser");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
