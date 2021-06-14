using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace LeaderEngine
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LitMaterial
    {
        public Vector3 Color;
        public bool HasDiffuse;
        public long DiffuseTexture;
    }
}
