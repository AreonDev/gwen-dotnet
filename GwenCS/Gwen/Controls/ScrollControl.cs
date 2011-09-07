﻿using System;
using System.Linq;

namespace Gwen.Controls
{
    public class ScrollControl : Base
    {
        protected bool m_CanScrollH;
        protected bool m_CanScrollV;
        protected bool m_AutoHideBars;

        protected BaseScrollBar m_VerticalScrollBar;
        protected BaseScrollBar m_HorizontalScrollBar;

        public bool CanScrollH { get { return m_CanScrollH; } }
        public bool CanScrollV { get { return m_CanScrollV; } }
        public bool AutoHideBars { get { return m_AutoHideBars; } set { m_AutoHideBars = value; } }

        public ScrollControl(Base parent)
            : base(parent)
        {
            MouseInputEnabled = false;

            m_VerticalScrollBar = new VerticalScrollBar(this);
            m_VerticalScrollBar.Dock = Pos.Right;
            m_VerticalScrollBar.OnBarMoved += VBarMoved;
            m_CanScrollV = true;
            m_VerticalScrollBar.NudgeAmount = 30;

            m_HorizontalScrollBar = new HorizontalScrollBar(this);
            m_HorizontalScrollBar.Dock = Pos.Bottom;
            m_HorizontalScrollBar.OnBarMoved += HBarMoved;
            m_CanScrollH = true;
            m_HorizontalScrollBar.NudgeAmount = 30;

            m_InnerPanel = new Base(this);
            m_InnerPanel.SetPos(0, 0);
            m_InnerPanel.Margin = new Margin(5, 5, 5, 5);
            m_InnerPanel.SendToBack();
            m_InnerPanel.MouseInputEnabled = false;

            m_AutoHideBars = false;
        }

        public override void Dispose()
        {
            m_VerticalScrollBar.Dispose();
            m_HorizontalScrollBar.Dispose();
            base.Dispose();
        }

        protected bool HScrollRequired
        {
            set
            {
                if (value)
                {
                    m_HorizontalScrollBar.SetScrollAmount(0, true);
                    m_HorizontalScrollBar.IsDisabled = true;
                    if (m_AutoHideBars)
                        m_HorizontalScrollBar.IsHidden = true;
                }
                else
                {
                    m_HorizontalScrollBar.IsHidden = false;
                    m_HorizontalScrollBar.IsDisabled = false;
                }
            }
        }

        protected bool VScrollRequired
        {
            set
            {
                if (value)
                {
                    m_VerticalScrollBar.SetScrollAmount(0, true);
                    m_VerticalScrollBar.IsDisabled = true;
                    if (m_AutoHideBars)
                        m_VerticalScrollBar.IsHidden = true;
                }
                else
                {
                    m_VerticalScrollBar.IsHidden = false;
                    m_VerticalScrollBar.IsDisabled = false;
                }
            }
        }

        public virtual void SetScroll(bool h, bool v)
        {
            m_CanScrollV = v;
            m_CanScrollH = h;
            m_VerticalScrollBar.IsHidden = !m_CanScrollV;
            m_HorizontalScrollBar.IsHidden = !m_CanScrollH;
        }

        protected virtual void SetInnerSize(int w, int h)
        {
            m_InnerPanel.SetSize(w, h);
        }

        protected virtual void VBarMoved(Base control)
        {
            Invalidate();
        }

        protected virtual void HBarMoved(Base control)
        {
            Invalidate();
        }

        internal override void onChildBoundsChanged(System.Drawing.Rectangle oldChildBounds, Base child)
        {
            UpdateScrollBars();
        }

        protected override void Layout(Skin.Base skin)
        {
            UpdateScrollBars();
            base.Layout(skin);
        }

        internal override bool onMouseWheeled(int delta)
        {
            if (CanScrollV && m_VerticalScrollBar.IsVisible)
            {
                if (m_VerticalScrollBar.SetScrollAmount(
                    m_VerticalScrollBar.ScrollAmount - m_VerticalScrollBar.NudgeAmount * (delta / 60.0f), true))
                    return true;
            }

            if (CanScrollH && m_HorizontalScrollBar.IsVisible)
            {
                if (m_HorizontalScrollBar.SetScrollAmount(
                    m_HorizontalScrollBar.ScrollAmount - m_HorizontalScrollBar.NudgeAmount * (delta / 60.0f), true))
                    return true;
            }

            return false;
        }

        protected override void Render(Skin.Base skin)
        {
#if false

    // Debug render - this shouldn't render ANYTHING REALLY - it should be up to the parent!

    Gwen::Rect rect = GetRenderBounds();
    Gwen::Renderer::Base* render = skin->GetRender();

    render->SetDrawColor( Gwen::Color( 255, 255, 0, 100 ) );
    render->DrawFilledRect( rect );

    render->SetDrawColor( Gwen::Color( 255, 0, 0, 100 ) );
    render->DrawFilledRect( m_InnerPanel->GetBounds() );

    render->RenderText( skin->GetDefaultFont(), Gwen::Point( 0, 0 ), Utility::Format( L"Offset: %i %i", m_InnerPanel->X(), m_InnerPanel->Y() ) );
#endif
        }

        protected virtual void UpdateScrollBars()
        {
            if (null == m_InnerPanel)
                return;

            //Get the max size of all our children together
            int childrenWidth = m_InnerPanel.Children.Count > 0 ? m_InnerPanel.Children.Max(x => x.Right) : 0;
            int childrenHeight = m_InnerPanel.Children.Count > 0 ? m_InnerPanel.Children.Max(x => x.Bottom) : 0;

            if (m_CanScrollH)
            {
                m_InnerPanel.SetSize(Math.Max(Width, childrenWidth), Math.Max(Height, childrenHeight));  
            }
            else
            {
                m_InnerPanel.SetSize(Width - (m_VerticalScrollBar.IsHidden ? 0 : m_VerticalScrollBar.Width),
                                     Math.Max(Height, childrenHeight));
            }

            float wPercent = Width/
                             (float) (childrenWidth + (m_VerticalScrollBar.IsHidden ? 0 : m_VerticalScrollBar.Width));
            float hPercent = Height/
                             (float)
                             (childrenHeight + (m_HorizontalScrollBar.IsHidden ? 0 : m_HorizontalScrollBar.Height));

            if (m_CanScrollV)
                VScrollRequired = hPercent >= 1;
            else
                m_VerticalScrollBar.IsHidden = true;

            if (m_CanScrollH)
                HScrollRequired = wPercent >= 1;
            else
                m_HorizontalScrollBar.IsHidden = true;


            m_VerticalScrollBar.ContentSize = m_InnerPanel.Height;
            m_VerticalScrollBar.ViewableContentSize = Height -
                                                      (m_HorizontalScrollBar.IsHidden ? 0 : m_HorizontalScrollBar.Height);


            m_HorizontalScrollBar.ContentSize = m_InnerPanel.Width;
            m_HorizontalScrollBar.ViewableContentSize = Width -
                                                        (m_VerticalScrollBar.IsHidden ? 0 : m_VerticalScrollBar.Width);

            int newInnerPanelPosX = 0;
            int newInnerPanelPosY = 0;

            if (CanScrollV && !m_VerticalScrollBar.IsHidden)
            {
                newInnerPanelPosY =
                    Global.Trunc(
                        -((m_InnerPanel.Height) - Height + (m_HorizontalScrollBar.IsHidden ? 0 : m_HorizontalScrollBar.Height))*
                        m_VerticalScrollBar.ScrollAmount);
            }
            if (CanScrollH && !m_HorizontalScrollBar.IsHidden)
            {
                newInnerPanelPosX =
                    Global.Trunc(
                        -((m_InnerPanel.Width) - Width + (m_VerticalScrollBar.IsHidden ? 0 : m_VerticalScrollBar.Width))*
                        m_HorizontalScrollBar.ScrollAmount);
            }

            m_InnerPanel.SetPos(newInnerPanelPosX, newInnerPanelPosY);
        }

        public virtual void ScrollToBottom()
        {
            if (!CanScrollV)
                return;

            UpdateScrollBars();
            m_VerticalScrollBar.ScrollToBottom();
        }

        public virtual void ScrollToTop()
        {
            if (CanScrollV)
            {
                UpdateScrollBars();
                m_VerticalScrollBar.ScrollToTop();
            }
        }

        public virtual void ScrollToLeft()
        {
            if (CanScrollH)
            {
                UpdateScrollBars();
                m_VerticalScrollBar.ScrollToLeft();
            }
        }

        public virtual void ScrollToRight()
        {
            if (CanScrollH)
            {
                UpdateScrollBars();
                m_VerticalScrollBar.ScrollToRight();
            }
        }

        public virtual void RemoveAll()
        {
            m_InnerPanel.RemoveAllChildren();
        }
    }
}
