using OpenTK.Mathematics;

namespace LeaderEngine
{
    public interface IShadowMapRenderer
    {
        public void RenderShadowMap(Matrix4 view, Matrix4 projection);
    }
}
