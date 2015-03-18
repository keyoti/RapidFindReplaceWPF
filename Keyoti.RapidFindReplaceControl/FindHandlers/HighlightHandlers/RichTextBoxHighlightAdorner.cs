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
using System.Windows.Threading;

namespace Keyoti.RapidFindReplace.WPF.FindHandlers.HighlightHandlers
{
    class RichTextBoxHighlightAdorner : TextBoxBaseHighlightAdorner
    {
        #region Caching
        Rect[] cachedRects;
        TextPointer cachedStartP;
        bool invalidatedCache;
        #endregion

        RichTextBox rtb;
        public RichTextBoxHighlightAdorner(RichTextBox rtb, Run run, int start, int length, double horizontalScrollBarHeight, System.Windows.Media.Brush bodyHighlightAdornerBrush, System.Windows.Media.Pen bodyHighlightAdornerPen, Brush bodyIterativeHighlightAdornerBrush, Pen bodyIterativeHighlightAdornerPen)
            : base(rtb, run, start, length, horizontalScrollBarHeight, bodyHighlightAdornerBrush, bodyHighlightAdornerPen, bodyIterativeHighlightAdornerBrush, bodyIterativeHighlightAdornerPen)
        {
            this.rtb = rtb;
        }



        internal override void BumpUnderlineTo(Run run, int start, int end)
        {
            invalidatedCache = true;
            base.BumpUnderlineTo(run, start, end);
        }

 

        protected override void scroller_ScrollChanged(object sender, RoutedEventArgs e)
        {
            invalidatedCache = true;
            base.scroller_ScrollChanged(sender, e);
        }

        internal override Rect[] GetCharacterRects(TextPointer startPointer, TextPointer endPointer)
        {
            if (!invalidatedCache && cachedStartP != null && startPointer.CompareTo(cachedStartP) == 0)
            {
                return cachedRects;
            }
            else
            {
                cachedRects = base.GetCharacterRects(startPointer, endPointer);
                cachedStartP = startPointer;
                invalidatedCache = false;
            }

            return cachedRects;
        }
       
    }
}
