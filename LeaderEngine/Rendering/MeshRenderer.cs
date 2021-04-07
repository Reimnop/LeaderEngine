namespace LeaderEngine
{
    public class MeshRenderer : Renderer
    {
        public Mesh Mesh;
        public Material Material;

        private UniformData uniforms = new UniformData();

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

            uniforms.SetUniform("mvp", new Uniform(UniformType.Matrix4,
                BaseTransform.ModelMatrix
                * renderer.WorldView
                * renderer.WorldProjection));

            Engine.Renderer.PushDrawData(DrawType.Opaque, new GLDrawData
            {
                Mesh = Mesh,
                Material = Material
            });
        }
    }
}
