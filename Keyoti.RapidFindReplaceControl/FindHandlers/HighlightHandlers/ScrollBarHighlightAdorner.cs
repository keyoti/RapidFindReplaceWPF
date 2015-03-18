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
    /// Adorner for scroll bar highlights
    /// </summary>
    class ScrollBarHighlightAdorner : Adorner
    {
        
        HighlightAdorner bodyHighlight;
        ScrollBarHighlightHandler handler;
        
        /// <summary>
        /// New instance
        /// </summary>
        /// <param name="handler">Handler for this highlight adorner.</param>
        /// <param name="bodyHighlight">The corresponding highlight adorner in the body of the text.</param>
        public ScrollBarHighlightAdorner(ScrollBarHighlightHandler handler, HighlightAdorner bodyHighlight)
            : base(handler.ScrollViewer)
        {
            this.bodyHighlight = bodyHighlight;
            this.handler = handler;
            IsHitTestVisible = false;

        }

        /// <summary>
        /// Renders the highlight in the scroll bar.
        /// </summary>
        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            if (!handler.IsHighlightInViewPort(bodyHighlight))//works for fldv and ?tb?
                drawingContext.DrawRectangle(handler.RenderBrush, null, handler.GetAdornerPosition(bodyHighlight));
        }


    }


}
