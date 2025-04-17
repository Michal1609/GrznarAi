using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GrznarAi.Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedHomePageLocalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "LocalizationStrings",
                columns: new[] { "Id", "Description", "Key", "ValueCs", "ValueEn" },
                values: new object[,]
                {
                    { 1, "Home page title", "HomePage.Title", "GrznarAI - Osobní Web", "GrznarAI - Personal Website" },
                    { 2, "Home Carousel 1 Title", "HomePage.Carousel1.Title", "Vítejte na GrznarAI", "Welcome to GrznarAI" },
                    { 3, "Home Carousel 1 Lead Text", "HomePage.Carousel1.Lead", "Osobní web s blogem, projekty a meteo daty", "Personal website with blog, projects and meteo data" },
                    { 4, "Carousel Button - Read Blog", "HomePage.Carousel.ReadBlogButton", "Číst Blog", "Read Blog" },
                    { 5, "Carousel Button - View Projects", "HomePage.Carousel.ViewProjectsButton", "Zobrazit Projekty", "View Projects" },
                    { 6, "Home Carousel 2 Title", "HomePage.Carousel2.Title", "Prozkoumejte Mé Projekty", "Explore My Projects" },
                    { 7, "Home Carousel 2 Lead Text", "HomePage.Carousel2.Lead", "Podívejte se na mé nejnovější GitHub projekty a experimenty", "Check out my latest GitHub projects and experiments" },
                    { 8, "Home Carousel 3 Title", "HomePage.Carousel3.Title", "Meteo Data", "Meteo Data" },
                    { 9, "Home Carousel 3 Lead Text", "HomePage.Carousel3.Lead", "Prozkoumejte statistiky počasí z mých osobních meteostanic", "Explore weather statistics from my personal meteo stations" },
                    { 10, "Carousel Button - View Meteo Data", "HomePage.Carousel.ViewMeteoButton", "Zobrazit Meteo Data", "View Meteo Data" },
                    { 11, "Carousel Control - Previous", "HomePage.Carousel.Previous", "Předchozí", "Previous" },
                    { 12, "Carousel Control - Next", "HomePage.Carousel.Next", "Další", "Next" },
                    { 13, "Featured Card Title - Blog", "HomePage.Featured.Blog.Title", "Blog", "Blog" },
                    { 14, "Featured Card Text - Blog", "HomePage.Featured.Blog.Text", "Sdílejte články ve formátu Markdown s full-text vyhledáváním a komentáři.", "Share articles in Markdown format with full-text search and commenting capabilities." },
                    { 15, "Featured Card Title - Projects", "HomePage.Featured.Projects.Title", "Projekty", "Projects" },
                    { 16, "Featured Card Text - Projects", "HomePage.Featured.Projects.Text", "Prozkoumejte mé GitHub projekty s dokumentací, changelogy a demy.", "Explore my GitHub projects with documentation, changelogs, and demos." },
                    { 17, "Featured Card Title - Meteo", "HomePage.Featured.Meteo.Title", "Meteo Data", "Meteo Data" },
                    { 18, "Featured Card Text - Meteo", "HomePage.Featured.Meteo.Text", "Získejte přístup ke statistikám počasí a datům z osobních meteostanic.", "Access weather statistics and data from personal meteo stations." },
                    { 19, "Latest Posts Section Title", "HomePage.LatestPosts.Title", "Nejnovější Příspěvky na Blogu", "Latest Blog Posts" },
                    { 20, "Latest Posts Button - Read More", "HomePage.LatestPosts.ReadMore", "Číst Více", "Read More" },
                    { 21, "Latest Posts Button - View All", "HomePage.LatestPosts.ViewAll", "Zobrazit Všechny Příspěvky", "View All Posts" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "LocalizationStrings",
                keyColumn: "Id",
                keyValue: 21);
        }
    }
}
