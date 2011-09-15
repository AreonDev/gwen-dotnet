﻿using System;
using Gwen.Control;

namespace Gwen.UnitTest
{
    public class ImagePanel : GUnit
    {
        public ImagePanel(Base parent)
            : base(parent)
        {
            // Normal
            {
                Control.ImagePanel img = new Control.ImagePanel(this);
                img.ImageName = "gwen.png";
                img.SetBounds(10, 10, 100, 100);
            }

            // Missing
            {
                Control.ImagePanel img = new Control.ImagePanel(this);
                img.ImageName = "missingimage.png";
                img.SetBounds(120, 10, 100, 100);
            }
        }
    }
}
