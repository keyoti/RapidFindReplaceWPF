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

namespace Keyoti.RapidFindReplace.WPF.FindHandlers.HighlightHandlers
{
    /// <summary>
    /// Convert TextPointers into plain text positions etc
    /// </summary>
    class RichTextBoxHelper
    {
        char symbolChar = (char)9999;//symbols marked by char 9999
        string symbol;
        System.Windows.Controls.RichTextBox txc;
        StringBuilder plainTextRegister = new StringBuilder();
        ///<summary>Creates a new proxy with a TextBoxBase</summary>
        public RichTextBoxHelper(System.Windows.Controls.RichTextBox textBox)
        {
            symbol = "" + symbolChar;
            this.txc = textBox;
            txc = textBox;
            InitPlainTextRegister();
            txc.TextChanged += txc_TextChanged;
        }

        void txc_TextChanged(object sender, TextChangedEventArgs e)
        {
            symbolTextInvalid = true;
            plainTextReady = false;
        }

        public void Dispose()
        {
            txc.TextChanged -= txc_TextChanged;
        }
    

        ///<summary>The text in the RichTextBox</summary>
        public string Text
        {
            get
            {
                InitPlainTextRegister();
                return plainTextRegister.ToString();
            }
        }

        bool plainTextReady = false;
        private void InitPlainTextRegister()
        {
            if (!plainTextReady)
            {
                TextPointer navigator = txc.Document.ContentStart;
                string content;
                plainTextRegister.Length = 0;
                while (navigator.CompareTo(txc.Document.ContentEnd) < 0)
                {
                    content = navigator.GetTextInRun(LogicalDirection.Forward);
                    if (content.Length > 0)
                    {
                        plainTextRegister.Append(content);
                    }
                    else if (isLineBreaker(navigator.Parent, navigator))
                        plainTextRegister.Append("\n");
                  
                    navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
                }
                if (plainTextRegister.Length>0 && plainTextRegister[plainTextRegister.Length - 1] == '\n')
                    plainTextRegister.Remove(plainTextRegister.Length - 1, 1);
                plainTextReady = true;
            }
        }

        bool isLineBreaker(DependencyObject parent, TextPointer navigator)
        {
            return (parent is Paragraph && (parent as Paragraph).ContentEnd.CompareTo(navigator) == 0)
                  || (parent is LineBreak && (parent as LineBreak).ContentEnd.CompareTo(navigator) == 0)
                  || (parent is ListItem && (parent as ListItem).ContentEnd.CompareTo(navigator) == 0);
        }

        StringBuilder symbolText = new StringBuilder();
        bool symbolTextInvalid = true;
        ///<summary></summary>
        string SymbolText
        {
            get
            {
                if (symbolTextInvalid)
                {
                    symbolText.Length = 0;
                    TextPointer navigator = txc.Document.ContentStart;
                    string content;
                    //int charOffset;
                    //bool pastFirstChar = false;
                    while (navigator.CompareTo(txc.Document.ContentEnd) < 0)
                    {
                        content = navigator.GetTextInRun(LogicalDirection.Forward);

                        if (isLineBreaker(navigator.Parent, navigator))
                            content = ("\n");
                        if (content.Length == 0)
                            content = symbol;


                        symbolText.Append(content);
                        navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
                    }
                    symbolTextInvalid = false;
                }
                return symbolText.ToString();

            }
        }

        int ConvertSymbolOffsetToPlain(int symbolOffset)
        {
            string symbolText = SymbolText;
            int offset = 0;
            for (int i = 0; i < symbolOffset; i++)
            {
                if (symbolText[i] != symbolChar)
                    offset++;
            }
            return offset;
        }

        /*//finds symbol offset match plain offset, gobbles as many symbols as possible (symbol offset is forward of previous insertion pt)
        int ConvertPlainOffsetToSymbol(int plainOffset)
        {
            string symbolText = SymbolText;
            string plainText = Text;
            int offset = 0;
            int symbols = 0;
            int i;
            for (i = 0; offset <= plainOffset && i<symbolText.Length; i++)
            {
                if (symbolText[i] != symbolChar)
                {
                    if (symbolText[i] == '\n')
                        symbols++;

                    offset++;
                }
                else
                {
  
                    symbols++;
                }
            }
            return --i;// plainOffset + symbols;
        }*/


        List<int> ConvertPlainOffsetsToSymbols(List<int[]> offsets)
        {
            List<int> symbolOffsets = new List<int>(offsets.Count);
            string symbolText = SymbolText;
            string plainText = Text;
            int symbolTextPtr, offset = 0, offsetsPtr = 0, targetPlainOffset = offsets[offsetsPtr++][0];
            for (symbolTextPtr = 0; symbolTextPtr < symbolText.Length; symbolTextPtr++)
            {
                if (symbolText[symbolTextPtr] != symbolChar)
                    offset++;


                if (offset == targetPlainOffset+1)
                {

                    symbolOffsets.Add(symbolTextPtr);

                    if (offsetsPtr == offsets.Count) break;
                    if (offsets[offsetsPtr-1][0] >= offsets[offsetsPtr][0]) throw new ApplicationException("Offsets not sorted");
                    targetPlainOffset = offsets[offsetsPtr++][0];
                }
            }
            return symbolOffsets;
        }

        int[] ConvertPlainOffsetToSymbol(int plainOffset, int plainOffset2)
        {
            string symbolText = SymbolText;
            string plainText = Text;
            //string plainText = Text;
            int[] symbolOffsets = new int[2];
            int offset = 0;
            int symbols = 0;
            int i;
            for (i = 0; offset <= plainOffset && i < symbolText.Length; i++)
            {
                if (symbolText[i] != symbolChar)
                {
                  //  if (symbolText[i] == '\n')
                  //      symbols++;

                    offset++;
                }
                else
                {
                 //   symbols++;
                }
            }
            symbolOffsets[0] = --i;
            if (plainOffset2 != -1)
            {
                for (i++; offset <= plainOffset2 && i < symbolText.Length; i++)
                {
                    if (symbolText[i] != symbolChar)
                    {
                        if (symbolText[i] == '\n')
                            symbols++;

                        offset++;
                    }
                    else
                    {
                        symbols++;
                    }
                }
                symbolOffsets[1] = --i;
            }
            else symbolOffsets[1] = -1;
            return symbolOffsets;
            
        }

        internal void GetPointerAtIndexes(List<int[]> highlightIndexes, List<TextPointer> highlightPointers)
        {
            List<int> symbols = ConvertPlainOffsetsToSymbols(highlightIndexes); //ConvertPlainOffsetToSymbol(ind1, ind2);
            for (int i = 0; i < symbols.Count; i++)
            {
                highlightPointers.Add(txc.Document.ContentStart.GetPositionAtOffset(symbols[i]));
            }
        }

        /// <summary>
        /// Gets TextPointers at two close indexes.
        /// </summary>
        public TextPointer[] GetPointerAtIndex(int ind1, int ind2)
        {
            TextPointer nav;// = txc.Document.ContentStart;

            TextPointer nav2=null;// = txc.Document.ContentStart;
            int[] symbols = ConvertPlainOffsetToSymbol(ind1, ind2);
            //int symbol2=-1;
            //if(ind2>=0) symbol2 = ConvertPlainOffsetToSymbol(ind2);
            nav = txc.Document.ContentStart.GetPositionAtOffset(symbols[0]);
            if (ind2 >= 0) nav2 = txc.Document.ContentStart.GetPositionAtOffset(symbols[1]);

            return new TextPointer[2] { nav, nav2 };
        }

        internal int GetIndexOfPointer(TextPointer targ)
        {
            
            return ConvertSymbolOffsetToPlain(txc.Document.ContentStart.GetOffsetToPosition(targ));
        }
     

        /// <summary>
        /// Gets the character index of a Point
        /// </summary>
        public int GetCharIndexFromPos(System.Windows.Point pos)
        {
            TextPointer p = txc.GetPositionFromPoint(pos, true);
            if (p != null)
                return ConvertSymbolOffsetToPlain(txc.Document.ContentStart.GetOffsetToPosition(p));
            else
                return -1;
        }



        /// <summary>
        /// Gets the Point of a char index.
        /// </summary>
        public System.Windows.Point GetPosFromCharIndex(int index)
        {            
            int sym = ConvertPlainOffsetToSymbol(index,-1)[0];
            Rect r = txc.Document.ContentStart.GetPositionAtOffset(sym).GetCharacterRect(LogicalDirection.Forward);
            return r.BottomLeft;
        }

        /// <summary>
        /// Gets a character rectangle around an char at index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Rect GetPosFromCharIndex2(int index)
        {

            int sym = ConvertPlainOffsetToSymbol(index,-1)[0];
            return txc.Document.ContentStart.GetPositionAtOffset(sym).GetCharacterRect(LogicalDirection.Forward);
        }









   
    }
}
