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
    

    abstract class AbstractTextMatcher : ITextMatcher
    {



        protected Query searchQuery;
        protected string text;
        protected int startPosition;
        protected StringComparison caseComparison;
        protected List<IMatchFilter> matchFilters;
        protected List<IIgnoreCharacterHandler> bodyIgnoreHandlers;
        protected List<IIgnoreCharacterHandler> queryIgnoreHandlers;
        public AbstractTextMatcher(Query searchQuery, string text, int startPosition, StringComparison caseComparison, List<IMatchFilter> matchFilters, List<IIgnoreCharacterHandler> bodyIgnoreHandlers, List<IIgnoreCharacterHandler> queryIgnoreHandlers)
        {
            this.queryIgnoreHandlers = queryIgnoreHandlers;
            this.bodyIgnoreHandlers = bodyIgnoreHandlers;
            this.startPosition = startPosition;
            this.searchQuery = searchQuery;
            searchQuery.PropertyChanged += searchQuery_PropertyChanged;
            this.SetText (text);

            SetQueryText();


            this.caseComparison = caseComparison;
            this.matchFilters = matchFilters;
            
        }

        public void UnhookHandlers()
        {
            searchQuery.PropertyChanged -= searchQuery_PropertyChanged;
        }

        void searchQuery_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "QueryText")
            {
                SetQueryText();
            }
        }

        protected virtual void SetQueryText()
        {            
            searchQuery.AddIgnoreHandlers(queryIgnoreHandlers);
        }

        /// <summary>
        /// Gets an enumerator
        /// </summary>
        public IEnumerator<Hit> GetEnumerator()
        {
            int curPos = startPosition;
            Hit find;
            while (curPos < text.Length)
            {
                find = GetMatch(curPos);
                if (find.Start > -1)
                {
                    bool isMatch = true;
                    foreach (IMatchFilter filter in matchFilters)
                        isMatch &= filter.IsMatch(searchQuery.QueryText, text, find);


                    curPos = find.Start + 1;
                    if (!isMatch) continue;//try again
                    else
                    {
                        Hit translatedHit = find;
                        for (int h = bodyIgnoreHandlers.Count - 1; h >= 0; h--)//we want to translate back in the opposite order than the text was translated in.
                        {
                            translatedHit = bodyIgnoreHandlers[h].TranslateHit(translatedHit);
                        }
                        yield return translatedHit;
                    }
                }
                else break;
            }
        }

        protected abstract Hit GetMatch(int startPos);

        /// <summary>
        /// The position in the text from where the next match will be searched for.
        /// </summary>
        public int CurrentPosition { get { return startPosition; } set { startPosition = value; } }

        /// <summary>
        /// The text the matcher is working on.
        /// </summary>
        public void SetText(string text)
        {
            this.text = text;
            for (int h = 0; h < bodyIgnoreHandlers.Count; h++)
            {
                //the text is translated from 0->Count, and when the hits are enum'd they are translated back to the original text from Count->0
                this.text = bodyIgnoreHandlers[h].TranslateToIgnored(this.text, this.startPosition, out this.startPosition);
            }
        }

        /// <summary>
        /// Processes any relevant find syntax (eg. $1 style groupings in regex).
        /// </summary>
        /// <returns>Modified <c>replace</c> string (eg. with group substitutions in regular expressions)</returns>
        public virtual string ProcessReplacementSyntax(string replace, Highlight match)
        {
            return replace;
        }
    }
}
