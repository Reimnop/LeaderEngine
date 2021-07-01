using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace LeaderEngine
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexData
    {
        [VertexAttrib(VertexAttribPointerType.Float, 3, false)]
        public Vector3 Normal;

        [VertexAttrib(VertexAttribPointerType.Float, 3, false)]
        public Vector3 Color;

        [VertexAttrib(VertexAttribPointerType.Float, 2, false)]
        public Vector2 UV;

        [VertexAttrib(VertexAttribPointerType.Float, 3, false)]
        public Vector3 Tangent;
    }
}
