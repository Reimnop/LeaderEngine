using LeaderEngine;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;

class Program
{
    static void Main(string[] args)
    {
        new Application(new GameWindowSettings(), new NativeWindowSettings()
        {
            APIVersion = new Version(4, 0),
            WindowBorder = WindowBorder.Fixed,
            API = ContextAPI.OpenGL,
            Flags = ContextFlags.ForwardCompatible,
            Profile = ContextProfile.Core,
            Size = new Vector2i(1600, 900)
        }, new Program().load).Run();
    }

    float[] vertices = new float[] {
        -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
         0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
         0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
         0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
         0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
         0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
         0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
        -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

        -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
        -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
        -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

         0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
         0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
         0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
         0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
         0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
         0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
         0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
         0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
         0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
         0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
         0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
         0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
        -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f
    };

    uint[] indices = new uint[] {
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29
    };

    string vertexShaderSource = "#version 330 core\n" +
        "layout (location = 0) in vec3 aPos;\n" +
        "layout (location = 1) in vec2 aTexCoord;\n" +
        "out vec2 TexCoord;\n" +
        "uniform mat4 mvp;" +
        "void main()\n" +
        "{\n" +
        "   gl_Position = vec4(aPos, 1.0) * mvp;\n" +
        "   TexCoord = aTexCoord;\n" +
        "}";

    string fragmentShaderSource = "#version 330 core\n" +
        "uniform sampler2D texture0;\n" +
        "in vec2 TexCoord;\n" +
        "out vec4 FragColor;\n" +
        "void main()\n" +
        "{\n" +
        "    FragColor = texture(texture0, TexCoord);\n" +
        "}";

    void load()
    {
        Texture tex = new Texture().FromFile("bricks.png");
        VertexArray vertexArray = new VertexArray(vertices, indices, new VertexAttrib[] {
            new VertexAttrib { location = 0, size = 3 },
            new VertexAttrib { location = 1, size = 2 }
        });
        Shader shader = new Shader(vertexShaderSource, fragmentShaderSource);

        GameObject cam = new GameObject("camera");
        cam.AddComponent<Camera>();
        cam.AddComponent<InputManager>();
        cam.AddComponent<TestCamComp>();

        GameObject go = new GameObject("test");
        go.AddComponent<MeshFilter>(vertexArray);
        go.AddComponent<MeshRenderer>(shader).SetTexture(tex);

        go.transform.position.Z = 5.0f;

        GameObject go2 = new GameObject("test");
        go2.AddComponent<MeshFilter>(vertexArray);
        go2.AddComponent<MeshRenderer>(shader).SetTexture(tex);

        go2.transform.position.Z = -5.0f;

        GameObject go3 = new GameObject("test");
        go3.AddComponent<MeshFilter>(vertexArray);
        go3.AddComponent<MeshRenderer>(shader).SetTexture(tex);

        go3.transform.position.X = -5.0f;
        go3.transform.rotationEuler.Y = MathF.PI / 2.0f;

        GameObject go4 = new GameObject("test");
        go4.AddComponent<MeshFilter>(vertexArray);
        go4.AddComponent<MeshRenderer>(shader).SetTexture(tex);

        go4.transform.position.X = 5.0f;
        go4.transform.rotationEuler.Y = MathF.PI / 2.0f;
    }
}
