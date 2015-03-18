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
using System.Windows.Controls;
using System.Windows.Documents;

namespace Keyoti.RapidFindReplace.WPF.FindHandlers.HighlightHandlers
{
    /// <summary>
    /// Hit Highlight for FlowDocumentScrollViewer
    /// </summary>
    public class FlowDocumentScrollViewerHighlight : Highlight
    {
        /// <summary>
        /// New instance.
        /// </summary>
        /// <param name="start">Where the highlight starts in <c>run</c></param>
        /// <param name="end">Where the highlight ends in <c>run</c></param>
        /// <param name="adorner">Adorner instance that will actually highlight</param>
        /// <param name="run">The Run where the highlight is</param>
        public FlowDocumentScrollViewerHighlight(int start, int end, HighlightAdorner adorner, System.Windows.Documents.Run run)
            : base(start, end, adorner, run)
        {
        }

        /// <summary>
        /// Selects the highlight as the current highlight when iterating through hits, bringing the control to focus.
        /// </summary>
        public override void Select()
        {
            System.Windows.Controls.FlowDocumentScrollViewer fdpv = (BodyAdorner.AdornedElement as System.Windows.Controls.FlowDocumentScrollViewer);
            fdpv.Selection.Select(run.ContentStart.GetPositionAtOffset(Start), run.ContentStart.GetPositionAtOffset(End));
            ScrollViewer scr = Utility.FindScrollViewer(fdpv);
            scr.ScrollToVerticalOffset(fdpv.Selection.Start.GetCharacterRect(LogicalDirection.Forward).Top);
            fdpv.Focus();//focusing is very important because not only is it correct user behaviour but it also allows us to track the user moving focus to other controls better
            base.Select();
        }

        /// <summary>
        /// Deselects the highlight.
        /// </summary>
        public override void Deselect()
        {
            System.Windows.Controls.FlowDocumentScrollViewer fdpv = (BodyAdorner.AdornedElement as System.Windows.Controls.FlowDocumentScrollViewer);
            fdpv.Selection.Select(fdpv.Selection.Start, fdpv.Selection.Start);
            base.Deselect();
        }
    }


    /// <summary>
    /// Hit Highlight for TextBox
    /// </summary>
    public class TextBoxHighlight : Highlight
    {
        /// <summary>
        /// New instance.
        /// </summary>
        /// <param name="start">Where the highlight starts in <c>run</c></param>
        /// <param name="end">Where the highlight ends in <c>run</c></param>
        /// <param name="adorner">Adorner instance that will actually highlight</param>
        /// <param name="run">The Run where the highlight is</param>
        public TextBoxHighlight(int start, int end, HighlightAdorner adorner, System.Windows.Documents.Run run)
            : base(start, end, adorner, run)
        {
        }
        /// <summary>
        /// Selects the highlight as the current highlight when iterating through hits, bringing the control to focus.
        /// </summary>
        public override void Select()
        {
            System.Windows.Controls.TextBox tb = (BodyAdorner.AdornedElement as System.Windows.Controls.TextBox);            
            tb.Focus();//focusing is very important because not only is it correct user behaviour but it also allows us to track the user moving focus to other controls better
            tb.Select(Start, End - Start);
            base.Select();
        }

        /// <summary>
        /// Replaces the text of the hit.
        /// </summary>
        /// <param name="replacement">The replacement string.</param>
        public override void ReplaceText(string replacement)
        {
            //we need to call here instead of in base because that will update the Run (which is not actually linked to the TextBox) and 
            //we don't want to update other highlight positions as the base does because we have doUpdates which monitors changes in text for
            //us anyway
            run.Text = run.Text.Substring(0, Start) + replacement + run.Text.Substring(End);
            
            System.Windows.Controls.TextBox tb = (BodyAdorner.AdornedElement as System.Windows.Controls.TextBox);
            tb.Text = tb.Text.Substring(0, Start) + replacement + tb.Text.Substring(End);

        }



        /// <summary>
        /// Whether ReplaceText can be called or not.  When false this is because the control where the highlight exist is readonly or disabled.
        /// </summary>
        public override bool IsReplaceable
        {
            get
            {
                System.Windows.Controls.TextBox tb = (BodyAdorner.AdornedElement as System.Windows.Controls.TextBox);
                return tb.IsEnabled && !tb.IsReadOnly;
            }
        }
    }

    /// <summary>
    /// Hit Highlight for RichTextBox
    /// </summary>
    public class RichTextBoxHighlight : Highlight
    {
        /// <summary>
        /// New instance.
        /// </summary>
        /// <param name="start">Where the highlight starts in <c>run</c></param>
        /// <param name="end">Where the highlight ends in <c>run</c></param>
        /// <param name="adorner">Adorner instance that will actually highlight</param>
        /// <param name="run">The Run where the highlight is</param>
        /// <param name="runStartIndex">The character index of the run in the RichTextBox's Document.</param>
        public RichTextBoxHighlight(int start, int end, HighlightAdorner adorner, System.Windows.Documents.Run run, int runStartIndex) : base(start, end, adorner, run)
        {
            this.runStartIndex = runStartIndex;
            
        }

        /// <summary>
        /// Move the highlight to a new Run.
        /// </summary>
        /// <param name="run">The run the highlight will go in</param>
        /// <param name="start">The position of the highlight relative to the Run</param>
        /// <param name="end">The end position of the highlight relative to the Run</param>
        /// <param name="absStart">The start position of the Run relative to the plain text of the document.</param>
        public void MoveTo(Run run, int start, int end, int absStart)
        {
            this.runStartIndex = absStart;
            MoveTo(run, start, end);
        }

        /// <summary>
        /// Selects the highlight as the current highlight when iterating through hits, bringing the control to focus.
        /// </summary>
        public override void Select()
        {
            System.Windows.Controls.RichTextBox rtb = (BodyAdorner.AdornedElement as System.Windows.Controls.RichTextBox);
            rtb.Focus();//focusing is very important because not only is it correct user behaviour but it also allows us to track the user moving focus to other controls better
            rtb.Selection.Select(run.ContentStart.GetPositionAtOffset(Start), run.ContentStart.GetPositionAtOffset(End));
            
            base.Select();
        }

        /// <summary>
        /// Deselects the highlight.
        /// </summary>
        public override void Deselect()
        {
            System.Windows.Controls.RichTextBox rtb = (BodyAdorner.AdornedElement as System.Windows.Controls.RichTextBox);
            rtb.Selection.Select(rtb.Selection.Start, rtb.Selection.Start);
            base.Deselect();
        }

        /// <summary>
        /// Whether this Highlight is before or at the same position of <c>highlight</c>.
        /// </summary>
        internal protected override bool IsBeforeOrAt(Highlight highlight)
        {
            if (highlight is RichTextBoxHighlight)
                return AbsoluteStart <= (highlight as RichTextBoxHighlight).AbsoluteStart;//works because highlights are sorted
            else throw new ApplicationException("Cannot compare this RichTextBoxHighlight against a highlight from a different control");

        }
        /// <summary>
        /// Replaces the text of the hit.
        /// </summary>
        /// <param name="replacement">The replacement string.</param>
        public override void ReplaceText(string replacement)
        {
            //we need to call here instead of in base because that will update the Run (which is not actually linked to the TextBox) and 
            //we don't want to update other highlight positions as the base does because we have doUpdates which monitors changes in text for
            //us anyway
            run.Text = run.Text.Substring(0, Start) + replacement + run.Text.Substring(End);
        }

        /// <summary>
        /// Whether ReplaceText can be called or not.  When false this is because the control where the highlight exist is readonly or disabled.
        /// </summary>
        public override bool IsReplaceable
        {
            get {
                System.Windows.Controls.RichTextBox rtb = (BodyAdorner.AdornedElement as System.Windows.Controls.RichTextBox);
                return rtb.IsEnabled && !rtb.IsReadOnly; 
            }
        }

    }

    class RichTextBoxHighlightComparer : IComparer<Highlight>
    {

        public int Compare(Highlight u1h, Highlight u2h)
        {
            RichTextBoxHighlight u1 = u1h as RichTextBoxHighlight;
            RichTextBoxHighlight u2 = u2h as RichTextBoxHighlight;

            if (u1.AbsoluteStart < u2.AbsoluteStart) return -1;
            if (u1.AbsoluteStart > u2.AbsoluteStart) return 1;
            if (u1.AbsoluteStart == u2.AbsoluteStart && u1.AbsoluteEnd == u2.AbsoluteEnd) return 0;
            if (u1.AbsoluteStart == u2.AbsoluteStart && u1.AbsoluteEnd < u2.AbsoluteEnd) return -1;
            return 1;
        }

    }
    class RunTextShiftEventArgs
    {
        public int CharacterPosition { get; internal set; }
        public int Delta { get; internal set; }
        public RunTextShiftEventArgs(int charPos, int delta)
        {
            this.CharacterPosition = charPos;
            this.Delta = delta;
        }
    }

    /// <summary>
    /// Hit Highlight base class.
    /// </summary>
    public class Highlight {
        /// <summary>
        /// The start index of the Highlight, as measured from the start of the Document.
        /// </summary>
        public int AbsoluteStart { get { return Start + runStartIndex; } }

        /// <summary>
        /// The end index of the Highlight, as measured from the start of the Document.
        /// </summary>
        public int AbsoluteEnd { get { return End + runStartIndex; } }

        /// <summary>
        /// The absolute start of the Run where the Highlight is.
        /// </summary>
        protected int runStartIndex;
        
        internal delegate void RunTextShiftEventHandler(object sender, RunTextShiftEventArgs e);
        internal event RunTextShiftEventHandler RunTextShift;

        private ScrollBarHighlightAdorner scrollBarAdorner;

        internal ScrollBarHighlightAdorner ScrollBarAdorner
        {
            get { return scrollBarAdorner; }
            set { scrollBarAdorner = value; }
        }
        int s, e;
        /// <summary>
        /// Move highlight to a different part of the current run.
        /// </summary>
        public void MoveTo(int start, int end)
        {
            if(BodyAdorner!=null)
                BodyAdorner.BumpUnderlineTo(run, start, end);
            s = start;
            e = end;
        }

        /// <summary>
        /// Move the highlight to a new run.
        /// </summary>
        public void MoveTo(Run run, int start, int end)
        {
            this.run = run;
            MoveTo(start, end);
        }

        /// <summary>
        /// The UIElement that this highlight is inside
        /// </summary>
        public System.Windows.UIElement HighlightedElement
        {
            get
            {
                return adorner.AdornedElement;
            }
        }

        bool hidden = false;
        HighlightAdorner adorner;

        /// <summary>
        /// The Run the Highlight is in.
        /// </summary>
        protected System.Windows.Documents.Run run; 
        /// <summary>
        /// The adorner
        /// </summary>
        internal HighlightAdorner BodyAdorner { get { return adorner; } }
        /// <summary>
        /// The Run where the highlight occurs.
        /// </summary>
        public System.Windows.Documents.Run Run { get { return run; } }

        /// <summary>
        /// The highlighted text.
        /// </summary>
        public string Text { get { return run.Text.Substring(Start, End - Start); } }
        /// <summary>
        /// Whether the underline is hidden
        /// </summary>
        public bool Hidden
        {
            get { return hidden; }
            set { hidden = value; }
        }


        /// <summary>
        /// New instance, with Run relative to zero index (ie no other Runs in container).
        /// </summary>
        public Highlight(int start, int end, HighlightAdorner adorner, System.Windows.Documents.Run run) : this(start, end, adorner, run, 0)
        {
        }

        /// <summary>
        /// New instance
        /// </summary>
        public Highlight(int start, int end, HighlightAdorner adorner, System.Windows.Documents.Run run, int runStartIndex)
        {
            this.adorner = adorner;
            this.s = start;
            this.e = end;
            this.run = run;
            this.runStartIndex = runStartIndex;
        }

        /// <summary>
        /// Start
        /// </summary>
        public int Start { get { return s; } set { s = value; } }

        /// <summary>
        /// End
        /// </summary>
        public int End { get { return e; } set { e = value; } }


/*
        internal bool IsSameRun(System.Windows.Documents.Run run)
        {
            if(run.
        }*/
        /// <summary>
        /// Selects the highlight as the current highlight when iterating through hits, bringing the control to focus.
        /// </summary>
        public virtual void Select()
        {            
            if(BodyAdorner!=null)BodyAdorner.IterativeHighlight();            
        }
        /// <summary>
        /// Deselects the highlight.
        /// </summary>
        public virtual void Deselect()
        {
            if (BodyAdorner != null) BodyAdorner.NormalHighlight();
        }

        /// <summary>
        /// Default implementation of ReplaceText, changes the hit instance in the Run to the <c>replacement</c>
        /// </summary>
        /// <param name="replacement"></param>
        public virtual void ReplaceText(string replacement)
        {
            run.Text = run.Text.Substring(0, Start) + replacement + run.Text.Substring(End);
            if (replacement.Length != End - Start && RunTextShift!=null)
            {
                RunTextShift(this, new RunTextShiftEventArgs(Start, replacement.Length - (End - Start)));
            }
        }

        /// <summary>
        /// Whether ReplaceText can be called or not.  When false this is because the control where the highlight exist is readonly or disabled.
        /// </summary>
        public virtual bool IsReplaceable
        {
            get { return false; }
        }

        /// <summary>
        /// Whether this Highlight is before or at the same position of <c>highlight</c>.
        /// </summary>

        internal protected virtual bool IsBeforeOrAt(Highlight highlight)
        {
            return AbsoluteStart <= highlight.AbsoluteStart;//works because highlights are sorted
        }


    }

    class HighlightComparer : IComparer<Highlight>
    {
        
        public int Compare(Highlight u1, Highlight u2)
        {
            if (u1.AbsoluteStart < u2.AbsoluteStart) return -1;
            if (u1.AbsoluteStart > u2.AbsoluteStart) return 1;
            if (u1.AbsoluteStart == u2.AbsoluteStart && u1.AbsoluteEnd == u2.AbsoluteEnd) return 0;
            if (u1.AbsoluteStart == u2.AbsoluteStart && u1.AbsoluteEnd < u2.AbsoluteEnd) return -1;
            return 1;
        }

    }

}
