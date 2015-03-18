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
using System.Windows.Documents;

namespace Keyoti.RapidFindReplace.WPF.FindHandlers.RunReaders
{
    /// <summary>
    /// IRunReader for FlowDocuments.
    /// </summary>
    public class FlowDocumentRunReader : IRunReader
    {
        /// <summary>
        /// Whether this reader can read Runs from <c>runContainer</c>.
        /// </summary>

        [System.Reflection.Obfuscation(Exclude = true)]
        public static bool DoesHandle(object runContainer)
        {
            return runContainer is FlowDocument;
        }

        FlowDocument doc;
        /// <summary>
        /// New instance.
        /// </summary>
        public FlowDocumentRunReader(FlowDocument doc)
        {
            this.doc = doc;
        }

        /// <summary>
        /// Gets an enumerator that will enumerate the Runs in the element being read.
        /// </summary>
        public IEnumerator<Run> GetEnumerator()
        {
            TextPointer t = doc.ContentStart;

            // Keep a TextPointer for FlowDocument.ContentEnd handy, 
            // so we know when we're done.
            TextPointer e = doc.ContentEnd;


            // Keep going until the TextPointer is equal to or greater than ContentEnd.
            while ((t != null) && (t.CompareTo(e) < 0))
            {
                // Find the TextPointerContext that identifies the purpose 
                // for this ContentElement.
                TextPointerContext ctx = t.GetPointerContext(LogicalDirection.Forward);


                if (ctx == TextPointerContext.Text && t.Parent is Run)
                    yield return t.Parent as Run;


                // Advance to the next ContentElement in the FlowDocument.
                t = t.GetNextContextPosition(LogicalDirection.Forward);
            }
        }
    }
}
