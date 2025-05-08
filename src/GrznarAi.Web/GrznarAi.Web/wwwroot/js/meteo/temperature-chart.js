// Funkce pro vykreslení teplotního grafu
window.renderTemperatureChart = function (elementId, categories, minData, avgData, maxData) {
    console.log("Vykreslování grafu do elementu: " + elementId);
    console.log("Počet bodů: " + categories.length);
    
    // Kontrola existence elementu
    const chartElement = document.getElementById(elementId);
    if (!chartElement) {
        console.error(`Element s ID '${elementId}' nebyl nalezen!`);
        return;
    }
    
    // Zrušit existující graf, pokud existuje
    if (window.temperatureChart) {
        try {
            window.temperatureChart.destroy();
        } catch (e) {
            console.warn("Nepodařilo se zničit předchozí graf:", e);
        }
        window.temperatureChart = null;
    }

    // Odstranit existující statistiky pokud existují (již není potřeba - statistiky jsou v HTML)
    const statsElementId = `${elementId}-stats`;
    const existingStats = document.getElementById(statsElementId);
    if (existingStats) {
        existingStats.remove();
    }

    // Zjistíme typ dat podle formátu kategorie (HH:00 pro hodiny, dd.MM pro dny, MM.yyyy pro měsíce)
    let chartTitle = 'Teplotní historie';
    let xaxisTitle = 'Čas';
    let tickAmount = undefined;
    
    if (categories && categories.length > 0) {
        const sampleCategory = categories[0];
        if (sampleCategory.includes(':00')) {
            chartTitle = 'Hodinová teplotní historie';
            xaxisTitle = 'Hodina';
        } else if (sampleCategory.includes(' - ')) {
            chartTitle = 'Týdenní teplotní historie';
            xaxisTitle = 'Týden';
        } else if (sampleCategory.includes('.') && !sampleCategory.includes('.20')) {
            chartTitle = 'Denní teplotní historie';
            xaxisTitle = 'Den';
        } else if (sampleCategory.includes('.20')) {
            chartTitle = 'Měsíční teplotní historie';
            xaxisTitle = 'Měsíc';
        }
    }

    try {
        // Vytvoření nového grafu
        window.temperatureChart = new ApexCharts(chartElement, {
            chart: {
                type: 'line',
                height: 380,
                toolbar: { show: true },
                zoom: { enabled: true },
                animations: {
                    enabled: true,
                    easing: 'easeinout',
                    speed: 800
                }
            },
            stroke: {
                curve: 'smooth',
                width: [3, 3, 3]
            },
            colors: ['#3498db', '#2ecc71', '#e74c3c'],
            title: {
                text: chartTitle,
                align: 'center'
            },
            xaxis: {
                categories: categories,
                title: {
                    text: xaxisTitle
                },
                tickAmount: tickAmount,
                labels: {
                    rotate: 0,
                    style: {
                        fontSize: '12px'
                    }
                }
            },
            yaxis: {
                title: {
                    text: 'Teplota (°C)'
                },
                min: function(min) { return Math.floor(min - 1); },
                max: function(max) { return Math.ceil(max + 1); },
                decimalsInFloat: 1
            },
            tooltip: {
                shared: true,
                intersect: false,
                y: {
                    formatter: function (value) { 
                        if (value === null || value === undefined) {
                            return 'N/A';
                        }
                        return value.toFixed(1) + ' °C'; 
                    }
                }
            },
            legend: {
                show: true,
                position: 'top',
                horizontalAlign: 'right'
            },
            markers: {
                size: 4,
                hover: { size: 6 }
            },
            grid: {
                borderColor: '#e7e7e7',
                row: {
                    colors: ['#f3f3f3', 'transparent'],
                    opacity: 0.5
                }
            },
            series: [
                {
                    name: 'Min',
                    data: minData
                },
                {
                    name: 'Průměr',
                    data: avgData
                },
                {
                    name: 'Max',
                    data: maxData
                }
            ],
            responsive: [{
                breakpoint: 768,
                options: {
                    chart: {
                        height: 300
                    },
                    xaxis: {
                        labels: {
                            rotate: -45,
                            style: {
                                fontSize: '10px'
                            }
                        }
                    }
                }
            }]
        });

        // Vykreslení grafu
        window.temperatureChart.render();
        console.log("Graf byl úspěšně vykreslen");
        
    } catch (error) {
        console.error("Chyba při vykreslování grafu:", error);
    }
}; 