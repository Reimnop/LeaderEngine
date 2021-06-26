using OpenTK.Mathematics;

namespace LeaderEngine
{
    public struct LightData
    {
        public Matrix4 ViewProjection;
    }

    public interface IShadowMapRenderer
    {
        public void RenderShadowMap(in LightData lightData);
    }
}
