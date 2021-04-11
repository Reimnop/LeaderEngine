using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class MeshRenderer : Component, IRenderer, IShadowMapRenderer
    {
        public Mesh Mesh;
        public Material Material;

        private UniformData uniforms = new UniformData();

        public Shader Shader = DefaultShaders.Lit;

        private void Start()
        {
            BaseEntity.Renderers.Add(this);
        }

        private void OnRemove()
        {
            BaseEntity.Renderers.Remove(this);
        }

        public void RenderShadowMap(Matrix4 view, Matrix4 projection)
        {
            throw new System.NotImplementedException();
        }

        public void Render(Matrix4 view, Matrix4 projection)
        {
            GLRenderer renderer = Engine.Renderer;

            uniforms.SetUniform("model", new Uniform(UniformType.Matrix4,
                BaseTransform.ModelMatrix));

            uniforms.SetUniform("view", new Uniform(UniformType.Matrix4, view));

            uniforms.SetUniform("projection", new Uniform(UniformType.Matrix4, projection));

            uniforms.SetUniform("mvp", new Uniform(UniformType.Matrix4,
                BaseTransform.ModelMatrix
                * view * projection));

            uniforms.SetUniform("camPos", new Uniform(UniformType.Vector3,
                Camera.Main.BaseTransform.Position));

            renderer.PushDrawData(DrawType.Opaque, new GLDrawData
            {
                Mesh = Mesh,
                Shader = Shader,
                Material = Material,
                Uniforms = uniforms
            });
        }
    }
}
