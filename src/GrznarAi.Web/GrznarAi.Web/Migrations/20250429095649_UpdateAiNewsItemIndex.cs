using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrznarAi.Web.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAiNewsItemIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AiNewsItems_TitleEn",
                table: "AiNewsItems");

            migrationBuilder.CreateIndex(
                name: "IX_AiNewsItems_TitleEn_ImportedDate",
                table: "AiNewsItems",
                columns: new[] { "TitleEn", "ImportedDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AiNewsItems_TitleEn_ImportedDate",
                table: "AiNewsItems");

            migrationBuilder.CreateIndex(
                name: "IX_AiNewsItems_TitleEn",
                table: "AiNewsItems",
                column: "TitleEn");
        }
    }
}
