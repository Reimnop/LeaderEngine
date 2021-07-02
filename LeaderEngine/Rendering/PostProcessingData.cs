using OpenTK.Mathematics;

namespace LeaderEngine
{
    public struct PostProcessingData
    {
        public Matrix4 View;
        public Matrix4 Projection;

        public int ColorTexture;
        public int DepthTexture;
        public int AlbedoTexture;
        public int PositionTexture;
        public int NormalTexture;
    }
}
