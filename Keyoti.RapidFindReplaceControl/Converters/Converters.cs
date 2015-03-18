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
using Keyoti.RapidFindReplace.WPF.Converters;

namespace Keyoti.RapidFindReplace.WPF//for convenience, not in converters namespace - easier to access from xaml
{
    /// <summary>
    /// Convenience instances of converters.
    /// </summary>
    public class ConverterInstances
    {
        /// <summary>
        /// A QueryConverter instance
        /// </summary>
        public static QueryConverter QueryConverter = new QueryConverter();
        /// <summary>
        /// A CamelCaseConverter instance
        /// </summary>
        public static CamelCaseConverter CamelCaseConverter = new CamelCaseConverter();
    }
}
