using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeaderEngine
{
    public class MeshRenderer : Component
    {
        private Shader shader;
        private Texture texture;
        private MeshFilter meshFilter;

        public MeshRenderer(Shader shader)
        {
            this.shader = shader;
        }

        public void SetTexture(Texture texture)
        {
            this.texture = texture;
        }

        public override void Start()
        {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();

            this.meshFilter = meshFilter;
        }

        public override void OnRender()
        {
            Matrix4 model = Matrix4.CreateScale(gameObject.transform.scale)
                 * Matrix4.CreateFromQuaternion(gameObject.transform.rotation) 
                 * Matrix4.CreateTranslation(gameObject.transform.position);

            shader.SetMatrix4("mvp", model * Camera.main.ViewMatrix * Camera.main.ProjectionMatrix);

            shader.Use();
            meshFilter.VertexArray.Use();
            texture?.Use(TextureUnit.Texture0);

            GL.DrawElements(PrimitiveType.Triangles, meshFilter.VertexArray.GetIndicesCount(), DrawElementsType.UnsignedInt, 0);
        }
    }
}
