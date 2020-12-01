using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LeaderEngine
{
    public enum Axis
    {
        Horizontal = 0,
        Vertical = 1
    }

    public class InputManager : Component
    {
        private static KeyboardState keyState;
        private static MouseState mouseState;

        public override void Update()
        {
            keyState = Application.instance.KeyboardState;
            mouseState = Application.instance.MouseState;
        }
        public static bool GetKeyDown(Keys key)
        {
            if (!Application.instance.IsFocused)
                return false;
            if (!keyState.WasKeyDown(key) && keyState.IsKeyDown(key))
                return true;
            return false;
        }
        public static bool GetKey(Keys key)
        {
            if (!Application.instance.IsFocused)
                return false;
            if (keyState.IsKeyDown(key))
                return true;
            return false;
        }
        public static float GetAxis(Axis axis)
        {
            switch (axis)
            {
                case Axis.Horizontal:
                    if (GetKey(Keys.D) || GetKey(Keys.Right))
                        return 1;
                    else if (GetKey(Keys.A) || GetKey(Keys.Left))
                        return -1;
                    break;
                case Axis.Vertical:
                    if (GetKey(Keys.W) || GetKey(Keys.Up))
                        return 1;
                    else if (GetKey(Keys.S) || GetKey(Keys.Down))
                        return -1;
                    break;
            }
            return 0;
        }
        public static bool GetKeyUp(Keys key)
        {
            if (!Application.instance.IsFocused)
                return false;
            if (!keyState.IsKeyDown(key) && keyState.WasKeyDown(key))
                return true;
            return false;
        }
    }
}
