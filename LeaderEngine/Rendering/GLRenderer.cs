using System;
using System.Collections.Generic;
using System.Text;

namespace LeaderEngine.Rendering
{
    public enum DrawType
    {
        Opaque,
        Transparent
    }

    public struct GLDrawData
    {

    }

    public abstract class GLRenderer
    {
        public abstract void Init();
        public abstract void PushDrawData(DrawType drawType, GLDrawData drawData);
        public abstract void Render();
    }
}
