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
using System.Windows.Media;
using Keyoti.RapidFindReplace.WPF;

namespace Keyoti.RapidFindReplace.WPF.FindHandlers.HighlightHandlers
{
    class HighlightHandlers : List<HighlightHandler>
    {
        public event Keyoti.RapidFindReplace.WPF.FindHandlers.HighlightHandlers.HighlightHandler.NewSearchNeededEventHandler NewSearchNeeded;

        protected void OnNewSearchNeeded(NewSearchNeededEventArgs e)
        {
            if (NewSearchNeeded != null) NewSearchNeeded(this, e);
        }

        IFindText searcher;
        public HighlightHandlers(IFindText searcher)
        {
            this.searcher = searcher;
        }

        public void ResetIterators()
        {
            iteratingHandlerPtr = -1;
            startHandlerIndex = -1;
            foreach (HighlightHandler handler in this)
                handler.ResetIterator();
        
        }

        /// <summary>
        /// Resets the state back to having no highlight handlers
        /// </summary>
        public void Reset()
        {

            ResetIterators();
            foreach (HighlightHandler handler in this)
            {
                //handler.ResetIterator();
                handler.Shutdown();
                handler.NewSearchNeeded -= newHandler_NewSearchNeeded;
                
            }
            Clear();
        }

        //public event EventHandler NewSearchNeeded;
        public HighlightHandler GetCreateHighlightHandler(DependencyObject element, System.Windows.Media.Brush bodyHighlightAdornerBrush, System.Windows.Media.Pen bodyHighlightAdornerPen, Brush bodyIterativeHighlightAdornerBrush, Pen bodyIterativeHighlightAdornerPen)
        {
            int existingHandlerIndex = GetHighlightHandlerIndex(element);
            if (existingHandlerIndex>=0) return this[existingHandlerIndex];

            HighlightHandler newHandler = FindHandlerRegistry.CreateHighlightHandlerFor(element, bodyHighlightAdornerBrush, bodyHighlightAdornerPen, bodyIterativeHighlightAdornerBrush, bodyIterativeHighlightAdornerPen);
            
            Add(newHandler);
            newHandler.NewSearchNeeded += newHandler_NewSearchNeeded;
            newHandler.ElementToHighlightGotFocus += newHandler_ElementToHighlightGotFocus;
            return newHandler;
        }



        int GetHighlightHandlerIndex(object element)
        {
            for(int i=0; i<Count; i++)
            {
                if (this[i].ElementToHighlight == element)
                    return i;
            }
            return -1;
        }

        void newHandler_NewSearchNeeded(object sender, NewSearchNeededEventArgs e)
        {
            OnNewSearchNeeded(e);
        }

        internal int TotalHitCount()
        {
            int hits = 0;
            foreach (HighlightHandler handler in this)
                hits += handler.HitCount;
            return hits;
        }


        public Highlight SelectNextMatch()
        {
            if (CurrentMatch != null) CurrentMatch.Deselect();
            
            Highlight h = NextMatch();
            if (h != null)
            {
                ignoreFocusChange = true;
                h.Select();
                ignoreFocusChange = false;
            }
            return h;
        }

        public Highlight SelectPreviousMatch()
        {
            if (CurrentMatch != null) CurrentMatch.Deselect();
           
            Highlight h = PreviousMatch();
            if (h != null)
            {
                ignoreFocusChange = true;
                h.Select();
                ignoreFocusChange = false;
            }
            return h;
        }

        bool focusHasChanged, ignoreFocusChange;
        void newHandler_ElementToHighlightGotFocus(object sender, EventArgs e)
        {
            focusHasChanged = !ignoreFocusChange;
        }

        int iteratingHandlerPtr = -1;
        int focussedControlIndex;
        int startHandlerIndex=-1;
        bool wrapForward;
        internal Highlight NextMatch()
        {
            wrapForward = true;
            if (iteratingHandlerPtr == -1) iteratingHandlerPtr = 0;

            focussedControlIndex = GetHandlerIndexOfLogicalFocusedControl();
            if (startHandlerIndex == -1)
                startHandlerIndex = focussedControlIndex;

                       
            //in case foccusedControlIndex was -1
            if (startHandlerIndex == -1) startHandlerIndex = 0;

            if (focusHasChanged)
            {
                if (CurrentHandler != null) CurrentHandler.ResetIterator();
                startHandlerIndex = focussedControlIndex;
                iteratingHandlerPtr = 0;
                focusHasChanged = false;
            }

            if (CurrentHandler == null) return null;

            if(CurrentHandler.NextMatch())
            {
                return CurrentHandler.CurrentMatch;
            }
            else 
            {
                do
                {
                    iteratingHandlerPtr++;                    
                } while (CurrentHandler!=null && !CurrentHandler.NextMatch());

                if (CurrentHandler == null)//ran out
                {
                    iteratingHandlerPtr = -1;
                    return null;
                }
                
                
            }
            return CurrentHandler.CurrentMatch;
        }


        internal Highlight PreviousMatch()
        {
            wrapForward = false;
            if (iteratingHandlerPtr == -1) iteratingHandlerPtr = 0;
            focussedControlIndex = GetHandlerIndexOfLogicalFocusedControl();
            if (startHandlerIndex == -1)
                startHandlerIndex = focussedControlIndex;

            if (startHandlerIndex == -1) startHandlerIndex = 0;

            if (focusHasChanged)
            {
                CurrentHandler.ResetIterator();
                //startHandlerIndex = focussedControlIndex+1;//************this appears to work but there is a problem when changing direction, prev to next
                startHandlerIndex = focussedControlIndex;
                //iteratingHandlerPtr = Count - 1;
                iteratingHandlerPtr = 0;
                focusHasChanged = false;
            }

            if (CurrentHandler.PreviousMatch())
            {
                return CurrentHandler.CurrentMatch;
            }
            else
            {
                //no matches for current handler, so cycle through handlers until find a match
                do
                {
                    iteratingHandlerPtr++;
                } while (CurrentHandler != null && !CurrentHandler.PreviousMatch());

                if (CurrentHandler == null)//ran out
                {
                    iteratingHandlerPtr = - 1;
                    return null;
                }

            }

            return CurrentHandler.CurrentMatch;
        }

       


        private int GetHandlerIndexOfLogicalFocusedControl()
        {
            if (searcher.FindIterativelyFrom == null) return 0;
            else 
                return GetHighlightHandlerIndex(searcher.FindIterativelyFrom);
        }




        int MapHandlerWrap(int linearWrapIndex)
        {
            if(wrapForward)//we want to go forward through the handlers, 0 (linearWrapIndex) = first element
                return (linearWrapIndex + startHandlerIndex) % Count;
            else//go backward 0 = last
                return (((Count-1) - linearWrapIndex) + startHandlerIndex) % Count;

        }


       

        HighlightHandler CurrentHandler
        {
            get
            {
                if (iteratingHandlerPtr < 0 || iteratingHandlerPtr >= Count || startHandlerIndex==-1) return null;
                else return this[MapHandlerWrap(iteratingHandlerPtr)];
            }
        }



        public Highlight CurrentMatch
        {
            get
            {
                if (CurrentHandler != null)
                    return CurrentHandler.CurrentMatch;
                else return null;
            }
        }





        internal void RemoveMatch(Highlight match)
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Handles(match))
                {
                    this[i].RemoveHighlight(match);
                    break;
                }
            }
        }
    }
}
