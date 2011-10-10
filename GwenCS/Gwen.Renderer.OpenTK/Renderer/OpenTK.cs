﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Gwen.Renderer
{
    public class OpenTK : Base
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Vertex
        {
            public float x, y, z;
            public float u, v;
            public byte r, g, b, a;
        }

        private const int MaxVerts = 1024;
        private Color m_Color;
        private int m_iVertNum;
        private readonly Vertex[] m_Vertices;
        private readonly int m_VertexSize;

        private readonly Dictionary<Tuple<String, Font>, TextRenderer> m_StringCache;
        private readonly Graphics m_Graphics; // only used for text measurement

        public OpenTK()
            : base()
        {
            m_Vertices = new Vertex[MaxVerts];
            m_iVertNum = 0;
            for (int i = 0; i < MaxVerts; i++)
                m_Vertices[i].z = 0.5f;

            m_VertexSize = Marshal.SizeOf(m_Vertices[0]);
            //Debug.Assert(Marshal.SizeOf(m_Vertices) != MaxVerts*m_VertexSize);

            m_StringCache = new Dictionary<Tuple<String, Font>, TextRenderer>();
            m_Graphics = Graphics.FromImage(new Bitmap(1024, 1024, PixelFormat.Format32bppArgb));
        }

        public override void Dispose()
        {
            FlushTextCache();
            base.Dispose();
        }

        public override void Begin()
        {
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.AlphaFunc(AlphaFunction.Greater, 1.0f);

            GL.Enable(EnableCap.Blend);
        }

        public override void End()
        {
            Flush();
        }

        /// <summary>
        /// Returns number of cached strings in the text cache.
        /// </summary>
        public int TextCacheSize { get { return m_StringCache.Count; } }

        /// <summary>
        /// Clears the text rendering cache. Make sure to call this if cached strings size becomes too big (check TextCacheSize).
        /// </summary>
        public void FlushTextCache()
        {
            // todo: some auto-expiring cache? based on numner of elements or age
            foreach (var textRenderer in m_StringCache.Values)
            {
                textRenderer.Dispose();
            }
            m_StringCache.Clear();
        }

        private unsafe void Flush()
        {
            if (m_iVertNum == 0) return;

            GL.EnableClientState(ArrayCap.VertexArray);
            fixed (float* ptr1 = &m_Vertices[0].x)
                GL.VertexPointer(3, VertexPointerType.Float, m_VertexSize, (IntPtr)ptr1);

            GL.EnableClientState(ArrayCap.ColorArray);
            fixed (byte* ptr2 = &m_Vertices[0].r)
                GL.ColorPointer(4, ColorPointerType.UnsignedByte, m_VertexSize, (IntPtr)ptr2);

            GL.EnableClientState(ArrayCap.TextureCoordArray);
            fixed (float* ptr3 = &m_Vertices[0].u)
                GL.TexCoordPointer(2, TexCoordPointerType.Float, m_VertexSize, (IntPtr)ptr3);

            GL.DrawArrays(BeginMode.Triangles, 0, m_iVertNum);

            m_iVertNum = 0;
            GL.Flush();

            // get rid of pending objects
            //foreach (IDisposable obj in m_StringCache.Values)
            //    obj.Dispose();
            //m_DisposeQueue.Clear();
        }

        private void AddVert(int x, int y, float u = 0.0f, float v = 0.0f)
        {
            if (m_iVertNum >= MaxVerts - 1)
            {
                Flush();
            }

            m_Vertices[m_iVertNum].x = x;
            m_Vertices[m_iVertNum].y = y;
            m_Vertices[m_iVertNum].u = u;
            m_Vertices[m_iVertNum].v = v;

            m_Vertices[m_iVertNum].r = m_Color.R;
            m_Vertices[m_iVertNum].g = m_Color.G;
            m_Vertices[m_iVertNum].b = m_Color.B;
            m_Vertices[m_iVertNum].a = m_Color.A;

            m_iVertNum++;
        }

        public override void DrawFilledRect(Rectangle rect)
        {
            bool texturesOn;

            GL.GetBoolean(GetPName.Texture2D, out texturesOn);
            if (texturesOn)
            {
                Flush();
                GL.Disable(EnableCap.Texture2D);
            }

            rect = Translate(rect);

            AddVert(rect.X, rect.Y);
            AddVert(rect.X + rect.Width, rect.Y);
            AddVert(rect.X, rect.Y + rect.Height);

            AddVert(rect.X + rect.Width, rect.Y);
            AddVert(rect.X + rect.Width, rect.Y + rect.Height);
            AddVert(rect.X, rect.Y + rect.Height);
        }

        public override Color DrawColor
        {
            get { return m_Color; }
            set
            {
                byte[] col = new byte[4];
                col[0] = value.R;
                col[1] = value.G;
                col[2] = value.B;
                col[3] = value.A;
                GL.Color4(col);
                m_Color = value;
            }
        }

        public override void StartClip()
        {
            Flush();
            Rectangle rect = ClipRegion;

            // OpenGL's coords are from the bottom left
            // so we need to translate them here.
            {
                int[] view = new int[4];
                GL.GetInteger(GetPName.Viewport, view);
                rect.Y = view[3] - (rect.Y + rect.Height);
            }

            GL.Scissor((int)(rect.X * Scale), (int)(rect.Y * Scale), (int)(rect.Width * Scale), (int)(rect.Height * Scale));
            GL.Enable(EnableCap.ScissorTest);
        }

        public override void EndClip()
        {
            Flush();
            GL.Disable(EnableCap.ScissorTest);
        }

        public override void DrawTexturedRect(Texture t, Rectangle rect, float u1 = 0, float v1 = 0, float u2 = 1, float v2 = 1)
        {
            // Missing image, not loaded properly?
            if (null == t.RendererData)
            {
                DrawMissingImage(rect);
                return;
            }

            int tex = (int)t.RendererData;
            rect = Translate(rect);

            int boundtex;

            bool texturesOn;
            GL.GetBoolean(GetPName.Texture2D, out texturesOn);
            GL.GetInteger(GetPName.TextureBinding2D, out boundtex);
            if (!texturesOn || tex != boundtex)
            {
                Flush();
                GL.BindTexture(TextureTarget.Texture2D, tex);
                GL.Enable(EnableCap.Texture2D);
            }

            AddVert(rect.X, rect.Y, u1, v1);
            AddVert(rect.X + rect.Width, rect.Y, u2, v1);
            AddVert(rect.X, rect.Y + rect.Height, u1, v2);

            AddVert(rect.X + rect.Width, rect.Y, u2, v1);
            AddVert(rect.X + rect.Width, rect.Y + rect.Height, u2, v2);
            AddVert(rect.X, rect.Y + rect.Height, u1, v2);
        }

        public override bool LoadFont(Font font)
        {
            Debug.Print(String.Format("LoadFont {0}", font.FaceName));
            font.RealSize = font.Size * Scale;
            System.Drawing.Font sysFont = font.RendererData as System.Drawing.Font;

            if (sysFont != null)
                sysFont.Dispose();
            
            // apaprently this can't fail @_@
            // "If you attempt to use a font that is not supported, or the font is not installed on the machine that is running the application, the Microsoft Sans Serif font will be substituted."
            sysFont = new System.Drawing.Font(font.FaceName, font.Size);
            font.RendererData = sysFont;
            return true;
        }

        public override void FreeFont(Font font)
        {
            Debug.Print(String.Format("FreeFont {0}", font.FaceName));
            if (font.RendererData == null)
                return;

            Debug.Print(String.Format("FreeFont {0} - actual free", font.FaceName));
            System.Drawing.Font sysFont = font.RendererData as System.Drawing.Font;
            if (sysFont == null)
                 throw new InvalidOperationException("Freeing empty font");

            sysFont.Dispose();
            font.RendererData = null;
        }
        
        public override Point MeasureText(Font font, string text)
        {
            //Debug.Print(String.Format("MeasureText {0}", font.FaceName));
            System.Drawing.Font sysFont = font.RendererData as System.Drawing.Font;
            if (sysFont == null || Math.Abs(font.RealSize - font.Size * Scale) > 2)
            {
                FreeFont(font);
                LoadFont(font);
                sysFont = font.RendererData as System.Drawing.Font;
            }

            var key = new Tuple<String, Font>(text, font);
            if (m_StringCache.ContainsKey(key))
            {
                var tex = m_StringCache[key].Texture;
                return new Point(tex.Width, tex.Height);
            }

            SizeF size = m_Graphics.MeasureString(text, sysFont);
            return new Point((int)size.Width, (int)size.Height);
        }
        
        public override void RenderText(Font font, Point position, string text)
        {
            //Debug.Print(String.Format("RenderText {0}", font.FaceName));

            System.Drawing.Font sysFont = font.RendererData as System.Drawing.Font;
            if (sysFont == null || Math.Abs(font.RealSize - font.Size * Scale) > 2)
            {
                FreeFont(font);
                LoadFont(font);
                sysFont = font.RendererData as System.Drawing.Font;
            }

            var key = new Tuple<String, Font>(text, font);
            if (!m_StringCache.ContainsKey(key))
            {
                // not cached - create text renderer
                Debug.Print(String.Format("RenderText: caching \"{0}\", {1}", text, font.FaceName));

                Point size = MeasureText(font, text);
                TextRenderer tr = new TextRenderer(size.X, size.Y, this);
                Brush brush = new SolidBrush(DrawColor); // todo: cache
                tr.DrawString(text, sysFont, brush, Point.Empty); // renders string on the texture

                DrawTexturedRect(tr.Texture, new Rectangle(position.X, position.Y, tr.Texture.Width, tr.Texture.Height));

                brush.Dispose();
                m_StringCache[key] = tr;
            }
            else
            {
                TextRenderer tr = m_StringCache[key];
                DrawTexturedRect(tr.Texture, new Rectangle(position.X, position.Y, tr.Texture.Width, tr.Texture.Height));
            }
        }
        
        internal static void LoadTextureInternal(Texture t, Bitmap bmp)
        {
            // todo: convert to proper format
            if (bmp.PixelFormat != PixelFormat.Format32bppArgb)
            {
                t.Failed = true;
                return;
            }

            int glTex;

            // Create the opengl texture
            GL.GenTextures(1, out glTex);
            GL.BindTexture(TextureTarget.Texture2D, glTex);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // Sort out our GWEN texture
            t.RendererData = glTex;
            t.Width = bmp.Width;
            t.Height = bmp.Height;

            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, t.Width, t.Height, 0, global::OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bmp.UnlockBits(data);
        }

        public override void LoadTexture(Texture t)
        {
            Bitmap bmp;
            try
            {
                bmp = new Bitmap(t.Name);
            }
            catch (Exception)
            {
                t.Failed = true;
                return;
            }

            LoadTextureInternal(t, bmp);
            bmp.Dispose();
        }

        public override void LoadTextureStream(Texture t, System.IO.Stream data)
        {
            Bitmap bmp;
            try
            {
                bmp = new Bitmap(data);
            }
            catch (Exception)
            {
                t.Failed = true;
                return;
            }

            LoadTextureInternal(t, bmp);
            bmp.Dispose();
        }

        public override void LoadTextureRaw(Texture t, byte[] pixelData)
        {
            Bitmap bmp;
            try
            {
                unsafe
                {
                    fixed (byte* ptr = &pixelData[0])
                        bmp = new Bitmap(t.Width, t.Height, 4 * t.Width, System.Drawing.Imaging.PixelFormat.Format32bppArgb, (IntPtr)ptr);
                }
            }
            catch (Exception)
            {
                t.Failed = true;
                return;
            }

            int glTex;

            // Create the opengl texture
            GL.GenTextures(1, out glTex);
            GL.BindTexture(TextureTarget.Texture2D, glTex);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // Sort out our GWEN texture
            t.RendererData = glTex;

            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, t.Width, t.Height, 0, global::OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bmp.UnlockBits(data);
            bmp.Dispose();
        }

        public override void FreeTexture(Texture t)
        {
            int tex = (int)t.RendererData;
            if (tex == 0)
                return;
            GL.DeleteTextures(1, ref tex);
            t.RendererData = null;
        }

        public override unsafe Color PixelColor(Texture texture, uint x, uint y, Color defaultColor)
        {
            int tex = (int)texture.RendererData;
            if (tex == 0)
                return defaultColor;

            Color pixel;
            GL.BindTexture(TextureTarget.Texture2D, tex);
            long offset = 4 * (x + y * texture.Width);
            byte[] data = new byte[4 * texture.Width * texture.Height];
            fixed (byte* ptr = &data[0])
            {
                //GL.GetTexImage(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, PixelType.UnsignedByte, (IntPtr)ptr);
                GL.GetTexImage(TextureTarget.Texture2D, 0, global::OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr)ptr);
                pixel = Color.FromArgb(data[offset + 3], data[offset + 0], data[offset + 1], data[offset + 2]);
            }
            // Retrieving the entire texture for a single pixel read
            // is kind of a waste - maybe cache this pointer in the texture
            // data and then release later on? It's never called during runtime
            // - only during initialization.
            return pixel;
        }
    }
}
