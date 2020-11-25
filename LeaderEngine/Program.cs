using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;

class Program
{
    static void Main(string[] args)
    {
        new LeaderEngine.Application(new GameWindowSettings(), new NativeWindowSettings()
        {
            APIVersion = new Version(4, 0),
            WindowBorder = WindowBorder.Fixed,
            API = ContextAPI.OpenGL,
            Flags = ContextFlags.ForwardCompatible,
            Profile = ContextProfile.Core,
            Size = new Vector2i(1600, 900)
        }, null);
    }
}
