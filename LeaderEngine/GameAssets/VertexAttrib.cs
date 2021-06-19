using OpenTK.Graphics.OpenGL4;

namespace LeaderEngine
{
    public struct VertexAttrib
    {
        public VertexAttribPointerType PointerType;
        public int Size;
        public bool Normalized;

        public VertexAttrib(VertexAttribPointerType pointerType, int size, bool normalized)
        {
            PointerType = pointerType;
            Size = size;
            Normalized = normalized;
        }
    }
}
