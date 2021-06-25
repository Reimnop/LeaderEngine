using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace LeaderEngine
{
    public static class TextureHelper
    {
        private readonly static Dictionary<PixelInternalFormat, int> internalFormatSize = new Dictionary<PixelInternalFormat, int>()
        {
            { (PixelInternalFormat)All.Red, 1 },
            { PixelInternalFormat.Rgba, 4 },
            { PixelInternalFormat.SrgbAlpha, 4 }
        };

        private readonly static Dictionary<PixelType, int> typeSize = new Dictionary<PixelType, int>()
        {
            { PixelType.UnsignedByte, 1 },
            { PixelType.Float, sizeof(float) }
        };

        public static int GetSinglePixelSize(PixelInternalFormat pixelInternalFormat, PixelType pixelType)
        {
            int iSize, tSize;

            if (!internalFormatSize.TryGetValue(pixelInternalFormat, out iSize))
                throw new NotImplementedException();

            if (!typeSize.TryGetValue(pixelType, out tSize))
                throw new NotImplementedException();

            return iSize * tSize;
        }
    }
}
