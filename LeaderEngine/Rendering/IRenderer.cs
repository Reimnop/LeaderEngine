using OpenTK.Mathematics;

namespace LeaderEngine
{
    public interface IRenderer
    {
        public void Render(Matrix4 view, Matrix4 projection);
    }
}
