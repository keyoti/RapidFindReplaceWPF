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
   
    class TextBoxHighlightHandler : HighlightHandler
    {
        string currentText, oldText;
        [System.Reflection.Obfuscation(Exclude = true)]
        public new static bool DoesHandle(DependencyObject element)
        {
            return element is TextBox;
        }


        System.Windows.Controls.TextBox textBox;
        public TextBoxHighlightHandler(System.Windows.Controls.TextBox elementToHighlight, System.Windows.Media.Brush bodyHighlightAdornerBrush, System.Windows.Media.Pen bodyHighlightAdornerPen, Brush bodyIterativeHighlightAdornerBrush, Pen bodyIterativeHighlightAdornerPen)
            : base(elementToHighlight, bodyHighlightAdornerBrush, bodyHighlightAdornerPen, bodyIterativeHighlightAdornerBrush, bodyIterativeHighlightAdornerPen)
        {
            
            textBox = elementToHighlight;
            oldText = textBox.Text;
            textBox.TextChanged += textBox_TextChanged;
        }

        public override void Shutdown()
        {
            textBox.TextChanged -= textBox_TextChanged;
            base.Shutdown();
        }

        void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentText = textBox.Text;
            DoUpdates();
        }

        internal override int GetIndexOfFirstHighlightAfterSelection()
        {
            int p = highlights.BinarySearch(new Highlight(textBox.SelectionStart, textBox.SelectionStart, null, null), underlineComparer);
            if (p < 0) p = ~p;
            return p;
        }


        public override void AddHighlight(Run run, int index, int length)
        {
            TextBoxHighlightAdorner adorner = new TextBoxHighlightAdorner(textBox, index, length, scrollBarHighlightHandler.HorizontalScrollBarHeight, BodyHighlightAdornerBrush, BodyHighlightAdornerPen, BodyIterativeHighlightAdornerBrush, BodyIterativeHighlightAdornerPen);
            RegisterHighlight(new TextBoxHighlight(index, index + length, adorner, run));
        }


        int[] changeIndices;


        ///<summary> Returns the start and end indices in the current text of the changed text.</summary>
        internal static int[] IdentifyChange(string currentText, string oldText)
        {
            //find start of difference in oldText and currentText
            int changeStart = -1;

            //scan from start (but only as far as shortest text.
            int scanLength = currentText.Length > oldText.Length ? oldText.Length : currentText.Length;

            for (int i = 0; i < scanLength && changeStart < 0; i++)
                if (currentText[i] != oldText[i]) changeStart = i;

            if (changeStart == -1) changeStart = currentText.Length > oldText.Length ? oldText.Length : currentText.Length;

            int changeEnd = -1;
            for (int i = 1; i <= scanLength && changeEnd < 0; i++)
                if (currentText[currentText.Length - i] != oldText[oldText.Length - i]) changeEnd = currentText.Length - i + 1;

            if (changeEnd < changeStart) changeEnd = changeStart;	//this happens when deleting identical chars: 'axxxd'->'axxd'

            if (scanLength == 0)
            {
                changeStart = 0;
                changeEnd = currentText.Length;
            }

            int[] changePoints = { changeStart, changeEnd };
            return changePoints;
        }


        internal void DoUpdates()
        {

            Highlight tu;

            //get indices of change
            changeIndices = TextBoxHighlightHandler.IdentifyChange(currentText, oldText);


            
            
            
                      
            
            //go through underlines and adjust for change
            int changeMagnitude = currentText.Length - oldText.Length;

            
            //set old text to current
            oldText = currentText;


            for (int i = 0; i < highlights.Count && i >= 0; i++)
            {
                tu = highlights[i];

                //text preceding text in change text US after UE change
                //                   ^--------------------------^
                //			removal
                //move start and end by change magnitude, if this moves index past start of change
                //then set the index to the point of change. (true if index was in removal seciton)

                if (changeIndices[0] == tu.Start || changeIndices[0] == tu.End ||
                    changeIndices[1] == tu.Start || changeIndices[1] == tu.End)
                    tu.Hidden = false;

                int newStart = tu.Start, newEnd = tu.End;

                //change was before underline, update to where underline will be NOW.
                if (changeIndices[0] <= tu.Start)
                {		//if the change began before the underline start (doesnt matter where it ended)
                    if (tu.Start + changeMagnitude >= changeIndices[0])
                    {
                        newStart = tu.Start + changeMagnitude;
                    }
                    else
                    {
                        newStart = changeIndices[0];
                    }
                }
                if (changeIndices[0] < tu.End)
                {		//if the change began before the underline end (doesnt matter where it ended)
                    if (tu.End + changeMagnitude >= changeIndices[0])
                    {
                        newEnd = tu.End + changeMagnitude;
                    }
                    else
                    {
                        newEnd = changeIndices[0];
                    }
                }

                if (newStart != tu.Start || newEnd != tu.End)
                    MoveHighlight(tu, newStart, newEnd);



                int c0 = changeIndices[0]
                , c1 = changeIndices[1]
                , us = tu.Start
                , ue = tu.End;



                //We must remove underlines that have been tampered with, because although in most cases it wont matter, if the tamper is an added or removed
                //space we must let the spell check add correct lines.  One underline broken into two, will not happen without this.
                if (
                    (c0 >= us && c0 < ue) ||							//change start is inside underline
                    (c1 > us && c1 <= ue) ||							//change end is inside underline
                    (c0 <= us && c1 >= ue) ||							//change is around underline
                    (us == ue)								//underline shrunk to nothing
                    )
                {
                    RemoveHighlight(tu.Start, tu.End);

                    --i;
                    continue;
                }
            }

            List<Run> runs = new List<Run>();
            int rescanStart = changeIndices[0];
            rescanStart -= 30;//recheck some words before
            if (rescanStart < 0) rescanStart = 0;
            else{//go from the next space
                int possibleStart;
                if ((possibleStart = currentText.IndexOf(' ', rescanStart)) > -1 && possibleStart<changeIndices[0]) rescanStart = possibleStart;
            }
            int rescanEnd = changeIndices[1];
            rescanEnd += 30;
            if (rescanEnd > currentText.Length) rescanEnd = currentText.Length;
            else
            {//go to a prev space
                int possibleEnd;
                if (rescanEnd < currentText.Length && (possibleEnd = currentText.LastIndexOf(' ', rescanEnd)) > -1 && possibleEnd > changeIndices[1]) rescanEnd = possibleEnd;
            }

            runs.Add(new Run(currentText.Substring(rescanStart, rescanEnd - rescanStart).PadLeft(rescanEnd)));
            OnNewSearchNeeded(runs);

            AdornerLayer.Update();
            

        }

        

        


    }
}
