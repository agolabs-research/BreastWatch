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
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace BreastWatch
{
    /// <summary>
    /// Interaction logic for SelectPlanSumWindow.xaml
    /// </summary>
    public partial class SelectPlanSumWindow : Window
    {
        public SelectPlanSumWindow(ScriptContext context)
        {
            InitializeComponent();

            foreach(PlanSum iPlanSum in context.PlanSumsInScope)
            {
                PlanSumsListBox.Items.Add(iPlanSum.Id);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            PlanSumsListBox.SelectedIndex = -1;
            this.DialogResult = false;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
        if(PlanSumsListBox.SelectedIndex != -1)
            this.DialogResult = true;
        }
    }
}
