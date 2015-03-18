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
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Keyoti.RapidFindReplace.WPF.TextMatching;
using Keyoti.RapidFindReplace.WPF.FindHandlers;
using Keyoti.RapidFindReplace.WPF.FindHandlers.HighlightHandlers;
using System.ComponentModel;

namespace Keyoti.RapidFindReplace.WPF
{

    /// <summary>
    /// Interface for classes that can do find operations.
    /// </summary>
    interface IFindText
    {
        /// <summary>
        /// Find text and highlight it.
        /// </summary>
        /// <param name="element">The DependencyObject (usually as UIElement) that should be searched.</param>
        void FindText(DependencyObject element);
        /// <summary>
        /// Find text and highlight it.
        /// </summary>
        /// <param name="runs">Collection of text Run objects that should be searched.</param>
        /// <param name="addHighlightDelegate">Method to call when highlights are found and need to be highlighted.</param>
        /// <param name="searchText">The query to search for.</param>
        void FindTextIn(List<Run> runs, Keyoti.RapidFindReplace.WPF.RapidFindReplaceControlViewModel.AddHighlight addHighlightDelegate, Query searchText);
        /// <summary>
        /// What to search in.
        /// </summary>
        DependencyObject FindScope { get; }
        /// <summary>
        /// What to search with in.
        /// </summary>
        DependencyObject FindIterativelyFrom { get; }
    }

    /// <summary>
    /// View model that performs actual searches.
    /// </summary>
    public class RapidFindReplaceControlViewModel : DependencyObject, IFindText
    {
    


        int maximumHitsToHighlight = 250;
        /// <summary>
        /// The number of highlights at which the highlighter will give up looking for new highlights
        /// </summary>
        public int MaximumHitsToHighlight
        {
            get { return maximumHitsToHighlight; }
            set { maximumHitsToHighlight = value; }
        }
        internal FocusMonitor focusMonitor;

        /// <summary>
        /// Fired when the search has finished.
        /// </summary>
        public event EventHandler FinishedSearching;

        /// <summary>
        /// The find options OptionsViewModel.
        /// </summary>
        public readonly static DependencyProperty FindOptionsProperty = DependencyProperty.Register("FindOptions", typeof(OptionsViewModel), typeof(RapidFindReplaceControlViewModel), new PropertyMetadata( new PropertyChangedCallback(FindOptionsChanged)));

        private static void FindOptionsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            //hook up handlers for property changes in the options object
            if (e.OldValue != null) (e.OldValue as OptionsViewModel).PropertyChanged -= (sender as RapidFindReplaceControlViewModel).options_PropertyChanged;
            if (e.NewValue != null) (e.NewValue as OptionsViewModel).PropertyChanged += (sender as RapidFindReplaceControlViewModel).options_PropertyChanged;

        }

        //When an option value changes, we receive notification here.
        void options_PropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(e.Property==OptionsViewModel.FindAsYouTypeProperty){
                if ((bool)e.NewValue == false)
                {
                    ResetHighlights();
                    NumberOfHits = 0;
                }
            }
            Query.ResetMatchers();
            if(FindOptions.FindAsYouType)
                FindTextCommand.Execute(null);
            
        }

        DependencyObject specifiedFindFrom = null;
        /// <summary>
        /// Which control to iteratively find matches from.
        /// </summary>
        public DependencyObject FindIterativelyFrom {
            get
            {

                if (specifiedFindFrom != null) return specifiedFindFrom;
                else
                {
                    if (focusMonitor != null)
                        return focusMonitor.LastKeyboardFocussedControl(this);
                    else return null;
                }
            }
            set
            {
                specifiedFindFrom = value;
            }
        }
        
        /// <summary>
        /// The options for Find operations.
        /// </summary>
        public OptionsViewModel FindOptions
        {
            get { return (OptionsViewModel)GetValue(FindOptionsProperty); }
            internal set { SetValue(FindOptionsProperty, value); }
        }

        /// <summary>
        /// The history of queries entered by the user.
        /// </summary>
        public readonly static DependencyProperty QueryHistoryProperty = DependencyProperty.Register("QueryHistory", typeof(ObservableBackwardsRingBuffer<string>), typeof(RapidFindReplaceControlViewModel));
        /// <summary>
        /// The history of queries entered by the user.
        /// </summary>
        public ObservableBackwardsRingBuffer<string> QueryHistory
        {
            get { return (ObservableBackwardsRingBuffer<string>)GetValue(QueryHistoryProperty); }
            set { SetValue(QueryHistoryProperty, value); }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        /// <summary>
        /// The current match that the user has iterated to using the next/previous buttons.
        /// </summary>
        public readonly static DependencyProperty CurrentMatchProperty = DependencyProperty.Register("CurrentMatch", typeof(Highlight), typeof(RapidFindReplaceControlViewModel));
        /// <summary>
        /// The current match that the user has iterated to using the next/previous buttons.
        /// </summary>
        public Highlight CurrentMatch
        {
            get { return (Highlight)GetValue(CurrentMatchProperty); }
            internal set { SetValue(CurrentMatchProperty, value); }
        }




        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary>
        /// Number of query history items to hold and display.
        /// </summary>
        public readonly static DependencyProperty QueryHistoryCapacityProperty = DependencyProperty.Register("QueryHistoryCapacity", typeof(int), typeof(RapidFindReplaceControlViewModel), new PropertyMetadata(new PropertyChangedCallback(QueryHistoryCapacityChanged)));
        static void QueryHistoryCapacityChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            RapidFindReplaceControlViewModel model = (sender as RapidFindReplaceControlViewModel);
            if(((int)e.NewValue)!=model.QueryHistory.Capacity)
                model.QueryHistory.Resize((int)e.NewValue);
        }


        /// <summary>
        /// Number of query history items to hold and display.
        /// </summary>
        public int QueryHistoryCapacity
        {
            get { return (int)GetValue(QueryHistoryCapacityProperty); }
            set { SetValue(QueryHistoryCapacityProperty, value); }
        }


  

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary>
        /// The query to search for.
        /// </summary>
        public readonly static DependencyProperty QueryProperty = DependencyProperty.Register("Query", typeof(Query), typeof(RapidFindReplaceControlViewModel), new PropertyMetadata(new Query("")));

        /// <summary>
        /// The query to search for.
        /// </summary>
        public Query Query
        {
            get { return (Query)GetValue(QueryProperty); }
            set { SetValue(QueryProperty, value); }
        }

        /// <summary>
        /// Control and it's children to find within.
        /// </summary>
        public readonly static DependencyProperty FindScopeProperty = DependencyProperty.Register("FindScope", typeof(DependencyObject), typeof(RapidFindReplaceControlViewModel), new PropertyMetadata(new PropertyChangedCallback(FindScopeChanged)));
        private static void FindScopeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            RapidFindReplaceControlViewModel kf = sender as RapidFindReplaceControlViewModel;
            if (kf.focusMonitor == null)
            {
                kf.focusMonitor = FocusMonitor.GetCreateInstanceFor(e.NewValue as DependencyObject);
            }
            else
                kf.focusMonitor.BeginMonitoring(e.NewValue as DependencyObject);

        }


        /// <summary>
        /// Control and it's children to find within.
        /// </summary>
        public DependencyObject FindScope
        {
            get { return (DependencyObject)GetValue(FindScopeProperty); }
            set { SetValue(FindScopeProperty, value); }
        }

        /// <summary>
        /// Number of hits found.
        /// </summary>
        public readonly static DependencyProperty NumberOfHitsProperty = DependencyProperty.Register("NumberOfHits", typeof(int), typeof(RapidFindReplaceControlViewModel));
        /// <summary>
        /// Number of hits found.
        /// </summary>
        public int NumberOfHits
        {
            get { return (int)GetValue(NumberOfHitsProperty); }
            set { SetValue(NumberOfHitsProperty, value); }
        }
        
        /// <summary>
        /// Adds a query to history.
        /// </summary>
        /// <param name="query"></param>
        public virtual void AddQueryToHistory(string query)
        {
            if (query == null || query.Length == 0) return;
            bool isPresentInHistory = false;

            foreach (string item in QueryHistory)
            {
                if (string.Compare(item, query, !FindOptions.MatchCase) == 0)
                {
                    isPresentInHistory = true;
                    break;
                }
            }


            if (!isPresentInHistory)
            {
                QueryHistory.AddItem(query);
            }

        }

        /// <summary>
        /// New
        /// </summary>
        public RapidFindReplaceControlViewModel()
        {
            
            Query = new Keyoti.RapidFindReplace.WPF.Query("");//dont really understand this, but the query was persisting across form openings.
            QueryHistory = new ObservableBackwardsRingBuffer<string>(5);
            QueryHistoryCapacity = 5;
            FindOptions = new OptionsViewModel();
            FindTextCommand = new ActionCommand(delegate()
            {
                FindText(FindScope/*, Query*/);
            }, () => true);

            SelectPreviousMatchCommand = new ActionCommand(delegate()
            {
                SelectPreviousMatch();
            }, () => true);

            SelectNextMatchCommand = new ActionCommand(delegate()
            {
                SelectNextMatch();
            }, () => true);

            ReplaceMatchCommand = new ActionCommand(delegate(string replacement) { 
                ReplaceMatch(/*Query.ProcessReplacementSyntax(*/ replacement /*)*/); 
                }, ()=>true);
            
            ReplaceAllMatchesCommand = new ActionCommand(delegate(string replacement) {
                ReplaceAllMatches(/*Query.ProcessReplacementSyntax(*/replacement/*)*/); 
                }, () => true);


        }
        
        /// <summary>
        /// Executes the find.
        /// </summary>
        public ICommand FindTextCommand
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Selects the next match.
        /// </summary>
        public ICommand SelectNextMatchCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Selects the previous match.
        /// </summary>
        public ICommand SelectPreviousMatchCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Selects the next match.
        /// </summary>
        public ICommand ReplaceMatchCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Selects the previous match.
        /// </summary>
        public ICommand ReplaceAllMatchesCommand
        {
            get;
            private set;
        }

       
        /// <summary>
        /// Replace the current match with <c>replacement</c>
        /// </summary>
        public void ReplaceMatch(string replacement)
        {
            int start = -replacement.Length;
            if (CurrentMatch != null) 
            {
                //Because the replacement might cause another match, we will want to skip the next 
                //CurrentMatch if it's within the scope of the replacement.
                start = CurrentMatch.AbsoluteStart;
        
                CurrentMatch.ReplaceText( Query.ProcessReplacementSyntax(replacement, CurrentMatch) );
                CurrentMatch.Deselect();
                highlightHandlers.RemoveMatch(CurrentMatch);
            }

            SelectNextMatch();

            //incase we selected in the region that we just replaced, move on
            while (CurrentMatch != null && !(CurrentMatch.AbsoluteEnd <= start || CurrentMatch.AbsoluteStart >= start + replacement.Length))
                SelectNextMatch();
        }

       // bool blockResearch = false;
        /// <summary>
        /// Replace all matches with <c>replacement</c>
        /// </summary>
        public void ReplaceAllMatches(string replacement)
        {
          //  blockResearch = true;
            highlightHandlers.ResetIterators();
            if (CurrentMatch == null)
                SelectNextMatch();

       //     int start = -replacement.Length;


            while (CurrentMatch != null /*&& (CurrentMatch.AbsoluteEnd <= start || CurrentMatch.AbsoluteStart >= start + replacement.Length)*/)
            {
          //      if (CurrentMatch != null)
         //           start = CurrentMatch.AbsoluteStart;//Because the replacement might cause another match, we will want to skip the next 
                                                        //CurrentMatch if it's within the scope of the replacement.
                ReplaceMatch(replacement);
            }

            //blockResearch = false;
        }

        /// <summary>
        /// Selects the next match.
        /// </summary>
        public void SelectNextMatch()
        {
            
            if ((CurrentMatch = highlightHandlers.SelectNextMatch()) == null && FinishedSearching != null)
                FinishedSearching(this, EventArgs.Empty);
        }

        /// <summary>
        /// Selects the previous match.
        /// </summary>
        public void SelectPreviousMatch()
        {
  
            if ((CurrentMatch = highlightHandlers.SelectPreviousMatch()) == null && FinishedSearching != null)
                FinishedSearching(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets the current hit count.
        /// </summary>
        public int HitCount()
        {
            return highlightHandlers.TotalHitCount();
        }
        System.Windows.Media.Brush bodyHighlightAdornerBrush;
        /// <summary>
        /// Body highlight brush to use
        /// </summary>
        public System.Windows.Media.Brush BodyHighlightAdornerBrush
        {
            get { return bodyHighlightAdornerBrush; }
            set { bodyHighlightAdornerBrush = value; }
        }

        System.Windows.Media.Pen bodyHighlightAdornerPen;
        /// <summary>
        /// Pen to use to draw border around body highlights.
        /// </summary>
        public System.Windows.Media.Pen BodyHighlightAdornerPen
        {
            get { return bodyHighlightAdornerPen; }
            set { bodyHighlightAdornerPen = value; }
        }

        System.Windows.Media.Brush bodyIterativeHighlightAdornerBrush;
        /// <summary>
        /// Brush to use when painting iterative highlights.  Iterative highlighting occurs when the user presses next/previous buttons.
        /// </summary>
        public System.Windows.Media.Brush BodyIterativeHighlightAdornerBrush
        {
            get { return bodyIterativeHighlightAdornerBrush; }
            set { bodyIterativeHighlightAdornerBrush = value; }
        }

        System.Windows.Media.Pen bodyIterativeHighlightAdornerPen;
        /// <summary>
        /// Pen to use when drawing iterative highlights.  Iterative highlighting occurs when the user presses next/previous buttons.
        /// </summary>
        public System.Windows.Media.Pen BodyIterativeHighlightAdornerPen
        {
            get { return bodyIterativeHighlightAdornerPen; }
            set { bodyIterativeHighlightAdornerPen = value; }
        }


#if DEBUG
        /// <summary>
        /// 
        /// </summary>
        public static bool simulateSlow = false;
#endif



        HighlightHandlers highlightHandlers;
        RunQueue runQueue = new RunQueue(5);
       // Query searchText;

/*
        /// <summary>
        /// Gets an enumerator
        /// </summary>
        public IEnumerator<Highlight> GetHighlightEnumerator()
        {

            foreach (HighlightHandler handler in highlightHandlers)
            {
                foreach (Highlight highlight in handler.Highlights)
                {
                    yield return highlight;
                }
            }
        }
        */

        /// <summary>
        /// Removes highlights and resets the highlight iterator.
        /// </summary>
        public virtual void ResetHighlights()
        {
            CurrentMatch = null;
            if (highlightHandlers != null)
                highlightHandlers.Reset();
            else
            {
                highlightHandlers = new HighlightHandlers(this);
                highlightHandlers.NewSearchNeeded += highlightHandlers_NewSearchNeeded;
            }
        }

        void highlightHandlers_NewSearchNeeded(object sender, NewSearchNeededEventArgs e)
        {
            //if (!blockResearch)
            //{
                if (e.Runs == null)
                    FindText(e.HighlightHandler.ElementToHighlight);
                else
                    FindTextIn(e.Runs, e.HighlightHandler.AddHighlight, null);

                NumberOfHits = HitCount();
            //}
        }

        /// <summary>
        /// Finds the current query in <c>searchIn</c>.
        /// </summary>
        /// <param name="searchIn">A DependencyObject that can be searched (handler is registered with FindHandlerRegistry).</param>
        public virtual void FindText(DependencyObject searchIn/*, Query searchText*/)//, bool isAsYouTypeFind)
        {
            /*
            if (searchText != null)//if it's null we redo the last search
                this.searchText = searchText;
           
            if (this.searchText == null)//we don't know what to look for
                return;
             */


            if (Query == null || Query.QueryText == null) return;
            if (searchIn == null)
            {
                //find the Window
                searchIn = Utility.CurrentWindow();
                FindScope = searchIn;
            }

            ResetHighlights();

            if (Query != null && Query.QueryText != null && Query.QueryText.Length > 0)// && (!isAsYouTypeFind || searchText.Length>=asYouTypeMinimumCharacters))
                if (!FindText(searchIn, false)) //FindText return false if no elements were marked with IsFindable=true or =false, so do again and assume searchIn (which could be the window) is all searchable
                    FindText(searchIn, true);

            NumberOfHits = HitCount();
        }

        /// <summary>
        /// Determines whether <c>element</c> will be searched.
        /// </summary>
        /// <param name="element">The element to decide whether to search</param>
        /// <param name="isFindable">The value of the attached property 'IsFindable', if set on <c>element</c></param>
        /// <param name="parentUIElementIsFindable">Whether <c>element</c>'s parent object was findable, usually if <c>element</c> has not been set explicitly to IsFindable=false, 
        /// then we want to search the element if its parent was also findable.  If however IsFindable is explicitly set true, then we will search it regardless of its parent.</param>
        /// <param name="searchChildren">Whether children should be searched.</param>
        protected internal virtual bool WillSearchInElement(UIElement element, bool parentUIElementIsFindable, ref bool searchChildren, bool? isFindable)
        {
            //will find if either the parent was and this element isn't explicitly set false, or if it is explicitly set true, regardless of the parent.
            bool isAllowedToSearchDueToIsFindableAttribute = ((parentUIElementIsFindable && isFindable != false) || isFindable == true);
            bool willSearch = isAllowedToSearchDueToIsFindableAttribute;
            //see if element is of ignored type
            if (FindOptions.IgnoredElementTypes != null)
            {
                foreach (Type ignoredType in FindOptions.IgnoredElementTypes)
                {
                    //if the element is ignored type
                    if (ignoredType.IsAssignableFrom(element.GetType()) || element.GetType().IsAssignableFrom(ignoredType))
                    {
                        willSearch = false;
                        searchChildren = false;
                    }
                }
            }
            //see if there are exclusive types to search
            if (FindOptions.ExclusiveFindElementTypes != null && FindOptions.ExclusiveFindElementTypes.Count>0)
            {
                bool isExclusiveType = false;
                foreach (Type exclusiveType in FindOptions.ExclusiveFindElementTypes)
                {
                    //if the element is exclusive
                    if (exclusiveType.IsAssignableFrom(element.GetType()) || element.GetType().IsAssignableFrom(exclusiveType))
                    {
                        isExclusiveType = true;
                    }
                }
                searchChildren = isAllowedToSearchDueToIsFindableAttribute;//if we haven't been explicitly blocked by IsFindable=false attribute, then allow children to be findable
                willSearch = isAllowedToSearchDueToIsFindableAttribute && isExclusiveType;//we only want to find if we already were going to find AND its the exclusive type
                
            }

            return willSearch;
        }


        //called recursively, searchIn doesn't have to be a UIElement because we will look at it's children for any child UIElemenets
        /// <summary>
        /// Recursively searches <c>searchIn</c> for the Query.
        /// </summary>
        /// <param name="searchIn">DependencyObject to search inside.</param>
        /// <param name="parentUIElementIsFindable">Whether the parent UI element has been determined as 'findable'.  If the parent is findable then <c>searchIn</c> is finable too unless explicitly marked otherwise with IsFindable="false".</param>
        /// <returns>Whether searchIn or any of it's children where marked as either 'findable' or not (IsFindable="true" or IsFindable="false").</returns>
        protected bool FindText(DependencyObject searchIn, bool parentUIElementIsFindable)
        {
            bool foundIsFindableAttribute = false;//whether this element searchIn or its children were marked with isfindable

            bool searchChildren = parentUIElementIsFindable;
            if (searchIn == null)
            {
                return foundIsFindableAttribute;
            }
            currentGlobalScanHighlightCount = 0;
            bool? isFindable = searchIn is UIElement ? RapidFindReplaceControl.GetIsFindable(searchIn as UIElement) : true;
            
            if(searchIn is UIElement)
                foundIsFindableAttribute = RapidFindReplaceControl.GetIsFindable(searchIn as UIElement) != null;
            

            IRunReader runReader = null;
            if (searchIn is UIElement && WillSearchInElement(searchIn as UIElement, parentUIElementIsFindable, ref searchChildren, isFindable))//GetIsFindable is bool? - if null then just not set so use parentUIElementIsFindable
            {
                searchChildren = true;
                runReader = FindHandlerRegistry.CreateIRunReaderHandlerFor(searchIn);
            } else if (!(searchIn is UIElement))
                runReader = FindHandlerRegistry.CreateIRunReaderHandlerFor(searchIn);//we might be able to get a runreader for non UIElement (eg RunCollectionRunReader)

            

            if (runReader != null)
            {
                HighlightHandler handler = highlightHandlers.GetCreateHighlightHandler(searchIn, bodyHighlightAdornerBrush, bodyHighlightAdornerPen, bodyIterativeHighlightAdornerBrush, bodyIterativeHighlightAdornerPen);
                FindTextIn(runReader, handler.AddHighlight, Query);
            }


            //recurse through children
            if (searchIn is Visual || searchIn is System.Windows.Media.Media3D.Visual3D)
            {
                var childCount = VisualTreeHelper.GetChildrenCount(searchIn);

                for (var i = 0; i < childCount; ++i)
                {
                    foundIsFindableAttribute |= FindText(VisualTreeHelper.GetChild(searchIn, i), searchChildren && isFindable != false);
                }
            }
            return foundIsFindableAttribute;
        }











        /// <summary>
        /// Finds the <c>searchText</c> query in Runs returned by <c>runReader</c>.
        /// </summary>
        /// <param name="runReader">IRunReader that will return an enumeration of Runs to search in</param>
        /// <param name="addHighlightDelegate">Delegate to call when hits are found</param>
        /// <param name="searchText">The query to search for</param>
        public void FindTextIn(IRunReader runReader, AddHighlight addHighlightDelegate, Query searchText)
        {
            runQueue.Clear();
            currentGlobalScanHighlightCount = 0;
            foreach (Run run in runReader)
            {
                ProcessRun(addHighlightDelegate, run, runQueue, searchText);
            }
            ProcessRemainingQueuedRuns(addHighlightDelegate, runQueue, searchText);
        }
        /// <summary>
        /// Finds the <c>searchText</c> query in Runs.
        /// </summary>
        /// <param name="runs">Collection of runs to look in</param>
        /// <param name="addHighlightDelegate">Delegate to call when hits are found</param>
        /// <param name="searchText">The query to search for</param>
        public void FindTextIn(List<Run> runs, AddHighlight addHighlightDelegate, Query searchText)
        {
            if (searchText == null) searchText = Query;
            runQueue.Clear();
            currentGlobalScanHighlightCount = 0;
            foreach (Run run in runs)
            {
                ProcessRun(addHighlightDelegate, run, runQueue, searchText);
            }
            ProcessRemainingQueuedRuns(addHighlightDelegate, runQueue, searchText);
        }

        void ProcessRun(AddHighlight addHighlightDelegate, Run run, RunQueue runQueue, Query searchText)
        {
            if (run != null && !string.IsNullOrEmpty(run.Text))
            {

                runQueue.Enqueue(run);
                ScanQueuedRuns(runQueue, searchText, addHighlightDelegate);

            }
        }

        void ProcessRemainingQueuedRuns(AddHighlight addHighlightDelegate, RunQueue runQueue, Query query)
        {
            while (runQueue.Count > 0)
            {
                runQueue.Dequeue();
                ScanQueuedRuns(runQueue, query, addHighlightDelegate);
            }
        }

        /// <summary>
        /// The add highlight delegate, called when hits are found.  
        /// </summary>
        /// <param name="run">The Run where hit was found</param>
        /// <param name="index">Character index of hit</param>
        /// <param name="length">Length of hit</param>
        public delegate void AddHighlight(Run run, int index, int length);

     /*   public static void ScanQueuedRuns(RunQueue runQueue, string searchText, AddHighlight addHighlightDelegate)
        {

        }
        */
#if DEBUG
        //testing usage
        /// <summary>
        /// 
        /// </summary>
        /// <param name="runQueue"></param>
        /// <param name="searchText"></param>
        /// <param name="addHighlightDelegate"></param>
        public void ScanQueuedRuns(RunQueue runQueue, string searchText, AddHighlight addHighlightDelegate)
        {
            ScanQueuedRuns(runQueue, new Keyoti.RapidFindReplace.WPF.Query(searchText), addHighlightDelegate);
        }
#endif
        int currentGlobalScanHighlightCount = 0;
        /// <summary>
        /// Scans a queue of runs and highlights matches.
        /// </summary>
        /// <param name="runQueue">Run queue</param>
        /// <param name="query">Query</param>
        /// <param name="addHighlightDelegate">Delegate to call when a hit is found</param>
        public virtual void ScanQueuedRuns(RunQueue runQueue, Query query, AddHighlight addHighlightDelegate)
        {
            //this.searchText = query;
            System.Text.StringBuilder queuedText = new System.Text.StringBuilder(100);
            queuedText.Length = 0;
            for (int i = 0; i < runQueue.Count && (i <= 1 || queuedText.Length < 100); i++)//look at the text in queue up until 100 chars of the 2nd run
            {
                queuedText.Append(runQueue[i].HitAvailableText);

            }
            int currentIndex = 0;
            string text = queuedText.ToString();
            int index;
            Hit hit;
            


            TextMatchers textMatchers = query.GetTextMatchers(text, FindOptions);

            while ((hit = GetNextMatch(textMatchers, currentIndex)).Start > -1 && currentGlobalScanHighlightCount < maximumHitsToHighlight)//find a hit for searchText in the plain text version.
            {
                index = hit.Start;
                //we have a hit, we need to find the runs it was in and highlight
                int highlightStart;
                int gobbledChars = 0;
                int runHitLength = hit.Length;
                int highlightLength;
                //string searchSegment = query.QueryText;
                bool moreToFind = true;
                while (moreToFind)
                {
                    Run runAtPos = runQueue.HitPosition(index, runHitLength, out highlightStart, out gobbledChars);
                   
                    //gobbledChars is the number of chars in runAtPos that were used (this could be the entire run length if the runHitLength is less than the 
                    //number of chars in the run).
                    //indexOffset is where in the run to start highlighting
                    if (gobbledChars < runHitLength)
                    {
                        moreToFind = true;
                        //there weren't enough chars in the run, so we'll need to look for more
                        index += gobbledChars;
                        runHitLength -= gobbledChars;
                        highlightLength = gobbledChars;
                    }
                    else
                    {
                        moreToFind = false;
                        highlightLength = runHitLength;
                    }

                    addHighlightDelegate(runAtPos, highlightStart, highlightLength);
                    currentGlobalScanHighlightCount++;

                    currentIndex = index + runHitLength;
                }


            }
        }


#if DEBUG
        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="currentIndex"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public Hit GetNextMatch_FOR_TESTING(string searchText, int currentIndex, string text)
        {
            Query q = new Query(searchText);

            return GetNextMatch(q.GetTextMatchers(text, FindOptions), currentIndex);
            
        }
#endif 

        /// <summary>
        /// Returns the next match for <c>searchText</c> in <c>text</c> starting at <c>currentIndex</c>.  
        /// </summary>
        /// <remarks>Some options are mutually exclusive (eg. find whole words only and match prefix), find whole words takes precedence.</remarks>
        /// <param name="currentIndex">Where to start looking from</param>
        /// <param name="matchers">Matchers to use in search for next hit.</param>
        /// <returns>Where current query occurs or -1 if it doesn't.</returns>
        protected virtual Hit GetNextMatch(TextMatchers matchers, int currentIndex)
        {
            List<IEnumerator<Hit>> matcherEnumerators = new List<IEnumerator<Hit>>(15);
            IEnumerator<Hit> enumerator;
            bool allHaveMatches = true;
            //TextMatchers matchers = query.GetTextMatchers(FindOptions);
            for (int i = 0; i < matchers.Count; i++)
            {
                matchers[i].CurrentPosition = currentIndex;
                enumerator = matchers[i].GetEnumerator();
                matcherEnumerators.Add(enumerator);
                //init enumerator
                allHaveMatches &= enumerator.MoveNext();
            }

            

            //At this point, each matcher can be iterated over to give matches, eg.
            // matcher[0] => (has matches) [0, 5, 9]
            // matcher[1] => (has matches) [5]
            // matcher[2] => (has matches) [0, 3, 5, 9]

            //we want the AND, so, first 'round' gives
            //0, 5, 0 (for matchers 0,1 and 2)
            //for 1st round, find lowest matcher (0 or 2, so take 2), and advance it for next round.
            //2nd round gives
            //0, 5, 3
            //lowest is match 0, so advance it
            //3rd round gives
            //5,5,3
            //4th round
            //5,5,5 => hit, so return it.
            //otherwise if we never lined them all up, return -1.

            int lowestMatcherValue=0, lowestMatcher=0;
            bool isHit = false;
            int previousMatcherValue=-1;

            
            
    

            //loop through rounds, until find isHit (all match).
            while (allHaveMatches && !isHit)
            {
                isHit = true;
                //find the lowest matcher
                for (int i = 0; i < matcherEnumerators.Count; i++)
                {
                    if (matcherEnumerators[i].Current.Start <= lowestMatcherValue)
                    {
                        lowestMatcher = i;
                        lowestMatcherValue = matcherEnumerators[i].Current.Start;
                    }
                    if (i > 0) isHit &= matcherEnumerators[i].Current.Start == previousMatcherValue;

                    previousMatcherValue = matcherEnumerators[i].Current.Start;
                }

                if (!isHit)
                {
                    //move the lowest matcher forward
                    if (!matcherEnumerators[lowestMatcher].MoveNext())
                    {
                        break;
                    }
                }
                else return matcherEnumerators[0].Current;//they're all the same, so return first
            }

            return new Hit(-1,0);
            
            


        }

        
      
    }



    


    

   
    
    



}
