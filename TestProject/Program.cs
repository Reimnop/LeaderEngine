using LeaderEngine;
using OpenTK.Windowing.Desktop;

namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Engine.Init(new GameWindowSettings(), new NativeWindowSettings()
            {
                
            });
        }
    }
}
