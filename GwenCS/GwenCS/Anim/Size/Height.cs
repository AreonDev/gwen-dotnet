﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gwen.Anim.Size
{
    class Height : TimedAnimation
    {
        protected int m_iStartSize;
        protected int m_iDelta;
        protected bool m_bHide;

        public Height(int iStartSize, int iEndSize, double fLength, bool bHide = false, double fDelay = 0.0, double fEase = 1.0) 
            : base( fLength, fDelay, fEase )
        {
            m_iStartSize = iStartSize;
            m_iDelta = iEndSize - m_iStartSize;
            m_bHide = bHide;
        }

        protected override void onStart() { base.onStart(); m_Control.Height = m_iStartSize; }
        protected override void Run(double delta) { base.Run(delta); m_Control.Height = Global.Trunc( m_iStartSize + (m_iDelta * delta)); }
        protected override void onFinish() { base.onFinish(); m_Control.Height = m_iStartSize + m_iDelta; m_Control.IsHidden = m_bHide; }
    }
}
