using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace LeaderEngine
{
    public static class IM
    {
        private static int VBO, EBO, VAO;

        private static PrimitiveType pType;

        private static List<float> vertList = new List<float>();

        private static Color4 currentColor = Color4.White;

        private static Shader imShader = Shader.ImmediateMode;

        public static void Init()
        {
            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public static void Begin(PrimitiveType primitiveType)
        {
            pType = primitiveType;
        }

        public static void Color(Color4 color)
        {
            currentColor = color;
        }

        public static void Color(float R, float G, float B)
        {
            currentColor = new Color4(R, G, B, 1.0f);
        }

        public static void Color(float R, float G, float B, float A)
        {
            currentColor = new Color4(R, G, B, A);
        }

        public static void Vertex3(Vector3 vert)
        {
            vertList.Add(vert.X);
            vertList.Add(vert.Y);
            vertList.Add(vert.Z);

            vertList.Add(currentColor.R);
            vertList.Add(currentColor.G);
            vertList.Add(currentColor.B);
            vertList.Add(currentColor.A);
        }

        public static void Vertex3(float X, float Y, float Z)
        {
            vertList.Add(X);
            vertList.Add(Y);
            vertList.Add(Z);

            vertList.Add(currentColor.R);
            vertList.Add(currentColor.G);
            vertList.Add(currentColor.B);
            vertList.Add(currentColor.A);
        }

        public static void End()
        {
            float[] vertArray = vertList.ToArray();
            vertList.Clear();

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertArray.Length * sizeof(float), vertArray, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            imShader.Use();
            imShader.SetMatrix4("mvp", RenderingGlobals.View * RenderingGlobals.Projection);

            GL.BindVertexArray(VAO);
            GL.DrawArrays(pType, 0, vertArray.Length);
        }
    }
}
