using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrznarAi.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAiNewsErrorProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiNewsSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    LastFetched = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiNewsSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiNewsErrors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StackTrace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceId = table.Column<int>(type: "int", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiNewsErrors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiNewsErrors_AiNewsSources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "AiNewsSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiNewsErrors_OccurredAt",
                table: "AiNewsErrors",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_AiNewsErrors_SourceId",
                table: "AiNewsErrors",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_AiNewsSources_Name",
                table: "AiNewsSources",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiNewsSources_Type",
                table: "AiNewsSources",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_AiNewsSources_Url",
                table: "AiNewsSources",
                column: "Url");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiNewsErrors");

            migrationBuilder.DropTable(
                name: "AiNewsSources");
        }
    }
}
