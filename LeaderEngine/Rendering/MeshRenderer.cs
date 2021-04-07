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

            uniforms.SetUniform("model", new Uniform(UniformType.Matrix4,
                BaseTransform.ModelMatrix));

            uniforms.SetUniform("view", new Uniform(UniformType.Matrix4,
                renderer.WorldView));

            uniforms.SetUniform("projection", new Uniform(UniformType.Matrix4,
                renderer.WorldProjection));

            uniforms.SetUniform("mvp", new Uniform(UniformType.Matrix4,
                BaseTransform.ModelMatrix
                * renderer.WorldView
                * renderer.WorldProjection));

            uniforms.SetUniform("camPos", new Uniform(UniformType.Vector3,
                Camera.Main.BaseTransform.Position));

            Engine.Renderer.PushDrawData(DrawType.Opaque, new GLDrawData
            {
                Mesh = Mesh,
                Material = Material,
                Uniforms = uniforms
            });
        }
    }
}
