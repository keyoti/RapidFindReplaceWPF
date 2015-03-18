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
    /// <summary>
    /// Default highlight adorner.
    /// </summary>
    public class HighlightAdorner : Adorner
    {
        /// <summary>
        /// Event fired when the adorner could not be rendered.
        /// </summary>
        public event EventHandler RenderImpossible;
        /// <summary>
        /// Whether the adorner was in the viewport (is visible).
        /// </summary>
        protected bool isInViewport;
        /// <summary>
        /// Whether the adorner was in the viewport (is visible).
        /// </summary>
        public bool IsInViewport { get { return isInViewport; } }
        //internal HighlightHandler handler;
        /// <summary>
        /// The Run the highlight is in
        /// </summary>
        protected Run run;
        /// <summary>
        /// The start of the highlight
        /// </summary>
        protected int start;
        /// <summary>
        /// The length of the index.
        /// </summary>
        protected int length;
        /// <summary>
        /// The Y coordinate of the top of this highlight.
        /// </summary>
        protected double yPos=-1;
        /// <summary>
        /// The Y coordinate of the top of this highlight.
        /// </summary>
        public double Y
        {
            get
            {
                //return yPos;
                if (yPos == -1)
                {
                    var textPointer = run.ContentStart;
                    if (!textPointer.HasValidLayout)
                        yPos = -1;
                    else
                    {
                        textPointer = textPointer.GetPositionAtOffset(start, LogicalDirection.Forward);
                        //yPos hasn't been calc'd yet, so figure it
                        yPos = GetCharacterRects(textPointer, null)[0].Top;
                    }
                }
                
                return yPos;
            }
        }

        /// <summary>
        /// Brush used to render the adorner.
        /// </summary>
        protected Brush renderBrush, iterativeRenderBrush, normalRenderBrush;
        /// <summary>
        /// Pen used to draw the adorner.
        /// </summary>
        protected Pen renderPen, iterativeRenderPen, normalRenderPen;
        /// <summary>
        /// The height of the horizontal scroll bar.
        /// </summary>
        protected double horizontalScrollBarHeight;
        /// <summary>
        /// New instance.
        /// </summary>
        /// <param name="adornedElement">The element that will be adorned.</param>
        /// <param name="run">The Run to highlight</param>
        /// <param name="start">The start index of the highlight</param>
        /// <param name="length">The length of the highlight</param>
        /// <param name="horizontalScrollBarHeight">Height of the horizontal scrollbar</param>
        /// <param name="bodyHighlightAdornerBrush">Brush to paint highlight with</param>
        /// <param name="bodyHighlightAdornerPen">Pen to paint highlight border with, specifying a pen can slow down painting when highlights span multiple lines.</param>
        /// <param name="bodyIterativeHighlightAdornerBrush">Brush to paint iterative highlight with</param>
        /// <param name="bodyIterativeHighlightAdornerPen">Pen to draw iterative highlight border with</param>
        public HighlightAdorner(UIElement adornedElement, Run run, int start, int length, double horizontalScrollBarHeight, Brush bodyHighlightAdornerBrush, Pen bodyHighlightAdornerPen, Brush bodyIterativeHighlightAdornerBrush, Pen bodyIterativeHighlightAdornerPen)
            : base(adornedElement)
        {

            this.run = run;
            this.start = start;
            this.length = length;
            //this.handler = handler;
            IsHitTestVisible = false;
            renderBrush = bodyHighlightAdornerBrush;
            renderPen = bodyHighlightAdornerPen;
            normalRenderBrush = renderBrush;
            normalRenderPen = renderPen;
            iterativeRenderBrush = bodyIterativeHighlightAdornerBrush;
            iterativeRenderPen = bodyIterativeHighlightAdornerPen;


           // renderBrush = (Brush) TryFindResource(new ComponentResourceKey(typeof(Keyoti.RapidFindReplace.WPF.RapidFindReplaceControl), "BodyHighlightAdornerBrush"));
           // renderBrush.Freeze();
            
            this.horizontalScrollBarHeight = horizontalScrollBarHeight;
        }

        /// <summary>
        /// New instance.
        /// </summary>
        protected HighlightAdorner(UIElement adornedElement) : base(adornedElement) { }

        /// <summary>
        /// Cleans up event handlers
        /// </summary>
        public virtual void Unregister()
        {
        }

        

        //returns [0]=Rect of char at end of first line, [1]=Rect of char at start of second line
        internal static Rect[] FindStartEndOfVisualLineRects(TextPointer start, Rect startRect)
        {
            TextPointer next = start;
            Rect nextRect;
            while ((next = next.GetNextInsertionPosition(LogicalDirection.Forward)) != null)
            {
                nextRect = next.GetCharacterRect(LogicalDirection.Forward);
                if (nextRect.Top != startRect.Top)
                    return new Rect[2] { next.GetCharacterRect(LogicalDirection.Backward), nextRect };//for some reason next.GetCharacterRect(LogicalDirection.Backward) is not the same as 'prior' (prev value of 'next')
            }
            return null;
        }

        /// <summary>
        /// Fires render impossible event
        /// </summary>
        protected void OnRenderImpossible()
        {
            if (RenderImpossible != null)
                RenderImpossible(this, EventArgs.Empty);
        }


        internal virtual Rect[] GetCharacterRects(TextPointer startPointer, TextPointer endPointer)
        {
            if(endPointer!=null)
                return new Rect[2]{startPointer.GetCharacterRect(LogicalDirection.Forward), endPointer.GetCharacterRect(LogicalDirection.Backward)};
            else
                return new Rect[1] { startPointer.GetCharacterRect(LogicalDirection.Forward) };
        }

        // A common way to implement an adorner's rendering behavior is to override the OnRender 
        // method, which is called by the layout system as part of a rendering pass. 
        /// <summary>
        /// Renders the adorner.
        /// </summary>
        protected override void OnRender(DrawingContext drawingContext)
        {
            isInViewport = false;


            var textPointer = run.ContentStart;


            if (!textPointer.HasValidLayout)
            {
                OnRenderImpossible();

                return;
            }

            
         //   drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, this.AdornedElement.RenderSize.Width, this.AdornedElement.RenderSize.Height - SystemParameters.ScrollHeight)));

            textPointer = textPointer.GetPositionAtOffset(start, LogicalDirection.Forward);
            if (textPointer == null) return;
            var textPointerEnd = textPointer.GetPositionAtOffset(length, LogicalDirection.Forward);
            /*
            var leftRectangle = GetCharacterRect(textPointer, LogicalDirection.Forward); //textPointer.GetCharacterRect(LogicalDirection.Forward);           
            var rightRectangle = GetCharacterRect(textPointerEnd, LogicalDirection.Backward); //textPointerEnd.GetCharacterRect(LogicalDirection.Backward);
            */
            Rect[] rects = GetCharacterRects(textPointer, textPointerEnd);
            Rect leftRectangle = rects[0], rightRectangle = rects[1];
            yPos = leftRectangle.Top;
                
            //clip if the rects are out of view
            AdjustRectanglesToView(ref leftRectangle, ref rightRectangle, AdornedElement.RenderSize.Height - horizontalScrollBarHeight);

            if (leftRectangle.Top > 1 && rightRectangle.Y > 1 && leftRectangle.Top < (AdornedElement.RenderSize.Height - horizontalScrollBarHeight) - 5)
            {
                isInViewport = true;
                if (RoughlyTheSame(leftRectangle.Top, rightRectangle.Top))
                {
                    //notsplit over more than 1 line
                    var rect = new Rect(leftRectangle.TopLeft, rightRectangle.BottomRight);

                    rect.Inflate(1, 1);
                    drawingContext.DrawRectangle(renderBrush, renderPen, rect);
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
                        Rect[] points = FindStartEndOfVisualLineRects(textPointer, leftRectangle);
                        DrawMultiLine(drawingContext, renderBrush, renderPen, ref leftRectangle, ref rightRectangle, points);
                    }
                }
            }
      //      drawingContext.Pop();
        }

        /// <summary>
        /// Adjust character rectangles to fit in the viewport.
        /// </summary>
        protected static void AdjustRectanglesToView(ref Rect leftRectangle, ref Rect rightRectangle, double viewPortHeight)
        {
            if (leftRectangle.Top < 2 && leftRectangle.Bottom >= 2)
            {
                leftRectangle.Height += leftRectangle.Top;
                leftRectangle.Y = 2;
            }
            if (rightRectangle.Top < 2 && rightRectangle.Bottom >= 2)
            {
                rightRectangle.Height += rightRectangle.Top;
                rightRectangle.Y = 2;
            }

            if (leftRectangle.Bottom > viewPortHeight - 5 && leftRectangle.Top < viewPortHeight-5)
                leftRectangle.Height -= leftRectangle.Bottom - (viewPortHeight-5);

            if (rightRectangle.Bottom > viewPortHeight - 5 && rightRectangle.Top < viewPortHeight - 5)
                rightRectangle.Height -= rightRectangle.Bottom - (viewPortHeight - 5);

        }

        /// <summary>
        /// Draws the highlight over multiple lines.  This is optimized when there is no <c>renderPen</c>
        /// </summary>
        public static void DrawMultiLine(DrawingContext drawingContext, Brush renderBrush, Pen renderPen, ref Rect leftRectangle, ref Rect rightRectangle, Rect[] points)
        {
            double lineStartX = points[1].Left;
            double lineEndX = points[0].Right;
            if (!(lineEndX - leftRectangle.X <= 0 || rightRectangle.Right - lineStartX <= 0))
            {
                Rect r1 = new Rect(leftRectangle.X, leftRectangle.Y, lineEndX - leftRectangle.X, leftRectangle.Height);
                Rect r2;
                if (rightRectangle.Top - leftRectangle.Bottom > 0)
                    r2 = new Rect(lineStartX, leftRectangle.Bottom, lineEndX - lineStartX, rightRectangle.Top - leftRectangle.Bottom);
                else
                    r2 = new Rect(lineStartX, leftRectangle.Bottom, lineEndX - lineStartX, -rightRectangle.Top + leftRectangle.Bottom);

                Rect r3 = new Rect(lineStartX, rightRectangle.Top, rightRectangle.Right - lineStartX, rightRectangle.Height);
                r1.Inflate(1, 0);
                r2.Inflate(1, 0);
                r3.Inflate(1, 0);

                if (renderPen == null)
                {
                    //efficient way
                    drawingContext.DrawRectangle(renderBrush, null, r1);
                    drawingContext.DrawRectangle(renderBrush, null, r2);
                    drawingContext.DrawRectangle(renderBrush, null, r3);
                }
                else
                {
                    PathFigure pf = new PathFigure();
                    pf.StartPoint = leftRectangle.TopLeft;
                    pf.Segments.Add(new LineSegment(r1.TopRight, true));
                    pf.Segments.Add(new LineSegment(r2.BottomRight, true));
                    pf.Segments.Add(new LineSegment(r3.TopRight, true));
                    pf.Segments.Add(new LineSegment(r3.BottomRight, true));
                    pf.Segments.Add(new LineSegment(r3.BottomLeft, true));
                    pf.Segments.Add(new LineSegment(r2.TopLeft, true));
                    pf.Segments.Add(new LineSegment(r1.BottomLeft, true));
                    pf.Segments.Add(new LineSegment(r1.TopLeft, true));
                    pf.IsClosed=true;
                    pf.IsFilled=true;
                    pf.Freeze();
                   PathGeometry highlightShape = new PathGeometry();
                   highlightShape.Figures.Add(pf);
                   drawingContext.DrawGeometry(renderBrush, renderPen, highlightShape);
                }
            }
        }




        internal static bool RoughlyTheSame(double p, double p_2)
        {
            if (p == p_2) return true;
            else
            {
                if (Math.Abs(p_2 - p) > p / 100) return false;
                else return true;
            }
        }




        //this is relative to the Run.
        internal virtual void BumpUnderlineTo(Run run, int start, int end)
        {
            this.run = run;
            this.start = start;
            this.length = end-start;
        }



        internal void IterativeHighlight()
        {
            renderBrush = iterativeRenderBrush;
            renderPen = iterativeRenderPen;
            InvalidateVisual();
        }

        internal void NormalHighlight()
        {
            renderBrush = normalRenderBrush;
            renderPen = normalRenderPen;
            InvalidateVisual();
        }
    }

}
