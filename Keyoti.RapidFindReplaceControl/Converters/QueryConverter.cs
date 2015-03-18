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
using System.Windows.Markup;
using Keyoti.RapidFindReplace.WPF;

namespace Keyoti.RapidFindReplace.WPF.Converters
{
    //http://drwpf.com/blog/category/value-converters/ explains how deriving from MarkupExtension allows use without defining staticresource
    /// <summary>
    /// Changes Query object to a string. 
    /// </summary>     
    public class QueryConverter : IValueConverter
    {

   

        /// <summary>
        /// Converts a Query object to a string.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return "";
            return (value as Query).QueryText;
        }
        /// <summary>
        /// Changes plain text to a Query object. 
        /// </summary>  
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {            
            string queryText = value as string;
            return new Query(queryText);
        }
    }
}
