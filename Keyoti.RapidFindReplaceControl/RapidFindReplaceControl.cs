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
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Keyoti.RapidFindReplace.WPF.FindHandlers.HighlightHandlers;

namespace Keyoti.RapidFindReplace.WPF
{
    /// <summary>
    /// Find/Replace control.
    /// </summary>
    [TemplatePart(Name = "PART_FindTextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_ReplaceTextBox", Type = typeof(TextBox))]    
    [TemplatePart(Name = "PART_FindButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_NextMatchButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_PreviousMatchButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ReplaceButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ReplaceAllButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_OptionsButton", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_EasingColorKeyFrameEnd", Type = typeof(EasingColorKeyFrame))]
    [TemplatePart(Name = "PART_EmptyText", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_OptionsList", Type = typeof(ListView))]
    [TemplatePart(Name = "PART_HistoryList", Type = typeof(ListView))]
    [TemplatePart(Name = "PART_OptionsPopup", Type = typeof(Popup))]
    [TemplateVisualState(Name = "MouseOver", GroupName = "MouseStates")]
    [TemplateVisualState(Name = "MouseOut", GroupName = "MouseStates")]
    [TemplateVisualState(Name = "Query", GroupName = "QueryStates")]
    [TemplateVisualState(Name = "NoQuery", GroupName = "QueryStates")]
    [TemplateVisualState(Name = "Focus", GroupName = "FocusStates")]
    [TemplateVisualState(Name = "Blur_NoQuery", GroupName = "FocusStates")]
    [TemplateVisualState(Name = "Blur_Query", GroupName = "FocusStates")]
    [TemplateVisualState(Name = "NoHits", GroupName = "HitStates")]
    [TemplateVisualState(Name = "Hits", GroupName = "HitStates")]
    [TemplateVisualState(Name = "NoFind", GroupName = "HitStates")]//No find operation has been performed, regardless of whether a query is present or not. 
    public class RapidFindReplaceControl : Control, System.ComponentModel.INotifyPropertyChanged
    {
        Query queryContainer;

        /// <summary>
        /// The find query.
        /// </summary>
        public Query QueryContainer
        {
            get { return queryContainer; }
            set {
                if (value != queryContainer)
                {
                    queryContainer = value;
                    Binding bind = new Binding { Source = QueryContainer, Path = new PropertyPath("Valid"), Mode = BindingMode.TwoWay, UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged };
                    bind.ValidationRules.Add(new QueryValidationRule { ValidatesOnTargetUpdated = true, Query=QueryContainer });
                    BindingOperations.SetBinding(this, RapidFindReplaceControl.IsQueryValidProperty, bind);
                    
                    RaisePropertyChanged("QueryContainer");
                } 
            }
        }

        /// <summary>
        /// RoutedEvent indicating that the search has finished.
        /// </summary>
        public static readonly RoutedEvent FinishedSearchingEvent = EventManager.RegisterRoutedEvent("FinishedSearching", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RapidFindReplaceControl));

        // Provide CLR accessors for the event 
        /// <summary>
        /// RoutedEvent indicating that the search has finished.
        /// </summary>
        public event RoutedEventHandler FinishedSearching
        {
            add { AddHandler(FinishedSearchingEvent, value); }
            remove { RemoveHandler(FinishedSearchingEvent, value); }
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
/*
        /// <summary>
        /// Whether NumberOfHits > 0.
        /// </summary>
        public bool AreHits
        {
            get { return NumberOfHits > 0; }
        }
        */
        RapidFindReplaceControlViewModel viewModel;
        OptionsViewModel findOptions;

        Popup _PART_OptionsPopup;

        /// <summary>
        /// The popup used for the options dropdown
        /// </summary>
        public Popup PART_OptionsPopup { get { return _PART_OptionsPopup; } set { _PART_OptionsPopup = value; } }

        /// <summary>
        /// The view model that performs the find.
        /// </summary>
        public RapidFindReplaceControlViewModel ViewModel
        {
            get {return viewModel; }
            set {
                if (viewModel != null) viewModel.FinishedSearching -= viewModel_FinishedSearching;
                viewModel = value;
                if (viewModel != null)
                {
                    //first time we need to set the FindOptions from the model ourselves
                   // viewModel.FindOptions = FindOptions;
                    //for these properties we want to copy from the view model first.
                    QueryHistory = viewModel.QueryHistory;
                    QueryHistoryCapacity = viewModel.QueryHistoryCapacity;

                    QueryContainer = viewModel.Query;
                    //Binding b = new Binding { Source = this, Path = new PropertyPath("Query"), Mode = BindingMode.TwoWay };
                    //BindingOperations.SetBinding(viewModel, RapidFindReplaceControlViewModel.QueryProperty, b);
                    Binding b = new Binding { Source = this, Path = new PropertyPath("QueryContainer"), Mode = BindingMode.TwoWay };
                    BindingOperations.SetBinding(viewModel, RapidFindReplaceControlViewModel.QueryProperty, b);
                    

                    BindingOperations.SetBinding(viewModel, RapidFindReplaceControlViewModel.QueryHistoryCapacityProperty, new Binding { Source = this, Path = new PropertyPath("QueryHistoryCapacity"), Mode = BindingMode.TwoWay });
                    BindingOperations.SetBinding(viewModel, RapidFindReplaceControlViewModel.QueryHistoryProperty, new Binding { Source = this, Path = new PropertyPath("QueryHistory"), Mode = BindingMode.OneWay });
                    BindingOperations.SetBinding(viewModel, RapidFindReplaceControlViewModel.FindScopeProperty, new Binding { Source = this, Path = new PropertyPath("FindScope"), Mode = BindingMode.TwoWay });
                    BindingOperations.SetBinding(viewModel, RapidFindReplaceControlViewModel.NumberOfHitsProperty, new Binding { Source = this, Path = new PropertyPath("NumberOfHits"), Mode = BindingMode.OneWayToSource });
                    BindingOperations.SetBinding(viewModel, RapidFindReplaceControlViewModel.FindOptionsProperty, new Binding { Source = this, Path = new PropertyPath("FindOptions"), Mode = BindingMode.OneWay });
                    BindingOperations.SetBinding(viewModel, RapidFindReplaceControlViewModel.CurrentMatchProperty, new Binding { Source = this, Path = new PropertyPath("CurrentMatch"), Mode = BindingMode.OneWayToSource });

                    BindingOperations.SetBinding(this, RapidFindReplaceControl.BodyHighlightAdornerBrushProperty, new Binding { Source = viewModel, Path = new PropertyPath("BodyHighlightAdornerBrush"), Mode = BindingMode.OneWayToSource });
                    

                    viewModel.FinishedSearching += viewModel_FinishedSearching;
                }
            }
        }

        void viewModel_FinishedSearching(object sender, EventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(RapidFindReplaceControl.FinishedSearchingEvent));
        }
        

        //default is created inside ViewModel and databound  to this property
        /// <summary>
        /// User options.
        /// </summary>
        public OptionsViewModel FindOptions
        {
            get {
                if (findOptions == null) findOptions = new OptionsViewModel();// OptionsViewModel.CreateDefaultOptions();
                return findOptions; 
            }
            set {
                if (findOptions != value)
                {
                    findOptions = value;
                    RaisePropertyChanged("FindOptions");
                }
            }
        }

        EasingColorKeyFrame easingColorKeyFrameEnd;
        
        #region Attached Properties
        /// <summary>
        /// Attached property used to indicate if an element is findable, or if it should be ignored.
        /// </summary>
        public static readonly DependencyProperty IsFindableProperty = DependencyProperty.RegisterAttached("IsFindable", typeof(bool?), typeof(RapidFindReplaceControl), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Attached property used to indicate if an element is findable, or if it should be ignored.
        /// </summary>
        public static bool? GetIsFindable(UIElement element)
        {
            if (element == null) throw new ArgumentNullException("element");
            return (bool?)element.GetValue(IsFindableProperty);
        }

        /// <summary>
        /// Attached property used to indicate if an element is findable, or if it should be ignored.
        /// </summary>
        public static void SetIsFindable(UIElement element, bool? value)
        {
            if (element == null) throw new ArgumentNullException("element");
            element.SetValue(IsFindableProperty, value);
        }

        /*

        public static Brush GetBodyHighlightAdornerBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(BodyHighlightAdornerBrushProperty);
        }

        public static void SetBodyHighlightAdornerBrush(DependencyObject obj, Brush value)
        {
            obj.SetValue(BodyHighlightAdornerBrushProperty, value);
        }

        // Using a DependencyProperty as the backing store for BodyHighlightAdornerBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BodyHighlightAdornerBrushProperty =
            DependencyProperty.RegisterAttached("BodyHighlightAdornerBrush", typeof(Brush), typeof(RapidFindReplaceControl), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0x66, 0xFD, 0xDB, 0x07))/*, new PropertyChangedCallback(BodyHighlightAdornerBrushChanged)*//*));
        
      /*  private static void BodyHighlightAdornerBrushChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as RapidFindReplaceControl).ViewModel.BodyHighlightAdornerBrush = e.NewValue as Brush;
            string g = "";
            g += "";
        }
        */
        #endregion

        #region Dependency Properties


        /// <summary>
        /// The current match that the user has iterated to using the next/previous buttons.
        /// </summary>
        public readonly static DependencyProperty CurrentMatchProperty = DependencyProperty.Register("CurrentMatch", typeof(Highlight), typeof(RapidFindReplaceControl));

        /// <summary>
        /// The current match that the user has iterated to using the next/previous buttons.
        /// </summary>
        public Highlight CurrentMatch
        {
            get { return (Highlight)GetValue(CurrentMatchProperty); }
        }


        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <summary>
        /// Whether the user's query is valid or not.
        /// </summary>
        /// <remarks>A query can be invalid for example if regular expressions are used but the expression is invalid.</remarks>
        public readonly static DependencyProperty IsQueryValidProperty = DependencyProperty.Register("IsQueryValid", typeof(bool), typeof(RapidFindReplaceControl), new PropertyMetadata(true, new PropertyChangedCallback(IsQueryValidChanged)));

        /// <summary>
        /// Whether the user's query is valid or not.
        /// </summary>
        /// <remarks>A query can be invalid for example if regular expressions are used but the expression is invalid.</remarks>
        public bool IsQueryValid
        {
            get { return (bool)GetValue(IsQueryValidProperty); }
            set { SetValue(IsQueryValidProperty, value); }
        }

        static void IsQueryValidChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            string g = "";
            g += "d";
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        /// <summary>
        /// Body highlight brush to use
        /// </summary>
        public readonly static DependencyProperty BodyHighlightAdornerBrushProperty = DependencyProperty.Register("BodyHighlightAdornerBrush", typeof(Brush), typeof(RapidFindReplaceControl), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0x66, 0xFD, 0xDB, 0x07)),new PropertyChangedCallback(BodyHighlightAdornerBrushChanged)));
        private static void BodyHighlightAdornerBrushChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as RapidFindReplaceControl).ViewModel.BodyHighlightAdornerBrush = e.NewValue as Brush;
            
        }

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
        public readonly static DependencyProperty BodyIterativeHighlightAdornerBrushProperty = DependencyProperty.Register("BodyIterativeHighlightAdornerBrush", typeof(Brush), typeof(RapidFindReplaceControl), new PropertyMetadata(new PropertyChangedCallback(BodyIterativeHighlightAdornerBrushChanged)));
        private static void BodyIterativeHighlightAdornerBrushChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as RapidFindReplaceControl).ViewModel.BodyIterativeHighlightAdornerBrush = e.NewValue as Brush;
        }
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
        public readonly static DependencyProperty BodyHighlightAdornerPenProperty = DependencyProperty.Register("BodyHighlightAdornerPen", typeof(Pen), typeof(RapidFindReplaceControl), new PropertyMetadata(new PropertyChangedCallback(BodyHighlightAdornerPenChanged)));
        private static void BodyHighlightAdornerPenChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as RapidFindReplaceControl).ViewModel.BodyHighlightAdornerPen = e.NewValue as Pen;
        }
        /// <summary>
        /// Pen to use to draw border around body highlights.
        /// </summary>
        public Pen BodyHighlightAdornerPen
        {
            get { return (Pen)GetValue(BodyHighlightAdornerPenProperty); }
            set { SetValue(BodyHighlightAdornerPenProperty, value); }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        /// <summary>
        /// Control and it's children to find within.
        /// </summary>
        public readonly static DependencyProperty FindScopeProperty = DependencyProperty.Register("FindScope", typeof(DependencyObject), typeof(RapidFindReplaceControl), new PropertyMetadata(new PropertyChangedCallback(FindScopeChanged)));
        private static void FindScopeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {


        }

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
        public readonly static DependencyProperty QueryProperty = DependencyProperty.Register("Query", typeof(string), typeof(RapidFindReplaceControl), new PropertyMetadata(new PropertyChangedCallback(QueryChanged)));
        private static void QueryChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            RapidFindReplaceControl box = (sender as RapidFindReplaceControl);
            if (box.Query.Length == 0)
            {
                VisualStateManager.GoToState(box, "NoQuery", true);

            }
            else
            {
                VisualStateManager.GoToState(box, "Query", true);
                if (!box.findOptions.FindAsYouType)
                {
                    box.hitState = HitStates.NoFind;
                    VisualStateManager.GoToState(box, "NoFind", true);
                }
            }

            

            box.QueryContainer.QueryText = (string)e.NewValue;
            if (box.FindOptions.FindAsYouType)//box.AsYouTypeFindEnabled)
            {
                if (box.Query.Length >= box.AsYouTypeFindMinimumCharacters)
                {
//                    box.QueryContainer.QueryText = (string)e.NewValue;
                    box.ViewModel.FindText(box.FindScope/*, box.queryContainer*/);
                    ProcessHits(box.NumberOfHits, box, false);
                    //box.ViewModel.FindTextCommand.Execute(null);
                }
                else
                {
                    box.ViewModel.ResetHighlights();
                    
                    ProcessHits(0, box, false);
                    //box.ProcessResults(0);
                    //box.hitState = HitStates.NoFind;
                    //VisualStateManager.GoToState(box, box.hitState.ToString(), true);
                }
            } 

        }
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
        /// Number of hits found.
        /// </summary>
        public readonly static DependencyProperty NumberOfHitsProperty = DependencyProperty.Register("NumberOfHits", typeof(int), typeof(RapidFindReplaceControl), new PropertyMetadata(new PropertyChangedCallback(NumberOfHitsChanged)));
        static void NumberOfHitsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e){
            RapidFindReplaceControl box = (sender as RapidFindReplaceControl);
            ProcessHits((int)e.NewValue, box, false);
           /* if (
                (((int)e.OldValue) == 0 && ((int)e.NewValue) > 0) ||
                (((int)e.OldValue) > 0 && ((int)e.NewValue) == 0)
                )
                box.RaisePropertyChanged("AreHits");
            */


        }


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
        /// Number of query history items to hold and display.
        /// </summary>
        public readonly static DependencyProperty QueryHistoryCapacityProperty = DependencyProperty.Register("QueryHistoryCapacity", typeof(int), typeof(RapidFindReplaceControl), new PropertyMetadata(5));


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
        public readonly static DependencyProperty AsYouTypeFindMinimumCharactersProperty = DependencyProperty.Register("AsYouTypeFindMinimumCharacters", typeof(int), typeof(RapidFindReplaceControl), new PropertyMetadata(2, new PropertyChangedCallback(AsYouTypeFindMinimumCharactersChanged)));
        private static void AsYouTypeFindMinimumCharactersChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            //(sender as RapidFindReplaceControl).UpdateStates(true);
        }
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
		public readonly static DependencyProperty NoHitsBorderBrushColorProperty = DependencyProperty.Register("NoHitsBorderBrushColor", typeof(Color), typeof(RapidFindReplaceControl), new PropertyMetadata(new PropertyChangedCallback(NoHitsBorderBrushColorChanged)));
        static void NoHitsBorderBrushColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e){
            if((sender as RapidFindReplaceControl).easingColorKeyFrameEnd!=null) (sender as RapidFindReplaceControl).easingColorKeyFrameEnd.Value = (Color)e.NewValue;
        }
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
 		public readonly static DependencyProperty WatermarkTextProperty = DependencyProperty.Register("WatermarkText", typeof(string), typeof(RapidFindReplaceControl)/*, new PropertyMetadata(new PropertyChangedCallback(WatermarkTextChanged))*/);
        /*private static void WatermarkTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }*/
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
        public readonly static DependencyProperty FindButtonIconProperty = DependencyProperty.Register("FindButtonIcon", typeof(Brush), typeof(RapidFindReplaceControl), new PropertyMetadata(new PropertyChangedCallback(FindButtonIconChanged)));
        private static void FindButtonIconChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {

        }
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
        public readonly static DependencyProperty OptionsButtonIconProperty = DependencyProperty.Register("OptionsButtonIcon", typeof(Brush), typeof(RapidFindReplaceControl), new PropertyMetadata(new PropertyChangedCallback(OptionsButtonIconChanged)));
        private static void OptionsButtonIconChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {

        }
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
        public readonly static DependencyProperty OptionsButtonCheckedIconProperty = DependencyProperty.Register("OptionsButtonCheckedIcon", typeof(Brush), typeof(RapidFindReplaceControl));
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
        public readonly static DependencyProperty IsReplaceOpenProperty = DependencyProperty.Register("IsReplaceOpen", typeof(bool), typeof(RapidFindReplaceControl), new PropertyMetadata(new PropertyChangedCallback(IsReplaceOpenChanged)));


        private static void IsReplaceOpenChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
           // (sender as RapidFindReplaceControl).
        }
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
        public readonly static DependencyProperty IsOptionsDropDownOpenProperty = DependencyProperty.Register("IsOptionsDropDownOpen", typeof(bool), typeof(RapidFindReplaceControl));
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
        public readonly static DependencyProperty QueryHistoryProperty = DependencyProperty.Register("QueryHistory", typeof(ObservableBackwardsRingBuffer<string>), typeof(RapidFindReplaceControl));
        /// <summary>
        /// The history of queries entered by the user.
        /// </summary>
        public ObservableBackwardsRingBuffer<string> QueryHistory
        {
            get { return (ObservableBackwardsRingBuffer<string>)GetValue(QueryHistoryProperty); }
            set { SetValue(QueryHistoryProperty, value); }
        }
        
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        /// <summary>
        /// Find options that are available to the user.
        /// </summary>
        public IEnumerable<OptionProperty> AvailableFindOptions
        {
            get
            {//we really want to pass DependencyProperty in to optionProperty, so can bind property to optionproperty and have that bound in xaml
                //OR just bind from xaml direct to DP instance in OptionProperty!!
                
            /*    PropertyInfo[] props = FindOptions.GetType().GetProperties();
                return from property in props
                       where property.GetCustomAttributes(typeof(OptionPropertyAttribute), true).Length > 0
                       select new OptionProperty { OptionName = property.Name, OptionType = property.PropertyType, OptionProperty = property, FindOptions=FindOptions };
                */
                //return FindOptions.GetType().GetFields(BindingFlags.Static | BindingFlags.Public).Where(p => p.FieldType.Equals(typeof(DependencyProperty))).Select(;
                //DependencyProperty.

                return from property in GetAttachedProperties(FindOptions) //FindOptions.GetType().GetFields(BindingFlags.Static | BindingFlags.Public)
                       where property.GetMetadata(FindOptions) is OptionPropertyMetaData && (property.GetMetadata(FindOptions) as OptionPropertyMetaData).VisibleToUser
                       select new OptionProperty { OptionName = property.Name, OptionType = property.PropertyType, OptionDependencyProperty = property, FindOptions = FindOptions, Description = (property.GetMetadata(FindOptions) as OptionPropertyMetaData).Description };

                
            }
        }




        IList<DependencyProperty> GetAttachedProperties(DependencyObject obj)
        {
            List<DependencyProperty> result = new List<DependencyProperty>();

            foreach (System.ComponentModel.PropertyDescriptor pd in System.ComponentModel.TypeDescriptor.GetProperties(obj,
                new Attribute[] { new System.ComponentModel.PropertyFilterAttribute(System.ComponentModel.PropertyFilterOptions.All) }))
            {
                System.ComponentModel.DependencyPropertyDescriptor dpd = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(pd);
                if (dpd != null)
                    result.Add(dpd.DependencyProperty);
            }

            return result;
        }

        #endregion


        

        private static void ProcessHits(int numberOfHits, RapidFindReplaceControl box, bool fromFindButton)
        {
            if (box.Query != null && (box.Query.Length >= box.AsYouTypeFindMinimumCharacters || fromFindButton))
            {
                if(box.FindOptions.FindAsYouType)   //-------------Find as you type
                    box.hitState = numberOfHits > 0 ? HitStates.Hits : HitStates.NoHits;
                else if (numberOfHits>0)            //-------------No find as you type, but hits anyway
                    box.hitState = HitStates.Hits;
                else if (fromFindButton)            //-------------No find as you type, but button and no hits
                    box.hitState = HitStates.NoHits;
                else                                //-------------No find as you type and not from button
                    box.hitState = HitStates.NoFind;

            }
            else box.hitState = HitStates.NoFind;
            VisualStateManager.GoToState(box, box.hitState.ToString(), true);
        }
        #region Template elements

        TextBox replaceTextBox;
        /// <summary>
        /// The replace textbox.
        /// </summary>
        public TextBox PART_ReplaceTextBox
        {
            get { return replaceTextBox; }
            private set
            {
                
                replaceTextBox = value;
                
            }
        }


        TextBox findTextBox;
        /// <summary>
        /// The find textbox.
        /// </summary>
        public TextBox PART_FindTextBox
        {
            get { return findTextBox; }
            private set
            {
                if (findTextBox != null)
                {
                    findTextBox.GotFocus -= findTextBox_GotFocus;
                    findTextBox.LostFocus -= findTextBox_LostFocus;
                    findTextBox.MouseEnter -= findTextBox_MouseEnter;
                    findTextBox.MouseLeave -= findTextBox_MouseLeave;
                    findTextBox.KeyUp -= findTextBox_KeyUp;
                }
                findTextBox = value;
                if (findTextBox != null)
                {
                    findTextBox.GotFocus += findTextBox_GotFocus;
                    findTextBox.LostFocus += findTextBox_LostFocus;
                    findTextBox.MouseEnter += findTextBox_MouseEnter;
                    findTextBox.MouseLeave += findTextBox_MouseLeave;
                    findTextBox.KeyUp += findTextBox_KeyUp;
                }
            }
        }

        void findTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.SelectNextMatchCommand.Execute(null);
            }
        }

        void findTextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "MouseOut", true);
        }

        void findTextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "MouseOver", true);
        }

        void findTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Query == null  || Query.Length == 0) VisualStateManager.GoToState(this, "Blur_NoQuery", true);
            else
            {
                VisualStateManager.GoToState(this, "Blur_Query", true);

                ViewModel.AddQueryToHistory(Query);//TODO make query history use Query obj
            }
        }

        
        void findTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Focus", true);
        }

       

       

        Button findButton;
        ToggleButton optionsButton;
        /// <summary>
        /// The find button.
        /// </summary>
        public Button PART_FindButton
        {
            get { return findButton; }
            private set
            {
                if (findButton != null) findButton.Click -= findButton_Click;
                findButton = value;
                if (findButton != null) findButton.Click += findButton_Click;
            }
        }

        Button previousMatchButton;
        /// <summary>
        /// The previous match button.
        /// </summary>
        public Button PART_PreviousMatchButton
        {
            get { return previousMatchButton; }
            private set
            {
                if (previousMatchButton != null) previousMatchButton.Click -= previousMatchButton_Click;
                previousMatchButton = value;
                if (previousMatchButton != null) previousMatchButton.Click += previousMatchButton_Click;
            }
        }


        Button nextMatchButton;
        /// <summary>
        /// The next match button.
        /// </summary>
        public Button PART_NextMatchButton
        {
            get { return nextMatchButton; }
            private set
            {
                if (nextMatchButton != null) nextMatchButton.Click -= nextMatchButton_Click;
                nextMatchButton = value;
                if (nextMatchButton != null) nextMatchButton.Click += nextMatchButton_Click;
            }
        }

        
        Button replaceButton;
        /// <summary>
        /// The replace button.
        /// </summary>
        public Button PART_ReplaceButton
        {
            get { return replaceButton; }
            private set
            {
                if (replaceButton != null) replaceButton.Click -= replaceButton_Click;
                replaceButton = value;
                if (replaceButton != null) replaceButton.Click += replaceButton_Click;
            }
        }

        Button replaceAllButton;
        /// <summary>
        /// The replace all button.
        /// </summary>
        public Button PART_ReplaceAllButton
        {
            get { return replaceAllButton; }
            private set
            {
                if (replaceAllButton != null) replaceAllButton.Click -= replaceAllButton_Click;
                replaceAllButton = value;
                if (replaceAllButton != null) replaceAllButton.Click += replaceAllButton_Click;
            }
        }

        void replaceButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ReplaceMatchCommand.Execute(PART_ReplaceTextBox.Text);
        }

        void replaceAllButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ReplaceAllMatchesCommand.Execute(PART_ReplaceTextBox.Text);
        }

       
        /// <summary>
        /// The options dropdown button.
        /// </summary>
        public ToggleButton PART_OptionsButton
        {
            get { return optionsButton; }
            private set
            {
                if (optionsButton != null) optionsButton.Click -= optionsButton_Click;
                optionsButton = value;
                if (optionsButton != null) optionsButton.Click += optionsButton_Click;
            }
        }

        ListView optionsList;
        /// <summary>
        /// The options ListView.
        /// </summary>
        public ListView PART_OptionsList
        {
            get { return optionsList; }
            private set
            {
                if (optionsList != null) optionsList.PreviewMouseDown -= optionsList_PreviewMouseDown;
                optionsList = value;
                if (optionsList != null) optionsList.PreviewMouseDown += optionsList_PreviewMouseDown;
            }
        }

        ListView historyList;
        /// <summary>
        /// The history ListView.
        /// </summary>
        public ListView PART_HistoryList
        {
            get { return historyList; }
            private set
            {
                if (historyList != null) historyList.SelectionChanged -= historyList_SelectionChanged;
                historyList = value;
                if (historyList != null) historyList.SelectionChanged += historyList_SelectionChanged;
            }
        }

        
        enum HitStates
        {
            NoFind,
            Hits,
            NoHits
        }

        HitStates hitState = HitStates.NoFind;

        void findButton_Click(object sender, RoutedEventArgs e)
        {
            if (hitState != HitStates.NoFind)
            {
                Query = "";
                ViewModel.ResetHighlights();
                //ProcessResults(0);

                hitState = HitStates.NoFind;
                VisualStateManager.GoToState(this, hitState.ToString(), true);

                PART_FindTextBox.Focus();
            }
            else
            {
                ViewModel.FindTextCommand.Execute(null);
                //ViewModel.FindText(FindScope, Query);
                ProcessHits(NumberOfHits, this, true);
            }
        }

        void previousMatchButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectPreviousMatchCommand.Execute(null);
        }

        void nextMatchButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectNextMatchCommand.Execute(null);
        }

       

        void optionsButton_Click(object sender, RoutedEventArgs e)
        {
            
        }


        void optionsList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

            var item = ItemsControl.ContainerFromElement(sender as ListView, e.OriginalSource as DependencyObject) as ListViewItem;
            if (item != null)
            {
                (item.Content as OptionProperty).Value = !(bool)(item.Content as OptionProperty).Value;
            }
/*            if (e.AddedItems.Count > 0)
            {
                (e.AddedItems[0] as OptionProperty).Value = !((bool)(e.AddedItems[0] as OptionProperty).Value);
            }*/
        }

        void historyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        #endregion

        /// <summary>
        /// Called when the control template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
           
            PART_FindTextBox = GetTemplateChild("PART_FindTextBox") as TextBox;
            PART_ReplaceTextBox = GetTemplateChild("PART_ReplaceTextBox") as TextBox;
            PART_FindButton = GetTemplateChild("PART_FindButton") as Button;
            PART_PreviousMatchButton = GetTemplateChild("PART_PreviousMatchButton") as Button;
            PART_ReplaceButton = GetTemplateChild("PART_ReplaceButton") as Button;
            PART_ReplaceAllButton = GetTemplateChild("PART_ReplaceAllButton") as Button;
            PART_NextMatchButton = GetTemplateChild("PART_NextMatchButton") as Button;
            PART_OptionsButton = GetTemplateChild("PART_OptionsButton") as ToggleButton;
            PART_OptionsList = GetTemplateChild("PART_OptionsList") as ListView;
            PART_HistoryList = GetTemplateChild("PART_HistoryList") as ListView;
            PART_OptionsPopup = GetTemplateChild("PART_OptionsPopup") as Popup;

            if(!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))//Blend uses a StandinPopup class so PART_OptionsPopup will be null
                PART_OptionsPopup.CustomPopupPlacementCallback += (Size popupSize, Size targetSize, Point offset) =>
                            new[] { new CustomPopupPlacement() { Point = new Point(targetSize.Width - popupSize.Width, targetSize.Height) } };

            easingColorKeyFrameEnd = GetTemplateChild("PART_EasingColorKeyFrameEnd") as EasingColorKeyFrame;
            if(easingColorKeyFrameEnd!=null) easingColorKeyFrameEnd.Value = NoHitsBorderBrushColor;
            //UpdateStates(false);
            VisualStateManager.GoToState(this, "NoQuery", false);
            VisualStateManager.GoToState(this, "Blur_NoQuery", true);



                    

            base.OnApplyTemplate();
        }


        static RapidFindReplaceControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RapidFindReplaceControl), new FrameworkPropertyMetadata(typeof(RapidFindReplaceControl)));
        }

        /// <summary>
        /// New.
        /// </summary>
        public RapidFindReplaceControl()
        {
            

            queryContainer = new Query("");
           // this.Resources.MergedDictionaries.Add(SharedDictionaryManager.SharedDictionary);
            //AsYouTypeFindMinimumCharacters = 2;
            //AsYouTypeFindEnabled = true;


            ViewModel = new RapidFindReplaceControlViewModel();//must use setter as it does a binding


        }

   

        object GetResource(string resourceName)
        {
            ComponentResourceKey key = new ComponentResourceKey(typeof(RapidFindReplaceControl), resourceName);
            return TryFindResource(key);
        }




        internal void ClearHighlights()
        {
            ViewModel.ResetHighlights();
        }
    }

    internal static class SharedDictionaryManager
    {
        internal static ResourceDictionary SharedDictionary
        {
            get
            {
                if (_sharedDictionary == null)
                {
                    System.Uri resourceLocater =
                        new System.Uri("/WPFFindControl;component/Resources/Dictionary1.xaml",
                                        System.UriKind.Relative);

                    _sharedDictionary =
                        (ResourceDictionary)Application.LoadComponent(resourceLocater);
                }

                return _sharedDictionary;
            }
        }

        private static ResourceDictionary _sharedDictionary;
    }
}
