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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BreastWatch
{
    /// <summary>
    /// Logica di interazione per UserControlFilterPopup.xaml
    /// </summary>
    public partial class UserControlFilterPopup : UserControl
    {
        public UserControlFilterPopup()
        {
            InitializeComponent();
        }

        private void Filter_Targets_ChestWall_Click(object sender, RoutedEventArgs e)
        {
            if (!this.IsVisible)
                return;

            MainWindow parentWindow = (MainWindow)(Window.GetWindow(this));
            parentWindow.FetchDVHData();
        }

        private void Filter_Targets_ChestWall_Click(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsVisible)
                return;

            MainWindow parentWindow = (MainWindow)(Window.GetWindow(this));
            parentWindow.FetchDVHData();

        }

    }
}
