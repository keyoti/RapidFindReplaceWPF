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

    /// <summary>
    /// 
    /// </summary>
#if DEBUG
    public
#endif
 class IgnoreCharacterClassHandler : AbstractIgnoreHandler
    {
        //its easy to add more classes, just add to enum and add more tests to the if below
        /// <summary>
        /// 
        /// </summary>
        public enum CharacterClass
        {
            /// <summary>
            /// 
            /// </summary>
            Whitespace = 1,
            /// <summary>
            /// 
            /// </summary>
            Punctuation = 2
        }

        CharacterClass characterClass;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="characterClass"></param>
        public IgnoreCharacterClassHandler(CharacterClass characterClass)
        {
            this.characterClass = characterClass;
        }

        /// <summary>
        /// Translates text to the ignored version (stripped ignored chars)
        /// </summary>
        /// <param name="text">The text to process</param>
        /// <param name="bookmark">A character index that needs to be translated to a mapped value</param>
        /// <param name="translatedBookmark">The translated value of <c>bookmark</c></param>
        /// <returns>Translated text.</returns>
        public override string TranslateToIgnored(string text, int bookmark, out int translatedBookmark)
        {
            ignoredIndexes.Clear();
            char c;
            translatedBookmark = -1;
            StringBuilder translated = new StringBuilder(text.Length / 2);
            for (int i = 0; i < text.Length; i++)
            {
                if (i == bookmark) translatedBookmark = translated.Length;
                c = text[i];
                if (characterClass == CharacterClass.Whitespace && Char.IsWhiteSpace(c)
                        ||
                        characterClass == CharacterClass.Punctuation && Char.IsPunctuation(c)
                    )
                {
                    ignoredIndexes.Add(translated.Length);
                }
                else
                    translated.Append(c);

            }
            if (bookmark == text.Length)//we didnt get to this point in the loop
                translatedBookmark = translated.Length;

            return translated.ToString();
        }
    }



}
