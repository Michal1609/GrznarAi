// Funkce pro vykreslení sloupcového grafu srážek
window.renderRainChart = function (elementId, categories, rainData) {
    console.log("Vykreslování grafu srážek do elementu: " + elementId);
    console.log("Počet bodů: " + categories.length);
    
    // Kontrola existence elementu
    const chartElement = document.getElementById(elementId);
    if (!chartElement) {
        console.error(`Element s ID '${elementId}' nebyl nalezen!`);
        return;
    }
    
    // Zrušit existující graf, pokud existuje
    if (window.rainChart) {
        try {
            window.rainChart.destroy();
        } catch (e) {
            console.warn("Nepodařilo se zničit předchozí graf srážek:", e);
        }
        window.rainChart = null;
    }

    // Zjistíme typ dat podle formátu kategorie (HH:00 pro hodiny, dd.MM pro dny, MM.yyyy pro měsíce)
    let chartTitle = 'Historie srážek';
    let xaxisTitle = 'Čas';
    let tickAmount = undefined;
    
    if (categories && categories.length > 0) {
        const sampleCategory = categories[0];
        if (sampleCategory.includes(':00')) {
            chartTitle = 'Hodinová historie srážek';
            xaxisTitle = 'Hodina';
        } else if (sampleCategory.includes(' - ')) {
            chartTitle = 'Týdenní historie srážek';
            xaxisTitle = 'Týden';
        } else if (sampleCategory.includes('.') && !sampleCategory.includes('.20')) {
            chartTitle = 'Denní historie srážek';
            xaxisTitle = 'Den';
        } else if (sampleCategory.includes('.20')) {
            chartTitle = 'Měsíční historie srážek';
            xaxisTitle = 'Měsíc';
        }
    }

    try {
        // Vytvoření nového grafu
        window.rainChart = new ApexCharts(chartElement, {
            chart: {
                type: 'bar',
                height: 380,
                toolbar: { show: true },
                zoom: { enabled: true },
                animations: {
                    enabled: true,
                    easing: 'easeinout',
                    speed: 800
                }
            },
            plotOptions: {
                bar: {
                    columnWidth: '70%',
                    borderRadius: 2,
                    dataLabels: {
                        position: 'top'
                    }
                }
            },
            colors: ['#3498db'], // Modrá barva pro sloupce srážek
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
                    text: 'Srážky (mm)'
                },
                min: 0,
                max: function(max) { 
                    // Pokud je maximum 0, nastavíme osu na 5, jinak přidáme trochu místa nad maximum
                    return max === 0 ? 5 : Math.ceil(max * 1.2); 
                },
                decimalsInFloat: 1
            },
            dataLabels: {
                enabled: true,
                formatter: function(val) {
                    if (val === null || val === undefined || val === 0) {
                        return '';
                    }
                    return val.toFixed(1) + ' mm';
                },
                style: {
                    fontSize: '12px',
                    colors: ["#333"]
                },
                offsetY: -20
            },
            tooltip: {
                y: {
                    formatter: function (value) { 
                        if (value === null || value === undefined) {
                            return 'N/A';
                        }
                        return value.toFixed(1) + ' mm'; 
                    }
                }
            },
            legend: {
                show: false
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
                    name: 'Srážky',
                    data: rainData
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
        window.rainChart.render();
        console.log("Graf srážek byl úspěšně vykreslen");
        
    } catch (error) {
        console.error("Chyba při vykreslování grafu srážek:", error);
    }
}; 