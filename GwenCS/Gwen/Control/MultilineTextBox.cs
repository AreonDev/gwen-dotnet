using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Gwen.Input;

namespace Gwen.Control
{
    public class MultilineTextBox : TextBox
    {
		private readonly ScrollControl m_ScrollControl;
		private Point m_CursorStart;
		private Point m_CursorEnd;

		/// <summary>
		/// Indicates whether the control will accept Tab characters as input.
		/// </summary>
        public bool AcceptTab;

        public int CurrentLine
        {
            get
            {
				return m_CursorStart.Y;
            }
        }

        public int TotalLines
        {
            get
            {
				return TextLines.Count;
            }
        }

		public override string Text {
			get {
				string ret = "";
				for (int i = 0; i < TotalLines; i++){
					ret += TextLines[i];
					if (i != TotalLines - 1) {
						ret += Environment.NewLine;
					}
				}
				return ret;
			}
			set {
				base.Text = value;
			}
		}

		public override bool HasSelection {
			get {
				return m_CursorStart != m_CursorEnd;
			}
		}

		List<string> TextLines = new List<string>();


        /// <summary>
        /// Initializes a new instance of the <see cref="TextBox"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public MultilineTextBox(Base parent) : base(parent)
        {
            AutoSizeToContents = false;
            AcceptTab = true;
            SetSize(200, 20);

            Alignment = Pos.Left | Pos.Top;
            TextPadding = new Padding(4, 2, 4, 2);

            IsTabable = false;

            m_ScrollControl = new ScrollControl(this);
            m_ScrollControl.Dock = Pos.Fill;
            m_ScrollControl.EnableScroll(true, true);
            m_ScrollControl.AutoHideBars = true;
            m_ScrollControl.Margin = Margin.One;

            m_InnerPanel = m_ScrollControl;

            m_Text.Parent = m_InnerPanel;

            m_ScrollControl.InnerPanel.BoundsChanged += new GwenEventHandler(ScrollChanged);

			//Todo halfofastaple: Figure out where these numbers come from. See if we can remove the magic numbers.
			//	This should be as simple as 'm_ScrollControl.AutoSizeToContents = true' or 'm_ScrollControl.NoBounds()'
            m_ScrollControl.SetInnerSize(1000, 1000);

			m_CursorStart = new Point(0, 0);
			m_CursorEnd = new Point(0, 0);

			TextLines.Add(String.Empty);
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

		/// <summary>
		/// Inserts text at current cursor position, erasing selection if any.
		/// </summary>
		/// <param name="text">Text to insert.</param>
		protected override void InsertText(string text) {
			// TODO: Make sure fits (implement maxlength)

			if (HasSelection) {
				EraseSelection();
			}

			string str = TextLines[m_CursorStart.Y];
			str = str.Insert(m_CursorStart.X, text);
			TextLines[m_CursorStart.Y] = str;

			m_CursorStart.X += text.Length;
			m_CursorEnd = m_CursorStart;

			RefreshCursorBounds();
			Invalidate();
		}

        protected override bool OnKeyTab(bool down)
        {
            if (!AcceptTab) return base.OnKeyTab(down);
            if (!down) return false;

            OnChar('\t');
            return true;
        }

        protected override void RefreshCursorBounds()
        {
            m_LastInputTime = Platform.Neutral.GetTimeInSeconds();

            MakeCaretVisible();

            Point pA = GetCharacterPosition(m_CursorStart);
            Point pB = GetCharacterPosition(m_CursorEnd);

			m_SelectionBounds.X = Math.Min(pA.X, pB.X);
			m_SelectionBounds.Y = TextY - 1;
			m_SelectionBounds.Width = Math.Max(pA.X, pB.X) - m_SelectionBounds.X;
			m_SelectionBounds.Height = TextHeight + 2;

            m_CaretBounds.X = pA.X;
            m_CaretBounds.Y = (pA.Y + 1);

            //m_CaretBounds.Y += pA.Y;
            m_CaretBounds.Y += m_ScrollControl.VerticalScroll;

            m_CaretBounds.Width = 1;
            m_CaretBounds.Height = Font.Size + 2;

            Redraw();
        }

		private Point GetCharacterPosition(Point CursorPosition) {
			if (TextLines.Count == 0) {
				return new Point(0, 0);
			}
			string CurrLine = TextLines[CursorPosition.Y].Substring(0, CursorPosition.X);

			string sub = "";
			for (int i = 0; i < CursorPosition.Y; i++) {
				sub += TextLines[i] + "\n";
			}
			
			Point p = new Point(Skin.Renderer.MeasureText(Font, CurrLine).X, Skin.Renderer.MeasureText(Font, sub).Y);

			return new Point(p.X + m_Text.X, p.Y + m_Text.Y + TextPadding.Top);
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
            if (down) return true;
            OnReturn();

			//Split current string, putting the rhs on a new line
			string CurrentLine = TextLines[m_CursorStart.Y];
			string lhs = CurrentLine.Substring(0, m_CursorStart.X);
			string rhs = CurrentLine.Substring(m_CursorStart.X);
			
			TextLines[m_CursorStart.Y] = lhs;
			TextLines.Insert(m_CursorStart.Y + 1, rhs);

			OnKeyDown(true);
			OnKeyHome(true);

            if (this.CurrentLine == TotalLines - 1)
            {
                m_ScrollControl.ScrollToBottom();
            }

			Invalidate();
            return true;
        }

		/// <summary>
		/// Handler for Down Arrow keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
        protected override bool OnKeyDown(bool down)
        {
            if (!down) return true;

            if (CurrentLine < TotalLines - 1)
            {
				m_CursorStart.Y += 1;
            }

            if (!Input.InputHandler.IsShiftDown)
            {
                m_CursorEnd = m_CursorStart;
            }

			Invalidate();

            return true;
        }

		/// <summary>
		/// Handler for Up Arrow keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
        protected override bool OnKeyUp(bool down)
        {
			if (!down) return true;

			if (CurrentLine > 0) {
				m_CursorStart.Y -= 1;
			}

			if (!Input.InputHandler.IsShiftDown) {
				m_CursorEnd = m_CursorStart;
			}

			Invalidate();

			return true;
        }

		/// <summary>
		/// Handler for Left Arrow keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool OnKeyLeft(bool down) {
			if (!down) return true;

			if (m_CursorStart.X > 0) {
				m_CursorStart.X -= 1;
			} else {
				if (m_CursorStart.Y > 0) {
					OnKeyUp(down);
					OnKeyEnd(down);
				}
			}

			if (!Input.InputHandler.IsShiftDown) {
				m_CursorEnd = m_CursorStart;
			}

			Invalidate();

			return true;
		}

		/// <summary>
		/// Handler for Right Arrow keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool OnKeyRight(bool down) {
			if (!down) return true;

			if (m_CursorStart.X < TextLines[CurrentLine].Length - 1) {
				m_CursorStart.X += 1;
			} else {
				if (m_CursorStart.Y < TextLines.Count - 1) {
					OnKeyDown(down);
					OnKeyHome(down);
				}
			}

			if (!Input.InputHandler.IsShiftDown) {
				m_CursorEnd = m_CursorStart;
			}

			Invalidate();

			return true;
		}

		/// <summary>
		/// Handler for Home Key keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool OnKeyHome(bool down) {
			if (!down) return true;

			m_CursorStart.X = 0;

			if (!Input.InputHandler.IsShiftDown) {
				m_CursorEnd = m_CursorStart;
			}

			Invalidate();

			return true;
		}

		/// <summary>
		/// Handler for End Key keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool OnKeyEnd(bool down) {
			if (!down) return true;

			m_CursorStart.X = TextLines[CurrentLine].Length;

			if (!Input.InputHandler.IsShiftDown) {
				m_CursorEnd = m_CursorStart;
			}

			Invalidate();

			return true;
		}


		/// <summary>
		/// Handler for Backspace keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool OnKeyBackspace(bool down) {
			if (!down) return true;

			if (HasSelection) {
				EraseSelection();
				return true;
			}

			if (m_CursorStart.X == 0) {
				if (m_CursorStart.Y == 0) {
					return true; //Nothing left to delete
				} else {
					string lhs = TextLines[m_CursorStart.Y - 1];
					string rhs = TextLines[m_CursorStart.Y];
					TextLines.RemoveAt(m_CursorStart.Y);
					OnKeyUp(true);
					OnKeyEnd(true);
					TextLines[m_CursorStart.Y] = lhs + rhs;
				}
			} else {
				string CurrentLine = TextLines[m_CursorStart.Y];
				string lhs = CurrentLine.Substring(0, m_CursorStart.X - 1);
				string rhs = CurrentLine.Substring(m_CursorStart.X);
				TextLines[m_CursorStart.Y] = lhs + rhs;
				OnKeyLeft(true);
			}

			Invalidate();

			return true;
		}

		/// <summary>
		/// Handler for Delete keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool OnKeyDelete(bool down) {
			if (!down) return true;

			if (HasSelection) {
				EraseSelection();
				return true;
			}

			if (m_CursorStart.X == TextLines[m_CursorStart.Y].Length) {
				if (m_CursorStart.Y == TextLines.Count - 1) {
					return true; //Nothing left to delete
				} else {
					string lhs = TextLines[m_CursorStart.Y];
					string rhs = TextLines[m_CursorStart.Y + 1];
					TextLines.RemoveAt(m_CursorStart.Y + 1);
					OnKeyEnd(true);
					TextLines[m_CursorStart.Y] = lhs + rhs;
				}
			} else {
				string CurrentLine = TextLines[m_CursorStart.Y];
				string lhs = CurrentLine.Substring(0, m_CursorStart.X);
				string rhs = CurrentLine.Substring(m_CursorStart.X + 1);
				TextLines[m_CursorStart.Y] = lhs + rhs;
			}

			Invalidate();

			return true;
		}


		/// <summary>
		/// Sets the label text.
		/// </summary>
		/// <param name="str">Text to set.</param>
		/// <param name="doEvents">Determines whether to invoke "text changed" event.</param>
		public override void SetText(string str, bool doEvents = true) {
			string EasySplit = str.Replace("\r\n", "\n").Replace("\r", "\n");
			string[] Lines = EasySplit.Split('\n');

			TextLines = new List<string>(Lines);

			Invalidate();
		}

		/// <summary>
		/// Deletes selected text.
		/// </summary>
		public override void EraseSelection() {
			throw new NotImplementedException();
			//int start = Math.Min(m_CursorPos, m_CursorEnd);
			//int end = Math.Max(m_CursorPos, m_CursorEnd);

			//DeleteText(start, end - start);

			//// Move the cursor to the start of the selection, 
			//// since the end is probably outside of the string now.
			//m_CursorPos = start;
			//m_CursorEnd = start;
		}

		/// <summary>
		/// Deletes text.
		/// </summary>
		/// <param name="startPos">Starting cursor position.</param>
		/// <param name="length">Length in characters.</param>
		public void DeleteText(Point startPos, int length) {
			throw new NotImplementedException();
			//string str = Text;
			//str = str.Remove(startPos, length);
			//SetText(str);

			//if (m_CursorPos > startPos) {
			//    CursorPos = m_CursorPos - length;
			//}

			//CursorEnd = m_CursorPos;
		}

		public override void Invalidate() {
			if (m_Text != null) {
				m_Text.String = Text;
			}
			if (AutoSizeToContents)
				SizeToContents();

			base.Invalidate();
			InvalidateParent();
			OnTextChanged();
		}

		/// <summary>
		/// Handler for text changed event.
		/// </summary>
		protected override void OnTextChanged() {
			//Todo halfofastaple: some bounds checking to make sure the cursor positions are valid
			if (TextChanged != null)
				TextChanged.Invoke(this);
		}
    }
}