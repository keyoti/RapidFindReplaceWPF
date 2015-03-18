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
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Documents;

namespace Keyoti.RapidFindReplace.WPF.FindHandlers.HighlightHandlers
{
    /// <summary>
    /// Highlight adorner for TextBoxBase.
    /// </summary>
    public class TextBoxBaseHighlightAdorner : HighlightAdorner
    {
        System.Windows.Controls.Primitives.TextBoxBase tbb;

        /// <summary>
        /// New instance.
        /// </summary>
        /// <param name="tbb">The textbox to highlight</param>
        /// <param name="run">The Run to highlight</param>
        /// <param name="start">The start index of the highlight</param>
        /// <param name="length">The length of the highlight</param>
        /// <param name="horizontalScrollBarHeight">Height of the horizontal scrollbar</param>
        /// <param name="bodyHighlightAdornerBrush">Brush to paint highlight with</param>
        /// <param name="bodyHighlightAdornerPen">Pen to paint highlight border with, specifying a pen can slow down painting when highlights span multiple lines.</param>
        /// <param name="bodyIterativeHighlightAdornerBrush">Brush to paint iterative highlight with</param>
        /// <param name="bodyIterativeHighlightAdornerPen">Pen to draw iterative highlight border with</param>
        public TextBoxBaseHighlightAdorner(System.Windows.Controls.Primitives.TextBoxBase tbb, Run run, int start, int length, double horizontalScrollBarHeight, Brush bodyHighlightAdornerBrush, Pen bodyHighlightAdornerPen, Brush bodyIterativeHighlightAdornerBrush, Pen bodyIterativeHighlightAdornerPen)
            : base(tbb, run, start, length, horizontalScrollBarHeight, bodyHighlightAdornerBrush, bodyHighlightAdornerPen, bodyIterativeHighlightAdornerBrush, bodyIterativeHighlightAdornerPen)
        {
            this.tbb = tbb;
            tbb.AddHandler(ScrollViewer.ScrollChangedEvent, new RoutedEventHandler(scroller_ScrollChanged));

        }



        /// <summary>
        /// Cleans up event handlers
        /// </summary>
        public override void Unregister()
        {
            tbb.RemoveHandler(ScrollViewer.ScrollChangedEvent, new RoutedEventHandler(scroller_ScrollChanged));
        }

        /// <summary>
        /// Handles scroll events.
        /// </summary>
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

    /// <summary>
    /// Highlight adorner for TextBox.
    /// </summary>
    public class TextBoxHighlightAdorner : TextBoxBaseHighlightAdorner
    {


        System.Windows.Controls.TextBox tb;
       // int start;
       // int length;

        /// <summary>
        /// New instance.
        /// </summary>
        /// <param name="adornedElement">The TextBox to highlight</param>
        /// <param name="start">Start index of the highlight</param>
        /// <param name="length">Length of the highlight</param>
        /// <param name="horizontalScrollBarHeight">Height of the horizontal scrollbar</param>
        /// <param name="bodyHighlightAdornerBrush">Brush to paint highlight with</param>
        /// <param name="bodyHighlightAdornerPen">Pen to paint highlight border with, specifying a pen can slow down painting when highlights span multiple lines.</param>
        /// <param name="bodyIterativeHighlightAdornerBrush">Brush to paint iterative highlight with</param>
        /// <param name="bodyIterativeHighlightAdornerPen">Pen to draw iterative highlight border with</param>
        public TextBoxHighlightAdorner(System.Windows.Controls.TextBox adornedElement, int start, int length, double horizontalScrollBarHeight, Brush bodyHighlightAdornerBrush, Pen bodyHighlightAdornerPen, Brush bodyIterativeHighlightAdornerBrush, Pen bodyIterativeHighlightAdornerPen)
            : base(adornedElement, null, start, length, horizontalScrollBarHeight, bodyHighlightAdornerBrush, bodyHighlightAdornerPen, bodyIterativeHighlightAdornerBrush, bodyIterativeHighlightAdornerPen)
        {
            tb = adornedElement;
        }



        //returns [0]=Rect of char at end of first line, [1]=Rect of char at start of second line

        Rect[] FindStartEndOfVisualLineRects(int start, Rect startRect)
        {

            int next = start, len = tb.Text.Length;
            Rect nextRect, prevRect = startRect;
            while ((next = start + 1) < len)
            {
                nextRect = tb.GetRectFromCharacterIndex(next); //next.GetCharacterRect(LogicalDirection.Forward);
                if (nextRect.Top != startRect.Top)
                    return new Rect[2] { prevRect, nextRect };

                prevRect = nextRect;
            }
            return null;
        }




        // A common way to implement an adorner's rendering behavior is to override the OnRender 
        // method, which is called by the layout system as part of a rendering pass. 
        /// <summary>
        /// Paints the highlight.
        /// </summary>
        protected override void OnRender(DrawingContext drawingContext)
        {
            
            //// Some arbitrary drawing implements.


            /* this is a nice shortcut but it means we can't set yPos, which is used by scroll highlighter
            int firstline = tb.GetFirstVisibleLineIndex();
            if (firstline == -1) { OnRenderImpossible(); return; }
            int lastline = tb.GetLastVisibleLineIndex();
            if (lastline == -1) { OnRenderImpossible(); return; }
            try
            {
                int charline = tb.GetLineIndexFromCharacterIndex(start);
                if (charline == -1) { OnRenderImpossible(); return; }
                if (charline < firstline - 1 || charline > lastline + 1) return;
            }
            catch (ArgumentOutOfRangeException) { OnRenderImpossible(); return; }
            */
            //drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, this.AdornedElement.RenderSize.Width, this.AdornedElement.RenderSize.Height)));

            var leftRectangle = tb.GetRectFromCharacterIndex(start);
            var rightRectangle = tb.GetRectFromCharacterIndex(start + length);

            if(Double.IsInfinity(leftRectangle.X) )
                OnRenderImpossible();

            yPos = leftRectangle.Top;

            AdjustRectanglesToView(ref leftRectangle, ref rightRectangle, this.AdornedElement.RenderSize.Height - horizontalScrollBarHeight);
            //       AdjustRectanglesToView(ref leftRectangle, ref rightRectangle, AdornedElement.RenderSize.Height - SystemParameters.ScrollHeight);

            if (leftRectangle.Top > 1 && rightRectangle.Y > 1 && leftRectangle.Top < (AdornedElement.RenderSize.Height - horizontalScrollBarHeight) &&
                leftRectangle.Left > 1 && rightRectangle.Right > 1 && leftRectangle.Left < (AdornedElement.RenderSize.Width - horizontalScrollBarHeight) //assume vertical scrollbar is as wide as horiz scroll is high
                
                
                )
            {

                if (rightRectangle.Left > AdornedElement.RenderSize.Width - horizontalScrollBarHeight-1)//if it ends to the right
                    rightRectangle.X = AdornedElement.RenderSize.Width - horizontalScrollBarHeight-1;

                isInViewport = true;
                if (RoughlyTheSame(leftRectangle.Top, rightRectangle.Top))
                {
                    //notsplit over more than 1 line
                    var rect = new Rect(leftRectangle.TopLeft, rightRectangle.BottomRight);

                    rect.Inflate(1, 1);
                    drawingContext.DrawRectangle(renderBrush, null, rect);
                }
                else
                {
                    if (leftRectangle != Rect.Empty && rightRectangle != Rect.Empty)
                    {
                        //paint as 3 rects
                        //r1 is top line from word to end of line
                        //r2 is start of line to end of line, between first line and last line
                        //r3 is last line from start of line to end of word

                        //we need to figure out the X coords of the start of the line and end of the line
                        Rect[] points = FindStartEndOfVisualLineRects(start, leftRectangle);
                        HighlightAdorner.DrawMultiLine(drawingContext, renderBrush, renderPen, ref leftRectangle, ref rightRectangle, points);
                    }
                }
            }
            else isInViewport = false;
            //drawingContext.Pop();

        }








    }
}
