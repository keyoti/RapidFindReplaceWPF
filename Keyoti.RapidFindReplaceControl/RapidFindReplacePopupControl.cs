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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Keyoti.RapidFindReplace.WPF.FindHandlers.HighlightHandlers;

namespace Keyoti.RapidFindReplace.WPF
{
    /// <summary>
    /// Popup control that hosts RapidFindReplaceControl.
    /// </summary>
    [TemplatePart(Name = "PART_Popup", Type = typeof(PinnedPopup))]
    [TemplatePart(Name = "PART_RapidFindReplaceControl", Type = typeof(RapidFindReplaceControl))]
    [TemplatePart(Name = "PART_CloseButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_Thumb", Type = typeof(Thumb))]
    public class RapidFindReplacePopupControl : Control, System.ComponentModel.INotifyPropertyChanged
    {

       
        internal FocusMonitor focusMonitor;

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

        PinnedPopup _PART_Popup;

        /// <summary>
        /// The popup template item, containing the find controls.
        /// </summary>
        public PinnedPopup PART_Popup { get { return _PART_Popup; } internal set { _PART_Popup = value; } }

        RapidFindReplaceControl _PART_RapidFindReplaceControl;

        /// <summary>
        /// The RapidFindReplaceControl in this popup.
        /// </summary>
        public RapidFindReplaceControl PART_RapidFindReplaceControl
        {
            get { return _PART_RapidFindReplaceControl; }
            internal set { _PART_RapidFindReplaceControl = value; }
        }

        Button _PART_CloseButton;

        /// <summary>
        /// Close button
        /// </summary>
        public Button PART_CloseButton
        {
            get { return _PART_CloseButton; }
            private set
            {
                if (_PART_CloseButton != null) _PART_CloseButton.Click -= new RoutedEventHandler(PART_CloseButton_Click);
                _PART_CloseButton = value;
                if (_PART_CloseButton != null) _PART_CloseButton.Click += new RoutedEventHandler(PART_CloseButton_Click);
            }
        }
        void PART_CloseButton_Click(object s, RoutedEventArgs e)
        {
            IsOpen = false;
            PART_RapidFindReplaceControl.ClearHighlights();
        }

        Thumb _PART_Thumb;

        /// <summary>
        /// Resize thumb.
        /// </summary>
        public Thumb PART_Thumb
        {
            get { return _PART_Thumb; }
            private set
            {
                if (_PART_Thumb != null)
                {
                    if (_PART_Thumb != null) _PART_Thumb.DragDelta -= _PART_Thumb_DragDelta;
                }
                _PART_Thumb = value;
                if (_PART_Thumb != null) _PART_Thumb.DragDelta += _PART_Thumb_DragDelta;
            }
        }

        void _PART_Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double newWidth = PART_Popup.Width + e.HorizontalChange;
            if (newWidth > MinWidth && newWidth > PART_Popup.MinWidth)
            {
                PART_Popup.Width += e.HorizontalChange;
                PART_Popup.HorizontalOffset -= e.HorizontalChange;
            }
        }

        Thumb _PART_MoveThumb;
        /// <summary>
        /// Header style thumb used to move the popup.
        /// </summary>
        public Thumb PART_MoveThumb
        {
            get { return _PART_MoveThumb; }
            private set
            {
                if (_PART_MoveThumb != null)
                {
                    if (_PART_MoveThumb != null) _PART_MoveThumb.DragDelta -= _PART_MoveThumb_DragDelta;
                }
                _PART_MoveThumb = value;
                if (_PART_MoveThumb != null) _PART_MoveThumb.DragDelta += _PART_MoveThumb_DragDelta;
            }
        }


        void _PART_MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            PART_Popup.HorizontalOffset += e.HorizontalChange;
            PART_Popup.VerticalOffset += e.VerticalChange;
        }


        

        #region Mirror properties from FindBox

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary>
        /// Body highlight brush to use
        /// </summary>
        public readonly static DependencyProperty BodyHighlightAdornerBrushProperty = DependencyProperty.Register("BodyHighlightAdornerBrush", typeof(Brush), typeof(RapidFindReplacePopupControl), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0x66, 0xFD, 0xDB, 0x07))));
        /// <summary>
        /// Body highlight brush to use
        /// </summary>
        public Brush BodyHighlightAdornerBrush
        {
            get { return (Brush)GetValue(BodyHighlightAdornerBrushProperty); }
            set { SetValue(BodyHighlightAdornerBrushProperty, value); }
        }


        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary>
        /// Brush to use when painting iterative highlights.  Iterative highlighting occurs when the user presses next/previous buttons.
        /// </summary>
        public readonly static DependencyProperty BodyIterativeHighlightAdornerBrushProperty = DependencyProperty.Register("BodyIterativeHighlightAdornerBrush", typeof(Brush), typeof(RapidFindReplacePopupControl)/*, new PropertyMetadata(new PropertyChangedCallback(BodyIterativeHighlightAdornerBrushChanged))*/);
        /// <summary>
        /// Brush to use when painting iterative highlights.  Iterative highlighting occurs when the user presses next/previous buttons.
        /// </summary>
        public Brush BodyIterativeHighlightAdornerBrush
        {
            get { return (Brush)GetValue(BodyIterativeHighlightAdornerBrushProperty); }
            set { SetValue(BodyIterativeHighlightAdornerBrushProperty, value); }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary>
        /// Pen to use to draw border around body highlights.
        /// </summary>
        public readonly static DependencyProperty BodyHighlightAdornerPenProperty = DependencyProperty.Register("BodyHighlightAdornerPen", typeof(Pen), typeof(RapidFindReplacePopupControl)/*, new PropertyMetadata(new PropertyChangedCallback(BodyHighlightAdornerPenChanged))*/);
        /// <summary>
        /// Pen to use to draw border around body highlights.
        /// </summary>
        public Pen BodyHighlightAdornerPen
        {
            get { return (Pen)GetValue(BodyHighlightAdornerPenProperty); }
            set { SetValue(BodyHighlightAdornerPenProperty, value); }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary>
        /// The current match that the user has iterated to using the next/previous buttons.
        /// </summary>
        public readonly static DependencyProperty CurrentMatchProperty = DependencyProperty.Register("CurrentMatch", typeof(Highlight), typeof(RapidFindReplacePopupControl));
        /// <summary>
        /// The current match that the user has iterated to using the next/previous buttons.
        /// </summary>
        public Highlight CurrentMatch
        {
            get { return (Highlight)GetValue(CurrentMatchProperty); }
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary>
        /// Number of hits found.
        /// </summary>
        public readonly static DependencyProperty NumberOfHitsProperty = DependencyProperty.Register("NumberOfHits", typeof(int), typeof(RapidFindReplacePopupControl));
        /// <summary>
        /// Number of hits found.
        /// </summary>
        public int NumberOfHits
        {
            get { return (int)GetValue(NumberOfHitsProperty); }
            set { SetValue(NumberOfHitsProperty, value); }
        }


        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary>
        /// Whether the user's query is valid or not.
        /// </summary>
        /// <remarks>A query can be invalid for example if regular expressions are used but the expression is invalid.</remarks>
        public readonly static DependencyProperty IsQueryValidProperty = DependencyProperty.Register("IsQueryValid", typeof(bool), typeof(RapidFindReplacePopupControl));
        /// <summary>
        /// Whether the user's query is valid or not.
        /// </summary>
        /// <remarks>A query can be invalid for example if regular expressions are used but the expression is invalid.</remarks>
        public bool IsQueryValid
        {
            get { return (bool)GetValue(IsQueryValidProperty); }
            set { SetValue(IsQueryValidProperty, value); }
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary>
        /// Control and it's children to find within.
        /// </summary>
        public readonly static DependencyProperty FindScopeProperty = DependencyProperty.Register("FindScope", typeof(DependencyObject), typeof(RapidFindReplacePopupControl));
        /// <summary>
        /// Control and it's children to find within.
        /// </summary>
        public DependencyObject FindScope
        {
            get { return (DependencyObject)GetValue(FindScopeProperty); }
            set { SetValue(FindScopeProperty, value); }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary>
        /// The query to search for.
        /// </summary>
        public readonly static DependencyProperty QueryProperty = DependencyProperty.Register("Query", typeof(string), typeof(RapidFindReplacePopupControl));

        /// <summary>
        /// The query to search for.
        /// </summary>
        public string Query
        {
            get { return (string)GetValue(QueryProperty); }
            set { SetValue(QueryProperty, value); }
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


        /// <summary>
        /// Number of query history items to hold and display.
        /// </summary>
        public readonly static DependencyProperty QueryHistoryCapacityProperty = DependencyProperty.Register("QueryHistoryCapacity", typeof(int), typeof(RapidFindReplacePopupControl), new PropertyMetadata(5));


        /// <summary>
        /// Number of query history items to hold and display.
        /// </summary>
        public int QueryHistoryCapacity
        {
            get { return (int)GetValue(QueryHistoryCapacityProperty); }
            set { SetValue(QueryHistoryCapacityProperty, value); }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        /// <summary>
        /// Number of query characters that must be entered before as you type find operations are performed.
        /// </summary>
        public readonly static DependencyProperty AsYouTypeFindMinimumCharactersProperty = DependencyProperty.Register("AsYouTypeFindMinimumCharacters", typeof(int), typeof(RapidFindReplacePopupControl), new PropertyMetadata(2));

        /// <summary>
        /// Number of query characters that must be entered before as you type find operations are performed.
        /// </summary>
        public int AsYouTypeFindMinimumCharacters
        {
            get { return (int)GetValue(AsYouTypeFindMinimumCharactersProperty); }
            set { SetValue(AsYouTypeFindMinimumCharactersProperty, value); }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        /// <summary>
        /// Border brush around textbox to use when no hits are found (eg. red).
        /// </summary>
        public readonly static DependencyProperty NoHitsBorderBrushColorProperty = DependencyProperty.Register("NoHitsBorderBrushColor", typeof(Color), typeof(RapidFindReplacePopupControl));
        /// <summary>
        /// Border brush around textbox to use when no hits are found (eg. red).
        /// </summary>
        public Color NoHitsBorderBrushColor
        {
            get { return (Color)GetValue(NoHitsBorderBrushColorProperty); }
            set { SetValue(NoHitsBorderBrushColorProperty, value); }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary>
        /// Watermark text to use in the find textbox.
        /// </summary>
        public readonly static DependencyProperty WatermarkTextProperty = DependencyProperty.Register("WatermarkText", typeof(string), typeof(RapidFindReplacePopupControl)/*, new PropertyMetadata(new PropertyChangedCallback(WatermarkTextChanged))*/);
        /// <summary>
        /// Watermark text to use in the find textbox.
        /// </summary>
        public string WatermarkText
        {
            get { return (string)GetValue(WatermarkTextProperty); }
            set { SetValue(WatermarkTextProperty, value); }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary>
        /// Icon Brush to use to paint Find button.
        /// </summary>
        public readonly static DependencyProperty FindButtonIconProperty = DependencyProperty.Register("FindButtonIcon", typeof(Brush), typeof(RapidFindReplacePopupControl));
        /// <summary>
        /// Icon Brush to use to paint Find button.
        /// </summary>
        public Brush FindButtonIcon
        {
            get { return (Brush)GetValue(FindButtonIconProperty); }
            set { SetValue(FindButtonIconProperty, value); }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary>
        /// Icon Brush to use to paint option dropdown button.
        /// </summary>
        public readonly static DependencyProperty OptionsButtonIconProperty = DependencyProperty.Register("OptionsButtonIcon", typeof(Brush), typeof(RapidFindReplacePopupControl));
        /// <summary>
        /// Icon Brush to use to paint option dropdown button.
        /// </summary>
        public Brush OptionsButtonIcon
        {
            get { return (Brush)GetValue(OptionsButtonIconProperty); }
            set { SetValue(OptionsButtonIconProperty, value); }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary>
        /// Icon Brush to use to paint option dropdown button when it is in the checked state.
        /// </summary>
        public readonly static DependencyProperty OptionsButtonCheckedIconProperty = DependencyProperty.Register("OptionsButtonCheckedIcon", typeof(Brush), typeof(RapidFindReplacePopupControl));
        /// <summary>
        /// Icon Brush to use to paint option dropdown button when it is in the checked state.
        /// </summary>
        public Brush OptionsButtonCheckedIcon
        {
            get { return (Brush)GetValue(OptionsButtonCheckedIconProperty); }
            set { SetValue(OptionsButtonCheckedIconProperty, value); }
        }


        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        /// <summary>
        /// Whether the replace part of the dialog is open.
        /// </summary>
        public readonly static DependencyProperty IsReplaceOpenProperty = DependencyProperty.Register("IsReplaceOpen", typeof(bool), typeof(RapidFindReplacePopupControl));
        /// <summary>
        /// Whether the replace part of the dialog is open.
        /// </summary>
        public bool IsReplaceOpen
        {
            get { return (bool)GetValue(IsReplaceOpenProperty); }
            set { SetValue(IsReplaceOpenProperty, value); }
        }


        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        /// <summary>
        /// Whether the options dropdown is open.
        /// </summary>
        public readonly static DependencyProperty IsOptionsDropDownOpenProperty = DependencyProperty.Register("IsOptionsDropDownOpen", typeof(bool), typeof(RapidFindReplacePopupControl));

        /// <summary>
        /// Whether the options dropdown is open.
        /// </summary>
        public bool IsOptionsDropDownOpen
        {
            get { return (bool)GetValue(IsOptionsDropDownOpenProperty); }
            set { SetValue(IsOptionsDropDownOpenProperty, value); }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary>
        /// The history of queries entered by the user.
        /// </summary>
        public readonly static DependencyProperty QueryHistoryProperty = DependencyProperty.Register("QueryHistory", typeof(ObservableBackwardsRingBuffer<string>), typeof(RapidFindReplacePopupControl));
        /// <summary>
        /// The history of queries entered by the user.
        /// </summary>
        public ObservableBackwardsRingBuffer<string> QueryHistory
        {
            get { return (ObservableBackwardsRingBuffer<string>)GetValue(QueryHistoryProperty); }
            set { SetValue(QueryHistoryProperty, value); }
        }
        

        #endregion
        //--------------------------------------------------------------------------------------------------
        /// <summary>
        /// If set, the Placement target that the popup will use. 
        /// </summary>
        public readonly static DependencyProperty PlacementTargetProperty = DependencyProperty.Register("PlacementTarget", typeof(UIElement), typeof(RapidFindReplacePopupControl));

        /// <summary>
        /// Target to place the popup against, if set.
        /// </summary>
        public UIElement PlacementTarget
        {
            get { return (UIElement)GetValue(PlacementTargetProperty); }
            set { SetValue(PlacementTargetProperty, value); }
        }
        //--------------------------------------------------------------------------------------------------

        /// <summary>
        /// Whether the popup is open. 
        /// </summary>
        public readonly static DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(RapidFindReplacePopupControl), new PropertyMetadata(new PropertyChangedCallback(IsOpenPropertyChanged)));

        static void IsOpenPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                RapidFindReplacePopupControl pop = sender as RapidFindReplacePopupControl;
                if (pop.PART_RapidFindReplaceControl != null && pop.PART_RapidFindReplaceControl.PART_FindTextBox != null)
                {
                    pop.PART_RapidFindReplaceControl.PART_FindTextBox.Focus();
                    if(pop._PART_RapidFindReplaceControl.FindOptions.FindAsYouType)
                        pop.PART_RapidFindReplaceControl.ViewModel.FindTextCommand.Execute(null);
                }
            }
        }

        /// <summary>
        /// Whether the popup is open.
        /// </summary>
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }
        //--------------------------------------------------------------------------------------------------

        /// <summary>
        /// Whether this popup will stay open when the form is deactivated.
        /// </summary>
        public readonly static DependencyProperty StaysOpenProperty = DependencyProperty.Register("StaysOpen", typeof(bool), typeof(RapidFindReplacePopupControl), new PropertyMetadata(true, new PropertyChangedCallback(StaysOpenPropertyChanged)));

        static void StaysOpenPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
                RapidFindReplacePopupControl pop = sender as RapidFindReplacePopupControl;
                pop._PART_Popup.StaysOpen = (bool)e.NewValue;                
        }

        /// <summary>
        /// Whether this popup will stay open when the form is deactivated.
        /// </summary>
        public bool StaysOpen
        {
            get { return (bool)GetValue(StaysOpenProperty); }
            set { SetValue(StaysOpenProperty, value); }
        }
        //--------------------------------------------------------------------------------------------------

        
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        /// <summary>
        /// Where to dock the popup.
        /// </summary>
        public readonly static DependencyProperty DockingPositionProperty = DependencyProperty.Register("DockingPosition", typeof(PinnedPopup.DockPosition), typeof(RapidFindReplacePopupControl), new PropertyMetadata(PinnedPopup.DockPosition.TopRight));

        /// <summary>
        /// Where to dock the popup.
        /// </summary>
        public PinnedPopup.DockPosition DockingPosition
        {
            get { return (PinnedPopup.DockPosition)GetValue(DockingPositionProperty); }
            set { SetValue(DockingPositionProperty, value); }
        }
        

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

 

        /// <summary>
        /// New.
        /// </summary>
        public RapidFindReplacePopupControl()
        {
            
        }

       

        static RapidFindReplacePopupControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RapidFindReplacePopupControl), new FrameworkPropertyMetadata(typeof(RapidFindReplacePopupControl)));
        }

        
        
       
        /// <summary>
        /// Called when the control template has been applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            
            PART_Popup = GetTemplateChild("PART_Popup") as PinnedPopup;
            PART_RapidFindReplaceControl = GetTemplateChild("PART_RapidFindReplaceControl") as RapidFindReplaceControl;
            PART_CloseButton = GetTemplateChild("PART_CloseButton") as Button;
            PART_Thumb = GetTemplateChild("PART_Thumb") as Thumb;
            PART_MoveThumb = GetTemplateChild("PART_MoveThumb") as Thumb;

            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))//Blend uses a StandinPopup class so PART_OptionsPopup will be null
            {
                Window parentWindow = Utility.FindWindow(this);

                if (PART_Popup.DockingPosition != PinnedPopup.DockPosition.None)
                    PART_Popup.DockingPosition = DockingPosition;//we want our instance to take precedence

                focusMonitor = FocusMonitor.GetCreateInstanceFor(parentWindow);


                //if the PlacementTarget isn't set, get the window containing us
                if (ReadLocalValue(PlacementTargetProperty) == DependencyProperty.UnsetValue && !DesignerProperties.GetIsInDesignMode(this))
                    PART_Popup.PlacementTarget = parentWindow;
                else
                    PART_Popup.PlacementTarget = PlacementTarget;

                BindingOperations.SetBinding(this, RapidFindReplacePopupControl.DockingPositionProperty, new Binding { Source = PART_Popup, Path = new PropertyPath("DockingPosition"), Mode = BindingMode.TwoWay });
                BindingOperations.SetBinding(this, RapidFindReplacePopupControl.PlacementTargetProperty, new Binding { Source = PART_Popup, Path = new PropertyPath("PlacementTarget"), Mode = BindingMode.TwoWay });
                BindingOperations.SetBinding(this, RapidFindReplacePopupControl.IsOpenProperty, new Binding { Source = PART_Popup, Path = new PropertyPath("IsOpen"), Mode = BindingMode.TwoWay });


                CommandBinding findCommandBinding = new CommandBinding(ApplicationCommands.Find, (object target, ExecutedRoutedEventArgs e) =>
                {
                    IsOpen = true;
                }, (object target, CanExecuteRoutedEventArgs e) => { e.CanExecute = true; e.Handled = true; });

                Window rootWindow = Utility.FindWindow(this);
                rootWindow.CommandBindings.Add(findCommandBinding);
            }

            #region Mirror properties from find box
            /*
//BodyHighlightAdornerBrush
            if (BodyHighlightAdornerBrush!=null && BodyHighlightAdornerBrush != DependencyProperty.UnsetValue)
                PART_RapidFindReplaceControl.BodyHighlightAdornerBrush = BodyHighlightAdornerBrush;
            else
                BodyHighlightAdornerBrush = PART_RapidFindReplaceControl.BodyHighlightAdornerBrush;

            BindingOperations.SetBinding(this, RapidFindReplacePopupControl.BodyHighlightAdornerBrushProperty, new Binding { Source = PART_RapidFindReplaceControl, Path = new PropertyPath("BodyHighlightAdornerBrush"), Mode = BindingMode.TwoWay, UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged });


//BodyIterativeHighlightAdornerBrush
            if (BodyIterativeHighlightAdornerBrush != null && BodyIterativeHighlightAdornerBrush != DependencyProperty.UnsetValue)
                PART_RapidFindReplaceControl.BodyIterativeHighlightAdornerBrush = BodyIterativeHighlightAdornerBrush;
            else
                BodyIterativeHighlightAdornerBrush = PART_RapidFindReplaceControl.BodyIterativeHighlightAdornerBrush;

            BindingOperations.SetBinding(this, RapidFindReplacePopupControl.BodyIterativeHighlightAdornerBrushProperty, new Binding { Source = PART_RapidFindReplaceControl, Path = new PropertyPath("BodyIterativeHighlightAdornerBrush"), Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

//BodyHighlightAdornerPen
            if (BodyHighlightAdornerPen != null && BodyHighlightAdornerPen != DependencyProperty.UnsetValue)
                PART_RapidFindReplaceControl.BodyHighlightAdornerPen = BodyHighlightAdornerPen;
            else
                BodyHighlightAdornerPen = PART_RapidFindReplaceControl.BodyHighlightAdornerPen;

            BindingOperations.SetBinding(this, RapidFindReplacePopupControl.BodyHighlightAdornerPenProperty, new Binding { Source = PART_RapidFindReplaceControl, Path = new PropertyPath("BodyHighlightAdornerPen"), Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            */
            if (PART_RapidFindReplaceControl != null)
            {
                BindTwoWayToFindBox(RapidFindReplacePopupControl.BodyHighlightAdornerBrushProperty, PART_RapidFindReplaceControl, RapidFindReplaceControl.BodyHighlightAdornerBrushProperty);
                BindTwoWayToFindBox(RapidFindReplacePopupControl.BodyIterativeHighlightAdornerBrushProperty, PART_RapidFindReplaceControl, RapidFindReplaceControl.BodyIterativeHighlightAdornerBrushProperty);
                BindTwoWayToFindBox(RapidFindReplacePopupControl.BodyHighlightAdornerPenProperty, PART_RapidFindReplaceControl, RapidFindReplaceControl.BodyHighlightAdornerPenProperty);


                BindingOperations.SetBinding(this, RapidFindReplacePopupControl.CurrentMatchProperty, new Binding { Source = PART_RapidFindReplaceControl, Path = new PropertyPath("CurrentMatch"), Mode = BindingMode.OneWay });
                BindingOperations.SetBinding(this, RapidFindReplacePopupControl.NumberOfHitsProperty, new Binding { Source = PART_RapidFindReplaceControl, Path = new PropertyPath("NumberOfHits"), Mode = BindingMode.OneWay });
                BindingOperations.SetBinding(this, RapidFindReplacePopupControl.IsQueryValidProperty, new Binding { Source = PART_RapidFindReplaceControl, Path = new PropertyPath("IsQueryValid"), Mode = BindingMode.OneWay });
                BindingOperations.SetBinding(this, RapidFindReplacePopupControl.QueryHistoryProperty, new Binding { Source = PART_RapidFindReplaceControl, Path = new PropertyPath("QueryHistory"), Mode = BindingMode.OneWay });


                BindTwoWayToFindBox(RapidFindReplacePopupControl.FindScopeProperty, PART_RapidFindReplaceControl, RapidFindReplaceControl.FindScopeProperty);
                BindTwoWayToFindBox(RapidFindReplacePopupControl.QueryProperty, PART_RapidFindReplaceControl, RapidFindReplaceControl.QueryProperty);
                BindTwoWayToFindBox(RapidFindReplacePopupControl.QueryHistoryCapacityProperty, PART_RapidFindReplaceControl, RapidFindReplaceControl.QueryHistoryCapacityProperty);
                BindTwoWayToFindBox(RapidFindReplacePopupControl.AsYouTypeFindMinimumCharactersProperty, PART_RapidFindReplaceControl, RapidFindReplaceControl.AsYouTypeFindMinimumCharactersProperty);
                BindTwoWayToFindBox(RapidFindReplacePopupControl.WatermarkTextProperty, PART_RapidFindReplaceControl, RapidFindReplaceControl.WatermarkTextProperty);
                BindTwoWayToFindBox(RapidFindReplacePopupControl.FindButtonIconProperty, PART_RapidFindReplaceControl, RapidFindReplaceControl.FindButtonIconProperty);
                BindTwoWayToFindBox(RapidFindReplacePopupControl.OptionsButtonIconProperty, PART_RapidFindReplaceControl, RapidFindReplaceControl.OptionsButtonIconProperty);
                BindTwoWayToFindBox(RapidFindReplacePopupControl.OptionsButtonCheckedIconProperty, PART_RapidFindReplaceControl, RapidFindReplaceControl.OptionsButtonCheckedIconProperty);
                BindTwoWayToFindBox(RapidFindReplacePopupControl.IsReplaceOpenProperty, PART_RapidFindReplaceControl, RapidFindReplaceControl.IsReplaceOpenProperty);
                BindTwoWayToFindBox(RapidFindReplacePopupControl.IsOptionsDropDownOpenProperty, PART_RapidFindReplaceControl, RapidFindReplaceControl.IsOptionsDropDownOpenProperty);
            }          


            #endregion


            base.OnApplyTemplate();
        }

    
            void BindTwoWayToFindBox(DependencyProperty myProperty, RapidFindReplaceControl findBox, DependencyProperty findBoxProperty){
                //if (FindScope != null && FindScope != DependencyProperty.UnsetValue)
                if(GetValue(myProperty)!=null && GetValue(myProperty)!=DependencyProperty.UnsetValue){
                    findBox.SetValue(findBoxProperty, GetValue(myProperty));
                    //PART_RapidFindReplaceControl.FindScope = FindScope;
                }else{
                    SetValue(myProperty, findBox.GetValue(findBoxProperty));
                    //FindScope = PART_RapidFindReplaceControl.FindScope;
                }
                BindingOperations.SetBinding(this, myProperty, new Binding { Source = findBox, Path = new PropertyPath(findBoxProperty.Name), Mode = BindingMode.TwoWay });
            
            }

        
        
    }
}
