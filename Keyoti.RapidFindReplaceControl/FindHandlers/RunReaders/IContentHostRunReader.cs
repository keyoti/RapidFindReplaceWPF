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

namespace Keyoti.RapidFindReplace.WPF.FindHandlers.RunReaders
{

    /// <summary>
    /// IRunReader for classes implementing IContentHost.
    /// </summary>
    public class IContentHostRunReader : IRunReader
    {
        /// <summary>
        /// Whether this reader can read Runs from <c>runContainer</c>.
        /// </summary>
        [System.Reflection.Obfuscation(Exclude = true)]
        public static bool DoesHandle(object runContainer)
        {
            return runContainer is IContentHost && runContainer is UIElement;
        }

        IContentHost contentHost;
        /// <summary>
        /// New instance.
        /// </summary>
        public IContentHostRunReader(IContentHost host)
        {
            this.contentHost = host;
        }

        /// <summary>
        /// Gets an enumerator that will enumerate the Runs in the element being read.
        /// </summary>
        public IEnumerator<Run> GetEnumerator()
        {
            TextBlock textBlock = contentHost as TextBlock;

            if (textBlock != null)
            {
                //This enables 'complex content' , so that HostedElements will return a Run.
                TextPointer contentStart = textBlock.ContentStart;
            }



            var hostedElements = contentHost.HostedElements;
            while (hostedElements.MoveNext())
            {
                if(hostedElements.Current is Run)
                    yield return hostedElements.Current as Run;
            }
        }
    }
}
