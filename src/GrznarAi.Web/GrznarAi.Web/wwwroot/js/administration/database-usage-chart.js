/**
 * Vykreslení koláčového grafu velikosti databáze
 * Používá knihovnu ApexCharts z CDN (načtenou v hlavním layoutu)
 */
window.renderDatabaseUsageChart = function (elementId, chartData) {
    // Kontrola existence elementu
    const chartElement = document.getElementById(elementId);
    if (!chartElement) {
        console.error(`Element s ID '${elementId}' nebyl nalezen!`);
        return;
    }
    
    // Zrušit existující graf, pokud existuje
    if (window.dbUsageChart) {
        try {
            window.dbUsageChart.destroy();
        } catch (e) {
            console.warn("Nepodařilo se zničit předchozí graf:", e);
        }
        window.dbUsageChart = null;
    }

    try {
        // Vytvoření nového grafu
        window.dbUsageChart = new ApexCharts(chartElement, {
            chart: {
                type: 'pie',
                height: 240,
                animations: {
                    enabled: true,
                    easing: 'easeinout',
                    speed: 800
                }
            },
            series: chartData.series,
            labels: chartData.labels,
            colors: chartData.colors,
            legend: {
                position: 'bottom'
            },
            plotOptions: {
                pie: {
                    donut: {
                        size: '65%'
                    }
                }
            },
            dataLabels: {
                formatter: function (val, opts) {
                    return opts.w.config.series[opts.seriesIndex].toFixed(1) + ' GB';
                }
            },
            tooltip: {
                y: {
                    formatter: function (value) {
                        return value.toFixed(2) + ' GB';
                    }
                }
            },
            responsive: [{
                breakpoint: 480,
                options: {
                    legend: {
                        show: false
                    }
                }
            }]
        });

        // Vykreslení grafu
        window.dbUsageChart.render();
        console.log("DB usage graf byl úspěšně vykreslen");
    } catch (error) {
        console.error("Chyba při vykreslování grafu využití databáze:", error);
    }
}; 