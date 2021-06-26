using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.IO;

namespace LeaderEngine
{
    public sealed class Shader : GameAsset
    {
        public override GameAssetType AssetType => GameAssetType.Shader;

        public int Handle => _handle;

        private int _handle;

        private readonly Dictionary<string, int> uniformLocations;

        public Shader(string name, string vertSource, string fragSource) : base(name)
        {
            bool success;

            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertSource);
            CompileShader(vertexShader, out success);

            if (!success)
                return;

            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragSource);
            CompileShader(fragmentShader, out success);

            if (!success)
                return;

            _handle = GL.CreateProgram();

            GL.AttachShader(_handle, vertexShader);
            GL.AttachShader(_handle, fragmentShader);

            LinkProgram(_handle, out success);

            if (!success)
                return;

            GL.DetachShader(_handle, vertexShader);
            GL.DetachShader(_handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            GL.GetProgram(_handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            uniformLocations = new Dictionary<string, int>();

            for (var i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(_handle, i, out _, out _);
                var location = GL.GetUniformLocation(_handle, key);

                uniformLocations.Add(key, location);
            }

            GL.ObjectLabel(ObjectLabelIdentifier.Program, _handle, name.Length, name);
        }

        public static Shader FromSourceFile(string name, string vertPath, string fragPath)
        {
            return new Shader(name, File.ReadAllText(vertPath), File.ReadAllText(fragPath));
        }

        private static void CompileShader(int shader, out bool success)
        {
            success = true;

            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                var infoLog = GL.GetShaderInfoLog(shader);
                Logger.LogError($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
                success = false;
            }
        }

        private static void LinkProgram(int program, out bool success)
        {
            success = true;

            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                Logger.LogError($"Error occurred whilst linking Program({program})");
                success = false;
            }
        }

        public int GetAttribLocation(string attribName)
        {
            if (uniformLocations.TryGetValue(attribName, out int loc))
                return loc;

            int attribLoc = GL.GetAttribLocation(_handle, attribName);
            if (attribLoc != -1)
                uniformLocations.Add(attribName, attribLoc);

            return attribLoc;
        }

        public override void Dispose()
        {
            base.Dispose();

            GL.DeleteProgram(_handle);
        }
    }
}
