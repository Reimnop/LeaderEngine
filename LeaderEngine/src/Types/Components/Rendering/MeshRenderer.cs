using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class MeshRenderer : EditorComponent
    {
        public MeshFilter MeshFilter;

        public Material Material;

        public override void EditorStart()
        {
            MeshFilter meshFilter = BaseEntity.GetComponent<MeshFilter>();

            if (meshFilter == null)
                meshFilter = BaseEntity.AddComponent<MeshFilter>();

            MeshFilter = meshFilter;
        }

        public override void OnRender()
        {
            if (MeshFilter == null)
                return;

            if (MeshFilter.Mesh == null)
                return;

            Matrix4 model = BaseTransform.ModelMatrix;

            Material renderMat = Material;
            if (Material == null)
                renderMat = Material.NoRender;

            if (RenderingGlobals.CurrentPass == RenderPass.Lighting)
                renderMat = Material.DepthOnly;
            else if (BaseEntity.RenderHint == RenderHint.Transparent)
                renderMat.Shader = Shader.Transparent;
            else
                renderMat.Shader = Shader.Lit;

            if (RenderingGlobals.CurrentPass != RenderPass.Lighting)
                LightingController.LightingShaderSetup(renderMat.Shader, model);

            renderMat.Use();
            renderMat.Shader.SetMatrix4("mvp", model * RenderingGlobals.View * RenderingGlobals.Projection);

            MeshFilter.Mesh.Use();

            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Cw);
            GL.CullFace(CullFaceMode.Back);

            GL.DrawElements(PrimitiveType.Triangles, MeshFilter.Mesh.GetIndicesCount(), DrawElementsType.UnsignedInt, 0);
        }
    }
}
