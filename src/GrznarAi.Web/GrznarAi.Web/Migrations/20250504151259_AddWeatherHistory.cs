using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrznarAi.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddWeatherHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeatherHistory",
                columns: table => new
                {
                    HistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TemperatureIn = table.Column<float>(type: "real", nullable: true),
                    TemperatureOut = table.Column<float>(type: "real", nullable: true),
                    Chill = table.Column<float>(type: "real", nullable: true),
                    DewIn = table.Column<float>(type: "real", nullable: true),
                    DewOut = table.Column<float>(type: "real", nullable: true),
                    HeatIn = table.Column<float>(type: "real", nullable: true),
                    Heat = table.Column<float>(type: "real", nullable: true),
                    HumidityIn = table.Column<float>(type: "real", nullable: true),
                    HumidityOut = table.Column<float>(type: "real", nullable: true),
                    WindSpeedHi = table.Column<float>(type: "real", nullable: true),
                    WindSpeedAvg = table.Column<float>(type: "real", nullable: true),
                    WindDirection = table.Column<float>(type: "real", nullable: true),
                    Bar = table.Column<float>(type: "real", nullable: true),
                    Rain = table.Column<float>(type: "real", nullable: true),
                    RainRate = table.Column<float>(type: "real", nullable: true),
                    SolarRad = table.Column<float>(type: "real", nullable: true),
                    Uvi = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherHistory", x => x.HistoryId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeatherHistory_Date",
                table: "WeatherHistory",
                column: "Date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeatherHistory");
        }
    }
}
