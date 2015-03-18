using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using SearchVisualTree.FindHandlers;
using SearchVisualTree.FindHandlers.HighlightHandlers;

namespace Keyoti.KFindBox
{

    interface IFindText
    {
        void FindText(DependencyObject element, string searchText);
        void FindTextIn(List<Run> runs, Keyoti.KFindBox.DependencyObjectTextFinder.AddHighlight addHighlightDelegate, string searchText);
    }


    /*should the FindTextIn methods and other control specific code in highlighting be grouped into class or namespace and moved out of here
     especially when we have rtb and tb keyboard handling.  findtextin is more of a RunHandler or TextRunHandler 
     */
    public class DependencyObjectTextFinder: IFindText
    {
        public int HitCount()
        {
            int hits = 0;
            foreach (HighlightHandler handler in highlightHandlers)
            {
                hits += handler.HitCount;
            }
            return hits;
        }
        System.Windows.Media.Brush bodyHighlightAdornerBrush;

        public System.Windows.Media.Brush BodyHighlightAdornerBrush
        {
            get { return bodyHighlightAdornerBrush; }
            set { bodyHighlightAdornerBrush = value; }
        }

        System.Windows.Media.Pen bodyHighlightAdornerPen;

        public System.Windows.Media.Pen BodyHighlightAdornerPen
        {
            get { return bodyHighlightAdornerPen; }
            set { bodyHighlightAdornerPen = value; }
        }


#if DEBUG
        public static bool simulateSlow = false;
#endif

 

        HighlightHandlers highlightHandlers;
        RunQueue runQueue = new RunQueue(5);
        string searchText;

      /*  public void FindText(DependencyObject searchIn, string searchText)
        {
            FindText(searchIn, searchText, false);
        }
        */

        public virtual void ResetHighlights()
        {
            if (highlightHandlers != null)
                highlightHandlers.Reset();
            else
                highlightHandlers = new HighlightHandlers(this);
        }

        public virtual void FindText(DependencyObject searchIn, string searchText)//, bool isAsYouTypeFind)
		{
            if(searchText!=null)//if it's null we redo the last search
                this.searchText=searchText;

            ResetHighlights();
            
            if(searchText!=null && searchText.Length>0)// && (!isAsYouTypeFind || searchText.Length>=asYouTypeMinimumCharacters))
                FindText(searchIn, false);

            
        }


        //called recursively
        protected void FindText(DependencyObject searchIn, bool parentUIElementIsFindable){
            
			if (searchIn == null)
				return;
        
            bool? isFindable = searchIn is UIElement?  KFindBox.GetIsFindable(searchIn as UIElement) : true;
            IRunReader runReader = null;
            if (searchIn is UIElement && ((parentUIElementIsFindable && isFindable != false) || isFindable == true))//GetIsFindable is bool? - if null then just not set so use parentUIElementIsFindable
            {
                parentUIElementIsFindable = true;
                runReader = FindHandlerRegistry.CreateIRunReaderHandlerFor(searchIn);
            }

            if (runReader != null)
            {
                HighlightHandler handler = highlightHandlers.GetCreateHighlightHandler(searchIn as UIElement, bodyHighlightAdornerBrush, bodyHighlightAdornerPen);
                FindTextIn(runReader, handler.AddHighlight, searchText);
            }
            

			//recurse through children
			var childCount = VisualTreeHelper.GetChildrenCount(searchIn);

			for (var i = 0; i < childCount; ++i)
			{
                FindText(VisualTreeHelper.GetChild(searchIn, i), parentUIElementIsFindable);
			}
		}












        public void FindTextIn(IRunReader runReader, AddHighlight addHighlightDelegate, string searchText)
        {
            runQueue.Clear();
            foreach (Run run in runReader)
            {
                ProcessRun(addHighlightDelegate, run, runQueue, searchText);
            }
            ProcessRemainingQueuedRuns(addHighlightDelegate, runQueue, searchText);
        }
        
        public void FindTextIn(List<Run> runs, AddHighlight addHighlightDelegate, string searchText)
        {
            if (searchText == null) searchText = this.searchText;
            runQueue.Clear();   
            foreach (Run run in runs)
            {
                ProcessRun(addHighlightDelegate, run, runQueue, searchText);
            }
            ProcessRemainingQueuedRuns(addHighlightDelegate, runQueue, searchText);
        }
        
        public static void ProcessRun(AddHighlight addHighlightDelegate, Run run, RunQueue runQueue, string searchText)
        {
            if (run != null && !string.IsNullOrEmpty(run.Text))
            {

                runQueue.Enqueue(run);
                ScanQueuedRuns(runQueue, searchText, addHighlightDelegate);

            }
        }

        public static void ProcessRemainingQueuedRuns(AddHighlight addHighlightDelegate,RunQueue runQueue, string searchText)
        {
            while (runQueue.Count > 0)
            {
                runQueue.Dequeue();
                ScanQueuedRuns(runQueue, searchText, addHighlightDelegate);
            }
        }

        public delegate void AddHighlight(Run run, int index, int length);

        public static void ScanQueuedRuns(RunQueue runQueue, string searchText, AddHighlight addHighlightDelegate)
        {
            System.Text.StringBuilder queuedText = new System.Text.StringBuilder(100);
            queuedText.Length = 0;
            for (int i = 0; i < runQueue.Count && (i==0 || queuedText.Length < 100); i++)//look at the text in queue up until 100 chars of the 2nd run
            {
                queuedText.Append(runQueue[i].HitAvailableText);
                
            }
            int currentIndex = 0;
            string text = queuedText.ToString();
            int index;
            while ((index = text.IndexOf(searchText, currentIndex, StringComparison.CurrentCultureIgnoreCase)) >-1)//find a hit for searchText in the plain text version.
            {
                
                    //we have a hit, we need to find the runs it was in and highlight
                    int indexOffset;
                    int remainingChars=0;
                    int hitLength=0;
                    string searchSegment = searchText;
                    bool moreToFind = true;
                    while (moreToFind)
                    {
                        
                        Run runAtPos = runQueue.HitPosition(index, searchSegment.Length, out indexOffset, out remainingChars);
                        //remainingChars is # of chars in the run after the hit position - so if the hit spreads over 2 or more runs
                        //then remainingChars will be less than the searchSegment.
                        if (remainingChars < searchSegment.Length)
                        {
                            //partial match, find parts with rest
                            moreToFind = true;
                            searchSegment = searchText.Substring(remainingChars);
                            hitLength = remainingChars;
                            index = text.IndexOf(searchSegment, index + hitLength, StringComparison.CurrentCultureIgnoreCase);
                        }
                        else
                        {
                            moreToFind = false;
                            hitLength = searchSegment.Length;
                        }

                        
                        addHighlightDelegate(runAtPos, indexOffset, hitLength);

                        currentIndex = index + hitLength;
                    }


            }
        }


      
        
        /*
		private void ApplyHighlighting(string text, Run run, UIElement uiHost, IContentHost contentHost)
		{
            HighlightHandler handler = highlightHandlers.GetCreateHighlightHandler(uiHost);
            

			var currentIndex = 0;

            

			while (true)
			{
				var index = text.IndexOf(searchText, currentIndex, StringComparison.CurrentCultureIgnoreCase);

				if (index == -1)
				{
					return;
				}

				
                handler.AddHighlight(run, index, searchText.Length);

				currentIndex = index + searchText.Length;
			}
		}
         */

     
    }

    

}
