using OpenTK.Mathematics;

namespace LeaderEngine
{
    public enum DrawType
    {
        Opaque,
        Transparent,
        GUI
    }

    public struct GLDrawData
    {
        public Mesh Mesh;
        public Shader Shader;
        public UniformData Uniforms;
        public Material Material;
    }

    public abstract class GLRenderer
    {
        public Vector2i ViewportSize = new Vector2i(1600, 900);

        public abstract void Init();
        public abstract void PushDrawData(DrawType drawType, GLDrawData drawData);
        public abstract void Update();
        public abstract void Render();
    }
}
