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
using System.Windows;
using System.Windows.Documents;

namespace Keyoti.RapidFindReplace.WPF.FindHandlers
{
    /// <summary>
    /// Container for a collection of Run instances.
    /// </summary>
    /// <remarks>Use this to search a collection of strings wrapped in Run objects.  The Run objects do not need to be rooted in a visual document.</remarks>
    public class RunCollectionContainer : DependencyObject
    {
        List<Run> backer;

        int iterationStartIndex = 0;

        /// <summary>
        /// Which character index to start iterating matches from (see remarks).
        /// </summary>
        /// <remarks>This is similar to the caret position in a text box.  The first match returned will be after this position, and at the end of the text the iterator will wrap to the start and continue back to this index.</remarks>
        public int IterationStartIndex
        {
            get { return iterationStartIndex; }
            set { iterationStartIndex = value; }
        }

        
        /// <summary>
        /// Collection of Run objects to search.
        /// </summary>
        public List<Run> RunCollection
        {
            get { return backer; }
            set { backer = value; }
        }

        /// <summary>
        /// New instance.
        /// </summary>
        public RunCollectionContainer(List<Run> runCollection)
        {
            backer = runCollection;
        }

        internal int GetRunAbsoluteOffset(Run run)
        {
            int l=0;
            for (int i = 0; i < backer.Count; i++)
            {
                if(backer[i]==run) return l;
                else l += backer[i].Text.Length;
            }
            return l;
        }
    }
}
