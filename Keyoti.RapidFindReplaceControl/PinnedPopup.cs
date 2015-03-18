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
using System.Windows.Controls.Primitives;

namespace Keyoti.RapidFindReplace.WPF
{
    /*staysopen and topmost issue.  if we leave it open when the form is deactivated, it will be TopMost.
     * one solution not implemented is http://chriscavanagh.wordpress.com/2008/08/13/non-topmost-wpf-popup/
     */ 

    /// <summary>
    /// Popup control used to hold RapidFindReplaceControl.
    /// </summary>
    public class PinnedPopup : Popup, System.ComponentModel.INotifyPropertyChanged
    {

#if DEBUG
                /// <summary>
        /// 
        /// </summary>
        public static bool DEBUG_DO_NOT_CLOSE_ON_DEACTIVATE = false;
#endif

        /// <summary>
        /// Where to dock the popup.  Leave at None to supply custom offsets.
        /// </summary>
        public enum DockPosition
        {
            /// <summary>
            /// No dock
            /// </summary>
            None=1,
            /// <summary>
            /// Top left
            /// </summary>
            TopLeft=2,
            /// <summary>
            /// Top center
            /// </summary>
            TopCenter=3,
            /// <summary>
            /// Top right
            /// </summary>
            TopRight=4,
            /// <summary>
            /// Middle left
            /// </summary>
            MiddleLeft=5,
            /// <summary>
            /// Middle center
            /// </summary>
            MiddleCenter=6,
            /// <summary>
            /// Middle right
            /// </summary>
            MiddleRight=7,
            /// <summary>
            /// Bottom left
            /// </summary>
            BottomLeft=8,
            /// <summary>
            /// Bottom center
            /// </summary>
            BottomCenter=9,
            /// <summary>
            /// Bottom right
            /// </summary>
            BottomRight=10
        }

        /// <summary>
        /// Where to dock this popup.
        /// </summary>
        public readonly static DependencyProperty DockingPositionProperty = DependencyProperty.Register("DockingPosition", typeof(DockPosition), typeof(PinnedPopup));
        /// <summary>
        /// Where to dock this popup.
        /// </summary>
        public DockPosition DockingPosition
        {
            get { return (DockPosition)GetValue(DockingPositionProperty); }
            set { SetValue(DockingPositionProperty, value); }
        }
        
        
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



       
        double _DockRightOffsetX = 0;

        /// <summary>
        /// The width of the placement target minus the width of this control, so that the popup can easily be aligned to the right edge of the placement target.
        /// </summary>
        public double DockRightOffsetX
        {
            get
            {
                return _DockRightOffsetX;
            }
            internal set
            {
                if (_DockRightOffsetX != value)
                {
                    _DockRightOffsetX = value;
                    RaisePropertyChanged("DockRightOffsetX");
                }
            }

        }

        bool closedTemporarily = false;
        /// <summary>
        /// New
        /// </summary>
        public PinnedPopup() : base() {
            Initialized += delegate(object s, EventArgs e)
            {
               /*Odd thing happening, without the LocationChanged and SizeChanged code below, the placement is off, and since the code below is useful when StaysOpen==true, will leave in place, even
                * if !StaysOpen
                * */

                    Window parentWindow = Window.GetWindow(this);
                    if (parentWindow != null)
                    {
                        parentWindow.LocationChanged += delegate(object sender2, EventArgs args)
                            {
                                HorizontalOffset += 1;
                                HorizontalOffset -= 1;
                            };

                        //if (StaysOpen)
                       // {
                            parentWindow.Deactivated += delegate(object sender, EventArgs args)
                            {
                //                IsOpen = false;
                                if (IsOpen && StaysOpen 
#if DEBUG                                    
                                    && !DEBUG_DO_NOT_CLOSE_ON_DEACTIVATE
#endif                                    
                                    )
                                {
                                    closedTemporarily = true;
                                    IsOpen = false;
                                }
                            };

                            parentWindow.Activated += delegate
                            {
                                if (closedTemporarily) IsOpen = true;
                                closedTemporarily = false;
                            };
                        //}

                        parentWindow.SizeChanged += delegate(object sender2, SizeChangedEventArgs args)
                        {
                            SetOffsets();
                            //HorizontalOffset = parentWindow.ActualWidth - 250; 
                           // HorizontalOffset += 1;
                           // HorizontalOffset -= 1;
                        };
                        //SetOffsets();
                        //HorizontalOffset = parentWindow.ActualWidth - 250; 
                           
                    }
                    
              /*
                CustomPopupPlacementCallback += (Size popupSize, Size targetSize, Point offset) =>
                            new[] { new CustomPopupPlacement() { Point = new Point(targetSize.Width - popupSize.Width, targetSize.Height - 2) } };
                
                */

            };

            Opened += delegate(object s, EventArgs e)
            {
                SetOffsets();
            };
        
        }

        void SetOffsets()
        {
            if (PlacementTarget is FrameworkElement && Child is FrameworkElement)
            {

                double targetWidth, targetHeight;

                if (PlacementTarget is Window)
                {
                    Size clientSize = Utility.GetClientRect(PlacementTarget as Window);
                    targetWidth = clientSize.Width;
                    targetHeight = clientSize.Height;
                }
                else
                {
                    targetWidth = (PlacementTarget as FrameworkElement).ActualWidth;
                    targetHeight = (PlacementTarget as FrameworkElement).ActualHeight;
                }
                double popWidth = (Child as FrameworkElement).ActualWidth, popHeight = (Child as FrameworkElement).ActualHeight;

                switch (DockingPosition)
                {

                    case DockPosition.TopLeft:
                        Placement = PlacementMode.Relative;
                        HorizontalOffset = 0;
                        VerticalOffset = 0;
                        break;
                    case DockPosition.TopCenter:
                        Placement = PlacementMode.Relative;
                        HorizontalOffset = (targetWidth - popWidth)/2;
                        VerticalOffset = 0;
                        break;
                    case DockPosition.TopRight:
                        Placement = PlacementMode.Relative;
                        HorizontalOffset = targetWidth - popWidth;
                        VerticalOffset = 0;
                        break;
                    case DockPosition.MiddleLeft:
                        Placement = PlacementMode.Relative;
                        HorizontalOffset = 0;
                        VerticalOffset = (targetHeight - popHeight) / 2;
                        break;
                    case DockPosition.MiddleCenter:
                        Placement = PlacementMode.Relative;
                        HorizontalOffset = (targetWidth - popWidth) / 2;
                        VerticalOffset = (targetHeight - popHeight) / 2;
                        break;
                    case DockPosition.MiddleRight:
                        Placement = PlacementMode.Relative;
                        HorizontalOffset = targetWidth - popWidth;
                        VerticalOffset = (targetHeight - popHeight) / 2;
                        break;
                    case DockPosition.BottomLeft:
                        Placement = PlacementMode.Relative;
                        HorizontalOffset = 0;
                        VerticalOffset = targetHeight - popHeight;
                        break;
                    case DockPosition.BottomCenter:
                        Placement = PlacementMode.Relative;
                        HorizontalOffset = (targetWidth - popWidth) / 2;
                        VerticalOffset = targetHeight - popHeight;
                        break;
                    case DockPosition.BottomRight:
                        Placement = PlacementMode.Relative;
                        HorizontalOffset = targetWidth - popWidth;
                        VerticalOffset = targetHeight - popHeight;
                        break;
                }
            }
        }

        
        
    }
}
