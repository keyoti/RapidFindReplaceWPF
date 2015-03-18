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
using Keyoti.RapidFindReplace.WPF.FindHandlers.HighlightHandlers;

namespace Keyoti.RapidFindReplace.WPF.TextMatching
{
    class RegularExpressionTextMatcher : AbstractTextMatcher
    {
        Regex regex;
        protected string queryPattern;//this may or may not be equal to searchQuery.QueryText (eg for wildcard it probably isnt)
        RegexOptions options;

        public RegularExpressionTextMatcher(Query searchQuery, string text, int startPosition, StringComparison caseComparison, List<IMatchFilter> matchFilters, List<IIgnoreCharacterHandler> bodyIgnoreHandlers, List<IIgnoreCharacterHandler> queryIgnoreHandlers)
            : this(searchQuery, text, startPosition, caseComparison, matchFilters, bodyIgnoreHandlers, queryIgnoreHandlers, true)
        {
            

        }

        internal RegularExpressionTextMatcher(Query searchQuery, string text, int startPosition, StringComparison caseComparison, List<IMatchFilter> matchFilters, List<IIgnoreCharacterHandler> bodyIgnoreHandlers, List<IIgnoreCharacterHandler> queryIgnoreHandlers, bool initRegex)
            : base(searchQuery, text, startPosition, caseComparison, matchFilters, bodyIgnoreHandlers, queryIgnoreHandlers)
        {
            queryPattern = searchQuery.QueryText;
            options = RegexOptions.None;
            if (caseComparison == StringComparison.CurrentCultureIgnoreCase)
                options |= RegexOptions.IgnoreCase;
            if (initRegex)
            {
                searchQuery.PropertyChanged += searchQuery_PropertyChanged;
                InitRegex();
            }

        }
        
        protected virtual void InitRegex()
        {
            
            RegexOptions options = RegexOptions.None;
            if (caseComparison == StringComparison.CurrentCultureIgnoreCase)
                options |= RegexOptions.IgnoreCase;
            try
            {
                regex = GetRegex(queryPattern); //new Regex(this.searchQuery.QueryText, options);//this.searchQuery has already been converted using ignore handlers, so may be different to 'searchQuery' in args
            }
            catch (ArgumentException ex)
            {
                this.searchQuery.ReasonInvalid = "Regular expression error, " + ex.Message;//set this first because next line will trigger validator
                this.searchQuery.Valid = false;
            }
        }

        


                void searchQuery_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
                {
                    if (e.PropertyName == "QueryText")
                    {
                        queryPattern = searchQuery.QueryText;
                        InitRegex();
                    }
                }
                
        static Regex lastRegex;
        static string lastQuery;
        static RegexOptions lastRegexOptions;
        protected override Hit GetMatch(int startPosition)
        {
            
            if (regex == null) return new Hit(-1, 0);
            Match m = regex.Match(text, startPosition);
            //Match m = Regex.Match(text, searchQuery.QueryText, options);
            if (!m.Success) return new Hit(-1, 0);
            else if (m.Length == 0) return new Hit(-1, 0);//with regex we can match 0 length, which is no use anyway
            else return new Hit(m.Index, m.Length);
        }

        private Regex GetRegex(string q)
        {


            if (lastQuery != q || lastRegexOptions!=options)
            {
                lastRegex = new Regex(q, options);
                lastQuery = q;
                lastRegexOptions = options;
            }
            return lastRegex;
        }

        /// <summary>
        /// Processes any relevant find syntax (eg. $1 style groupings in regex).
        /// </summary>
        /// <returns>Modified <c>replace</c> string (eg. with group substitutions in regular expressions)</returns>
        public override string ProcessReplacementSyntax(string replace, Highlight match)
        {
            return Regex.Replace(match.Text, queryPattern, replace);
        }
    }
}
