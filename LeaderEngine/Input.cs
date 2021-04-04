using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LeaderEngine
{
    public enum Axis
    {
        Horizontal = 0,
        Vertical = 1
    }

    public static class Input
    {
        private static KeyboardState keyState;
        private static MouseState mouseState;

        public static void Init(KeyboardState ks, MouseState ms)
        {
            keyState = ks;
            mouseState = ms;

            Logger.Log("Input initialized.");
        }

        public static Vector2 GetMouseDelta()
        {
            return mouseState.Delta;
        }

        public static Vector2 GetMousePosition()
        {
            return mouseState.Position;
        }

        public static bool GetMouseDown(MouseButton button)
        {
            if (!Engine.MainWindow.IsFocused)
                return false;
            if (!mouseState.WasButtonDown(button) && mouseState.IsButtonDown(button))
                return true;
            return false;
        }
        public static bool GetMouse(MouseButton button)
        {
            if (!Engine.MainWindow.IsFocused)
                return false;
            return mouseState.IsButtonDown(button);
        }
        public static bool GetMouseUp(MouseButton button)
        {
            if (!Engine.MainWindow.IsFocused)
                return false;
            if (!mouseState.IsButtonDown(button) && mouseState.WasButtonDown(button))
                return true;
            return false;
        }
        public static bool GetKeyDown(Keys key)
        {
            if (!Engine.MainWindow.IsFocused)
                return false;
            if (!keyState.WasKeyDown(key) && keyState.IsKeyDown(key))
                return true;
            return false;
        }
        public static bool GetKey(Keys key)
        {
            if (!Engine.MainWindow.IsFocused)
                return false;
            return keyState.IsKeyDown(key);
        }
        public static bool GetKeyUp(Keys key)
        {
            if (!Engine.MainWindow.IsFocused)
                return false;
            if (!keyState.IsKeyDown(key) && keyState.WasKeyDown(key))
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
        public static bool GetKeyCombo(params Keys[] keys)
        {
            bool anyKey = false;
            for (int i = 0; i < keys.Length; i++)
                anyKey = anyKey || GetKeyDown(keys[i]);

            bool pressed = true;
            for (int i = 0; i < keys.Length; i++)
                pressed = pressed && GetKey(keys[i]);

            return pressed && anyKey;
        }
    }
}
