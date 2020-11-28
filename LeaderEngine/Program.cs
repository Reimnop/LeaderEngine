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
        -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
         0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
         0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
         0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
        -0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
        -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,

        -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
         0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
         0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
         0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
        -0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
        -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,

        -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,
        -0.5f,  0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
        -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
        -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
        -0.5f, -0.5f,  0.5f, -1.0f,  0.0f,  0.0f,
        -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,

         0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,
         0.5f,  0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
         0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
         0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
         0.5f, -0.5f,  0.5f,  1.0f,  0.0f,  0.0f,
         0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,

        -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,
         0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,
         0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
         0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
        -0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,

        -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,
         0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,
         0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
         0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
        -0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
        -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f
    };

    uint[] indices = new uint[] {
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35
    };

    string vertexShaderSource = "#version 330 core\n" +
        "layout (location = 0) in vec3 aPos;\n" +
        "layout (location = 1) in vec3 aNormal;\n" +
        "out vec3 Normal;\n" +
        "out vec3 FragPos;\n" +
        "uniform mat4 model;" +
        "uniform mat4 view;" +
        "uniform mat4 projection;" +
        "void main()\n" +
        "{\n" +
        "   gl_Position = vec4(aPos, 1.0) * model * view * projection;\n" +
        "   FragPos = vec3(model * vec4(aPos, 1.0));\n" +
        "   Normal = aNormal;\n" +
        "}";

    string fragmentShaderSource = "#version 330 core\n" +
        "uniform sampler2D texture0;\n" +
        "in vec2 TexCoord;\n" +
        "out vec4 FragColor;\n" +
        "void main()\n" +
        "{\n" +
        "    FragColor = texture(texture0, TexCoord);\n" +
        "}";

    string lightFrag = "#version 330 core\n" +
"out vec4 FragColor;" +
"uniform vec3 objectColor;" +
"uniform vec3 lightColor;" +
"uniform vec3 lightPos;" +
"uniform vec3 viewPos;" +
"in vec3 Normal;" +
"in vec3 FragPos;" +
"float specularStrength = 0.5;" +
"void main()\r\n" +
"{\r\n" +
"    vec3 norm = normalize(Normal);" +
"    vec3 lightDir = normalize(lightPos - FragPos);" +
"    float diff = max(dot(norm, lightDir), 0.0);" +
"    vec3 diffuse = diff * lightColor;" +
"    vec3 viewDir = normalize(viewPos - FragPos);" +
"    vec3 reflectDir = reflect(-lightDir, norm);" +
"    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 64);" +
"    vec3 specular = specularStrength * spec * lightColor;" +
"    float ambientStrength = 0.1;\r\n" +
"    vec3 ambient = ambientStrength * lightColor;\r\n" +
"    vec3 result = (ambient + diffuse + specular) * objectColor;\r\n" +
"    FragColor = vec4(result, 1.0);\r\n" +
"}  \r\n";

    string lightSourceFrag = "#version 330 core\r\n" +
"out vec4 FragColor;\r\n" +
"in vec3 FragPos;\n" +
"void main()\r\n" +
"{\r\n" +
"    FragColor = vec4(1.0); // set all 4 vector values to 1.0\r\n" +
"}\r\n";

    void load()
    {
        Texture tex = new Texture().FromFile("bricks.png");
        VertexArray vertexArray = new VertexArray(vertices, indices, new VertexAttrib[] {
            new VertexAttrib { location = 0, size = 3 },
            new VertexAttrib { location = 1, size = 3 }
        });
        Shader shader = new Shader(vertexShaderSource, fragmentShaderSource);

        Shader lightingShader = new Shader(vertexShaderSource, lightFrag);
        lightingShader.SetVector3("objectColor", new Vector3(1.0f, 0.5f, 0.31f));
        lightingShader.SetVector3("lightColor", new Vector3(1.0f, 1.0f, 1.0f));

        Shader lightSourceShader = new Shader(vertexShaderSource, lightSourceFrag);


        GameObject cam = new GameObject("camera");
        cam.AddComponent<Camera>();
        cam.AddComponent<InputManager>();
        cam.AddComponent<TestCamComp>();

        lightingShader.UniformCallback(() => lightingShader.SetVector3("viewPos", cam.transform.position));

        GameObject go = new GameObject("test");
        go.AddComponent<MeshFilter>(vertexArray);
        go.AddComponent<MeshRenderer>(lightingShader);

        go.transform.position.Z = 5.0f;
        go.transform.position.X = 3.0f;

        GameObject go2 = new GameObject("test");
        go2.AddComponent<MeshFilter>(vertexArray);
        go2.AddComponent<MeshRenderer>(lightSourceShader);

        go2.transform.position.Z = -5.0f;
        go2.transform.position.Y = 4.0f;

        lightingShader.SetVector3("lightPos", go2.transform.position - go.transform.position);
    }
}
