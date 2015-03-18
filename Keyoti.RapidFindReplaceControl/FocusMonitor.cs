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
using System.Windows.Input;

namespace Keyoti.RapidFindReplace.WPF
{
    /// <summary>
    /// Monitors control focus
    /// </summary>
    class FocusMonitor
    {
        static List<WeakReference> instances =new List<WeakReference>();
        public static FocusMonitor GetCreateInstanceFor(DependencyObject focusScopeToMonitor){
            foreach(WeakReference reference in instances){
                if(reference.IsAlive){
                    FocusMonitor refMonitor = reference.Target as FocusMonitor;
                    if (refMonitor.focusScopeToMonitorRef!=null && refMonitor.focusScopeToMonitorRef.IsAlive)
                    {
                        if((refMonitor.focusScopeToMonitorRef.Target as DependencyObject)==focusScopeToMonitor)
                            return reference.Target as FocusMonitor;
                    }
                }
            }
            FocusMonitor monitor = new FocusMonitor(focusScopeToMonitor);
            instances.Add(new WeakReference(monitor));
            return monitor;
        }

        WeakReference focusScopeToMonitorRef = null;//, lastKeyboardFocusRef =null;
        ObservableBackwardsRingBuffer<WeakReference> focussedElementBuffer = new ObservableBackwardsRingBuffer<WeakReference>(10);//because we don't know which controls to track until after the RapidFindReplaceControl is created, we must record a list and pick the first one that is pertinent
        
        public FocusMonitor(DependencyObject focusScopeToMonitor)
        {
            if (focusScopeToMonitor!=null && FocusManager.GetIsFocusScope(focusScopeToMonitor))
            {
                this.focusScopeToMonitorRef = new WeakReference(focusScopeToMonitor);
            }

            //poll for focus to track ordering of focused elements
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += delegate{
                if (focusScopeToMonitorRef!=null && focusScopeToMonitorRef.IsAlive)
                {
                    IInputElement logicalFocus = FocusManager.GetFocusedElement(focusScopeToMonitorRef.Target as DependencyObject);
                    IInputElement keyboardFocus = Keyboard.FocusedElement;
                    if (logicalFocus == keyboardFocus && keyboardFocus is UIElement)
                    {
                        //if(RapidFindReplaceControl.GetIsFindable(keyboardFocus as UIElement))
                            //lastKeyboardFocusRef = new WeakReference(keyboardFocus as DependencyObject);
                        if (focussedElementBuffer.Count==0 || (focussedElementBuffer[0] != null && focussedElementBuffer[0].IsAlive && focussedElementBuffer[0].Target != keyboardFocus))
                            focussedElementBuffer.AddItem(new WeakReference(keyboardFocus as UIElement));
                    }
                }
                else
                    dispatcherTimer.Stop();
            };
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
        }

        public DependencyObject LastKeyboardFocussedControl(RapidFindReplaceControlViewModel model)
        {

            //look for most recent UIElement that got focus that we will search (mostly this is to ignore menus and buttons that might have opened the find popup)
                foreach (WeakReference reference in focussedElementBuffer)
                {
                    if (reference != null && reference.IsAlive)
                    {
                        UIElement targ = reference.Target as UIElement;
                        bool dummy=true;
                        if (model.WillSearchInElement(targ, true, ref dummy, RapidFindReplaceControl.GetIsFindable(targ)))
                        {
                            return targ as DependencyObject;
                        }
                    }
                    
                }
                return null;

            
        }

        public void BeginMonitoring(DependencyObject focusScopeToMonitor)
        {
            if (focusScopeToMonitor != null && FocusManager.GetIsFocusScope(focusScopeToMonitor))
            {
                this.focusScopeToMonitorRef = new WeakReference(focusScopeToMonitor);
            }
            else this.focusScopeToMonitorRef = null;           
        }
    }

    

}
