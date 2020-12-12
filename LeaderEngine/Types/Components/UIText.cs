using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace LeaderEngine
{
    public class UIText : Component
    {
        public string Text = "Text";

        public float Width = 250.0f;
        public float Height = 100.0f;

        private float lastW = 250.0f, lastH = 100.0f;
        private string lastText = "Text";

        private Texture textTexture;
        private Shader shader = Shader.TextShader;
        private VertexArray vertexArray;

        private void UpdateText()
        {
            Bitmap bmp = new Bitmap((int)Width, (int)Height);
            Graphics g = Graphics.FromImage(bmp);

            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            g.DrawString(Text, new Font("Times New Roman", 26), Brushes.White, new RectangleF(0.0f, 0.0f, Width, Height));

            g.Flush();

            textTexture?.Dispose();
            textTexture = new Texture().FromBitmap(bmp);

            float w = Width * 0.5f;
            float h = Height * 0.5f;

            float[] vertices = new float[]
            {
                -w, -h, 0.0f, 0.0f, 1.0f,
                -w,  h, 0.0f, 0.0f, 0.0f,
                 w,  h, 0.0f, 1.0f, 0.0f,
                 w, -h, 0.0f, 1.0f, 1.0f
            };

            uint[] indices = new uint[]
            {
                0, 1, 3,
                1, 2, 3
            };

            vertexArray = new VertexArray(vertices, indices, new VertexAttrib[]
            {
                new VertexAttrib { location = 0, size = 3 },
                new VertexAttrib { location = 1, size = 2 }
            });
        }

        private bool CheckChanged()
        {
            if (lastW != Width || lastH != Height || Text != lastText)
            {
                lastW = Width;
                lastH = Height;
                lastText = Text;

                return true;
            }
            return false;
        }

        public override void Start()
        {
            UpdateText();
        }

        public override void OnRenderGui()
        {
            if (CheckChanged())
                UpdateText();

            //TODO: transparency
            //GL.DepthMask(false);
            //GL.Enable(EnableCap.Blend);
            //GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            Matrix4 model = Matrix4.CreateScale(gameObject.Transform.Scale)
                 * Matrix4.CreateFromQuaternion(gameObject.Transform.Rotation)
                 * Matrix4.CreateTranslation(gameObject.Transform.Position);

            shader.SetMatrix4("mvp", model * RenderingGlobals.View * RenderingGlobals.Projection);

            textTexture.Use(TextureUnit.Texture0);

            shader.Use();
            vertexArray.Use();

            GL.DrawElements(PrimitiveType.Triangles, vertexArray.GetIndicesCount(), DrawElementsType.UnsignedInt, 0);

            //GL.DepthMask(true);
            //GL.Disable(EnableCap.Blend);
        }
    }
}
