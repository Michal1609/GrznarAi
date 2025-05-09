// Funkce pro vykreslení grafu slunečního záření
window.renderSolarRadiationChart = function (elementId, categories, seriesData, seriesTitles, xAxisTitle, yAxisTitle, chartTypes) {
    console.log("Vykreslování grafu slunečního záření do elementu: " + elementId);
    console.log("Počet bodů: " + categories.length);
    
    // Kontrola existence elementu
    const chartElement = document.getElementById(elementId);
    if (!chartElement) {
        console.error(`Element s ID '${elementId}' nebyl nalezen!`);
        return;
    }
    
    // Zrušit existující graf, pokud existuje
    if (window.solarRadiationChart) {
        try {
            window.solarRadiationChart.destroy();
        } catch (e) {
            console.warn("Nepodařilo se zničit předchozí graf slunečního záření:", e);
        }
        window.solarRadiationChart = null;
    }

    // Kontrola, zda máme správné počty dat
    if (!seriesData || !Array.isArray(seriesData) || !seriesTitles || !Array.isArray(seriesTitles)) {
        console.error('Neplatná data pro graf slunečního záření:', { seriesData, seriesTitles });
        return;
    }

    // Kontrola, zda byly předány typy grafů
    if (!chartTypes || !Array.isArray(chartTypes)) {
        console.warn('Nebyly předány typy grafů, použijeme výchozí typy (line, line, column)');
        chartTypes = ['line', 'line', 'column'];
    }

    // Zjistíme typ dat podle formátu kategorie (HH:00 pro hodiny, dd.MM pro dny, apod.)
    let chartTitle = 'Historie slunečního záření';
    let xaxisTitle = 'Čas';
    let tickAmount = undefined;
    
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
            color = '#0dcaf0'; // info color
        } else if (i === 1) { 
            // Druhá série (maximální hodnota)
            color = '#dc3545'; // danger color
        } else if (i === 2) { 
            // Třetí série (hodiny slunečního svitu)
            color = '#ffc107'; // warning color
        } else {
            color = '#6c757d'; // secondary color
        }

        const series = {
            name: seriesTitles[i] || `Série ${i+1}`,
            type: chartTypes[i] || 'line',
            data: seriesData[i],
            color: color
        };

        // Nastavení používání druhé osy Y pro sloupcový graf (hodiny slunečního svitu)
        if (i === 2) { // Třetí série pro hodiny slunečního svitu vždy na druhé ose
            series.yaxis = 2; // Index 1 = první osa, 2 = druhá osa, atd.
        }

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
            width: [3, 3, 0] // Tloušťka čáry - 0 pro sloupcový graf
        },
        fill: {
            opacity: [1, 1, 0.7] // Průhlednost výplně - nižší pro sloupcový graf
        },
        markers: {
            size: [3, 3, 0], // Velikost značek - 0 pro sloupcový graf
            hover: {
                size: 6
            }
        },
        plotOptions: {
            bar: {
                columnWidth: '60%'
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
        yaxis: [
            {
                // První osa Y pro sluneční záření (W/m²)
                axisTicks: {
                    show: true
                },
                axisBorder: {
                    show: true,
                    color: '#0dcaf0'
                },
                title: {
                    text: yAxisTitle || 'W/m²'
                },
                labels: {
                    formatter: function(value) {
                        return value.toFixed(1);
                    }
                },
                min: function(min) {
                    return min < 0 ? 0 : min;
                }
            },
            {
                // Druhá osa Y pro hodiny slunečního svitu
                opposite: true,
                axisTicks: {
                    show: true
                },
                axisBorder: {
                    show: true,
                    color: '#ffc107'
                },
                title: {
                    text: 'Hodiny slunečního svitu'
                },
                labels: {
                    formatter: function(value) {
                        return value.toFixed(1) + ' h';
                    }
                },
                min: 0
            }
        ],
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
                formatter: function(value, { seriesIndex }) {
                    if (value === null || value === undefined) {
                        return "N/A";
                    }
                    if (seriesIndex === 2) { // Pro třetí sérii (hodiny slunečního svitu)
                        return value.toFixed(1) + ' hodin';
                    }
                    return value.toFixed(1) + ' W/m²';
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
        window.solarRadiationChart = new ApexCharts(chartElement, options);
        window.solarRadiationChart.render();
    } catch (error) {
        console.error('Chyba při inicializaci nebo vykreslování grafu slunečního záření:', error);
    }
}; 