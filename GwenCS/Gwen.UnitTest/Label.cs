﻿using System;
using System.Drawing;
using Gwen.Control;

namespace Gwen.UnitTest
{
    public class Label : GUnit
    {
        private readonly Font font1;
        private readonly Font font2;
        private readonly Font font3;

        public Label(Base parent) : base(parent)
        {
            {
                Control.Label label = new Control.Label(this);
                label.AutoSizeToContents = true;
                label.Text = "Standard label";
                label.SetPosition(10, 10);
            }
            {
                Control.Label label = new Control.Label(this);
                label.AutoSizeToContents = true;
                label.Text = "Chinese: \u4E45\u6709\u5F52\u5929\u613F \u7EC8\u8FC7\u9B3C\u95E8\u5173";
                label.SetPosition(10, 30);
            }
            {
                Control.Label label = new Control.Label(this);
                label.AutoSizeToContents = true;
                label.Text = "Japanese: \u751F\u3080\u304E\u3000\u751F\u3054\u3081\u3000\u751F\u305F\u307E\u3054";
                label.SetPosition(10, 50);
            }
            {
                Control.Label label = new Control.Label(this);
                label.AutoSizeToContents = true;
                label.Text = "Korean: \uADF9\uC9C0\uD0D0\uD5D8\u3000\uD611\uD68C\uACB0\uC131\u3000\uCCB4\uACC4\uC801\u3000\uC5F0\uAD6C";
                label.SetPosition(10, 70);
            }
            {
                Control.Label label = new Control.Label(this);
                label.AutoSizeToContents = true;
                label.Text = "Hindi: \u092F\u0947 \u0905\u0928\u0941\u091A\u094D\u091B\u0947\u0926 \u0939\u093F\u0928\u094D\u0926\u0940 \u092E\u0947\u0902 \u0939\u0948\u0964";
                label.SetPosition(10, 90);
            }
            {
                Control.Label label = new Control.Label(this);
                label.AutoSizeToContents = true;
                label.Text = "Arabic: \u0627\u0644\u0622\u0646 \u0644\u062D\u0636\u0648\u0631 \u0627\u0644\u0645\u0624\u062A\u0645\u0631 \u0627\u0644\u062F\u0648\u0644\u064A";
                label.SetPosition(10, 110);
            }
            {
                Control.Label label = new Control.Label(this);
                label.AutoSizeToContents = true;
                label.MouseInputEnabled = true; // needed for tooltip
                label.Text = "Wow, Coloured Text (and tooltip)";
                label.TextColor = Color.Blue;
                label.SetToolTipText("I'm a tooltip");
                font3 = new Font(Skin.Renderer, "Motorwerk", 20);
                ((Control.Label) label.ToolTip).Font = font3;
                label.SetPosition(10, 130);
            }
            {
                Control.Label label = new Control.Label(this);
                label.AutoSizeToContents = true;
                label.Text = "Coloured Text With Alpha";
                label.TextColor = Color.FromArgb(100, 0, 0, 255);
                label.SetPosition(10, 150);
            }
            {
                // Note that when using a custom font, this font object has to stick around
                // for the lifetime of the label. Rethink, or is that ideal?
                font1 = new Font(Skin.Renderer);
                font1.FaceName = "Comic Sans MS";
                font1.Size = 25;

                Control.Label label = new Control.Label(this);
                label.AutoSizeToContents = true;
                label.Text = "Custom Font (Comic Sans 25)";
                label.SetPosition(10, 170);
                label.Font = font1;
            }
            {
                font2 = new Font(Skin.Renderer, "French Script MT", 35);

                Control.Label label = new Control.Label(this);
                label.AutoSizeToContents = true;
                label.Font = font2;
                label.SetPosition(10, 210);
                label.Text = "Custom Font (French Script MT 35)";
            }
        }

        public override void Dispose()
        {
            font1.Dispose();
            font2.Dispose();
            font3.Dispose();
            base.Dispose();
        }
    }
}
