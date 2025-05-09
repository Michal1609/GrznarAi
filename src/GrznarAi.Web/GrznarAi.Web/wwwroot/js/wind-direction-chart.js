// Funkce pro vykreslení grafu směru větru
window.renderWindDirectionChart = function (elementId, categories, windDirectionData) {
    console.log("Vykreslování grafu směru větru do elementu: " + elementId);
    console.log("Počet bodů: " + categories.length);
    
    // Kontrola existence elementu
    const chartElement = document.getElementById(elementId);
    if (!chartElement) {
        console.error(`Element s ID '${elementId}' nebyl nalezen!`);
        return;
    }
    
    // Zrušit existující graf, pokud existuje
    if (window.windDirectionChart) {
        try {
            window.windDirectionChart.destroy();
        } catch (e) {
            console.warn("Nepodařilo se zničit předchozí graf:", e);
        }
        window.windDirectionChart = null;
    }

    // Převedeme data do formátu vhodného pro scatter plot
    const scatterData = [];
    for (let i = 0; i < categories.length; i++) {
        if (windDirectionData[i] !== null && windDirectionData[i] !== undefined) {
            scatterData.push({
                x: categories[i],
                y: windDirectionData[i]
            });
        }
    }

    // Zjistíme typ dat podle formátu kategorie
    let chartTitle = 'Historie směru větru';
    let xaxisTitle = 'Čas';
    let tickAmount = undefined;
    
    if (categories && categories.length > 0) {
        const sampleCategory = categories[0];
        if (sampleCategory.includes(':00')) {
            chartTitle = 'Hodinová historie směru větru';
            xaxisTitle = 'Hodina';
        } else if (sampleCategory.includes(' - ')) {
            chartTitle = 'Týdenní historie směru větru';
            xaxisTitle = 'Týden';
        } else if (sampleCategory.includes('.') && !sampleCategory.includes('.20')) {
            chartTitle = 'Denní historie směru větru';
            xaxisTitle = 'Den';
        } else if (sampleCategory.includes('.20')) {
            chartTitle = 'Měsíční historie směru větru';
            xaxisTitle = 'Měsíc';
        }
    }

    try {
        // Vytvoření nového grafu
        window.windDirectionChart = new ApexCharts(chartElement, {
            chart: {
                type: 'scatter',
                height: 380,
                toolbar: { show: true },
                zoom: { enabled: true },
                animations: {
                    enabled: true,
                    easing: 'easeinout',
                    speed: 800
                }
            },
            colors: ['#9b59b6'],
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
                    text: 'Směr větru (°)'
                },
                min: 0,
                max: 360,
                tickAmount: 8,
                labels: {
                    formatter: function(val) {
                        // Přidáme textové směry k hlavním hodnotám na ose Y
                        if (val === 0) return 'S (0°)';
                        if (val === 45) return 'SV (45°)';
                        if (val === 90) return 'V (90°)';
                        if (val === 135) return 'JV (135°)';
                        if (val === 180) return 'J (180°)';
                        if (val === 225) return 'JZ (225°)';
                        if (val === 270) return 'Z (270°)';
                        if (val === 315) return 'SZ (315°)';
                        if (val === 360) return 'S (360°)';
                        return val.toFixed(0) + '°';
                    }
                }
            },
            tooltip: {
                shared: false,
                intersect: true,
                y: {
                    formatter: function (value) { 
                        if (value === null || value === undefined) {
                            return 'N/A';
                        }
                        // Přidáme textový směr do tooltipu
                        let direction = '';
                        if (value >= 337.5 || value < 22.5) direction = 'S (Sever)';
                        else if (value >= 22.5 && value < 67.5) direction = 'SV (Severovýchod)';
                        else if (value >= 67.5 && value < 112.5) direction = 'V (Východ)';
                        else if (value >= 112.5 && value < 157.5) direction = 'JV (Jihovýchod)';
                        else if (value >= 157.5 && value < 202.5) direction = 'J (Jih)';
                        else if (value >= 202.5 && value < 247.5) direction = 'JZ (Jihozápad)';
                        else if (value >= 247.5 && value < 292.5) direction = 'Z (Západ)';
                        else if (value >= 292.5 && value < 337.5) direction = 'SZ (Severozápad)';
                        
                        return value.toFixed(1) + '° ' + direction; 
                    }
                }
            },
            legend: {
                show: false
            },
            markers: {
                size: 6,
                shape: "circle",
                hover: { size: 8 }
            },
            grid: {
                borderColor: '#e7e7e7',
                row: {
                    colors: ['#f3f3f3', 'transparent'],
                    opacity: 0.5
                }
            },
            series: [{
                name: 'Směr větru',
                data: scatterData
            }],
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
        window.windDirectionChart.render();
        console.log("Graf směru větru byl úspěšně vykreslen");
    } catch (error) {
        console.error("Chyba při vykreslování grafu směru větru:", error);
    }
}; 