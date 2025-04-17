using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrznarAi.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalizationStrings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocalizationStrings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ValueCs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValueEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalizationStrings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationStrings_Key",
                table: "LocalizationStrings",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocalizationStrings");
        }
    }
}
