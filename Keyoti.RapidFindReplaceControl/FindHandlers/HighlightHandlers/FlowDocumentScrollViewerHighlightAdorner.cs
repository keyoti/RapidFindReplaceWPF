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
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Keyoti.RapidFindReplace.WPF.FindHandlers.HighlightHandlers
{
    class FlowDocumentScrollViewerHighlightAdorner : HighlightAdorner
    {
        FlowDocumentScrollViewer flowDocumentScrollViewer;
        public FlowDocumentScrollViewerHighlightAdorner(FlowDocumentScrollViewer flowDocumentScrollViewer, Run run, int start, int length, double horizontalScrollBarHeight, Brush bodyHighlightAdornerBrush, Pen bodyHighlightAdornerPen, Brush bodyIterativeHighlightAdornerBrush, Pen bodyIterativeHighlightAdornerPen)
            : base(flowDocumentScrollViewer, run, start, length, horizontalScrollBarHeight, bodyHighlightAdornerBrush, bodyHighlightAdornerPen, bodyIterativeHighlightAdornerBrush, bodyIterativeHighlightAdornerPen)
        {
            this.flowDocumentScrollViewer = flowDocumentScrollViewer;

            flowDocumentScrollViewer.AddHandler(ScrollViewer.ScrollChangedEvent, new RoutedEventHandler(scroller_ScrollChanged));

        }



        /// <summary>
        /// Cleans up event handlers
        /// </summary>
        public override void Unregister()
        {
            flowDocumentScrollViewer.RemoveHandler(ScrollViewer.ScrollChangedEvent, new RoutedEventHandler(scroller_ScrollChanged));
        }


        protected virtual void scroller_ScrollChanged(object sender, RoutedEventArgs e)
        {
            if (this.AdornedElement != null)
            {
                AdornerLayer l = AdornerLayer.GetAdornerLayer(this.AdornedElement);
                if (l != null)
                    l.Update();
            }
        }

    }
}
