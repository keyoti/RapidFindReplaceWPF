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

namespace Keyoti.RapidFindReplace.WPF.TextMatching
{

    class StandardTextMatcher : AbstractTextMatcher
    {
        public StandardTextMatcher(Query searchQuery, string text, int startPosition, StringComparison caseComparison, List<IMatchFilter> matchFilters, List<IIgnoreCharacterHandler> bodyIgnoreHandlers, List<IIgnoreCharacterHandler> queryIgnoreHandlers)
            : base(searchQuery, text, startPosition, caseComparison, matchFilters, bodyIgnoreHandlers, queryIgnoreHandlers)
        { }

        protected override Hit GetMatch(int startPosition)
        {
            if (startPosition >= text.Length) return new Hit(-1, 0);
            int matchStart = text.IndexOf(searchQuery.QueryText, startPosition, caseComparison);
            if (matchStart > -1)
                return new Hit(matchStart, searchQuery.QueryText.Length);
            else
                return new Hit(-1, 0);

        }
    }

    /*works and tested, but unused - replaced by filter
    class WordPartTextMatcher : AbstractTextMatcher
    {
        public enum Part{
            WholeWord,
            Prefix,
            Suffix
        }

        Part _part;

        public WordPartTextMatcher(Part part, string searchQuery, string text, int startPosition, StringComparison caseComparison) : base(searchQuery, text, startPosition, caseComparison)
        {
            _part = part;
        }

        protected override int[] GetMatch(int startPosition)
        {
            int possibleMatch;
            bool keepLooking = false;
            do
            {
                keepLooking = false;
                possibleMatch = text.IndexOf(searchQuery, startPosition, caseComparison);

                if ( possibleMatch > -1)
                {
                    //check surrounded by something other than letters or digits
                    bool startBoundaryOK = _part==Part.Suffix || ( possibleMatch == 0 || !Char.IsLetterOrDigit(text[possibleMatch - 1]));
                    bool endBoundaryOK = _part == Part.Prefix || ((possibleMatch + searchQuery.Length >= text.Length) || !Char.IsLetterOrDigit(text[possibleMatch + searchQuery.Length]));
                    if (!(startBoundaryOK && endBoundaryOK))
                        keepLooking = true;
                    
                    
                }

                

                if (possibleMatch == -1) keepLooking = false;
                else startPosition = possibleMatch + searchQuery.Length;

            } while (keepLooking && startPosition < text.Length);

            return new int[2] {possibleMatch, searchQuery.Length};
        }
    }
    */
}
