using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProsessPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithCorrectStepTypes : Migration
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
                    { 1, new DateTime(2025, 11, 23, 21, 36, 55, 275, DateTimeKind.Utc).AddTicks(3980), "Systemadministrator med full tilgang", "Admin" },
                    { 2, new DateTime(2025, 11, 23, 21, 36, 55, 275, DateTimeKind.Utc).AddTicks(4300), "Eier av prosesser, kan redigere og godkjenne", "ProsessEier" },
                    { 3, new DateTime(2025, 11, 23, 21, 36, 55, 275, DateTimeKind.Utc).AddTicks(4300), "Kvalitetssikring, kan godkjenne endringer", "QA" },
                    { 4, new DateTime(2025, 11, 23, 21, 36, 55, 275, DateTimeKind.Utc).AddTicks(4300), "Fagekspert, kan foreslå endringer", "SME" },
                    { 5, new DateTime(2025, 11, 23, 21, 36, 55, 275, DateTimeKind.Utc).AddTicks(4300), "Vanlig bruker, kun lesetilgang", "Bruker" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IsActive", "LastLoginAt", "LastName", "PasswordHash", "Username" },
                values: new object[] { 1, new DateTime(2025, 11, 23, 17, 0, 0, 0, DateTimeKind.Utc), "admin@prosessportal.no", "System", true, null, "Administrator", "$2a$11$N8gNZ4Hg/JQXS4PZH6YX6e3rR5Zf6KjV8W5Qf2S4h3G6Yq9Pb7Ed", "admin" });

            migrationBuilder.InsertData(
                table: "Prosesser",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedByUserId", "Description", "GitBranch", "GitPath", "GitRepository", "IsActive", "LastAccessedAt", "OwnerId", "Status", "Title", "UpdatedAt", "ViewCount" },
                values: new object[,]
                {
                    { 1, "HR", new DateTime(2025, 10, 24, 17, 0, 0, 0, DateTimeKind.Utc), 1, "Komplett prosess for å ta imot nye medarbeidere", "main", null, null, true, null, null, 3, "Ny medarbeider onboarding", new DateTime(2025, 11, 18, 17, 0, 0, 0, DateTimeKind.Utc), 25 },
                    { 2, "IT", new DateTime(2025, 10, 9, 17, 0, 0, 0, DateTimeKind.Utc), 1, "Prosess for bestilling av nytt IT-utstyr til medarbeidere", "main", null, null, true, null, null, 3, "IT-utstyr bestilling", new DateTime(2025, 11, 13, 17, 0, 0, 0, DateTimeKind.Utc), 18 },
                    { 3, "Økonomi", new DateTime(2025, 11, 3, 17, 0, 0, 0, DateTimeKind.Utc), 1, "Standard prosess for håndtering og godkjenning av fakturaer", "main", null, null, true, null, null, 3, "Fakturahåndtering", new DateTime(2025, 11, 21, 17, 0, 0, 0, DateTimeKind.Utc), 42 },
                    { 4, "Kundeservice", new DateTime(2025, 11, 16, 17, 0, 0, 0, DateTimeKind.Utc), 1, "Håndtering av kundehenvendelser i support-system", "main", null, null, true, null, null, 1, "Kundehenvendelser support", new DateTime(2025, 11, 22, 17, 0, 0, 0, DateTimeKind.Utc), 8 }
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
                name: "IX_Permissions_Resource_Action",
                table: "Permissions",
                columns: new[] { "Resource", "Action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prosesser_Category",
                table: "Prosesser",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Prosesser_CreatedByUserId",
                table: "Prosesser",
                column: "CreatedByUserId");

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
                name: "ProsessTags");

            migrationBuilder.DropTable(
                name: "ProsessVersions");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "StepConnections");

            migrationBuilder.DropTable(
                name: "UserRoles");

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
