using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GrznarAi.Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedNavMenuLocalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "LocalizationStrings",
                columns: new[] { "Id", "Description", "Key", "LanguageCode", "Value" },
                values: new object[,]
                {
                    { 43, "NavMenu Link - Home", "NavMenu.Home", "cs", "Domů" },
                    { 44, "NavMenu Link - Home", "NavMenu.Home", "en", "Home" },
                    { 45, "NavMenu Link - Blog", "NavMenu.Blog", "cs", "Blog" },
                    { 46, "NavMenu Link - Blog", "NavMenu.Blog", "en", "Blog" },
                    { 47, "NavMenu Link - Projects", "NavMenu.Projects", "cs", "Projekty" },
                    { 48, "NavMenu Link - Projects", "NavMenu.Projects", "en", "Projects" },
                    { 49, "NavMenu Link - Meteo", "NavMenu.Meteo", "cs", "Meteo" },
                    { 50, "NavMenu Link - Meteo", "NavMenu.Meteo", "en", "Meteo" },
                    { 51, "NavMenu Dropdown - Admin", "NavMenu.Admin.Title", "cs", "Administrace" },
                    { 52, "NavMenu Dropdown - Admin", "NavMenu.Admin.Title", "en", "Administration" },
                    { 53, "NavMenu Admin Link - Projects", "NavMenu.Admin.Projects", "cs", "Projekty" },
                    { 54, "NavMenu Admin Link - Projects", "NavMenu.Admin.Projects", "en", "Projects" },
                    { 55, "NavMenu Admin Link - Localization", "NavMenu.Admin.Localization", "cs", "Lokalizace" },
                    { 56, "NavMenu Admin Link - Localization", "NavMenu.Admin.Localization", "en", "Localization" },
                    { 57, "NavMenu User Dropdown - Manage", "NavMenu.User.ManageAccount", "cs", "Správa účtu" },
                    { 58, "NavMenu User Dropdown - Manage", "NavMenu.User.ManageAccount", "en", "Manage Account" },
                    { 59, "NavMenu User Dropdown - Logout", "NavMenu.User.Logout", "cs", "Odhlásit se" },
                    { 60, "NavMenu User Dropdown - Logout", "NavMenu.User.Logout", "en", "Logout" },
                    { 61, "NavMenu Auth Link - Register", "NavMenu.Auth.Register", "cs", "Registrovat" },
                    { 62, "NavMenu Auth Link - Register", "NavMenu.Auth.Register", "en", "Register" },
                    { 63, "NavMenu Auth Link - Login", "NavMenu.Auth.Login", "cs", "Přihlásit se" },
                    { 64, "NavMenu Auth Link - Login", "NavMenu.Auth.Login", "en", "Login" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 64);
        }
    }
}
