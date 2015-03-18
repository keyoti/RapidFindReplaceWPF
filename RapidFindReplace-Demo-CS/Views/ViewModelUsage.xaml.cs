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
using Keyoti.RapidFindReplace.WPF;

namespace RapidFindReplace_Demo_CS.Views
{
    /// <summary>
    /// Interaction logic for ViewModelUsage.xaml
    /// </summary>
    public partial class ViewModelUsage : Window
    {
        public ViewModelUsage()
        {
            InitializeComponent();
            _searchTextBox.Focus();
        }

        private void _searchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (cb1.IsChecked==true)
            {
                RapidFindReplaceControlViewModel viewModel = _searchTextBox.DataContext as RapidFindReplaceControlViewModel;
                viewModel.FindText(viewModel.FindScope);
            }
        }


    }
}
