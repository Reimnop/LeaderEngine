using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

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
            shadowMapCmd.SetUniformMatrix4(shader, "mvp", BaseTransform.GlobalModelMatrix * lightData.ViewProjection);

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

            Matrix4 globalModel = BaseTransform.GlobalModelMatrix;
            mainCmd.SetUniformMatrix4(shader, "model", globalModel);
            mainCmd.SetUniformMatrix4(shader, "mvp", globalModel * renderData.ViewProjection);

            mainCmd.SetUniformVector3(shader, "camPos", Camera.Main.BaseTransform.Position);

            if (DirectionalLight.Main != null)
            {
                mainCmd.SetUniformFloat(shader, "lightIntensity", DirectionalLight.Main.Intensity);
                mainCmd.SetUniformVector3(shader, "lightDir", -DirectionalLight.Main.BaseTransform.Forward);
            }

            mainCmd.SetUniformInt(shader, "cascadeCount", renderData.CascadeCount);

            for (int i = 0; i < renderData.CascadeCount; i++)
            {
                mainCmd.SetUniformFloat(shader.GetAttribLocation("cascadeDepths[0]") + i, renderData.CascadeDepths[i]);
                mainCmd.SetUniformMatrix4(shader.GetAttribLocation("cascadeViewProjs[0]") + i, renderData.CascadeViewProjections[i]);

                mainCmd.SetUniformInt(shader.GetAttribLocation("cascadeShadowMaps[0]") + i, i);
                mainCmd.BindTexture(TextureUnit.Texture0 + i, renderData.CascadeShadowMaps[i]);
            }

            mainCmd.SetUniformFloat(shader.GetAttribLocation("cascadeDepths[0]") + renderData.CascadeCount, renderData.CascadeDepths[renderData.CascadeCount]);

            mainCmd.BindMaterial(0, Material);

            mainCmd.BindMesh(Mesh);
            mainCmd.DrawMesh(Mesh);

            Engine.Renderer.QueueCommandsOpaque(mainCmd);
        }
    }
}
