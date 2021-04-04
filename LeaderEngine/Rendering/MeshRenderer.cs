using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class MeshRenderer : Renderer
    {
        public Mesh Mesh;
        public Material Material;

        private void Start()
        {
            BaseEntity.Renderers.Add(this);
        }

        private void OnRemove()
        {
            BaseEntity.Renderers.Remove(this);
        }

        public override void Render()
        {
            GLRenderer renderer = Engine.Renderer;

            Material.SetMatrix4("mvp", 
                BaseTransform.ModelMatrix 
                * renderer.WorldView 
                * renderer.WorldProjection);

            Engine.Renderer.PushDrawData(DrawType.Opaque, new GLDrawData
            {
                Mesh = Mesh,
                Material = Material
            });
        }
    }
}
