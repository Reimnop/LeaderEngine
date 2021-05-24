using OpenTK.Mathematics;

namespace LeaderEngine
{
    public struct RenderData
    {
        public Matrix4 View;
        public Matrix4 Projection;
        public Matrix4 LightView;
        public Matrix4 LightProjection;
        public int ShadowMapTexture;
    }

    public interface IRenderer
    {
        public void Render(in RenderData renderData);
    }
}
