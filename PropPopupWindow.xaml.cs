// This is BreastWatch for ESAPI
// Author: Stefano Agostinelli
// 
// Licensed under the terms described in the LICENSE file
// included with this source code.
//
// This software is provided for research purposes only.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OxyPlot;
using OxyPlot.Wpf;
using OxyPlot.Series;
using OxyPlot.Annotations;
using System.Diagnostics;
using OxyPlot.Axes;
using VMS.TPS.Common.Model.Types;

namespace BreastWatch 
{
    /// <summary>
    /// Interaction logic for PropPopupWindow.xaml
    /// </summary>
    /// 

    public class InspectorViewModel
    {
        public InspectorViewModel()
        {
            this.MyModelInspector = new PlotModel { };
        }
        public PlotModel MyModelInspector { get; private set; }
    }

    public class InspectorViewModelChrono
    {
        public InspectorViewModelChrono()
        {
            this.MyModelInspectorChrono = new PlotModel { };

            
        }
        public PlotModel MyModelInspectorChrono { get; private set; }
    }

    public partial class PropPopupWindow : Window
    {
        // Create dummy PlotView to force OxyPlot .Wpf to be loaded (Rex Anderson trick)
        private static readonly PlotView PlotView = new PlotView();

        public enum ScoreTails { ST_POSITIVE = 0, ST_NEGATIVE = 1, ST_BOTH = 2};
        ScoreTails scoreTails;

        public PropPopupWindow(List<double> values, MyItem itemSelected)
        {
            InitializeComponent();

            double itemSelectedResults = Convert.ToDouble(itemSelected.Results);
            double constValue = 0, accept_constValue = 0;
            
            if (itemSelected.Const == "NOC")
            {
                scoreTails = ScoreTails.ST_BOTH;
            }
            else if (itemSelected.Data.Contains("C"))
            {
                if (itemSelected.Const.Contains('/')) // check for optimal/acceptable couple
                {
                    string trimmedConst = itemSelected.Const.TrimStart('>');
                    constValue = Convert.ToDouble(trimmedConst.Substring(0, itemSelected.Const.IndexOf('/') - 1));
                    accept_constValue = Convert.ToDouble(trimmedConst.Substring(itemSelected.Const.IndexOf('/')));
                }
                else
                {
                    constValue = Convert.ToDouble(itemSelected.Const.TrimStart('>'));
                }
                scoreTails = ScoreTails.ST_POSITIVE;
            }
            else
            {
                if (itemSelected.Const.Contains('/'))
                {
                    string trimmedConst = itemSelected.Const.TrimStart('<');
                    constValue = Convert.ToDouble(trimmedConst.Substring(0, itemSelected.Const.IndexOf('/') - 1));
                    accept_constValue = Convert.ToDouble(trimmedConst.Substring(itemSelected.Const.IndexOf('/')));
                }
                else
                {
                    constValue = Convert.ToDouble(itemSelected.Const.Replace('<', ' ').Replace('~', ' '));
                }
                scoreTails = ScoreTails.ST_NEGATIVE;
            }

            Title = "Data Inspector (" + itemSelected.Data + " = " + itemSelected.Results + ")";
            PropRecords.Text = "FILTERED RECORDS = " + values.Count();

            if (scoreTails != ScoreTails.ST_BOTH)
                StatsScore.ToolTip = "Represents the % of filtered records with values worse than current Result";
            else
                StatsScore.ToolTip = "100/50/25 if current Result within 1/2/3 SDs from average";

            HistogramSeries IpsiLungHistogramSeries = new HistogramSeries();
            int numberOfBins = (int)Math.Sqrt(values.Count() + 0.5) + 1;
            BinningOptions binningOptions = new BinningOptions(BinningOutlierMode.CountOutliers, BinningIntervalType.InclusiveLowerBound, BinningExtremeValueMode.IncludeExtremeValues);

            double minValue = 0, maxValue = 0, avgValue = 0, devstValue = 0;
            double score = 0;
            minValue = values.Min();
            maxValue = values.Max();
            avgValue = values.Average();
            devstValue = Math.Sqrt(values.Sum(d => Math.Pow(d - avgValue, 2)) / values.Count());

            if (scoreTails == ScoreTails.ST_POSITIVE)
                score = values.Count(d => itemSelectedResults >= d) * 100 / values.Count();
            else if (scoreTails == ScoreTails.ST_NEGATIVE)
                score = values.Count(d => itemSelectedResults <= d) * 100 / values.Count();
            else if (scoreTails == ScoreTails.ST_BOTH)
            {
                if (itemSelectedResults <= avgValue + devstValue && itemSelectedResults >= avgValue - devstValue)
                    score = 100;
                else if (itemSelectedResults <= avgValue + 2*devstValue && itemSelectedResults >= avgValue - 2*devstValue)
                    score = 50;
                else if (itemSelectedResults <= avgValue + 3*devstValue && itemSelectedResults >= avgValue - 3*devstValue)
                    score = 25;
                else
                    score = 0;
            }

            if (maxValue == minValue)
                maxValue = minValue + 0.01;

            var bins = HistogramHelpers.CreateUniformBins(minValue, maxValue, numberOfBins);

            var items = HistogramHelpers.Collect(values, bins, binningOptions);
            IpsiLungHistogramSeries.Items.AddRange(items);

            LineAnnotation Line = new LineAnnotation()
            {
                StrokeThickness = 3,
                Color = OxyColors.Red,
                Type = LineAnnotationType.Vertical,
                Text = itemSelected.Results,
                TextColor = OxyColors.Red,
                LineStyle = LineStyle.Dash,
                X = itemSelectedResults,
                Y = 0.1
            };

            LineAnnotation LineAvg = new LineAnnotation()
            {
                StrokeThickness = 2,
                Color = OxyColors.Blue,
                Type = LineAnnotationType.Vertical,
                Text = avgValue.ToString("F1"),
                TextColor = OxyColors.Blue,
                TextLinePosition = 0.7,
                LineStyle = LineStyle.Solid,
                X = avgValue,
                Y = 0.1
            };

            LineAnnotation LineAvgPosSD = new LineAnnotation()
            {
                StrokeThickness = 2,
                Color = OxyColors.Blue,
                Type = LineAnnotationType.Vertical,
                LineStyle = LineStyle.Dash,
                X= avgValue + devstValue,
                Y = 0.1
            };

            LineAnnotation LineAvgNegSD = new LineAnnotation()
            {
                StrokeThickness = 2,
                Color = OxyColors.Blue,
                Type = LineAnnotationType.Vertical,
                LineStyle = LineStyle.Dash,
                X = avgValue - devstValue,
                Y = 0.1
            };

            LineAnnotation LineConst = new LineAnnotation()
            {
                StrokeThickness = 2,
                Color = OxyColors.Magenta,
                Type = LineAnnotationType.Vertical,
                Text = constValue.ToString("F1"),
                TextColor = OxyColors.Magenta,
                TextLinePosition = 0.7,
                LineStyle = LineStyle.Solid,
                X = constValue,
                Y = 0.1
            };

            LineAnnotation LineAcceptConst = new LineAnnotation()
            {
                StrokeThickness = 2,
                Color = OxyColors.Magenta,
                Type = LineAnnotationType.Vertical,
                Text = accept_constValue.ToString("F1"),
                TextColor = OxyColors.Magenta,
                TextLinePosition = 0.7,
                LineStyle = LineStyle.Dash,
                X = accept_constValue,
                Y = 0.1
            };

            StatsMin.Content = minValue.ToString("F2");
            StatsMax.Content = maxValue.ToString("F2");
            StatsAvg.Content = avgValue.ToString("F2");
            StatsDevSt.Content = devstValue.ToString("F2");
            StatsScore.Content = score.ToString("F0");
            if (score > 75)
                StatsScore.Foreground = Brushes.Green;
            else if (score > 50)
                StatsScore.Foreground = Brushes.Orange;
            else
                StatsScore.Foreground = Brushes.Red;

            InspectorPlotView.Model.Series.Clear();
            InspectorPlotView.Model.Series.Add(IpsiLungHistogramSeries);

            InspectorPlotView.Model.Annotations.Add(Line);
            InspectorPlotView.Model.Annotations.Add(LineAvg);
            if (scoreTails == ScoreTails.ST_POSITIVE)
                InspectorPlotView.Model.Annotations.Add(LineAvgNegSD);
            else if (scoreTails == ScoreTails.ST_NEGATIVE)
                InspectorPlotView.Model.Annotations.Add(LineAvgPosSD);
            else if (scoreTails == ScoreTails.ST_BOTH)
            {
                InspectorPlotView.Model.Annotations.Add(LineAvgPosSD);
                InspectorPlotView.Model.Annotations.Add(LineAvgNegSD);
            }

            if (constValue > 0)
                InspectorPlotView.Model.Annotations.Add(LineConst);
            if(accept_constValue > 0)
                InspectorPlotView.Model.Annotations.Add(LineAcceptConst);

            InspectorPlotView.Model.InvalidatePlot(true);

            double graph_maxValue = Math.Max(Math.Max(maxValue,itemSelectedResults), Math.Max(constValue, accept_constValue));
            graph_maxValue = Math.Max(graph_maxValue, avgValue + devstValue);
            double graph_minValue = Math.Min(minValue, itemSelectedResults);
            if (constValue > 0)
                graph_minValue = Math.Min(graph_minValue, constValue);
            if (accept_constValue > 0)
                graph_minValue = Math.Min(graph_minValue, accept_constValue);
            graph_minValue = Math.Min(graph_minValue, avgValue - devstValue);

            double pad_max = (graph_maxValue - graph_minValue) / 8; // pad value to get additional space for line labels
            double pad_min = (graph_maxValue - graph_minValue) / 8; // pad value to get additional space for line labels
            graph_maxValue += pad_max;
            graph_minValue -= pad_min;

            IpsiLungHistogramSeries.XAxis.Minimum = graph_minValue;
            IpsiLungHistogramSeries.XAxis.Maximum = graph_maxValue;

            InspectorPlotView.Model.InvalidatePlot(true);

            // Chronological Time Series plot
            
            ScatterSeries ChronoSeries = new ScatterSeries { MarkerType = MarkerType.Circle, MarkerSize = 2, MarkerFill = OxyColors.Black};

            int i = 0;
            foreach (var item in values)
                {
                i++;
                ChronoSeries.Points.Add(new ScatterPoint(i, item));
                }

            int n = 10;
            double f = 1;
            if (n >= values.Count)
            {
                if (values.Count == 3)
                    n = 2;
                else if (values.Count < 3)
                    n = 1;
                else
                    n = values.Count / 2;
            }

            LineSeries RunningSeries = new LineSeries {Color = OxyColors.CadetBlue };
            List<double> ma = GetMovingAverages(values, n);
            ma[values.Count] = ma[values.Count - 1];

            AreaSeries RunningBBSeries = new AreaSeries { Color = OxyColors.Orange, Color2 = OxyColors.Green, };
            List<double> msd = GetMovingSquares(values, n);
            msd[values.Count] = msd[values.Count - 1];

            for (i=1; i<=msd.Count; i++)
            {
                RunningSeries.Points.Add(new DataPoint(i, ma[i-1]));
                RunningBBSeries.Points.Add(new DataPoint(i, ma[i-1] + f*Math.Sqrt(msd[i-1]/n)));
                RunningBBSeries.Points2.Add(new DataPoint(i, ma[i-1] - f*Math.Sqrt(msd[i-1]/n)));
            }

            ScatterSeries ThisValueSeries = new ScatterSeries { MarkerType = MarkerType.Circle, MarkerSize = 3, MarkerFill = OxyColors.Red };
            ThisValueSeries.Points.Add(new ScatterPoint(values.Count + 1,itemSelectedResults));

            InspectorPlotViewChrono.Model.Series.Add(RunningBBSeries);
            InspectorPlotViewChrono.Model.Series.Add(RunningSeries);
            InspectorPlotViewChrono.Model.Series.Add(ChronoSeries);
            InspectorPlotViewChrono.Model.Series.Add(ThisValueSeries);

            LineAnnotation LineConstChrono = new LineAnnotation()
            {
                StrokeThickness = 2,
                Color = OxyColors.Magenta,
                Type = LineAnnotationType.Horizontal,
                Text = constValue.ToString("F1"),
                TextColor = OxyColors.Magenta,
                TextLinePosition = 1,
                TextVerticalAlignment = OxyPlot.VerticalAlignment.Bottom,
                LineStyle = LineStyle.Solid,
                X = 0.1,
                Y = constValue
            };

            LineAnnotation LineAcceptConstChrono = new LineAnnotation()
            {
                StrokeThickness = 2,
                Color = OxyColors.Magenta,
                Type = LineAnnotationType.Horizontal,
                Text = accept_constValue.ToString("F1"),
                TextColor = OxyColors.Magenta,
                TextLinePosition = 1,
                TextVerticalAlignment = OxyPlot.VerticalAlignment.Bottom,
                LineStyle = LineStyle.Dash,
                X = 0.1,
                Y = accept_constValue
            };

            if (itemSelected.Const != "NOC")
            {
                InspectorPlotViewChrono.Model.Annotations.Add(LineConstChrono);
                if (accept_constValue > 0)
                    InspectorPlotViewChrono.Model.Annotations.Add(LineAcceptConstChrono);
            }

            InspectorPlotViewChrono.Model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, MinimumPadding = 0.05, MaximumPadding = 0.05});
            InspectorPlotViewChrono.Model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, MinimumPadding = 0.1, MaximumPadding = 0.1, MajorGridlineStyle = LineStyle.Automatic, MinorGridlineStyle = LineStyle.Automatic });

            InspectorPlotViewChrono.Model.InvalidatePlot(true);
        }
        
        // n is the window for your Simple Moving Average
        public List<double> GetMovingAverages(List<double> values, int n)
        {
            var movingAverages = new double[values.Count+1];
            var runningTotal = 0.0d;
            
            for (int i = 0; i < values.Count; ++i)
            {
                runningTotal += values[i];
                if (i - n >= 0)
                {
                    var lost = values[i - n];
                    runningTotal -= lost;
                    movingAverages[i] = runningTotal / n;
                }
            }

            for (int i = 0; i < n; ++i)
                movingAverages[i] = double.NaN;
            
            /*
            runningTotal = 0.0d;
            for (int i = 0; i < n; ++i)
            {
                runningTotal += values[i];
                movingAverages[i] = runningTotal / (i+1);
            }
            */

            return movingAverages.ToList();
        }

        public List<double> GetMovingSquares(List<double> values, int n)
        {
            var movingAverages = new double[values.Count+1];
            var movingSquares = new double[values.Count+1];
            var runningTotal = 0.0d;
            var runningSquare = 0.0d;
            

            for (int i = 0; i < values.Count; ++i)
            {
                runningTotal += values[i];
                runningSquare += Math.Pow(values[i], 2);
                if (i - n >= 0)
                {
                    var lost = values[i - n];
                    var lostSquare = Math.Pow(values[i - n], 2);
                    runningTotal -= lost;
                    runningSquare -= lostSquare;
                    movingAverages[i] = runningTotal / n;
                    movingSquares[i] = runningSquare - Math.Pow(runningTotal,2) / n;
                }
            }

            for (int i = 0; i < n; ++i)
                movingSquares[i] = double.NaN;
            
            return movingSquares.ToList();
        }
    }
}
