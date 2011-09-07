﻿using System;
using System.Drawing;

namespace Gwen.Controls
{
    public class ColorLerpBox : Base
    {
        protected Point m_CursorPos;
        protected bool m_Depressed;
        protected byte m_Hue;
        protected Texture m_Texture; // [omeg] added

        public event ControlCallback OnSelectionChanged;

        public ColorLerpBox(Base parent) : base(parent)
        {
            SetColor(Color.FromArgb(255, 255, 128, 0));
            SetSize(128, 128);
            MouseInputEnabled = true;
            m_Depressed = false;

            // texture initialized in Render() if null
        }

        public static Color Lerp(Color toColor, Color fromColor, float amount)
        {
            Color delta = toColor.Subtract(fromColor);
            delta = delta.Multiply(amount);
            return fromColor.Add(delta);
        }

        public Color SelectedColor
        {
            get { return GetColorAt(m_CursorPos.X, m_CursorPos.Y); }
        }

        public void SetColor(Color value, bool onlyHue = true)
        {
            HSV hsv = value.ToHSV();
            m_Hue = (byte) (hsv.h);
            if (!onlyHue)
            {
                m_CursorPos.X = Global.Trunc(hsv.s*Width);
                m_CursorPos.Y = Global.Trunc((1 - hsv.v)*Height);
            }
            Invalidate();

            if (OnSelectionChanged != null)
                OnSelectionChanged.Invoke(this);
        }

        internal override void onMouseMoved(int x, int y, int dx, int dy)
        {
            if (m_Depressed)
            {
                m_CursorPos = CanvasPosToLocal(new Point(x, y));
                //Do we have clamp?
                if (m_CursorPos.X < 0)
                    m_CursorPos.X = 0;
                if (m_CursorPos.X > Width)
                    m_CursorPos.X = Width;

                if (m_CursorPos.Y < 0)
                    m_CursorPos.Y = 0;
                if (m_CursorPos.Y > Height)
                    m_CursorPos.Y = Height;

                if (OnSelectionChanged != null)
                    OnSelectionChanged.Invoke(this);
            }
        }

        internal override void onMouseClickLeft(int x, int y, bool down)
        {
            m_Depressed = down;
            if (down)
                Global.MouseFocus = this;
            else
                Global.MouseFocus = null;

            onMouseMoved(x, y, 0, 0);
        }

        public Color GetColorAt(int x, int y)
        {
            float xPercent = (x / (float)Width);
            float yPercent = 1 - (y / (float)Height);

            Color result = Global.HSVToColor(m_Hue, xPercent, yPercent);

            return result;
        }

        // [omeg] added
        public override void Invalidate()
        {
            if (m_Texture != null)
            {
                m_Texture.Dispose();
                m_Texture = null;
            }
            base.Invalidate();
        }

        public override void Dispose()
        {
            base.Dispose();
            m_Texture.Dispose();
        }

        protected override void Render(Skin.Base skin)
        {
            base.Render(skin);
            if (m_Texture == null)
            {
                byte[] pixelData = new byte[Width*Height*4];

                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        Color c = GetColorAt(x, y);
                        pixelData[4*(x + y*Width)] = c.R;
                        pixelData[4*(x + y*Width) + 1] = c.G;
                        pixelData[4*(x + y*Width) + 2] = c.B;
                        pixelData[4*(x + y*Width) + 3] = c.A;
                    }
                }

                m_Texture = new Texture(skin.Renderer);
                m_Texture.Width = Width;
                m_Texture.Height = Height;
                m_Texture.LoadRaw(Width, Height, pixelData);
            }

            skin.Renderer.DrawTexturedRect(m_Texture, RenderBounds);


            skin.Renderer.DrawColor = Color.Black;
            skin.Renderer.DrawLinedRect(RenderBounds);

            Color selected = SelectedColor;
            if ((selected.R + selected.G + selected.B)/3 < 170)
                skin.Renderer.DrawColor = Color.White;
            else
                skin.Renderer.DrawColor = Color.Black;

            Rectangle testRect = new Rectangle(m_CursorPos.X - 3, m_CursorPos.Y - 3, 6, 6);

            skin.Renderer.DrawShavedCornerRect(testRect);
        }
    }
}
