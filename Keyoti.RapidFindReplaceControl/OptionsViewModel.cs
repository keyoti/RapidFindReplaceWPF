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

namespace Keyoti.RapidFindReplace.WPF
{
    /// <summary>
    /// Meta data used for option properties.
    /// </summary>
    public class OptionPropertyMetaData : PropertyMetadata
    {
        string description;
        /// <summary>
        /// Description of the property.
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
        bool visibleToUser;
        /// <summary>
        /// Whether the option is visible to the user in the options dropdown.
        /// </summary>
        public bool VisibleToUser
        {
            get { return visibleToUser; }
            set { visibleToUser = value; }
        }
        /// <summary>
        /// New
        /// </summary>
        public OptionPropertyMetaData(object defaultValue, string description, bool visibleToUser) : base(defaultValue) { this.description = description; this.visibleToUser = visibleToUser; }
        /// <summary>
        /// New
        /// </summary>
        public OptionPropertyMetaData(object defaultValue, string description, bool visibleToUser, PropertyChangedCallback callback) : base(defaultValue, callback) { this.description = description; this.visibleToUser = visibleToUser; }
    }

    /// <summary>
    /// The options view model
    /// </summary>
    public class OptionsViewModel : DependencyObject
    {
        /// <summary>
        /// Fired when an option property has changed.
        /// </summary>
        public event DependencyPropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// New
        /// </summary>
        public OptionsViewModel()
        {
                MatchCase = false;
                FindWholeWordsOnly = false;
                UseWildcards = false;
                UseRegularExpressions = false;
                FindAsYouType = true;
                MatchPrefix = false;
                MatchSuffix = false;
                IgnorePunctuationCharacters = false;
                IgnoreWhitespaceCharacters = false;
                if (IgnoredElementTypes == null) IgnoredElementTypes = new List<Type>(1);
                if(IgnoredElementTypes.Count==0) IgnoredElementTypes.Add(typeof (System.Windows.Controls.Primitives.ButtonBase));
                if (ExclusiveFindElementTypes == null) ExclusiveFindElementTypes = new List<Type>(1);
        }

        private static void ValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            DependencyPropertyChangedEventHandler ev = (sender as OptionsViewModel).PropertyChanged;
            if (ev != null) ev(sender as OptionsViewModel, e);
        }

        /// <summary>
        /// Collection of types that should not be searched.
        /// </summary>
        public readonly static DependencyProperty IgnoredElementTypesProperty = DependencyProperty.Register("IgnoredElementTypes", typeof(List<Type>), typeof(OptionsViewModel), new OptionPropertyMetaData(new List<Type>(), "UIElement types that should be ignored.", false, new PropertyChangedCallback(ValueChanged)));

        /// <summary>
        /// Collection of types that should not be searched.
        /// </summary>
        public List<Type> IgnoredElementTypes
        {
            get { return (List<Type>)GetValue(IgnoredElementTypesProperty); }
            internal set { SetValue(IgnoredElementTypesProperty, value); }
        }

        /// <summary>
        /// Collection of types that should only be searched.
        /// </summary>
        public readonly static DependencyProperty ExclusiveFindElementTypesProperty = DependencyProperty.Register("ExclusiveFindElementTypes", typeof(List<Type>), typeof(OptionsViewModel), new OptionPropertyMetaData(new List<Type>(), "UIElement types that should only be searched.", false, new PropertyChangedCallback(ValueChanged)));

        /// <summary>
        /// Collection of types that should only be searched.
        /// </summary>
        public List<Type> ExclusiveFindElementTypes
        {
            get { return (List<Type>)GetValue(ExclusiveFindElementTypesProperty); }
            internal set { SetValue(ExclusiveFindElementTypesProperty, value); }
        }
        
        /// <summary>
        /// Whether to only find hits where the case matches.
        /// </summary>
        public readonly static DependencyProperty MatchCaseProperty = DependencyProperty.Register("MatchCase", typeof(bool), typeof(OptionsViewModel), new OptionPropertyMetaData(false, "Whether to only find hits where the case matches.", true, new PropertyChangedCallback(ValueChanged)));
        /// <summary>
        /// Whether to only find hits where the case matches.
        /// </summary>
        public bool MatchCase
        {
            get { return (bool)GetValue(MatchCaseProperty); }            set { SetValue(MatchCaseProperty, value); }
        }

        /// <summary>
        /// Whether to only find hits which match an entire word.
        /// </summary>
        public readonly static DependencyProperty FindWholeWordsOnlyProperty = DependencyProperty.Register("FindWholeWordsOnly", typeof(bool), typeof(OptionsViewModel), new OptionPropertyMetaData(false, "Whether to only find hits which match an entire word.", true, new PropertyChangedCallback(ValueChanged)));
        /// <summary>
        /// Whether to only find hits which match an entire word.
        /// </summary>
        public bool FindWholeWordsOnly
        {
            get { return (bool)GetValue(FindWholeWordsOnlyProperty); }            set { SetValue(FindWholeWordsOnlyProperty, value); }
        }

        /// <summary>
        /// Whether to treat * and ? as wildcards (also supports classes [], groups () and repetitions {}).
        /// </summary>
        public readonly static DependencyProperty UseWildcardsProperty = DependencyProperty.Register("UseWildcards", typeof(bool), typeof(OptionsViewModel), new OptionPropertyMetaData(false, "Whether to treat * and ? as wildcards (also supports classes [], groups () and repetitions {}).", true, new PropertyChangedCallback(ValueChanged)));
        /// <summary>
        /// Whether to treat * and ? as wildcards (also supports classes [], groups () and repetitions {}).
        /// </summary>
        public bool UseWildcards
        {
            get { return (bool)GetValue(UseWildcardsProperty); }            set { SetValue(UseWildcardsProperty, value); }
        }

        /// <summary>
        /// Whether to use regular expressions as queries.
        /// </summary>
        public readonly static DependencyProperty UseRegularExpressionsProperty = DependencyProperty.Register("UseRegularExpressions", typeof(bool), typeof(OptionsViewModel), new OptionPropertyMetaData(false, "Whether to use regular expressions as queries.", true, new PropertyChangedCallback(ValueChanged)));
        /// <summary>
        /// Whether to use regular expressions as queries.
        /// </summary>
        public bool UseRegularExpressions
        {
            get { return (bool)GetValue(UseRegularExpressionsProperty); }
            set { SetValue(UseRegularExpressionsProperty, value); }
        }

        /// <summary>
        /// Whether to highlight hits as you type.
        /// </summary>
        public readonly static DependencyProperty FindAsYouTypeProperty = DependencyProperty.Register("FindAsYouType", typeof(bool), typeof(OptionsViewModel), new OptionPropertyMetaData(false, "Whether to highlight hits as you type.", true, new PropertyChangedCallback(ValueChanged)));
        /// <summary>
        /// Whether to highlight hits as you type.
        /// </summary>
        public bool FindAsYouType
        {
            get { return (bool)GetValue(FindAsYouTypeProperty); }            set { SetValue(FindAsYouTypeProperty, value); }
        }

        /// <summary>
        /// Whether to only find hits that match the start of a word.
        /// </summary>
        public readonly static DependencyProperty MatchPrefixProperty = DependencyProperty.Register("MatchPrefix", typeof(bool), typeof(OptionsViewModel), new OptionPropertyMetaData(false, "Whether to only find hits that match the start of a word.", true, new PropertyChangedCallback(ValueChanged)));
        /// <summary>
        /// Whether to only find hits that match the start of a word.
        /// </summary>
        public bool MatchPrefix
        {
            get { return (bool)GetValue(MatchPrefixProperty); }            set { SetValue(MatchPrefixProperty, value); }
        }

        /// <summary>
        /// Whether to only find hits that match the end of a word.
        /// </summary>
        public readonly static DependencyProperty MatchSuffixProperty = DependencyProperty.Register("MatchSuffix", typeof(bool), typeof(OptionsViewModel), new OptionPropertyMetaData(false, "Whether to only find hits that match the end of a word.", true, new PropertyChangedCallback(ValueChanged)));
        /// <summary>
        /// Whether to only find hits that match the end of a word.
        /// </summary>
        public bool MatchSuffix
        {
            get { return (bool)GetValue(MatchSuffixProperty); }            set { SetValue(MatchSuffixProperty, value); }
        }

        /// <summary>
        /// Whether to ignore punctuation characters in the text and query.
        /// </summary>
        public readonly static DependencyProperty IgnorePunctuationCharactersProperty = DependencyProperty.Register("IgnorePunctuationCharacters", typeof(bool), typeof(OptionsViewModel), new OptionPropertyMetaData(false, "Whether to ignore punctuation characters in the text and query.", true, new PropertyChangedCallback(ValueChanged)));
        /// <summary>
        /// Whether to ignore punctuation characters in the text and query.
        /// </summary>
        public bool IgnorePunctuationCharacters
        {
            get { return (bool)GetValue(IgnorePunctuationCharactersProperty); }            set { SetValue(IgnorePunctuationCharactersProperty, value); }
        }

        /// <summary>
        /// Whether to ignore whitespace characters in the text and query.
        /// </summary>
        public readonly static DependencyProperty IgnoreWhitespaceCharactersProperty = DependencyProperty.Register("IgnoreWhitespaceCharacters", typeof(bool), typeof(OptionsViewModel), new OptionPropertyMetaData(false, "Whether to ignore whitespace characters in the text and query.", true, new PropertyChangedCallback(ValueChanged)));
        /// <summary>
        /// Whether to ignore whitespace characters in the text and query.
        /// </summary>
        public bool IgnoreWhitespaceCharacters
        {
            get { return (bool)GetValue(IgnoreWhitespaceCharactersProperty); }            set { SetValue(IgnoreWhitespaceCharactersProperty, value); }
        }

    }
}
