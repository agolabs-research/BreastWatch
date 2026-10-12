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

            StatsMin.Content = minValue.ToString("F1");
            StatsMax.Content = maxValue.ToString("F1");
            StatsAvg.Content = avgValue.ToString("F1");
            StatsDevSt.Content = devstValue.ToString("F1");
            StatsScore.Content = score.ToString("F1");
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

            double pad_max = (graph_maxValue - graph_minValue) / 10; // pad value to get additional space for line labels
            double pad_min = (graph_maxValue - graph_minValue) / 10; // pad value to get additional space for line labels
            graph_maxValue += pad_max;
            graph_minValue -= pad_min;

            IpsiLungHistogramSeries.XAxis.Minimum = graph_minValue;
            IpsiLungHistogramSeries.XAxis.Maximum = graph_maxValue;

            InspectorPlotView.Model.InvalidatePlot(true);
        }
    }
}
