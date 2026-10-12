// This is BreastWatch for ESAPI
// Author: Stefano Agostinelli
// 
// Licensed under the terms described in the LICENSE file
// included with this source code.
//
// This software is provided for research purposes only.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Reflection;
using System.Windows.Controls;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

//[assembly: ESAPIScript(IsWriteable = true)]

// Do not change namespace and class name
// otherwise Eclipse will not be able to run the script.
namespace VMS.TPS
{
    class Script
    {
        public Script()
        {
        }

        public string GetTagContent(string prefString, string Section, string Tag)
        {
            string TagContent = "";
            if (prefString.Contains(Section))
            {
                int iSec_start = prefString.IndexOf(Section);
                int iSec_length = prefString.IndexOf("[", iSec_start + Section.Length);
                if (iSec_length == -1) // no next section
                    iSec_length = prefString.Length;
                iSec_length -= iSec_start;

                int iTag = prefString.IndexOf(Tag, iSec_start, iSec_length);
                if (iTag > 0)
                {
                    int iTagContent = iTag + Tag.Length + 1;
                    int iLengthTagContent = 0;
                    int iNextEOL = prefString.IndexOf('\n', iTagContent);
                    iLengthTagContent = (iNextEOL > 0) ? iNextEOL - iTagContent - 1: iLengthTagContent = prefString.Length - iTagContent;

                    TagContent = prefString.Substring(iTagContent, iLengthTagContent);
                }
            }
            return TagContent;
        }

        public void Execute(ScriptContext context)//, Window window)
        {
            PlanSetup planSetup = context.PlanSetup;
            Patient pat = context.Patient;
            PlanSum planSum = null;

            if (planSetup == null)
            {
                if (context.PlanSumsInScope.Count() == 0)
                    throw new ApplicationException("Please open a calculated Plan or Plan Sum before using this script.");
                else
                {
                    if(context.PlanSumsInScope.Count() == 1)
                        planSum = context.PlanSumsInScope.First();
                    else
                    {
                        var sumAskWindow = new BreastWatch.SelectPlanSumWindow(context);
                        if (sumAskWindow != null)
                        {
                            bool dialogResult = (bool)sumAskWindow.ShowDialog();
                            if(dialogResult)
                            {
                                planSum = context.PlanSumsInScope.ElementAt(sumAskWindow.PlanSumsListBox.SelectedIndex);
                            }
                        }
                        if(planSum == null)
                            throw new ApplicationException("No Plan Sum has been selected. Aborting.");
                    }
                }
            }

            bool isDoseValid = false;
            if (planSetup != null)
                {
                isDoseValid = planSetup.IsDoseValid;
                }
            else if(planSum != null)
            {
                foreach (PlanSetup plansetupinsum in planSum.PlanSetups)
                {
                    isDoseValid = plansetupinsum.IsDoseValid;
                    if (!isDoseValid)
                        break;
                }
            }
            if(!isDoseValid)
                throw new ApplicationException("No valid dose has been detected.\nPlease open a calculated Plan or Plan Sum before using this script.");

            // Retrieve StructureSet
            StructureSet structureSet = null;
            if(planSetup != null)
                structureSet = planSetup.StructureSet;
            else if(planSum != null)
                structureSet = planSum.StructureSet;

            if (structureSet == null)
                throw new ApplicationException("The selected Plan does not reference a Structure Set.");

            // Retrieve the number of PTVs structures
            int numPTVs = 0;
            foreach (var structure in structureSet.Structures)
            {
                if (structure.DicomType == "PTV")
                {
                    numPTVs++;
                }
            }

            if (numPTVs == 0)
                throw new ApplicationException("The selected Plan does not include a PTV.");

            // Load Preferences file
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string preffilePath = assemblyFolder + "\\" + "AgolabsPreferences.txt";

            string ExportPath = "";
            string DataInspectorPolicy = "";
            double DataInspectorRestrictUserPercentage = 0;
            bool AnonymizePatientData = false;

            bool isPrefsFile = false;
            if (File.Exists(preffilePath))
            {
                string prefString = File.ReadAllText(preffilePath);
                isPrefsFile = true;

                string Section = "[BREASTWATCH]";
                string Tag = "ExportPath";
                string TagContent = GetTagContent(prefString, Section, Tag);
                ExportPath = TagContent;

                Section = "[BREASTWATCH]";
                Tag = "AnonymizePatientData";
                TagContent = GetTagContent(prefString, Section, Tag);
                if (TagContent == "TRUE")
                    AnonymizePatientData = true;
                else if (TagContent == "FALSE")
                    AnonymizePatientData = false;
                else
                    AnonymizePatientData = false;

                Section = "[BREASTWATCH]";
                Tag = "DataInspectorPolicy";
                TagContent = GetTagContent(prefString, Section, Tag);
                DataInspectorPolicy = TagContent;

                if (TagContent == "RESTRICTUSER")
                {
                    Section = "[BREASTWATCH]";
                    Tag = "DataInspectorRestrictUserPercentage";
                    TagContent = GetTagContent(prefString, Section, Tag);
                    try
                    {
                        DataInspectorRestrictUserPercentage = Convert.ToDouble(TagContent);
                        if (DataInspectorRestrictUserPercentage < 5 || DataInspectorRestrictUserPercentage > 50)
                        {
                            string message = "DataInspectorRestrictUserPercentage value should be in 5-50 range.\nPlease correct its value in AgoPreferences.txt file.\nUsing the default value = 15.";
                            MessageBox.Show(message, "Breast Watch", MessageBoxButton.OK, MessageBoxImage.Warning);
                            DataInspectorRestrictUserPercentage = 15;
                        }
                    }
                    catch
                    {
                        string message = "Invalid DataInspectorRestrictUserPercentage TAG in AgoPreferences.txt file.\nUsing the default value = 15.";
                        MessageBox.Show(message, "Breast Watch", MessageBoxButton.OK, MessageBoxImage.Warning);
                        DataInspectorRestrictUserPercentage = 15;
                    }
                }
            }

            // Create main window
            var mainWindow = new BreastWatch.MainWindow(pat, planSetup, planSum, structureSet, context.CurrentUser.Name, ExportPath, DataInspectorPolicy, DataInspectorRestrictUserPercentage, AnonymizePatientData);

            int ProtocolIndex = -1;
            bool IsTotalBoostPrescription = false;
            if (planSetup != null)
            {
                if (planSetup.TotalDose.Dose >= 40.049 && planSetup.TotalDose.Dose <= 40.051) //RT0G1005
                    ProtocolIndex = 0;
                else if (numPTVs >= 2 && planSetup.TotalDose.Dose >= 47.99 && planSetup.TotalDose.Dose <= 48.01) //RT0G1005 w BOOST
                {
                    ProtocolIndex = 0;
                    IsTotalBoostPrescription = true;
                }
                else if (planSetup.TotalDose.Dose >= 25.99 && planSetup.TotalDose.Dose <= 26.01) //NHSFF
                {
                    ProtocolIndex = 1;
                }
               else if(planSetup.TotalDose.Dose >= 29.99 && planSetup.TotalDose.Dose <= 30.01)
               {
                    if (numPTVs >= 2)  //NHSFF w BOOST
                    {
                        ProtocolIndex = 1;
                        IsTotalBoostPrescription = true;
                    }
                    else  //EUROPA v2.1 PBI 30 Gy
                        ProtocolIndex = 5;
                }
                else if (planSetup.TotalDose.Dose >= 28.49 && planSetup.TotalDose.Dose <= 28.51) //FAST
                {
                    ProtocolIndex = 2;
                }
                else if(numPTVs >= 2 && planSetup.TotalDose.Dose >= 30.99 && planSetup.TotalDose.Dose <= 31.01) //FAST w BOOST
                {
                    ProtocolIndex = 2;
                    IsTotalBoostPrescription = true;
                }
            }
            else if(planSum!= null)
            {
                double TotalPrescribedDose = 0;
                foreach (PlanSetup plansetupinsum in planSum.PlanSetups)
                    TotalPrescribedDose += plansetupinsum.TotalDose.Dose;

                if (TotalPrescribedDose >= 40.049 && TotalPrescribedDose <= 40.051) //RTOG1005
                    ProtocolIndex = 0;
                else if (TotalPrescribedDose >= 25.99 && TotalPrescribedDose <= 26.01) //NHSFF
                    ProtocolIndex = 1;
                else if (TotalPrescribedDose >= 28.49 && TotalPrescribedDose <= 28.51) //FAST
                    ProtocolIndex = 2;
                else if (TotalPrescribedDose >= 29.99 && TotalPrescribedDose <= 30.01) //EUROPA v2.1 PBI 30 Gy
                {
                    if (numPTVs >= 2)  //NHSFF w BOOST
                    {
                        ProtocolIndex = 1;
                        IsTotalBoostPrescription = true;
                    }
                    else  //EUROPA v2.1 PBI 30 Gy
                        ProtocolIndex = 5;
                }
                else if (numPTVs >= 2 && TotalPrescribedDose >= 47.99 && TotalPrescribedDose <= 48.01) //RT0G1005 w BOOST
                {
                    ProtocolIndex = 0;
                    IsTotalBoostPrescription = true;
                }
                else if (numPTVs >= 2 && TotalPrescribedDose >= 29.99 && TotalPrescribedDose <= 30.01)
                {
                    ProtocolIndex = 1;
                    IsTotalBoostPrescription = true;
                }
                else if (numPTVs >= 2 && TotalPrescribedDose >= 30.99 && TotalPrescribedDose <= 31.01) //FAST w BOOST
                {
                    ProtocolIndex = 2;
                    IsTotalBoostPrescription = true;
                }
            }

            bool isPBI = false;
            if (ProtocolIndex == 0)
            {
                mainWindow.ProtocolComboBox.SelectedIndex = 0;
                ComboBoxItem cbi = mainWindow.ProtocolComboBox.Items[0] as ComboBoxItem;
                cbi.IsEnabled = true;
                if (!IsTotalBoostPrescription)
                {
                    cbi = mainWindow.ProtocolComboBox.Items[3] as ComboBoxItem;
                    cbi.IsEnabled = true;
                }
            }
            else if (ProtocolIndex == 1)
            {
                mainWindow.ProtocolComboBox.SelectedIndex = 1;
                ComboBoxItem cbi = mainWindow.ProtocolComboBox.Items[1] as ComboBoxItem;
                cbi.IsEnabled = true;
                if (!IsTotalBoostPrescription)
                {
                    cbi = mainWindow.ProtocolComboBox.Items[4] as ComboBoxItem;
                    cbi.IsEnabled = true;
                }
            }
            else if (ProtocolIndex == 2)
            {
                mainWindow.ProtocolComboBox.SelectedIndex = 2;
                ComboBoxItem cbi = mainWindow.ProtocolComboBox.Items[2] as ComboBoxItem;
                cbi.IsEnabled = true;
            }
            else if (ProtocolIndex == 5)
            {
                mainWindow.ProtocolComboBox.SelectedIndex = 5;
                ComboBoxItem cbi = mainWindow.ProtocolComboBox.Items[5] as ComboBoxItem;
                cbi.IsEnabled = true;
                isPBI = true;
            }
            else
            {
                if (planSetup != null)
                    throw new ApplicationException("The selected Plan does not use a supported breast protocol.\nPrescription is " + mainWindow.PrescriptionDose);
                else if (planSum != null)
                    throw new ApplicationException("The selected Plan Sum does not use a supported breast protocol.\nTotal Prescription is " + mainWindow.PrescriptionDose);
            }

            bool isTargetLateralityInLabel_Left = false;
            bool isTargetLateralityInLabel_Right = false;
            foreach (var structure in structureSet.Structures)
            {
                if (structure.Volume != 0)
                {
                    if (structure.DicomType == "PTV" || structure.DicomType == "CTV")
                    {
                        if (!isPBI) // Not a PBI
                            mainWindow.TargetBreastList.Items.Add(structure.Id);
                        mainWindow.TargetBoostList.Items.Add(structure.Id);
                        mainWindow.TargetSPCList.Items.Add(structure.Id);
                        if (structure.Id.Contains("sx") || structure.Id.Contains("SX") || structure.Id.Contains("Sx") || structure.Id.Contains("sn") || structure.Id.Contains("SN") || structure.Id.Contains("Sn") || structure.Id.Contains("Breast_L") || structure.Id.Contains("Breast L") || structure.Id.Contains("breast_L") || structure.Id.Contains("breast L") || structure.Id.Contains("LEFT") || structure.Id.Contains("left"))
                            isTargetLateralityInLabel_Left = true;
                        else if (structure.Id.Contains("dx") || structure.Id.Contains("DX") || structure.Id.Contains("Dx") || structure.Id.Contains("ds") || structure.Id.Contains("DS") || structure.Id.Contains("Ds") || structure.Id.Contains("Breast_R") || structure.Id.Contains("Breast R") || structure.Id.Contains("breast_R") || structure.Id.Contains("breast R") || structure.Id.Contains("RIGHT") || structure.Id.Contains("right"))
                            isTargetLateralityInLabel_Right = true;

                        if (structure.DicomType == "PTV")
                        {
                            if (!isPBI) // Not a PBI
                            {
                                if (structure.Id.Contains("BOOST") || structure.Id.Contains("boost") || structure.Id.Contains("Boost"))
                                    mainWindow.TargetBoostList.SelectedItem = structure.Id;
                                else if (structure.Id.Contains("SPC") || structure.Id.Contains("spc") || structure.Id.Contains("Spc") || structure.Id.Contains("SSC") || structure.Id.Contains("ssc") || structure.Id.Contains("Ssc") || structure.Id.Contains(" N") || structure.Id.Contains("_N"))
                                    mainWindow.TargetSPCList.SelectedItem = structure.Id;
                                else
                                {
                                    // if it's a plan and it contains a Target Volume, select it as main Target, otherwise select the first PTV not looking like a Boost or SPC
                                    if(planSetup != null)
                                    {
                                        if(structure.Id == planSetup.TargetVolumeID)
                                            mainWindow.TargetBreastList.SelectedItem = structure.Id;
                                    }
                                    if (mainWindow.TargetBreastList.SelectedIndex == -1)
                                        mainWindow.TargetBreastList.SelectedItem = structure.Id;
                                }
                            }
                            else // PBI
                            {
                                if (mainWindow.TargetBoostList.SelectedIndex == -1)
                                    mainWindow.TargetBoostList.SelectedItem = structure.Id;
                            }
                        }
                    }
                    else
                    {
                        if (isPBI) // PBI
                        {
                            mainWindow.TargetBreastList.Items.Add(structure.Id);
                            mainWindow.TargetSPCList.Items.Add(structure.Id);
                        }
                        if (!structure.Id.Contains("Bolus"))
                            {
                            mainWindow.IpsiLungList.Items.Add(structure.Id);
                            mainWindow.ContraLungList.Items.Add(structure.Id);
                            mainWindow.HeartList.Items.Add(structure.Id);
                            mainWindow.ContraBreastList.Items.Add(structure.Id);
                            mainWindow.LeftCoronaryList.Items.Add(structure.Id);
                            mainWindow.ThyroidList.Items.Add(structure.Id);
                            mainWindow.EsophagusList.Items.Add(structure.Id);
                            mainWindow.LiverList.Items.Add(structure.Id);
                            mainWindow.StomachList.Items.Add(structure.Id);
                            mainWindow.SkinList.Items.Add(structure.Id);
                            }
                    }
                }
            }

            bool isLaterality = false;
            if (isTargetLateralityInLabel_Right && !isTargetLateralityInLabel_Left)
            {
                mainWindow.RightBreastRadio.IsChecked = true;
                mainWindow.LeftBreastRadio.IsChecked = false;
                isLaterality = true;
            }
            else if (!isTargetLateralityInLabel_Right && isTargetLateralityInLabel_Left)
            {
                mainWindow.RightBreastRadio.IsChecked = false;
                mainWindow.LeftBreastRadio.IsChecked = true;
                isLaterality = true;
            }
            else
            {
                string message = "Breast Watch cannot guess breast laterality.\nIs this a right breast?\n(Yes = Right breast | No = Left breast)";
                MessageBoxResult result = MessageBox.Show(message, "Breast Watch", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    mainWindow.RightBreastRadio.IsChecked = true;
                    mainWindow.LeftBreastRadio.IsChecked = false;
                }
                else
                {
                    mainWindow.RightBreastRadio.IsChecked = false;
                    mainWindow.LeftBreastRadio.IsChecked = true;
                }
                isLaterality = true;
            }

            if (isLaterality)
            {
                foreach (var structure in structureSet.Structures)
                {
                    if (structure.DicomType != "PTV" && structure.DicomType != "CTV")
                    {
                        if (structure.Id.Contains("Heart"))
                            mainWindow.HeartList.SelectedItem = structure.Id;
                        if (structure.Id.Contains("A_Coronary") || structure.Id.Contains("LAD"))
                            mainWindow.LeftCoronaryList.SelectedItem = structure.Id;
                        if (!isPBI) // Not a PBI
                        {
                            if (structure.Id.Contains("Breast"))
                                mainWindow.ContraBreastList.SelectedItem = structure.Id;
                        }
                        else // PBI
                        {
                            if (structure.Id.Contains("Control"))
                                mainWindow.TargetSPCList.SelectedItem = structure.Id;
                        }
                        if ((bool)mainWindow.RightBreastRadio.IsChecked)
                        {
                            if (isPBI) // PBI
                            {
                                if (structure.Id.Contains("Breast_R"))
                                    mainWindow.TargetBreastList.SelectedItem = structure.Id;
                                if (structure.Id.Contains("Breast_L"))
                                    mainWindow.ContraBreastList.SelectedItem = structure.Id;
                            }
                            if (structure.Id.Contains("Lung_R"))
                                mainWindow.IpsiLungList.SelectedItem = structure.Id;
                            if (structure.Id.Contains("Lung_L"))
                                mainWindow.ContraLungList.SelectedItem = structure.Id;
                        }
                        else
                        {
                            if (isPBI) // PBI
                            {
                                if (structure.Id.Contains("Breast_L"))
                                    mainWindow.TargetBreastList.SelectedItem = structure.Id;
                                if (structure.Id.Contains("Breast_R"))
                                    mainWindow.ContraBreastList.SelectedItem = structure.Id;
                            }
                            if (structure.Id.Contains("Lung_L"))
                                mainWindow.IpsiLungList.SelectedItem = structure.Id;
                            if (structure.Id.Contains("Lung_R"))
                                mainWindow.ContraLungList.SelectedItem = structure.Id;
                        }
                        if (structure.Id.Contains("Thyroid"))
                            mainWindow.ThyroidList.SelectedItem = structure.Id;
                        if (structure.Id.Contains("Esophagus"))
                            mainWindow.EsophagusList.SelectedItem = structure.Id;
                        if (structure.Id.Contains("Liver"))
                            mainWindow.LiverList.SelectedItem = structure.Id;
                        if (structure.Id.Contains("Stomach"))
                            mainWindow.StomachList.SelectedItem = structure.Id;
                        if (structure.Id.Contains("Skin"))
                            mainWindow.SkinList.SelectedItem = structure.Id;
                    }
                }
            }

            if (structureSet.Id.Contains("BH") || structureSet.Id.Contains("bh") || structureSet.Id.Contains("HOLD") || structureSet.Id.Contains("Hold") || structureSet.Id.Contains("hold") || structureSet.Id.Contains("APN") || structureSet.Id.Contains("apn"))
                mainWindow.BreathHoldCheckBox.IsChecked = true;
 
            if (planSetup != null)
            {
                mainWindow.SUMCheckBox.IsChecked = false;
            
                int nBeamsWBolus = 0;
                int nBeamsVMAT = 0;
                int nBeamsIMRT = 0;
                int nBeamsStatic = 0;
                int nBeams = 0;
                foreach(var iBeam in planSetup.Beams)
                {
                    if (iBeam.IsSetupField == false)
                    {
                        nBeams++;
                        if(iBeam.Boluses.Count() >= 1)
                            nBeamsWBolus++;
                        if (iBeam.MLCPlanType == MLCPlanType.VMAT)
                            nBeamsVMAT++;
                        else if (iBeam.MLCPlanType == MLCPlanType.DoseDynamic)
                            nBeamsIMRT++;
                        else
                            nBeamsStatic++;
                    }
                }

                if(nBeams == 0) // Dose plan
                {
                    mainWindow.Technique3DCRT.IsEnabled = true;
                    mainWindow.TechniqueIMRT.IsEnabled = true;
                    mainWindow.TechniqueVMAT.IsEnabled = true;
                }

                if (nBeamsStatic > 0)
                    mainWindow.Technique3DCRT.IsEnabled = true;
                if (nBeamsIMRT > 0)
                    mainWindow.TechniqueIMRT.IsEnabled = true;
                if (nBeamsVMAT > 0)
                    mainWindow.TechniqueVMAT.IsEnabled = true;

                if (nBeamsIMRT == 0 && nBeamsVMAT == 0)
                {
                    mainWindow.Technique3DCRT.IsChecked = true;
                    mainWindow.TangentialCheckBox.IsEnabled = false;

                }
                else
                {
                    if (nBeamsVMAT >= nBeamsIMRT)
                        mainWindow.TechniqueVMAT.IsChecked = true;
                    else if (nBeamsIMRT > nBeamsVMAT)
                        mainWindow.TechniqueIMRT.IsChecked = true;

                    if (!isPBI && nBeamsVMAT == 0 && nBeamsStatic == 0 && nBeamsIMRT <= 4)
                        mainWindow.TangentialCheckBox.IsChecked = true;

                    if (nBeamsStatic != 0)
                        mainWindow.HybridCheckBox.IsChecked = true;
                }

                if (nBeamsWBolus == 0)
                    mainWindow.BolusComboBox.SelectedIndex = 0;
                else if(nBeamsWBolus == nBeams)
                    mainWindow.BolusComboBox.SelectedIndex = 2;
                else
                    mainWindow.BolusComboBox.SelectedIndex = 1;
            }
            else if(planSum != null)
            {
                mainWindow.SUMCheckBox.IsChecked = true;

                int nBeamsWBolus = 0;
                int nBeamsVMAT = 0;
                int nBeamsIMRT = 0;
                int nBeamsStatic = 0;
                int nBeams = 0;
                foreach (var iPlanSetup in planSum.PlanSetups)
                {
                    foreach (var iBeam in iPlanSetup.Beams)
                    {
                        if (iBeam.IsSetupField == false)
                        {
                            nBeams++;
                            if (iBeam.Boluses.Count() >= 1)
                                nBeamsWBolus++;
                            if (iBeam.MLCPlanType == MLCPlanType.VMAT)
                                nBeamsVMAT++;
                            else if (iBeam.MLCPlanType == MLCPlanType.DoseDynamic)
                                nBeamsIMRT++;
                            else
                                nBeamsStatic++;
                        }
                    }
                }

                if (nBeams == 0) // Dose plan
                {
                    mainWindow.Technique3DCRT.IsEnabled = true;
                    mainWindow.TechniqueIMRT.IsEnabled = true;
                    mainWindow.TechniqueVMAT.IsEnabled = true;
                }

                if (nBeamsStatic > 0)
                    mainWindow.Technique3DCRT.IsEnabled = true;
                if (nBeamsIMRT > 0)
                    mainWindow.TechniqueIMRT.IsEnabled = true;
                if (nBeamsVMAT > 0)
                    mainWindow.TechniqueVMAT.IsEnabled = true;

                if (nBeamsIMRT == 0 && nBeamsVMAT == 0)
                {
                    mainWindow.Technique3DCRT.IsChecked = true;
                    mainWindow.TangentialCheckBox.IsEnabled = false;
                }
                else
                {
                    if (nBeamsVMAT >= nBeamsIMRT)
                        mainWindow.TechniqueVMAT.IsChecked = true;
                    else if (nBeamsIMRT > nBeamsVMAT)
                        mainWindow.TechniqueIMRT.IsChecked = true;

                    if (!isPBI && nBeamsVMAT == 0 && nBeamsStatic == 0 && nBeamsIMRT <= 4)
                        mainWindow.TangentialCheckBox.IsChecked = true;

                    if (nBeamsStatic != 0)
                        mainWindow.HybridCheckBox.IsChecked = true;
                }

                if (nBeamsWBolus == 0)
                    mainWindow.BolusComboBox.SelectedIndex = 0;
                else if (nBeamsWBolus == nBeams)
                    mainWindow.BolusComboBox.SelectedIndex = 2;
                else
                    mainWindow.BolusComboBox.SelectedIndex = 1;
            }

            if (ProtocolIndex <= 2)
            {
                if (mainWindow.TargetBoostList.SelectedItem != null)
                {
                    if (mainWindow.TechniqueVMAT.IsChecked == true)
                        mainWindow.BoostCheckBox.IsChecked = true;
                    else if (mainWindow.TechniqueIMRT.IsChecked == true)
                        mainWindow.BoostCheckBox.IsChecked = true;
                    else if (mainWindow.Technique3DCRT.IsChecked == true && mainWindow.SUMCheckBox.IsChecked == true)
                        mainWindow.BoostCheckBox.IsChecked = true;
                    else
                    {
                        string message = "Breast Watch is loading a Plan containing a Boost structure.\nProbably you should load a Plan Sum (WBI Plan + Boost Plan) not a single WBI Plan for this treatment.";
                        MessageBox.Show(message, "Breast Watch", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }

            bool DialogResult = (bool)mainWindow.ShowDialog();
            if (DialogResult)
            {
                string message_final = "Data has been appended to Export file.\nClick OK to Exit.";
                MessageBox.Show(message_final, "Breast Watch", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }
    }
}
