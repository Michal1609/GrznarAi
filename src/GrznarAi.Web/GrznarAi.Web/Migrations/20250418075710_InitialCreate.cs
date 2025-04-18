using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GrznarAi.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Blogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocalizationStrings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalizationStrings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GitHubUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlogContents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlogId = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogContents_Blogs_BlogId",
                        column: x => x.BlogId,
                        principalTable: "Blogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectContents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectContents_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "LocalizationStrings",
                columns: new[] { "Id", "Description", "Key", "LanguageCode", "Value" },
                values: new object[,]
                {
                    { 1, "Home page title", "HomePage.Title", "cs", "GrznarAI - Osobní Web" },
                    { 2, "Home page title", "HomePage.Title", "en", "GrznarAI - Personal Website" },
                    { 3, "Home Carousel 1 Title", "HomePage.Carousel1.Title", "cs", "Vítejte na GrznarAI" },
                    { 4, "Home Carousel 1 Title", "HomePage.Carousel1.Title", "en", "Welcome to GrznarAI" },
                    { 5, "Home Carousel 1 Lead Text", "HomePage.Carousel1.Lead", "cs", "Osobní web s blogem, projekty a meteo daty" },
                    { 6, "Home Carousel 1 Lead Text", "HomePage.Carousel1.Lead", "en", "Personal website with blog, projects and meteo data" },
                    { 7, "Carousel Button - Read Blog", "HomePage.Carousel.ReadBlogButton", "cs", "Číst Blog" },
                    { 8, "Carousel Button - Read Blog", "HomePage.Carousel.ReadBlogButton", "en", "Read Blog" },
                    { 9, "Carousel Button - View Projects", "HomePage.Carousel.ViewProjectsButton", "cs", "Zobrazit Projekty" },
                    { 10, "Carousel Button - View Projects", "HomePage.Carousel.ViewProjectsButton", "en", "View Projects" },
                    { 11, "Home Carousel 2 Title", "HomePage.Carousel2.Title", "cs", "Prozkoumejte Mé Projekty" },
                    { 12, "Home Carousel 2 Title", "HomePage.Carousel2.Title", "en", "Explore My Projects" },
                    { 13, "Home Carousel 2 Lead Text", "HomePage.Carousel2.Lead", "cs", "Podívejte se na mé nejnovější GitHub projekty a experimenty" },
                    { 14, "Home Carousel 2 Lead Text", "HomePage.Carousel2.Lead", "en", "Check out my latest GitHub projects and experiments" },
                    { 15, "Home Carousel 3 Title", "HomePage.Carousel3.Title", "cs", "Meteo Data" },
                    { 16, "Home Carousel 3 Title", "HomePage.Carousel3.Title", "en", "Meteo Data" },
                    { 17, "Home Carousel 3 Lead Text", "HomePage.Carousel3.Lead", "cs", "Prozkoumejte statistiky počasí z mých osobních meteostanic" },
                    { 18, "Home Carousel 3 Lead Text", "HomePage.Carousel3.Lead", "en", "Explore weather statistics from my personal meteo stations" },
                    { 19, "Carousel Button - View Meteo Data", "HomePage.Carousel.ViewMeteoButton", "cs", "Zobrazit Meteo Data" },
                    { 20, "Carousel Button - View Meteo Data", "HomePage.Carousel.ViewMeteoButton", "en", "View Meteo Data" },
                    { 21, "Carousel Control - Previous", "HomePage.Carousel.Previous", "cs", "Předchozí" },
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
                    { 42, "Latest Posts Button - View All", "HomePage.LatestPosts.ViewAll", "en", "View All Posts" },
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
                    { 64, "NavMenu Auth Link - Login", "NavMenu.Auth.Login", "en", "Login" },
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

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BlogContents_BlogId_LanguageCode",
                table: "BlogContents",
                columns: new[] { "BlogId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationStrings_Key_LanguageCode",
                table: "LocalizationStrings",
                columns: new[] { "Key", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectContents_ProjectId_LanguageCode",
                table: "ProjectContents",
                columns: new[] { "ProjectId", "LanguageCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BlogContents");

            migrationBuilder.DropTable(
                name: "LocalizationStrings");

            migrationBuilder.DropTable(
                name: "ProjectContents");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Blogs");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
