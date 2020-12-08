using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class MeshRenderer : Component
    {
        private Material material;
        private Texture texture;
        private MeshFilter meshFilter;

        public MeshRenderer SetTexture(Texture texture)
        {
            this.texture = texture;
            return this;
        }

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
            Matrix4 model = Matrix4.CreateScale(gameObject.transform.scale)
                 * Matrix4.CreateFromQuaternion(gameObject.transform.rotation)
                 * Matrix4.CreateTranslation(gameObject.transform.position);

            Material renderMat = material;
            if (material == null)
                renderMat = Material.NoRender;

            renderMat.SetMatrix4("mvp", model * RenderingGlobals.View * RenderingGlobals.Projection);

            renderMat.Use();
            meshFilter.VertexArray.Use();
            texture?.Use(TextureUnit.Texture0);

            GL.DrawElements(PrimitiveType.Triangles, meshFilter.VertexArray.GetIndicesCount(), DrawElementsType.UnsignedInt, 0);
        }
    }
}
