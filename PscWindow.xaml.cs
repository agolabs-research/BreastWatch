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

namespace BreastWatch
{
    /// <summary>
    /// Interaction logic for PscWindow.xaml
    /// </summary>
    public partial class PscWindow : Window
    {
        List<double> MUSums_values;
        double MUSum;

        public PscWindow(List<double> inMUSums_values,double resMUSum)
        {
            InitializeComponent();

            MUSums_values = new List<double>(inMUSums_values);
            MUSum = resMUSum;
        }

        private void MUSumTestPassed_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (MUSum == 0 || MUSums_values.Count == 0)
                return;

            MainWindow mainWindow = Owner as MainWindow;

            MyItem item = new MyItem();
            item.Data = "MU-Sum";
            item.Const = "NOC";
            item.Results = MUSum.ToString("F1");

            PropPopupWindow prpwindow = new PropPopupWindow(MUSums_values, item);
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
}

