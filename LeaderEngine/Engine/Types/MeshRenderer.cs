using OpenTK.Graphics.OpenGL4;
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
            shader.Use();
            meshFilter.VertexArray.Use();
            texture?.Use(TextureUnit.Texture0);

            GL.DrawElements(PrimitiveType.Triangles, meshFilter.VertexArray.GetIndicesCount(), DrawElementsType.UnsignedInt, 0);
        }
    }
}
