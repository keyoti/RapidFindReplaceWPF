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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RapidFindReplace_Demo_CS.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RunWindow(winList.SelectedItem as Type);
        }

        private static void RunWindow(Type selectedItem)
        {
            Type window = System.Reflection.Assembly.GetExecutingAssembly().GetType(selectedItem.FullName);
            Window win = (Activator.CreateInstance(window) as Window);
            win.Topmost = true;
            Application.Current.Dispatcher.BeginInvoke((Action) (()=>{win.Show();}));
        }

        void ListBoxItem_DoubleClick(object sender, MouseEventArgs e)
        {
            RunWindow(winList.SelectedItem as Type);
        }
    }
}
