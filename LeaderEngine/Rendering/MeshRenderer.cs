using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class MeshRenderer : Component, IRenderer, IShadowMapRenderer
    {
        public Mesh Mesh;
        public Material Material;
        public Shader Shader = DefaultShaders.Lit;

        private CommandBuffer shadowMapCmd = new CommandBuffer();
        private CommandBuffer mainCmd = new CommandBuffer();

        public void RenderShadowMap(in LightData lightData)
        {
            if (!Enabled)
                return;

            var shader = DefaultShaders.ShadowMap;

            shadowMapCmd.Clear();

            shadowMapCmd.BindShader(shader);
            shadowMapCmd.SetUniformMatrix4(shader, "mvp", BaseTransform.ModelMatrix * lightData.View * lightData.Projection);

            shadowMapCmd.BindMesh(Mesh);
            shadowMapCmd.DrawMesh(Mesh);

            Engine.Renderer.QueueCommandsShadowMap(shadowMapCmd);
        }

        public void Render(in RenderData renderData)
        {
            if (!Enabled)
                return;

            mainCmd.Clear();

            mainCmd.BindShader(Shader);
            mainCmd.SetUniformMatrix4(Shader, "model", BaseTransform.ModelMatrix);
            mainCmd.SetUniformMatrix4(Shader, "mvp", BaseTransform.ModelMatrix * renderData.View * renderData.Projection);

            mainCmd.SetUniformVector3(Shader, "camPos", Camera.Main.BaseTransform.Position);

            if (DirectionalLight.Main != null)
            {
                mainCmd.SetUniformFloat(Shader, "lightIntensity", DirectionalLight.Main.Intensity);
                mainCmd.SetUniformVector3(Shader, "lightDir", -DirectionalLight.Main.BaseTransform.Forward);
            }

            mainCmd.SetUniformMatrix4(Shader, "lightSpaceMat", renderData.LightView * renderData.LightProjection);

            mainCmd.SetUniformInt(Shader, "shadowMap", 1);
            mainCmd.BindTexture(TextureUnit.Texture1, renderData.ShadowMapTexture);

            mainCmd.BindMaterial(Material, Shader);

            mainCmd.BindMesh(Mesh);
            mainCmd.DrawMesh(Mesh);

            Engine.Renderer.QueueCommandsOpaque(mainCmd);
        }
    }
}
