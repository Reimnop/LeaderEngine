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
        -0.5f, -0.5f, 0.0f, 0.0f, 0.0f,
         0.5f, -0.5f, 0.0f, 1.0f, 0.0f,
         0.0f,  0.5f, 0.0f, 0.5f, 1.0f
    };

    uint[] indices = new uint[] {
        0, 1, 2
    };

    string vertexShaderSource = "#version 330 core\n" +
"layout (location = 0) in vec3 aPos;\n" +
"layout (location = 1) in vec2 texCoord;\n" +
"out vec2 coord;\n" +
"void main()\n" +
"{\n" +
"   gl_Position = vec4(aPos.x, aPos.y, aPos.z, 1.0);\n" +
"   coord = texCoord;\n" +
"}";

    string fragmentShaderSource = "#version 330 core\n" +
"in vec2 coord;\n" +
"uniform sampler2D texture0;\n" +
"out vec4 FragColor;\n" +
"void main()\n" +
"{\n" +
"    FragColor = texture(texture0, coord);\n" +
"}";

    void load()
    {
        Texture tex = new Texture().FromFile("tex.jpg");
        VertexArray vertexArray = new VertexArray(vertices, indices, new VertexAttrib[] {
            new VertexAttrib { location = 0, size = 3 },
            new VertexAttrib { location = 1, size = 2 }
        });
        Shader shader = new Shader(vertexShaderSource, fragmentShaderSource);
        

        GameObject go = new GameObject("test");
        go.AddComponent<MeshFilter>(vertexArray);
        go.AddComponent<MeshRenderer>(shader, tex);
    }
}
