﻿import { Component, Injector, AfterViewInit } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';

@Component({
    templateUrl: './home.component.html',
    animations: [appModuleAnimation()]
})
export class HomeComponent extends AppComponentBase implements AfterViewInit {

    constructor(
        injector: Injector
    ) {
        super(injector);
    }

    ngAfterViewInit(): void {

        $(function () {
            //Widgets count
            $('.count-to').countTo();

            //Sales count to
            $('.sales-count-to').countTo({
                formatter: function (value, options) {
                    return '$' + value.toFixed(2).replace(/(\d)(?=(\d\d\d)+(?!\d))/g, ' ').replace('.', ',');
                }
            });

            initRealTimeChart();
            initPieChart();
            initSparkline();
        });

        var realtime = 'on';
        function initRealTimeChart() {         
            var plot = ($ as any).plot('#real_time_chart', [getRandomData()], {
                series: {
                    shadowSize: 0,
                    color: 'rgb(0, 188, 212)'
                },
                grid: {
                    borderColor: '#f3f3f3',
                    borderWidth: 1,
                    tickColor: '#f3f3f3'
                },
                lines: {
                    fill: true
                },
                yaxis: {
                    min: 0,
                    max: 100
                },
                xaxis: {
                    min: 0,
                    max: 100
                }
            });

            function updateRealTime() {
                plot.setData([getRandomData()]);
                plot.draw();

                var timeout;
                if (realtime === 'on') {
                    timeout = setTimeout(updateRealTime, 320);
                } else {
                    clearTimeout(timeout);
                }
            }

            updateRealTime();

            $('#realtime').on('change', function () {
                realtime = this.checked ? 'on' : 'off';
                updateRealTime();
            });            
        }

        function initSparkline() {
            $(".sparkline").each(function () {
                var $this = $(this);
                $this.sparkline('html', $this.data());
            });
        }

        var pieChartData = [   
                                { label: 'Chrome', data: 33, color:'#E91E63'}, 
                                { label: 'Firefox',data: 30, color:'#03A9F4'}, 
                                { label: 'Safari', data: 18, color:'#FFC107'}, 
                                { label: 'Opera', data: 12, color:'#009688' },
                                { label: 'Other', data: 7, color:'#8DC44E' }
                            ]; 

        function initPieChart() {
            var plot = ($ as any).plot('#pie_chart', pieChartData, {
                series: {
                    pie: {
                        show: true,
                        radius: 1,
                        label: {
                            show: true,
                            radius: 3 / 4,
                            formatter: labelFormatter,
                            background: {
                                opacity: 0.5
                            }
                        }
                    }
                },
                legend: {
                    show: false
                }
            });
        }

        function labelFormatter(label, series) {
            return '<div style="font-size:8pt; text-align:center; padding:2px; color:white;">' + label + '<br/>' + Math.round(series.percent) + '%</div>';
        }

        var data = [], totalPoints = 110;
        
        function getRandomData() {
            if (data.length > 0) data = data.slice(1);

            while (data.length < totalPoints) {
                var prev = data.length > 0 ? data[data.length - 1] : 50, y = prev + Math.random() * 10 - 5;
                if (y < 0) { y = 0; } else if (y > 100) { y = 100; }

                data.push(y);
            }

            var res = [];
            for (var i = 0; i < data.length; ++i) {
                res.push([i, data[i]]);
            }

            return res;
        }
    }
}