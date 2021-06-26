using OpenTK.Mathematics;

namespace LeaderEngine
{
    public struct RenderData
    {
        public Matrix4 View;
        public Matrix4 Projection;
        public Matrix4 ViewProjection;
        public int CascadeCount;
        public float[] CascadeDepths;
        public int[] CascadeShadowMaps;
        public Matrix4[] CascadeViewProjections;
    }

    public interface IRenderer
    {
        public void Render(in RenderData renderData);
    }
}
