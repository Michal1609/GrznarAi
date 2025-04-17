﻿// <auto-generated />
using System;
using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace GrznarAi.Web.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250417131147_SeedFooterLocalization")]
    partial class SeedFooterLocalization
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("GrznarAi.Web.Data.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("GrznarAi.Web.Data.LocalizationString", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("LanguageCode")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("Key", "LanguageCode")
                        .IsUnique();

                    b.ToTable("LocalizationStrings");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Description = "Home page title",
                            Key = "HomePage.Title",
                            LanguageCode = "cs",
                            Value = "GrznarAI - Osobní Web"
                        },
                        new
                        {
                            Id = 2,
                            Description = "Home page title",
                            Key = "HomePage.Title",
                            LanguageCode = "en",
                            Value = "GrznarAI - Personal Website"
                        },
                        new
                        {
                            Id = 3,
                            Description = "Home Carousel 1 Title",
                            Key = "HomePage.Carousel1.Title",
                            LanguageCode = "cs",
                            Value = "Vítejte na GrznarAI"
                        },
                        new
                        {
                            Id = 4,
                            Description = "Home Carousel 1 Title",
                            Key = "HomePage.Carousel1.Title",
                            LanguageCode = "en",
                            Value = "Welcome to GrznarAI"
                        },
                        new
                        {
                            Id = 5,
                            Description = "Home Carousel 1 Lead Text",
                            Key = "HomePage.Carousel1.Lead",
                            LanguageCode = "cs",
                            Value = "Osobní web s blogem, projekty a meteo daty"
                        },
                        new
                        {
                            Id = 6,
                            Description = "Home Carousel 1 Lead Text",
                            Key = "HomePage.Carousel1.Lead",
                            LanguageCode = "en",
                            Value = "Personal website with blog, projects and meteo data"
                        },
                        new
                        {
                            Id = 7,
                            Description = "Carousel Button - Read Blog",
                            Key = "HomePage.Carousel.ReadBlogButton",
                            LanguageCode = "cs",
                            Value = "Číst Blog"
                        },
                        new
                        {
                            Id = 8,
                            Description = "Carousel Button - Read Blog",
                            Key = "HomePage.Carousel.ReadBlogButton",
                            LanguageCode = "en",
                            Value = "Read Blog"
                        },
                        new
                        {
                            Id = 9,
                            Description = "Carousel Button - View Projects",
                            Key = "HomePage.Carousel.ViewProjectsButton",
                            LanguageCode = "cs",
                            Value = "Zobrazit Projekty"
                        },
                        new
                        {
                            Id = 10,
                            Description = "Carousel Button - View Projects",
                            Key = "HomePage.Carousel.ViewProjectsButton",
                            LanguageCode = "en",
                            Value = "View Projects"
                        },
                        new
                        {
                            Id = 11,
                            Description = "Home Carousel 2 Title",
                            Key = "HomePage.Carousel2.Title",
                            LanguageCode = "cs",
                            Value = "Prozkoumejte Mé Projekty"
                        },
                        new
                        {
                            Id = 12,
                            Description = "Home Carousel 2 Title",
                            Key = "HomePage.Carousel2.Title",
                            LanguageCode = "en",
                            Value = "Explore My Projects"
                        },
                        new
                        {
                            Id = 13,
                            Description = "Home Carousel 2 Lead Text",
                            Key = "HomePage.Carousel2.Lead",
                            LanguageCode = "cs",
                            Value = "Podívejte se na mé nejnovější GitHub projekty a experimenty"
                        },
                        new
                        {
                            Id = 14,
                            Description = "Home Carousel 2 Lead Text",
                            Key = "HomePage.Carousel2.Lead",
                            LanguageCode = "en",
                            Value = "Check out my latest GitHub projects and experiments"
                        },
                        new
                        {
                            Id = 15,
                            Description = "Home Carousel 3 Title",
                            Key = "HomePage.Carousel3.Title",
                            LanguageCode = "cs",
                            Value = "Meteo Data"
                        },
                        new
                        {
                            Id = 16,
                            Description = "Home Carousel 3 Title",
                            Key = "HomePage.Carousel3.Title",
                            LanguageCode = "en",
                            Value = "Meteo Data"
                        },
                        new
                        {
                            Id = 17,
                            Description = "Home Carousel 3 Lead Text",
                            Key = "HomePage.Carousel3.Lead",
                            LanguageCode = "cs",
                            Value = "Prozkoumejte statistiky počasí z mých osobních meteostanic"
                        },
                        new
                        {
                            Id = 18,
                            Description = "Home Carousel 3 Lead Text",
                            Key = "HomePage.Carousel3.Lead",
                            LanguageCode = "en",
                            Value = "Explore weather statistics from my personal meteo stations"
                        },
                        new
                        {
                            Id = 19,
                            Description = "Carousel Button - View Meteo Data",
                            Key = "HomePage.Carousel.ViewMeteoButton",
                            LanguageCode = "cs",
                            Value = "Zobrazit Meteo Data"
                        },
                        new
                        {
                            Id = 20,
                            Description = "Carousel Button - View Meteo Data",
                            Key = "HomePage.Carousel.ViewMeteoButton",
                            LanguageCode = "en",
                            Value = "View Meteo Data"
                        },
                        new
                        {
                            Id = 21,
                            Description = "Carousel Control - Previous",
                            Key = "HomePage.Carousel.Previous",
                            LanguageCode = "cs",
                            Value = "Předchozí"
                        },
                        new
                        {
                            Id = 22,
                            Description = "Carousel Control - Previous",
                            Key = "HomePage.Carousel.Previous",
                            LanguageCode = "en",
                            Value = "Previous"
                        },
                        new
                        {
                            Id = 23,
                            Description = "Carousel Control - Next",
                            Key = "HomePage.Carousel.Next",
                            LanguageCode = "cs",
                            Value = "Další"
                        },
                        new
                        {
                            Id = 24,
                            Description = "Carousel Control - Next",
                            Key = "HomePage.Carousel.Next",
                            LanguageCode = "en",
                            Value = "Next"
                        },
                        new
                        {
                            Id = 25,
                            Description = "Featured Card Title - Blog",
                            Key = "HomePage.Featured.Blog.Title",
                            LanguageCode = "cs",
                            Value = "Blog"
                        },
                        new
                        {
                            Id = 26,
                            Description = "Featured Card Title - Blog",
                            Key = "HomePage.Featured.Blog.Title",
                            LanguageCode = "en",
                            Value = "Blog"
                        },
                        new
                        {
                            Id = 27,
                            Description = "Featured Card Text - Blog",
                            Key = "HomePage.Featured.Blog.Text",
                            LanguageCode = "cs",
                            Value = "Sdílejte články ve formátu Markdown s full-text vyhledáváním a komentáři."
                        },
                        new
                        {
                            Id = 28,
                            Description = "Featured Card Text - Blog",
                            Key = "HomePage.Featured.Blog.Text",
                            LanguageCode = "en",
                            Value = "Share articles in Markdown format with full-text search and commenting capabilities."
                        },
                        new
                        {
                            Id = 29,
                            Description = "Featured Card Title - Projects",
                            Key = "HomePage.Featured.Projects.Title",
                            LanguageCode = "cs",
                            Value = "Projekty"
                        },
                        new
                        {
                            Id = 30,
                            Description = "Featured Card Title - Projects",
                            Key = "HomePage.Featured.Projects.Title",
                            LanguageCode = "en",
                            Value = "Projects"
                        },
                        new
                        {
                            Id = 31,
                            Description = "Featured Card Text - Projects",
                            Key = "HomePage.Featured.Projects.Text",
                            LanguageCode = "cs",
                            Value = "Prozkoumejte mé GitHub projekty s dokumentací, changelogy a demy."
                        },
                        new
                        {
                            Id = 32,
                            Description = "Featured Card Text - Projects",
                            Key = "HomePage.Featured.Projects.Text",
                            LanguageCode = "en",
                            Value = "Explore my GitHub projects with documentation, changelogs, and demos."
                        },
                        new
                        {
                            Id = 33,
                            Description = "Featured Card Title - Meteo",
                            Key = "HomePage.Featured.Meteo.Title",
                            LanguageCode = "cs",
                            Value = "Meteo Data"
                        },
                        new
                        {
                            Id = 34,
                            Description = "Featured Card Title - Meteo",
                            Key = "HomePage.Featured.Meteo.Title",
                            LanguageCode = "en",
                            Value = "Meteo Data"
                        },
                        new
                        {
                            Id = 35,
                            Description = "Featured Card Text - Meteo",
                            Key = "HomePage.Featured.Meteo.Text",
                            LanguageCode = "cs",
                            Value = "Získejte přístup ke statistikám počasí a datům z osobních meteostanic."
                        },
                        new
                        {
                            Id = 36,
                            Description = "Featured Card Text - Meteo",
                            Key = "HomePage.Featured.Meteo.Text",
                            LanguageCode = "en",
                            Value = "Access weather statistics and data from personal meteo stations."
                        },
                        new
                        {
                            Id = 37,
                            Description = "Latest Posts Section Title",
                            Key = "HomePage.LatestPosts.Title",
                            LanguageCode = "cs",
                            Value = "Nejnovější Příspěvky na Blogu"
                        },
                        new
                        {
                            Id = 38,
                            Description = "Latest Posts Section Title",
                            Key = "HomePage.LatestPosts.Title",
                            LanguageCode = "en",
                            Value = "Latest Blog Posts"
                        },
                        new
                        {
                            Id = 39,
                            Description = "Latest Posts Button - Read More",
                            Key = "HomePage.LatestPosts.ReadMore",
                            LanguageCode = "cs",
                            Value = "Číst Více"
                        },
                        new
                        {
                            Id = 40,
                            Description = "Latest Posts Button - Read More",
                            Key = "HomePage.LatestPosts.ReadMore",
                            LanguageCode = "en",
                            Value = "Read More"
                        },
                        new
                        {
                            Id = 41,
                            Description = "Latest Posts Button - View All",
                            Key = "HomePage.LatestPosts.ViewAll",
                            LanguageCode = "cs",
                            Value = "Zobrazit Všechny Příspěvky"
                        },
                        new
                        {
                            Id = 42,
                            Description = "Latest Posts Button - View All",
                            Key = "HomePage.LatestPosts.ViewAll",
                            LanguageCode = "en",
                            Value = "View All Posts"
                        },
                        new
                        {
                            Id = 43,
                            Description = "NavMenu Link - Home",
                            Key = "NavMenu.Home",
                            LanguageCode = "cs",
                            Value = "Domů"
                        },
                        new
                        {
                            Id = 44,
                            Description = "NavMenu Link - Home",
                            Key = "NavMenu.Home",
                            LanguageCode = "en",
                            Value = "Home"
                        },
                        new
                        {
                            Id = 45,
                            Description = "NavMenu Link - Blog",
                            Key = "NavMenu.Blog",
                            LanguageCode = "cs",
                            Value = "Blog"
                        },
                        new
                        {
                            Id = 46,
                            Description = "NavMenu Link - Blog",
                            Key = "NavMenu.Blog",
                            LanguageCode = "en",
                            Value = "Blog"
                        },
                        new
                        {
                            Id = 47,
                            Description = "NavMenu Link - Projects",
                            Key = "NavMenu.Projects",
                            LanguageCode = "cs",
                            Value = "Projekty"
                        },
                        new
                        {
                            Id = 48,
                            Description = "NavMenu Link - Projects",
                            Key = "NavMenu.Projects",
                            LanguageCode = "en",
                            Value = "Projects"
                        },
                        new
                        {
                            Id = 49,
                            Description = "NavMenu Link - Meteo",
                            Key = "NavMenu.Meteo",
                            LanguageCode = "cs",
                            Value = "Meteo"
                        },
                        new
                        {
                            Id = 50,
                            Description = "NavMenu Link - Meteo",
                            Key = "NavMenu.Meteo",
                            LanguageCode = "en",
                            Value = "Meteo"
                        },
                        new
                        {
                            Id = 51,
                            Description = "NavMenu Dropdown - Admin",
                            Key = "NavMenu.Admin.Title",
                            LanguageCode = "cs",
                            Value = "Administrace"
                        },
                        new
                        {
                            Id = 52,
                            Description = "NavMenu Dropdown - Admin",
                            Key = "NavMenu.Admin.Title",
                            LanguageCode = "en",
                            Value = "Administration"
                        },
                        new
                        {
                            Id = 53,
                            Description = "NavMenu Admin Link - Projects",
                            Key = "NavMenu.Admin.Projects",
                            LanguageCode = "cs",
                            Value = "Projekty"
                        },
                        new
                        {
                            Id = 54,
                            Description = "NavMenu Admin Link - Projects",
                            Key = "NavMenu.Admin.Projects",
                            LanguageCode = "en",
                            Value = "Projects"
                        },
                        new
                        {
                            Id = 55,
                            Description = "NavMenu Admin Link - Localization",
                            Key = "NavMenu.Admin.Localization",
                            LanguageCode = "cs",
                            Value = "Lokalizace"
                        },
                        new
                        {
                            Id = 56,
                            Description = "NavMenu Admin Link - Localization",
                            Key = "NavMenu.Admin.Localization",
                            LanguageCode = "en",
                            Value = "Localization"
                        },
                        new
                        {
                            Id = 57,
                            Description = "NavMenu User Dropdown - Manage",
                            Key = "NavMenu.User.ManageAccount",
                            LanguageCode = "cs",
                            Value = "Správa účtu"
                        },
                        new
                        {
                            Id = 58,
                            Description = "NavMenu User Dropdown - Manage",
                            Key = "NavMenu.User.ManageAccount",
                            LanguageCode = "en",
                            Value = "Manage Account"
                        },
                        new
                        {
                            Id = 59,
                            Description = "NavMenu User Dropdown - Logout",
                            Key = "NavMenu.User.Logout",
                            LanguageCode = "cs",
                            Value = "Odhlásit se"
                        },
                        new
                        {
                            Id = 60,
                            Description = "NavMenu User Dropdown - Logout",
                            Key = "NavMenu.User.Logout",
                            LanguageCode = "en",
                            Value = "Logout"
                        },
                        new
                        {
                            Id = 61,
                            Description = "NavMenu Auth Link - Register",
                            Key = "NavMenu.Auth.Register",
                            LanguageCode = "cs",
                            Value = "Registrovat"
                        },
                        new
                        {
                            Id = 62,
                            Description = "NavMenu Auth Link - Register",
                            Key = "NavMenu.Auth.Register",
                            LanguageCode = "en",
                            Value = "Register"
                        },
                        new
                        {
                            Id = 63,
                            Description = "NavMenu Auth Link - Login",
                            Key = "NavMenu.Auth.Login",
                            LanguageCode = "cs",
                            Value = "Přihlásit se"
                        },
                        new
                        {
                            Id = 64,
                            Description = "NavMenu Auth Link - Login",
                            Key = "NavMenu.Auth.Login",
                            LanguageCode = "en",
                            Value = "Login"
                        },
                        new
                        {
                            Id = 65,
                            Description = "Footer Heading - Links",
                            Key = "Footer.Links",
                            LanguageCode = "cs",
                            Value = "Odkazy"
                        },
                        new
                        {
                            Id = 66,
                            Description = "Footer Heading - Links",
                            Key = "Footer.Links",
                            LanguageCode = "en",
                            Value = "Links"
                        },
                        new
                        {
                            Id = 67,
                            Description = "Footer Heading - Connect",
                            Key = "Footer.Connect",
                            LanguageCode = "cs",
                            Value = "Spojte se"
                        },
                        new
                        {
                            Id = 68,
                            Description = "Footer Heading - Connect",
                            Key = "Footer.Connect",
                            LanguageCode = "en",
                            Value = "Connect"
                        },
                        new
                        {
                            Id = 69,
                            Description = "Footer Link - GitHub",
                            Key = "Footer.GitHub",
                            LanguageCode = "cs",
                            Value = "GitHub"
                        },
                        new
                        {
                            Id = 70,
                            Description = "Footer Link - GitHub",
                            Key = "Footer.GitHub",
                            LanguageCode = "en",
                            Value = "GitHub"
                        },
                        new
                        {
                            Id = 71,
                            Description = "Footer Link - Contact",
                            Key = "Footer.Contact",
                            LanguageCode = "cs",
                            Value = "Kontakt"
                        },
                        new
                        {
                            Id = 72,
                            Description = "Footer Link - Contact",
                            Key = "Footer.Contact",
                            LanguageCode = "en",
                            Value = "Contact"
                        },
                        new
                        {
                            Id = 73,
                            Description = "Footer Copyright Text (with year placeholder {0})",
                            Key = "Footer.Copyright",
                            LanguageCode = "cs",
                            Value = "&copy; {0} GrznarAI. Všechna práva vyhrazena."
                        },
                        new
                        {
                            Id = 74,
                            Description = "Footer Copyright Text (with year placeholder {0})",
                            Key = "Footer.Copyright",
                            LanguageCode = "en",
                            Value = "&copy; {0} GrznarAI. All rights reserved."
                        });
                });

            modelBuilder.Entity("GrznarAi.Web.Data.Project", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("GitHubUrl")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("GrznarAi.Web.Data.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("GrznarAi.Web.Data.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrznarAi.Web.Data.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("GrznarAi.Web.Data.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
