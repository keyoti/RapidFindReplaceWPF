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
    class FlowDocumentPageViewerHighlightHandler : HighlightHandler
    {
        [System.Reflection.Obfuscation(Exclude = true)]
        public new static bool DoesHandle(DependencyObject element)
        {
            return element is FlowDocumentPageViewer;
        }

        FlowDocumentPageViewer flowDocumentPageViewer;
        bool ignorePropertyChange = false;
        public FlowDocumentPageViewerHighlightHandler(FlowDocumentPageViewer flowDocumentPageViewer, System.Windows.Media.Brush bodyHighlightAdornerBrush, System.Windows.Media.Pen bodyHighlightAdornerPen, Brush bodyIterativeHighlightAdornerBrush, Pen bodyIterativeHighlightAdornerPen)
            : base(flowDocumentPageViewer, bodyHighlightAdornerBrush, bodyHighlightAdornerPen, bodyIterativeHighlightAdornerBrush, bodyIterativeHighlightAdornerPen)
        {
            this.flowDocumentPageViewer = flowDocumentPageViewer;
            ignorePropertyChange = true;
            BindingOperations.SetBinding(this, FlowDocumentPageViewerMasterPageNumberProperty, new Binding { Source = flowDocumentPageViewer, Path = new PropertyPath("MasterPageNumber"), Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(this, FlowDocumentPageViewerZoomProperty, new Binding { Source = flowDocumentPageViewer, Path = new PropertyPath("Zoom"), Mode = BindingMode.OneWay });
            ignorePropertyChange = false;
        }

        #region Dependency Property Mirrors
        public readonly static DependencyProperty FlowDocumentPageViewerZoomProperty = DependencyProperty.Register("FlowDocumentPageViewerZoom", typeof(double), typeof(FlowDocumentPageViewerHighlightHandler), new PropertyMetadata(new PropertyChangedCallback(FlowDocumentPageViewerZoomChanged)));

        private static void FlowDocumentPageViewerZoomChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            //sender.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() => { AdornerLayer.GetAdornerLayer((sender as HighlightAdorner).AdornedElement).Update(); }));
            FlowDocumentPageViewerHighlightHandler handler = sender as FlowDocumentPageViewerHighlightHandler;
            if (!handler.ignorePropertyChange)
            {
                handler.flowDocumentPageViewer.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() => { UpdateHighlighting(handler.flowDocumentPageViewer, handler.scrollBarHighlightHandler); }));
            }
        }


        public double FlowDocumentPageViewerZoom
        {
            get { return (double)GetValue(FlowDocumentPageViewerZoomProperty); }
            set { SetValue(FlowDocumentPageViewerZoomProperty, value); }
        }


        public readonly static DependencyProperty FlowDocumentPageViewerMasterPageNumberProperty = DependencyProperty.Register("FlowDocumentPageViewerMasterPageNumber", typeof(int), typeof(FlowDocumentPageViewerHighlightHandler), new PropertyMetadata(new PropertyChangedCallback(FlowDocumentPageViewerMasterPageNumberChanged)));

        private static void FlowDocumentPageViewerMasterPageNumberChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            FlowDocumentPageViewerHighlightHandler handler = sender as FlowDocumentPageViewerHighlightHandler;
            if (!handler.ignorePropertyChange)
            {
                //ClearHighlighting(handler.flowDocumentPageViewer);
                handler.ClearHighlighting();
                //do new search
                handler.flowDocumentPageViewer.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() => { handler.OnNewSearchNeeded(); }));
            }
        }

        public int FlowDocumentPageViewerMasterPageNumber
        {
            get { return (int)GetValue(FlowDocumentPageViewerMasterPageNumberProperty); }
            set { SetValue(FlowDocumentPageViewerMasterPageNumberProperty, value); }
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




        

    }
}
