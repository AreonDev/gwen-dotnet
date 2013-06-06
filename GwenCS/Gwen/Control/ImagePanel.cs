﻿using System;
using System.Drawing;

namespace Gwen.Control
{
    /// <summary>
    /// Image container.
    /// </summary>
    public class ImagePanel : Base
    {
        private readonly Texture m_Texture;
        private readonly float[] m_uv;
        private Color m_DrawColor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImagePanel"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ImagePanel(Base parent)
            : base(parent)
        {
            m_uv = new float[4];
            m_Texture = new Texture(Skin.Renderer);
            SetUV(0, 0, 1, 1);
            MouseInputEnabled = true;
            m_DrawColor = Color.White;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            m_Texture.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Sets the texture coordinates of the image.
        /// </summary>
        public virtual void SetUV(float u1, float v1, float u2, float v2)
        {
            m_uv[0] = u1;
            m_uv[1] = v1;
            m_uv[2] = u2;
            m_uv[3] = v2;
        }

        /// <summary>
        /// Texture name.
        /// </summary>
        public String ImageName
        {
            get { return m_Texture.Name; }
            set { m_Texture.Load(value); }
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.Base skin)
        {
            base.Render(skin);
            skin.Renderer.DrawColor = m_DrawColor;
            skin.Renderer.DrawTexturedRect(m_Texture, RenderBounds, m_uv[0], m_uv[1], m_uv[2], m_uv[3]);
        }

        /// <summary>
        /// Sizes the control to its contents.
        /// </summary>
        public virtual void SizeToContents()
        {
            SetSize(m_Texture.Width, m_Texture.Height);
        }

        /// <summary>
        /// Invoked when the Image Panel is released.
        /// </summary>
        public event GwenEventHandler Clicked;

        /// <summary>
        /// "Clicks" the button.
        /// </summary>
        public virtual void Press(Base control = null)
        {
            OnClicked();
        }

        /// <summary>
        /// Handler invoked on mouse click (left) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="down">If set to <c>true</c> mouse button is down.</param>
        protected override void OnMouseClickedLeft(int x, int y, bool down)
        {
            //base.OnMouseClickedLeft(x, y, down);
            if (IsHovered && down)
            {
                OnClicked();
            }

            Redraw();
        }

        /// <summary>
        /// Internal OnPressed implementation.
        /// </summary>
        protected virtual void OnClicked()
        {
            if (Clicked != null)
                Clicked.Invoke(this);
        }

        /// <summary>
        /// Control has been clicked - invoked by input system. Windows use it to propagate activation.
        /// </summary>
        public override void Touch()
        {
            base.Touch();
        }

        /// <summary>
        /// Handler for Space keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeySpace(bool down)
        {
            if (down)
                OnClicked();
            return true;
        }

        /// <summary>
        /// Default accelerator handler.
        /// </summary>
        protected override void OnAccelerator()
        {
            OnClicked();
        }
    }
}
