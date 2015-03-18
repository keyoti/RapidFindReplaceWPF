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

namespace Keyoti.RapidFindReplace.WPF.TextMatching
{
    /// <summary>
    /// A hit in a text string.
    /// </summary>
    public struct Hit
    {
        int start;
        int length;
        /// <summary>
        /// New
        /// </summary>
        public Hit(int start, int length)
        {
            this.start = start;
            this.length = length;
            
        }
        /// <summary>
        /// The start position of the hit.
        /// </summary>
        public int Start { get { return start; } }
        /// <summary>
        /// The end position of the hit.
        /// </summary>
        public int Length { get { return length; } }

    }


    
}
