// Funkce pro vykreslení grafu UV indexu
window.renderUVIndexChart = function (elementId, categories, seriesData, seriesTitles, xAxisTitle, yAxisTitle, chartTypes) {
    console.log("Vykreslování grafu UV indexu do elementu: " + elementId);
    console.log("Počet bodů: " + categories.length);
    
    // Kontrola existence elementu
    const chartElement = document.getElementById(elementId);
    if (!chartElement) {
        console.error(`Element s ID '${elementId}' nebyl nalezen!`);
        return;
    }
    
    // Zrušit existující graf, pokud existuje
    if (window.uvIndexChart) {
        try {
            window.uvIndexChart.destroy();
        } catch (e) {
            console.warn("Nepodařilo se zničit předchozí graf UV indexu:", e);
        }
        window.uvIndexChart = null;
    }

    // Kontrola, zda máme správné počty dat
    if (!seriesData || !Array.isArray(seriesData) || !seriesTitles || !Array.isArray(seriesTitles)) {
        console.error('Neplatná data pro graf UV indexu:', { seriesData, seriesTitles });
        return;
    }

    // Kontrola, zda byly předány typy grafů
    if (!chartTypes || !Array.isArray(chartTypes)) {
        console.warn('Nebyly předány typy grafů, použijeme výchozí typy (line, line)');
        chartTypes = ['line', 'line'];
    }

    // Zjistíme typ dat podle formátu kategorie (HH:00 pro hodiny, dd.MM pro dny, apod.)
    let chartTitle = 'Historie UV indexu';
    let xaxisTitle = 'Čas';
    
    if (categories.length > 0) {
        const firstCategory = categories[0] || '';
        
        if (firstCategory.includes(':')) {
            // Hodinová data
            xaxisTitle = 'Hodina';
        } else if (firstCategory.includes('.') && firstCategory.length <= 6) {
            // Denní data (dd.MM)
            xaxisTitle = 'Den';
        } else if (firstCategory.includes(' ')) {
            // Měsíční data (MMM yyyy)
            xaxisTitle = 'Měsíc';
        }
    }

    // Zpracování null/undefined hodnot - zásadní pro předcházení chyb v ApexCharts
    for (let i = 0; i < seriesData.length; i++) {
        if (Array.isArray(seriesData[i])) {
            // Převést null/undefined hodnoty na 0 nebo prázdné pole 
            seriesData[i] = seriesData[i].map(value => (value === null || value === undefined) ? 0 : value);
        } else {
            // Pokud série není pole, vytvořit prázdné pole
            console.warn(`Série dat ${i} není pole, vytváříme prázdné pole.`);
            seriesData[i] = [];
        }
    }

    // Vytvoření pole sérií dat s odpovídajícími typy grafů
    const combinedSeries = [];
    for (let i = 0; i < seriesData.length; i++) {
        if (i >= seriesTitles.length || i >= chartTypes.length) {
            console.warn(`Chybí název nebo typ grafu pro sérii ${i}`);
            continue;
        }

        // Určení barvy podle typu série
        let color;
        if (i === 0) { 
            // První série (průměrná hodnota)
            color = '#ffc107'; // warning color
        } else if (i === 1) { 
            // Druhá série (maximální hodnota)
            color = '#dc3545'; // danger color
        } else {
            color = '#6c757d'; // secondary color
        }

        const series = {
            name: seriesTitles[i] || `Série ${i+1}`,
            type: chartTypes[i] || 'line',
            data: seriesData[i],
            color: color
        };

        combinedSeries.push(series);
    }

    // Nastavení grafu
    const options = {
        series: combinedSeries,
        chart: {
            height: 400,
            type: 'line', // Základní typ grafu - bude přepsáno pro jednotlivé série
            stacked: false,
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
                speed: 800
            }
        },
        stroke: {
            curve: 'smooth',
            width: [3, 3] // Tloušťka čáry
        },
        fill: {
            opacity: [1, 1] // Průhlednost výplně
        },
        markers: {
            size: [3, 3], // Velikost značek
            hover: {
                size: 6
            }
        },
        xaxis: {
            categories: categories || [],
            title: {
                text: xAxisTitle || 'Čas'
            },
            labels: {
                rotate: -45,
                rotateAlways: false
            }
        },
        yaxis: {
            axisTicks: {
                show: true
            },
            axisBorder: {
                show: true,
                color: '#ffc107'
            },
            title: {
                text: yAxisTitle || 'UV Index'
            },
            labels: {
                formatter: function(value) {
                    return value.toFixed(1);
                }
            },
            min: 0
        },
        dataLabels: {
            enabled: false
        },
        legend: {
            position: 'top',
            horizontalAlign: 'center'
        },
        tooltip: {
            shared: true,
            intersect: false,
            y: {
                formatter: function(value) {
                    if (value === null || value === undefined) {
                        return "N/A";
                    }
                    return value.toFixed(1);
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
        ],
        noData: {
            text: 'Nejsou dostupná žádná data',
            align: 'center',
            verticalAlign: 'middle',
            offsetX: 0,
            offsetY: 0,
            style: {
                color: '#6c757d',
                fontSize: '16px',
                fontFamily: 'inherit'
            }
        }
    };

    try {
        // Inicializace grafu
        window.uvIndexChart = new ApexCharts(chartElement, options);
        window.uvIndexChart.render();
    } catch (error) {
        console.error('Chyba při inicializaci nebo vykreslování grafu UV indexu:', error);
    }
}; 