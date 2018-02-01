var baseUri = './Report/';

function ChartConfig() { }
ChartConfig.prototype.type = "";
ChartConfig.prototype.div = "";
ChartConfig.prototype.title = "";
ChartConfig.prototype.categories = [];
ChartConfig.prototype.data = [];
ChartConfig.prototype.names = [];
ChartConfig.prototype.colors = [];
ChartConfig.prototype.mode = "";
ChartConfig.prototype.height = 0;

function createChart(chartConfig) {

    var graphic = new Highcharts.Chart({

        chart: {
            renderTo: chartConfig.div,
            type: chartConfig.type,
            height: chartConfig.height
        },
        title: {
            text: chartConfig.title,
            style: {
                'fontSize': '15px'
            }
        },
        xAxis: {
            categories: [],
            tickInterval: 1,
            labels: {
                style: {
                    fontSize: '8px'
                }
            }
        },
        yAxis: {
            min: 0,
            text: '',
            title: {
                text: '',
                style: {
                    color: 'black'
                }
            }
        },
        plotOptions: {

            column: {
                dataLabels: {
                    enabled: true,
                    formatter: function () {
                        if (this.y != 0)
                            return chartConfig.mode === 1 ? this.y.toFixed(2) : this.y;
                        else
                            return null;
                    }
                },
                enableMouseTracking: false
            }
        },
        legend: {
            layout: 'horizontal',
            floating: true,
            textshadow: false,
            y: 20,
            itemStyle: {
                'fontSize': '10px',
                color: '#606060'
            },
            itemHoverStyle: {
                'fontSize': '10px',
                color: '#606060'
            }
        },
        tooltip: {
            formatter: function () {
                return '<b>' + this.series.name + '</b><br/>' +
                    this.x + ': ' + this.y.toFixed(2);
            }
        },
        series: [],
        credits: { enabled: false }
    });

    graphic.xAxis[0].setCategories(chartConfig.categories);

    for (var i = 0; i < chartConfig.data.length; i++) {

        graphic.addSeries({
            name: chartConfig.names[i],
            data: chartConfig.data[i],
            color: chartConfig.colors[i]
        });
    }
}

function generateChartByDay() {

    var month = document.getElementById("month").value;
    var monthLabel = document.getElementById("month").options[document.getElementById("month").selectedIndex].text;
    var year = document.getElementById("year").value;
    var util = new Util();

    if (month && year) {
        var strDate = "01/" + month + "/" + year;
        var date = util.stringToDate(strDate);
        var util = new Util();
        util.doAjax(baseUri + 'GetSendsByMonth', '{date: "' + date.toDateString() + '"}').then(function (data) {

            chartConfig = new ChartConfig();
            chartConfig.type = 'column';
            chartConfig.div = 'monthChart';
            chartConfig.names = ['Envios', 'Recebidos'];
            chartConfig.colors = ["#17c57b", "#0681cc"];
            chartConfig.data = [data[1], data[2]];
            chartConfig.title = 'Envios/Recebimentos por Dia (' + monthLabel + "/" + year + ')';
            chartConfig.categories = data[0];
            chartConfig.mode = 0;
            chartConfig.height = 300;

            createChart(chartConfig);
        }, function (err) { console.log(err.responseText); });

    }
}

function generateChartByMonth() {

    var year = document.getElementById("year").value;
    var util = new Util();
    util.doAjax(baseUri + 'GetSends', '{year: "' + year + '"}').then(function (data) {

        chartConfig = new ChartConfig();
        chartConfig.type = 'column';
        chartConfig.div = 'mainChart';
        chartConfig.names = ['Envios', 'Recebidos'];
        chartConfig.colors = ["#17c57b", "#0681cc"];
        chartConfig.data = [data[1], data[2]];
        chartConfig.title = 'Envios/Recebimentos (' + year + ')';
        chartConfig.categories = data[0];
        chartConfig.mode = 0;
        chartConfig.height = 300;

        createChart(chartConfig);
    }, function (err) { console.log(err); });
}

function generateSedeChartInYear() {

    var year = document.getElementById("year").value;
    var util = new Util();
    util.doAjax(baseUri + 'GetSedeSends', '{year: "' + year + '"}').then(function (data) {

        chartConfig = new ChartConfig();
        chartConfig.type = 'column';
        chartConfig.div = 'mainChartSede';
        chartConfig.names = ["Envios", "Recebimentos"];
        chartConfig.colors = ["#17c57b", "#0681cc"];
        chartConfig.data = [data[1], data[2]];
        chartConfig.title = 'Envios/Recebimentos por Sede (' + year + ')';
        chartConfig.categories = data[0];
        chartConfig.mode = 0;
        chartConfig.height = 300;

        createChart(chartConfig);
    }, function (err) { console.log(err); });
}

function generateSedeChartInMonth() {

    var month = document.getElementById("month").value;
    var monthLabel = document.getElementById("month").options[document.getElementById("month").selectedIndex].text;
    var year = document.getElementById("year").value;
    var util = new Util();

    if (month && year) {
        var strDate = "01/" + month + "/" + year;
        var date = util.stringToDate(strDate);
        var util = new Util();
        util.doAjax(baseUri + 'GetSedeSendsByMonth', '{date: "' + date.toDateString() + '"}').then(function (data) {

            chartConfig = new ChartConfig();
            chartConfig.type = 'column';
            chartConfig.div = 'monthChartSede';
            chartConfig.names = ["Envios", "Recebimentos"];
            chartConfig.colors = ["#17c57b", "#0681cc"];
            chartConfig.data = [data[1], data[2]];
            chartConfig.title = 'Envios/Recebimentos por Sede (' + monthLabel + "/" + year + ')';
            chartConfig.categories = data[0];
            chartConfig.mode = 0;
            chartConfig.height = 300;

            createChart(chartConfig);
        }, function (err) { console.log(err); });
    }
}

function update() {
    generateChartByMonth();
    generateSedeChartInYear();
    generateChartByDay();
    generateSedeChartInMonth();
}

document.getElementById("actionBtn").addEventListener('click', update);
//when the page load, give the current year as default option
document.getElementById("year").value = new Date().getFullYear();
document.getElementById("month").selectedIndex = new Date().getUTCMonth();
update();