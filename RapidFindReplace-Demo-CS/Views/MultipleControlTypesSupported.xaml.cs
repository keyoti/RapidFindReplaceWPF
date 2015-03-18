using System;
using System.Collections.Generic;
using System.IO;
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

namespace RapidFindReplace_Demo_CS.Views
{
    /// <summary>
    /// Interaction logic for MultipleControlTypesSupported.xaml
    /// </summary>
    public partial class MultipleControlTypesSupported : Window
    {
        public MultipleControlTypesSupported()
        {
            InitializeComponent();
        }

        private void fld_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3 || (e.Key == Key.F && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))))
            {
                e.Handled = true;
                ApplicationCommands.Find.Execute(null,this);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //Ensure the FlowDocumentScrollViewer doesn't have focus, otherwise Find command will go to it.
            Focus();
            ApplicationCommands.Find.Execute(null, this);
        }

        
    }

    public class Customer
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
