/*  
    RapidFindReplace WPF - a find/replace control for WPF applications
    Copyright (C) 2014-2015 Keyoti Inc.

    
    This program is licensed as either free software or commercial use: 
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, version 2 of the License.
    Alternatively you may purchase
    a commercial license at http://keyoti.com/products/rapidfindreplace/wpf/index.html

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/





using System;
using System.Reflection;
using System.Windows;
using System.Windows.Data;


namespace Keyoti.RapidFindReplace.WPF
{
    /// <summary>
    /// Property member attribute used to declare which properties of the OptionsViewModel class are 
    /// used option settings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)] 
    public class OptionPropertyAttribute : Attribute
    {
        string description;
        /// <summary>
        /// The description of the option property.
        /// </summary>
        public string Description { get { return description; } }

        /// <summary>
        /// New instance
        /// </summary>
        /// <param name="description"></param>
        public OptionPropertyAttribute(string description)
        {
            this.description = description;
        }
    }

    /// <summary>
    /// A user option property.
    /// </summary>
    public class OptionProperty : System.ComponentModel.INotifyPropertyChanged
    {
        /// <summary>
        /// Fired when the property changed.
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// DependencyProperty for the option.
        /// </summary>
        public System.Windows.DependencyProperty OptionDependencyProperty { get; set; }
        OptionsViewModel findOptions;
        /// <summary>
        /// Parent OptionsViewModel that this option belongs to. 
        /// </summary>
        public OptionsViewModel FindOptions
        {
            get
            {
                return findOptions;
            }
            set
            {
                findOptions = value;
                Value = FindOptions.GetValue(OptionDependencyProperty);
                BindingOperations.SetBinding(findOptions, OptionDependencyProperty, new Binding { Source = this, Path = new PropertyPath("Value"), Mode = BindingMode.TwoWay });
            }
        }

        /// <summary>
        /// Option description.
        /// </summary>
        public string Description { get; set; }

        object v;
        /// <summary>
        /// Option value.
        /// </summary>
        public object Value
        {
            get
            {
                return v;// FindOptions.GetValue(OptionDependencyProperty);
            }
            set
            {
                //FindOptions.SetValue(OptionDependencyProperty, value);
                if (v != value)
                {
                    v = value;

                    RaisePropertyChanged("Value");
                }
            }
        }

        private void RaisePropertyChanged(string p)
        {
            if (PropertyChanged != null) PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(p));
        }
        /// <summary>
        /// Option name.
        /// </summary>
        public string OptionName { get; set; }
        /// <summary>
        /// Option type.
        /// </summary>
        public Type OptionType { get; set; }
    }
}
