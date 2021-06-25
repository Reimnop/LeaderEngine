using OpenTK.Graphics.OpenGL4;
using System;

namespace LeaderEngine
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class VertexAttribAttribute : Attribute
    {
        public VertexAttribPointerType PointerType;
        public int Size;
        public bool Normalized;

        public VertexAttribAttribute(VertexAttribPointerType pointerType, int size, bool normalized)
        {
            PointerType = pointerType;
            Normalized = normalized;
            Size = size;
        }
    }
}
