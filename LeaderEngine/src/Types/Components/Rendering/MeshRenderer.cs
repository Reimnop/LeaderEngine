using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class MeshRenderer : EditorComponent
    {
        public MeshFilter MeshFilter;

        private Material material = Material.Lit;

        public MeshRenderer SetMaterial(Material material)
        {
            this.material = material;
            return this;
        }

        public override void EditorStart()
        {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();

            this.MeshFilter = meshFilter;
        }

        public override void OnRender()
        {
            if (MeshFilter == null)
                return;

            if (MeshFilter.Mesh == null)
                return;

            Matrix4 model = Matrix4.CreateScale(transform.Scale)
                 * Matrix4.CreateFromQuaternion(transform.Rotation)
                 * Matrix4.CreateTranslation(transform.Position + RenderingGlobals.GlobalPosition);

            Material renderMat = material;
            if (material == null)
                renderMat = Material.NoRender;

            if (RenderingGlobals.CurrentPass == RenderPass.Lighting)
                renderMat = Material.DepthOnly;

            renderMat.SetMatrix4("mvp", model * RenderingGlobals.View * RenderingGlobals.Projection);

            var vertArrays = MeshFilter.Mesh.GetAllVertexArrays();

            foreach (var vertArray in vertArrays)
            {
                GL.Enable(EnableCap.CullFace);
                GL.FrontFace(FrontFaceDirection.Cw);
                GL.CullFace(CullFaceMode.Back);

                vertArray.Use();

                if (RenderingGlobals.CurrentPass != RenderPass.Lighting)
                {
                    Texture texture = vertArray.GetTexture();

                    if (texture != null)
                        renderMat.SetInt("useTexture", 1);
                    else
                        renderMat.SetInt("useTexture", 0);

                    renderMat.SetInt("texture0", 0);
                    texture?.Use(TextureUnit.Texture0);

                    LightingController.LightingShaderSetup(renderMat, transform.Position + RenderingGlobals.GlobalPosition, transform.Rotation, transform.Scale);
                }

                renderMat.Use();

                GL.DrawElements(PrimitiveType.Triangles, vertArray.GetIndicesCount(), DrawElementsType.UnsignedInt, 0);
            }
        }
    }
}
