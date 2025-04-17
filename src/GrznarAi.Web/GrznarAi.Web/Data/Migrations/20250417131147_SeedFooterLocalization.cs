using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GrznarAi.Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedFooterLocalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "LocalizationStrings",
                columns: new[] { "Id", "Description", "Key", "LanguageCode", "Value" },
                values: new object[,]
                {
                    { 65, "Footer Heading - Links", "Footer.Links", "cs", "Odkazy" },
                    { 66, "Footer Heading - Links", "Footer.Links", "en", "Links" },
                    { 67, "Footer Heading - Connect", "Footer.Connect", "cs", "Spojte se" },
                    { 68, "Footer Heading - Connect", "Footer.Connect", "en", "Connect" },
                    { 69, "Footer Link - GitHub", "Footer.GitHub", "cs", "GitHub" },
                    { 70, "Footer Link - GitHub", "Footer.GitHub", "en", "GitHub" },
                    { 71, "Footer Link - Contact", "Footer.Contact", "cs", "Kontakt" },
                    { 72, "Footer Link - Contact", "Footer.Contact", "en", "Contact" },
                    { 73, "Footer Copyright Text (with year placeholder {0})", "Footer.Copyright", "cs", "&copy; {0} GrznarAI. Všechna práva vyhrazena." },
                    { 74, "Footer Copyright Text (with year placeholder {0})", "Footer.Copyright", "en", "&copy; {0} GrznarAI. All rights reserved." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 74);
        }
    }
}
