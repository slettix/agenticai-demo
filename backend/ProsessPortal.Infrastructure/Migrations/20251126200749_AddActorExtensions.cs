using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProsessPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActorExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 26, 20, 7, 49, 193, DateTimeKind.Utc).AddTicks(3300));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 26, 20, 7, 49, 193, DateTimeKind.Utc).AddTicks(3670));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 26, 20, 7, 49, 193, DateTimeKind.Utc).AddTicks(3670));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 26, 20, 7, 49, 193, DateTimeKind.Utc).AddTicks(3670));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 26, 20, 7, 49, 193, DateTimeKind.Utc).AddTicks(3670));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 26, 20, 4, 4, 533, DateTimeKind.Utc).AddTicks(1810));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 26, 20, 4, 4, 533, DateTimeKind.Utc).AddTicks(2130));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 26, 20, 4, 4, 533, DateTimeKind.Utc).AddTicks(2130));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 26, 20, 4, 4, 533, DateTimeKind.Utc).AddTicks(2140));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 26, 20, 4, 4, 533, DateTimeKind.Utc).AddTicks(2140));
        }
    }
}
