using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class Sprite : EditorComponent
    {
        public Color4 Color = Color4.White;
        public Texture Texture;

        private VertexArray vertexArray;

        private Shader shader = Shader.SpriteShader;

        public override void EditorStart()
        {
            float[] vertices =
            {
                -0.5f, -0.5f, 0.0f, 0.0f, 1.0f,
                -0.5f,  0.5f, 0.0f, 0.0f, 0.0f,
                 0.5f,  0.5f, 0.0f, 1.0f, 0.0f,
                 0.5f, -0.5f, 0.0f, 1.0f, 1.0f
            };

            uint[] indices =
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

        public override void OnRender()
        {
            if (Texture == null)
                return;

            Matrix4 model = Matrix4.CreateScale(transform.Scale)
                 * Matrix4.CreateFromQuaternion(transform.Rotation)
                 * Matrix4.CreateTranslation(transform.Position + RenderingGlobals.GlobalPosition);

            shader.SetMatrix4("mvp", model * RenderingGlobals.View * RenderingGlobals.Projection);
            shader.SetVector4("color", new Vector4(Color.R, Color.G, Color.B, Color.A));

            Texture.Use(TextureUnit.Texture0);

            shader.Use();
            vertexArray.Use();

            GL.DrawElements(PrimitiveType.Triangles, vertexArray.GetIndicesCount(), DrawElementsType.UnsignedInt, 0);
        }
    }
}
