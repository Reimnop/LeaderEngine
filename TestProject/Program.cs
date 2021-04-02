using LeaderEngine;

namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Engine.Init(new OpenTK.Windowing.Desktop.GameWindowSettings(), new OpenTK.Windowing.Desktop.NativeWindowSettings()
            {

            });
        }
    }
}
