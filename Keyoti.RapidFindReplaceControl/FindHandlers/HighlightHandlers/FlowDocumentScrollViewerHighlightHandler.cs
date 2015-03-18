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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace Keyoti.RapidFindReplace.WPF.FindHandlers.HighlightHandlers
{
    class FlowDocumentScrollViewerHighlightHandler : HighlightHandler
    {
        [System.Reflection.Obfuscation(Exclude = true)]
        public new static bool DoesHandle(DependencyObject element)
        {
            return element is FlowDocumentScrollViewer;
        }

        FlowDocumentScrollViewer flowDocumentScrollViewer;
        bool ignorePropertyChange = false;
        public FlowDocumentScrollViewerHighlightHandler(FlowDocumentScrollViewer flowDocumentScrollViewer, System.Windows.Media.Brush bodyHighlightAdornerBrush, System.Windows.Media.Pen bodyHighlightAdornerPen, Brush bodyIterativeHighlightAdornerBrush, Pen bodyIterativeHighlightAdornerPen)
            : base(flowDocumentScrollViewer, bodyHighlightAdornerBrush, bodyHighlightAdornerPen, bodyIterativeHighlightAdornerBrush, bodyIterativeHighlightAdornerPen)
        {
            this.flowDocumentScrollViewer = flowDocumentScrollViewer;
            ignorePropertyChange = true;
            
            BindingOperations.SetBinding(this, FlowDocumentScrollViewerZoomProperty, new Binding { Source = flowDocumentScrollViewer, Path = new PropertyPath("Zoom"), Mode = BindingMode.OneWay });
            ignorePropertyChange = false;
        }

        #region Dependency Property Mirrors
        public readonly static DependencyProperty FlowDocumentScrollViewerZoomProperty = DependencyProperty.Register("FlowDocumentScrollViewerZoom", typeof(double), typeof(FlowDocumentScrollViewerHighlightHandler), new PropertyMetadata(new PropertyChangedCallback(FlowDocumentScrollViewerZoomChanged)));

        private static void FlowDocumentScrollViewerZoomChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            //sender.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() => { AdornerLayer.GetAdornerLayer((sender as HighlightAdorner).AdornedElement).Update(); }));
            FlowDocumentScrollViewerHighlightHandler handler = sender as FlowDocumentScrollViewerHighlightHandler;
            if (!handler.ignorePropertyChange)
            {
                handler.flowDocumentScrollViewer.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() => { UpdateHighlighting(handler.flowDocumentScrollViewer, handler.scrollBarHighlightHandler); }));
            }
        }


        public double FlowDocumentScrollViewerZoom
        {
            get { return (double)GetValue(FlowDocumentScrollViewerZoomProperty); }
            set { SetValue(FlowDocumentScrollViewerZoomProperty, value); }
        }


       
        #endregion



        private static void UpdateHighlighting(UIElement uiHost, ScrollBarHighlightHandler scrollBarHighlightHandler)
        {
            AdornerLayer lay = AdornerLayer.GetAdornerLayer(uiHost);
            if (lay != null)
            {
                lay.Update();
            }
            scrollBarHighlightHandler.Update();
        }

        /// <summary>
        /// Overrides should call RegisterHighlight to ensure proper registration of the Highlight.
        /// </summary>
        public override void AddHighlight(Run run, int index, int length)
        {
            HighlightAdorner adorner = new FlowDocumentScrollViewerHighlightAdorner(flowDocumentScrollViewer, run, index, length, scrollBarHighlightHandler.HorizontalScrollBarHeight, BodyHighlightAdornerBrush, BodyHighlightAdornerPen, BodyIterativeHighlightAdornerBrush, BodyIterativeHighlightAdornerPen);
            RegisterHighlight(new FlowDocumentScrollViewerHighlight(index, index + length, adorner, run));
        }


        

    }
}
