using OpenTK.Mathematics;

namespace LeaderEngine
{
    public enum RenderPass
    {
        None,
        Lighting,
        World,
        Gui
    }

    public static class RenderingGlobals
    {
        public static Matrix4 View = Matrix4.Identity;
        public static Matrix4 Projection = Matrix4.Identity;

        public static Vector3 GlobalPosition = Vector3.Zero;

        public static Shader ForcedShader = null;

        public static RenderPass CurrentPass = RenderPass.None;

        public static bool RenderingEnabled = true;
    }
}
