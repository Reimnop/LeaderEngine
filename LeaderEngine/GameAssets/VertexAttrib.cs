using OpenTK.Graphics.OpenGL4;
using System;

namespace LeaderEngine
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class VertexAttrib : Attribute
    {
        public VertexAttribPointerType PointerType;
        public int Location;
        public int Size;
        public bool Normalized;

        public VertexAttrib(VertexAttribPointerType pointerType, int location, int size, bool normalized)
        {
            if (location == 0)
                throw new Exception("Location cannot be 0!");

            PointerType = pointerType;
            Location = location;
            Normalized = normalized;
            Size = size;
        }
    }
}
