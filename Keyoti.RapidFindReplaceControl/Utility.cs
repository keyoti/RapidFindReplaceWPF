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
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Keyoti.RapidFindReplace.WPF
{
    class Utility
    {

        
        /// <summary>
        /// Gets the scroll viewer contained within the FlowDocumentScrollViewer control
        /// </summary>
        public static ScrollViewer FindScrollViewer(DependencyObject obj)
        {
            ScrollViewer scrollViewer;
            do
            {
                if (VisualTreeHelper.GetChildrenCount(obj) > 0)
                    obj = VisualTreeHelper.GetChild(obj as Visual, 0);
                else
                    return null;
            }
            while (!(obj is ScrollViewer));

            scrollViewer = obj as ScrollViewer;
            return scrollViewer;

        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static Window FindWindow(DependencyObject obj)
        {
            if(obj is Window) return obj as Window;
            else 
                return FindWindow(VisualTreeHelper.GetParent(obj));
        }

        public static Window CurrentWindow()
        {
            if (Application.Current!=null && Application.Current.Windows != null)
            {
                foreach (Window w in Application.Current.Windows)
                {
                    if (w.IsActive)
                        return w;
                }
            }
            return null;
        }

        //below from http://blogs.microsoft.co.il/blogs/alex_golesh/archive/2009/09/20/wpf-quick-tip-how-to-get-wpf-window-client-area-size.aspx
        #region Native Methods
        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left_, int top_, int right_, int bottom_)
            {
                Left = left_;
                Top = top_;
                Right = right_;
                Bottom = bottom_;
            }

            public int Height { get { return Bottom - Top; } }
            public int Width { get { return Right - Left; } }
            public Size Size { get { return new Size(Width, Height); } }

            public Point Location { get { return new Point(Left, Top); } }

            // Handy method for converting to a System.Drawing.Rectangle
            public Rect ToRectangle()
            { return new Rect(Left, Top, Right, Bottom); }

            public static RECT FromRectangle(Rect rectangle)
            {
                return new Rect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
            }

            public override int GetHashCode()
            {
                return Left ^ ((Top << 13) | (Top >> 0x13))
                  ^ ((Width << 0x1a) | (Width >> 6))
                  ^ ((Height << 7) | (Height >> 0x19));
            }

            #region Operator overloads

            public static implicit operator Rect(RECT rect)
            {
                return rect.ToRectangle();
            }

            public static implicit operator RECT(Rect rect)
            {
                return FromRectangle(rect);
            }

            #endregion
        }

        static WeakReference lastWindow;
        static IntPtr lastWindowPtr;
        static PresentationSource presentationSource =null;

        /// <summary>
        /// Get the client area size of a window
        /// </summary>
        public static Size GetClientRect(Window window)
        {
            IntPtr hWnd;
            if (lastWindow!=null && lastWindow.IsAlive && (lastWindow.Target as Window) == window) hWnd = lastWindowPtr;
            else
            {
                hWnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
                lastWindow = new WeakReference(window);
                lastWindowPtr = hWnd;
            }
            RECT result = new RECT();
            GetClientRect(hWnd, out result);

            if(presentationSource==null)
                presentationSource = PresentationSource.FromVisual(window);

            if (presentationSource.CompositionTarget == null) return new Size(window.ActualWidth-20, window.ActualHeight);//best we can do, but does cause bit of misplacement

            double pixelCalculationFactorX = presentationSource.CompositionTarget.TransformFromDevice.M11;
            double pixelCalculationFactorY = presentationSource.CompositionTarget.TransformFromDevice.M22;

            return new Size(result.Width * pixelCalculationFactorX, result.Height * pixelCalculationFactorY);
        }
        #endregion

    }

    
}
