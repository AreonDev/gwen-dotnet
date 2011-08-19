﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gwen.Anim.Size
{
    class Width : TimedAnimation
    {
        protected int m_iStartSize;
        protected int m_iDelta;
        protected bool m_bHide;

        public Width(int iStartSize, int iEndSize, float fLength, bool bHide = false, float fDelay = 0.0f, float fEase = 1.0f)
            : base(fLength, fDelay, fEase)
        {
            m_iStartSize = iStartSize;
            m_iDelta = iEndSize - m_iStartSize;
            m_bHide = bHide;
        }

        protected override void onStart()
        {
            base.onStart(); 
            m_Control.Width = m_iStartSize; 
        }

        protected override void Run(float delta) 
        {
            base.Run(delta);
            m_Control.Width = (int)Math.Round(m_iStartSize + (m_iDelta * delta)); 
        }

        protected override void onFinish()
        {
            base.onFinish();
            m_Control.Width = m_iStartSize + m_iDelta; m_Control.IsHidden = m_bHide;
        }
    }
}
