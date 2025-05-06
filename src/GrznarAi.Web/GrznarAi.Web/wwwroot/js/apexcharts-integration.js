// ApexCharts integration for Meteo Trends

// Function to load ApexCharts script dynamically
window.loadApexChartsScript = function () {
    return new Promise((resolve, reject) => {
        // Skript již načítáme přímo v layoutu, takže jen vrátíme resolve
        resolve();
    });
};

// Function to render temperature chart
window.renderTemperatureChart = function (
    elementId,
    categories,
    seriesData,
    seriesTitles,
    xAxisTitle,
    yAxisTitle,
    minY,
    maxY
) {
    // Destroy existing chart if it exists
    if (window.temperatureChart && typeof window.temperatureChart.destroy === 'function') {
        try {
            window.temperatureChart.destroy();
        } catch (error) {
            console.error('Error destroying chart:', error);
            // Pokračujeme i při chybě destroy
        }
    }

    const options = {
        series: [
            {
                name: seriesTitles[0],
                data: seriesData[0],
                color: '#0dcaf0' // info color (for min temp)
            },
            {
                name: seriesTitles[1],
                data: seriesData[1],
                color: '#6c757d' // secondary color (for avg temp)
            },
            {
                name: seriesTitles[2],
                data: seriesData[2],
                color: '#dc3545' // danger color (for max temp)
            }
        ],
        chart: {
            height: 400,
            type: 'line',
            zoom: {
                enabled: true,
                type: 'x'
            },
            toolbar: {
                show: true,
                tools: {
                    download: true,
                    selection: true,
                    zoom: true,
                    zoomin: true,
                    zoomout: true,
                    pan: true,
                    reset: true
                },
                autoSelected: 'zoom'
            },
            animations: {
                enabled: true,
                easing: 'easeinout',
                speed: 800,
                animateGradually: {
                    enabled: true,
                    delay: 150
                },
                dynamicAnimation: {
                    enabled: true,
                    speed: 350
                }
            },
            fontFamily: 'inherit'
        },
        dataLabels: {
            enabled: false
        },
        stroke: {
            curve: 'smooth',
            width: [2, 2, 2]
        },
        markers: {
            size: 4,
            hover: {
                size: 6
            }
        },
        title: {
            text: undefined,
            align: 'left'
        },
        grid: {
            borderColor: '#e7e7e7',
            row: {
                colors: ['#f3f3f3', 'transparent'],
                opacity: 0.5
            }
        },
        xaxis: {
            categories: categories,
            title: {
                text: xAxisTitle
            },
            labels: {
                rotate: -45,
                rotateAlways: false,
                style: {
                    fontSize: '12px',
                    fontWeight: 400
                },
                formatter: function(value) {
                    return value;
                }
            }
        },
        yaxis: {
            title: {
                text: yAxisTitle
            },
            min: Math.floor(minY),
            max: Math.ceil(maxY),
            labels: {
                formatter: function (val) {
                    return val.toFixed(1) + ' °C';
                }
            }
        },
        legend: {
            position: 'bottom',
            horizontalAlign: 'center',
            floating: false,
            offsetY: 0,
            offsetX: 0
        },
        tooltip: {
            shared: true,
            intersect: false,
            y: {
                formatter: function (val) {
                    return val.toFixed(1) + ' °C';
                }
            }
        },
        responsive: [
            {
                breakpoint: 768,
                options: {
                    chart: {
                        height: 320
                    },
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        ]
    };

    // Zkontrolujeme, zda element existuje
    const chartElement = document.getElementById(elementId);
    if (!chartElement) {
        console.error(`Element with ID "${elementId}" not found`);
        return;
    }

    try {
        // Initialize chart
        window.temperatureChart = new ApexCharts(chartElement, options);
        window.temperatureChart.render();
    } catch (error) {
        console.error('Error initializing or rendering chart:', error);
    }
};

// Function to render humidity chart
window.renderHumidityChart = function (
    elementId,
    categories,
    seriesData,
    seriesTitles,
    xAxisTitle,
    yAxisTitle,
    minY,
    maxY
) {
    // Destroy existing chart if it exists
    if (window.humidityChart && typeof window.humidityChart.destroy === 'function') {
        try {
            window.humidityChart.destroy();
        } catch (error) {
            console.error('Error destroying humidity chart:', error);
            // Pokračujeme i při chybě destroy
        }
    }

    const options = {
        series: [
            {
                name: seriesTitles[0],
                data: seriesData[0],
                color: '#0dcaf0' // info color (for min humidity)
            },
            {
                name: seriesTitles[1],
                data: seriesData[1],
                color: '#6c757d' // secondary color (for avg humidity)
            },
            {
                name: seriesTitles[2],
                data: seriesData[2],
                color: '#dc3545' // danger color (for max humidity)
            }
        ],
        chart: {
            height: 400,
            type: 'line',
            zoom: {
                enabled: true,
                type: 'x'
            },
            toolbar: {
                show: true,
                tools: {
                    download: true,
                    selection: true,
                    zoom: true,
                    zoomin: true,
                    zoomout: true,
                    pan: true,
                    reset: true
                },
                autoSelected: 'zoom'
            },
            animations: {
                enabled: true,
                easing: 'easeinout',
                speed: 800,
                animateGradually: {
                    enabled: true,
                    delay: 150
                },
                dynamicAnimation: {
                    enabled: true,
                    speed: 350
                }
            },
            fontFamily: 'inherit'
        },
        dataLabels: {
            enabled: false
        },
        stroke: {
            curve: 'smooth',
            width: [2, 2, 2]
        },
        markers: {
            size: 4,
            hover: {
                size: 6
            }
        },
        title: {
            text: undefined,
            align: 'left'
        },
        grid: {
            borderColor: '#e7e7e7',
            row: {
                colors: ['#f3f3f3', 'transparent'],
                opacity: 0.5
            }
        },
        xaxis: {
            categories: categories,
            title: {
                text: xAxisTitle
            },
            labels: {
                rotate: -45,
                rotateAlways: false,
                style: {
                    fontSize: '12px',
                    fontWeight: 400
                },
                formatter: function(value) {
                    return value;
                }
            }
        },
        yaxis: {
            title: {
                text: yAxisTitle
            },
            min: Math.floor(minY),
            max: Math.ceil(maxY),
            labels: {
                formatter: function (val) {
                    return val.toFixed(1) + ' %';
                }
            }
        },
        legend: {
            position: 'bottom',
            horizontalAlign: 'center',
            floating: false,
            offsetY: 0,
            offsetX: 0
        },
        tooltip: {
            shared: true,
            intersect: false,
            y: {
                formatter: function (val) {
                    return val.toFixed(1) + ' %';
                }
            }
        },
        responsive: [
            {
                breakpoint: 768,
                options: {
                    chart: {
                        height: 320
                    },
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        ]
    };

    // Zkontrolujeme, zda element existuje
    const chartElement = document.getElementById(elementId);
    if (!chartElement) {
        console.error(`Element with ID "${elementId}" not found`);
        return;
    }

    try {
        // Initialize chart
        window.humidityChart = new ApexCharts(chartElement, options);
        window.humidityChart.render();
    } catch (error) {
        console.error('Error initializing or rendering humidity chart:', error);
    }
};

// Function to render pressure chart
window.renderPressureChart = function (
    elementId,
    categories,
    seriesData,
    seriesTitles,
    xAxisTitle,
    yAxisTitle,
    minY,
    maxY
) {
    // Destroy existing chart if it exists
    if (window.pressureChart && typeof window.pressureChart.destroy === 'function') {
        try {
            window.pressureChart.destroy();
        } catch (error) {
            console.error('Error destroying pressure chart:', error);
            // Pokračujeme i při chybě destroy
        }
    }

    const options = {
        series: [
            {
                name: seriesTitles[0],
                data: seriesData[0],
                color: '#0dcaf0' // info color (for min pressure)
            },
            {
                name: seriesTitles[1],
                data: seriesData[1],
                color: '#6c757d' // secondary color (for avg pressure)
            },
            {
                name: seriesTitles[2],
                data: seriesData[2],
                color: '#dc3545' // danger color (for max pressure)
            }
        ],
        chart: {
            height: 400,
            type: 'line',
            zoom: {
                enabled: true,
                type: 'x'
            },
            toolbar: {
                show: true,
                tools: {
                    download: true,
                    selection: true,
                    zoom: true,
                    zoomin: true,
                    zoomout: true,
                    pan: true,
                    reset: true
                },
                autoSelected: 'zoom'
            },
            animations: {
                enabled: true,
                easing: 'easeinout',
                speed: 800,
                animateGradually: {
                    enabled: true,
                    delay: 150
                },
                dynamicAnimation: {
                    enabled: true,
                    speed: 350
                }
            },
            fontFamily: 'inherit'
        },
        dataLabels: {
            enabled: false
        },
        stroke: {
            curve: 'smooth',
            width: [2, 2, 2]
        },
        markers: {
            size: 4,
            hover: {
                size: 6
            }
        },
        title: {
            text: undefined,
            align: 'left'
        },
        grid: {
            borderColor: '#e7e7e7',
            row: {
                colors: ['#f3f3f3', 'transparent'],
                opacity: 0.5
            }
        },
        xaxis: {
            categories: categories,
            title: {
                text: xAxisTitle
            },
            labels: {
                rotate: -45,
                rotateAlways: false,
                style: {
                    fontSize: '12px',
                    fontWeight: 400
                },
                formatter: function(value) {
                    return value;
                }
            }
        },
        yaxis: {
            title: {
                text: yAxisTitle
            },
            min: Math.floor(minY),
            max: Math.ceil(maxY),
            labels: {
                formatter: function (val) {
                    return val.toFixed(1) + ' hPa';
                }
            }
        },
        legend: {
            position: 'bottom',
            horizontalAlign: 'center',
            floating: false,
            offsetY: 0,
            offsetX: 0
        },
        tooltip: {
            shared: true,
            intersect: false,
            y: {
                formatter: function (val) {
                    return val.toFixed(1) + ' hPa';
                }
            }
        },
        responsive: [
            {
                breakpoint: 768,
                options: {
                    chart: {
                        height: 320
                    },
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        ]
    };

    // Zkontrolujeme, zda element existuje
    const chartElement = document.getElementById(elementId);
    if (!chartElement) {
        console.error(`Element with ID "${elementId}" not found`);
        return;
    }

    try {
        // Initialize chart
        window.pressureChart = new ApexCharts(chartElement, options);
        window.pressureChart.render();
    } catch (error) {
        console.error('Error initializing or rendering pressure chart:', error);
    }
};

// Function to render wind speed chart
window.renderWindSpeedChart = function (
    elementId,
    categories,
    seriesData,
    seriesTitles,
    xAxisTitle,
    yAxisTitle,
    minY,
    maxY
) {
    // Destroy existing chart if it exists
    if (window.windSpeedChart && typeof window.windSpeedChart.destroy === 'function') {
        try {
            window.windSpeedChart.destroy();
        } catch (error) {
            console.error('Error destroying wind speed chart:', error);
            // Pokračujeme i při chybě destroy
        }
    }

    const options = {
        series: [
            {
                name: seriesTitles[0],
                data: seriesData[0],
                color: '#0dcaf0' // info color (for min wind speed)
            },
            {
                name: seriesTitles[1],
                data: seriesData[1],
                color: '#6c757d' // secondary color (for avg wind speed)
            },
            {
                name: seriesTitles[2],
                data: seriesData[2],
                color: '#dc3545' // danger color (for max wind speed)
            },
            {
                name: seriesTitles[3],
                data: seriesData[3],
                color: '#ffc107' // warning color (for wind gust)
            }
        ],
        chart: {
            height: 400,
            type: 'line',
            zoom: {
                enabled: true,
                type: 'x'
            },
            toolbar: {
                show: true,
                tools: {
                    download: true,
                    selection: true,
                    zoom: true,
                    zoomin: true,
                    zoomout: true,
                    pan: true,
                    reset: true
                },
                autoSelected: 'zoom'
            },
            animations: {
                enabled: true,
                easing: 'easeinout',
                speed: 800,
                animateGradually: {
                    enabled: true,
                    delay: 150
                },
                dynamicAnimation: {
                    enabled: true,
                    speed: 350
                }
            },
            fontFamily: 'inherit'
        },
        dataLabels: {
            enabled: false
        },
        stroke: {
            curve: 'smooth',
            width: [2, 2, 2, 2]
        },
        markers: {
            size: 4,
            hover: {
                size: 6
            }
        },
        title: {
            text: undefined,
            align: 'left'
        },
        grid: {
            borderColor: '#e7e7e7',
            row: {
                colors: ['#f3f3f3', 'transparent'],
                opacity: 0.5
            }
        },
        xaxis: {
            categories: categories,
            title: {
                text: xAxisTitle
            },
            labels: {
                rotate: -45,
                rotateAlways: false,
                style: {
                    fontSize: '12px',
                    fontWeight: 400
                },
                formatter: function(value) {
                    return value;
                }
            }
        },
        yaxis: {
            title: {
                text: yAxisTitle
            },
            min: Math.floor(minY),
            max: Math.ceil(maxY),
            labels: {
                formatter: function (val) {
                    return val.toFixed(1) + ' m/s';
                }
            }
        },
        legend: {
            position: 'bottom',
            horizontalAlign: 'center',
            floating: false,
            offsetY: 0,
            offsetX: 0
        },
        tooltip: {
            shared: true,
            intersect: false,
            y: {
                formatter: function (val) {
                    return val.toFixed(1) + ' m/s';
                }
            }
        },
        responsive: [
            {
                breakpoint: 768,
                options: {
                    chart: {
                        height: 320
                    },
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        ]
    };

    // Zkontrolujeme, zda element existuje
    const chartElement = document.getElementById(elementId);
    if (!chartElement) {
        console.error(`Element with ID "${elementId}" not found`);
        return;
    }

    try {
        // Initialize chart
        window.windSpeedChart = new ApexCharts(chartElement, options);
        window.windSpeedChart.render();
    } catch (error) {
        console.error('Error initializing or rendering wind speed chart:', error);
    }
};

// Function to render rainfall chart as bar chart
window.renderRainfallChart = function (
    elementId,
    categories,
    seriesData,
    seriesTitles,
    xAxisTitle,
    yAxisTitle,
    minY,
    maxY
) {
    // Destroy existing chart if it exists
    if (window.rainfallChart && typeof window.rainfallChart.destroy === 'function') {
        try {
            window.rainfallChart.destroy();
        } catch (error) {
            console.error('Error destroying rainfall chart:', error);
            // Pokračujeme i při chybě destroy
        }
    }

    const options = {
        series: [
            {
                name: seriesTitles[0],
                data: seriesData[0],
                color: '#0d6efd', // primary color (total rainfall)
                type: 'column'
            },
            {
                name: seriesTitles[1],
                data: seriesData[1],
                color: '#0dcaf0', // info color (rainfall rate)
                type: 'line'
            }
        ],
        chart: {
            height: 400,
            type: 'line',
            zoom: {
                enabled: true,
                type: 'x'
            },
            toolbar: {
                show: true,
                tools: {
                    download: true,
                    selection: true,
                    zoom: true,
                    zoomin: true,
                    zoomout: true,
                    pan: true,
                    reset: true
                },
                autoSelected: 'zoom'
            },
            animations: {
                enabled: true,
                easing: 'easeinout',
                speed: 800,
                animateGradually: {
                    enabled: true,
                    delay: 150
                },
                dynamicAnimation: {
                    enabled: true,
                    speed: 350
                }
            },
            fontFamily: 'inherit',
            stacked: false
        },
        plotOptions: {
            bar: {
                columnWidth: '70%',
                borderRadius: 2
            }
        },
        dataLabels: {
            enabled: false
        },
        stroke: {
            curve: 'smooth',
            width: [0, 3] // sloupcový graf nemá obrys, čarový graf má šířku 3
        },
        markers: {
            size: [0, 4], // skryjeme markery pro sloupce, zobrazíme pro čáru
            hover: {
                size: 6
            }
        },
        title: {
            text: undefined,
            align: 'left'
        },
        grid: {
            borderColor: '#e7e7e7',
            row: {
                colors: ['#f3f3f3', 'transparent'],
                opacity: 0.5
            }
        },
        xaxis: {
            categories: categories,
            title: {
                text: xAxisTitle
            },
            labels: {
                rotate: -45,
                rotateAlways: false,
                style: {
                    fontSize: '12px',
                    fontWeight: 400
                },
                formatter: function(value) {
                    return value;
                }
            }
        },
        yaxis: {
            title: {
                text: yAxisTitle
            },
            min: Math.floor(minY),
            max: Math.ceil(maxY),
            labels: {
                formatter: function (val) {
                    return val.toFixed(1) + ' mm';
                }
            }
        },
        legend: {
            position: 'bottom',
            horizontalAlign: 'center',
            floating: false,
            offsetY: 0,
            offsetX: 0
        },
        tooltip: {
            shared: true,
            intersect: false,
            y: {
                formatter: function (val, { seriesIndex }) {
                    if (seriesIndex === 0) {
                        return val.toFixed(1) + ' mm';
                    } else {
                        return val.toFixed(1) + ' mm/h';
                    }
                }
            }
        },
        responsive: [
            {
                breakpoint: 768,
                options: {
                    chart: {
                        height: 320
                    },
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        ]
    };

    // Zkontrolujeme, zda element existuje
    const chartElement = document.getElementById(elementId);
    if (!chartElement) {
        console.error(`Element with ID "${elementId}" not found`);
        return;
    }

    try {
        // Initialize chart
        window.rainfallChart = new ApexCharts(chartElement, options);
        window.rainfallChart.render();
    } catch (error) {
        console.error('Error initializing or rendering rainfall chart:', error);
    }
};

// Function to render solar radiation chart with sunshine hours as column
window.renderSolarRadiationChart = function (
    elementId,
    categories,
    seriesData,
    seriesTitles,
    xAxisTitle,
    yAxisTitle
) {
    // Destroy existing chart if it exists
    if (window.solarRadiationChart && typeof window.solarRadiationChart.destroy === 'function') {
        try {
            window.solarRadiationChart.destroy();
        } catch (error) {
            console.error('Error destroying solar radiation chart:', error);
            // Pokračujeme i při chybě destroy
        }
    }

    // Oddělení údajů pro sluneční záření (čáry) a hodiny slunečního svitu (sloupce)
    const solarRadData = [
        {
            name: seriesTitles[0], // Min
            type: 'line',
            data: seriesData[0],
            color: '#0dcaf0' // info color
        },
        {
            name: seriesTitles[1], // Avg
            type: 'line',
            data: seriesData[1],
            color: '#6c757d' // secondary color
        },
        {
            name: seriesTitles[2], // Max
            type: 'line',
            data: seriesData[2],
            color: '#dc3545' // danger color
        }
    ];

    const sunshineHoursData = {
        name: seriesTitles[3], // Sunshine hours
        type: 'column',
        data: seriesData[3],
        color: '#ffc107', // warning color
        yAxisIndex: 1 // Použití druhé osy Y
    };

    // Spojení dat do jednoho pole
    const combinedSeries = [...solarRadData, sunshineHoursData];

    const options = {
        series: combinedSeries,
        chart: {
            height: 400,
            type: 'line',
            zoom: {
                enabled: true,
                type: 'x'
            },
            toolbar: {
                show: true,
                tools: {
                    download: true,
                    selection: true,
                    zoom: true,
                    zoomin: true,
                    zoomout: true,
                    pan: true,
                    reset: true
                },
                autoSelected: 'zoom'
            },
            animations: {
                enabled: true,
                easing: 'easeinout',
                speed: 800,
                animateGradually: {
                    enabled: true,
                    delay: 150
                },
                dynamicAnimation: {
                    enabled: true,
                    speed: 350
                }
            },
            fontFamily: 'inherit'
        },
        dataLabels: {
            enabled: false
        },
        stroke: {
            curve: 'smooth',
            width: [2, 2, 2, 0] // Poslední 0 pro sloupcový graf
        },
        fill: {
            opacity: [1, 1, 1, 0.7] // Nižší průhlednost pro sloupcový graf
        },
        markers: {
            size: 4,
            hover: {
                size: 6
            }
        },
        title: {
            text: undefined,
            align: 'left'
        },
        grid: {
            borderColor: '#e7e7e7',
            row: {
                colors: ['#f3f3f3', 'transparent'],
                opacity: 0.5
            }
        },
        xaxis: {
            categories: categories,
            title: {
                text: xAxisTitle
            },
            labels: {
                rotate: -45,
                rotateAlways: false,
                style: {
                    fontSize: '12px',
                    fontWeight: 400
                }
            }
        },
        yaxis: [
            {
                title: {
                    text: yAxisTitle
                },
                labels: {
                    formatter: function (val) {
                        return val.toFixed(1) + ' W/m²';
                    }
                }
            },
            {
                opposite: true,
                title: {
                    text: 'Sunshine Hours (h)'
                },
                labels: {
                    formatter: function (val) {
                        return val.toFixed(1) + ' h';
                    }
                }
            }
        ],
        legend: {
            position: 'bottom',
            horizontalAlign: 'center',
            floating: false,
            offsetY: 0,
            offsetX: 0
        },
        tooltip: {
            shared: true,
            intersect: false,
            y: [
                {
                    formatter: function (val) {
                        return val.toFixed(1) + ' W/m²';
                    }
                },
                {
                    formatter: function (val) {
                        return val.toFixed(1) + ' W/m²';
                    }
                },
                {
                    formatter: function (val) {
                        return val.toFixed(1) + ' W/m²';
                    }
                },
                {
                    formatter: function (val) {
                        return val.toFixed(1) + ' h';
                    }
                }
            ]
        },
        responsive: [
            {
                breakpoint: 768,
                options: {
                    chart: {
                        height: 320
                    },
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        ]
    };

    // Zkontrolujeme, zda element existuje
    const chartElement = document.getElementById(elementId);
    if (!chartElement) {
        console.error(`Element with ID "${elementId}" not found`);
        return;
    }

    try {
        // Initialize chart
        window.solarRadiationChart = new ApexCharts(chartElement, options);
        window.solarRadiationChart.render();
    } catch (error) {
        console.error('Error initializing or rendering solar radiation chart:', error);
    }
};

// Function to render UV index chart
window.renderUVIndexChart = function (
    elementId,
    categories,
    seriesData,
    seriesTitles,
    xAxisTitle,
    yAxisTitle,
    minY,
    maxY
) {
    // Destroy existing chart if it exists
    if (window.uvIndexChart && typeof window.uvIndexChart.destroy === 'function') {
        try {
            window.uvIndexChart.destroy();
        } catch (error) {
            console.error('Error destroying UV index chart:', error);
            // Pokračujeme i při chybě destroy
        }
    }

    const options = {
        series: [
            {
                name: seriesTitles[0],
                data: seriesData[0],
                color: '#0dcaf0' // info color (for min UV)
            },
            {
                name: seriesTitles[1],
                data: seriesData[1],
                color: '#6c757d' // secondary color (for avg UV)
            },
            {
                name: seriesTitles[2],
                data: seriesData[2],
                color: '#dc3545' // danger color (for max UV)
            }
        ],
        chart: {
            height: 400,
            type: 'line',
            zoom: {
                enabled: true,
                type: 'x'
            },
            toolbar: {
                show: true,
                tools: {
                    download: true,
                    selection: true,
                    zoom: true,
                    zoomin: true,
                    zoomout: true,
                    pan: true,
                    reset: true
                },
                autoSelected: 'zoom'
            },
            animations: {
                enabled: true,
                easing: 'easeinout',
                speed: 800,
                animateGradually: {
                    enabled: true,
                    delay: 150
                },
                dynamicAnimation: {
                    enabled: true,
                    speed: 350
                }
            },
            fontFamily: 'inherit'
        },
        dataLabels: {
            enabled: false
        },
        stroke: {
            curve: 'smooth',
            width: [2, 2, 2]
        },
        markers: {
            size: 4,
            hover: {
                size: 6
            }
        },
        title: {
            text: undefined,
            align: 'left'
        },
        grid: {
            borderColor: '#e7e7e7',
            row: {
                colors: ['#f3f3f3', 'transparent'],
                opacity: 0.5
            }
        },
        xaxis: {
            categories: categories,
            title: {
                text: xAxisTitle
            },
            labels: {
                rotate: -45,
                rotateAlways: false,
                style: {
                    fontSize: '12px',
                    fontWeight: 400
                }
            }
        },
        yaxis: {
            title: {
                text: yAxisTitle
            },
            min: Math.floor(minY),
            max: Math.ceil(maxY),
            tickAmount: Math.ceil(maxY) <= 11 ? Math.ceil(maxY) : undefined,
            labels: {
                formatter: function (val) {
                    return val.toFixed(0);
                }
            }
        },
        legend: {
            position: 'bottom',
            horizontalAlign: 'center',
            floating: false,
            offsetY: 0,
            offsetX: 0
        },
        tooltip: {
            shared: true,
            intersect: false,
            y: {
                formatter: function (val) {
                    return val.toFixed(1);
                }
            }
        },
        responsive: [
            {
                breakpoint: 768,
                options: {
                    chart: {
                        height: 320
                    },
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        ]
    };

    // Zkontrolujeme, zda element existuje
    const chartElement = document.getElementById(elementId);
    if (!chartElement) {
        console.error(`Element with ID "${elementId}" not found`);
        return;
    }

    try {
        // Initialize chart
        window.uvIndexChart = new ApexCharts(chartElement, options);
        window.uvIndexChart.render();
    } catch (error) {
        console.error('Error initializing or rendering UV index chart:', error);
    }
};