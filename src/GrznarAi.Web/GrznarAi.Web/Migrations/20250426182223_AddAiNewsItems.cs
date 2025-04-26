using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrznarAi.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAiNewsItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiNewsItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TitleEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleCz = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContentEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentCz = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SummaryEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SummaryCz = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SourceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ImportedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiNewsItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiNewsItems_ImportedDate",
                table: "AiNewsItems",
                column: "ImportedDate");

            migrationBuilder.CreateIndex(
                name: "IX_AiNewsItems_PublishedDate",
                table: "AiNewsItems",
                column: "PublishedDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiNewsItems");
        }
    }
}
