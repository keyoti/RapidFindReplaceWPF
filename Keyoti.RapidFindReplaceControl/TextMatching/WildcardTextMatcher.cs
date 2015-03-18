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

namespace Keyoti.RapidFindReplace.WPF.TextMatching
{

    //http://word.mvps.org/faqs/general/usingwildcards.htm
    //http://office.microsoft.com/en-ca/word-help/add-power-to-word-searches-with-regular-expressions-HA001087305.aspx
    /// <summary>
    /// Wildcard which can be used as part of words in queries.  Wildcards can consume whitespace.  There can be multiple wildcards
    /// </summary>
    class WildcardTextMatcher : RegularExpressionTextMatcher
    {
        public WildcardTextMatcher(Query searchQuery, string text, int startPosition, StringComparison caseComparison, List<IMatchFilter> matchFilters, List<IIgnoreCharacterHandler> bodyIgnoreHandlers, List<IIgnoreCharacterHandler> queryIgnoreHandlers)
            : base(searchQuery, text, startPosition, caseComparison, matchFilters, bodyIgnoreHandlers, queryIgnoreHandlers, true)
        {
            //this.searchQuery.QueryText = ProcessQueryForRegex(searchQuery);
            queryPattern = ProcessQueryForRegex(searchQuery);
            
        }

        private static string ProcessQueryForRegex(Query searchQuery)
        {
            //In Word, trailing * is ignored
            string processedQuery = searchQuery.QueryText;
            while (processedQuery.EndsWith("*"))
            {
                processedQuery = processedQuery.Substring(0, processedQuery.Length - 1);
            }

            processedQuery = Regex.Escape(processedQuery)
                                .Replace("\\*", ".*?")
                                .Replace("\\?", ".")
                                .Replace("<", "\\b")
                                .Replace(">", "\\b")
                                .Replace("\\[", "[")
                                .Replace("\\]", "]")
                                .Replace("\\{", "{")
                                .Replace("\\}", "}")
                                .Replace("\\(", "(")
                                .Replace("\\)", ")")
                                .Replace("[!", "[^");
            return processedQuery;
        }

        protected override void InitRegex()
        {
            //this.searchQuery.SetQueryTextNoNotifyChange(ProcessQueryForRegex(searchQuery));
            queryPattern = ProcessQueryForRegex(searchQuery);
            base.InitRegex();
        }

 

    }
}

  