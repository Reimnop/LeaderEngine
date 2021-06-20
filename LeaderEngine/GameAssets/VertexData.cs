using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LeaderEngine
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexData
    {
        [VertexAttribAttribute(VertexAttribPointerType.Float, 3, false)]
        public Vector3 Normal;

        [VertexAttribAttribute(VertexAttribPointerType.Float, 3, false)]
        public Vector3 Color;

        [VertexAttribAttribute(VertexAttribPointerType.Float, 2, false)]
        public Vector2 UV;
    }
}
