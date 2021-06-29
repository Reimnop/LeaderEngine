using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;

namespace LeaderEngine
{
    public static class FramebufferManager
    {
        private static Stack<int> framebufferStack = new Stack<int>();

        public static void PushFramebuffer(int framebuffer)
        {
            framebufferStack.Push(framebuffer);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
        }

        public static void PopFramebuffer()
        {
            framebufferStack.Pop();

            int framebuffer = 0;
            if (framebufferStack.Count > 0)
                framebuffer = framebufferStack.Peek();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
        }
    }
}
