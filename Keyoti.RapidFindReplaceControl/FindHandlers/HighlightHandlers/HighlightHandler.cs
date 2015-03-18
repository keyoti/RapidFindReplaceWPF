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
    /// <summary>
    /// Event args for NewSearchNeededEvent
    /// </summary>
    public class NewSearchNeededEventArgs
    {
        /// <summary>
        /// Empty args.
        /// </summary>
        public static NewSearchNeededEventArgs Empty(HighlightHandler handler){return new NewSearchNeededEventArgs(null, handler);}
        List<Run> run;
        HighlightHandler hh;
        /// <summary>
        /// New instance.
        /// </summary>
        /// <param name="r">List of Run objects that need to be searched.</param>
        /// <param name="hh">Highlight handler to send hits to.</param>
        public NewSearchNeededEventArgs(List<Run> r, HighlightHandler hh) { run = r; this.hh = hh; }
        /// <summary>
        /// List of Run objects that need to be searched.
        /// </summary>
        public List<Run> Runs { get { return run; } }
        /// <summary>
        /// Highlight handler to send hits to.
        /// </summary>
        public HighlightHandler HighlightHandler { get { return hh; } }
    }

    /// <summary>
    /// Default highlight handler for UIElements, used unless a more suitable handler class is registered.
    /// </summary>
    public class HighlightHandler : DependencyObject
    {
        /// <summary>
        /// Fired when the UIElement gets focus.
        /// </summary>
        public event EventHandler ElementToHighlightGotFocus;

        internal ScrollBarHighlightHandler scrollBarHighlightHandler;
        
        /// <summary>
        /// Scrollbar track, if it exists in the UIElement that is being highlighted.
        /// </summary>
        protected System.Windows.Controls.Primitives.Track track;
        AdornerLayer _AdornerLayer;
        /// <summary>
        /// AdornerLayer hosting the highlight.
        /// </summary>
        protected virtual AdornerLayer AdornerLayer
        {
            get
            {
                if (_AdornerLayer == null) _AdornerLayer = AdornerLayer.GetAdornerLayer(uiElementToHighlight);
                return _AdornerLayer;
            }
        }

        /// <summary>
        /// Whether this HighlightHandler can handle the DependencyObject.
        /// </summary>
        [System.Reflection.Obfuscation(Exclude=true)]
        public static bool DoesHandle(DependencyObject element)
        {
            return element is UIElement;
        }

        /// <summary>
        /// Number of hits that this object is handling.
        /// </summary>
        public int HitCount
        {
            get { return highlights.Count; }
        }

        /// <summary>
        /// Collection of Highlight objects being handled.
        /// </summary>
        protected List<Highlight> highlights = new List<Highlight>();
        /// <summary>
        /// Fired when a new search is needed, typically because an option or the text has changed.
        /// </summary>
        public event NewSearchNeededEventHandler NewSearchNeeded;
        /// <summary>
        /// NewSearchNeeded event handler.
        /// </summary>
        public delegate void NewSearchNeededEventHandler(object sender, NewSearchNeededEventArgs e);
        bool needUpdateAdorners = false;

        /// <summary>
        /// Raises the NewSearchNeeded event.
        /// </summary>
        protected void OnNewSearchNeeded()
        {
            if (NewSearchNeeded != null) NewSearchNeeded(this, NewSearchNeededEventArgs.Empty(this));
        }

        /// <summary>
        /// Raises the NewSearchNeeded event.
        /// </summary>
        protected void OnNewSearchNeeded(List<Run> runs)
        {
            if (NewSearchNeeded != null) NewSearchNeeded(this, new NewSearchNeededEventArgs(runs, this));
        }

        DependencyObject elementToHighlight;
        UIElement uiElementToHighlight;

        /// <summary>
        /// The element to highlight.
        /// </summary>
        public DependencyObject ElementToHighlight
        {
            get { return elementToHighlight; }
        }
        /// <summary>
        /// New instance.
        /// </summary>
        /// <param name="elementToHighlight">The element being highlighted.</param>
        /// <param name="bodyHighlightAdornerBrush">Brush to paint highlight with</param>
        /// <param name="bodyHighlightAdornerPen">Pen to paint highlight border with, specifying a pen can slow down painting when highlights span multiple lines.</param>
        /// <param name="bodyIterativeHighlightAdornerBrush">Brush to paint iterative highlight with</param>
        /// <param name="bodyIterativeHighlightAdornerPen">Pen to draw iterative highlight border with</param>
        public HighlightHandler(DependencyObject elementToHighlight, Brush bodyHighlightAdornerBrush, Pen bodyHighlightAdornerPen, Brush bodyIterativeHighlightAdornerBrush, Pen bodyIterativeHighlightAdornerPen)
        {
            this.BodyHighlightAdornerBrush = bodyHighlightAdornerBrush;
            this.BodyHighlightAdornerPen = bodyHighlightAdornerPen;
            this.BodyIterativeHighlightAdornerBrush = bodyIterativeHighlightAdornerBrush;
            this.BodyIterativeHighlightAdornerPen = bodyIterativeHighlightAdornerPen;
            this.elementToHighlight = elementToHighlight;
            this.uiElementToHighlight = elementToHighlight as UIElement;
            if (this.uiElementToHighlight != null)
            {
                this.uiElementToHighlight.GotFocus += elementToHighlight_GotFocus;
                this.uiElementToHighlight.LayoutUpdated += elementToHighlight_LayoutUpdated;
                scrollBarHighlightHandler = new ScrollBarHighlightHandler(this.uiElementToHighlight);
            }
            //ProbeScrollElements(elementToHighlight);
        }

        void elementToHighlight_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ElementToHighlightGotFocus != null) ElementToHighlightGotFocus(this, EventArgs.Empty);
        }


        void elementToHighlight_LayoutUpdated(object sender, EventArgs e)
        {
            if (needUpdateAdorners)
            {
                if(AdornerLayer!=null)AdornerLayer.Update();
                //elementToHighlight.UpdateLayout();
                //scrollViewer.UpdateLayout();
            }
            needUpdateAdorners = false;
            
        }

        /// <summary>
        /// Clears all the highlighting.
        /// </summary>
        public void ClearHighlighting()
        {
            for (int i = highlights.Count - 1; i >= 0; i--)
                RemoveHighlight(highlights[i]);
            highlights.Clear();           
        }
        
        /// <summary>
        /// Removes an adorner when it is no longer used.
        /// </summary>
        /// <param name="adorner"></param>
        public virtual void RemoveAdorner(HighlightAdorner adorner)
        {
            
            if (adorner != null && AdornerLayer != null)
            {
                AdornerLayer.Remove(adorner);
                adorner.Unregister();
                adorner.RenderImpossible -= new EventHandler(adorner_RenderImpossible);
            }
        }

        /// <summary>
        /// Overrides should call RegisterHighlight to ensure proper registration of the Highlight.
        /// </summary>
        public virtual void AddHighlight(Run run, int index, int length)
        {
            HighlightAdorner adorner = new HighlightAdorner(uiElementToHighlight, run, index, length, scrollBarHighlightHandler.HorizontalScrollBarHeight, BodyHighlightAdornerBrush, BodyHighlightAdornerPen, BodyIterativeHighlightAdornerBrush, BodyIterativeHighlightAdornerPen);
            RegisterHighlight(new Highlight(index, index + length, adorner,  run));    
        }


        /// <summary>
        /// Registers a Highlight
        /// </summary>
        protected virtual bool RegisterHighlight(Highlight highlight){
#if DEBUG
            if (Keyoti.RapidFindReplace.WPF.RapidFindReplaceControlViewModel.simulateSlow) System.Threading.Thread.Sleep(80);
#endif
            if (!IsRegistered(highlight))
            {
                highlights.Add(highlight);
                if(highlight.BodyAdorner!=null)highlight.BodyAdorner.RenderImpossible += adorner_RenderImpossible;
                highlight.RunTextShift += highlight_RunTextShift;
                if (AdornerLayer != null) AdornerLayer.Add(highlight.BodyAdorner);
                if(scrollBarHighlightHandler!=null)scrollBarHighlightHandler.AttachTo(highlight);
                
                return true;
            }
            else return false;
        }

        void highlight_RunTextShift(object sender, RunTextShiftEventArgs e)
        {
            //a shift occurred in a run, so need to adjust all highlights in that run that occur after the shift
            Highlight highlight = sender as Highlight;
            for (int i = 0; i < highlights.Count; i++)
            {
                if (highlights[i].Run == highlight.Run && highlights[i].Start >= e.CharacterPosition)
                {
                    highlights[i].MoveTo(highlights[i].Start + e.Delta, highlights[i].End + e.Delta);
                }
            }
        }

        /// <summary>
        /// Highlight position comparer, used for binary searching.
        /// </summary>
        protected IComparer<Highlight> highlightComparer = new HighlightComparer();
        /// <summary>
        /// Determines whether a Highlight is registered.
        /// </summary>
        protected virtual bool IsRegistered(Highlight highlight){
            highlights.Sort(highlightComparer);
            return highlights.BinarySearch(highlight, highlightComparer)>-1;
        }


        void adorner_RenderImpossible(object sender, EventArgs e)
        {
            needUpdateAdorners = true;
        }

        /// <summary>
        /// Removes an existing underline
        /// </summary>        
        public virtual void RemoveHighlight(Highlight h)
        {
            RemoveAdorner(h.BodyAdorner);
            
            if (CurrentMatch != null && h.IsBeforeOrAt(CurrentMatch))            
                iteratingHighlightPtr--;//we have to move the iteratorPTR back since a highlight before it was deleted
            //if startHighlightIndex is after where highlight is, then it must be decremented
            //eg highlight indexes:0 1 2 3
            //startHighlightIndex=2
            //remove highlight #1, means startHighlightIndex=>2-1=1
            //but if remove highlight #3, startHighlightIndex doesnt change
            if(startHighlightIndex>=highlights.Count || h.IsBeforeOrAt(highlights[startHighlightIndex]))
                startHighlightIndex--;
            
            highlights.Remove(h);
            if(scrollBarHighlightHandler!=null)scrollBarHighlightHandler.Remove(h.ScrollBarAdorner);
        }

        /// <summary>
        /// Removes a highlight specified by it's character index.
        /// </summary>
        public virtual void RemoveHighlight(int start, int end){
            
            int count = highlights.Count;

            Highlight existing;
            int insertPos;

            if ((existing = FindHighlightAt(start, end, out insertPos, false)) != null && insertPos >= 0)
            {
                if (insertPos <= iteratingHighlightPtr)
                    iteratingHighlightPtr--;
                if (insertPos <= startHighlightIndex)
                    startHighlightIndex--;
                highlights.RemoveAt(insertPos);
                RemoveAdorner(existing.BodyAdorner);
                scrollBarHighlightHandler.Remove(existing.ScrollBarAdorner);
            }
              
        }
        internal HighlightComparer underlineComparer = new HighlightComparer();

        /// <summary>
        /// Returns a highlight at specified position.
        /// </summary>
        protected Highlight FindHighlightAt(int start, int end, out int insert, bool adjust)
        {
            int p = highlights.BinarySearch(new Highlight(start, end, null,null), underlineComparer);
            Highlight tu;


            if (p < 0)
            {
                insert = ~p;
                for (int i = insert > 0 ? (insert - 1) : 0; i < highlights.Count && i <= insert + 1; i++)//check insert-1, insert, and insert+1
                {
                    tu = highlights[i];
                    if ((tu.Start >= start && tu.End <= end) || (tu.Start <= start && tu.End >= end))//our search encompasses the underline, or viceversa
                    {
                        if (adjust)
                        {
                            MoveHighlight(tu, start, end);//we weren't bumping here (in RS), only because that was how the code was at time
                            //of refactoring, it's possible that it should be bumped - however testing has shown everything stable so far
                        }
                        insert = i;
                        return tu;

                    }

                }
                return null; //unfound
            }
            else
            {
                insert = p;
                return highlights[p];
            }

        }

        /// <summary>
        /// Moves the underline due to user operations (typing)
        /// </summary>
        protected virtual void MoveHighlight(Highlight u, int start, int end)
        {
            
           // u.Start = start;
           // u.End = end;
            u.MoveTo(start, end);
        }

        /// <summary>
        /// Shutdown the handler when no longer needed.
        /// </summary>
        public virtual void Shutdown()
        {
            foreach (Highlight highlight in highlights)
            {
                if (highlight.BodyAdorner != null) highlight.BodyAdorner.RenderImpossible -= adorner_RenderImpossible;
                highlight.RunTextShift -= highlight_RunTextShift;
            }
            if (uiElementToHighlight != null)
            {
                uiElementToHighlight.GotFocus -= elementToHighlight_GotFocus;
                uiElementToHighlight.LayoutUpdated -= elementToHighlight_LayoutUpdated;
            }
            if(scrollBarHighlightHandler!=null)scrollBarHighlightHandler.Shutdown();
            
            ClearHighlighting();            
            
        }


        /// <summary>
        /// Brush used for highlights.
        /// </summary>
        public System.Windows.Media.Brush BodyHighlightAdornerBrush { get; set; }
        /// <summary>
        /// Pen used to draw highlight border.
        /// </summary>
        public System.Windows.Media.Pen BodyHighlightAdornerPen { get; set; }
        /// <summary>
        /// Brush used for iterative highlights.
        /// </summary>
        public System.Windows.Media.Brush BodyIterativeHighlightAdornerBrush { get; set; }
        /// <summary>
        /// Pen used to draw highlight border, for iterative highlights.
        /// </summary>
        public System.Windows.Media.Pen BodyIterativeHighlightAdornerPen { get; set; }

        internal List<Highlight> Highlights { get { return highlights; } }

        //textbox and richtextbox highlight handlers are more interesting
        internal virtual int GetIndexOfFirstHighlightAfterSelection()
        {
            return 0;            
        }


        int iteratingHighlightPtr = -1;
        int startHighlightIndex=-1;
        internal bool NextMatch()
        {
            if (iteratingHighlightPtr >= Highlights.Count)//loop round when we've gone off the end, for this to have happened we would have returned false that time
                iteratingHighlightPtr = -1;
            
            iteratingHighlightPtr++;
            
            if (iteratingHighlightPtr >= Highlights.Count)
                return false;
            else 
                return true;

        }

        internal bool PreviousMatch()
        {
            if (iteratingHighlightPtr < 0) //loop round when we've gone off the end
                iteratingHighlightPtr = Highlights.Count;
            
            iteratingHighlightPtr--;
            
            if (iteratingHighlightPtr <0 )
                return false;
            else 
                return true;
        }

        //startHighlightIndex is the highlight to start iterating from (first highlight after the selection
        //linearWrapIndex is 0 (start) to N (end), where N=highlights.Count
        //return is real index in highlights list
        int MapSelectionWrap(int linearWrapIndex)
        {
            return (linearWrapIndex + startHighlightIndex) % Highlights.Count;
        }

        /// <summary>
        /// When the user iterates over matches, this property returns the current match that is highlighted.
        /// </summary>
        public Highlight CurrentMatch
        {
            get
            {
                if (startHighlightIndex == -1) //find where we're counting relative too, used by CurrentMatch
                    startHighlightIndex = GetIndexOfFirstHighlightAfterSelection();
                if (iteratingHighlightPtr < 0 || iteratingHighlightPtr >= Highlights.Count) return null;
                return Highlights[MapSelectionWrap(iteratingHighlightPtr)];
            }
        }

        /// <summary>
        /// Resets the match iterator
        /// </summary>
        public void ResetIterator()
        {
            startHighlightIndex = -1;
            iteratingHighlightPtr = -1;
        }




        internal bool Handles(Highlight match)
        {
            for (int i = 0; i < highlights.Count; i++)
            {
                if (highlights[i] == match) return true;
            }
            return false;
        }
    }
}
