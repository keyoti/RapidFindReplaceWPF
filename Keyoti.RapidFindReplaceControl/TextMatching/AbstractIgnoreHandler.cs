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
 abstract class AbstractIgnoreHandler : IIgnoreCharacterHandler
    {


        /// <summary>
        /// List of character indexes that have been ignored (removed) from the translated text.
        /// </summary>
        /// <remarks>
        /// Eg. 
        /// originalText = "this is it"
        /// then
        /// translatedText="thisisit"
        /// and ignoredIndexes = {4, 7}
        /// 
        /// To translate back to original index;
        /// translatedIndex = 5
        /// original = 5 + skipped tally (1) = 6
        /// </remarks>
        protected List<int> ignoredIndexes = new List<int>(10);

        /// <summary>
        /// Translates from a character index in the translated string, to a character index in the original string.
        /// </summary>
        /// <param name="index">Char index in translated string</param>
        /// <param name="shiftForward">Whether to return a position after any ignored chars that occurred </param>
        protected int TranslateIndex(int index, bool shiftForward)
        {
            //count how many ignored indexes there are between 0 and index
            int tally = 0;
            for (int i = 0; i < ignoredIndexes.Count; i++)
            {
                if (ignoredIndexes[i] < index || (shiftForward && ignoredIndexes[i] == index))
                    tally++;
                else
                    break;
            }
            return index + tally;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="bookmark"></param>
        /// <param name="translatedBookmark"></param>
        /// <returns></returns>
        public abstract string TranslateToIgnored(string text, int bookmark, out int translatedBookmark);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hit"></param>
        /// <returns></returns>
        public Hit TranslateHit(Hit hit)
        {
            int translatedStart = TranslateIndex(hit.Start, true);
            int translatedEnd = TranslateIndex(hit.Start + hit.Length, false);
            if (translatedEnd < translatedStart) translatedEnd = translatedStart;//this can happen when the search string is empty and theres a whitespace that was ignored
            return new Hit(translatedStart, translatedEnd - translatedStart);
        }
    }

}
