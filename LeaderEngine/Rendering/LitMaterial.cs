using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;
using System.Text;

namespace LeaderEngine
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LitMaterial
    {
        public Vector3 Color;

        public int HasDiffuse;

        public long DiffuseTexture;
    }
}
