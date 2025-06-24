using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrznarAi.Web.Data;
using GrznarAi.Web.Services;
using GrznarAi.Web.Api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GrznarAi.Web.Tests
{
    /// <summary>
    /// Testy pro BroadcastAnnouncementService - filtrování podle textu a data
    /// </summary>
    public class BroadcastAnnouncementServiceTests
    {
        // TODO: Implementace testů pro:
        // - Filtrování podle textu (case insensitive, partial match)
        // - Filtrování podle data (přesný den)
        // - Kombinaci obou filtrů
        // - Okrajové případy (prázdný dotaz, null datum, žádné výsledky)

        [Fact]
        public async Task GetPagedAnnouncementsAsync_FiltersByText_CaseInsensitivePartialMatch()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(options);
            context.BroadcastAnnouncements.AddRange(new[]
            {
                new BroadcastAnnouncement { Content = "Hlášení o vodě", BroadcastDateTime = DateTime.Today, IsActive = true },
                new BroadcastAnnouncement { Content = "Odstávka elektřiny", BroadcastDateTime = DateTime.Today, IsActive = true },
                new BroadcastAnnouncement { Content = "Vodovodní havárie", BroadcastDateTime = DateTime.Today, IsActive = true },
                new BroadcastAnnouncement { Content = "Jiná zpráva", BroadcastDateTime = DateTime.Today, IsActive = true }
            });
            context.SaveChanges();

            var contextFactoryMock = new Mock<IDbContextFactory<ApplicationDbContext>>();
            contextFactoryMock.Setup(f => f.CreateDbContextAsync(default)).ReturnsAsync(context);
            var globalSettingsMock = new Mock<IGlobalSettingsService>();
            globalSettingsMock.Setup(s => s.GetInt(It.IsAny<string>(), It.IsAny<int>())).Returns(10);
            var loggerMock = new Mock<ILogger<BroadcastAnnouncementService>>();

            var service = new BroadcastAnnouncementService(contextFactoryMock.Object, globalSettingsMock.Object, loggerMock.Object);

            // Act
            var result = await service.GetPagedAnnouncementsAsync(1, 10, "vod", null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount); // "Hlášení o vodě" a "Vodovodní havárie"
            Assert.All(result.Announcements, a => Assert.Contains("vod", a.Content, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetPagedAnnouncementsAsync_FiltersByDay_ExactMatch()
        {
            // Arrange
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(options);
            context.BroadcastAnnouncements.AddRange(new[]
            {
                new BroadcastAnnouncement { Content = "Dnesní hlášení", BroadcastDateTime = today, IsActive = true },
                new BroadcastAnnouncement { Content = "Včerejší hlášení", BroadcastDateTime = yesterday, IsActive = true },
                new BroadcastAnnouncement { Content = "Jiný den", BroadcastDateTime = today.AddDays(-2), IsActive = true }
            });
            context.SaveChanges();

            var contextFactoryMock = new Mock<IDbContextFactory<ApplicationDbContext>>();
            contextFactoryMock.Setup(f => f.CreateDbContextAsync(default)).ReturnsAsync(context);
            var globalSettingsMock = new Mock<IGlobalSettingsService>();
            globalSettingsMock.Setup(s => s.GetInt(It.IsAny<string>(), It.IsAny<int>())).Returns(10);
            var loggerMock = new Mock<ILogger<BroadcastAnnouncementService>>();

            var service = new BroadcastAnnouncementService(contextFactoryMock.Object, globalSettingsMock.Object, loggerMock.Object);

            // Act
            var result = await service.GetPagedAnnouncementsAsync(1, 10, null, today);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Announcements);
            Assert.Equal("Dnesní hlášení", result.Announcements[0].Content);
        }
    }
} 