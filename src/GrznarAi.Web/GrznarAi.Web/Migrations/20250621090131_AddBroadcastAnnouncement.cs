using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrznarAi.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddBroadcastAnnouncement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BroadcastAnnouncements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    BroadcastDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImportedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BroadcastAnnouncements", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BroadcastAnnouncements_BroadcastDateTime",
                table: "BroadcastAnnouncements",
                column: "BroadcastDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_BroadcastAnnouncements_ImportedDateTime",
                table: "BroadcastAnnouncements",
                column: "ImportedDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_BroadcastAnnouncements_IsActive",
                table: "BroadcastAnnouncements",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BroadcastAnnouncements");
        }
    }
}
