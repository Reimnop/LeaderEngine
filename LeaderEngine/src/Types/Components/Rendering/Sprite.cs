using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class Sprite : EditorComponent
    {
        public Color4 Color = Color4.White;
        public Texture Texture;

        private Mesh mesh;

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

            mesh = new Mesh("SpriteMesh", vertices, indices, new VertexAttrib[] 
            {
                new VertexAttrib { location = 0, size = 3 },
                new VertexAttrib { location = 1, size = 2 }
            });
        }

        public override void OnRender()
        {
            if (Texture == null)
                return;

            Matrix4 model = transform.ModelMatrix;

            shader.SetMatrix4("mvp", model * RenderingGlobals.View * RenderingGlobals.Projection);
            shader.SetVector4("color", new Vector4(Color.R, Color.G, Color.B, Color.A));

            Texture.Use(TextureUnit.Texture0);

            shader.Use();
            mesh.Use();

            GL.DrawElements(PrimitiveType.Triangles, mesh.GetIndicesCount(), DrawElementsType.UnsignedInt, 0);
        }
    }
}
