using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class MeshRenderer : Component, IRenderer, IShadowMapRenderer
    {
        public Mesh Mesh;
        public Material Material;
        public Shader Shader = DefaultShaders.Lit;

        private UniformData shadowMapUniforms = new UniformData();
        private UniformData uniforms = new UniformData();

        public void RenderShadowMap(Matrix4 view, Matrix4 projection)
        {
            if (!Enabled)
                return;

            shadowMapUniforms.SetUniform("mvp", new Uniform(UniformType.Matrix4,
                BaseTransform.ModelMatrix
                * view * projection));

            Engine.Renderer.PushDrawData(DrawType.ShadowMap, new GLDrawData
            {
                SourceEntity = BaseEntity,
                Mesh = Mesh,
                Shader = DefaultShaders.ShadowMap,
                Uniforms = shadowMapUniforms
            });
        }

        public void Render(Matrix4 view, Matrix4 projection)
        {
            if (!Enabled)
                return;

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

            if (DirectionalLight.Main != null)
                uniforms.SetUniform("lightIntensity", new Uniform(UniformType.Float,
                    DirectionalLight.Main.Intensity));

            if (DirectionalLight.Main != null)
                uniforms.SetUniform("lightDir", new Uniform(UniformType.Vector3,
                    -DirectionalLight.Main.BaseTransform.Forward));

            uniforms.SetUniform("lightSpaceMat", new Uniform(UniformType.Matrix4, LightingGlobals.LightView * LightingGlobals.LightProjection));

            uniforms.SetUniform("shadowMap", new Uniform(UniformType.Texture2D, new TextureData(TextureUnit.Texture1, LightingGlobals.ShadowMap)));

            renderer.PushDrawData(DrawType.Opaque, new GLDrawData
            {
                SourceEntity = BaseEntity,
                Mesh = Mesh,
                Shader = Shader,
                Material = Material,
                Uniforms = uniforms
            });
        }
    }
}
