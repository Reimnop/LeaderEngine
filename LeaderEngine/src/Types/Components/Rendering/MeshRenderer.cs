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
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();

            MeshFilter = meshFilter;
        }

        public override void OnRender()
        {
            if (MeshFilter == null)
                return;

            if (MeshFilter.Mesh == null)
                return;

            Matrix4 model = transform.ModelMatrix;

            Material renderMat = Material;
            if (Material == null)
                renderMat = Material.NoRender;

            if (RenderingGlobals.CurrentPass == RenderPass.Lighting)
                renderMat = Material.DepthOnly;

            if (RenderingGlobals.CurrentPass != RenderPass.Lighting)
                LightingController.LightingShaderSetup(renderMat, model);

            renderMat.Use();
            renderMat.Shader.SetMatrix4("mvp", model * RenderingGlobals.View * RenderingGlobals.Projection);
            renderMat.Shader.SetMatrix4("modelWorldSpace", model);

            MeshFilter.Mesh.Use();

            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Cw);
            GL.CullFace(CullFaceMode.Back);

            GL.DrawElements(PrimitiveType.Triangles, MeshFilter.Mesh.GetIndicesCount(), DrawElementsType.UnsignedInt, 0);
        }
    }
}
