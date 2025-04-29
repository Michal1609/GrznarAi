using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrznarAi.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddTitleEnIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TitleEn",
                table: "AiNewsItems",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_AiNewsItems_TitleEn",
                table: "AiNewsItems",
                column: "TitleEn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AiNewsItems_TitleEn",
                table: "AiNewsItems");

            migrationBuilder.AlterColumn<string>(
                name: "TitleEn",
                table: "AiNewsItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
