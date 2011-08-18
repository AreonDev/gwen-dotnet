﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Gwen.Controls
{
    public class HorizontalSlider : Slider
    {
        public HorizontalSlider(Base parent) : base(parent)
        {
            
        }

        protected override float CalculateValue()
        {
            return (float)m_SliderBar.X / (Width - m_SliderBar.Width);
        }

        protected override void UpdateBarFromValue()
        {
            m_SliderBar.MoveTo((int)((Width - m_SliderBar.Width) * (m_fValue)), m_SliderBar.Y);
        }

        internal override void onMouseClickLeft(int x, int y, bool pressed)
        {
            m_SliderBar.MoveTo((int)(CanvasPosToLocal(new Point(x, y)).X - m_SliderBar.Width*0.5), m_SliderBar.Y);
            m_SliderBar.onMouseClickLeft(x, y, pressed);
            onMoved(m_SliderBar);
        }

        protected override void Layout(Skin.Base skin)
        {
            m_SliderBar.SetSize(10, Height);
        }

        protected override void Render(Skin.Base skin)
        {
            skin.DrawSlider(this, true, m_bClampToNotches ? m_iNumNotches : 0, m_SliderBar.Width);
        }
    }
}
