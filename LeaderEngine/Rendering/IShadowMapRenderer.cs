using OpenTK.Mathematics;

namespace LeaderEngine
{
    public struct LightData
    {
        public Matrix4 View;
        public Matrix4 Projection;
    }

    public interface IShadowMapRenderer
    {
        public void RenderShadowMap(in LightData lightData);
    }
}
