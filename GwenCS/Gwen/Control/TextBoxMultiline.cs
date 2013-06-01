using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Gwen.Input;

namespace Gwen.Control
{
    public class TextBoxMultiline : TextBox
    {
        private readonly ScrollControl m_ScrollControl;

        public bool AcceptTab;

        public int CurrentLine
        {
            get
            {
                string sub = Text.Substring(0, m_CursorPos);
                return sub.Length - sub.Replace("\n", "").Length;
            }
        }
        public int TotalLines
        {
            get
            {
                return Text.Length - Text.Replace("\n", "").Length + 1;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="TextBox"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public TextBoxMultiline(Base parent)
            : base(parent)
        {
            AutoSizeToContents = false;
            AcceptTab = false;
            SetSize(200, 20);

            Alignment = Pos.Left | Pos.Top;
            TextPadding = new Padding(4, 2, 4, 2);

            IsTabable = true;

            m_ScrollControl = new ScrollControl(this);
            m_ScrollControl.Dock = Pos.Fill;
            m_ScrollControl.EnableScroll(false, true);
            m_ScrollControl.AutoHideBars = true;
            m_ScrollControl.Margin = Margin.One;

            m_InnerPanel = m_ScrollControl;

            m_Text.Parent = m_InnerPanel;

            m_ScrollControl.InnerPanel.BoundsChanged += new GwenEventHandler(ScrollChanged);

            m_ScrollControl.SetInnerSize(1000, 1000); // todo: why such arbitrary numbers?
        }

        void ScrollChanged(Base control)
        {
            RefreshCursorBounds();
        }

        /// <summary>
        /// Handler invoked when control children's bounds change.
        /// </summary>
        /// <param name="oldChildBounds"></param>
        /// <param name="child"></param>
        protected override void OnChildBoundsChanged(System.Drawing.Rectangle oldChildBounds, Base child)
        {
            if (m_ScrollControl != null)
            {
                m_ScrollControl.UpdateScrollBars();
            }
        }

        /// <summary>
        /// Handler for character input event.
        /// </summary>
        /// <param name="chr">Character typed.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnChar(char chr)
        {
            //base.OnChar(chr);
            if (chr == '\t' && !AcceptTab) return false;

            InsertText(chr.ToString());
            return true;
        }

        protected override bool OnKeyTab(bool down)
        {
            if (!AcceptTab) return base.OnKeyTab(down);
            if (!down) return false;

            OnChar(' ');
            OnChar(' ');
            OnChar(' ');
            OnChar(' ');
            return true;
        }

        protected override void RefreshCursorBounds()
        {
            m_LastInputTime = Platform.Neutral.GetTimeInSeconds();

            MakeCaretVisible();

            Point pA = GetCharacterPosition(m_CursorPos, true);
            Point pB = GetCharacterPosition(m_CursorEnd, true);

            m_SelectionBounds.X = Math.Min(pA.X, pB.X);
            m_SelectionBounds.Y = TextY - 1;
            m_SelectionBounds.Width = Math.Max(pA.X, pB.X) - m_SelectionBounds.X;
            m_SelectionBounds.Height = TextHeight + 2;

            m_CaretBounds.X = pA.X;
            m_CaretBounds.Y = (pA.Y + 1);

            float vlinesep = 5.2f;
            int newlines = Text.Substring(0, m_CursorPos).Length - Text.Substring(0, m_CursorPos).Replace("\n", "").Length;
            m_CaretBounds.Y += (int)(newlines * vlinesep);
            m_CaretBounds.Y += m_ScrollControl.VerticalScroll;

            m_CaretBounds.Width = 1;
            m_CaretBounds.Height = Font.Size + 2;

            Redraw();
        }

        /// <summary>
        /// Handler for Return keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeyReturn(bool down)
        {
            base.OnKeyReturn(down);
            if (down) return true;

            OnReturn();

            OnChar('\n');
            if (CurrentLine == TotalLines - 1)
            {
                m_ScrollControl.ScrollToBottom();
            }

            return true;
        }

        protected override bool OnKeyDown(bool down)
        {
            base.OnKeyDown(down);
            if (!down) return true;

            int clinebegin = -1;
            if (CursorPos > 0)
            {
                clinebegin = Text.LastIndexOf('\n', CursorPos - 1, CursorPos);
            }
            int nextlinebegin = Text.IndexOf('\n', CursorPos);
            if (nextlinebegin == -1)
            {
                CursorPos = Text.Length;
            }
            else
            {
                int diff = CursorPos - clinebegin;
                int next_nextlinebegin = Text.IndexOf('\n', nextlinebegin + 1);
                if (next_nextlinebegin == -1) next_nextlinebegin = Text.Length;
                if (diff <= next_nextlinebegin - nextlinebegin)
                {
                    CursorPos = nextlinebegin + diff;
                }
                else
                {
                    CursorPos = next_nextlinebegin;
                }
            }

            if (!Input.InputHandler.IsShiftDown)
            {
                m_CursorEnd = m_CursorPos;
            }

            return true;
        }

        protected override bool OnKeyUp(bool down)
        {
            base.OnKeyUp(down);
            if (!down) return true;

            if (CursorPos != 0)
            {
                int clinebegin = Text.LastIndexOf('\n', CursorPos - 1, CursorPos);
                if (clinebegin == -1)
                {
                    CursorPos = 0;
                }
                else
                {
                    int val = clinebegin - 1;
                    if (val == -1)
                    {
                        val = 0;
                    }
                    int prevlinebegin = Text.LastIndexOf('\n', val);
                    int diff = CursorPos - clinebegin;
                    if (diff < clinebegin - prevlinebegin)
                    {
                        CursorPos = prevlinebegin + diff;
                    }
                    else
                    {
                        CursorPos = clinebegin;
                    }
                }
            }

            if (!Input.InputHandler.IsShiftDown)
            {
                m_CursorEnd = m_CursorPos;
            }

            //m_ScrollControl

            return true;
        }
    }
}
