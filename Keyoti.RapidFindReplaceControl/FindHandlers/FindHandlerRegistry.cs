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
using System.Windows.Media;
using Keyoti.RapidFindReplace.WPF.FindHandlers.HighlightHandlers;
using Keyoti.RapidFindReplace.WPF.FindHandlers.RunReaders;

namespace Keyoti.RapidFindReplace.WPF.FindHandlers
{

    /// <summary>
    /// Registry against which HighlightHandler instances and RunReader instances are registered.
    /// </summary>
    public class FindHandlerRegistry
    {
        static FindHandlerRegistry()
        {
            FindHandlerRegistry.RegisterHighlightHandler(typeof(HighlightHandler));//We register this first to ensure it's the last handler in the list, our default.
            FindHandlerRegistry.RegisterHighlightHandler(typeof(FlowDocumentPageViewerHighlightHandler));
            FindHandlerRegistry.RegisterHighlightHandler(typeof(FlowDocumentScrollViewerHighlightHandler));
            FindHandlerRegistry.RegisterHighlightHandler(typeof(RichTextBoxHighlightHandler));
            FindHandlerRegistry.RegisterHighlightHandler(typeof(TextBoxHighlightHandler));
            FindHandlerRegistry.RegisterHighlightHandler(typeof(RunCollectionHighlightHandler));
            


            FindHandlerRegistry.RegisterRunReader(typeof(FlowDocumentScrollViewerRunReader));
            FindHandlerRegistry.RegisterRunReader(typeof(DocumentViewerBaseRunReader));
            FindHandlerRegistry.RegisterRunReader(typeof(FlowDocumentRunReader));
            FindHandlerRegistry.RegisterRunReader(typeof(RichTextBoxRunReader));
            FindHandlerRegistry.RegisterRunReader(typeof(TextBoxRunReader));
            FindHandlerRegistry.RegisterRunReader(typeof(IContentHostRunReader));
            FindHandlerRegistry.RegisterRunReader(typeof(RunCollectionRunReader));
        }

        static List<Type> highlightHandlerRegistry = new List<Type>();

        /// <summary>
        /// Registers a HighlightHandler so that it can be used to highlight a UIElement.
        /// </summary>
        /// <param name="highlightHandlerType">The Type to register.</param>
        public static void RegisterHighlightHandler(Type highlightHandlerType)
        {
            highlightHandlerRegistry.Insert(0, highlightHandlerType);
        }

        static List<Type> iRunReaderTypeRegistry = new List<Type>();

        /// <summary>
        /// Registers RunReader (IRunReader) type so that it can be used to read Run objects from a UIElement.
        /// </summary>
        /// <param name="iRunReaderType">The Type to register.</param>
        public static void RegisterRunReader(Type iRunReaderType)
        {
            iRunReaderTypeRegistry.Add(iRunReaderType);
        }

        /// <summary>
        /// Creates and returns a registered HighlightHandler for the <c>uiElement</c>.
        /// </summary>
        /// <param name="uiElement">The DependencyObject that the HighlightHandler will work on</param>
        /// <param name="bodyHighlightAdornerBrush">Brush for highlights</param>
        /// <param name="bodyHighlightAdornerPen">Pen for highlights</param>
        /// <param name="bodyIterativeHighlightAdornerBrush">Iterative highlight Brush</param>
        /// <param name="bodyIterativeHighlightAdornerPen">Iterative highlight Pen</param>
        /// <returns></returns>
        public static HighlightHandler CreateHighlightHandlerFor(DependencyObject uiElement, System.Windows.Media.Brush bodyHighlightAdornerBrush, System.Windows.Media.Pen bodyHighlightAdornerPen, Brush bodyIterativeHighlightAdornerBrush, Pen bodyIterativeHighlightAdornerPen)
        {
            foreach(Type handlerType in highlightHandlerRegistry){
                if ((bool)handlerType.GetMethod("DoesHandle", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Invoke(null, new object[] { uiElement }))
                {
                    return (HighlightHandler)Activator.CreateInstance(handlerType, new object[] { uiElement, bodyHighlightAdornerBrush, bodyHighlightAdornerPen, bodyIterativeHighlightAdornerBrush, bodyIterativeHighlightAdornerPen });
                }
            }
            return new HighlightHandler(uiElement, bodyHighlightAdornerBrush, bodyHighlightAdornerPen, bodyIterativeHighlightAdornerBrush, bodyIterativeHighlightAdornerPen);

        }

        /// <summary>
        /// Creates and returns a registered IRunReader for the <c>runContainer</c>.
        /// </summary>
        /// <param name="runContainer">An object that contains Run objects.</param>
        /// <returns>IRunReader that can enumerate Run objects.</returns>
        public static IRunReader CreateIRunReaderHandlerFor(object runContainer)
        {
            foreach (Type handlerType in iRunReaderTypeRegistry)
            {
                if ((bool)handlerType.GetMethod("DoesHandle", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Invoke(null, new object[] { runContainer }))
                {
                    return (IRunReader)Activator.CreateInstance(handlerType, new object[] { runContainer });
                }
            }
            return null;

        }
    }
}
