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
using System.Windows.Media;

namespace Keyoti.RapidFindReplace.WPF.FindHandlers.HighlightHandlers
{
    class RunCollectionHighlightHandler : HighlightHandler
    {
        RunCollectionContainer container;
        public RunCollectionHighlightHandler(DependencyObject elementToHighlight, Brush bodyHighlightAdornerBrush, Pen bodyHighlightAdornerPen, Brush bodyIterativeHighlightAdornerBrush, Pen bodyIterativeHighlightAdornerPen)
            : base(elementToHighlight,bodyHighlightAdornerBrush,bodyHighlightAdornerPen,bodyIterativeHighlightAdornerBrush, bodyIterativeHighlightAdornerPen)
        {
            container = elementToHighlight as RunCollectionContainer;
        }
        [System.Reflection.Obfuscation(Exclude = true)]
        public new static bool DoesHandle(DependencyObject element)
        {
            return element is RunCollectionContainer;
        }

        internal override int GetIndexOfFirstHighlightAfterSelection()
        {
            int p = highlights.BinarySearch(new Highlight(container.IterationStartIndex, container.IterationStartIndex, null, null), underlineComparer);
            if (p < 0) p = ~p;
            return p;
        }

        protected override System.Windows.Documents.AdornerLayer AdornerLayer
        {
            get
            {
                return null;
            }
        }

        public override void AddHighlight(Run run, int index, int length) {
            RegisterHighlight(new Highlight(index, index + length, null, run, container.GetRunAbsoluteOffset(run)));
        }
    }
}
