using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProsessPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ActorModelUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 26, 20, 8, 27, 371, DateTimeKind.Utc).AddTicks(5640));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 26, 20, 8, 27, 371, DateTimeKind.Utc).AddTicks(6000));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 26, 20, 8, 27, 371, DateTimeKind.Utc).AddTicks(6000));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 26, 20, 8, 27, 371, DateTimeKind.Utc).AddTicks(6000));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 26, 20, 8, 27, 371, DateTimeKind.Utc).AddTicks(6000));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
