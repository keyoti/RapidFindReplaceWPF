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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Keyoti.RapidFindReplace.WPF.Converters
{
            /// <summary>
        /// Changes camel case to sentence case. 
        /// </summary>     
    public class CamelCaseConverter : IValueConverter
    {
                /// <summary>
        /// Changes camel case to sentence case. 
        /// </summary>     
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            StringBuilder text = new StringBuilder(value as string);
            int prevInsert = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (i > 0 && Char.IsUpper(text[i]) && i < text.Length - 1 && (Char.IsLower(text[i + 1]) || Char.IsLower(text[i - 1])) && prevInsert < i - 1)
                {

                    text.Insert(i++, ' ');
                    text[i] = char.ToLower(text[i]);
                    prevInsert = i;
                }
            }
            return text.ToString();
        
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
