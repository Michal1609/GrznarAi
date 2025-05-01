using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrznarAi.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadingDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NoteCategoryRelations_Notes_NoteId",
                table: "NoteCategoryRelations");

            migrationBuilder.AddForeignKey(
                name: "FK_NoteCategoryRelations_Notes_NoteId",
                table: "NoteCategoryRelations",
                column: "NoteId",
                principalTable: "Notes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NoteCategoryRelations_Notes_NoteId",
                table: "NoteCategoryRelations");

            migrationBuilder.AddForeignKey(
                name: "FK_NoteCategoryRelations_Notes_NoteId",
                table: "NoteCategoryRelations",
                column: "NoteId",
                principalTable: "Notes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
