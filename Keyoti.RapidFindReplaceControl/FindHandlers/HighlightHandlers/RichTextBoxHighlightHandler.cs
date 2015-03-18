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
    

    class RichTextBoxHighlightHandler: HighlightHandler
    {
        string currentText, oldText;
        int[] changeIndices;
        RichTextBoxHelper adapter;
        System.Windows.Controls.RichTextBox textBox;

        [System.Reflection.Obfuscation(Exclude = true)]
        public new static bool DoesHandle(DependencyObject element)
        {
            return element is RichTextBox;
        }

        public RichTextBoxHighlightHandler(System.Windows.Controls.RichTextBox elementToHighlight, System.Windows.Media.Brush bodyHighlightAdornerBrush, System.Windows.Media.Pen bodyHighlightAdornerPen, Brush bodyIterativeHighlightAdornerBrush, Pen bodyIterativeHighlightAdornerPen)
            : base(elementToHighlight, bodyHighlightAdornerBrush, bodyHighlightAdornerPen, bodyIterativeHighlightAdornerBrush, bodyIterativeHighlightAdornerPen)
        {
            textBox = elementToHighlight;
            adapter = new RichTextBoxHelper(elementToHighlight);
            oldText = adapter.Text;
            textBox.TextChanged += textBox_TextChanged;
            highlightComparer = new RichTextBoxHighlightComparer();

            
        }

        public override void Shutdown()
        {
            adapter.Dispose();
            textBox.TextChanged -= textBox_TextChanged;
            base.Shutdown();
        }

        internal override int GetIndexOfFirstHighlightAfterSelection()
        {
            Run r;
            TextPointer highlightPointer;
            for (int i = 0; i < highlights.Count; i++)
            {
                r = highlights[i].Run;
                highlightPointer = r.ContentStart.GetPositionAtOffset(highlights[i].Start);
                if (highlightPointer.CompareTo(textBox.Selection.Start) >= 0) return i;

            }
            return 0;
            /*int start = adapter.GetIndexOfPointer(textBox.Selection.Start);
            int p = highlights.BinarySearch(new Highlight(start, start, null, null), underlineComparer);
            if (p < 0) p = ~p;
            return p;*/
        }

        #region Highlight methods
        void ClearHighlightsInParagraph(Paragraph para)
        {
            for(int i=0; i<highlights.Count; i++){
                if (RunIsInParagraph(highlights[i].Run, para))
                {
                    RemoveHighlight(highlights[i]);
                }
            }
        }

        public override void AddHighlight(Run run, int index, int length)
        {
            RichTextBoxHighlightAdorner adorner = new RichTextBoxHighlightAdorner(textBox, run, index, length, scrollBarHighlightHandler.HorizontalScrollBarHeight, BodyHighlightAdornerBrush, BodyHighlightAdornerPen, BodyIterativeHighlightAdornerBrush, BodyIterativeHighlightAdornerPen);
            RegisterHighlight(new RichTextBoxHighlight(index, index + length, adorner, run, adapter.GetIndexOfPointer(run.ContentStart)));
        }
        
       void MoveHighlight(RichTextBoxHighlight highlight, TextPointer startPoint, int length)
        {
            //find the Run
            Run run = null ;
            //if (startPoint.Paragraph != null)
                run = GetRun(startPoint, startPoint.Paragraph);
            //else if(startPoint.Parent
             //   run = GetRun(startPoint, startPoint.Parent);

            if (run != null)
            {
                int start = run.ContentStart.GetOffsetToPosition(startPoint);
                highlight.MoveTo(run, start, start + length, adapter.GetIndexOfPointer(run.ContentStart));
            }
            else
            {
                //we seem to have lost the adorner but still tracking the highlight, so lose it too
                RemoveHighlight(highlight);
            }
        }
        #endregion

  


        #region Run based methods
        private bool RunIsInParagraph(Run run, Paragraph para)
        {
            TextElement obj, pObj = run;
            while ((obj = pObj.Parent as TextElement) != null)
            {
                if (obj == para) return true;
                pObj = obj;
            }
            return false;
        }
        List<Run> GetRuns(TextPointer p, TextPointer p2)
        {
            List<Run> runs = new List<Run>();
            List<Block> blocks = GetBlocksBetween(p, p2);

            if (blocks.Count == 1) runs.Add(GetRun(p, blocks[0]));//there's just 1 paragraph holding the pointers, so assume we can just look at one run, simple case of character insertion
            else //it's more complex and spans paras, therefore just check all runs in the paras
            {
                foreach (Block para in GetBlocksBetween(p, p2))
                {
                   
                    AddRuns(para, runs);
                }
            }
              
            return runs;
        }

        void AddRuns(Block b, List<Run> runs)
        {
            if (b is Paragraph)
            {
                foreach (Inline i in (b as Paragraph).Inlines)
                {
                    if (i is Run)
                    {
                        runs.Add(i as Run);
                    }
                }
            }
            if (b is Section)
            {
                foreach (Block subBlock in (b as Section).Blocks)
                    AddRuns(subBlock, runs);
            }
            if (b is List)
            {
                foreach (ListItem li in (b as List).ListItems)
                {
                    foreach (Block subBlock in li.Blocks)
                        AddRuns(subBlock, runs);
                }
            }
            if (b is Table)
            {
                foreach (TableRowGroup trg in (b as Table).RowGroups)
                {
                    foreach (TableRow row in trg.Rows)
                    {
                        foreach (TableCell cell in row.Cells)
                        {
                            foreach (Block subBlock in cell.Blocks)
                                AddRuns(subBlock, runs);
                        }

                    }
                }
            }
        }

        Run GetRun(TextPointer p, Block b)
        {
            if (b.ContentStart.CompareTo(p) <= 0 && b.ContentEnd.CompareTo(p) >= 0)
            {
                Run run;
                if (b is Paragraph) return GetRun(p, b as Paragraph);
                if (b is Section)
                {
                    foreach (Block subBlock in (b as Section).Blocks)
                    {
                        run = GetRun(p, subBlock);
                        if (run != null) return run;
                    }
                    return null;
                }
                if (b is List)
                {
                    foreach (ListItem li in (b as List).ListItems)
                    {
                        foreach (Block subBlock in li.Blocks)
                        {
                            run = GetRun(p, subBlock);
                            if (run != null) return run;
                        }
                    }
                    return null;
                }
                if (b is Table)
                {
                    foreach (TableRowGroup trg in (b as Table).RowGroups)
                    {
                        foreach (TableRow row in trg.Rows)
                        {
                            foreach (TableCell cell in row.Cells)
                            {
                                foreach (Block subBlock in cell.Blocks)
                                {
                                    run = GetRun(p, subBlock);
                                    if (run != null) return run;
                                }
                            }

                        }
                    }
                    return null;
                }
            }
            return null;
        }

        Run GetRun(TextPointer p,  Paragraph para)
        {
            foreach (Inline i in para.Inlines)
            {
                Run r = GetRun(p, i);
                if (r != null) return r;
            }
            return null;
        }

        Run GetRun(TextPointer p, Inline i)
        {
            if (i.ContentStart.CompareTo(p) <= 0 && i.ContentEnd.CompareTo(p) >= 0)
            {
                Run r;
                r = i as Run;
                if (r != null)
                {
                    return r;
                }
                else if (i is Span)
                {
                    foreach (Inline subInline in (i as Span).Inlines)
                    {
                        r = GetRun(p, subInline);
                        if (r != null) return r;
                    }
                }
                else if (i is AnchoredBlock)
                {
                    foreach (Block subBlock in (i as AnchoredBlock).Blocks)
                    {
                        r = GetRun(p, subBlock);
                        if (r != null) return r;
                    }
                }
            }
            return null;
        }

        List<Block> GetBlocksBetween(TextPointer p1, TextPointer p2)
        {
            List<Block> blocks = new List<Block>();
            foreach (Block block in textBox.Document.Blocks)
            {
                //Paragraph para = block as Paragraph;
                if (block !=null && 
                    p1.CompareTo(block.ContentEnd)<=0 && //p1 is before the end of the paragraph
                    p2.CompareTo(block.ContentStart)>=0  //p2 is after the start of the paragraph
                    )
                {
                    blocks.Add(block);
                }
            }
            return blocks;
        }
#endregion

        #region Text change scanning
        void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentText = adapter.Text;
            DoUpdates();
        }

        internal void DoUpdates()
        {
            RichTextBoxHighlight tu;
            List<Run> newCheckRuns = new List<Run>();
            //get indices of change
            changeIndices = TextBoxHighlightHandler.IdentifyChange(currentText, oldText);

            TextPointer[] editPoints = adapter.GetPointerAtIndex(changeIndices[0], changeIndices[1]);
            if (editPoints[0] != null && editPoints[1]!=null)
            {
                newCheckRuns.AddRange(GetRuns(editPoints[0], editPoints[1]));
            }


            //go through underlines and adjust for change
            int changeMagnitude = currentText.Length - oldText.Length;


            //set old text to current
            oldText = currentText;

            //need highlights to be in sorted order for highlightIndexes to work
            highlights.Sort(highlightComparer);

            List<TextPointer> highlightPointers = new List<TextPointer>(highlights.Count);
            List<int[]> highlightIndexes = new List<int[]>(highlights.Count);
            List<RichTextBoxHighlight> highlightsToMove = new List<RichTextBoxHighlight>(highlights.Count);

            bool willMove = false;

            

            for (int i = 0; i < highlights.Count && i >= 0; i++)
            {
                willMove = false;
                tu = highlights[i] as RichTextBoxHighlight;

                //text preceding text in change text US after UE change
                //                   ^--------------------------^
                //			removal
                //move start and end by change magnitude, if this moves index past start of change
                //then set the index to the point of change. (true if index was in removal seciton)

                if (changeIndices[0] == tu.AbsoluteStart || changeIndices[0] == tu.AbsoluteEnd ||
                    changeIndices[1] == tu.AbsoluteStart || changeIndices[1] == tu.AbsoluteEnd)
                    tu.Hidden = false;

                int newStart = tu.AbsoluteStart, newEnd = tu.AbsoluteEnd;

                //change was before underline, update to where underline will be NOW.
                if (changeIndices[0] <= tu.AbsoluteStart)
                {		//if the change began before the underline start (doesnt matter where it ended)
                    if (tu.AbsoluteStart + changeMagnitude >= changeIndices[0])
                    {
                        newStart = tu.AbsoluteStart + changeMagnitude;
                    }
                    else
                    {
                        newStart = changeIndices[0];
                    }
                }
                if (changeIndices[0] < tu.AbsoluteEnd)
                {		//if the change began before the underline end (doesnt matter where it ended)
                    if (tu.AbsoluteEnd + changeMagnitude >= changeIndices[0])
                    {
                        newEnd = tu.AbsoluteEnd + changeMagnitude;
                    }
                    else
                    {
                        newEnd = changeIndices[0];
                    }
                }

                if (newStart != tu.AbsoluteStart || newEnd != tu.AbsoluteEnd)
                {
                    willMove = true;
                
                 //   highlightIndexes.Add(new int[2]{newStart, newEnd});
                 //   highlightsToMove.Add(tu);
                /*    TextPointer[] startPoints = adapter.GetPointerAtIndex(newStart, newEnd);
                    if(startPoints!=null)
                        MoveHighlight(tu, startPoints[0],newEnd-newStart);
                  */   
                }
                    //MoveHighlight(tu, newStart, newEnd, true);



                int c0 = changeIndices[0]
                , c1 = changeIndices[1]
                , us = tu.AbsoluteStart
                , ue = tu.AbsoluteEnd;



                //We must remove underlines that have been tampered with, because although in most cases it wont matter, if the tamper is an added or removed
                //space we must let the spell check add correct lines.  One underline broken into two, will not happen without this.
                if (
                    (c0 >= us && c0 < ue) ||							//change start is inside underline
                    (c1 > us && c1 <= ue) ||							//change end is inside underline
                    (c0 <= us && c1 >= ue) ||							//change is around underline
                    (us == ue)								//underline shrunk to nothing
                    )
                {
                    RemoveHighlight(tu);
                    willMove=false;
                    //newCheckRuns.Add(tu.Run);


                    --i;
                 //   continue;
                }

                if (willMove && newStart!=newEnd)
                {
                    highlightIndexes.Add(new int[2] { newStart, newEnd });
                    highlightsToMove.Add(tu);
                }

            }

            if (highlightsToMove.Count > 0)
            {
                //highlightIndexes.Clear();
                //highlightIndexes.Add(new int[] { 11382, 1 });
                adapter.GetPointerAtIndexes(highlightIndexes, highlightPointers);
                for (int i = 0; i < highlightsToMove.Count; i++)
                {
                    if (i >= highlightPointers.Count)//we have a highlight left over from an edit it seems
                    {
                        RemoveHighlight(highlightsToMove[i]);
                    }
                    else
                    {
                        TextPointer startPoints = highlightPointers[i]; //adapter.GetPointerAtIndex(newStart, newEnd);
                        if (startPoints != null)
                            MoveHighlight(highlightsToMove[i], startPoints, highlightIndexes[i][1] - highlightIndexes[i][0]);
                    }
                }
            }
            

            OnNewSearchNeeded(newCheckRuns);
            AdornerLayer.Update();


        }
        #endregion
    }
}
