using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GrznarAi.Web.Migrations
{
    /// <inheritdoc />
    public partial class RefactorLocalizationSchemaAndSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LocalizationStrings_Key",
                table: "LocalizationStrings");

            migrationBuilder.DropColumn(
                name: "ValueCs",
                table: "LocalizationStrings");

            migrationBuilder.RenameColumn(
                name: "ValueEn",
                table: "LocalizationStrings",
                newName: "Value");

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "LocalizationStrings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "LanguageCode", "Value" },
                values: new object[] { "cs", "GrznarAI - Osobní Web" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Key", "LanguageCode", "Value" },
                values: new object[] { "Home page title", "HomePage.Title", "en", "GrznarAI - Personal Website" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Key", "LanguageCode", "Value" },
                values: new object[] { "Home Carousel 1 Title", "HomePage.Carousel1.Title", "cs", "Vítejte na GrznarAI" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Key", "LanguageCode", "Value" },
                values: new object[] { "Home Carousel 1 Title", "HomePage.Carousel1.Title", "en", "Welcome to GrznarAI" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Key", "LanguageCode", "Value" },
                values: new object[] { "Home Carousel 1 Lead Text", "HomePage.Carousel1.Lead", "cs", "Osobní web s blogem, projekty a meteo daty" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "Key", "LanguageCode", "Value" },
                values: new object[] { "Home Carousel 1 Lead Text", "HomePage.Carousel1.Lead", "en", "Personal website with blog, projects and meteo data" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Description", "Key", "LanguageCode", "Value" },
                values: new object[] { "Carousel Button - Read Blog", "HomePage.Carousel.ReadBlogButton", "cs", "Číst Blog" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Description", "Key", "LanguageCode", "Value" },
                values: new object[] { "Carousel Button - Read Blog", "HomePage.Carousel.ReadBlogButton", "en", "Read Blog" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Description", "Key", "LanguageCode", "Value" },
                values: new object[] { "Carousel Button - View Projects", "HomePage.Carousel.ViewProjectsButton", "cs", "Zobrazit Projekty" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Description", "Key", "LanguageCode", "Value" },
                values: new object[] { "Carousel Button - View Projects", "HomePage.Carousel.ViewProjectsButton", "en", "View Projects" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Description", "Key", "LanguageCode", "Value" },
                values: new object[] { "Home Carousel 2 Title", "HomePage.Carousel2.Title", "cs", "Prozkoumejte Mé Projekty" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Description", "Key", "LanguageCode", "Value" },
                values: new object[] { "Home Carousel 2 Title", "HomePage.Carousel2.Title", "en", "Explore My Projects" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Description", "Key", "LanguageCode", "Value" },
                values: new object[] { "Home Carousel 2 Lead Text", "HomePage.Carousel2.Lead", "cs", "Podívejte se na mé nejnovější GitHub projekty a experimenty" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Description", "Key", "LanguageCode", "Value" },
                values: new object[] { "Home Carousel 2 Lead Text", "HomePage.Carousel2.Lead", "en", "Check out my latest GitHub projects and experiments" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Description", "Key", "LanguageCode", "Value" },
                values: new object[] { "Home Carousel 3 Title", "HomePage.Carousel3.Title", "cs", "Meteo Data" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Description", "Key", "LanguageCode", "Value" },
                values: new object[] { "Home Carousel 3 Title", "HomePage.Carousel3.Title", "en", "Meteo Data" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Description", "Key", "LanguageCode", "Value" },
                values: new object[] { "Home Carousel 3 Lead Text", "HomePage.Carousel3.Lead", "cs", "Prozkoumejte statistiky počasí z mých osobních meteostanic" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Description", "Key", "LanguageCode", "Value" },
                values: new object[] { "Home Carousel 3 Lead Text", "HomePage.Carousel3.Lead", "en", "Explore weather statistics from my personal meteo stations" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Description", "Key", "LanguageCode", "Value" },
                values: new object[] { "Carousel Button - View Meteo Data", "HomePage.Carousel.ViewMeteoButton", "cs", "Zobrazit Meteo Data" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Description", "Key", "LanguageCode", "Value" },
                values: new object[] { "Carousel Button - View Meteo Data", "HomePage.Carousel.ViewMeteoButton", "en", "View Meteo Data" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Description", "Key", "LanguageCode", "Value" },
                values: new object[] { "Carousel Control - Previous", "HomePage.Carousel.Previous", "cs", "Předchozí" });

            migrationBuilder.InsertData(
                table: "LocalizationStrings",
                columns: new[] { "Id", "Description", "Key", "LanguageCode", "Value" },
                values: new object[,]
                {
                    { 22, "Carousel Control - Previous", "HomePage.Carousel.Previous", "en", "Previous" },
                    { 23, "Carousel Control - Next", "HomePage.Carousel.Next", "cs", "Další" },
                    { 24, "Carousel Control - Next", "HomePage.Carousel.Next", "en", "Next" },
                    { 25, "Featured Card Title - Blog", "HomePage.Featured.Blog.Title", "cs", "Blog" },
                    { 26, "Featured Card Title - Blog", "HomePage.Featured.Blog.Title", "en", "Blog" },
                    { 27, "Featured Card Text - Blog", "HomePage.Featured.Blog.Text", "cs", "Sdílejte články ve formátu Markdown s full-text vyhledáváním a komentáři." },
                    { 28, "Featured Card Text - Blog", "HomePage.Featured.Blog.Text", "en", "Share articles in Markdown format with full-text search and commenting capabilities." },
                    { 29, "Featured Card Title - Projects", "HomePage.Featured.Projects.Title", "cs", "Projekty" },
                    { 30, "Featured Card Title - Projects", "HomePage.Featured.Projects.Title", "en", "Projects" },
                    { 31, "Featured Card Text - Projects", "HomePage.Featured.Projects.Text", "cs", "Prozkoumejte mé GitHub projekty s dokumentací, changelogy a demy." },
                    { 32, "Featured Card Text - Projects", "HomePage.Featured.Projects.Text", "en", "Explore my GitHub projects with documentation, changelogs, and demos." },
                    { 33, "Featured Card Title - Meteo", "HomePage.Featured.Meteo.Title", "cs", "Meteo Data" },
                    { 34, "Featured Card Title - Meteo", "HomePage.Featured.Meteo.Title", "en", "Meteo Data" },
                    { 35, "Featured Card Text - Meteo", "HomePage.Featured.Meteo.Text", "cs", "Získejte přístup ke statistikám počasí a datům z osobních meteostanic." },
                    { 36, "Featured Card Text - Meteo", "HomePage.Featured.Meteo.Text", "en", "Access weather statistics and data from personal meteo stations." },
                    { 37, "Latest Posts Section Title", "HomePage.LatestPosts.Title", "cs", "Nejnovější Příspěvky na Blogu" },
                    { 38, "Latest Posts Section Title", "HomePage.LatestPosts.Title", "en", "Latest Blog Posts" },
                    { 39, "Latest Posts Button - Read More", "HomePage.LatestPosts.ReadMore", "cs", "Číst Více" },
                    { 40, "Latest Posts Button - Read More", "HomePage.LatestPosts.ReadMore", "en", "Read More" },
                    { 41, "Latest Posts Button - View All", "HomePage.LatestPosts.ViewAll", "cs", "Zobrazit Všechny Příspěvky" },
                    { 42, "Latest Posts Button - View All", "HomePage.LatestPosts.ViewAll", "en", "View All Posts" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationStrings_Key_LanguageCode",
                table: "LocalizationStrings",
                columns: new[] { "Key", "LanguageCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LocalizationStrings_Key_LanguageCode",
                table: "LocalizationStrings");

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "LocalizationStrings");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "LocalizationStrings",
                newName: "ValueEn");

            migrationBuilder.AddColumn<string>(
                name: "ValueCs",
                table: "LocalizationStrings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ValueCs", "ValueEn" },
                values: new object[] { "GrznarAI - Osobní Web", "GrznarAI - Personal Website" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Key", "ValueCs", "ValueEn" },
                values: new object[] { "Home Carousel 1 Title", "HomePage.Carousel1.Title", "Vítejte na GrznarAI", "Welcome to GrznarAI" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Key", "ValueCs", "ValueEn" },
                values: new object[] { "Home Carousel 1 Lead Text", "HomePage.Carousel1.Lead", "Osobní web s blogem, projekty a meteo daty", "Personal website with blog, projects and meteo data" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Key", "ValueCs", "ValueEn" },
                values: new object[] { "Carousel Button - Read Blog", "HomePage.Carousel.ReadBlogButton", "Číst Blog", "Read Blog" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Key", "ValueCs", "ValueEn" },
                values: new object[] { "Carousel Button - View Projects", "HomePage.Carousel.ViewProjectsButton", "Zobrazit Projekty", "View Projects" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "Key", "ValueCs", "ValueEn" },
                values: new object[] { "Home Carousel 2 Title", "HomePage.Carousel2.Title", "Prozkoumejte Mé Projekty", "Explore My Projects" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Description", "Key", "ValueCs", "ValueEn" },
                values: new object[] { "Home Carousel 2 Lead Text", "HomePage.Carousel2.Lead", "Podívejte se na mé nejnovější GitHub projekty a experimenty", "Check out my latest GitHub projects and experiments" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Description", "Key", "ValueCs", "ValueEn" },
                values: new object[] { "Home Carousel 3 Title", "HomePage.Carousel3.Title", "Meteo Data", "Meteo Data" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Description", "Key", "ValueCs", "ValueEn" },
                values: new object[] { "Home Carousel 3 Lead Text", "HomePage.Carousel3.Lead", "Prozkoumejte statistiky počasí z mých osobních meteostanic", "Explore weather statistics from my personal meteo stations" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Description", "Key", "ValueCs", "ValueEn" },
                values: new object[] { "Carousel Button - View Meteo Data", "HomePage.Carousel.ViewMeteoButton", "Zobrazit Meteo Data", "View Meteo Data" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Description", "Key", "ValueCs", "ValueEn" },
                values: new object[] { "Carousel Control - Previous", "HomePage.Carousel.Previous", "Předchozí", "Previous" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Description", "Key", "ValueCs", "ValueEn" },
                values: new object[] { "Carousel Control - Next", "HomePage.Carousel.Next", "Další", "Next" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Description", "Key", "ValueCs", "ValueEn" },
                values: new object[] { "Featured Card Title - Blog", "HomePage.Featured.Blog.Title", "Blog", "Blog" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Description", "Key", "ValueCs", "ValueEn" },
                values: new object[] { "Featured Card Text - Blog", "HomePage.Featured.Blog.Text", "Sdílejte články ve formátu Markdown s full-text vyhledáváním a komentáři.", "Share articles in Markdown format with full-text search and commenting capabilities." });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Description", "Key", "ValueCs", "ValueEn" },
                values: new object[] { "Featured Card Title - Projects", "HomePage.Featured.Projects.Title", "Projekty", "Projects" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Description", "Key", "ValueCs", "ValueEn" },
                values: new object[] { "Featured Card Text - Projects", "HomePage.Featured.Projects.Text", "Prozkoumejte mé GitHub projekty s dokumentací, changelogy a demy.", "Explore my GitHub projects with documentation, changelogs, and demos." });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Description", "Key", "ValueCs", "ValueEn" },
                values: new object[] { "Featured Card Title - Meteo", "HomePage.Featured.Meteo.Title", "Meteo Data", "Meteo Data" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Description", "Key", "ValueCs", "ValueEn" },
                values: new object[] { "Featured Card Text - Meteo", "HomePage.Featured.Meteo.Text", "Získejte přístup ke statistikám počasí a datům z osobních meteostanic.", "Access weather statistics and data from personal meteo stations." });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Description", "Key", "ValueCs", "ValueEn" },
                values: new object[] { "Latest Posts Section Title", "HomePage.LatestPosts.Title", "Nejnovější Příspěvky na Blogu", "Latest Blog Posts" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Description", "Key", "ValueCs", "ValueEn" },
                values: new object[] { "Latest Posts Button - Read More", "HomePage.LatestPosts.ReadMore", "Číst Více", "Read More" });

            migrationBuilder.UpdateData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Description", "Key", "ValueCs", "ValueEn" },
                values: new object[] { "Latest Posts Button - View All", "HomePage.LatestPosts.ViewAll", "Zobrazit Všechny Příspěvky", "View All Posts" });

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationStrings_Key",
                table: "LocalizationStrings",
                column: "Key",
                unique: true);
        }
    }
}
