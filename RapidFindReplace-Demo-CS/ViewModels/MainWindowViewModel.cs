using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RapidFindReplace_Demo_CS.ViewModels
{
    class MainWindowViewModel
    {

        /*
         * Auto generate a list of demo Windows
         * */
        List<Type> windows = null;
        public List<Type> AvailableWindows
        {
            get
            {
                if (windows == null)
                {
                    var children = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(System.Windows.Window).IsAssignableFrom(t));

                    windows = new List<Type>();
                    foreach (Type t in children)
                    {
                        if (t.Name != "MainWindow")
                            windows.Add(t);
                        if (t.Name == "Basic") SelectedWindow = t;
                    }
                }
                windows.Reverse();
                return windows;
            }
        }

        Type _SelectedWindow;
        public Type SelectedWindow
        {
            get
            {
                return _SelectedWindow;
            }
            set
            {
                _SelectedWindow = value;
            }
        }
    }
}
