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