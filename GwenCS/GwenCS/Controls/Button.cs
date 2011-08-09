﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gwen.Controls
{
    public class Button : Label
    {
        protected bool m_bDepressed;
        protected bool m_bToggle;
        protected bool m_bToggleStatus;
        protected bool m_bCenterImage;
        protected ImagePanel m_Image;

        public event ControlCallback OnPress;
        public event ControlCallback OnDown;
        public event ControlCallback OnUp;
        public event ControlCallback OnDoubleClick;
        public event ControlCallback OnToggle;
        public event ControlCallback OnToggleOn;
        public event ControlCallback OnToggleOff;

        public bool IsDepressed { get { return m_bDepressed; } }
        public bool IsToggle { get { return m_bToggle; } set { m_bToggle = value; } }
        public bool ToggleState
        {
            get { return m_bToggleStatus; }
            set
            {
                if (m_bToggleStatus == value) return;

                m_bToggleStatus = value;

                if (OnToggle != null)
                    OnToggle.Invoke(this);

                if (m_bToggleStatus)
                {
                    if (OnToggleOn != null)
                        OnToggleOn.Invoke(this);
                }
                else
                {
                    if (OnToggleOff != null)
                        OnToggleOff.Invoke(this);
                }
            }
        }

        public Button(Base parent)
            : base(parent)
        {
            SetSize(100, 20);
            MouseInputEnabled = true;
            Alignment = Pos.Center;
            TextPadding = new Padding(3, 0, 3, 0);
        }

        public virtual void Toggle()
        {
            ToggleState = !ToggleState;
        }

        public virtual void ReceiveEventPress(Base control)
        {
            onPress();
        }

        protected override void Render(Skin.Base skin)
        {
            base.Render(skin);

            if (ShouldDrawBackground)
            {
                bool bDrawDepressed = IsDepressed && IsHovered;
                if (IsToggle)
                    bDrawDepressed = bDrawDepressed || ToggleState;

                bool bDrawHovered = IsHovered && ShouldDrawHover;

                skin.DrawButton(this, bDrawDepressed, bDrawHovered);
            }
        }

        internal override void onMouseClickLeft(int x, int y, bool pressed)
        {
            base.onMouseClickLeft(x, y, pressed);
            if (pressed)
            {
                m_bDepressed = true;
                Global.MouseFocus = this;
                if (OnDown != null)
                    OnDown.Invoke(this);
            }
            else
            {
                if (IsHovered && m_bDepressed)
                {
                    onPress();
                }

                m_bDepressed = false;
                Global.MouseFocus = null;
                if (OnUp != null)
                    OnUp.Invoke(this);
            }

            Redraw();
        }

        protected virtual void onPress()
        {
            if (IsToggle)
            {
                Toggle();
            }

            if (OnPress != null)
                OnPress.Invoke(this);
        }
        
        public virtual void SetImage(String name, bool center = false)
        {
            if (String.IsNullOrEmpty(name))
            {
                m_Image = null;
                return;
            }

            if (m_Image == null)
            {
                m_Image = new ImagePanel(this);
            }

            m_Image.ImageName = name;
            m_Image.SizeToContents( );
            m_Image.SetPos(m_Padding.Left, 2);
            m_bCenterImage = center;

            int IdealTextPadding = m_Image.Right + m_Padding.Left + 4;
            if (m_rTextPadding.Left < IdealTextPadding)
            {
                m_rTextPadding.Left = IdealTextPadding;
            }
        }

        public override void SizeToContents()
        {
            base.SizeToContents();
            if (m_Image != null)
            {
                int height = m_Image.Height + 4;
                if (Height < height)
                {
                    Height = height;
                }
            }
        }

        internal override bool onKeySpace(bool bDown)
        {
            base.onKeySpace(bDown);
            onMouseClickLeft(0, 0, bDown);
            return true;
        }

        protected override void AcceleratePressed()
        {
            base.AcceleratePressed();
            onPress();
        }

        protected override void Layout(Skin.Base skin)
        {
            base.Layout(skin);
            if (m_Image != null)
            {
                Align.CenterVertically(m_Image);

                if (m_bCenterImage)
                    Align.CenterHorizontally(m_Image);
            }
        }
    }
}
