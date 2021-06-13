using OpenTK.Graphics.OpenGL4;

namespace LeaderEngine
{
    public class MeshRenderer : Component, IRenderer, IShadowMapRenderer
    {
        public Mesh Mesh;
        public Material Material;

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

            Shader shader = Material.Shader;

            mainCmd.Clear();

            mainCmd.BindShader(shader);
            mainCmd.SetUniformMatrix4(shader, "model", BaseTransform.ModelMatrix);
            mainCmd.SetUniformMatrix4(shader, "mvp", BaseTransform.ModelMatrix * renderData.View * renderData.Projection);

            mainCmd.SetUniformVector3(shader, "camPos", Camera.Main.BaseTransform.Position);

            if (DirectionalLight.Main != null)
            {
                mainCmd.SetUniformFloat(shader, "lightIntensity", DirectionalLight.Main.Intensity);
                mainCmd.SetUniformVector3(shader, "lightDir", -DirectionalLight.Main.BaseTransform.Forward);
            }

            mainCmd.SetUniformMatrix4(shader, "lightSpaceMat", renderData.LightView * renderData.LightProjection);

            mainCmd.SetUniformInt(shader, "shadowMap", 1);
            mainCmd.BindTexture(TextureUnit.Texture1, renderData.ShadowMapTexture);

            mainCmd.BindMaterial(0, Material);

            mainCmd.BindMesh(Mesh);
            mainCmd.DrawMesh(Mesh);

            Engine.Renderer.QueueCommandsOpaque(mainCmd);
        }
    }
}
