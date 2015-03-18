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
using Keyoti.RapidFindReplace.WPF.FindHandlers.HighlightHandlers;

namespace Keyoti.RapidFindReplace.WPF.TextMatching
{
    /// <summary>
    /// Interface for text matchers.
    /// </summary>
    public interface ITextMatcher
    {
        //int GetMatch(string searchQuery, string text, int startPosition, StringComparison caseComparison);
        /// <summary>
        /// Gets an enumerator
        /// </summary>
        IEnumerator<Hit> GetEnumerator();
        /// <summary>
        /// The position in the text from where the next match will be searched for.
        /// </summary>
        int CurrentPosition { get; set; }

        /// <summary>
        /// Sets the text to search.
        /// </summary>
        void SetText(string text);

        /// <summary>
        /// Processes any relevant find syntax (eg. $1 style groupings in regex).
        /// </summary>
        /// <returns>Modified <c>replace</c> string (eg. with group substitutions in regular expressions)</returns>
        string ProcessReplacementSyntax(string replace, Highlight match);

        /// <summary>
        /// Unhooks matcher from associated Query.
        /// </summary>
        void UnhookHandlers();
    }

}
