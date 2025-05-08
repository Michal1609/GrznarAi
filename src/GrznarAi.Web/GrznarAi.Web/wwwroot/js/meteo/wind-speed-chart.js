// Funkce pro vykreslení grafu rychlosti větru
window.renderWindSpeedChart = function (elementId, categories, minData, avgData, maxData, gustData) {
    console.log("Vykreslování grafu rychlosti větru do elementu: " + elementId);
    console.log("Počet bodů: " + categories.length);
    
    // Kontrola existence elementu
    const chartElement = document.getElementById(elementId);
    if (!chartElement) {
        console.error(`Element s ID '${elementId}' nebyl nalezen!`);
        return;
    }
    
    // Zrušit existující graf, pokud existuje
    if (window.windSpeedChart) {
        try {
            window.windSpeedChart.destroy();
        } catch (e) {
            console.warn("Nepodařilo se zničit předchozí graf rychlosti větru:", e);
        }
        window.windSpeedChart = null;
    }

    // Odstranit existující statistiky pokud existují (již není potřeba - statistiky jsou v HTML)
    const statsElementId = `${elementId}-stats`;
    const existingStats = document.getElementById(statsElementId);
    if (existingStats) {
        existingStats.remove();
    }

    // Zjistíme typ dat podle formátu kategorie (HH:00 pro hodiny, dd.MM pro dny, MM.yyyy pro měsíce)
    let chartTitle = 'Historie rychlosti větru';
    let xaxisTitle = 'Čas';
    let tickAmount = undefined;
    
    if (categories && categories.length > 0) {
        const sampleCategory = categories[0];
        if (sampleCategory.includes(':00')) {
            chartTitle = 'Hodinová historie rychlosti větru';
            xaxisTitle = 'Hodina';
        } else if (sampleCategory.includes(' - ')) {
            chartTitle = 'Týdenní historie rychlosti větru';
            xaxisTitle = 'Týden';
        } else if (sampleCategory.includes('.') && !sampleCategory.includes('.20')) {
            chartTitle = 'Denní historie rychlosti větru';
            xaxisTitle = 'Den';
        } else if (sampleCategory.includes('.20')) {
            chartTitle = 'Měsíční historie rychlosti větru';
            xaxisTitle = 'Měsíc';
        }
    }

    // Konverze dat z m/s na km/h (násobení 3.6)
    const convertToKmh = (dataArray) => {
        return dataArray.map(value => value !== null && value !== undefined ? value * 3.6 : value);
    };

    // Převedení všech hodnotových polí na km/h
    const minDataKmh = convertToKmh(minData);
    const avgDataKmh = convertToKmh(avgData);
    const maxDataKmh = convertToKmh(maxData);
    const gustDataKmh = convertToKmh(gustData);

    try {
        // Vytvoření nového grafu
        window.windSpeedChart = new ApexCharts(chartElement, {
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
                width: [3, 3, 3, 3]
            },
            colors: ['#3498db', '#2ecc71', '#e74c3c', '#9b59b6'],
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
                    text: 'Rychlost větru (km/h)'
                },
                min: function(min) { return Math.floor(min); },
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
                        return value.toFixed(1) + ' km/h'; 
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
                    data: minDataKmh
                },
                {
                    name: 'Průměr',
                    data: avgDataKmh
                },
                {
                    name: 'Max',
                    data: maxDataKmh
                },
                {
                    name: 'Nárazový vítr',
                    data: gustDataKmh
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
        window.windSpeedChart.render();
        console.log("Graf rychlosti větru byl úspěšně vykreslen");
        
    } catch (error) {
        console.error("Chyba při vykreslování grafu rychlosti větru:", error);
    }
}; 