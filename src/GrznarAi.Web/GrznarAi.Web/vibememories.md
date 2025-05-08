## Meteo Trends

The MeteoTrends page (/meteo/trends) shows historical weather data in graphical format. It provides:

- Temperature graph (min, avg, max)
- Humidity graph (min, avg, max)
- Pressure graph (min, avg, max)
- Wind speed graph (min, avg, max, gusts) displayed in km/h

The page allows filtering data by different time periods:
- Day
- Week
- Month
- Year

Data is loaded from the database table WeatherHistory using specialized services:
- TemperatureHistoryService
- HumidityHistoryService
- PressureHistoryService
- WindSpeedHistoryService

Each service provides data aggregation for different time periods, converting UTC database times to local times.

The graphs are rendered using ApexCharts through JavaScript functions in the meteo directory:
- temperature-chart.js
- humidity-chart.js
- pressure-chart.js
- wind-speed-chart.js

### Wind Speed Chart

The wind speed chart shows four lines:
- Minimum wind speed (from WindSpeedAvg)
- Average wind speed (from WindSpeedAvg)
- Maximum wind speed (from WindSpeedAvg)
- Wind gusts (from WindSpeedHi)

While data is stored in the database in m/s, it's displayed in the user interface in km/h (converted by multiplying by 3.6). This conversion happens in the JavaScript code for better user experience and more intuitive readings. 