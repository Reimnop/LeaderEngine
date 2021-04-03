namespace LeaderEngine
{
    public enum DrawType
    {
        Opaque,
        Transparent
    }

    public struct GLDrawData
    {
        public Mesh Mesh;
        public Shader Shader;
        public Texture Texture;
    }

    public abstract class GLRenderer
    {
        public abstract void Init();
        public abstract void PushDrawData(DrawType drawType, GLDrawData drawData);
        public abstract void Render();
    }
}
