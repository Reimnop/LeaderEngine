using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace LeaderEngine
{
    public class MeshRenderer : Component
    {
        private Material material = Material.Lit;
        private MeshFilter meshFilter;

        public MeshRenderer SetMaterial(Material material)
        {
            this.material = material;
            return this;
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
            if (meshFilter.Mesh == null)
                return;

            GL.FrontFace(FrontFaceDirection.Cw);
            GL.CullFace(CullFaceMode.Back);

            Matrix4 model = Matrix4.CreateScale(gameObject.Transform.Scale)
                 * Matrix4.CreateFromQuaternion(gameObject.Transform.Rotation)
                 * Matrix4.CreateTranslation(gameObject.Transform.Position);

            Material renderMat = material;
            if (material == null)
                renderMat = Material.NoRender;

            renderMat.SetMatrix4("mvp", model * RenderingGlobals.View * RenderingGlobals.Projection);

            var vertArrays = meshFilter.Mesh.GetAllVertexArrays();

            foreach (var vertArray in vertArrays)
            {
                Texture texture = vertArray.GetTexture();

                vertArray.Use();

                if (texture != null)
                {
                    renderMat.SetInt("useTexture", 1);
                    texture.Use(TextureUnit.Texture0);
                }
                else
                {
                    renderMat.SetInt("useTexture", 0);
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                }

                renderMat.SetMatrix4("model", model);

                renderMat.Use();

                GL.DrawElements(PrimitiveType.Triangles, vertArray.GetIndicesCount(), DrawElementsType.UnsignedInt, 0);
            }
        }
    }
}
