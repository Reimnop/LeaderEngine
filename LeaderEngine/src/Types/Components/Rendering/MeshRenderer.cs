using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class MeshRenderer : EditorComponent
    {
        private Material material = Material.Lit;
        private MeshFilter meshFilter;

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

            this.meshFilter = meshFilter;
        }

        public override void OnRender()
        {
            if (meshFilter.Mesh == null)
                return;

            Matrix4 model = Matrix4.CreateScale(transform.Scale)
                 * Matrix4.CreateFromQuaternion(transform.Rotation)
                 * Matrix4.CreateTranslation(transform.Position + RenderingGlobals.GlobalPosition);

            Material renderMat = material;
            if (material == null)
                renderMat = Material.NoRender;

            renderMat.SetMatrix4("mvp", model * RenderingGlobals.View * RenderingGlobals.Projection);

            var vertArrays = meshFilter.Mesh.GetAllVertexArrays();

            foreach (var vertArray in vertArrays)
            {
                GL.Enable(EnableCap.CullFace);
                GL.FrontFace(FrontFaceDirection.Cw);
                GL.CullFace(CullFaceMode.Back);

                vertArray.Use();

                Texture texture = vertArray.GetTexture();

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

                renderMat.Use();

                if (RenderingGlobals.CurrentPass != RenderPass.Lighting)
                    LightingController.LightingShaderSetup(renderMat.Shader, transform.Position + RenderingGlobals.GlobalPosition, transform.Rotation, transform.Scale);

                GL.DrawElements(PrimitiveType.Triangles, vertArray.GetIndicesCount(), DrawElementsType.UnsignedInt, 0);
            }
        }
    }
}
