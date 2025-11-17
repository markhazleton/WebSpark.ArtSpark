using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebSpark.ArtSpark.Demo.Migrations
{
    /// <inheritdoc />
    public partial class AddProfilePhotoAndAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailVerified",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePhotoFileName",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePhotoThumbnail128",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePhotoThumbnail256",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePhotoThumbnail64",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 260,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActionType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Details = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    AdminUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    TargetUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    CorrelationId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_AspNetUsers_AdminUserId",
                        column: x => x.AdminUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLogs_AspNetUsers_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_AdminUserId_CreatedAtUtc",
                table: "AuditLogs",
                columns: new[] { "AdminUserId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TargetUserId_CreatedAtUtc",
                table: "AuditLogs",
                columns: new[] { "TargetUserId", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "EmailVerified",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoFileName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoThumbnail128",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoThumbnail256",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoThumbnail64",
                table: "AspNetUsers");
        }
    }
}
