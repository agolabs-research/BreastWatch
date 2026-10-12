// This is BreastWatch for ESAPI
// Author: Stefano Agostinelli
// 
// Licensed under the terms described in the LICENSE file
// included with this source code.
//
// This software is provided for research purposes only.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Xml.Linq;
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
using System.Globalization;
using System.IO;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using OxyPlot;
using OxyPlot.Wpf;
using OxyPlot.Series;
using OxyPlot.Annotations;
using static BreastWatch.PropPopupWindow;
using System.Net;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Runtime.Remoting.Lifetime;
using System.Security.Cryptography;
using System.Security.Policy;

namespace BreastWatch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 

    public enum ScoreStyle { Protocol = 0, Community, Gradient };

    // Protocol Style
    public class mListViewItemProtocolStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item,
            DependencyObject container)
        {
            Style st = new Style();
            st.TargetType = typeof(ListViewItem);
            Setter backGroundSetter = new Setter();
            backGroundSetter.Property = ListViewItem.BackgroundProperty;
            ListView listView =
                ItemsControl.ItemsControlFromItemContainer(container)
                  as ListView;

            MyItem imyItem = (MyItem)item;
            if (String.IsNullOrEmpty(imyItem.DTName) == false)
            {
                Setter pSetter = new Setter();
                pSetter.Property = ListViewItem.BorderBrushProperty;
                pSetter.Value = Brushes.Blue;
                st.Setters.Add(pSetter);
            }

            if (imyItem.Results == null)
            {
                backGroundSetter.Value = Brushes.LightGray;
            }
            else
            {
                SolidColorBrush passColor, failColor, warnColor;

                if (imyItem.Data.Contains("*"))
                {
                    passColor = Brushes.LightGreen;
                    warnColor = Brushes.LightYellow;
                    failColor = Brushes.Orange;
                }
                else
                {
                    passColor = Brushes.LightGreen;
                    warnColor = Brushes.Yellow;
                    failColor = Brushes.Tomato;
                }

                if (imyItem.Data.Contains("Avg"))
                {
                    if (imyItem.Score >= 1)
                        backGroundSetter.Value = passColor;
                    else if (imyItem.Score == 0)
                        backGroundSetter.Value = failColor;
                    else
                        backGroundSetter.Value = warnColor;

                }
                else if (imyItem.Data.Contains("Max"))
                {
                    if (imyItem.Score >= 1)
                        backGroundSetter.Value = passColor;
                    else if (imyItem.Score == 0)
                        backGroundSetter.Value = failColor;
                    else
                        backGroundSetter.Value = warnColor;
                }
                else if (imyItem.Data.Contains('V'))
                {
                    if (imyItem.Score >= 1)
                        backGroundSetter.Value = passColor;
                    else if (imyItem.Score == 0)
                        backGroundSetter.Value = failColor;
                    else
                        backGroundSetter.Value = warnColor;
                }
                else if (imyItem.Data.Contains('C'))
                {
                    if (imyItem.Score >= 1)
                        backGroundSetter.Value = passColor;
                    else if (imyItem.Score == 0)
                        backGroundSetter.Value = failColor;
                    else
                        backGroundSetter.Value = warnColor;
                }
                else if (imyItem.Data.Contains('R'))
                {
                    if (imyItem.Score >= 1)
                        backGroundSetter.Value = passColor;
                    else if (imyItem.Score == 0)
                        backGroundSetter.Value = failColor;
                    else
                        backGroundSetter.Value = warnColor;
                }
                else if (imyItem.Data.Contains("D%")) // Dose received by volume in %
                {
                    if (imyItem.Score >= 1)
                        backGroundSetter.Value = passColor;
                    else if (imyItem.Score == 0)
                        backGroundSetter.Value = failColor;
                    else
                        backGroundSetter.Value = warnColor;
                }
                else if (imyItem.Data.Contains("Dc")) // Dose received by volume in cc
                {
                    if (imyItem.Score >= 1)
                        backGroundSetter.Value = passColor;
                    else if (imyItem.Score == 0)
                        backGroundSetter.Value = failColor;
                    else
                        backGroundSetter.Value = warnColor;
                }

                else
                    backGroundSetter.Value = Brushes.Blue;
            }
            st.Setters.Add(backGroundSetter);

            return st;
        }
    }

    public static class ScorePalette
    {
        // Red to Green color palette https://loading.io/color/feature/RdYlGn-9/
        //static uint[] ScorePalette = {0Xd73027,0xf46d43,0xfdae61,0xfee08b,0xffffbf,0xd9ef8b,0xa6d96a,0x66bd63,0x1a9850};
        // Modified palette: Tomato-Yellow-Light Green

        public static uint[] ScorePalette_rgb = { 0Xff6347, 0xf46d43, 0xfdae61, 0xfee08b, 0xffff00, 0xddfe3f, 0xbefa5f, 0xa4f57a, 0x90ee90 };
        public static int ScorePalette_levels = 9;
        public static uint GetPaletteRGB(double Score)
        {
            // Score 0-100
            if (Score < 0 || Score > 100)
                return 0;
            else
                return ScorePalette_rgb[(int)(Score * (ScorePalette_levels - 1) / 100.0 + 0.5)];
        }
    }

    // Community Gradient Style
    public class mListViewItemCommunityGradientStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item,
            DependencyObject container)
        {
            Style st = new Style();
            st.TargetType = typeof(ListViewItem);
            Setter backGroundSetter = new Setter();
            backGroundSetter.Property = ListViewItem.BackgroundProperty;
            ListView listView =
                ItemsControl.ItemsControlFromItemContainer(container)
                  as ListView;

            MyItem imyItem = (MyItem)item;
            if (String.IsNullOrEmpty(imyItem.DTName) == false)
            {
                Setter pSetter = new Setter();
                pSetter.Property = ListViewItem.BorderBrushProperty;
                pSetter.Value = Brushes.Blue;
                st.Setters.Add(pSetter);
            }

            if (imyItem.Results == null)
            {
                backGroundSetter.Value = Brushes.LightGray;
            }
            else
            {
                SolidColorBrush passColor, failColor, warnColor;
                SolidColorBrush ScoreColor;

                if (imyItem.Data.Contains("*"))
                {
                    passColor = Brushes.LightGreen;
                    warnColor = Brushes.LightYellow;
                    failColor = Brushes.Orange;
                }
                else
                {
                    passColor = Brushes.LightGreen;
                    warnColor = Brushes.Yellow;
                    failColor = Brushes.Tomato;
                }

                if (imyItem.Data.Contains("Avg"))
                {
                    if (imyItem.Score >= 1)
                        ScoreColor = passColor;
                    else if (imyItem.Score == 0)
                        ScoreColor = failColor;
                    else
                        ScoreColor = warnColor;

                }
                else if (imyItem.Data.Contains("Max"))
                {
                    if (imyItem.Score >= 1)
                        ScoreColor = passColor;
                    else if (imyItem.Score == 0)
                        ScoreColor = failColor;
                    else
                        ScoreColor = warnColor;
                }
                else if (imyItem.Data.Contains('V'))
                {
                    if (imyItem.Score >= 1)
                        ScoreColor = passColor;
                    else if (imyItem.Score == 0)
                        ScoreColor = failColor;
                    else
                        ScoreColor = warnColor;
                }
                else if (imyItem.Data.Contains('C'))
                {
                    if (imyItem.Score >= 1)
                        ScoreColor = passColor;
                    else if (imyItem.Score == 0)
                        ScoreColor = failColor;
                    else
                        ScoreColor = warnColor;
                }
                else if (imyItem.Data.Contains('R'))
                {
                    if (imyItem.Score >= 1)
                        ScoreColor = passColor;
                    else if (imyItem.Score == 0)
                        ScoreColor = failColor;
                    else
                        ScoreColor = warnColor;
                }
                else if (imyItem.Data.Contains("D%")) // Dose received by volume in %
                {
                    if (imyItem.Score >= 1)
                        ScoreColor = passColor;
                    else if (imyItem.Score == 0)
                        ScoreColor = failColor;
                    else
                        ScoreColor = warnColor;
                }
                else if (imyItem.Data.Contains("Dc")) // Dose received by volume in cc
                {
                    if (imyItem.Score >= 1)
                        ScoreColor = passColor;
                    else if (imyItem.Score == 0)
                        ScoreColor = failColor;
                    else
                        ScoreColor = warnColor;
                }

                else
                    ScoreColor = Brushes.Blue;

                if (imyItem.CommunityScore != -1 && imyItem.CommunityScore >= 0 && imyItem.CommunityScore <= 100)
                {
                    uint cbRGB = ScorePalette.GetPaletteRGB(imyItem.CommunityScore);

                    LinearGradientBrush CBColor = new LinearGradientBrush(ScoreColor.Color, Color.FromRgb((byte)((cbRGB >> 16) & 0xFF), (byte)((cbRGB >> 8) & 0xFF), (byte)(cbRGB & 0xFF)), new Point(0.5, 0.5), new Point(1, 0.5));
                    backGroundSetter.Value = CBColor;
                }
                else
                {
                    LinearGradientBrush CBColor = new LinearGradientBrush(ScoreColor.Color, Brushes.LightGray.Color, new Point(0.5, 0.5), new Point(1, 0.5));
                    backGroundSetter.Value = CBColor;
                }
            }

            st.Setters.Add(backGroundSetter);

            return st;
        }
    }

    // Community Style
    public class mListViewItemCommunityStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item,
            DependencyObject container)
        {
            Style st = new Style();
            st.TargetType = typeof(ListViewItem);
            Setter backGroundSetter = new Setter();
            backGroundSetter.Property = ListViewItem.BackgroundProperty;
            ListView listView =
                ItemsControl.ItemsControlFromItemContainer(container)
                  as ListView;

            MyItem imyItem = (MyItem)item;
            if (String.IsNullOrEmpty(imyItem.DTName) == false)
            {
                Setter pSetter = new Setter();
                pSetter.Property = ListViewItem.BorderBrushProperty;
                pSetter.Value = Brushes.Blue;
                st.Setters.Add(pSetter);
            }

            if (imyItem.Results == null)
            {
                backGroundSetter.Value = Brushes.LightGray;
            }
            else
            {
                SolidColorBrush passColor, failColor, warnColor;
                SolidColorBrush ScoreColor;

                if (imyItem.Data.Contains("*"))
                {
                    passColor = Brushes.LightGreen;
                    warnColor = Brushes.LightYellow;
                    failColor = Brushes.Orange;
                }
                else
                {
                    passColor = Brushes.LightGreen;
                    warnColor = Brushes.Yellow;
                    failColor = Brushes.Tomato;
                }

                if (imyItem.Data.Contains("Avg"))
                {
                    if (imyItem.Score >= 1)
                        ScoreColor = passColor;
                    else if (imyItem.Score == 0)
                        ScoreColor = failColor;
                    else
                        ScoreColor = warnColor;

                }
                else if (imyItem.Data.Contains("Max"))
                {
                    if (imyItem.Score >= 1)
                        ScoreColor = passColor;
                    else if (imyItem.Score == 0)
                        ScoreColor = failColor;
                    else
                        ScoreColor = warnColor;
                }
                else if (imyItem.Data.Contains('V'))
                {
                    if (imyItem.Score >= 1)
                        ScoreColor = passColor;
                    else if (imyItem.Score == 0)
                        ScoreColor = failColor;
                    else
                        ScoreColor = warnColor;
                }
                else if (imyItem.Data.Contains('C'))
                {
                    if (imyItem.Score >= 1)
                        ScoreColor = passColor;
                    else if (imyItem.Score == 0)
                        ScoreColor = failColor;
                    else
                        ScoreColor = warnColor;
                }
                else if (imyItem.Data.Contains('R'))
                {
                    if (imyItem.Score >= 1)
                        ScoreColor = passColor;
                    else if (imyItem.Score == 0)
                        ScoreColor = failColor;
                    else
                        ScoreColor = warnColor;
                }
                else if (imyItem.Data.Contains("D%")) // Dose received by volume in %
                {
                    if (imyItem.Score >= 1)
                        ScoreColor = passColor;
                    else if (imyItem.Score == 0)
                        ScoreColor = failColor;
                    else
                        ScoreColor = warnColor;
                }
                else if (imyItem.Data.Contains("Dc")) // Dose received by volume in cc
                {
                    if (imyItem.Score >= 1)
                        ScoreColor = passColor;
                    else if (imyItem.Score == 0)
                        ScoreColor = failColor;
                    else
                        ScoreColor = warnColor;
                }

                else
                    ScoreColor = Brushes.Blue;

                if (imyItem.CommunityScore != -1 && imyItem.CommunityScore >= 0 && imyItem.CommunityScore <= 100)
                {
                    uint cbRGB = ScorePalette.GetPaletteRGB(imyItem.CommunityScore);

                    SolidColorBrush CBColor = new SolidColorBrush(Color.FromRgb((byte)((cbRGB >> 16) & 0xFF), (byte)((cbRGB >> 8) & 0xFF), (byte)(cbRGB & 0xFF)));
                    backGroundSetter.Value = CBColor;
                }
                else
                    backGroundSetter.Value = Brushes.LightGray;
            }

            st.Setters.Add(backGroundSetter);

            return st;
        }
    }

    public class MyItem : INotifyPropertyChanged
    {
        private double _ReferenceDose;
        public double ReferenceDose
        {
            get { return _ReferenceDose; }
            set { _ReferenceDose = value; OnPropertyChanged(); }
        }
        private string _itemData;
        public string Data
        {
            get { return _itemData; }
            set { _itemData = value; OnPropertyChanged(); }
        }
        private string _itemConst;
        public string Const
        {
            get { return _itemConst; }
            set { _itemConst = value; OnPropertyChanged(); }
        }
        private string _itemResults;
        public string Results
        {
            get { return _itemResults; }
            set { _itemResults = value; OnPropertyChanged(); }
        }
        private string _itemResultsPerc;
        public string ResultsPerc
        {
            get { return _itemResultsPerc; }
            set { _itemResultsPerc = value; OnPropertyChanged(); }
        }
        private double _itemScore;
        public double Score
        {
            get { return _itemScore; }
            set { _itemScore = value; OnPropertyChanged(); }
        }
        private double _itemCBScore = -1;
        public double CommunityScore
        {
            get { return _itemCBScore; }
            set { _itemCBScore = value; OnPropertyChanged(); }
        }
        private string _itemCBScoreString;
        public string CommunityScoreString
        {
            get { if (_itemCBScore == -1) return "NA"; else return _itemCBScore.ToString("F0"); }
            set { _itemCBScoreString = value; OnPropertyChanged(); }
        }
        private string _itemDTName;
        public string DTName
        {
            get { return _itemDTName; }
            set { _itemDTName = value; OnPropertyChanged(); }
        }

        public double GetCommunityScore(DataRow[] filteredRows)
        {
            string SelectedItemDTName = this.DTName;
            if (string.IsNullOrEmpty(SelectedItemDTName))
                return -1;

            PropPopupWindow.ScoreTails ItemScoreTails;
            double itemSelectedResults = Convert.ToDouble(this.Results);

            if (this.Const == "NOC")
                ItemScoreTails = ScoreTails.ST_BOTH;
            else if (this.Data.Contains("C"))
                ItemScoreTails = ScoreTails.ST_POSITIVE;
            else
                ItemScoreTails = ScoreTails.ST_NEGATIVE;

            double k;
            List<double> values = new List<double>();
            values = filteredRows.AsEnumerable().Where(x => double.TryParse(x[SelectedItemDTName].ToString(), out k) == true).Select(x => Convert.ToDouble(x[SelectedItemDTName])).ToList();
            if (values.Count > 0)
            {
                double minValue = 0, maxValue = 0, avgValue = 0, devstValue = 0;
                double cbscore = 0;
                minValue = values.Min();
                maxValue = values.Max();
                avgValue = values.Average();
                devstValue = Math.Sqrt(values.Sum(d => Math.Pow(d - avgValue, 2)) / values.Count());

                if (ItemScoreTails == ScoreTails.ST_POSITIVE)
                    cbscore = values.Count(d => itemSelectedResults >= d) * 100 / values.Count();
                else if (ItemScoreTails == ScoreTails.ST_NEGATIVE)
                    cbscore = values.Count(d => itemSelectedResults <= d) * 100 / values.Count();
                else if (ItemScoreTails == ScoreTails.ST_BOTH)
                {
                    if (itemSelectedResults <= avgValue + devstValue && itemSelectedResults >= avgValue - devstValue)
                        cbscore = 100;
                    else if (itemSelectedResults <= avgValue + 2 * devstValue && itemSelectedResults >= avgValue - 2 * devstValue)
                        cbscore = 50;
                    else if (itemSelectedResults <= avgValue + 3 * devstValue && itemSelectedResults >= avgValue - 3 * devstValue)
                        cbscore = 25;
                    else
                        cbscore = 0;
                }

                this.CommunityScore = cbscore;
                return cbscore;
            }
            return -1;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class MainWindow : Window
    {
        // Create dummy PlotView to force OxyPlot .Wpf to be loaded (Rex Anderson trick)
        private static readonly PlotView PlotView = new PlotView();

        private Patient pat;
        private PlanSetup planSetup;
        private PlanSum planSum;
        private StructureSet structureSet;
        private string userName;
        bool isPscOk;
        string PscCheckString;
        string PlanDataString;

        public DoseValue PrescriptionDose;
        private Structure BodyStructure;
        DVHData bodydvhDataVolCC;

        int ProtocolSelectedIndex = -1;

        public DataTable dataTable;
        public enum DataTableContents { DTC_NONE = -1, DTC_RTOG1005 = 0, DTC_NHSFF = 1, DTC_FAST = 2, DTC_EuropaWBI15 =3, DTC_EuropaWBI5 = 4, DTC_EuropaPBI =5 };
        public DataTableContents dataTable_dtc;
        public enum DataTableStatus { DTS_NOTABLE = 0, DTS_NOUSER = 1, DTS_BASICUSER = 2, DTS_TRUEBELIEVER = 3 };
        public DataTableStatus dataTable_dts;
        public enum DataInspectorPolicy { DIP_NONE = -1, DIP_RESTRICTUSER = 0, DIP_ALLTB = 1};
        public DataInspectorPolicy dataTable_dip;
        public double DataInspectorRestrictUserPercentage;
        public DataRow[] dataTable_filteredRows; // this contains filtered records. If policy restricts to current user, dataTable_filteredRows = dataTable_filteredRows_CurrentUser, otherwise dataTable_filteredRows will contain ALL users
        public DataRow[] dataTable_filteredRows_CurrentUser; // filtered records restricted to current user

        public bool AnonymizePatientData;

        // Contains TotalMUs for each Plan/PlanSum in DB
        List<double> planBeamsTotalMUs;
        public double PlanMUMetric_MUSum;
        public double PlanMUMetric_AvgMUPerBeam;

        private double DVHGetVolumeAtDose(DVHData dvhData, DoseValue dv)
        {
            double vol = 0;
            int i;

            for (i = 0; i < dvhData.CurveData.Count(); i++)
            {
                DoseValue idv = dvhData.CurveData[i].DoseValue;
                double iDose_Gy = (idv.Unit == DoseValue.DoseUnit.Gy ? idv.Dose : idv.Dose * 100);
                if (iDose_Gy >= dv.Dose)
                {
                    vol = dvhData.CurveData[i].Volume;
                    break;
                }
            }
            return vol;
        }

        private double DVHGetDoseAtVolumeCC(DVHData dvhDataVolCC, double Volume_incc)
        {
            double dose = 0;
            int i;

            for (i = 0; i < dvhDataVolCC.CurveData.Count(); i++)
            {
                DoseValue idv = dvhDataVolCC.CurveData[i].DoseValue;
                double iDose_Gy = idv.Unit == DoseValue.DoseUnit.Gy ? idv.Dose : idv.Dose * 100;

                if (dvhDataVolCC.CurveData[i].Volume <= Volume_incc)
                {
                    dose = iDose_Gy;
                    break;
                }
            }
            return dose;
        }

        private double DVHGetDoseAtVolume(DVHData dvhData, double Volume_inperc)
        {
            double dose = 0;
            int i;

            for (i = 0; i < dvhData.CurveData.Count(); i++)
            {
                DoseValue idv = dvhData.CurveData[i].DoseValue;
                double iDose_Gy = idv.Unit == DoseValue.DoseUnit.Gy ? idv.Dose : idv.Dose * 100;

                if (dvhData.CurveData[i].Volume <= Volume_inperc)
                {
                    dose = iDose_Gy;
                    break;
                }
            }
            return dose;
        }

        public MainWindow(Patient pat, PlanSetup planSetup, PlanSum planSum, StructureSet structureSet, string UserName, string ExportPath, string DataInspectorPolicy, double DataInspectorRestrictUserPercentage, bool AnonymizePatientData, ScoreStyle DefaultScoreStyle)
        {
            this.pat = pat;
            this.structureSet = structureSet;
            this.planSetup = planSetup;
            this.planSum = planSum;
            this.userName = UserName;

            InitializeComponent();

            dataTable = new DataTable();
            dataTable_dtc = DataTableContents.DTC_NONE;
            dataTable_dts = DataTableStatus.DTS_NOTABLE;

            planBeamsTotalMUs = new List<double>();
            PlanMUMetric_MUSum = 0;
            PlanMUMetric_AvgMUPerBeam = 0;

            PscHyperLink.ToolTip = "Click to perform basic PLAN sanity check v2.0:\n" +
                                    " - number of isocenters == 1\n" +
                                    " - setup fields with couch rtn <> 0\n" +
                                    " - VMAT beams with jaws rtn = 0\n" +
                                    " - VMAT beams with couch rtn <> 0\n" +
                                    " - VMAT beams with DR < 600 MU/min\n" +
                                    " - VMAT/IMRT beams with X > 14.5 cm\n" +
                                    " - static wedged beams with DR < 600 MU/min\n" +
                                    " - MU Sum within expected range";


            Title = pat.Name + " - " + Title;

            UserNameLabel.Text = "user = " + UserName;

            PatientNameID.Text = pat.FirstName + " " + pat.LastName + " (" + pat.Id + ";" + pat.Id2 + ")";

            ExportDrawer.Text = ExportPath;

            this.AnonymizePatientData = AnonymizePatientData;

            if (DefaultScoreStyle == BreastWatch.ScoreStyle.Protocol)
                ScoreStyleComboBox.SelectedIndex = 0;
            else if (DefaultScoreStyle == BreastWatch.ScoreStyle.Community)
                ScoreStyleComboBox.SelectedIndex = 1;
            if (DefaultScoreStyle == BreastWatch.ScoreStyle.Gradient)
                ScoreStyleComboBox.SelectedIndex = 2;

            this.DataInspectorRestrictUserPercentage = DataInspectorRestrictUserPercentage;
            if (DataInspectorPolicy == "RESTRICTUSER")
            {
                dataTable_dip = MainWindow.DataInspectorPolicy.DIP_RESTRICTUSER;
                Table.ToolTip += "\n(applying a " + DataInspectorRestrictUserPercentage.ToString("F1") + "% RESTRICTUSER cut off)";
            }
            else if (DataInspectorPolicy == "ALLTB")
                dataTable_dip = MainWindow.DataInspectorPolicy.DIP_ALLTB;
            else
                dataTable_dip = MainWindow.DataInspectorPolicy.DIP_NONE;

            DoseValue PrescriptionDose = new DoseValue(0, DoseValue.DoseUnit.Gy);
            if (planSetup != null)
            {
                PlanId.Text = planSetup.Id;
                if (planSetup.Beams.Count() == 0) // Dose plan
                    TXUnitId.Text = "NA";
                else
                    TXUnitId.Text = planSetup.Beams.First().TreatmentUnit.Id;

                Prescription.Text = planSetup.TotalDose.ToString() + " / " + planSetup.NumberOfFractions + " FR";
                if (planSetup.NumberOfFractions > 1)
                    Prescription.Text += "s";
                Target.Text = planSetup.TargetVolumeID;
                if(planSetup.TotalDose.Unit == DoseValue.DoseUnit.Gy)
                    PrescriptionDose = planSetup.TotalDose;
                else
                    PrescriptionDose = new DoseValue(planSetup.TotalDose.Dose * 100,DoseValue.DoseUnit.Gy);
            }
            else if (planSum != null)
            {
                PscHyperLink.Inlines.Clear();
                PscHyperLink.Inlines.Add("PLAN SUM");
                PlanId.Text = planSum.Id;
                if (planSum.PlanSetups.First().Beams.Count() == 0) // Dose plan
                    TXUnitId.Text = "NA";
                else
                    TXUnitId.Text = planSum.PlanSetups.First().Beams.First().TreatmentUnit.Id;
                Prescription.Text = "SUM = ";
                Target.Text = "";
                string fractionsString = " / ";
                foreach (PlanSetup plansetupinsum in planSum.PlanSetups)
                {
                    PrescriptionDose += plansetupinsum.TotalDose;
                    if (plansetupinsum == planSum.PlanSetups.Last())
                    {
                        Prescription.Text += plansetupinsum.TotalDose.ToString();
                        Target.Text += plansetupinsum.TargetVolumeID;
                        fractionsString += plansetupinsum.NumberOfFractions + " FRs";
                    }
                    else
                    {
                        Prescription.Text += plansetupinsum.TotalDose.ToString() + " + ";
                        Target.Text += plansetupinsum.TargetVolumeID + " / ";
                        fractionsString += plansetupinsum.NumberOfFractions + "+";
                    }
                }
                Prescription.Text += fractionsString;
            }
            this.PrescriptionDose = PrescriptionDose;

            PreparePlanDataString();

            // Find Body structure
            bodydvhDataVolCC = null;
            BodyStructure = null;
            foreach (var structure in structureSet.Structures)
            {
                if (structure.DicomType == "EXTERNAL") //if (structure.DicomType == "BODY" || structure.DicomType == "External" || structure.DicomType.Contains("Ext") || structure.Id.Contains("BODY") || structure.Id.Contains("body") || structure.Id.Contains("Ext") || structure.Id.Contains("ext"))
                {
                    if (planSetup != null)
                        bodydvhDataVolCC = planSetup.GetDVHCumulativeData(structure,
                                          DoseValuePresentation.Absolute,
                                          VolumePresentation.AbsoluteCm3, 0.1);
                    else if (planSum != null)
                        bodydvhDataVolCC = planSum.GetDVHCumulativeData(structure,
                                          DoseValuePresentation.Absolute,
                                          VolumePresentation.AbsoluteCm3, 0.1);
                    BodyStructure = structure;
                    break;
                }
            }
            if (BodyStructure == null)
            {
                string message = "Cannot find Body structure.\nSome data will not be available.";
                MessageBox.Show(message, "Breast Watch", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (bodydvhDataVolCC != null)
                BodyDmax.Text = bodydvhDataVolCC.MaxDose.ToString() + " (" + (bodydvhDataVolCC.MaxDose * 100 / PrescriptionDose).ToString("F1") + "%)";

            DateTime? birth = pat.DateOfBirth;
            if (birth != null)
            {
                DateTime tbirth = (DateTime)birth;
                DateTime today = DateTime.Today;
                TimeSpan tspat = (TimeSpan)(today - tbirth);
                int age_years = tspat.Days;
                age_years /= 365;

                PatientBirthAge.Text = tbirth.ToShortDateString() + " (" + age_years + ")";
            }
            else
            {
                PatientBirthAge.Text = "NA";
            }

            if (planSetup != null)
            {
                if (planSetup.Name.Contains("PAR") || planSetup.Name.Contains("Par") || planSetup.Name.Contains("par") || planSetup.Name.Contains("CW") || planSetup.Name.Contains("cw") || planSetup.Name.Contains("Chest") || planSetup.Name.Contains("chest"))
                {
                    ChestWallCheckBox.IsChecked = true;
                    ExpanderCheckBox.IsEnabled = true;
                    ExpanderHSCheckBox.IsEnabled = true;
                    OptsExpander.Header = "opts*";
                }
            }
            else if (planSum != null)
            {
                if (planSum.PlanSetups.First().Name.Contains("PAR") || planSum.PlanSetups.First().Name.Contains("Par") || planSum.PlanSetups.First().Name.Contains("par") || planSum.PlanSetups.First().Name.Contains("CW") || planSum.PlanSetups.First().Name.Contains("cw") || planSum.PlanSetups.First().Name.Contains("Chest") || planSum.PlanSetups.First().Name.Contains("chest"))
                {
                    ChestWallCheckBox.IsChecked = true;
                    ExpanderCheckBox.IsEnabled = true;
                    ExpanderHSCheckBox.IsEnabled = true;
                    OptsExpander.Header = "opts*";
                }
            }

            var TargetBreastDataGridView = new GridView();
            TargetBreastDataListView.View = TargetBreastDataGridView;
            TargetBreastDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Data",
                DisplayMemberBinding = new Binding("Data")
            });
            TargetBreastDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Const",
                DisplayMemberBinding = new Binding("Const")
            });
            TargetBreastDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Results",
                DisplayMemberBinding = new Binding("Results")
            });
            TargetBreastDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "%PD",
                DisplayMemberBinding = new Binding("ResultsPerc")
            });
            TargetBreastDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "CB",
                DisplayMemberBinding = new Binding("CommunityScoreString")
            });

            // get default from ScoreStyle Tag
            StyleSelector myListViewItemStyleSelector;
            if (ScoreStyleComboBox.SelectedIndex == 1)
                myListViewItemStyleSelector = new mListViewItemCommunityStyleSelector();
            else if (ScoreStyleComboBox.SelectedIndex == 2)
                myListViewItemStyleSelector = new mListViewItemCommunityGradientStyleSelector();
            else
                myListViewItemStyleSelector = new mListViewItemProtocolStyleSelector();

            TargetBreastDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;

            var TargetBoostDataGridView = new GridView();
            TargetBoostDataListView.View = TargetBoostDataGridView;
            TargetBoostDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Data",
                DisplayMemberBinding = new Binding("Data")
            });
            TargetBoostDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Const",
                DisplayMemberBinding = new Binding("Const")
            });
            TargetBoostDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Results",
                DisplayMemberBinding = new Binding("Results")
            });
            TargetBoostDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "%PD",
                DisplayMemberBinding = new Binding("ResultsPerc")
            });
            TargetBoostDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "CB",
                DisplayMemberBinding = new Binding("CommunityScoreString")
            });

            TargetBoostDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;

            var TargetSPCDataGridView = new GridView();
            TargetSPCDataListView.View = TargetSPCDataGridView;
            TargetSPCDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Data",
                DisplayMemberBinding = new Binding("Data")
            });
            TargetSPCDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Const",
                DisplayMemberBinding = new Binding("Const")
            });
            TargetSPCDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Results",
                DisplayMemberBinding = new Binding("Results")
            });
            TargetSPCDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "%PD",
                DisplayMemberBinding = new Binding("ResultsPerc")
            });
            TargetSPCDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "CB",
                DisplayMemberBinding = new Binding("CommunityScoreString")
            });

            TargetSPCDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;

            var IpsiLungDataGridView = new GridView();
            IpsiLungDataListView.View = IpsiLungDataGridView;
            IpsiLungDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Data",
                DisplayMemberBinding = new Binding("Data")
            });
            IpsiLungDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Const",
                DisplayMemberBinding = new Binding("Const")
            });
            IpsiLungDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Results",
                DisplayMemberBinding = new Binding("Results")
            });
            IpsiLungDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "CB",
                DisplayMemberBinding = new Binding("CommunityScoreString")
            });

            IpsiLungDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;

            var ContraLungDataGridView = new GridView();
            ContraLungDataListView.View = ContraLungDataGridView;
            ContraLungDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Data",
                DisplayMemberBinding = new Binding("Data")
            });
            ContraLungDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Const",
                DisplayMemberBinding = new Binding("Const")
                
            });
            ContraLungDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Results",
                DisplayMemberBinding = new Binding("Results")
            });
            ContraLungDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "CB",
                DisplayMemberBinding = new Binding("CommunityScoreString")
            });

            ContraLungDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;

            var HeartDataGridView = new GridView();
            HeartDataListView.View = HeartDataGridView;
            HeartDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Data",
                DisplayMemberBinding = new Binding("Data")
            });
            HeartDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Const",
                DisplayMemberBinding = new Binding("Const")
            });
            HeartDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Results",
                DisplayMemberBinding = new Binding("Results")
            });
            HeartDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "CB",
                DisplayMemberBinding = new Binding("CommunityScoreString")
            });

            HeartDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;

            var LeftCoronaryDataGridView = new GridView();
            LeftCoronaryDataListView.View = LeftCoronaryDataGridView;
            LeftCoronaryDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Data",
                DisplayMemberBinding = new Binding("Data")
            });
            LeftCoronaryDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Const",
                DisplayMemberBinding = new Binding("Const")
            });
            LeftCoronaryDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Results",
                DisplayMemberBinding = new Binding("Results")
            });
            LeftCoronaryDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "CB",
                DisplayMemberBinding = new Binding("CommunityScoreString")
            });

            LeftCoronaryDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;

            var ContraBreastDataGridView = new GridView();
            ContraBreastDataListView.View = ContraBreastDataGridView;
            ContraBreastDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Data",
                DisplayMemberBinding = new Binding("Data")
            });
            ContraBreastDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Const",
                DisplayMemberBinding = new Binding("Const")
            });
            ContraBreastDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Results",
                DisplayMemberBinding = new Binding("Results")
            });
            ContraBreastDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "CB",
                DisplayMemberBinding = new Binding("CommunityScoreString")
            });

            ContraBreastDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;

            var ThyroidDataGridView = new GridView();
            ThyroidDataListView.View = ThyroidDataGridView;
            ThyroidDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Data",
                DisplayMemberBinding = new Binding("Data")
            });
            ThyroidDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Const",
                DisplayMemberBinding = new Binding("Const")
            });
            ThyroidDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Results",
                DisplayMemberBinding = new Binding("Results")
            });
            ThyroidDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "CB",
                DisplayMemberBinding = new Binding("CommunityScoreString")
            });

            ThyroidDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;

            var EsophagusDataGridView = new GridView();
            EsophagusDataListView.View = EsophagusDataGridView;
            EsophagusDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Data",
                DisplayMemberBinding = new Binding("Data")
            });
            EsophagusDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Const",
                DisplayMemberBinding = new Binding("Const")
            });
            EsophagusDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Results",
                DisplayMemberBinding = new Binding("Results")
            });
            EsophagusDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "CB",
                DisplayMemberBinding = new Binding("CommunityScoreString")
            });

            EsophagusDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;

            var LiverDataGridView = new GridView();
            LiverDataListView.View = LiverDataGridView;
            LiverDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Data",
                DisplayMemberBinding = new Binding("Data")
            });
            LiverDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Const",
                DisplayMemberBinding = new Binding("Const")
            });
            LiverDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Results",
                DisplayMemberBinding = new Binding("Results")
            });
            LiverDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "CB",
                DisplayMemberBinding = new Binding("CommunityScoreString")
            });

            LiverDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;

            var StomachDataGridView = new GridView();
            StomachDataListView.View = StomachDataGridView;
            StomachDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Data",
                DisplayMemberBinding = new Binding("Data")
            });
            StomachDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Const",
                DisplayMemberBinding = new Binding("Const")
            });
            StomachDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Results",
                DisplayMemberBinding = new Binding("Results")
            });
            StomachDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "CB",
                DisplayMemberBinding = new Binding("CommunityScoreString")
            });

            StomachDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;

            var SkinDataGridView = new GridView();
            SkinDataListView.View = SkinDataGridView;
            SkinDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Data",
                DisplayMemberBinding = new Binding("Data")
            });
            SkinDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Const",
                DisplayMemberBinding = new Binding("Const")
            });
            SkinDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "Results",
                DisplayMemberBinding = new Binding("Results")
            });
            SkinDataGridView.Columns.Add(new GridViewColumn
            {
                Header = "CB",
                DisplayMemberBinding = new Binding("CommunityScoreString")
            });

            SkinDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;
        }

        private void PreparePlanDataString()
        {
            if (planSetup != null)
            {
                int i = 1;
                PlanDataString += "<PLAN>";
                foreach (Beam iBeam in planSetup.Beams)
                {
                    if (iBeam.IsSetupField == false)
                    {
                        PlanDataString += "<B>";
                        PlanDataString += "<MU>" + iBeam.Meterset.Value.ToString("F1") + "</MU>";
                        PlanDataString += "</B>";
                        i++;
                    }
                }
                PlanDataString += "</PLAN>";
            }
            else if (planSum != null)
            {
                int i = 1, j = 1;
                PlanDataString += "<PLANSUM>";
                foreach (PlanSetup iPlan in planSum.PlanSetups)
                {
                    PlanDataString += "<PLAN>";
                    foreach (Beam iBeam in iPlan.Beams)
                    {
                        if (iBeam.IsSetupField == false)
                        {
                            PlanDataString += "<B>";
                            PlanDataString += "<MU>" + iBeam.Meterset.Value.ToString("F1") + "</MU>";
                            PlanDataString += "</B>";
                            i++;
                        }
                    }
                    j++;
                    PlanDataString += "</PLAN>";
                }
                PlanDataString += "</PLANSUM>";
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(ExportDrawer.Text) == false)
            {
                string message = "Export drawer does not exist or it's not accessible.\nPlease specify a valid Export Drawer.";
                MessageBox.Show(message, "Breast Watch", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                FileStream fs = File.OpenWrite(ExportDrawer.Text + '\\' + ExportFileName.Text);
                fs.Close();
            }
            catch (Exception)
            {
                string message = "Export file is currently locked. Please retry later.";
                MessageBox.Show(message, "Breast Watch", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (BoostCheckBox.IsChecked == true)
            {
                if (TargetBoostList.SelectedIndex == -1)
                {
                    string message = "Please select a boost PTV before saving data.";
                    MessageBox.Show(message, "Breast Watch", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (SPCNCheckBox.IsChecked == true)
            {
                if (TargetSPCList.SelectedIndex == -1)
                {
                    string message = "Please select a SPC/N PTV before saving data.";
                    MessageBox.Show(message, "Breast Watch", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }


            if (Technique3DCRT.IsChecked == false && TechniqueIMRT.IsChecked == false && TechniqueVMAT.IsChecked == false) // this can happen for doseplans
            {
                string message = "Please specify treatment technique (3DCRT, IMRT or VMAT) before saving data.";
                MessageBox.Show(message, "Breast Watch", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (ProtocolComboBox.SelectedIndex == 0) // RTOG1005
            {
                if (TargetBreastList.SelectedIndex != -1 && IpsiLungList.SelectedIndex != -1 && HeartList.SelectedIndex != -1)
                {
                    using (StreamWriter sw = File.AppendText(ExportDrawer.Text + '\\' + ExportFileName.Text))
                    {
                        if (WriteHeadersCheckBox.IsChecked == true)
                        {
                            string HeaderString = PrepareHeaderString();
                            sw.WriteLine(HeaderString);
                        }
                        string DataString = PrepareDataString();
                        sw.WriteLine(DataString);
                    }
                }
                else
                {
                    string message = "Please select at minimum a breast PTV, ispilateral lung and heart organs before saving data.";
                    MessageBox.Show(message, "Breast Watch", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else if (ProtocolComboBox.SelectedIndex == 1) // NHSFF
            {
                if (TargetBreastList.SelectedIndex != -1 && IpsiLungList.SelectedIndex != -1 && HeartList.SelectedIndex != -1)
                {
                    using (StreamWriter sw = File.AppendText(ExportDrawer.Text + '\\' + ExportFileName.Text))
                    {
                        if (WriteHeadersCheckBox.IsChecked == true)
                        {
                            string HeaderString = PrepareHeaderString();
                            sw.WriteLine(HeaderString);
                        }
                        string DataString = PrepareDataString();
                        sw.WriteLine(DataString);
                    }
                }
                else
                {
                    string message = "Please select at minimum a breast PTV, ispilateral lung and heart organs before saving data.";
                    MessageBox.Show(message, "Breast Watch", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else if (ProtocolComboBox.SelectedIndex == 2) // START
            {
                if (TargetBreastList.SelectedIndex != -1 && IpsiLungList.SelectedIndex != -1 && HeartList.SelectedIndex != -1)
                {
                    using (StreamWriter sw = File.AppendText(ExportDrawer.Text + '\\' + ExportFileName.Text))
                    {
                        if (WriteHeadersCheckBox.IsChecked == true)
                        {
                            string HeaderString = PrepareHeaderString();
                            sw.WriteLine(HeaderString);
                        }
                        string DataString = PrepareDataString();
                        sw.WriteLine(DataString);
                    }
                }
                else
                {
                    string message = "Please select at minimum a breast PTV, ispilateral lung and heart organs before saving data.";
                    MessageBox.Show(message, "Breast Watch", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else if (ProtocolComboBox.SelectedIndex == 3) // EUROPAWBI15
            {
                if (TargetBreastList.SelectedIndex != -1 && IpsiLungList.SelectedIndex != -1 && HeartList.SelectedIndex != -1)
                {
                    using (StreamWriter sw = File.AppendText(ExportDrawer.Text + '\\' + ExportFileName.Text))
                    {
                        if (WriteHeadersCheckBox.IsChecked == true)
                        {
                            string HeaderString = PrepareHeaderString();
                            sw.WriteLine(HeaderString);
                        }
                        string DataString = PrepareDataString();
                        sw.WriteLine(DataString);
                    }
                }
                else
                {
                    string message = "Please select at minimum a breast PTV, ispilateral lung and heart organs before saving data.";
                    MessageBox.Show(message, "Breast Watch", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else if (ProtocolComboBox.SelectedIndex == 4) // EUROPAWBI5
            {
                if (TargetBreastList.SelectedIndex != -1 && IpsiLungList.SelectedIndex != -1 && HeartList.SelectedIndex != -1)
                {
                    using (StreamWriter sw = File.AppendText(ExportDrawer.Text + '\\' + ExportFileName.Text))
                    {
                        if (WriteHeadersCheckBox.IsChecked == true)
                        {
                            string HeaderString = PrepareHeaderString();
                            sw.WriteLine(HeaderString);
                        }
                        string DataString = PrepareDataString();
                        sw.WriteLine(DataString);
                    }
                }
                else
                {
                    string message = "Please select at minimum a breast PTV, ispilateral lung and heart organs before saving data.";
                    MessageBox.Show(message, "Breast Watch", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else if (ProtocolComboBox.SelectedIndex == 5) // EUROPAPBI5
            {
                if (TargetBreastList.SelectedIndex != -1 && TargetBoostList.SelectedIndex != -1 && IpsiLungList.SelectedIndex != -1 && HeartList.SelectedIndex != -1)
                {
                    using (StreamWriter sw = File.AppendText(ExportDrawer.Text + '\\' + ExportFileName.Text))
                    {
                        if (WriteHeadersCheckBox.IsChecked == true)
                        {
                            string HeaderString = PrepareHeaderString();
                            sw.WriteLine(HeaderString);
                        }
                        string DataString = PrepareDataString();
                        sw.WriteLine(DataString);
                    }
                }
                else
                {
                    string message = "Please select at minimum a PBI PTV, ispilateral breast, ispilateral lung and heart organs before saving data.";
                    MessageBox.Show(message, "Breast Watch", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            this.DialogResult = true;
        }

        // 18.10.24 added CB Score
        private void ComputeScores()
        {
            int numTotal = 0;
            int numCBTotal = 0;
            double bwScore = -1;
            double cbScore = -1;

            MyItem iMyItem;
            if (TargetBreastList.SelectedIndex != -1)
            {
                foreach (var iItem in TargetBreastDataListView.Items)
                {
                    iMyItem = (MyItem)iItem;
                    bwScore += iMyItem.Score;
                    if (iMyItem.CommunityScore != -1)
                    { 
                        cbScore += iMyItem.CommunityScore;
                        numCBTotal++;
                    }
                    numTotal++;
                }
            }
            if (TargetBoostList.SelectedIndex != -1)
            {
                foreach (var iItem in TargetBoostDataListView.Items)
                {
                    iMyItem = (MyItem)iItem;
                    bwScore += iMyItem.Score;
                    if (iMyItem.CommunityScore != -1)
                    {
                        cbScore += iMyItem.CommunityScore;
                        numCBTotal++;
                    }
                    numTotal++;
                }
            }
            if (TargetSPCList.SelectedIndex != -1)
            {

                foreach (var iItem in TargetSPCDataListView.Items)
                {
                    iMyItem = (MyItem)iItem;
                    bwScore += iMyItem.Score;
                    if (iMyItem.CommunityScore != -1)
                    {
                        cbScore += iMyItem.CommunityScore;
                        numCBTotal++;
                    }
                    numTotal++;
                }
            }
            if (IpsiLungList.SelectedIndex != -1)
            {
                foreach (var iItem in IpsiLungDataListView.Items)
                {
                    iMyItem = (MyItem)iItem;
                    bwScore += iMyItem.Score;
                    if (iMyItem.CommunityScore != -1)
                    {
                        cbScore += iMyItem.CommunityScore;
                        numCBTotal++;
                    }
                    numTotal++;
                }
            }
            if (ContraLungList.SelectedIndex != -1)
            {
                foreach (var iItem in ContraLungDataListView.Items)
                {
                    iMyItem = (MyItem)iItem;
                    bwScore += iMyItem.Score;
                    if (iMyItem.CommunityScore != -1)
                    {
                        cbScore += iMyItem.CommunityScore;
                        numCBTotal++;
                    }
                    numTotal++;
                }
            }
            if (HeartList.SelectedIndex != -1)
            {
                foreach (var iItem in HeartDataListView.Items)
                {
                    iMyItem = (MyItem)iItem;
                    bwScore += iMyItem.Score;
                    if (iMyItem.CommunityScore != -1)
                    {
                        cbScore += iMyItem.CommunityScore;
                        numCBTotal++;
                    }
                    numTotal++;
                }
            }
            if (LeftCoronaryList.SelectedIndex != -1)
            {
                foreach (var iItem in LeftCoronaryDataListView.Items)
                {
                    iMyItem = (MyItem)iItem;
                    bwScore += iMyItem.Score;
                    if (iMyItem.CommunityScore != -1)
                    {
                        cbScore += iMyItem.CommunityScore;
                        numCBTotal++;
                    }
                    numTotal++;
                }
            }
            if (ContraBreastList.SelectedIndex != -1)
            {
                foreach (var iItem in ContraBreastDataListView.Items)
                {
                    iMyItem = (MyItem)iItem;
                    bwScore += iMyItem.Score;
                    if (iMyItem.CommunityScore != -1)
                    {
                        cbScore += iMyItem.CommunityScore;
                        numCBTotal++;
                    }
                    numTotal++;
                }
            }

            if (bwScore != -1)
            {
                bwScore += +1;
                
                bwScore *= 100;
                bwScore /= numTotal;
                PlanScore.Value = bwScore;

                PlanScore.IsIndeterminate = false;
                if (bwScore < 60)
                    PlanScore.Foreground = Brushes.Red;
                else
                    PlanScore.Foreground = Brushes.LimeGreen;
            }
            else
            {
                PlanScore.IsIndeterminate = true;
                PlanScore.Foreground = Brushes.Blue;
            }

            if (cbScore != -1)
            {
                cbScore += +1;

                cbScore /= numCBTotal;
                CBPlanScore.Value = cbScore;

                uint bwRGB = ScorePalette.GetPaletteRGB(cbScore);
                if (bwRGB == 0)
                {
                    CBPlanScore.IsIndeterminate = true;
                    CBPlanScore.Foreground = Brushes.Blue;
                }
                else
                {
                    CBPlanScore.IsIndeterminate = false;
                    CBPlanScore.Foreground = new SolidColorBrush(Color.FromRgb((byte)((bwRGB >> 16) & 0xFF), (byte)((bwRGB >> 8) & 0xFF), (byte)(bwRGB & 0xFF)));
                }
            }
            else
            {
                CBPlanScore.Value = 0;
                CBPlanScore.IsIndeterminate = true;
                CBPlanScore.Foreground = Brushes.Blue;
            }
        }

        public void PrepareProtocolTooltip()
        {
            string HeaderString = PrepareHeaderString();
            string FormattedHeaderString = HeaderString.Replace('\t', '\n');
            string BannerString = PrepareBannerString();
            ProtocolLabel.ToolTip = BannerString + '\n' + "-------------------------------" + '\n' + FormattedHeaderString;
        }

        private string PrepareBannerString()
        {
            string bString = "PROTOCOL ";

            if (ProtocolComboBox.SelectedIndex == 0) //RTOG1005
            {
                bString += "RTOG1005 ARM II - 15 Fractions" + '\n' + "PTV BREAST 2.67 Gy/FR (40.05 Gy)" + '\n' + "PTV BOOST SIB 3.2 Gy/FR (48 Gy)" + '\n' + "SPC = NO (OPT)" + '\n' + "TECHNIQUE = 3DCRT/IMRT";
            }
            else if (ProtocolComboBox.SelectedIndex == 1) //NHSFF
            {
                bString += "FAST FORWARD ARM 2 - 5 Fractions" + '\n' + "PTV BREAST 5.2 Gy/FR (26 Gy)" + '\n' + "PTV BOOST SIB 6.0 Gy/FR (30 Gy)" + '\n' + "SPC = NO" + '\n' + "TECHNIQUE = 3DCRT";
            }
            else if (ProtocolComboBox.SelectedIndex == 2) //FAST
            {
                bString += "FAST - 5 Fractions" + '\n' + "PTV BREAST 5.7 Gy/FR (28.5 Gy)" + '\n' + "PTV BOOST SIB 6.2 Gy/FR (31 Gy)" + '\n' + "SPC = NO (OPT)" + '\n' + "TECHNIQUE = ANY";
            }
            else if (ProtocolComboBox.SelectedIndex == 3) //EUROPA v2.1 WBI 40.05 Gy
            {
                bString += "EUROPA v2.1 WBI - 15 Fractions" + '\n' + "PTV BREAST 2.67 Gy/FR (40.05 Gy)" + '\n' + "PTV BOOST = NO " + '\n' + "SPC = NO" + '\n' + "TECHNIQUE = 3DCRT/IMRT";
            }
            else if (ProtocolComboBox.SelectedIndex == 4) //EUROPA v2.1 WBI 26 Gy
            {
                bString += "EUROPA v2.1 WBI - 5 Fractions" + '\n' + "PTV BREAST 5.2 Gy/FR (26 Gy)" + '\n' + "PTV BOOST = NO " + '\n' + "SPC = NO" + '\n' + "TECHNIQUE = 3DCRT/IMRT";
            }
            else if (ProtocolComboBox.SelectedIndex == 5) //EUROPA v2.1 PBI 30 Gy
            {
                bString += "EUROPA v2.1 PBI - 5 Fractions" + '\n' + "BREAST = OAR" + '\n' + "PTV BOOST 6 Gy/FR (30 Gy)" + '\n' + "SPC = NO" + '\n' + "TECHNIQUE = 3DCRT/IMRT";
            }
            return bString;
        }

        private string PrepareHeaderString()
        {
            string fString = "SAVE_DATE" + '\t' + "PAT_NAME" + '\t' + "PAT_ID" + '\t' + "PAT_ID2" + '\t' + "DATE_OF_BIRTH" + '\t' + "PROTOCOL" + '\t' + "PROTOCOL_PAT_ID" + '\t' + "LATERALITY" + '\t' + "TECHNIQUE" + '\t' + "MACHINE" + '\t' + "BOLUS" + '\t' + "PLAN_NAME" + '\t' + "PLAN_BEAMS" + '\t' + "OPTIONS" + '\t' + "wADDTARGETS" + '\t';

            if (ProtocolComboBox.SelectedIndex == 0) // RTOG1005 V1.2
            {
                fString += "BREAST_VOL" + '\t' + "BREAST_C38.05" + '\t' + "BREAST_C36.05" + '\t' + "BREAST_V43.2" + '\t' + "BREAST_V48" + '\t' + "BREAST_Max" + '\t' + "BREAST_Avg" + '\t' + "BREAST_R38.05" + '\t';
                fString += "BOOST_VOL" + '\t' + "BOOST_C45.6" + '\t' + "BOOST_C43.2" + '\t' + "BOOST_V52.8" + '\t' + "BOOST_Max" + '\t' + "BOOST_Avg" + '\t';
                fString += "SPCN_VOL" + '\t' + "SPCN_C38.05" + '\t' + "SPCN_Max" + '\t' + "SPCN_Avg" + '\t';
                fString += "IPSI-LUNG_V16" + '\t' + "IPSI-LUNG_V8" + '\t' + "IPSI-LUNG_V4" + '\t' + "IPSI-LUNG_V12" + '\t' + "IPSI-LUNG_Avg" + '\t';
                fString += "CONTRA-LUNG_V4" + '\t' + "CONTRA-LUNG_V2.5" + '\t' + "CONTRA-LUNG_Avg" + '\t';
                fString += "HEART_V20" + '\t' + "HEART_V16" + '\t' + "HEART_V8" + '\t' + "HEART_Max" + '\t' + "HEART_V10" + '\t' + "HEART_V2" + '\t' + "HEART_Avg" + '\t';
                fString += "LEFT-COR_Max" + '\t' + "LEFT-COR_Avg" + '\t';
                fString += "CONTRA-BREAST_V4" + '\t' + "CONTRA-BREAST_V2.4" + '\t' + "CONTRA-BREAST_Max" + '\t' + "CONTRA-BREAST_Avg" + '\t';
                fString += "THYROID_V24" + '\t' + "THYROID_Max" + '\t' + "THYROID_Avg" + '\t';
                fString += "ESOPHAGUS_V20" + '\t' + "ESOPHAGUS_Max" + '\t' + "ESOPHAGUS_Avg" + '\t';
                fString += "LIVER_V20" + '\t' + "LIVER_Max" + '\t' + "LIVER_Avg" + '\t';
                fString += "STOMACH_V20" + '\t' + "STOMACH_Max" + '\t' + "STOMACH_Avg" + '\t';
            }
            else if (ProtocolComboBox.SelectedIndex == 1) // NHSFF
            {
                fString += "BREAST_VOL" + '\t' + "BREAST_C24.7" + '\t' + "BREAST_V27.3" + '\t' + "BREAST_V27.82" + '\t' + "BREAST_Max" + '\t' + "BREAST_Avg" + '\t' + "BREAST_R24.7" + '\t';
                fString += "BOOST_VOL" + '\t' + "BOOST_C28.5" + '\t' + "BOOST_V31.5" + '\t' + "BOOST_Max" + '\t' + "BOOST_Avg" + '\t';
                fString += "SPCN_VOL" + '\t' + "SPCN_C24.7" + '\t' + "SPCN_Max" + '\t' + "SPCN_Avg" + '\t';
                fString += "IPSI-LUNG_V8" + '\t' + "IPSI-LUNG_V5" + '\t' + "IPSI-LUNG_V3" + '\t' + "IPSI-LUNG_Avg" + '\t';
                fString += "CONTRA-LUNG_V3" + '\t' + "CONTRA-LUNG_Avg" + '\t';
                fString += "HEART_V10" + '\t' + "HEART_V7" + '\t' + "HEART_V1.5" + '\t' + "HEART_Max" + '\t' + "HEART_Avg" + '\t';
                fString += "LEFT-COR_Max" + '\t' + "LEFT-COR_Avg" + '\t';
                fString += "CONTRA-BREAST_V3" + '\t' + "CONTRA-BREAST_Max" + '\t' + "CONTRA-BREAST_Avg" + '\t';
                fString += "THYROID_V16" + '\t' + "THYROID_Max" + '\t' + "THYROID_Avg" + '\t';
                fString += "ESOPHAGUS_V13" + '\t' + "ESOPHAGUS_Max" + '\t' + "ESOPHAGUS_Avg" + '\t';
                fString += "LIVER_V13" + '\t' + "LIVER_Max" + '\t' + "LIVER_Avg" + '\t';
                fString += "STOMACH_V13" + '\t' + "STOMACH_Max" + '\t' + "STOMACH_Avg" + '\t';
            }
            else if (ProtocolComboBox.SelectedIndex == 2) // FAST
            {
                fString += "BREAST_VOL" + '\t' + "BREAST_C27.08" + '\t' + "BREAST_V29.93" + '\t' + "BREAST_V30.50" + '\t' + "BREAST_Max" + '\t' + "BREAST_Avg" + '\t' + "BREAST_R27.08" + '\t';
                fString += "BOOST_VOL" + '\t' + "BOOST_C29.45" + '\t' + "BOOST_V32.55" + '\t' + "BOOST_Max" + '\t' + "BOOST_Avg" + '\t';
                fString += "SPCN_VOL" + '\t' + "SPCN_C27.08" + '\t' + "SPCN_Max" + '\t' + "SPCN_Avg" + '\t';
                fString += "IPSI-LUNG_V8" + '\t' + "IPSI-LUNG_V5" + '\t' + "IPSI-LUNG_V3" + '\t' + "IPSI-LUNG_Avg" + '\t';
                fString += "CONTRA-LUNG_V3" + '\t' + "CONTRA-LUNG_Avg" + '\t';
                fString += "HEART_V10" + '\t' + "HEART_V7" + '\t' + "HEART_V1.5" + '\t' + "HEART_Max" + '\t' + "HEART_Avg" + '\t';
                fString += "LEFT-COR_Max" + '\t' + "LEFT-COR_Avg" + '\t';
                fString += "CONTRA-BREAST_V3" + '\t' + "CONTRA-BREAST_Max" + '\t' + "CONTRA-BREAST_Avg" + '\t';
                fString += "THYROID_V16" + '\t' + "THYROID_Max" + '\t' + "THYROID_Avg" + '\t';
                fString += "ESOPHAGUS_V13" + '\t' + "ESOPHAGUS_Max" + '\t' + "ESOPHAGUS_Avg" + '\t';
                fString += "LIVER_V13" + '\t' + "LIVER_Max" + '\t' + "LIVER_Avg" + '\t';
                fString += "STOMACH_V13" + '\t' + "STOMACH_Max" + '\t' + "STOMACH_Avg" + '\t';
            }
            else if (ProtocolComboBox.SelectedIndex == 3) // EUROPA TRIAL WBI 15 FR (RTOG1005)
            {
                fString += "BREAST_VOL" + '\t' + "BREAST_C38.05" + '\t' + "BREAST_C36.05" + '\t' + "BREAST_Max" + '\t' + "BREAST_Avg" + '\t' + "BREAST_R38.05" + '\t';
                fString += "IPSI-LUNG_V18" + '\t' + "IPSI-LUNG_V8" + '\t' + "IPSI-LUNG_V4" + '\t' + "IPSI-LUNG_Avg" + '\t';
                fString += "CONTRA-LUNG_V4" + '\t' + "CONTRA-LUNG_V2.5" + '\t' + "CONTRA-LUNG_Avg" + '\t';
                fString += "HEART_V10" + '\t' + "HEART_V2" + '\t' + "HEART_Max" + '\t' + "HEART_Avg" + '\t';
                fString += "LEFT-COR_Max" + '\t' + "LEFT-COR_Avg" + '\t';
                fString += "CONTRA-BREAST_V4" + '\t' + "CONTRA-BREAST_V2.4" + '\t' + "CONTRA-BREAST_Max" + '\t' + "CONTRA-BREAST_Avg" + '\t';
                fString += "THYROID_V24" + '\t' + "THYROID_Max" + '\t' + "THYROID_Avg" + '\t';
                fString += "ESOPHAGUS_V20" + '\t' + "ESOPHAGUS_Max" + '\t' + "ESOPHAGUS_Avg" + '\t';
                fString += "LIVER_V20" + '\t' + "LIVER_Max" + '\t' + "LIVER_Avg" + '\t';
                fString += "STOMACH_V20" + '\t' + "STOMACH_Max" + '\t' + "STOMACH_Avg" + '\t';
            }
            else if (ProtocolComboBox.SelectedIndex == 4) // EUROPA TRIAL WBI 5 FR (NHSFF)
            {
                fString += "BREAST_VOL" + '\t' + "BREAST_C24.7" + '\t' + "BREAST_V27.3" + '\t' + "BREAST_V27.82" + '\t' + "BREAST_Max" + '\t' + "BREAST_Avg" + '\t' + "BREAST_R24.7" + '\t';
                fString += "IPSI-LUNG_V8" + '\t' + "IPSI-LUNG_V5" + '\t' + "IPSI-LUNG_V3" + '\t' + "IPSI-LUNG_Avg" + '\t';
                fString += "CONTRA-LUNG_V3" + '\t' + "CONTRA-LUNG_Avg" + '\t';
                fString += "HEART_V10" + '\t' + "HEART_V7" + '\t' + "HEART_V1.5" + '\t' + "HEART_Max" + '\t' + "HEART_Avg" + '\t';
                fString += "LEFT-COR_Max" + '\t' + "LEFT-COR_Avg" + '\t';
                fString += "CONTRA-BREAST_V3" + '\t' + "CONTRA-BREAST_Max" + '\t' + "CONTRA-BREAST_Avg" + '\t';
                fString += "THYROID_V16" + '\t' + "THYROID_Max" + '\t' + "THYROID_Avg" + '\t';
                fString += "ESOPHAGUS_V13" + '\t' + "ESOPHAGUS_Max" + '\t' + "ESOPHAGUS_Avg" + '\t';
                fString += "LIVER_V13" + '\t' + "LIVER_Max" + '\t' + "LIVER_Avg" + '\t';
                fString += "STOMACH_V13" + '\t' + "STOMACH_Max" + '\t' + "STOMACH_Avg" + '\t';
            }
            else if (ProtocolComboBox.SelectedIndex == 5) // EUROPA TRIAL PBI 5 FR
            {
                fString += "IPSI-BREAST_VOL" + '\t' + "IPSI-BREAST_V15" + '\t' + "IPSI-BREAST_Avg" + '\t';
                fString += "PBI-PTV_VOL" + '\t' + "PBI-PTV_C28.5" + '\t' + "PBI-PTV_V31.5" + '\t' + "PBI-PTV_Max" + '\t' + "PBI-PTV_Avg" + '\t' + "PBI-PTV_R28.5" + '\t';
                fString += "IPSI-PTV_V24" + '\t' + "IPSI-PTV_V15" + '\t' + "IPSI-PTV_Avg" + '\t';
                fString += "IPSI-LUNG_V10" + '\t' + "IPSI-LUNG_V8" + '\t' + "IPSI-LUNG_V3" + '\t' + "IPSI-LUNG_Avg" + '\t';
                fString += "CONTRA-LUNG_V5" + '\t' + "CONTRA-LUNG_Avg" + '\t';
                fString += "HEART_V3" + '\t' + "HEART_V1.5" + '\t' + "HEART_Max" + '\t' + "HEART_Avg" + '\t';
                fString += "LEFT-COR_Max" + '\t' + "LEFT-COR_Avg" + '\t';
                fString += "CONTRA-BREAST_V3" + '\t' + "CONTRA-BREAST_Max" + '\t' + "CONTRA-BREAST_Avg" + '\t';
                fString += "THYROID_V16" + '\t' + "THYROID_Max" + '\t' + "THYROID_Avg" + '\t';
                fString += "ESOPHAGUS_V13" + '\t' + "ESOPHAGUS_Max" + '\t' + "ESOPHAGUS_Avg" + '\t';
                fString += "LIVER_V13" + '\t' + "LIVER_Max" + '\t' + "LIVER_Avg" + '\t';
                fString += "STOMACH_V13" + '\t' + "STOMACH_Max" + '\t' + "STOMACH_Avg" + '\t';
                fString += "SKIN_Max" + '\t' + "SKIN_D1cc" + '\t' + "SKIN_D10cc" + '\t' + "SKIN_D125cc" + '\t';
            }

            fString += "BWSCORE" + '\t' + "USER" + '\t' + "COMMENTS";

            return fString;
        }

        private string PrepareDataString()
        {
            string fString;

            if (AnonymizePatientData == false)
            {
                fString = DateTime.Today.ToShortDateString() + '\t' + pat.FirstName + " " + pat.LastName + '\t' + pat.Id + '\t' + pat.Id2 + '\t';
                DateTime? birth = pat.DateOfBirth;
                if (birth != null)
                {
                    DateTime tbirth = (DateTime)birth;
                    fString += tbirth.ToShortDateString() + '\t';
                }
                else
                    fString += "NA" + '\t';
            }
            else
            {
                fString = DateTime.Today.ToShortDateString() + '\t' + "ANON" + pat.Id + '\t' + pat.Id + '\t' + pat.Id2 + '\t' + '\t';
            }

            fString += ProtocolComboBox.Text + '\t';
            fString += ProtocolID.Text + '\t';

            if (BilateralCheckBox.IsChecked == true)
                fString += "BIL";
            else
                fString += ((bool)LeftBreastRadio.IsChecked) ? "L" : "R";
            if (PBICheckBox.IsChecked == true)
                fString += "|PBI";
            if (ProneCheckBox.IsChecked == true)
                fString += "|PRONE";
            fString += '\t';

            if ((bool)TechniqueVMAT.IsChecked)
                fString += "VMAT";
            else if ((bool)Technique3DCRT.IsChecked)
                fString += "3DCRT";
            else if ((bool)TechniqueIMRT.IsChecked)
                fString += "IMRT";
            else
                fString += "NA";

            if ((bool)HybridCheckBox.IsChecked)
                fString += "|HYBRID";

            if (BreathHoldCheckBox.IsChecked == true)
                fString += "|BH";
            if ((bool)AutoPlannedCheckBox.IsChecked)
                fString += "|AUTO";
            if ((bool)TangentialCheckBox.IsChecked)
                fString += "|TANG";

            fString += '\t';
            if (planSetup != null)
                fString += planSetup.Beams.First().TreatmentUnit.Id;
            else
                fString += planSum.PlanSetups.First().Beams.First().TreatmentUnit.Id;
            fString += '\t';

            if (BolusComboBox.SelectedIndex == 0)
                fString += "NONE";
            else if (BolusComboBox.SelectedIndex == 1)
                fString += "PARTIAL";
            else if (BolusComboBox.SelectedIndex == 2)
                fString += "ALWAYS";
            fString += '\t';

            fString += PlanId.Text + '\t';

            fString += PlanDataString + '\t';

            if (ChestWallCheckBox.IsChecked == true)
            {
                if (ExpanderCheckBox.IsChecked == true)
                    fString += "CHESTWALL|EXP";
                else if (ExpanderHSCheckBox.IsChecked == true)
                    fString += "CHESTWALL|EXPHS";
                else
                    fString += "CHESTWALL";
            }
            else
                fString += "NONE";
            fString += '\t';

            if (BoostCheckBox.IsChecked == true && SPCNCheckBox.IsChecked == true)
                fString += "YES-BOOST|YES-SPCN";
            else if (BoostCheckBox.IsChecked == true)
                fString += "YES-BOOST";
            else if (SPCNCheckBox.IsChecked == true)
                fString += "YES-SPCN";
            else
                fString += "NO";
            fString += '\t';

            if (ProtocolComboBox.SelectedIndex == 0) // START RTOG 1005
            {
                fString += TargetBreastId.ToolTip.ToString().TrimEnd('c').TrimEnd() + '\t'; // VOLUME in cc
                if (BoostCheckBox.IsChecked == true)
                {
                    MyItem imyItem;
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(0);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(1);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(2);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(4);
                    fString += imyItem.Results + "\t\t";
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(5);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(6);
                    fString += imyItem.Results + '\t';

                    fString += TargetBoostId.ToolTip.ToString().TrimEnd('c').TrimEnd() + '\t';  // VOLUME in cc
                    imyItem = (MyItem)TargetBoostDataListView.Items.GetItemAt(0);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBoostDataListView.Items.GetItemAt(1);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBoostDataListView.Items.GetItemAt(2);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBoostDataListView.Items.GetItemAt(3);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBoostDataListView.Items.GetItemAt(4);
                    fString += imyItem.Results + '\t';
                }
                else
                {
                    MyItem imyItem;
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(0);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(1);
                    fString += imyItem.Results + '\t' + '\t' + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(2);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(3);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(4);
                    fString += imyItem.Results + '\t';

                    // skip Boost columns
                    fString += "\t\t\t\t\t\t";
                }

                if (TargetSPCList.SelectedIndex != -1)
                {
                    fString += TargetSPCId.ToolTip.ToString().TrimEnd('c').TrimEnd() + '\t';  // VOLUME in cc
                    foreach (var iItem in TargetSPCDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t\t";

                if (IpsiLungList.SelectedIndex != -1)
                {
                    foreach (var iItem in IpsiLungDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t\t\t";

                if (ContraLungList.SelectedIndex != -1)
                {
                    if (BilateralCheckBox.IsChecked == false)
                    {
                        foreach (var iItem in ContraLungDataListView.Items)
                        {
                            MyItem imyItem = (MyItem)iItem;
                            fString += imyItem.Results + '\t';
                        }
                    }
                    else
                    {
                        MyItem imyItem;
                        imyItem = (MyItem)ContraLungDataListView.Items.GetItemAt(2);
                        fString += imyItem.Results + '\t';
                        imyItem = (MyItem)ContraLungDataListView.Items.GetItemAt(3);
                        fString += imyItem.Results + '\t';
                        imyItem = (MyItem)ContraLungDataListView.Items.GetItemAt(4);
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";

                if (HeartList.SelectedIndex != -1)
                {
                    foreach (var iItem in HeartDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t\t\t\t\t";

                if (LeftCoronaryList.SelectedIndex != -1)
                {
                    foreach (var iItem in LeftCoronaryDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t";

                if (ContraBreastList.SelectedIndex != -1)
                {
                    MyItem imyItem;
                    imyItem = (MyItem)ContraBreastDataListView.Items.GetItemAt(0);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)ContraBreastDataListView.Items.GetItemAt(1);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)ContraBreastDataListView.Items.GetItemAt(3);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)ContraBreastDataListView.Items.GetItemAt(4);
                    fString += imyItem.Results + '\t';
                }
                else
                    fString += "\t\t\t\t";


                if (ThyroidList.SelectedIndex != -1)
                {
                    foreach (var iItem in ThyroidDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";

                if (EsophagusList.SelectedIndex != -1)
                {
                    foreach (var iItem in EsophagusDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";


                if (LiverList.SelectedIndex != -1)
                {
                    foreach (var iItem in LiverDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";

                if (StomachList.SelectedIndex != -1)
                {
                    foreach (var iItem in StomachDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";
            } // END RTOG 1005
            else if (ProtocolComboBox.SelectedIndex == 1) // START NHSFF
            {
                fString += TargetBreastId.ToolTip.ToString().TrimEnd('c').TrimEnd() + '\t'; // VOLUME in cc
                if (BoostCheckBox.IsChecked == true)
                {
                    MyItem imyItem;
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(0);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(1);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(2);
                    fString += imyItem.Results + '\t' + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(3);
                    fString += imyItem.Results + "\t";
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(4);
                    fString += imyItem.Results + '\t';

                    fString += TargetBoostId.ToolTip.ToString().TrimEnd('c').TrimEnd() + '\t';  // VOLUME in cc
                    imyItem = (MyItem)TargetBoostDataListView.Items.GetItemAt(0);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBoostDataListView.Items.GetItemAt(1);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBoostDataListView.Items.GetItemAt(2);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBoostDataListView.Items.GetItemAt(3);
                    fString += imyItem.Results + '\t';
                }
                else
                {
                    MyItem imyItem;
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(0);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(1);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(2);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(3);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(4);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(5);
                    fString += imyItem.Results + '\t';

                    // skip Boost columns
                    fString += "\t\t\t\t\t";
                }

                if (TargetSPCList.SelectedIndex != -1)
                {
                    fString += TargetSPCId.ToolTip.ToString().TrimEnd('c').TrimEnd() + '\t';  // VOLUME in cc
                    foreach (var iItem in TargetSPCDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t\t";

                if (IpsiLungList.SelectedIndex != -1)
                {
                    foreach (var iItem in IpsiLungDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t\t";

                if (ContraLungList.SelectedIndex != -1)
                {
                    if (BilateralCheckBox.IsChecked == false)
                    {
                        foreach (var iItem in ContraLungDataListView.Items)
                        {
                            MyItem imyItem = (MyItem)iItem;
                            fString += imyItem.Results + '\t';
                        }
                    }
                    else
                    {
                        MyItem imyItem;
                        imyItem = (MyItem)ContraLungDataListView.Items.GetItemAt(2);
                        fString += imyItem.Results + '\t';
                        imyItem = (MyItem)ContraLungDataListView.Items.GetItemAt(3);
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t";

                if (HeartList.SelectedIndex != -1)
                {
                    foreach (var iItem in HeartDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t\t\t";

                if (LeftCoronaryList.SelectedIndex != -1)
                {
                    foreach (var iItem in LeftCoronaryDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t";

                if (ContraBreastList.SelectedIndex != -1)
                {
                    foreach (var iItem in ContraBreastDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";


                if (ThyroidList.SelectedIndex != -1)
                {
                    foreach (var iItem in ThyroidDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";

                if (EsophagusList.SelectedIndex != -1)
                {
                    foreach (var iItem in EsophagusDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";


                if (LiverList.SelectedIndex != -1)
                {
                    foreach (var iItem in LiverDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";

                if (StomachList.SelectedIndex != -1)
                {
                    foreach (var iItem in StomachDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";
            } //END_NHSFF
            else if (ProtocolComboBox.SelectedIndex == 2) // START FAST
            {
                fString += TargetBreastId.ToolTip.ToString().TrimEnd('c').TrimEnd() + '\t'; // VOLUME in cc
                if (BoostCheckBox.IsChecked == true)
                {
                    MyItem imyItem;
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(0);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(1);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(2);
                    fString += imyItem.Results + '\t' + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(3);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(4);
                    fString += imyItem.Results + '\t';

                    fString += TargetBoostId.ToolTip.ToString().TrimEnd('c').TrimEnd() + '\t';  // VOLUME in cc
                    imyItem = (MyItem)TargetBoostDataListView.Items.GetItemAt(0);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBoostDataListView.Items.GetItemAt(1);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBoostDataListView.Items.GetItemAt(2);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBoostDataListView.Items.GetItemAt(3);
                    fString += imyItem.Results + '\t';
                }
                else
                {
                    MyItem imyItem;
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(0);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(1);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(2);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(3);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(4);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(5);
                    fString += imyItem.Results + '\t';

                    // skip Boost columns
                    fString += "\t\t\t\t\t";
                }

                if (TargetSPCList.SelectedIndex != -1)
                {
                    fString += TargetSPCId.ToolTip.ToString().TrimEnd('c').TrimEnd() + '\t';  // VOLUME in cc
                    foreach (var iItem in TargetSPCDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t\t";

                if (IpsiLungList.SelectedIndex != -1)
                {
                    foreach (var iItem in IpsiLungDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t\t";

                if (ContraLungList.SelectedIndex != -1)
                {
                    if (BilateralCheckBox.IsChecked == false)
                    {
                        foreach (var iItem in ContraLungDataListView.Items)
                        {
                            MyItem imyItem = (MyItem)iItem;
                            fString += imyItem.Results + '\t';
                        }
                    }
                    else
                    {
                        MyItem imyItem;
                        imyItem = (MyItem)ContraLungDataListView.Items.GetItemAt(2);
                        fString += imyItem.Results + '\t';
                        imyItem = (MyItem)ContraLungDataListView.Items.GetItemAt(3);
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t";

                if (HeartList.SelectedIndex != -1)
                {
                    foreach (var iItem in HeartDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t\t\t";

                if (LeftCoronaryList.SelectedIndex != -1)
                {
                    foreach (var iItem in LeftCoronaryDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t";

                if (ContraBreastList.SelectedIndex != -1)
                {
                    foreach (var iItem in ContraBreastDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";


                if (ThyroidList.SelectedIndex != -1)
                {
                    foreach (var iItem in ThyroidDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";

                if (EsophagusList.SelectedIndex != -1)
                {
                    foreach (var iItem in EsophagusDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";


                if (LiverList.SelectedIndex != -1)
                {
                    foreach (var iItem in LiverDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";

                if (StomachList.SelectedIndex != -1)
                {
                    foreach (var iItem in StomachDataListView.Items)
                    {
                        MyItem imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";
            } // END_FAST
            else if (ProtocolComboBox.SelectedIndex == 3) // START EUROPAWBI15
            {
                fString += TargetBreastId.ToolTip.ToString().TrimEnd('c').TrimEnd() + '\t'; // VOLUME in cc

                MyItem imyItem;
                imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(0);
                fString += imyItem.Results + '\t';
                imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(1);
                fString += imyItem.Results + '\t';
                imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(2);
                fString += imyItem.Results + '\t';
                imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(3);
                fString += imyItem.Results + "\t";
                imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(4);
                fString += imyItem.Results + '\t';

                if (IpsiLungList.SelectedIndex != -1)
                {
                    foreach (var iItem in IpsiLungDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t\t";

                if (ContraLungList.SelectedIndex != -1)
                {
                    if (BilateralCheckBox.IsChecked == false)
                    {
                        foreach (var iItem in ContraLungDataListView.Items)
                        {
                            imyItem = (MyItem)iItem;
                            fString += imyItem.Results + '\t';
                        }
                    }
                    else
                    {
                        imyItem = (MyItem)ContraLungDataListView.Items.GetItemAt(2);
                        fString += imyItem.Results + '\t';
                        fString += '\t';
                        imyItem = (MyItem)ContraLungDataListView.Items.GetItemAt(3);
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";

                if (HeartList.SelectedIndex != -1)
                {
                    foreach (var iItem in HeartDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t\t";

                if (LeftCoronaryList.SelectedIndex != -1)
                {
                    foreach (var iItem in LeftCoronaryDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t";

                if (ContraBreastList.SelectedIndex != -1)
                {
                    foreach (var iItem in ContraBreastDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t\t";


                if (ThyroidList.SelectedIndex != -1)
                {
                    foreach (var iItem in ThyroidDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";

                if (EsophagusList.SelectedIndex != -1)
                {
                    foreach (var iItem in EsophagusDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";


                if (LiverList.SelectedIndex != -1)
                {
                    foreach (var iItem in LiverDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";

                if (StomachList.SelectedIndex != -1)
                {
                    foreach (var iItem in StomachDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";
            } // END EUROPAWBI15
            else if (ProtocolComboBox.SelectedIndex == 4) // START EUROPAWBI5
            {
                fString += TargetBreastId.ToolTip.ToString().TrimEnd('c').TrimEnd() + '\t'; // VOLUME in cc
                
                MyItem imyItem;
                imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(0);
                fString += imyItem.Results + '\t';
                imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(1);
                fString += imyItem.Results + '\t';
                imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(2);
                fString += imyItem.Results + '\t' + '\t';
                imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(3);
                fString += imyItem.Results + "\t";
                imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(4);
                fString += imyItem.Results + '\t';
                imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(5);
                fString += imyItem.Results + '\t';

                if (IpsiLungList.SelectedIndex != -1)
                {
                    foreach (var iItem in IpsiLungDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t\t";

                if (ContraLungList.SelectedIndex != -1)
                {
                    if (BilateralCheckBox.IsChecked == false)
                    {
                        foreach (var iItem in ContraLungDataListView.Items)
                        {
                            imyItem = (MyItem)iItem;
                            fString += imyItem.Results + '\t';
                        }
                    }
                    else
                    {
                        imyItem = (MyItem)ContraLungDataListView.Items.GetItemAt(2);
                        fString += imyItem.Results + '\t';
                        imyItem = (MyItem)ContraLungDataListView.Items.GetItemAt(3);
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t";

                if (HeartList.SelectedIndex != -1)
                {
                    foreach (var iItem in HeartDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t\t\t";

                if (LeftCoronaryList.SelectedIndex != -1)
                {
                    foreach (var iItem in LeftCoronaryDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t";

                if (ContraBreastList.SelectedIndex != -1)
                {
                    foreach (var iItem in ContraBreastDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";


                if (ThyroidList.SelectedIndex != -1)
                {
                    foreach (var iItem in ThyroidDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";

                if (EsophagusList.SelectedIndex != -1)
                {
                    foreach (var iItem in EsophagusDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";


                if (LiverList.SelectedIndex != -1)
                {
                    foreach (var iItem in LiverDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";

                if (StomachList.SelectedIndex != -1)
                {
                    foreach (var iItem in StomachDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";
            } //END_EUROPAWBI5
            else if (ProtocolComboBox.SelectedIndex == 5) // START EUROPAPBI5
            {
                fString += TargetBreastId.ToolTip.ToString().TrimEnd('c').TrimEnd() + '\t'; // VOLUME in cc
                MyItem imyItem;
                imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(0);
                fString += imyItem.Results + '\t';
                imyItem = (MyItem)TargetBreastDataListView.Items.GetItemAt(1);
                fString += imyItem.Results + '\t';

                fString += TargetBoostId.ToolTip.ToString().TrimEnd('c').TrimEnd() + '\t'; // VOLUME in cc
                imyItem = (MyItem)TargetBoostDataListView.Items.GetItemAt(0);
                fString += imyItem.Results + '\t';
                imyItem = (MyItem)TargetBoostDataListView.Items.GetItemAt(1);
                fString += imyItem.Results + '\t';
                imyItem = (MyItem)TargetBoostDataListView.Items.GetItemAt(2);
                fString += imyItem.Results + '\t';
                imyItem = (MyItem)TargetBoostDataListView.Items.GetItemAt(3);
                fString += imyItem.Results + '\t';
                imyItem = (MyItem)TargetBoostDataListView.Items.GetItemAt(4);
                fString += imyItem.Results + '\t';

                if (TargetSPCList.SelectedIndex != -1)
                {
                    imyItem = (MyItem)TargetSPCDataListView.Items.GetItemAt(0);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetSPCDataListView.Items.GetItemAt(1);
                    fString += imyItem.Results + '\t';
                    imyItem = (MyItem)TargetSPCDataListView.Items.GetItemAt(2);
                    fString += imyItem.Results + '\t';
                }
                else
                    fString += "\t\t\t";

                if (IpsiLungList.SelectedIndex != -1)
                {
                    foreach (var iItem in IpsiLungDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t\t";

                if (ContraLungList.SelectedIndex != -1)
                {
                    if (BilateralCheckBox.IsChecked == false)
                    {
                        foreach (var iItem in ContraLungDataListView.Items)
                        {
                            imyItem = (MyItem)iItem;
                            fString += imyItem.Results + '\t';
                        }
                    }
                    else
                    {
                        fString += '\t';
                        imyItem = (MyItem)ContraLungDataListView.Items.GetItemAt(3);
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t";

                if (HeartList.SelectedIndex != -1)
                {
                    foreach (var iItem in HeartDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t\t";

                if (LeftCoronaryList.SelectedIndex != -1)
                {
                    foreach (var iItem in LeftCoronaryDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t";

                if (ContraBreastList.SelectedIndex != -1)
                {
                    foreach (var iItem in ContraBreastDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";


                if (ThyroidList.SelectedIndex != -1)
                {
                    foreach (var iItem in ThyroidDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";

                if (EsophagusList.SelectedIndex != -1)
                {
                    foreach (var iItem in EsophagusDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";


                if (LiverList.SelectedIndex != -1)
                {
                    foreach (var iItem in LiverDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";

                if (StomachList.SelectedIndex != -1)
                {
                    foreach (var iItem in StomachDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t";

                if (SkinList.SelectedIndex != -1)
                {
                    foreach (var iItem in SkinDataListView.Items)
                    {
                        imyItem = (MyItem)iItem;
                        fString += imyItem.Results + '\t';
                    }
                }
                else
                    fString += "\t\t\t\t";
            } //END_EUROPAPBI5

            fString += PlanScore.Value.ToString("F2") + '\t' + userName + '\t' + CommentsBox.Text;

            return fString;
        }

        // applies to FilterPopup filters the values found/selected for the current plan; this makes filtering look for "exact match" in the plan library
        public void ApplyFilterDefaults_OnPopup()
        {
            if (BilateralCheckBox.IsChecked == true)
            {
                ((ComboBoxItem)(FilterPopup.Filter_BreastLaterality_ComboBox.Items.GetItemAt(2))).IsEnabled = false;
                FilterPopup.Filter_BreastLaterality_ComboBox.SelectedIndex = 2;
            }
            else
            {
                ((ComboBoxItem)(FilterPopup.Filter_BreastLaterality_ComboBox.Items.GetItemAt(2))).IsEnabled = false;
                if (LeftBreastRadio.IsChecked == true)
                    FilterPopup.Filter_BreastLaterality_ComboBox.SelectedIndex = 0;
                else if (RightBreastRadio.IsChecked == true)
                    FilterPopup.Filter_BreastLaterality_ComboBox.SelectedIndex = 1;
            }

            FilterPopup.Filter_Targets_BoostToggleButton.IsChecked = BoostCheckBox.IsChecked;
            FilterPopup.Filter_Targets_SPCNToggleButton.IsChecked = SPCNCheckBox.IsChecked;

            FilterPopup.Filter_Targets_ChestWall.IsChecked = ChestWallCheckBox.IsChecked;
            FilterPopup.Filter_Targets_ChestWall_EXP.IsChecked = ExpanderCheckBox.IsChecked;
            FilterPopup.Filter_Targets_ChestWall_EXP_HS.IsChecked = ExpanderHSCheckBox.IsChecked;

            FilterPopup.Filter_Setup_PBI.IsChecked = PBICheckBox.IsChecked;
            FilterPopup.Filter_Setup_Prone.IsChecked = ProneCheckBox.IsChecked;

            if (Technique3DCRT.IsChecked == true)
            {
                FilterPopup.Filter_Technique_ComboBox.SelectedIndex = 0;
                FilterPopup.Filter_Technique_Hybrid.IsEnabled = false;
            }
            else if (TechniqueIMRT.IsChecked == true)
                FilterPopup.Filter_Technique_ComboBox.SelectedIndex = 1;
            else if (TechniqueVMAT.IsChecked == true)
                FilterPopup.Filter_Technique_ComboBox.SelectedIndex = 2;
            FilterPopup.Filter_Technique_Hybrid.IsChecked = HybridCheckBox.IsChecked;

            FilterPopup.Filter_Technique_BreathHold.IsChecked = BreathHoldCheckBox.IsChecked;
            FilterPopup.Filter_Technique_Tangential.IsChecked = TangentialCheckBox.IsChecked;
            FilterPopup.Filter_Technique_Autoplanned.IsChecked = AutoPlannedCheckBox.IsChecked;
        }

        // applies filters to get filtered rows
        public void ApplyFilterOnDataTable_FromPopup()
        {
            if (dataTable_dip == DataInspectorPolicy.DIP_NONE || dataTable_dts == DataTableStatus.DTS_NOTABLE)
                return;
            else if (dataTable_dip == DataInspectorPolicy.DIP_RESTRICTUSER && dataTable_dts == DataTableStatus.DTS_NOUSER)
                return;

            // Apply Filter on Laterality, Add Targets (Boost/SPCN) and Technique
            string filterstring_Laterality;
            if (FilterPopup.Filter_BreastLaterality_ComboBox.SelectedIndex == 2)
                filterstring_Laterality = "BIL";
            else if (FilterPopup.Filter_BreastLaterality_ComboBox.SelectedIndex == 0)
                filterstring_Laterality = "L";
            else
                filterstring_Laterality = "R";

            if (FilterPopup.Filter_Setup_PBI.IsChecked == true)
                filterstring_Laterality += "|PBI";
            if (FilterPopup.Filter_Setup_Prone.IsChecked == true)
                filterstring_Laterality += "|PRONE";

            string filterstring_Technique;
            if (FilterPopup.Filter_Technique_ComboBox.SelectedIndex == 0)
                filterstring_Technique = "3DCRT";
            else if (FilterPopup.Filter_Technique_ComboBox.SelectedIndex == 1)
                filterstring_Technique = "IMRT";
            else if (FilterPopup.Filter_Technique_ComboBox.SelectedIndex == 2)
                filterstring_Technique = "VMAT";
            else
                filterstring_Technique = "NA";

            if (FilterPopup.Filter_Technique_Hybrid.IsChecked == true)
                filterstring_Technique += "|HYBRID";
            if (FilterPopup.Filter_Technique_BreathHold.IsChecked == true)
                filterstring_Technique += "|BH";
            if (FilterPopup.Filter_Technique_Tangential.IsChecked == true)
                filterstring_Technique += "|TANG";
            if (FilterPopup.Filter_Technique_Autoplanned.IsChecked == true)
                filterstring_Technique += "|AUTO";

            string filterstring_wAddTargets = "NO";
            if (FilterPopup.Filter_Targets_BoostToggleButton.IsChecked == true)
            {
                filterstring_wAddTargets = "YES-BOOST";
                if (FilterPopup.Filter_Targets_SPCNToggleButton.IsChecked == true)
                    filterstring_wAddTargets += "|YES-SPCN";
            }
            else if (FilterPopup.Filter_Targets_SPCNToggleButton.IsChecked == true)
                filterstring_wAddTargets = "YES-SPCN";

            string filterString_Options = "NONE";
            if (FilterPopup.Filter_Targets_ChestWall.IsChecked == true)
                filterString_Options = "CHESTWALL";
            if (FilterPopup.Filter_Targets_ChestWall_EXP.IsChecked == true)
                filterString_Options += "|EXP";
            if (FilterPopup.Filter_Targets_ChestWall_EXP_HS.IsChecked == true)
                filterString_Options += "|EXP_HS";

            string filterString = "LATERALITY ='" + filterstring_Laterality + "'" + "AND TECHNIQUE ='" + filterstring_Technique + "'" + "AND wADDTARGETS ='" + filterstring_wAddTargets + "'" + "AND OPTIONS ='" + filterString_Options + "'";
            string filterString_UserRestricted = filterString + "AND USER='" + userName + "'";
            if (dataTable_dip == DataInspectorPolicy.DIP_RESTRICTUSER && dataTable_dts == DataTableStatus.DTS_BASICUSER)  // restrict data to USER only
                filterString += "AND USER='" + userName + "'";
            DataRow[] datatable_filteredRows_All = dataTable.Select(filterString);
            if (FilterPopup.Filter_Users_CurrentRadioButton.IsChecked == true)
                filterString += "AND USER='" + userName + "'";
            dataTable_filteredRows = dataTable.Select(filterString);
            dataTable_filteredRows_CurrentUser = dataTable.Select(filterString_UserRestricted);

            if (dataTable_filteredRows != null)
            {
                int ProtocolRecordsCount = dataTable_filteredRows.Count();
                if (ProtocolRecordsCount > 0)
                {
                    CBRecords.Text = "(" + ProtocolRecordsCount.ToString("F0") + " RECS)";
                    FilterPopup.AllAvailableFilterCountTextBlock.Text = datatable_filteredRows_All.Count().ToString("F0");
                    if (dataTable_dip == DataInspectorPolicy.DIP_RESTRICTUSER && dataTable_dts == DataTableStatus.DTS_BASICUSER)  // restrict data to USER only
                        FilterPopup.AllAvailableFilterCountTextBlock.Text += "(user restricted)";

                    FilterPopup.CurrentUserFilterCountTextBlock.Text = dataTable_filteredRows_CurrentUser.Count().ToString("F0");
                }
                else
                {
                    CBRecords.Text = "(NO RECS)";
                    FilterPopup.AllAvailableFilterCountTextBlock.Text = "0";
                    FilterPopup.CurrentUserFilterCountTextBlock.Text = "0";
                }
            }
            else
            {
                CBRecords.Text = "(NO RECS)";
                FilterPopup.AllAvailableFilterCountTextBlock.Text = "0";
                FilterPopup.CurrentUserFilterCountTextBlock.Text = "0";
            }
        }

        private void ComputeDVHData(ListView DataListView, Structure volume, DVHData dvhData, DVHData dvhDataVolCC)
        {
            // Compute Results and Protocol Scores 
            foreach (var iItem in DataListView.Items)
            {
                MyItem imyItem = (MyItem)iItem;
                if (imyItem.Data.Contains("Avg"))   //Average dose
                {
                    double results = dvhData.MeanDose.Unit == DoseValue.DoseUnit.Gy ? dvhData.MeanDose.Dose : dvhData.MeanDose.Dose*100;
                    imyItem.Results = results.ToString("F2");

                    if (imyItem.Const[0] == '~')
                    {
                        double constr = Convert.ToDouble(imyItem.Const.TrimStart('~'));
                        double percdiff = (results - constr) / constr * 100;

                        if (imyItem.ReferenceDose > 0)
                            imyItem.ResultsPerc = (results * 100.0 / imyItem.ReferenceDose).ToString("F1");

                        //if (percdiff >= -5 && percdiff <= 5)
                        if (percdiff >= -3 && percdiff <= 3)
                            imyItem.Score = 1;
                        else
                            imyItem.Score = 0;
                    }
                    else if (imyItem.Const[0] == '<')
                    {
                        double constr, acceptconstr;
                        if (imyItem.Const.Contains('/')) // check for optimal/acceptable couple
                        {
                            string trimmedConst = imyItem.Const.TrimStart('<');
                            constr = Convert.ToDouble(trimmedConst.Substring(0, imyItem.Const.IndexOf('/') - 1));
                            acceptconstr = Convert.ToDouble(trimmedConst.Substring(imyItem.Const.IndexOf('/')));
                        }
                        else
                        {
                            constr = Convert.ToDouble(imyItem.Const.TrimStart('<'));
                            acceptconstr = -1;
                        }

                        if (results <= constr)
                            imyItem.Score = 1;
                        else if (results <= acceptconstr)
                            imyItem.Score = 0.5;
                        else
                            imyItem.Score = 0;
                    }
                }
                else if (imyItem.Data.Contains("Max"))  //Maximum dose
                {
                    double results = dvhData.MaxDose.Unit == DoseValue.DoseUnit.Gy? dvhData.MaxDose.Dose : dvhData.MaxDose.Dose*100;
                    imyItem.Results = results.ToString("F2");
                    if (imyItem.ReferenceDose > 0)
                        imyItem.ResultsPerc = (results * 100.0 / imyItem.ReferenceDose).ToString("F1");

                    double constr, acceptconstr;
                    if (imyItem.Const.Contains('/')) // check for optimal/acceptable couple
                    {
                        string trimmedConst = imyItem.Const.TrimStart('<');
                        constr = Convert.ToDouble(trimmedConst.Substring(0, imyItem.Const.IndexOf('/') - 1));
                        acceptconstr = Convert.ToDouble(trimmedConst.Substring(imyItem.Const.IndexOf('/')));
                    }
                    else
                    {
                        constr = Convert.ToDouble(imyItem.Const.TrimStart('<'));
                        acceptconstr = -1;
                    }
                    if (results <= constr)
                        imyItem.Score = 1;
                    else if (results <= acceptconstr)
                        imyItem.Score = 0.5;
                    else
                        imyItem.Score = 0;
                }
                else if (imyItem.Data.Contains('V'))    //VX Volume in % receiving more than X dose (Gy)
                {
                    double doseValueGy = 0;

                    if (imyItem.Data.Contains('*'))
                        doseValueGy = Convert.ToDouble(imyItem.Data.TrimStart('*').TrimStart('V'));
                    else
                        doseValueGy = Convert.ToDouble(imyItem.Data.TrimStart('V'));

                    DoseValue DV = new DoseValue(doseValueGy, DoseValue.DoseUnit.Gy);
                    double Organ_V = 0;
                    if (planSetup != null)
                    {
                        Organ_V = planSetup.GetVolumeAtDose(volume, DV, VolumePresentation.Relative);
                        if (double.IsNaN(Organ_V))
                            Organ_V = DVHGetVolumeAtDose(dvhData, DV);

                    }
                    else if (planSum != null)
                        Organ_V = DVHGetVolumeAtDose(dvhData, DV);
                    imyItem.Results = Organ_V.ToString("F2");

                    double results = Organ_V;
                    double constr, acceptconstr;
                    if (imyItem.Const.Contains('/')) // check for optimal/acceptable couple
                    {
                        string trimmedConst = imyItem.Const.TrimStart('<');
                        constr = Convert.ToDouble(trimmedConst.Substring(0, imyItem.Const.IndexOf('/') - 1));
                        acceptconstr = Convert.ToDouble(trimmedConst.Substring(imyItem.Const.IndexOf('/')));
                    }
                    else
                    {
                        constr = Convert.ToDouble(imyItem.Const.TrimStart('<'));
                        acceptconstr = -1;
                    }

                    if (constr == 0 && results - constr <= 0.001) // check for V-constraint = 0
                        imyItem.Score = 1;
                    else if (results <= constr)
                        imyItem.Score = 1;
                    else if (results <= acceptconstr)
                        imyItem.Score = 0.5;
                    else
                        imyItem.Score = 0;
                }
                else if (imyItem.Data.Contains('C'))    //CX Coverage (targets): % of target receiving more than X dose (Gy)
                {
                    double coverageValueGy = 0;

                    if (imyItem.Data.Contains('*'))
                        coverageValueGy = Convert.ToDouble(imyItem.Data.TrimStart('*').TrimStart('C'));
                    else
                        coverageValueGy = Convert.ToDouble(imyItem.Data.TrimStart('C'));

                    DoseValue DV = new DoseValue(coverageValueGy, DoseValue.DoseUnit.Gy);
                    double Target_V = 0;
                    if (planSetup != null)
                    {
                        Target_V = planSetup.GetVolumeAtDose(volume, DV, VolumePresentation.Relative);
                        if (double.IsNaN(Target_V))
                            Target_V = DVHGetVolumeAtDose(dvhData, DV);
                    }
                    else if (planSum != null)
                        Target_V = DVHGetVolumeAtDose(dvhData, DV);
                    imyItem.Results = Target_V.ToString("F2");

                    double results = Target_V;
                    double constr, acceptconstr;
                    if (imyItem.Const.Contains('/')) // check for optimal/acceptable couple
                    {
                        string trimmedConst = imyItem.Const.TrimStart('>');
                        constr = Convert.ToDouble(trimmedConst.Substring(0, imyItem.Const.IndexOf('/') - 1));
                        acceptconstr = Convert.ToDouble(trimmedConst.Substring(imyItem.Const.IndexOf('/')));
                    }
                    else
                    {
                        constr = Convert.ToDouble(imyItem.Const.TrimStart('>'));
                        acceptconstr = 101;
                    }

                    if (results >= constr)
                        imyItem.Score = 1;
                    else if (results >= acceptconstr)
                        imyItem.Score = 0.5;
                    else
                        imyItem.Score = 0;
                }
                else if (imyItem.Data.Contains("D%"))   //D%X Dose received by X % of volume
                {
                    double volume_inperc = 0;

                    if (imyItem.Data.Contains('*'))
                        volume_inperc = Convert.ToDouble(imyItem.Data.TrimStart('*').TrimStart('D').TrimStart('%'));
                    else
                        volume_inperc = Convert.ToDouble(imyItem.Data.TrimStart('D').TrimStart('%'));


                    double dose_atvol = 0;
                    if (planSetup != null)
                    {
                        DoseValue Organ_D = Organ_D = planSetup.GetDoseAtVolume(volume, volume_inperc, VolumePresentation.Relative, DoseValuePresentation.Absolute);
                        dose_atvol = Organ_D.Dose;

                    }
                    else if (planSum != null)
                        dose_atvol = DVHGetDoseAtVolume(dvhData, volume_inperc);

                    imyItem.Results = dose_atvol.ToString("F2");

                    double results = dose_atvol;
                    double constr, acceptconstr;
                    if (imyItem.Const.Contains('/')) // check for optimal/acceptable couple
                    {
                        string trimmedConst = imyItem.Const.TrimStart('<');
                        constr = Convert.ToDouble(trimmedConst.Substring(0, imyItem.Const.IndexOf('/') - 1));
                        acceptconstr = Convert.ToDouble(trimmedConst.Substring(imyItem.Const.IndexOf('/')));
                    }
                    else
                    {
                        constr = Convert.ToDouble(imyItem.Const.TrimStart('<'));
                        acceptconstr = -1;
                    }

                    if (results <= constr)
                        imyItem.Score = 1;
                    else if (results <= acceptconstr)
                        imyItem.Score = 0.5;
                    else
                        imyItem.Score = 0;
                }
                else if (imyItem.Data.Contains("Dc"))   //DcX Dose received by X cc of volume
                {
                    double volume_incc = 0;

                    if (imyItem.Data.Contains('*'))
                        volume_incc = Convert.ToDouble(imyItem.Data.TrimStart('*').TrimStart('D').TrimStart('c'));
                    else
                        volume_incc = Convert.ToDouble(imyItem.Data.TrimStart('D').TrimStart('c'));

                    double Organ_D = DVHGetDoseAtVolumeCC(dvhDataVolCC, volume_incc);
                    imyItem.Results = Organ_D.ToString("F1");

                    double results = Organ_D;
                    double constr, acceptconstr;
                    if (imyItem.Const.Contains('/')) // check for optimal/acceptable couple
                    {
                        string trimmedConst = imyItem.Const.TrimStart('<');
                        constr = Convert.ToDouble(trimmedConst.Substring(0, imyItem.Const.IndexOf('/') - 1));
                        acceptconstr = Convert.ToDouble(trimmedConst.Substring(imyItem.Const.IndexOf('/')));
                    }
                    else
                    {
                        constr = Convert.ToDouble(imyItem.Const.TrimStart('<'));
                        acceptconstr = -1;
                    }

                    if (results <= constr)
                        imyItem.Score = 1;
                    else if (results <= acceptconstr)
                        imyItem.Score = 0.5;
                    else
                        imyItem.Score = 0;
                }
                else if (imyItem.Data.Contains('R'))    // RX Conformity Index: volume of body receving more than X dose (Gy) / target volume
                {
                    double coverageValueGy = 0;

                    if (imyItem.Data.Contains('*'))
                        coverageValueGy = Convert.ToDouble(imyItem.Data.TrimStart('*').TrimStart('R'));
                    else
                        coverageValueGy = Convert.ToDouble(imyItem.Data.TrimStart('R'));

                    DoseValue DV = new DoseValue(coverageValueGy, DoseValue.DoseUnit.Gy);

                    double conformity_ratio = 0;
                    double Target_V_Cm3 = 0;
                    double Body_V_Cm3 = 0;
                    if (bodydvhDataVolCC != null)
                    {
                        if (planSetup != null)
                        {
                            Target_V_Cm3 = dvhData.Volume;
                            Body_V_Cm3 = planSetup.GetVolumeAtDose(BodyStructure, DV, VolumePresentation.AbsoluteCm3);
                            if (double.IsNaN(Body_V_Cm3))
                                Body_V_Cm3 = DVHGetVolumeAtDose(bodydvhDataVolCC, DV);

                        }
                        else if (planSum != null)
                        {
                            Target_V_Cm3 = dvhData.Volume;
                            Body_V_Cm3 = DVHGetVolumeAtDose(bodydvhDataVolCC, DV);
                        }

                        if (Target_V_Cm3 > 0)
                            conformity_ratio = Body_V_Cm3 / Target_V_Cm3;
                        else
                            conformity_ratio = 0;
                        imyItem.Results = conformity_ratio.ToString("F2");

                        double results = conformity_ratio;
                        double constr, acceptconstr;
                        if (imyItem.Const.Contains('/')) // check for optimal/acceptable couple
                        {
                            string trimmedConst = imyItem.Const.TrimStart('<');
                            constr = Convert.ToDouble(trimmedConst.Substring(0, imyItem.Const.IndexOf('/') - 1));
                            acceptconstr = Convert.ToDouble(trimmedConst.Substring(imyItem.Const.IndexOf('/')));
                        }
                        else
                        {
                            constr = Convert.ToDouble(imyItem.Const.TrimStart('<'));
                            acceptconstr = -1;
                        }

                        if (results == 0)
                            imyItem.Score = 0;
                        else if (results <= constr)
                            imyItem.Score = 1;
                        else if (results <= acceptconstr)
                            imyItem.Score = 0.5;
                        else
                            imyItem.Score = 0;
                    }
                    else
                    {
                        imyItem.Results = "0";
                        imyItem.Score = 0;
                    }
                }
            }

            
            // Compute CommunityScore
            if (dataTable_dip == DataInspectorPolicy.DIP_NONE || dataTable_dts == DataTableStatus.DTS_NOTABLE)
                return;
            else if (dataTable_dip == DataInspectorPolicy.DIP_RESTRICTUSER && dataTable_dts == DataTableStatus.DTS_NOUSER)
                return;

            if (dataTable_filteredRows != null)
            {

                int ProtocolRecordsCount = dataTable_filteredRows.Count();
                foreach (var iItem in DataListView.Items)
                {
                    MyItem imyItem = (MyItem)iItem;
                    if (ProtocolRecordsCount > 0)
                        imyItem.GetCommunityScore(dataTable_filteredRows);
                    else
                        imyItem.CommunityScore = -1;
                }
            }
        }

        public void FetchDVHData()
        {
            if (!this.IsVisible)
                return;

            ApplyFilterOnDataTable_FromPopup();

            TargetBreastDataListView.Items.Clear();
            TargetBoostDataListView.Items.Clear();
            TargetSPCDataListView.Items.Clear();

            IpsiLungDataListView.Items.Clear();
            ContraLungDataListView.Items.Clear();
            HeartDataListView.Items.Clear();
            LeftCoronaryDataListView.Items.Clear();
            ContraBreastDataListView.Items.Clear();
            ThyroidDataListView.Items.Clear();
            EsophagusDataListView.Items.Clear();
            LiverDataListView.Items.Clear();
            StomachDataListView.Items.Clear();
            SkinDataListView.Items.Clear();

            if ((bool)BilateralCheckBox.IsChecked)
            {
                IpsiLungLabel.Content = "Left Lung";
                ContraLungLabel.Content = "Right Lung";

                ContraBreastList.IsEnabled = false;
                ContraBreastDataListView.IsEnabled = false;
            }
            else
            {
                IpsiLungLabel.Content = "Ipsi Lung";
                ContraLungLabel.Content = "Contra Lung";

                ContraBreastList.IsEnabled = true;
                ContraBreastDataListView.IsEnabled = true;
            }

            if (ProtocolComboBox.SelectedIndex == 0) // START_RTOG1005
            {
                if (BoostCheckBox.IsChecked == true)
                {
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "C38.05", Const = ">95", DTName = "BREAST_C38.05" });
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "C36.05", Const = ">90", DTName = "BREAST_C36.05" });
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "V43.2", Const = "<50", DTName = "BREAST_V43.2" });
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "V44.8", Const = "<50" }); // this will not be saved in file
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "V48", Const = "<30/35", DTName = "BREAST_V48" });
                    // v1.0 TargetBreastDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "~40.05", ReferenceDose = 40.05, DTName = "BREAST_Avg" });
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        TargetBreastDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "~41.2", ReferenceDose = 40.05, DTName = "BREAST_Avg" }); // v1.1
                    else
                        TargetBreastDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "~42.5", ReferenceDose = 40.05, DTName = "BREAST_Avg" }); // v1.1

                    TargetBreastDataListView.Items.Add(new MyItem { Data = "R38.05", Const = "<2", DTName = "BREAST_R38.05" });

                    TargetBoostDataListView.Items.Add(new MyItem { Data = "C45.6", Const = ">95", DTName = "BOOST_C45.6" });
                    TargetBoostDataListView.Items.Add(new MyItem { Data = "C43.2", Const = ">90", DTName = "BOOST_C43.2" });
                    TargetBoostDataListView.Items.Add(new MyItem { Data = "V52.8", Const = "<5/10", DTName = "BOOST_V52.8" });
                    TargetBoostDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<50.4", DTName = "BOOST_Max", ReferenceDose = 48 }); //105% (protocol says 115/120%)
                    TargetBoostDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "~48", DTName = "BOOST_Avg", ReferenceDose = 48 });
                    TargetBoostDataListView.Items.Add(new MyItem { Data = "R45.6", Const = "<2.5" });  // this will not be saved in file
                }
                else
                {
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "C38.05", Const = ">95", DTName = "BREAST_C38.05" });
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "C36.05", Const = ">90", DTName = "BREAST_C36.05" });
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<42.85", DTName = "BREAST_Max", ReferenceDose = 40.05 }); //107% (protocol says 115/120%)
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "~40.05", DTName = "BREAST_Avg", ReferenceDose = 40.05 });
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "R38.05", Const = "<2", DTName = "BREAST_R38.05" });

                    TargetBoostDataListView.Items.Add(new MyItem { Data = "C38.05", Const = ">95" });
                    TargetBoostDataListView.Items.Add(new MyItem { Data = "C36.05", Const = ">90" });
                    TargetBoostDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<42.85", ReferenceDose = 40.05 }); //107% (protocol says 115/120%)
                    TargetBoostDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "~40.05", ReferenceDose = 40.05 });
                }

                TargetSPCDataListView.Items.Add(new MyItem { Data = "C38.05", Const = ">95", DTName = "SPCN_C38.05" }); // SPC not in protocol
                TargetSPCDataListView.Items.Add(new MyItem { Data = "Max", Const = "<42.05", DTName = "SPCN_Max", ReferenceDose = 40.05 }); // 105%
                TargetSPCDataListView.Items.Add(new MyItem { Data = "Avg", Const = "~40.05", DTName = "SPCN_Avg", ReferenceDose = 40.05 }); //

                // IPSI LUNG

                IpsiLungDataListView.Items.Add(new MyItem { Data = "V16", Const = "<15/20", DTName = "IPSI-LUNG_V16" });
                IpsiLungDataListView.Items.Add(new MyItem { Data = "V8", Const = "<35/40", DTName = "IPSI-LUNG_V8" });
                IpsiLungDataListView.Items.Add(new MyItem { Data = "V4", Const = "<50/55", DTName = "IPSI-LUNG_V4" });

                IpsiLungDataListView.Items.Add(new MyItem { Data = "*V12", Const = "<15", DTName = "IPSI-LUNG_V12" }); // Not in protocol; v1.2 from START
                // v1.0 IpsiLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<10", DTName = "IPSI-LUNG_Avg" }); // Not in protocol
                if (SPCNCheckBox.IsChecked == true)
                {
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        IpsiLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<8", DTName = "IPSI-LUNG_Avg" }); // Not in protocol; v1.1
                    else
                        IpsiLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<7", DTName = "IPSI-LUNG_Avg" }); // Not in protocol; v1.1
                }
                else
                {
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        IpsiLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<7", DTName = "IPSI-LUNG_Avg" }); // Not in protocol; v1.1
                    else
                        IpsiLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<6", DTName = "IPSI-LUNG_Avg" }); // Not in protocol; v1.1
                }

                // CONTRA LUNG

                if ((bool)BilateralCheckBox.IsChecked)
                {
                    // clone IpsiLungDataListView
                    ContraLungDataListView.Items.Add(new MyItem { Data = "V16", Const = "<15/20" });
                    ContraLungDataListView.Items.Add(new MyItem { Data = "V8", Const = "<35/40" });
                    ContraLungDataListView.Items.Add(new MyItem { Data = "V4", Const = "<50/55", DTName = "CONTRA-LUNG_V4" });

                    ContraLungDataListView.Items.Add(new MyItem { Data = "*V12", Const = "<15", DTName = "CONTRA-LUNG_V12" }); // Not in protocol; v1.2 from START
                    // v1.0 ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<10", DTName = "CONTRA-LUNG_Avg" }); // Not in protocol
                    if (SPCNCheckBox.IsChecked == true)
                    {
                        if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                            ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<8", DTName = "CONTRA-LUNG_Avg" }); // Not in protocol; v1.1
                        else
                            ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<7", DTName = "CONTRA-LUNG_Avg" }); // Not in protocol; v1.1
                    }
                    else
                    {
                        if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                            ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<7", DTName = "CONTRA-LUNG_Avg" }); // Not in protocol; v1.1
                        else
                            ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<6", DTName = "CONTRA-LUNG_Avg" }); // Not in protocol; v1.1
                    }
                }
                else
                {
                    ContraLungDataListView.Items.Add(new MyItem { Data = "V4", Const = "<10/15", DTName = "CONTRA-LUNG_V4" });
                    ContraLungDataListView.Items.Add(new MyItem { Data = "*V2.5", Const = "<15", DTName = "CONTRA-LUNG_V2.5" }); // Not in protocol; v1.2 from EUROPA
                    // v1.0 ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3", DTName = "CONTRA-LUNG_Avg" }); // Not in protocol
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "CONTRA-LUNG_Avg" }); // Not in protocol; v1.1
                    else
                        ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<0.3", DTName = "CONTRA-LUNG_Avg" }); // Not in protocol; v1.1
                }

                // CONTRA BREAST

                if (!(bool)BilateralCheckBox.IsChecked)
                {
                    // v1.0 ContraBreastDataListView.Items.Add(new MyItem { Data = "*V4", Const = "<10/15", DTName = "CONTRA-BREAST_V4" }); // Not in protocol
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        ContraBreastDataListView.Items.Add(new MyItem { Data = "*V4", Const = "<10/15", DTName = "CONTRA-BREAST_V4" }); // Not in protocol
                    else
                        ContraBreastDataListView.Items.Add(new MyItem { Data = "*V4", Const = "<1", DTName = "CONTRA-BREAST_V4" }); // Not in protocol; v1.1

                    ContraBreastDataListView.Items.Add(new MyItem { Data = "V2.4", Const = "<5", DTName = "CONTRA-BREAST_V2.4" });
                    ContraBreastDataListView.Items.Add(new MyItem { Data = "D%5", Const = "<1.44/2.4" });
                    ContraBreastDataListView.Items.Add(new MyItem { Data = "Max", Const = "<2.4/3.84", DTName = "CONTRA-BREAST_Max" });
                    // v1.0 ContraBreastDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2.5", DTName = "CONTRA-BREAST_Avg" }); //3 Gy Humanitas (not in protocol)
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        ContraBreastDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<1.5/2.5", DTName = "CONTRA-BREAST_Avg" }); // Not in protocol; v1.1
                    else
                        ContraBreastDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<0.5", DTName = "CONTRA-BREAST_Avg" }); // Not in protocol; v1.1
                }

                // HEART AND LEFT_CORONARY

                if ((bool)LeftBreastRadio.IsChecked || (bool)BilateralCheckBox.IsChecked)
                {
                    HeartDataListView.Items.Add(new MyItem { Data = "V20", Const = "<5", DTName = "HEART_V20" });
                    HeartDataListView.Items.Add(new MyItem { Data = "V16", Const = "<5", DTName = "HEART_V16" });
                    HeartDataListView.Items.Add(new MyItem { Data = "V8", Const = "<30/35", DTName = "HEART_V8" });
                    // v1.0 HeartDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<28", DTName = "HEART_Max" }); //70% Not in protocol
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        HeartDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<20", DTName = "HEART_Max" }); //Not in protocol; v1.1
                    else
                        HeartDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<25", DTName = "HEART_Max" }); //Not in protocol; v1.1

                    HeartDataListView.Items.Add(new MyItem { Data = "*V10", Const = "<5", DTName = "HEART_V10" }); // Not in protocol; v1.2 from START
                    HeartDataListView.Items.Add(new MyItem { Data = "*V2", Const = "<30", DTName = "HEART_V2" }); // Not in protocol; v1.2 from START
                    HeartDataListView.Items.Add(new MyItem { Data = "Avg", Const = "<3.2/4", DTName = "HEART_Avg" });

                    LeftCoronaryDataListView.Items.Add(new MyItem { Data = "Max", Const = "<16", DTName = "LEFT-COR_Max" }); // Not in protocol
                    // v1.0 LeftCoronaryDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<8", DTName = "LEFT-COR_Avg" }); // Not in protocol
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        LeftCoronaryDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<6", DTName = "LEFT-COR_Avg" }); // Not in protocol: v1.1
                    else
                        LeftCoronaryDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3", DTName = "LEFT-COR_Avg" }); // Not in protocol: v1.1
                }
                else if ((bool)RightBreastRadio.IsChecked)
                {
                    HeartDataListView.Items.Add(new MyItem { Data = "V20", Const = "<0", DTName = "HEART_V20" });
                    HeartDataListView.Items.Add(new MyItem { Data = "V16", Const = "<0", DTName = "HEART_V16" });
                    HeartDataListView.Items.Add(new MyItem { Data = "V8", Const = "<10/15", DTName = "HEART_V8" });
                    // v1.0 HeartDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<16", DTName = "HEART_Max" });  // Not in protocol
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        HeartDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<10", DTName = "HEART_Max" });  // Not in protocol; v1.1
                    else
                        HeartDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<4", DTName = "HEART_Max" });  // Not in protocol; v1.1

                    HeartDataListView.Items.Add(new MyItem { Data = "*V10", Const = "<5", DTName = "HEART_V10" }); // Not in protocol; v1.2 from START
                    HeartDataListView.Items.Add(new MyItem { Data = "*V2", Const = "<30", DTName = "HEART_V2" }); // Not in protocol; v1.2 from START
                    HeartDataListView.Items.Add(new MyItem { Data = "Avg", Const = "<3.2", DTName = "HEART_Avg" });

                    LeftCoronaryDataListView.Items.Add(new MyItem { Data = "Max", Const = "<16", DTName = "LEFT-COR_Max" }); // Not in protocol
                    // v1.0 LeftCoronaryDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<5", DTName = "LEFT-COR_Avg" }); // Not in protocol
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        LeftCoronaryDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3", DTName = "LEFT-COR_Avg" }); // Not in protocol; v1.1
                    else
                        LeftCoronaryDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<0.3", DTName = "LEFT-COR_Avg" }); // Not in protocol; v1.1
                }

                // THYROID

                if (SPCNCheckBox.IsChecked == true)
                {
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*V24", Const = "<50", DTName = "THYROID_V24" }); // Not in protocol
                    // v.1.0 ThyroidDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<42.05", DTName = "THYROID_Max" }); // 105% not in protocol
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<41.3", DTName = "THYROID_Max" }); // 103% not in protocol; v1.1
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<20", DTName = "THYROID_Avg" }); // Not in protocol
                }
                else
                {
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*V24", Const = "<0", DTName = "THYROID_V24" });
                    ThyroidDataListView.Items.Add(new MyItem { Data = "Max", Const = "<1/1.44", DTName = "THYROID_Max" });
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<1", DTName = "THYROID_Avg" }); // Not in protocol
                }

                // ESOPHAGUS

                // v1.0 EsophagusDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<10", DTName = "ESOPHAGUS_V20" }); // Not in protocol
                if (SPCNCheckBox.IsChecked == true)
                {
                    if ((bool)LeftBreastRadio.IsChecked || (bool)BilateralCheckBox.IsChecked)
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<10", DTName = "ESOPHAGUS_V20" }); // Not in protocol; v1.1
                    else if ((bool)RightBreastRadio.IsChecked)
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<5", DTName = "ESOPHAGUS_V20" }); // Not in protocol; v1.1
                }
                else
                {
                    if ((bool)LeftBreastRadio.IsChecked || (bool)BilateralCheckBox.IsChecked)
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<0", DTName = "ESOPHAGUS_V20" }); // Not in protocol; v1.1
                    else if ((bool)RightBreastRadio.IsChecked)
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<0", DTName = "ESOPHAGUS_V20" }); // Not in protocol; v1.1
                }

                // v1.0 EsophagusDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<42.05", DTName = "ESOPHAGUS_Max" }); // 105% not in protocol
                if (SPCNCheckBox.IsChecked == true)
                {
                    if ((bool)LeftBreastRadio.IsChecked || (bool)BilateralCheckBox.IsChecked)
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<38", DTName = "ESOPHAGUS_Max" }); // Not in protocol; v1.1
                    else if ((bool)RightBreastRadio.IsChecked)
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<30", DTName = "ESOPHAGUS_Max" }); // Not in protocol; v1.1
                }
                else
                {
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<6", DTName = "ESOPHAGUS_Max" }); // Not in protocol; v1.1
                    else
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<0", DTName = "ESOPHAGUS_Max" }); // Not in protocol; v1.1
                }


                // v1.0 EsophagusDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<10", DTName = "ESOPHAGUS_Avg" }); // Not in protocol
                if (SPCNCheckBox.IsChecked == true)
                {
                    if ((bool)LeftBreastRadio.IsChecked || (bool)BilateralCheckBox.IsChecked)
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<8", DTName = "ESOPHAGUS_Avg" }); // Not in protocol; v1.1
                    else if ((bool)RightBreastRadio.IsChecked)
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<7", DTName = "ESOPHAGUS_Avg" }); // Not in protocol; v1.1
                }
                else
                {
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3", DTName = "ESOPHAGUS_Avg" }); // Not in protocol; v1.1
                    else
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<0.5", DTName = "ESOPHAGUS_Avg" }); // Not in protocol; v1.1
                }

                // LIVER

                // v1.0
                /*
                if ((bool)BilateralCheckBox.IsChecked)
                {
                    LiverDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<10", DTName = "LIVER_V20" }); // Not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<40.05", DTName = "LIVER_Max" }); // 100% not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<5", DTName = "LIVER_Avg" }); // Not in protocol

                    StomachDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<5", DTName = "STOMACH_V20" }); // Not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<24", DTName = "STOMACH_Max" }); // 60% not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<5", DTName = "STOMACH_Avg" }); // Not in protocol
                }
                else if ((bool)LeftBreastRadio.IsChecked)
                {
                    LiverDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<0", DTName = "LIVER_V20" }); // Not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<8", DTName = "LIVER_Max" }); // 20% not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "LIVER_Avg" }); // 5% not in protocol

                    StomachDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<5", DTName = "STOMACH_V20" }); // Not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<24", DTName = "STOMACH_Max" }); // 60% not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<5", DTName = "STOMACH_Avg" }); // Not in protocol
                }
                else if ((bool)RightBreastRadio.IsChecked)
                {
                    LiverDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<10", DTName = "LIVER_V20" }); // Not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<40.05", DTName = "LIVER_Max" }); // 100% not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<5", DTName = "LIVER_Avg" }); // Not in protocol

                    StomachDataListView.Items.Add(new MyItem { Data = "*V20", Const = "0", DTName = "STOMACH_V20" }); // Not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<8", DTName = "STOMACH_Max" }); // 20% not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "STOMACH_Avg" }); // 5% not in protocol
                }
                */
                // v1.1
                if ((bool)BilateralCheckBox.IsChecked)
                {
                    LiverDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<2", DTName = "LIVER_V20" }); // Not in protocol; v1.1
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<38", DTName = "LIVER_Max" }); // Not in protocol; v1.1
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "LIVER_Avg" }); // Not in protocol; v1.1

                    StomachDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<2", DTName = "STOMACH_V20" }); // Not in protocol; v1.1
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<16", DTName = "STOMACH_Max" }); // Not in protocol; v1.1
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "STOMACH_Avg" }); // Not in protocol; v1.1
                }
                else if ((bool)LeftBreastRadio.IsChecked)
                {
                    LiverDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<0", DTName = "LIVER_V20" }); // Not in protocol; v1.1
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<8", DTName = "LIVER_Max" }); // Not in protocol; v1.1
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<1", DTName = "LIVER_Avg" }); // Not in protocol; v1.1

                    StomachDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<2", DTName = "STOMACH_V20" }); // Not in protocol; v1.1
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<16", DTName = "STOMACH_Max" }); // Not in protocol; v1.1
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "STOMACH_Avg" }); // Not in protocol; v1.1
                }
                else if ((bool)RightBreastRadio.IsChecked)
                {
                    LiverDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<2", DTName = "LIVER_V20" }); // Not in protocol; v1.1
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<38", DTName = "LIVER_Max" }); // Not in protocol; v1.1
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "LIVER_Avg" }); // Not in protocol; v1.1

                    StomachDataListView.Items.Add(new MyItem { Data = "*V20", Const = "0", DTName = "STOMACH_V20" }); // Not in protocol; v1.1
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<4", DTName = "STOMACH_Max" }); // Not in protocol; v1.1
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<1", DTName = "STOMACH_Avg" }); // Not in protocol; v1.1
                }

                SkinDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<41.3" }); // 103% not in protocol
                SkinDataListView.Items.Add(new MyItem { Data = "*Dc1", Const = "<40.05" }); // 100% not in protocol
                SkinDataListView.Items.Add(new MyItem { Data = "*Dc10", Const = "<40.05" }); // 100% not in protocol
                SkinDataListView.Items.Add(new MyItem { Data = "*Dc125", Const = "<20.0" }); // 50% not in protocol

            } // END_RTOG1005
            else if (ProtocolComboBox.SelectedIndex == 1) // START_FF 26 Gy
            {
                if (BoostCheckBox.IsChecked == true)
                {
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "C24.7", Const = ">95", DTName = "BREAST_C24.7" });
                    //v1.0 TargetBreastDataListView.Items.Add(new MyItem { Data = "*V27.3", Const = "<50", DTName = "BREAST_V27.3" });  // From RTOG1005
                    //v1.0 TargetBreastDataListView.Items.Add(new MyItem { Data = "*V27.82", Const = "<30", DTName = "BREAST_V27.82" });  // From RTOG1005
                    //v1.0 TargetBreastDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "~26", DTName = "BREAST_Avg", ReferenceDose = 26 });
                    //v1.0 TargetBreastDataListView.Items.Add(new MyItem { Data = "R24.7", Const = "<2", DTName = "BREAST_R24.7" });
                    string const1 = "50", const2 = "35", const3 = "27.3", const4 = "1.5";
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                    {
                        const1 = "24";
                        const2 = "22";
                        const3 = "26.5";
                        const4 = "1.2";
                    }
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "*V27.3", Const = "<" + const1, DTName = "BREAST_V27.3" });  // NEW
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "*V27.82", Const = "<" + const2, DTName = "BREAST_V27.82" });  // NEW
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "~" + const3, DTName = "BREAST_Avg", ReferenceDose = 26 }); // NEW
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "*R24.7", Const = "<" + const4, DTName = "BREAST_R24.7" }); // NEW


                    TargetBoostDataListView.Items.Add(new MyItem { Data = "C28.5", Const = ">95", DTName = "BOOST_C28.5" });
                    TargetBoostDataListView.Items.Add(new MyItem { Data = "V31.5", Const = "<5", DTName = "BOOST_V31.5" });
                    TargetBoostDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<31.5", DTName = "BOOST_Max", ReferenceDose = 30 }); //105% (protocol says 115%)
                    TargetBoostDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "~30", DTName = "BOOST_Avg", ReferenceDose = 30 });
                }
                else
                {
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "C24.7", Const = ">95", DTName = "BREAST_C24.7" });
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "V27.3", Const = "<5", DTName = "BREAST_V27.3" });
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "V27.82", Const = "<2", DTName = "BREAST_V27.82" });
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "Max", Const = "<28.6", DTName = "BREAST_Max", ReferenceDose = 26 }); //107%
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "~26", DTName = "BREAST_Avg", ReferenceDose = 26 });
                    //v1.0 TargetBreastDataListView.Items.Add(new MyItem { Data = "*R24.7", Const = "<2", DTName = "BREAST_R24.7" });
                    string const5 = "1.4";
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        const5 = "1.1";
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "*R24.7", Const = "<" + const5, DTName = "BREAST_R24.7" });

                    TargetBoostDataListView.Items.Add(new MyItem { Data = "C24.7", Const = ">95" });
                    TargetBoostDataListView.Items.Add(new MyItem { Data = "V27.3", Const = "<5" });
                    TargetBoostDataListView.Items.Add(new MyItem { Data = "V27.82", Const = "<2" });
                    TargetBoostDataListView.Items.Add(new MyItem { Data = "Max", Const = "<28.6", ReferenceDose = 26 });
                    TargetBoostDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "~26", ReferenceDose = 26 });
                }

                TargetSPCDataListView.Items.Add(new MyItem { Data = "C24.7", Const = ">95" }); // SPC not in protocol should never hapenn
                TargetSPCDataListView.Items.Add(new MyItem { Data = "Max", Const = "<27.82", ReferenceDose = 26 }); // 107%
                TargetSPCDataListView.Items.Add(new MyItem { Data = "Avg", Const = "~26", ReferenceDose = 26 }); //

                IpsiLungDataListView.Items.Add(new MyItem { Data = "V8", Const = "<15", DTName = "IPSI-LUNG_V8" });
                //v1.0 IpsiLungDataListView.Items.Add(new MyItem { Data = "*V5", Const = "<35", DTName = "IPSI-LUNG_V5" }); // Not in protocol
                //v1.0 IpsiLungDataListView.Items.Add(new MyItem { Data = "*V3", Const = "<50", DTName = "IPSI-LUNG_V3" }); // Not in protocol
                //v1.0 IpsiLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<6.5", DTName = "IPSI-LUNG_Avg" }); // Not in protocol
                string const6 = "17", const7 = "26", const8 = "4";
                if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                {
                    const6 = "25";
                    const7 = "50";
                    const8 = "4.5";
                }

                IpsiLungDataListView.Items.Add(new MyItem { Data = "*V5", Const = "<" + const6, DTName = "IPSI-LUNG_V5" }); // NEW
                IpsiLungDataListView.Items.Add(new MyItem { Data = "*V3", Const = "<" + const7, DTName = "IPSI-LUNG_V3" }); // NEW
                IpsiLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<" + const8, DTName = "IPSI-LUNG_Avg" }); // NEW

                if ((bool)BilateralCheckBox.IsChecked)
                {
                    // clone IpsiLungDataListView
                    ContraLungDataListView.Items.Add(new MyItem { Data = "V8", Const = "<15" });
                    ContraLungDataListView.Items.Add(new MyItem { Data = "*V5", Const = "<" + const6 }); // Not in protocol
                    ContraLungDataListView.Items.Add(new MyItem { Data = "*V3", Const = "<" + const7, DTName = "CONTRA-LUNG_V3" }); // NEW
                    ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<" + const8, DTName = "CONTRA-LUNG_Avg" }); // NEW
                }
                else
                {
                    string const9 = "0", const10 = "0.2";
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                    {
                        const9 = "10";
                        const10 = "1.3";
                    }
                    //v1.0 ContraLungDataListView.Items.Add(new MyItem { Data = "*V3", Const = "<10", DTName = "CONTRA-LUNG_V3" });
                    //v1.0 ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "CONTRA-LUNG_Avg" }); // Not in protocol
                    ContraLungDataListView.Items.Add(new MyItem { Data = "*V3", Const = "<" + const9, DTName = "CONTRA-LUNG_V3" }); // NEW
                    ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<" + const10, DTName = "CONTRA-LUNG_Avg" }); // NEW
                }

                if (!(bool)BilateralCheckBox.IsChecked)
                {
                    string const11 = "0.1", const12 = "1.6", const13 = "0.2";
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                    {
                        const11 = "10";
                        const12 = "5";
                        const13 = "2";
                    }
                    //v1.0 ContraBreastDataListView.Items.Add(new MyItem { Data = "*V3", Const = "<10", DTName = "CONTRA-BREAST_V3" }); // Not in protocol
                    //v1.0 ContraBreastDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<1.6", DTName = "CONTRA-BREAST_Max" });
                    //v1.0 ContraBreastDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "CONTRA-BREAST_Avg" }); // Not in protocol
                    ContraBreastDataListView.Items.Add(new MyItem { Data = "*V3", Const = "<" + const11, DTName = "CONTRA-BREAST_V3" }); // NEW
                    ContraBreastDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<" + const12, DTName = "CONTRA-BREAST_Max" }); // NEW
                    ContraBreastDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<" + const13, DTName = "CONTRA-BREAST_Avg" }); // NEW
                }

                if ((bool)LeftBreastRadio.IsChecked || (bool)BilateralCheckBox.IsChecked)
                {
                    //v1.0 HeartDataListView.Items.Add(new MyItem { Data = "*V10", Const = "<5", DTName = "HEART_V10" }); // Not in protocol   
                    string const14 = "0.3", const15 = "0.6";
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                    {
                        const14 = "0.6";
                        const15 = "2";
                    }
                    HeartDataListView.Items.Add(new MyItem { Data = "*V10", Const = "<" + const14, DTName = "HEART_V10" }); // Not in protocol
                    HeartDataListView.Items.Add(new MyItem { Data = "V7", Const = "<5", DTName = "HEART_V7" });
                    HeartDataListView.Items.Add(new MyItem { Data = "V1.5", Const = "<30", DTName = "HEART_V1.5" });
                    HeartDataListView.Items.Add(new MyItem { Data = "Max", Const = "<27", DTName = "HEART_Max" }); // Not in protocol (UK consensus)
                    //v1.0 HeartDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "HEART_Avg" });
                    HeartDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<" + const15, DTName = "HEART_Avg" }); // NEW

                    LeftCoronaryDataListView.Items.Add(new MyItem { Data = "Max", Const = "<12", DTName = "LEFT-COR_Max" }); // Not in protocol
                    //v1.0 LeftCoronaryDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<5", DTName = "LEFT-COR_Avg" }); // Not in protocol
                    string const16 = "2";
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        const16 = "5";
                    LeftCoronaryDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<" + const16, DTName = "LEFT-COR_Avg" }); // NEW
                }
                else if ((bool)RightBreastRadio.IsChecked)
                {
                    //v1.0 HeartDataListView.Items.Add(new MyItem { Data = "*V10", Const = "<0", DTName = "HEART_V10" }); // Not in protocol
                    string const14 = "0", const15 = "0.3";
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                    {
                        const14 = "0.1";
                        const15 = "1.5";
                    }
                    HeartDataListView.Items.Add(new MyItem { Data = "*V10", Const = "<" + const14, DTName = "HEART_V10" }); // NEW
                    HeartDataListView.Items.Add(new MyItem { Data = "V7", Const = "<5", DTName = "HEART_V7" });
                    HeartDataListView.Items.Add(new MyItem { Data = "V1.5", Const = "<30", DTName = "HEART_V1.5" });
                    HeartDataListView.Items.Add(new MyItem { Data = "Max", Const = "<10", DTName = "HEART_Max" }); // Not in protocol
                    //v1.0 HeartDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "HEART_Avg" });
                    HeartDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<" + const15, DTName = "HEART_Avg" }); // NEW

                    LeftCoronaryDataListView.Items.Add(new MyItem { Data = "Max", Const = "<12", DTName = "LEFT-COR_Max" }); // Not in protocol
                    //v1.0 LeftCoronaryDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "LEFT-COR_Avg" }); // Not in protocol
                    string const16 = "0.1";
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        const16 = "2.5";
                    LeftCoronaryDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<" + const16, DTName = "LEFT-COR_Avg" }); // NEW
                }

                if (TargetSPCList.SelectedIndex != -1)
                {
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*V16", Const = "<50", DTName = "THYROID_V16" }); // Not in protocol
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<27.3", DTName = "THYROID_Max" }); // 105% not in protocol
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<13", DTName = "THYROID_Avg" }); // Not in protocol
                }
                else
                {
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*V16", Const = "<0", DTName = "THYROID_V16" }); // Not in protocol
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<1", DTName = "THYROID_Max" }); // Not in protocol
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<1", DTName = "THYROID_Avg" }); // Not in protocol
                }

                EsophagusDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<10", DTName = "ESOPHAGUS_V13" }); // Not in protocol
                EsophagusDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<26", DTName = "ESOPHAGUS_Max" }); // 100% not in protocol
                EsophagusDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<6.5", DTName = "ESOPHAGUS_Avg" }); // Not in protocol

                if ((bool)BilateralCheckBox.IsChecked)
                {
                    //v1.0 LiverDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<10", DTName = "LIVER_V13" }); // Not in protocol
                    //v1.0 LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<26", DTName = "LIVER_Max" }); // 100% not in protocol
                    //v1.0 LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "LIVER_Avg" }); // Not in protocol
                    string const17 = "3", const18 = "26", const19 = "1.5";
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                    {
                        const17 = "3";
                        const18 = "26";
                        const19 = "2.5";
                    }
                    LiverDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<" + const17, DTName = "LIVER_V13" }); // NEW
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<" + const18, DTName = "LIVER_Max" }); // NEW
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<" + const19, DTName = "LIVER_Avg" }); // NEW

                    StomachDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<5", DTName = "STOMACH_V13" }); // Not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<16", DTName = "STOMACH_Max" }); // 60% not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "STOMACH_Avg" }); // Not in protocol
                }
                else if ((bool)LeftBreastRadio.IsChecked)
                {
                    //v1.0 LiverDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<10", DTName = "LIVER_V13" }); // Not in protocol
                    //v1.0 LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<26", DTName = "LIVER_Max" }); // 100% not in protocol
                    //v1.0 LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "LIVER_Avg" }); // Not in protocol
                    string const17 = "0", const18 = "1", const19 = "0";
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                    {
                        const17 = "0";
                        const18 = "5";
                        const19 = "1";
                    }
                    LiverDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<" + const17, DTName = "LIVER_V13" }); // NEW
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<" + const18, DTName = "LIVER_Max" }); // NEW
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<" + const19, DTName = "LIVER_Avg" }); // NEW

                    StomachDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<5", DTName = "STOMACH_V13" }); // Not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<16", DTName = "STOMACH_Max" }); // 60% not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "STOMACH_Avg" }); // Not in protocol
                }
                else if ((bool)RightBreastRadio.IsChecked)
                {
                    //v1.0 LiverDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<10", DTName = "LIVER_V13" }); // Not in protocol
                    //v1.0 LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<26", DTName = "LIVER_Max" }); // 100% not in protocol
                    //v1.0 LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "LIVER_Avg" }); // Not in protocol
                    string const17 = "3", const18 = "26", const19 = "1.5";
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                    {
                        const17 = "3";
                        const18 = "26";
                        const19 = "2.5";
                    }
                    LiverDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<" + const17, DTName = "LIVER_V13" }); // Not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<" + const18, DTName = "LIVER_Max" }); // 100% not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<" + const19, DTName = "LIVER_Avg" }); // Not in protocol

                    StomachDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<0", DTName = "STOMACH_V13" }); // Not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<5", DTName = "STOMACH_Max" }); // 20% not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<1.5", DTName = "STOMACH_Avg" }); // 5% not in protocol
                }

                SkinDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<26.8" }); // 103% not in protocol
                SkinDataListView.Items.Add(new MyItem { Data = "*Dc1", Const = "<26.0" }); // 100% not in protocol
                SkinDataListView.Items.Add(new MyItem { Data = "*Dc10", Const = "<26.0" }); // 100% not in protocol
                SkinDataListView.Items.Add(new MyItem { Data = "*Dc125", Const = "<13.0" }); // 50% not in protocol

            } // END_FF
            else if (ProtocolComboBox.SelectedIndex == 2) // START_FAST 28.5 Gy
            {
                if (BoostCheckBox.IsChecked == true)
                {
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "C27.08", Const = ">95", DTName = "BREAST_C27.08" });
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "*V29.93", Const = "<50", DTName = "BREAST_V29.93" }); // From RTOG1005
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "*V30.5", Const = "<30", DTName = "BREAST_V30.5" }); // From RTOG1005
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "~28.5", DTName = "BREAST_Avg", ReferenceDose = 28.5 });
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "R27.08", Const = "<2", DTName = "BREAST_R27.08" });

                    TargetBoostDataListView.Items.Add(new MyItem { Data = "C29.45", Const = ">95", DTName = "BOOST_C29.45" });
                    TargetBoostDataListView.Items.Add(new MyItem { Data = "V32.55", Const = "<5", DTName = "BOOST_V32.55" });
                    TargetBoostDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<32.55", DTName = "BOOST_Max", ReferenceDose = 31 }); //105% 
                    TargetBoostDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "~31", DTName = "BOOST_Avg", ReferenceDose = 31 });
                }
                else
                {
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "C27.08", Const = ">95", DTName = "BREAST_C27.08" });
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "*V29.93", Const = "<3", DTName = "BREAST_V29.93" }); // 3% arbitrary
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "V30.5", Const = "<0", DTName = "BREAST_V30.5" });
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "Max", Const = "<30.5", DTName = "BREAST_Max", ReferenceDose = 28.5 }); // 107% (FAST protocol says to use full compensation if dose 107%)
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "~28.5", DTName = "BREAST_Avg", ReferenceDose = 28.5 });
                    TargetBreastDataListView.Items.Add(new MyItem { Data = "R27.08", Const = "<2", DTName = "BREAST_R27.08" });

                    TargetBoostDataListView.Items.Add(new MyItem { Data = "C27.08", Const = ">95" });
                    TargetBoostDataListView.Items.Add(new MyItem { Data = "*V29.93", Const = "<3" }); // 3% arbitrary
                    TargetBoostDataListView.Items.Add(new MyItem { Data = "V30.5", Const = "<0" });
                    TargetBoostDataListView.Items.Add(new MyItem { Data = "Max", Const = "<30.5", ReferenceDose = 28.5 });
                    TargetBoostDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "~28.5", ReferenceDose = 28.5 });
                }

                TargetSPCDataListView.Items.Add(new MyItem { Data = "C27.08", Const = ">95" }); // SPC not in protocol but optional
                TargetSPCDataListView.Items.Add(new MyItem { Data = "Max", Const = "<30.5", ReferenceDose = 28.5 }); // 107%
                TargetSPCDataListView.Items.Add(new MyItem { Data = "Avg", Const = "~28.5", ReferenceDose = 28.5 }); //

                IpsiLungDataListView.Items.Add(new MyItem { Data = "V8", Const = "<15", DTName = "IPSI-LUNG_V8" });
                IpsiLungDataListView.Items.Add(new MyItem { Data = "*V5", Const = "<35", DTName = "IPSI-LUNG_V5" }); // Not in protocol
                IpsiLungDataListView.Items.Add(new MyItem { Data = "*V3", Const = "<50", DTName = "IPSI-LUNG_V3" }); // Not in protocol
                IpsiLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<6.5", DTName = "IPSI-LUNG_Avg" }); // Not in protocol

                if ((bool)BilateralCheckBox.IsChecked)
                {
                    // clone IpsiLungDataListView
                    ContraLungDataListView.Items.Add(new MyItem { Data = "V8", Const = "<15" });
                    ContraLungDataListView.Items.Add(new MyItem { Data = "*V5", Const = "<35" }); // Not in protocol
                    ContraLungDataListView.Items.Add(new MyItem { Data = "*V3", Const = "<50", DTName = "CONTRA-LUNG_V3" }); // Not in protocol
                    ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<6.5", DTName = "CONTRA-LUNG_Avg" }); // Not in protocol
                }
                else
                {
                    ContraLungDataListView.Items.Add(new MyItem { Data = "*V3", Const = "<10", DTName = "CONTRA-LUNG_V3" }); // Not in protocol
                    ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "CONTRA-LUNG_Avg" }); // Not in protocol
                }

                if (!(bool)BilateralCheckBox.IsChecked)
                {
                    ContraBreastDataListView.Items.Add(new MyItem { Data = "*V3", Const = "<10", DTName = "CONTRA-BREAST_V3" }); // Not in protocol
                    ContraBreastDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<1.6", DTName = "CONTRA-BREAST_Max" });
                    ContraBreastDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "CONTRA-BREAST_Avg" }); // Not in protocol
                }

                if ((bool)LeftBreastRadio.IsChecked || (bool)BilateralCheckBox.IsChecked)
                {
                    HeartDataListView.Items.Add(new MyItem { Data = "*V10", Const = "<5", DTName = "HEART_V10" }); // Not in protocol
                    HeartDataListView.Items.Add(new MyItem { Data = "V7", Const = "<5", DTName = "HEART_V7" });
                    HeartDataListView.Items.Add(new MyItem { Data = "V1.5", Const = "<30", DTName = "HEART_V1.5" });
                    HeartDataListView.Items.Add(new MyItem { Data = "Max", Const = "<27", DTName = "HEART_Max" }); // Not in protocol UK consensus
                    HeartDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "HEART_Avg" });

                    LeftCoronaryDataListView.Items.Add(new MyItem { Data = "Max", Const = "<12", DTName = "LEFT-COR_Max" }); // Not in protocol
                    LeftCoronaryDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<5", DTName = "LEFT-COR_Avg" }); // Not in protocol
                }
                else if ((bool)RightBreastRadio.IsChecked)
                {
                    HeartDataListView.Items.Add(new MyItem { Data = "*V10", Const = "<0", DTName = "HEART_V10" }); // Not in protocol
                    HeartDataListView.Items.Add(new MyItem { Data = "V7", Const = "<5", DTName = "HEART_V7" });
                    HeartDataListView.Items.Add(new MyItem { Data = "V1.5", Const = "<30", DTName = "HEART_V1.5" });
                    HeartDataListView.Items.Add(new MyItem { Data = "Max", Const = "<10", DTName = "HEART_Max" }); // Not in protocol
                    HeartDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "HEART_Avg" });

                    LeftCoronaryDataListView.Items.Add(new MyItem { Data = "Max", Const = "<12", DTName = "LEFT-COR_Max" }); // Not in protocol
                    LeftCoronaryDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "LEFT-COR_Avg" }); // Not in protocol
                }

                if (TargetSPCList.SelectedIndex != -1)
                {
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*V16", Const = "<50", DTName = "THYROID_V16" }); // Not in protocol
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<29.9", DTName = "THYROID_Max" }); // 105% not in protocol
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<13", DTName = "THYROID_Avg" }); // Not in protocol
                }
                else
                {
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*V16", Const = "<0", DTName = "THYROID_V16" }); // Not in protocol
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<1", DTName = "THYROID_Max" }); // Not in protocol
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<1", DTName = "THYROID_Avg" }); // Not in protocol
                }

                EsophagusDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<10", DTName = "ESOPHAGUS_V13" }); // Not in protocol
                EsophagusDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<26", DTName = "ESOPHAGUS_Max" }); // 100% Not in protocol
                EsophagusDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<6.5", DTName = "ESOPHAGUS_Avg" }); // Not in protocol

                if ((bool)BilateralCheckBox.IsChecked)
                {
                    LiverDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<10", DTName = "LIVER_V13" }); // Not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<28.5", DTName = "LIVER_Max" }); // 100% Not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "LIVER_Max" }); // Not in protocol

                    StomachDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<5", DTName = "STOMACH_V13" }); // Not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<17", DTName = "STOMACH_Max" }); // 60% not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "STOMACH_Avg" }); // Not in protocol
                }
                else if ((bool)LeftBreastRadio.IsChecked)
                {
                    LiverDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<0", DTName = "LIVER_V13" }); // Not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<5", DTName = "LIVER_Max" }); // 20% not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<1.5", DTName = "LIVER_Avg" }); // 5% not in protocol

                    StomachDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<5", DTName = "STOMACH_V13" }); // Not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<17", DTName = "STOMACH_Max" }); // 60% not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "STOMACH_Avg" }); // Not in protocol
                }
                else if ((bool)RightBreastRadio.IsChecked)
                {
                    LiverDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<10", DTName = "LIVER_V13" }); // Not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<28.5", DTName = "LIVER_Max" }); // 100% Not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "LIVER_Max" }); // Not in protocol

                    StomachDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<0", DTName = "STOMACH_V13" }); // Not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<5", DTName = "STOMACH_Max" }); // 20% not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<1.5", DTName = "STOMACH_Avg" }); // 5% not in protocol
                }

                SkinDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<29.4" }); // 103% not in protocol
                SkinDataListView.Items.Add(new MyItem { Data = "*Dc1", Const = "<28.5" }); // 100% not in protocol
                SkinDataListView.Items.Add(new MyItem { Data = "*Dc10", Const = "<28.5" }); // 100% not in protocol
                SkinDataListView.Items.Add(new MyItem { Data = "*Dc125", Const = "<14.3" }); // 50% not in protocol
            } // END_FAST
            else if (ProtocolComboBox.SelectedIndex == 3) // START_EUROPAWBI15 40.05 Gy
            {
                TargetBreastDataListView.Items.Add(new MyItem { Data = "C38.05", Const = ">95/90", DTName = "BREAST_C38.05" });
                TargetBreastDataListView.Items.Add(new MyItem { Data = "*C36.05", Const = ">90", DTName = "BREAST_C36.05" }); // From RTOG1005 not in protocol
                TargetBreastDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<42.85", DTName = "BREAST_Max", ReferenceDose = 40.05 }); // 107% not in protocol
                TargetBreastDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "~40.05", DTName = "BREAST_Avg", ReferenceDose = 40.05 });
                TargetBreastDataListView.Items.Add(new MyItem { Data = "*R38.05", Const = "<2", DTName = "BREAST_R38.05" }); // From RTOG1005 not in protocol

                TargetBoostDataListView.Items.Add(new MyItem { Data = "C38.05", Const = ">95" }); // Boost not in protocol
                TargetBoostDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<42.85", ReferenceDose = 40.05 }); // 107% not in protocol
                TargetBoostDataListView.Items.Add(new MyItem { Data = "Avg", Const = "~40.05", ReferenceDose = 40.05 }); //

                TargetSPCDataListView.Items.Add(new MyItem { Data = "C38.05", Const = ">95" }); // SPC not in protocol
                TargetSPCDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<42.85", ReferenceDose = 40.05 }); // 107%
                TargetSPCDataListView.Items.Add(new MyItem { Data = "Avg", Const = "~40.05", ReferenceDose = 40.05 }); //

                // IPSI LUNG

                IpsiLungDataListView.Items.Add(new MyItem { Data = "V18", Const = "<15", DTName = "IPSI-LUNG_V18" });
                IpsiLungDataListView.Items.Add(new MyItem { Data = "*V8", Const = "<35/40", DTName = "IPSI-LUNG_V8" }); // From RTOG1005 not in protocol
                IpsiLungDataListView.Items.Add(new MyItem { Data = "*V4", Const = "<50/55", DTName = "IPSI-LUNG_V4" }); // From RTOG1005 not in protocol
                // v1.0 IpsiLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<10", DTName = "IPSI-LUNG_Avg" }); // Not in protocol
                if (SPCNCheckBox.IsChecked == true)
                {
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        IpsiLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<8", DTName = "IPSI-LUNG_Avg" }); // Not in protocol; v1.1
                    else
                        IpsiLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<7", DTName = "IPSI-LUNG_Avg" }); // Not in protocol; v1.1
                }
                else
                {
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        IpsiLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<7", DTName = "IPSI-LUNG_Avg" }); // Not in protocol; v1.1
                    else
                        IpsiLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<6", DTName = "IPSI-LUNG_Avg" }); // Not in protocol; v1.1
                }

                // CONTRA LUNG

                if ((bool)BilateralCheckBox.IsChecked)
                {
                    // clone IpsiLungDataListView
                    ContraLungDataListView.Items.Add(new MyItem { Data = "V18", Const = "<15" });
                    ContraLungDataListView.Items.Add(new MyItem { Data = "*V8", Const = "<35/40" }); // From RTOG1005 not in protocol
                    ContraLungDataListView.Items.Add(new MyItem { Data = "*V4", Const = "<50/55", DTName = "CONTRA-LUNG_V4" }); // From RTOG1005 not in protocol
                    // v1.0 ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<10", DTName = "CONTRA-LUNG_Avg" }); // Not in protocol
                    if (SPCNCheckBox.IsChecked == true)
                    {
                        if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                            ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<8", DTName = "CONTRA-LUNG_Avg" }); // Not in protocol; v1.1
                        else
                            ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<7", DTName = "CONTRA-LUNG_Avg" }); // Not in protocol; v1.1
                    }
                    else
                    {
                        if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                            ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<7", DTName = "CONTRA-LUNG_Avg" }); // Not in protocol; v1.1
                        else
                            ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<6", DTName = "CONTRA-LUNG_Avg" }); // Not in protocol; v1.1
                    }

                }
                else
                {
                    ContraLungDataListView.Items.Add(new MyItem { Data = "*V4", Const = "<10/15", DTName = "CONTRA-LUNG_V4" }); // From RTOG1005 not in protocol
                    ContraLungDataListView.Items.Add(new MyItem { Data = "V2.5", Const = "<15", DTName = "CONTRA-LUNG_V2.5" });
                    // v1.0 ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3", DTName = "CONTRA-LUNG_Avg" }); // Not in protocol
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "CONTRA-LUNG_Avg" }); // Not in protocol; v1.1
                    else
                        ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<0.3", DTName = "CONTRA-LUNG_Avg" }); // Not in protocol; v1.1
                }

                // CONTRA BREAST

                if (!(bool)BilateralCheckBox.IsChecked)
                {
                    // v1.0 ContraBreastDataListView.Items.Add(new MyItem { Data = "*V4", Const = "<10/15", DTName = "CONTRA-BREAST_V4" }); // Not in protocol
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        ContraBreastDataListView.Items.Add(new MyItem { Data = "*V4", Const = "<10/15", DTName = "CONTRA-BREAST_V4" }); // Not in protocol
                    else
                        ContraBreastDataListView.Items.Add(new MyItem { Data = "*V4", Const = "<1", DTName = "CONTRA-BREAST_V4" }); // Not in protocol; v1.1
                    ContraBreastDataListView.Items.Add(new MyItem { Data = "*V2.4", Const = "<5", DTName = "CONTRA-BREAST_V2.4" }); // From RTOG1005 not in protocol
                    ContraBreastDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<2.4/3.84", DTName = "CONTRA-BREAST_Max" }); // From RTOG1005 not in protocol
                    ContraBreastDataListView.Items.Add(new MyItem { Data = "Avg", Const = "<1.5", DTName = "CONTRA-BREAST_Avg" });
                }

                // HEART AND LEFT_CORONARY

                if ((bool)LeftBreastRadio.IsChecked || (bool)BilateralCheckBox.IsChecked)
                {
                    HeartDataListView.Items.Add(new MyItem { Data = "V10", Const = "<5", DTName = "HEART_V10" });
                    HeartDataListView.Items.Add(new MyItem { Data = "V2", Const = "<30", DTName = "HEART_V2" });
                    // v1.0 HeartDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<28", DTName = "HEART_Max" }); //70% Not in protocol
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        HeartDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<20", DTName = "HEART_Max" }); //Not in protocol; v1.1
                    else
                        HeartDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<25", DTName = "HEART_Max" }); //Not in protocol; v1.1

                    HeartDataListView.Items.Add(new MyItem { Data = "Avg", Const = "<2.5", DTName = "HEART_Avg" });

                    LeftCoronaryDataListView.Items.Add(new MyItem { Data = "Max", Const = "<16", DTName = "LEFT-COR_Max" }); // Not in protocol
                    // v1.0 LeftCoronaryDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<8", DTName = "LEFT-COR_Avg" }); // Not in protocol
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        LeftCoronaryDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<6", DTName = "LEFT-COR_Avg" }); // Not in protocol: v1.1
                    else
                        LeftCoronaryDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3", DTName = "LEFT-COR_Avg" }); // Not in protocol: v1.1
                }
                else if ((bool)RightBreastRadio.IsChecked)
                {
                    HeartDataListView.Items.Add(new MyItem { Data = "V10", Const = "<5", DTName = "HEART_V10" });
                    HeartDataListView.Items.Add(new MyItem { Data = "V2", Const = "<30", DTName = "HEART_V2" });
                    // v1.0 HeartDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<16", DTName = "HEART_Max" });  // Not in protocol
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        HeartDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<10", DTName = "HEART_Max" });  // Not in protocol; v1.1
                    else
                        HeartDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<4", DTName = "HEART_Max" });  // Not in protocol; v1.1

                    HeartDataListView.Items.Add(new MyItem { Data = "Avg", Const = "<2.5", DTName = "HEART_Avg" });

                    LeftCoronaryDataListView.Items.Add(new MyItem { Data = "Max", Const = "<16", DTName = "LEFT-COR_Max" }); // Not in protocol
                    // v1.0 LeftCoronaryDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<5", DTName = "LEFT-COR_Avg" }); // Not in protocol
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        LeftCoronaryDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3", DTName = "LEFT-COR_Avg" }); // Not in protocol; v1.1
                    else
                        LeftCoronaryDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<0.3", DTName = "LEFT-COR_Avg" }); // Not in protocol; v1.1
                }

                // THYROID

                if (SPCNCheckBox.IsChecked == true)
                {
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*V24", Const = "<50", DTName = "THYROID_V24" }); // Not in protocol
                    // v.1.0 ThyroidDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<42.05", DTName = "THYROID_Max" }); // 105% not in protocol
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<41.3", DTName = "THYROID_Max" }); // 103% not in protocol; v1.1
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<20", DTName = "THYROID_Avg" }); // Not in protocol
                }
                else
                {
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*V24", Const = "<0", DTName = "THYROID_V24" });
                    ThyroidDataListView.Items.Add(new MyItem { Data = "Max", Const = "<1/1.44", DTName = "THYROID_Max" });
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<1", DTName = "THYROID_Avg" }); // Not in protocol
                }

                // ESOPHAGUS

                // v1.0 EsophagusDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<10", DTName = "ESOPHAGUS_V20" }); // Not in protocol
                if (SPCNCheckBox.IsChecked == true)
                {
                    if ((bool)LeftBreastRadio.IsChecked || (bool)BilateralCheckBox.IsChecked)
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<10", DTName = "ESOPHAGUS_V20" }); // Not in protocol; v1.1
                    else if ((bool)RightBreastRadio.IsChecked)
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<5", DTName = "ESOPHAGUS_V20" }); // Not in protocol; v1.1
                }
                else
                {
                    if ((bool)LeftBreastRadio.IsChecked || (bool)BilateralCheckBox.IsChecked)
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<0", DTName = "ESOPHAGUS_V20" }); // Not in protocol; v1.1
                    else if ((bool)RightBreastRadio.IsChecked)
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<0", DTName = "ESOPHAGUS_V20" }); // Not in protocol; v1.1
                }

                // v1.0 EsophagusDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<42.05", DTName = "ESOPHAGUS_Max" }); // 105% not in protocol
                if (SPCNCheckBox.IsChecked == true)
                {
                    if ((bool)LeftBreastRadio.IsChecked || (bool)BilateralCheckBox.IsChecked)
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<38", DTName = "ESOPHAGUS_Max" }); // Not in protocol; v1.1
                    else if ((bool)RightBreastRadio.IsChecked)
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<30", DTName = "ESOPHAGUS_Max" }); // Not in protocol; v1.1
                }
                else
                {
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<6", DTName = "ESOPHAGUS_Max" }); // Not in protocol; v1.1
                    else
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<0", DTName = "ESOPHAGUS_Max" }); // Not in protocol; v1.1
                }


                // v1.0 EsophagusDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<10", DTName = "ESOPHAGUS_Avg" }); // Not in protocol
                if (SPCNCheckBox.IsChecked == true)
                {
                    if ((bool)LeftBreastRadio.IsChecked || (bool)BilateralCheckBox.IsChecked)
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<8", DTName = "ESOPHAGUS_Avg" }); // Not in protocol; v1.1
                    else if ((bool)RightBreastRadio.IsChecked)
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<7", DTName = "ESOPHAGUS_Avg" }); // Not in protocol; v1.1
                }
                else
                {
                    if (TechniqueVMAT.IsChecked == true || TechniqueIMRT.IsChecked == true)
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3", DTName = "ESOPHAGUS_Avg" }); // Not in protocol; v1.1
                    else
                        EsophagusDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<0.5", DTName = "ESOPHAGUS_Avg" }); // Not in protocol; v1.1
                }

                // LIVER

                // v1.0
                /*
                if ((bool)BilateralCheckBox.IsChecked)
                {
                    LiverDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<10", DTName = "LIVER_V20" }); // Not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<40.05", DTName = "LIVER_Max" }); // 100% not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<5", DTName = "LIVER_Avg" }); // Not in protocol

                    StomachDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<5", DTName = "STOMACH_V20" }); // Not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<24", DTName = "STOMACH_Max" }); // 60% not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<5", DTName = "STOMACH_Avg" }); // Not in protocol
                }
                else if ((bool)LeftBreastRadio.IsChecked)
                {
                    LiverDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<0", DTName = "LIVER_V20" }); // Not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<8", DTName = "LIVER_Max" }); // 20% not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "LIVER_Avg" }); // 5% not in protocol

                    StomachDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<5", DTName = "STOMACH_V20" }); // Not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<24", DTName = "STOMACH_Max" }); // 60% not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<5", DTName = "STOMACH_Avg" }); // Not in protocol
                }
                else if ((bool)RightBreastRadio.IsChecked)
                {
                    LiverDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<10", DTName = "LIVER_V20" }); // Not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<40.05", DTName = "LIVER_Max" }); // 100% not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<5", DTName = "LIVER_Avg" }); // Not in protocol

                    StomachDataListView.Items.Add(new MyItem { Data = "*V20", Const = "0", DTName = "STOMACH_V20" }); // Not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<8", DTName = "STOMACH_Max" }); // 20% not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "STOMACH_Avg" }); // 5% not in protocol
                }
                */
                // v1.1
                if ((bool)BilateralCheckBox.IsChecked)
                {
                    LiverDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<2", DTName = "LIVER_V20" }); // Not in protocol; v1.1
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<38", DTName = "LIVER_Max" }); // Not in protocol; v1.1
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "LIVER_Avg" }); // Not in protocol; v1.1

                    StomachDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<2", DTName = "STOMACH_V20" }); // Not in protocol; v1.1
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<16", DTName = "STOMACH_Max" }); // Not in protocol; v1.1
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "STOMACH_Avg" }); // Not in protocol; v1.1
                }
                else if ((bool)LeftBreastRadio.IsChecked)
                {
                    LiverDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<0", DTName = "LIVER_V20" }); // Not in protocol; v1.1
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<8", DTName = "LIVER_Max" }); // Not in protocol; v1.1
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<1", DTName = "LIVER_Avg" }); // Not in protocol; v1.1

                    StomachDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<2", DTName = "STOMACH_V20" }); // Not in protocol; v1.1
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<16", DTName = "STOMACH_Max" }); // Not in protocol; v1.1
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "STOMACH_Avg" }); // Not in protocol; v1.1
                }
                else if ((bool)RightBreastRadio.IsChecked)
                {
                    LiverDataListView.Items.Add(new MyItem { Data = "*V20", Const = "<2", DTName = "LIVER_V20" }); // Not in protocol; v1.1
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<38", DTName = "LIVER_Max" }); // Not in protocol; v1.1
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "LIVER_Avg" }); // Not in protocol; v1.1

                    StomachDataListView.Items.Add(new MyItem { Data = "*V20", Const = "0", DTName = "STOMACH_V20" }); // Not in protocol; v1.1
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<4", DTName = "STOMACH_Max" }); // Not in protocol; v1.1
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<1", DTName = "STOMACH_Avg" }); // Not in protocol; v1.1
                }

                SkinDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<41.3" }); // 103% not in protocol
                SkinDataListView.Items.Add(new MyItem { Data = "*Dc1", Const = "<40.05" }); // 100% not in protocol
                SkinDataListView.Items.Add(new MyItem { Data = "*Dc10", Const = "<40.05" }); // 100% not in protocol
                SkinDataListView.Items.Add(new MyItem { Data = "*Dc125", Const = "<20.0" }); // 50% not in protocol

            } // END_EUROPAWBI15
            else if (ProtocolComboBox.SelectedIndex == 4) // START_EUROPAWBI5 26 Gy
            {
                TargetBreastDataListView.Items.Add(new MyItem { Data = "C24.7", Const = ">95/90", DTName = "BREAST_C24.7" });
                TargetBreastDataListView.Items.Add(new MyItem { Data = "V27.3", Const = "<5", DTName = "BREAST_V27.3" });
                TargetBreastDataListView.Items.Add(new MyItem { Data = "V27.82", Const = "<2", DTName = "BREAST_V27.82" });
                TargetBreastDataListView.Items.Add(new MyItem { Data = "Max", Const = "<28.6", DTName = "BREAST_Max", ReferenceDose = 26 });
                TargetBreastDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "~26", DTName = "BREAST_Avg", ReferenceDose = 26 });
                TargetBreastDataListView.Items.Add(new MyItem { Data = "*R24.7", Const = "<2", DTName = "BREAST_R24.7" });

                TargetBoostDataListView.Items.Add(new MyItem { Data = "C24.7", Const = ">95" }); // Boost not in protocol should never happen
                TargetBoostDataListView.Items.Add(new MyItem { Data = "Max", Const = "<27.3", ReferenceDose = 26 }); // 105%
                TargetBoostDataListView.Items.Add(new MyItem { Data = "Avg", Const = "~26", ReferenceDose = 26 }); //

                TargetSPCDataListView.Items.Add(new MyItem { Data = "C24.7", Const = ">95" }); // SPC not in protocol should never happen
                TargetSPCDataListView.Items.Add(new MyItem { Data = "Max", Const = "<27.3", ReferenceDose = 26 }); // 105%
                TargetSPCDataListView.Items.Add(new MyItem { Data = "Avg", Const = "~26", ReferenceDose = 26 }); //

                IpsiLungDataListView.Items.Add(new MyItem { Data = "V8", Const = "<15/17", DTName = "IPSI-LUNG_V8" });
                IpsiLungDataListView.Items.Add(new MyItem { Data = "*V5", Const = "<35", DTName = "IPSI-LUNG_V5" }); // Not in protocol
                IpsiLungDataListView.Items.Add(new MyItem { Data = "*V3", Const = "<50", DTName = "IPSI-LUNG_V3" }); // Not in protocol
                IpsiLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<6.5", DTName = "IPSI-LUNG_Avg" }); // Not in protocol

                if ((bool)BilateralCheckBox.IsChecked)
                {
                    ContraLungDataListView.Items.Add(new MyItem { Data = "V8", Const = "<15/17" });
                    ContraLungDataListView.Items.Add(new MyItem { Data = "*V5", Const = "<35" }); // Not in protocol
                    ContraLungDataListView.Items.Add(new MyItem { Data = "*V3", Const = "<50", DTName = "CONTRA-LUNG_V3" }); // Not in protocol
                    ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<6.5", DTName = "CONTRA-LUNG_Avg" }); // Not in protocol
                }
                else
                {
                    ContraLungDataListView.Items.Add(new MyItem { Data = "*V3", Const = "<10", DTName = "CONTRA-LUNG_V3" }); // Not in protocol
                    ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "CONTRA-LUNG_Avg" }); // Not in protocol
                }

                ContraBreastDataListView.Items.Add(new MyItem { Data = "*V3", Const = "<10", DTName = "CONTRA-BREAST_V3" }); // Not in protocol
                ContraBreastDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<1.6", DTName = "CONTRA-BREAST_Max" }); // Not in protocol
                ContraBreastDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "CONTRA-BREAST_Avg" }); // Not in protocol

                if ((bool)LeftBreastRadio.IsChecked || (bool)BilateralCheckBox.IsChecked)
                {
                    HeartDataListView.Items.Add(new MyItem { Data = "*V10", Const = "<5", DTName = "HEART_V10" }); // Not in protocol
                    HeartDataListView.Items.Add(new MyItem { Data = "V7", Const = "<5", DTName = "HEART_V7" });
                    HeartDataListView.Items.Add(new MyItem { Data = "V1.5", Const = "<30", DTName = "HEART_V1.5" });
                    HeartDataListView.Items.Add(new MyItem { Data = "Max", Const = "<27", DTName = "HEART_Max" }); // Not in protocol UK consensus
                    HeartDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "HEART_Avg" });

                    LeftCoronaryDataListView.Items.Add(new MyItem { Data = "Max", Const = "<12", DTName = "LEFT-COR_Max" }); // Not in protocol
                    LeftCoronaryDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<5", DTName = "LEFT-COR_Avg" }); // Not in protocol
                }
                else if ((bool)RightBreastRadio.IsChecked)
                {
                    HeartDataListView.Items.Add(new MyItem { Data = "*V10", Const = "<0", DTName = "HEART_V10" }); // Not in protocol
                    HeartDataListView.Items.Add(new MyItem { Data = "V7", Const = "<5", DTName = "HEART_V7" });
                    HeartDataListView.Items.Add(new MyItem { Data = "V1.5", Const = "<30", DTName = "HEART_V1.5" });
                    HeartDataListView.Items.Add(new MyItem { Data = "Max", Const = "<10", DTName = "HEART_Max" }); // Not in protocol
                    HeartDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "HEART_Avg" });

                    LeftCoronaryDataListView.Items.Add(new MyItem { Data = "Max", Const = "<12", DTName = "LEFT-COR_Max" }); // Not in protocol
                    LeftCoronaryDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "LEFT-COR_Avg" }); // Not in protocol
                }

                if (TargetSPCList.SelectedIndex != -1)
                {
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*V16", Const = "<50", DTName = "THYROID_V16" }); // Not in protocol
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<27.3", DTName = "THYROID_Max" }); // 105% not in protocol
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<13", DTName = "THYROID_Avg" }); // Not in protocol
                }
                else
                {
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*V16", Const = "<0", DTName = "THYROID_V16" }); // Not in protocol
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<1", DTName = "THYROID_Max" }); // Not in protocol
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<1", DTName = "THYROID_Avg" }); // Not in protocol
                }

                EsophagusDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<10", DTName = "ESOPHAGUS_V13" }); // Not in protocol
                EsophagusDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<26", DTName = "ESOPHAGUS_Max" }); // 100% not in protocol
                EsophagusDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<6.5", DTName = "ESOPHAGUS_Avg" }); // Not in protocol

                if ((bool)BilateralCheckBox.IsChecked)
                {
                    LiverDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<10", DTName = "LIVER_V13" }); // Not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<26", DTName = "LIVER_Max" }); // 100% not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "LIVER_Avg" }); // Not in protocol

                    StomachDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<5", DTName = "STOMACH_V13" }); // Not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<16", DTName = "STOMACH_Max" }); // 60% not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "STOMACH_Avg" }); // Not in protocol
                }
                else if ((bool)LeftBreastRadio.IsChecked)
                {
                    LiverDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<0", DTName = "LIVER_V13" }); // Not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<5", DTName = "LIVER_Max" }); // 20% not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<1.5", DTName = "LIVER_Avg" }); // 5% in protocol

                    StomachDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<5", DTName = "STOMACH_V13" }); // Not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<16", DTName = "STOMACH_Max" }); // 60% not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "STOMACH_Avg" }); // Not in protocol
                }
                else if ((bool)RightBreastRadio.IsChecked)
                {
                    LiverDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<10", DTName = "LIVER_V13" }); // Not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<26", DTName = "LIVER_Max" }); // 100% not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "LIVER_Avg" }); // Not in protocol

                    StomachDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<0", DTName = "STOMACH_V13" }); // Not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<5", DTName = "STOMACH_Max" }); // 20% not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<1.5", DTName = "STOMACH_Avg" }); // 5% not in protocol
                }

                SkinDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<26.8" }); // 103% not in protocol
                SkinDataListView.Items.Add(new MyItem { Data = "*Dc1", Const = "<26.0" }); // 100% not in protocol
                SkinDataListView.Items.Add(new MyItem { Data = "*Dc10", Const = "<26.0" }); // 100% not in protocol
                SkinDataListView.Items.Add(new MyItem { Data = "*Dc125", Const = "<13.0" }); // 50% not in protocol

            } // END_EUROPAWBI5
            else if (ProtocolComboBox.SelectedIndex == 5) // START_EUROPAPBI5 30 Gy
            {
                // Refers to Ipsi Breast
                TargetBreastDataListView.Items.Add(new MyItem { Data = "V15", Const = "<50/60", DTName = "IPSI_BREAST_V15" });
                TargetBreastDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<15", DTName = "IPSI_BREAST_Avg" }); // Not in protocol

                TargetBoostDataListView.Items.Add(new MyItem { Data = "C28.5", Const = ">95", DTName = "PBI_PTV_C28.5" });
                TargetBoostDataListView.Items.Add(new MyItem { Data = "V31.5", Const = "<5/10", DTName = "PBI_PTV_V31.5" });
                TargetBoostDataListView.Items.Add(new MyItem { Data = "Max", Const = "<33", DTName = "PBI_PTV_Max", ReferenceDose = 30 });
                TargetBoostDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "~30", DTName = "PBI_PTV_Avg", ReferenceDose = 30 });
                TargetBoostDataListView.Items.Add(new MyItem { Data = "*R28.5", Const = "<2", DTName = "PBI_PTV_R28.5" }); // Not in protocol

                // Refers to Ipsi Breast - PTV structure if any
                TargetSPCDataListView.Items.Add(new MyItem { Data = "*V24", Const = "<20", DTName = "IPSI-PTV_V24" }); // Not in protocol
                TargetSPCDataListView.Items.Add(new MyItem { Data = "*V15", Const = "<50/60", DTName = "IPSI-PTV_V15" }); // Not in protocol
                TargetSPCDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<13", DTName = "IPSI-PTV_Avg" }); // Not in protocol

                IpsiLungDataListView.Items.Add(new MyItem { Data = "V10", Const = "<20", DTName = "IPSI-LUNG_V10" });
                IpsiLungDataListView.Items.Add(new MyItem { Data = "*V8", Const = "<15", DTName = "IPSI-LUNG_V8" }); // Not in protocol
                IpsiLungDataListView.Items.Add(new MyItem { Data = "*V3", Const = "<50", DTName = "IPSI-LUNG_V3" }); // Not in protocol
                IpsiLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<6.5", DTName = "IPSI-LUNG_Avg" }); // Not in protocol

                if ((bool)BilateralCheckBox.IsChecked)
                {
                    ContraLungDataListView.Items.Add(new MyItem { Data = "V10", Const = "<20" });
                    ContraLungDataListView.Items.Add(new MyItem { Data = "*V8", Const = "<15" }); // Not in protocol
                    ContraLungDataListView.Items.Add(new MyItem { Data = "*V3", Const = "<50" }); // Not in protocol
                    ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<6.5", DTName = "CONTRA-LUNG_Avg" }); // Not in protocol
                }
                else
                {
                    ContraLungDataListView.Items.Add(new MyItem { Data = "V5", Const = "<10", DTName = "CONTRA-LUNG_V5" });
                    ContraLungDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "CONTRA-LUNG_Avg" }); // Not in protocol
                }

                if (!(bool)BilateralCheckBox.IsChecked)
                {
                    ContraBreastDataListView.Items.Add(new MyItem { Data = "*V3", Const = "<10", DTName = "CONTRA-BREAST_V3" }); // Not in protocol
                    ContraBreastDataListView.Items.Add(new MyItem { Data = "Max", Const = "<2", DTName = "CONTRA-BREAST_Max" });
                    ContraBreastDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "CONTRA-BREAST_Avg" }); // Not in protocol
                }

                if ((bool)LeftBreastRadio.IsChecked || (bool)BilateralCheckBox.IsChecked)
                {
                    HeartDataListView.Items.Add(new MyItem { Data = "V3", Const = "<10", DTName = "HEART_V3" });
                    HeartDataListView.Items.Add(new MyItem { Data = "*V1.5", Const = "<30", DTName = "HEART_V1.5" });
                    HeartDataListView.Items.Add(new MyItem { Data = "Max", Const = "<18.5", DTName = "HEART_Max" }); // Not in protocol; value well below UK consensus
                    HeartDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "HEART_Avg" });

                    LeftCoronaryDataListView.Items.Add(new MyItem { Data = "Max", Const = "<12", DTName = "LEFT-COR_Max" }); // Not in protocol
                    LeftCoronaryDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<5", DTName = "LEFT-COR_Avg" }); // Not in protocol
                }
                else if ((bool)RightBreastRadio.IsChecked)
                {
                    HeartDataListView.Items.Add(new MyItem { Data = "V3", Const = "<10", DTName = "HEART_V3" });
                    HeartDataListView.Items.Add(new MyItem { Data = "*V1.5", Const = "<30", DTName = "HEART_V1.5" });
                    HeartDataListView.Items.Add(new MyItem { Data = "Max", Const = "<10", DTName = "HEART_Max" }); // Not in protocol
                    HeartDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<2", DTName = "HEART_Avg" });

                    LeftCoronaryDataListView.Items.Add(new MyItem { Data = "Max", Const = "<12", DTName = "LEFT-COR_Max" }); // Not in protocol
                    LeftCoronaryDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "LEFT-COR_Avg" }); // Not in protocol
                }

                if (TargetSPCList.SelectedIndex != -1)
                {
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*V16", Const = "<50", DTName = "THYROID_V16" }); // Not in protocol
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<27.3", DTName = "THYROID_Max" }); // 105% not in protocol
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<13", DTName = "THYROID_Avg" }); // Not in protocol
                }
                else
                {
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*V16", Const = "<0", DTName = "THYROID_V16" }); // Not in protocol
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<1", DTName = "THYROID_Max" }); // Not in protocol
                    ThyroidDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<1", DTName = "THYROID_Avg" }); // Not in protocol
                }

                EsophagusDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<10", DTName = "ESOPHAGUS_V13" }); // Not in protocol
                EsophagusDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<26", DTName = "ESOPHAGUS_Max" }); // 100% not in protocol
                EsophagusDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<6.5", DTName = "ESOPHAGUS_Avg" }); // Not in protocol

                if ((bool)BilateralCheckBox.IsChecked)
                {
                    LiverDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<10", DTName = "LIVER_V13" }); // Not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<26", DTName = "LIVER_Max" }); // 100% not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "LIVER_Avg" }); // Not in protocol

                    StomachDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<5", DTName = "STOMACH_V13" }); // Not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<16", DTName = "STOMACH_Max" }); // 60% not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "STOMACH_Avg" }); // Not in protocol
                }
                else if ((bool)LeftBreastRadio.IsChecked)
                {
                    LiverDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<0", DTName = "LIVER_V13" }); // Not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<5", DTName = "LIVER_Max" }); // 20% not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<1.5", DTName = "LIVER_Avg" }); // 5% in protocol

                    StomachDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<5", DTName = "STOMACH_V13" }); // Not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<16", DTName = "STOMACH_Max" }); // 60% not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "STOMACH_Avg" }); // Not in protocol
                }
                else if ((bool)RightBreastRadio.IsChecked)
                {
                    LiverDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<10", DTName = "LIVER_V13" }); // Not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<26", DTName = "LIVER_Max" }); // 100% not in protocol
                    LiverDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<3.5", DTName = "LIVER_Avg" }); // Not in protocol

                    StomachDataListView.Items.Add(new MyItem { Data = "*V13", Const = "<0", DTName = "STOMACH_V13" }); // Not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<5", DTName = "STOMACH_Max" }); // 20% not in protocol
                    StomachDataListView.Items.Add(new MyItem { Data = "*Avg", Const = "<1.5", DTName = "STOMACH_Avg" }); // 5% not in protocol
                }

                SkinDataListView.Items.Add(new MyItem { Data = "*Max", Const = "<26.8", DTName = "SKIN_Max" }); // 103% not in protocol
                SkinDataListView.Items.Add(new MyItem { Data = "*Dc1", Const = "<26.0", DTName = "SKIN_D1cc" }); // 100% not in protocol
                SkinDataListView.Items.Add(new MyItem { Data = "*Dc10", Const = "<26.0", DTName = "SKIN_D10cc" }); // 100% not in protocol
                SkinDataListView.Items.Add(new MyItem { Data = "*Dc125", Const = "<13.0", DTName = "SKIN_D125cc" }); // 50% not in protocol

            } // END_EUROPAPBI5

            TargetBreastId.Text = "-";
            TargetBoostId.Text = "-";
            TargetSPCId.Text = "-";
            IpsiLungId.Text = "-";
            ContraLungId.Text = "-";
            HeartId.Text = "-";
            LeftCoronaryId.Text = "-";
            ContraBreastId.Text = "-";
            ThyroidId.Text = "-";
            EsophagusId.Text = "-";
            LiverId.Text = "-";
            StomachId.Text = "-";
            SkinId.Text = "-";

            foreach (var structure in structureSet.Structures)
            {
                // Retrieve DVH data
                DVHData dvhData = null;
                DVHData dvhDataVolCC = null;

                if (structure.Id != (string)TargetBreastList.SelectedItem && structure.Id != (string)TargetBoostList.SelectedItem && structure.Id != (string)TargetSPCList.SelectedItem && structure.Id != (string)IpsiLungList.SelectedItem && structure.Id != (string)ContraLungList.SelectedItem && structure.Id != (string)HeartList.SelectedItem && structure.Id != (string)ContraBreastList.SelectedItem && structure.Id != (string)LeftCoronaryList.SelectedItem && structure.Id != (string)ThyroidList.SelectedItem && structure.Id != (string)LiverList.SelectedItem && structure.Id != (string)StomachList.SelectedItem && structure.Id != (string)EsophagusList.SelectedItem && structure.Id != (string)SkinList.SelectedItem)
                    continue;

                if (planSetup != null)
                {
                    dvhData = planSetup.GetDVHCumulativeData(structure,
                                      DoseValuePresentation.Absolute,
                                      VolumePresentation.Relative, 0.1);
                    dvhDataVolCC = planSetup.GetDVHCumulativeData(structure,
                                      DoseValuePresentation.Absolute,
                                      VolumePresentation.AbsoluteCm3, 0.1);
                }
                else if (planSum != null)
                {
                    dvhData = planSum.GetDVHCumulativeData(structure,
                                      DoseValuePresentation.Absolute,
                                      VolumePresentation.Relative, 0.1);
                    dvhDataVolCC = planSum.GetDVHCumulativeData(structure,
                                      DoseValuePresentation.Absolute,
                                      VolumePresentation.AbsoluteCm3, 0.1);
                }

                if (structure.Id == (string)TargetBreastList.SelectedItem)
                {
                    TargetBreastId.Text = (string)TargetBreastList.SelectedItem;
                    TargetBreastId.ToolTip = structure.Volume.ToString("F2") + " cc";
                    ComputeDVHData(TargetBreastDataListView, structure, dvhData, dvhDataVolCC);
                }

                if (structure.Id == (string)TargetBoostList.SelectedItem)
                {
                    TargetBoostId.Text = (string)TargetBoostList.SelectedItem;
                    TargetBoostId.ToolTip = structure.Volume.ToString("F2") + " cc";
                    ComputeDVHData(TargetBoostDataListView, structure, dvhData, dvhDataVolCC);
                }

                if (structure.Id == (string)TargetSPCList.SelectedItem)
                {
                    TargetSPCId.Text = (string)TargetSPCList.SelectedItem;
                    TargetSPCId.ToolTip = structure.Volume.ToString("F2") + " cc";
                    ComputeDVHData(TargetSPCDataListView, structure, dvhData, dvhDataVolCC);
                }

                if (structure.Id == (string)IpsiLungList.SelectedItem)
                {
                    IpsiLungId.Text = (string)IpsiLungList.SelectedItem;
                    ComputeDVHData(IpsiLungDataListView, structure, dvhData, dvhDataVolCC);
                }

                if (structure.Id == (string)HeartList.SelectedItem)
                {
                    HeartId.Text = (string)HeartList.SelectedItem;
                    ComputeDVHData(HeartDataListView, structure, dvhData, dvhDataVolCC);
                }

                if (structure.Id == (string)ContraLungList.SelectedItem)
                {
                    ContraLungId.Text = (string)ContraLungList.SelectedItem;
                    ComputeDVHData(ContraLungDataListView, structure, dvhData, dvhDataVolCC);
                }

                if (structure.Id == (string)ContraBreastList.SelectedItem)
                {
                    ContraBreastId.Text = (string)ContraBreastList.SelectedItem;
                    ComputeDVHData(ContraBreastDataListView, structure, dvhData, dvhDataVolCC);
                }

                if (structure.Id == (string)LeftCoronaryList.SelectedItem)
                {
                    LeftCoronaryId.Text = (string)LeftCoronaryList.SelectedItem;
                    ComputeDVHData(LeftCoronaryDataListView, structure, dvhData, dvhDataVolCC);
                }

                if (structure.Id == (string)ThyroidList.SelectedItem)
                {
                    ThyroidId.Text = (string)ThyroidList.SelectedItem;
                    ComputeDVHData(ThyroidDataListView, structure, dvhData, dvhDataVolCC);
                }

                if (structure.Id == (string)EsophagusList.SelectedItem)
                {
                    EsophagusId.Text = (string)EsophagusList.SelectedItem;
                    ComputeDVHData(EsophagusDataListView, structure, dvhData, dvhDataVolCC);
                }

                if (structure.Id == (string)LiverList.SelectedItem)
                {
                    LiverId.Text = (string)LiverList.SelectedItem;
                    ComputeDVHData(LiverDataListView, structure, dvhData, dvhDataVolCC);
                }

                if (structure.Id == (string)StomachList.SelectedItem)
                {
                    StomachId.Text = (string)StomachList.SelectedItem;
                    ComputeDVHData(StomachDataListView, structure, dvhData, dvhDataVolCC);
                }

                if (structure.Id == (string)SkinList.SelectedItem)
                {
                    SkinId.Text = (string)SkinList.SelectedItem;
                    ComputeDVHData(SkinDataListView, structure, dvhData, dvhDataVolCC);
                }
            }

            ComputeScores();

            // start Plan-sanity-check v2.0
            isPscOk = true;
            PscCheckString = "";
            if (planSetup != null)
                isPscOk = PscCheckPlan(planSetup, ref PscCheckString);
            else if (planSum != null)
                isPscOk = PscCheckPlanSum(planSum, ref PscCheckString);

            if (isPscOk)
                PscHyperLink.Foreground = Brushes.Green;
            else
                PscHyperLink.Foreground = Brushes.Red;
            // end Plan-sanity-check
        }

        private void BoostCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (BoostCheckBox.IsChecked == true)
                TargetBoostLabel.FontWeight = System.Windows.FontWeights.Bold;
            else
                TargetBoostLabel.FontWeight = System.Windows.FontWeights.Normal;
            
            ApplyFilterDefaults_OnPopup();
            FetchDVHData();
        }

        private void SPCNCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (SPCNCheckBox.IsChecked == true)
                TargetSPCLabel.FontWeight = System.Windows.FontWeights.Bold;
            else
                TargetSPCLabel.FontWeight = System.Windows.FontWeights.Normal;

            ApplyFilterDefaults_OnPopup();
            FetchDVHData();
        }

        private void LeftBreastRadio_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilterDefaults_OnPopup();
            FetchDVHData();
        }

        private void RightBreastRadio_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilterDefaults_OnPopup();
            FetchDVHData();
        }

        private void BilateralCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilterDefaults_OnPopup();
            FetchDVHData();
        }

        private void ProneCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilterDefaults_OnPopup();
            FetchDVHData();
        }

        private void BreathHoldCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilterDefaults_OnPopup();
            FetchDVHData();
        }

        /*
        private void TargetSPCList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TargetSPCList.SelectedIndex == -1)
                SPCNCheckBox.IsChecked = false;
            else
            {
                if (PBICheckBox.IsChecked == true)
                    SPCNCheckBox.IsChecked = false;
                else
                    SPCNCheckBox.IsChecked = true;
            }
            TargetBreastList_SelectionChanged(sender, e);
        }
        */

        private void TargetBreastList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FetchDVHData();
        }

        private void ChestWallCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (ChestWallCheckBox.IsChecked == false)
            {
                ExpanderCheckBox.IsChecked = false;
                ExpanderHSCheckBox.IsChecked = false;
                ExpanderCheckBox.IsEnabled = false;
                ExpanderHSCheckBox.IsEnabled = false;
                OptsExpander.Header = "opts";
            }
            else
            {
                ExpanderCheckBox.IsEnabled = true;
                ExpanderHSCheckBox.IsEnabled = true;
                OptsExpander.Header = "opts*";
            }
            ApplyFilterDefaults_OnPopup();
            FetchDVHData();
        }

        private void ExpanderCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ExpanderHSCheckBox.IsChecked = false;
            ApplyFilterDefaults_OnPopup();
            FetchDVHData();
        }

        private void ExpanderHSCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ExpanderCheckBox.IsChecked = false;
            ApplyFilterDefaults_OnPopup();
            FetchDVHData();
        }

        private void PBICheckClick(object sender, RoutedEventArgs e)
        {
            BoostCheckBox.IsChecked = false;
            ApplyFilterDefaults_OnPopup();
            FetchDVHData();
        }

        // This is used by PSC only
        private void AcquireDataTableBeamData()
        {
            planBeamsTotalMUs.Clear();
            // Filter DB table and acquire additional data (Beams, MUs, etc...)
            if (dataTable_dip != DataInspectorPolicy.DIP_NONE && dataTable_dts != DataTableStatus.DTS_NOTABLE)
            {
                if (dataTable_dip == DataInspectorPolicy.DIP_ALLTB || (dataTable_dip == DataInspectorPolicy.DIP_RESTRICTUSER && dataTable_dts != DataTableStatus.DTS_NOUSER))
                {
                    // Filter on Laterality, Add Targets (Boost/SPCN) and Technique
                    string filterstring_Laterality;
                    if (BilateralCheckBox.IsChecked == true)
                        filterstring_Laterality = "BIL";
                    else
                        filterstring_Laterality = LeftBreastRadio.IsChecked == true ? "L" : "R";

                    if (PBICheckBox.IsChecked == true)
                        filterstring_Laterality += "|PBI";
                    if (ProneCheckBox.IsChecked == true)
                        filterstring_Laterality += "|PRONE";

                    string filterstring_Technique;
                    if ((bool)TechniqueVMAT.IsChecked)
                        filterstring_Technique = "VMAT";
                    else if ((bool)Technique3DCRT.IsChecked)
                        filterstring_Technique = "3DCRT";
                    else if ((bool)TechniqueIMRT.IsChecked)
                        filterstring_Technique = "IMRT";
                    else
                        filterstring_Technique = "NA";

                    if ((bool)HybridCheckBox.IsChecked)
                        filterstring_Technique += "|HYBRID";
                    if (BreathHoldCheckBox.IsChecked == true)
                        filterstring_Technique += "|BH";
                    if (TangentialCheckBox.IsChecked == true)
                        filterstring_Technique += "|TANG";

                    string filterstring_wAddTargets = "NO";
                    if (BoostCheckBox.IsChecked == true && SPCNCheckBox.IsChecked == true)
                        filterstring_wAddTargets = "YES-BOOST|YES-SPCN";
                    else if (BoostCheckBox.IsChecked == true)
                        filterstring_wAddTargets = "YES-BOOST";
                    else if (SPCNCheckBox.IsChecked == true)
                        filterstring_wAddTargets = "YES-SPCN";

                    // Filter also on PlanBeams to separate PLANSUMs from PLANs
                    string filterstring_PlanBeams;
                    if (SUMCheckBox.IsChecked == true)
                        filterstring_PlanBeams = "<PLANSUM>";
                    else
                        filterstring_PlanBeams = "<PLAN>";

                    string filterString = "LATERALITY ='" + filterstring_Laterality + "'" + "AND TECHNIQUE ='" + filterstring_Technique + "'" + "AND wADDTARGETS ='" + filterstring_wAddTargets + "'" + "AND PLAN_BEAMS LIKE '" + filterstring_PlanBeams + "%'";
                    if (dataTable_dip == DataInspectorPolicy.DIP_RESTRICTUSER && dataTable_dts == DataTableStatus.DTS_BASICUSER)  // restrict data to USER only
                        filterString += "AND USER='" + userName + "'";

                    DataRow[] filteredRows = dataTable.Select(filterString);
                    int FilteredProtocolRecordsCount = filteredRows.Count();
                    // Find MU stats in DB
                    if (FilteredProtocolRecordsCount > 0)
                    {
                        List<string> planBeamsStringList = new List<string>();
                        planBeamsStringList = filteredRows.AsEnumerable().Select(x => x["PLAN_BEAMS"].ToString()).ToList();
                        if (planBeamsStringList.Count > 0)
                        {
                            foreach (string iplanBeamsString in planBeamsStringList)
                            {
                                double iplanBeamsMUs = 0;

                                XDocument doc = XDocument.Parse(iplanBeamsString);
                                var plans = doc.Descendants("PLAN"); // look at PLAN tags
                                foreach (XElement currentPlan in plans)
                                {
                                    var beams = currentPlan.Descendants("B");
                                    foreach (XElement currentBeam in beams)
                                    {
                                        iplanBeamsMUs += Convert.ToDouble(currentBeam.Element("MU").Value);
                                    }
                                }
                                planBeamsTotalMUs.Add(iplanBeamsMUs);
                            }
                        }
                    }
                }
            }
        }

        private void ProtocolComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsVisible)
                return;
            
            if (ProtocolSelectedIndex == ProtocolComboBox.SelectedIndex)
                return;
            ProtocolSelectedIndex = ProtocolComboBox.SelectedIndex;

            PBICheckBox.IsChecked = false;

            // SPC/N available only for RTOG1005
            if (ProtocolComboBox.SelectedIndex == 0)
            {
                SPCNCheckBox.IsEnabled = true;
                if (TargetSPCList.SelectedItem != null)
                {
                    if (TechniqueVMAT.IsChecked == true)
                        SPCNCheckBox.IsChecked = true;
                    else if (TechniqueIMRT.IsChecked == true)
                        SPCNCheckBox.IsChecked = true;
                    else if (Technique3DCRT.IsChecked == true && SUMCheckBox.IsChecked == true)
                        SPCNCheckBox.IsChecked = true;
                }
            }
            else
            {
                SPCNCheckBox.IsEnabled = false;
                SPCNCheckBox.IsChecked = false;
            }

            // Boost available only for RTOG1005, NHSFF and NHS FAST
            if (ProtocolComboBox.SelectedIndex <= 2)
            {
                BoostCheckBox.IsEnabled = true;
                if (TargetBoostList.SelectedItem != null)
                {
                    if (TechniqueVMAT.IsChecked == true)
                        BoostCheckBox.IsChecked = true;
                    else if (TechniqueIMRT.IsChecked == true)
                        BoostCheckBox.IsChecked = true;
                    else if (Technique3DCRT.IsChecked == true && SUMCheckBox.IsChecked == true)
                        BoostCheckBox.IsChecked = true;
                }
            }
            else
            {
                BoostCheckBox.IsEnabled = false;
                BoostCheckBox.IsChecked = false;
            }

            if (ProtocolComboBox.SelectedIndex == 5)
                PBICheckBox.IsChecked = true;
            else
                PBICheckBox.IsChecked = false;

            if (ProtocolComboBox.SelectedIndex == 0)
            {
                ExportFileName.Text = "bwRTOG1005_15FR.txt";
                TargetBreastLabel.FontWeight = System.Windows.FontWeights.Bold;
                if (BoostCheckBox.IsChecked == true)
                    TargetBoostLabel.FontWeight = System.Windows.FontWeights.Bold;
                else
                    TargetBoostLabel.FontWeight = System.Windows.FontWeights.Normal;
                IpsiLungLabel.FontWeight = System.Windows.FontWeights.Bold;
                HeartLabel.FontWeight = System.Windows.FontWeights.Bold;
            }
            else if (ProtocolComboBox.SelectedIndex == 1)
            {
                ExportFileName.Text = "bwNHSFF_5FR.txt";
                TargetBreastLabel.FontWeight = System.Windows.FontWeights.Bold;
                if (BoostCheckBox.IsChecked == true)
                    TargetBoostLabel.FontWeight = System.Windows.FontWeights.Bold;
                else
                    TargetBoostLabel.FontWeight = System.Windows.FontWeights.Normal;
                IpsiLungLabel.FontWeight = System.Windows.FontWeights.Bold;
                HeartLabel.FontWeight = System.Windows.FontWeights.Bold;
            }
            else if (ProtocolComboBox.SelectedIndex == 2)
            {
                ExportFileName.Text = "bwFAST_5FR.txt";
                TargetBreastLabel.FontWeight = System.Windows.FontWeights.Bold;
                if (BoostCheckBox.IsChecked == true)
                    TargetBoostLabel.FontWeight = System.Windows.FontWeights.Bold;
                else
                    TargetBoostLabel.FontWeight = System.Windows.FontWeights.Normal;
                IpsiLungLabel.FontWeight = System.Windows.FontWeights.Bold;
                HeartLabel.FontWeight = System.Windows.FontWeights.Bold;
            }
            else if (ProtocolComboBox.SelectedIndex == 3)
            {
                ExportFileName.Text = "bwEUROPA21_WBI_15FR.txt";
                TargetBreastLabel.FontWeight = System.Windows.FontWeights.Bold;
                IpsiLungLabel.FontWeight = System.Windows.FontWeights.Bold;
                HeartLabel.FontWeight = System.Windows.FontWeights.Bold;
            }
            else if (ProtocolComboBox.SelectedIndex == 4)
            {
                ExportFileName.Text = "bwEUROPA21_WBI_5FR.txt";
                TargetBreastLabel.FontWeight = System.Windows.FontWeights.Bold;
                IpsiLungLabel.FontWeight = System.Windows.FontWeights.Bold;
                HeartLabel.FontWeight = System.Windows.FontWeights.Bold;
            }
            else if (ProtocolComboBox.SelectedIndex == 5)
            {
                TargetTabItem.Header = "PBI Targets";
                TargetBreastLabel.Content = "Ipsi Breast";
                TargetBoostLabel.Content = "PBI PTV";
                TargetSPCLabel.Content = "Ipsi Breast - PTV";

                OptsExpander.Visibility = Visibility.Hidden;

                ExportFileName.Text = "bwEUROPA21_PBI_5FR.txt";

                TargetBreastLabel.FontWeight = System.Windows.FontWeights.Bold;
                TargetBoostLabel.FontWeight = System.Windows.FontWeights.Bold;
                IpsiLungLabel.FontWeight = System.Windows.FontWeights.Bold;
                HeartLabel.FontWeight = System.Windows.FontWeights.Bold;
            }

            PrepareProtocolTooltip();

            //Clear DB table if selection has changed
            int ProtocolRecordsCount = dataTable.Rows.Count;
            if (ProtocolRecordsCount > 0)
            {
                if (ProtocolComboBox.SelectedIndex != (int)dataTable_dtc)
                {
                    dataTable.Reset();
                    dataTable_dts = DataTableStatus.DTS_NOTABLE;
                    planBeamsTotalMUs.Clear();
                }
            }

            ProtocolRecordsCount = dataTable.Rows.Count;

            // Read CSV file for Data Inspector if not already read
            bool isExportFileOK = false;
            if (ProtocolRecordsCount == 0)
            {
                dataTable_dts = DataTableStatus.DTS_NOTABLE;
                RecordsInFile.Text = "RECS = NONE";

                string ExportFullFileName = ExportDrawer.Text + '\\' + ExportFileName.Text;

                if (Directory.Exists(ExportDrawer.Text) == false)
                {
                    string message = "Export Drawer does not exist or it's not accessible.\nPlease specify a valid Export Drawer in AgolabsPreferences.txt file.";
                    MessageBox.Show(message, "Breast Watch", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    try
                    {
                        FileStream fs = File.OpenRead(ExportFullFileName);
                        fs.Close();
                        isExportFileOK = true;
                    }
                    catch (Exception)
                    {
                        string message = "Cannot open Export File. Data Inspector will not be available.";
                        MessageBox.Show(message, "Breast Watch", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                if (isExportFileOK)
                {
                    using (StreamReader sr = new StreamReader(ExportFullFileName))
                    {
                        try
                        {
                            string[] headers = sr.ReadLine().Split('\t');
                            foreach (string header in headers)
                            {
                                dataTable.Columns.Add(header);
                            }
                            
                            while (!sr.EndOfStream)
                            {
                                string[] rows = sr.ReadLine().Split('\t');
                                DataRow dr = dataTable.NewRow();
                                for (int i = 0; i < headers.Length; i++)
                                {
                                    dr[i] = rows[i];
                                }
                                dataTable.Rows.Add(dr);
                            }

                            if (dataTable.Rows.Count == 0)
                            {
                                string message = "Export File is empty.\nAppend at least one plan to Export File to use Data Inspector.\nIf this is the first time please check the write headers checkbox.";
                                MessageBox.Show(message, "Breast Watch", MessageBoxButton.OK, MessageBoxImage.Error);
                                isExportFileOK = false;
                            }
                            else
                            {

                                DataRow[] recordsUser = dataTable.Select("USER='" + userName + "'");
                                int dataTable_UserContribCount = recordsUser.Count();
                                int dataTable_Count = dataTable.Rows.Count;
                                if (dataTable_UserContribCount == 0)
                                    dataTable_dts = DataTableStatus.DTS_NOUSER;
                                else if (dataTable_UserContribCount < (dataTable.Rows.Count * DataInspectorRestrictUserPercentage) / 100.0)
                                    dataTable_dts = DataTableStatus.DTS_BASICUSER;
                                else
                                    dataTable_dts = DataTableStatus.DTS_TRUEBELIEVER;

                                RecordsInFile.Text = "RECS = " + dataTable_UserContribCount + "/" + dataTable_Count;
                                dataTable_dtc = (DataTableContents)ProtocolComboBox.SelectedIndex;
                            }
                        }
                        catch (Exception)
                        {
                            string message = "Cannot read Export File. Data Inspector will not be available.";
                            MessageBox.Show(message, "Breast Watch", MessageBoxButton.OK, MessageBoxImage.Error);
                            isExportFileOK = false;
                        }
                    }
                }
            }
            // end read CSV file

            if (isExportFileOK == false)
            {
                dataTable_dts = DataTableStatus.DTS_NOTABLE;
                TogglePopupButton.IsEnabled = false;
            }
            else
            {
                ApplyFilterDefaults_OnPopup();
                TogglePopupButton.IsEnabled = true;
            }

            // Update table icon
            switch (dataTable_dts)
            {
                case DataTableStatus.DTS_NOTABLE:
                    Table.Visibility = Visibility.Hidden;
                    TableError.Visibility = Visibility.Visible;
                    TableMissing.Visibility = Visibility.Hidden;
                    TableOK.Visibility = Visibility.Hidden;
                    break;
                case DataTableStatus.DTS_NOUSER:
                    Table.Visibility = Visibility.Hidden;
                    TableError.Visibility = Visibility.Hidden;
                    TableMissing.Visibility = Visibility.Visible;
                    TableOK.Visibility = Visibility.Hidden;
                    if (dataTable_dip == DataInspectorPolicy.DIP_RESTRICTUSER || dataTable_dip == DataInspectorPolicy.DIP_NONE)
                    {
                        string message = "Current User not present in Export File. Data Inspector not available.";
                        MessageBox.Show(message, "Breast Watch", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else if (dataTable_dip == DataInspectorPolicy.DIP_ALLTB)
                    {
                        string message = "Current User not present in Export File.";
                        MessageBox.Show(message, "Breast Watch", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    break;
                case DataTableStatus.DTS_BASICUSER:
                    Table.Visibility = Visibility.Visible;
                    TableError.Visibility = Visibility.Hidden;
                    TableMissing.Visibility = Visibility.Hidden;
                    TableOK.Visibility = Visibility.Hidden;
                    if (dataTable_dip == DataInspectorPolicy.DIP_RESTRICTUSER || dataTable_dip == DataInspectorPolicy.DIP_NONE)
                    {
                        string message = "Applying User Restriction to Export File Plan Library.";
                        MessageBox.Show(message, "Breast Watch", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    break;
                case DataTableStatus.DTS_TRUEBELIEVER:
                    Table.Visibility = Visibility.Hidden;
                    TableError.Visibility = Visibility.Hidden;
                    TableMissing.Visibility = Visibility.Hidden;
                    TableOK.Visibility = Visibility.Visible;
                    break;
            }

            //Override given current DIP
            if (dataTable_dts != DataTableStatus.DTS_NOTABLE)
            {
                if (dataTable_dip == DataInspectorPolicy.DIP_NONE)
                {
                    Table.Visibility = Visibility.Hidden;
                    TableError.Visibility = Visibility.Visible;
                    TableMissing.Visibility = Visibility.Hidden;
                    TableOK.Visibility = Visibility.Hidden;
                }
                else if (dataTable_dip == DataInspectorPolicy.DIP_ALLTB)
                {
                    Table.Visibility = Visibility.Hidden;
                    TableError.Visibility = Visibility.Hidden;
                    TableMissing.Visibility = Visibility.Hidden;
                    TableOK.Visibility = Visibility.Visible;
                }
                else if (dataTable_dip == DataInspectorPolicy.DIP_RESTRICTUSER)
                {
                }
            }

                FetchDVHData();
        }

        private void ComputeMUMetric()
        {
            // Sum all MUs in plans
            PlanMUMetric_MUSum = 0;
            PlanMUMetric_AvgMUPerBeam = 0;
            if (planSetup != null)
            {
                int nBeams = 0;
                foreach (Beam iBeam in planSetup.Beams)
                {
                    if (iBeam.IsSetupField == false)
                    {
                        PlanMUMetric_MUSum += iBeam.Meterset.Value;
                        PlanMUMetric_AvgMUPerBeam += iBeam.Meterset.Value;
                        nBeams++;
                    }
                }
                PlanMUMetric_AvgMUPerBeam /= nBeams;
            }
            else if (planSum != null)
            {
                int nBeams = 0;
                foreach (PlanSetup iPlan in planSum.PlanSetups)
                {
                    foreach (Beam iBeam in iPlan.Beams)
                    {
                        if (iBeam.IsSetupField == false)
                        {
                            PlanMUMetric_MUSum += iBeam.Meterset.Value;
                            PlanMUMetric_AvgMUPerBeam += iBeam.Meterset.Value;
                            nBeams++;
                        }
                    }
                }
                PlanMUMetric_AvgMUPerBeam += PlanMUMetric_MUSum / nBeams;
            }
        }

        private bool PscCheckPlanSum(PlanSum plansumtocheck, ref string PscCheckString)
        {
        bool rv = true;

        foreach (PlanSetup plantocheck in planSum.PlanSetups)
        {
            if (PscCheckPlan(plantocheck, ref PscCheckString) == false)
                rv = false;
        }

        VVector firstIsocenter = new VVector(0,0,0);
        if (plansumtocheck.PlanSetups.First().Beams.Count() > 0)
            firstIsocenter = plansumtocheck.PlanSetups.First().Beams.First().IsocenterPosition;

       foreach (PlanSetup plantocheck in planSum.PlanSetups)
        {
                if (plantocheck.Beams.Count() > 0)
                {
                    Beam firstBeam = plantocheck.Beams.First();
                    if (firstBeam.IsocenterPosition.x != firstIsocenter.x || firstBeam.IsocenterPosition.y != firstIsocenter.y || firstBeam.IsocenterPosition.z != firstIsocenter.z)
                        PscCheckString += "IS2";
                }
        }
        return rv;
    }

    private bool PscCheckPlan(PlanSetup plantocheck, ref string PscCheckString)
    {
        bool rv = true;
        PscCheckString = "";

        if (plantocheck == null)
            return false;

        if (plantocheck.Beams.Count() == 0)
            PscCheckString = "IS0";
        else
        {
            VVector firstIsocenter = plantocheck.Beams.First().IsocenterPosition;
            foreach (Beam iBeam in plantocheck.Beams)
            { 
                if(iBeam.IsocenterPosition.x != firstIsocenter.x || iBeam.IsocenterPosition.y != firstIsocenter.y || iBeam.IsocenterPosition.z != firstIsocenter.z)
                {
                    PscCheckString = "IS1";
                    break;
                }
            }
        }

        foreach (Beam iBeam in plantocheck.Beams)
        {
            if (iBeam.IsSetupField)
            {
                foreach (ControlPoint iCP in iBeam.ControlPoints)
                {
                    if (iCP.PatientSupportAngle != 0)
                        PscCheckString += "SP1";
                }
            }
            else
            {
                if (iBeam.MLCPlanType == MLCPlanType.VMAT)
                {
                    if (iBeam.DoseRate < 600)
                        PscCheckString += "VD1";

                    foreach (ControlPoint iCP in iBeam.ControlPoints)
                    {
                        if (iCP.JawPositions.X1 + iCP.JawPositions.X2 > 145)
                            PscCheckString += "VJ1";
                        if (iCP.CollimatorAngle == 0)
                            PscCheckString += "VC0";
                        if (iCP.PatientSupportAngle != 0)
                            PscCheckString += "VP1";
                    }
                }
                else if (iBeam.MLCPlanType == MLCPlanType.DoseDynamic)
                {
                    if (iBeam.DoseRate < 600)
                        PscCheckString += "ID1";

                    foreach (ControlPoint iCP in iBeam.ControlPoints)
                    {
                        if (iCP.JawPositions.X1 + iCP.JawPositions.X2 > 145)
                            PscCheckString += "IJ1";
                    }
                }
                else if (iBeam.MLCPlanType == MLCPlanType.Static)
                {
                    if (iBeam.Wedges.Count() >= 1)
                        if (iBeam.DoseRate < 600)
                            PscCheckString += "WD1";
                }
            }
        }

        if (PscCheckString != "")
            rv = false;

        AcquireDataTableBeamData();
        if (PlanMUMetric_MUSum > 0 && planBeamsTotalMUs.Count > 0)
            {
                // Check plan MUSum vs DB MUSums
                double dataTable_avgMUMetric = planBeamsTotalMUs.Average();
                double dataTable_devstMUMetric = Math.Sqrt(planBeamsTotalMUs.Sum(d => Math.Pow(d - dataTable_avgMUMetric, 2)) / planBeamsTotalMUs.Count());
                if (dataTable_devstMUMetric < 0.1)
                    dataTable_devstMUMetric = 0.1;
                if (PlanMUMetric_MUSum > (dataTable_avgMUMetric + dataTable_devstMUMetric) || PlanMUMetric_MUSum < (dataTable_avgMUMetric - dataTable_devstMUMetric))
                { 
                    PscCheckString += "MU1:" + PlanMUMetric_MUSum.ToString("F1");
                    rv = false;
                }
            }
        else
            PscCheckString += "MU1:NA";
   
        return rv;
        }

    private void HelpHandler(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://agolabs.com/breastwatch-history/");
        }

    private void PscHandler(object sender, RoutedEventArgs e)
    {
            AcquireDataTableBeamData();
            if (PlanMUMetric_MUSum > 0 && planBeamsTotalMUs.Count > 0)
            {
                // Check plan MUSum vs DB MUSums
                double dataTable_avgMUMetric = planBeamsTotalMUs.Average();
                double dataTable_devstMUMetric = Math.Sqrt(planBeamsTotalMUs.Sum(d => Math.Pow(d - dataTable_avgMUMetric, 2)) / planBeamsTotalMUs.Count());
                if (dataTable_devstMUMetric < 0.1)
                    dataTable_devstMUMetric = 0.1;
                if (PlanMUMetric_MUSum > (dataTable_avgMUMetric + dataTable_devstMUMetric) || PlanMUMetric_MUSum < (dataTable_avgMUMetric - dataTable_devstMUMetric))
                    PscCheckString += "MU1:" + PlanMUMetric_MUSum.ToString("F1");
            }
            else
                PscCheckString += "MU1:NA";

            PscWindow pscWindow = new PscWindow(planBeamsTotalMUs, PlanMUMetric_MUSum);
            if(pscWindow != null)
            {
                if (PscCheckString.Contains("IS"))
                {
                    pscWindow.BeamsISOTestFailed.Visibility = Visibility.Visible;
                    pscWindow.BeamsISOTestPassed.Visibility = Visibility.Hidden;
                    if(PscCheckString.Contains("IS0"))
                        pscWindow.BeamsISOTestFailed.ToolTip = "Plan contains no beam";
                    else if (PscCheckString.Contains("IS1"))
                        pscWindow.BeamsISOTestFailed.ToolTip = "Plan contains multiple isocenters";
                    else if (PscCheckString.Contains("IS2"))
                        pscWindow.BeamsISOTestFailed.ToolTip = "Plan Sum contains multiple isocenters";
                }
                else
                {
                    pscWindow.BeamsISOTestFailed.Visibility = Visibility.Hidden;
                    pscWindow.BeamsISOTestPassed.Visibility = Visibility.Visible;
                }

                if (PscCheckString.Contains("D1"))
                {
                    pscWindow.DoseRateTestFailed.Visibility = Visibility.Visible;
                    pscWindow.DoseRateTestPassed.Visibility = Visibility.Hidden;
                    if (PscCheckString.Contains("WD1"))
                        pscWindow.DoseRateTestFailed.ToolTip = "Dose Rate is < 600 MU/min in a wedged beam";
                    else if (PscCheckString.Contains("VD1"))
                        pscWindow.DoseRateTestFailed.ToolTip = "Dose Rate is < 600 MU/min in a VMAT beam\n";
                    else if (PscCheckString.Contains("ID1"))
                        pscWindow.DoseRateTestFailed.ToolTip = "Dose Rate is < 600 MU/min in a IMRT beam\n";
                }
                else
                {
                    pscWindow.DoseRateTestFailed.Visibility = Visibility.Hidden;
                    pscWindow.DoseRateTestPassed.Visibility = Visibility.Visible;
                }

                pscWindow.JawAngleTestFailed.Visibility = PscCheckString.Contains("VC0") == true ? Visibility.Visible : Visibility.Hidden;
                pscWindow.JawAngleTestFailed.ToolTip = "Jaws angle = 0 in a VMAT beam";
                pscWindow.JawAngleTestPassed.Visibility = (pscWindow.JawAngleTestFailed.Visibility == Visibility.Visible) ? Visibility.Hidden : Visibility.Visible;

                pscWindow.CouchRtnTestFailed.Visibility = PscCheckString.Contains("VP1") == true ? Visibility.Visible : Visibility.Hidden;
                pscWindow.CouchRtnTestFailed.ToolTip = "Couch rotation <> 0 in a VMAT beam";
                pscWindow.CouchRtnTestPassed.Visibility = (pscWindow.CouchRtnTestFailed.Visibility == Visibility.Visible) ? Visibility.Hidden : Visibility.Visible;

                pscWindow.CouchRtnTestFailed.Visibility = PscCheckString.Contains("SP1") == true ? Visibility.Visible : Visibility.Hidden;
                pscWindow.CouchRtnTestFailed.ToolTip += "\nCouch rotation <> 0 in a setup beam";
                pscWindow.CouchRtnTestPassed.Visibility = (pscWindow.CouchRtnTestFailed.Visibility == Visibility.Visible) ? Visibility.Hidden : Visibility.Visible;

                pscWindow.FieldXMLCTestFailed.Visibility = PscCheckString.Contains("J1") == true ? Visibility.Visible : Visibility.Hidden;
                pscWindow.FieldXMLCTestFailed.ToolTip = "Field width (X) exceeds 14.5 cm in a VMAT/IMRT beam";
                pscWindow.FieldXMLCTestPassed.Visibility = (pscWindow.FieldXMLCTestFailed.Visibility == Visibility.Visible) ? Visibility.Hidden : Visibility.Visible;

                if (PscCheckString.Contains("MU1:NA"))
                {
                    pscWindow.MUSumTestFailed.Visibility = Visibility.Visible;
                    pscWindow.MUSumTestFailed.ToolTip = "MU Sum check not available";
                    pscWindow.MUSumTestPassed.Visibility = Visibility.Hidden;
                }
                else if (PscCheckString.Contains("MU1:"))
                {
                    double dataTable_avgMUSum = planBeamsTotalMUs.Average();
                    double dataTable_devstMUSum = Math.Sqrt(planBeamsTotalMUs.Sum(d => Math.Pow(d - dataTable_avgMUSum, 2)) / planBeamsTotalMUs.Count());
                    pscWindow.MUSumTestFailed.Visibility = Visibility.Visible;
                    pscWindow.MUSumTestFailed.ToolTip = "MU Sum = " + PlanMUMetric_MUSum.ToString("F1") + " while Export Table MU Sum = " + dataTable_avgMUSum.ToString("F1") + "+/- " + dataTable_devstMUSum.ToString("F1") + " (1SD).\nDouble click to open Data Inpector window.";
                    pscWindow.MUSumTestPassed.Visibility = Visibility.Hidden;
                }
                else
                {
                    pscWindow.MUSumTestFailed.Visibility = Visibility.Hidden;
                    pscWindow.MUSumTestPassed.Visibility = Visibility.Visible;
                }

                Label lsender = (sender as Hyperlink).Parent as Label;
                var point = lsender.PointToScreen(new Point(0,0));
                    
                pscWindow.Left = point.X - pscWindow.Width+30;
                pscWindow.Top = point.Y+10;

                pscWindow.Owner = this;
                pscWindow.ShowDialog();
                e.Handled = true;
            }
    }

    private void IpsiLungDataListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (dataTable_dip == DataInspectorPolicy.DIP_NONE || dataTable_dts == DataTableStatus.DTS_NOTABLE)
            return;
        else if (dataTable_dip == DataInspectorPolicy.DIP_RESTRICTUSER && dataTable_dts == DataTableStatus.DTS_NOUSER)
            return;

        int SelectedIndex = (sender as ListView).SelectedIndex;
        if (SelectedIndex == -1)
            return;

        MyItem itemSelected = (MyItem)(sender as ListView).SelectedItem;
        string SelectedItemDTName = itemSelected.DTName;
        if (string.IsNullOrEmpty(SelectedItemDTName))
            return;

        int ProtocolRecordsCount = dataTable_filteredRows.Count();
        
        // Plot data
        if (ProtocolRecordsCount > 0)
            {
            double d;
            List<double> values = new List<double>();
            values = dataTable_filteredRows.AsEnumerable().Where(x => double.TryParse(x[SelectedItemDTName].ToString(),out d) == true).Select(x => Convert.ToDouble(x[SelectedItemDTName])).ToList();
            if (values.Count > 0)
                {
                PropPopupWindow prpwindow = new PropPopupWindow(values, itemSelected);
                if (prpwindow != null)
                    {
                    var relativePosition = e.GetPosition(this);
                    var point = PointToScreen(relativePosition);

                    prpwindow.Left = point.X - prpwindow.Width + 12;
                    prpwindow.Top = point.Y - 12;

                    prpwindow.Owner = this;
                    prpwindow.ShowDialog();
                    }
                }
            }
        // end Plot data
        }

        private void ScoreColorsComboBox_DropDownClosed(object sender, EventArgs e)
        {
            StyleSelector myListViewItemStyleSelector;


            if (ScoreStyleComboBox.SelectedIndex == 1)
                myListViewItemStyleSelector = new mListViewItemCommunityStyleSelector();
            else if (ScoreStyleComboBox.SelectedIndex == 2)
                myListViewItemStyleSelector = new mListViewItemCommunityGradientStyleSelector();
            else
                myListViewItemStyleSelector = new mListViewItemProtocolStyleSelector();

            TargetBreastDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;
            TargetBoostDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;
            TargetSPCDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;

            IpsiLungDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;
            ContraLungDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;
            HeartDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;
            LeftCoronaryDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;
            ContraBreastDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;
            ThyroidDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;
            EsophagusDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;
            LiverDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;
            StomachDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;
            SkinDataListView.ItemContainerStyleSelector = myListViewItemStyleSelector;
        }

        private void MainCommentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ComputeMUMetric();

            ProtocolComboBox_SelectionChanged(this, null);

            ApplyFilterDefaults_OnPopup();
            FetchDVHData();
        }
    }
}