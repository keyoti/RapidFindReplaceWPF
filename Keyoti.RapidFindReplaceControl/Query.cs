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
using Keyoti.RapidFindReplace.WPF.TextMatching;
using Keyoti.RapidFindReplace.WPF.FindHandlers.HighlightHandlers;

namespace Keyoti.RapidFindReplace.WPF
{
    
    /// <summary>
    /// Query validation.
    /// </summary>
    public class QueryValidationRule : System.Windows.Controls.ValidationRule
    {
       
        Query _query;
        /// <summary>
        /// The query to validate
        /// </summary>
        public Query Query
        {
            get { return _query; }
            set { _query = value; }
        }

        /// <summary>
        /// Validates the query.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public override System.Windows.Controls.ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            return new System.Windows.Controls.ValidationResult((bool)value, Query.ReasonInvalid);
          
        }
    }

    /// <summary>
    /// A find query.
    /// </summary>
    public class Query : System.ComponentModel.INotifyPropertyChanged
    {
        string originalQueryBeforeIgnoreHandlerChanges;
        TextMatchers textMatchers;
        OptionsViewModel findOptions;
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Raises the property changed event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        internal void AddIgnoreHandlers(List<IIgnoreCharacterHandler> handlers)
        {
            originalQueryBeforeIgnoreHandlerChanges = queryString;
            int dummy;
            for (int h = 0; h < handlers.Count; h++)
                queryString = handlers[h].TranslateToIgnored(QueryText, 0, out dummy);
        }

        TextMatchers CreateMatchers()
        {
            int currentIndex = 0;
            string text = "";
            StringComparison comparison;
            if (findOptions.MatchCase) comparison = StringComparison.CurrentCulture;
            else comparison = StringComparison.CurrentCultureIgnoreCase;



            //collection of match enumerators from the matchers.
            //List<IEnumerator<Hit>> matcherEnumerators = new List<IEnumerator<Hit>>(15);
            TextMatchers matchers = new TextMatchers(15);
            List<IMatchFilter> matchFilters = new List<IMatchFilter>(5);
            List<IIgnoreCharacterHandler> bodyIgnoreHandlers = new List<IIgnoreCharacterHandler>(2);
            List<IIgnoreCharacterHandler> queryIgnoreHandlers = new List<IIgnoreCharacterHandler>(2);

            //because wildcards and regexs will blow up our prefix/suffix/wholeword matchers, we have to treat them as filters instead.  
            //---Filters
            if (findOptions.FindWholeWordsOnly)
                matchFilters.Add(new WordPartFilter(WordPartFilter.Part.WholeWord));

            if (findOptions.MatchPrefix)
                matchFilters.Add(new WordPartFilter(WordPartFilter.Part.Prefix));

            if (findOptions.MatchSuffix)
                matchFilters.Add(new WordPartFilter(WordPartFilter.Part.Suffix));

            //---Ignore handlers
            if (findOptions.IgnorePunctuationCharacters)
            {
                bodyIgnoreHandlers.Add(new IgnoreCharacterClassHandler(IgnoreCharacterClassHandler.CharacterClass.Punctuation));
                //translating the query is not compatible with wildcards or regex because both use punct. as special symbols
                if (!findOptions.UseRegularExpressions && !findOptions.UseWildcards)
                    queryIgnoreHandlers.Add(new IgnoreCharacterClassHandler(IgnoreCharacterClassHandler.CharacterClass.Punctuation));
            }

            if (findOptions.IgnoreWhitespaceCharacters)
            {
                bodyIgnoreHandlers.Add(new IgnoreCharacterClassHandler(IgnoreCharacterClassHandler.CharacterClass.Whitespace));
                queryIgnoreHandlers.Add(new IgnoreCharacterClassHandler(IgnoreCharacterClassHandler.CharacterClass.Whitespace));
            }



            //---Match Enumerators
            if (findOptions.UseRegularExpressions)
                matchers.Add(new RegularExpressionTextMatcher(this, text, currentIndex, comparison, matchFilters, bodyIgnoreHandlers, queryIgnoreHandlers));

            if (findOptions.UseWildcards)
                matchers.Add(new WildcardTextMatcher(this, text, currentIndex, comparison, matchFilters, bodyIgnoreHandlers, queryIgnoreHandlers));

            if (matchers.Count == 0) //add default
                matchers.Add(new StandardTextMatcher(this, text, currentIndex, comparison, matchFilters, bodyIgnoreHandlers, queryIgnoreHandlers));
            return matchers;
        }

        /// <summary>
        /// New.
        /// </summary>
        /// <param name="queryText">The text of the query.</param>
        public Query(string queryText)
        {
            this.queryString = queryText;
        }

        bool valid = true;
        string invalidReason = "";
        string queryString;
        /// <summary>
        /// The query text
        /// </summary>
        public string QueryText
        {
            get { return queryString; }
#if DEBUG
            //let be public for unit tests
#else
            internal 
#endif            
            set {
                if (queryString != value)
                {
                    queryString = value;
                    Valid = true;//assume its valid until its proven not (eg by an exception in the matcher)
                    RaisePropertyChanged("QueryText");
                }
            }
        }
        /// <summary>
        /// Gets the chosen text matchers for the query options.
        /// </summary>
        /// <param name="textToSearch">Text being searched.</param>
        /// <param name="findOptions">The chosen options.</param>
        /// <returns>Collection of matchers.</returns>
        public TextMatchers GetTextMatchers(string textToSearch, OptionsViewModel findOptions)
        {
            this.findOptions = findOptions;
            if (textMatchers == null)
                textMatchers = CreateMatchers();

            for (int i = 0; i < textMatchers.Count; i++)
            {
                textMatchers[i].SetText(textToSearch);
            }

            return textMatchers;
        }


        /// <summary>
        /// Any reason why the query is invalid, if it is otherwise empty.
        /// </summary>
        public string ReasonInvalid
        {
            get { return invalidReason; }
            set {
                if (invalidReason != value)
                {
                    invalidReason = value;
                    RaisePropertyChanged("ReasonInvalid");
                }
            }
        }

        /// <summary>
        /// Whether the query is valid, relative to the find options specified.
        /// </summary>
        /// <remarks>Eg. the query '[*' would be invalid if regular expressions are enabled.</remarks>
        public bool Valid
        {
            get
            {
                return valid;
            }
            set
            {
                if (valid != value)
                {
                    valid = value;
                    RaisePropertyChanged("Valid");
                }
            }
        }


        internal void ResetMatchers()
        {
            //undo any changes to the query that ignore matchers made.
            queryString = originalQueryBeforeIgnoreHandlerChanges;
            if(textMatchers!=null)textMatchers.UnhookHandlers();
            textMatchers = null;
        }

        /// <summary>
        /// Uses any enabled text matchers to process any relevant find syntax (eg. \1 style groupings in regex).
        /// </summary>
        public virtual string ProcessReplacementSyntax(string replacement, Highlight match)
        {
            string newReplace = replacement;
            foreach (ITextMatcher matcher in textMatchers)
            {
                newReplace = matcher.ProcessReplacementSyntax(newReplace, match);
            }
            return newReplace;
        }
    }
}
