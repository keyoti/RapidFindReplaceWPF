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
using Keyoti.RapidFindReplace.WPF;


namespace Keyoti.RapidFindReplace.WPF.FindHandlers.HighlightHandlers
{
    /// <summary>
    /// Highlight handler for scroll bar highlights.
    /// </summary>
    class ScrollBarHighlightHandler
    {
        Brush renderBrush;

        /// <summary>
        /// Brush used to render the highlight.
        /// </summary>
        public Brush RenderBrush
        {
            get { return renderBrush; }
        }
        System.Windows.Controls.Primitives.Track track;
        double scrollButtonSize = 0;
        bool scrollViewerIsChild;
        AdornerLayer scrollBarAdornerLayer;
        ScrollViewer scrollViewer;

        /// <summary>
        /// Whether there is a valid scroll bar adorner layer to use.
        /// </summary>
        public bool IsUsable { get { return scrollBarAdornerLayer != null; } }

        /// <summary>
        /// The ScrollViewer that is being highlighted.
        /// </summary>
        public ScrollViewer ScrollViewer { get { return scrollViewer; } }
        double elementHorizontalScrollBarHeight;
        /// <summary>
        /// New instance.
        /// </summary>
        /// <param name="targetElement">Any UIElement that has a ScrollViewer in it.</param>
        public ScrollBarHighlightHandler(UIElement targetElement)
        {
            renderBrush = new SolidColorBrush(Colors.Blue);
            renderBrush.Opacity = 0.4;
            ProbeScrollElements(targetElement);
        }

        /// <summary>
        /// Creates an adorner of type ScrollBarHighlightAdorner.
        /// </summary>
        /// <param name="highlightAdorner"></param>
        /// <returns></returns>
        public ScrollBarHighlightAdorner CreateScrollBarHighlightAdorner(HighlightAdorner highlightAdorner)
        {
            if (!IsUsable)
                return null;
            else return new ScrollBarHighlightAdorner(this, highlightAdorner);
        }

        #region Probe scroll elements
        void ProbeScrollElements(UIElement elementToHighlight)
        {
            if (elementToHighlight is Control)
            {
                scrollViewer = (elementToHighlight as Control).Template.FindName("PART_ContentHost", elementToHighlight as Control) as ScrollViewer;
                if (scrollViewer != null)//we have scrollviewer inside control
                {
                    scrollViewerIsChild = true;
                    ProbeScrollViewer();
                }
                else //we might have scrollviewer holding the control
                {

                    if ((elementToHighlight as Control).Parent is ScrollViewer)
                    {
                        scrollViewerIsChild = false;
                        scrollViewer = (elementToHighlight as Control).Parent as ScrollViewer;
                        ProbeScrollViewer();
                    }
                }
            }
            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged += scrollViewer_ScrollChanged;
                foreach (System.Windows.Controls.Primitives.RepeatButton repeat in Utility.FindVisualChildren<System.Windows.Controls.Primitives.RepeatButton>(scrollViewer))
                {
                    scrollButtonSize = repeat.ActualHeight;
                    break;
                }
            }
        }

        void ProbeScrollViewer()
        {
            System.Windows.Controls.Primitives.ScrollBar vertBar = scrollViewer.Template.FindName("PART_VerticalScrollBar", scrollViewer) as System.Windows.Controls.Primitives.ScrollBar;
            if (vertBar != null)
            {
                track = vertBar.Template.FindName("PART_Track", vertBar) as System.Windows.Controls.Primitives.Track;
                if (track != null)
                {
                    scrollBarAdornerLayer = AdornerLayer.GetAdornerLayer(track);
                    //scrollViewer.UpdateLayout();
                }

            }
            if (scrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible)
            {
                System.Windows.Controls.Primitives.ScrollBar horizBar = scrollViewer.Template.FindName("PART_HorizontalScrollBar", scrollViewer) as System.Windows.Controls.Primitives.ScrollBar;
                if (horizBar != null)
                {
                    elementHorizontalScrollBarHeight = horizBar.ActualHeight;
                }
                else elementHorizontalScrollBarHeight = 0;

            }
            else elementHorizontalScrollBarHeight = 0;
        }
        #endregion

        void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!scrollViewerIsChild)//its taken care of automatically when the scroller is part of the (eg textbox) control
            {
                if (scrollBarAdornerLayer == null)
                    scrollBarAdornerLayer = AdornerLayer.GetAdornerLayer(scrollViewer);
                if (scrollBarAdornerLayer != null)
                    scrollBarAdornerLayer.Update();
            }
        }

        /// <summary>
        /// Whether the Y coordinate in the argument is visible in the scroll viewer's viewport.
        /// </summary>
        public bool IsHighlightInViewPort(HighlightAdorner bodyHighlight)
        {
            if (scrollViewerIsChild)
                return bodyHighlight.IsInViewport;
            else
                return bodyHighlight.Y >= scrollViewer.ContentVerticalOffset && bodyHighlight.Y <= scrollViewer.ContentVerticalOffset + scrollViewer.ViewportHeight;
        }

        /// <summary>
        /// Gets the position of <c>bodyHighlight</c>.
        /// </summary>
        public Rect GetAdornerPosition(HighlightAdorner bodyHighlight)
        {
            double y = bodyHighlight.Y + (scrollViewerIsChild ? scrollViewer.VerticalOffset : 0);
            return new Rect(scrollViewer.ActualWidth - track.ActualWidth, scrollButtonSize + ((y / scrollViewer.ExtentHeight) * track.ActualHeight), track.ActualWidth, 3);
        }

        internal void Shutdown()
        {
            if (scrollViewer != null) scrollViewer.ScrollChanged -= scrollViewer_ScrollChanged;
        }

        internal void Add(ScrollBarHighlightAdorner scrollBarHighlightAdorner)
        {
            if (IsUsable) scrollBarAdornerLayer.Add(scrollBarHighlightAdorner);
        }

        internal void Remove(ScrollBarHighlightAdorner scrollBarHighlightAdorner)
        {
            if (IsUsable) scrollBarAdornerLayer.Remove(scrollBarHighlightAdorner);
        }

        /// <summary>
        /// Height of the horizontal scroll bar.
        /// </summary>
        public double HorizontalScrollBarHeight
        {
            get
            {
                return elementHorizontalScrollBarHeight;
            }
        }

        internal void Update()
        {
            if (IsUsable) scrollBarAdornerLayer.Update();
        }



        internal void AttachTo(Highlight highlight)
        {
            highlight.ScrollBarAdorner = CreateScrollBarHighlightAdorner(highlight.BodyAdorner);
            Add(highlight.ScrollBarAdorner);
        }
    }

}
