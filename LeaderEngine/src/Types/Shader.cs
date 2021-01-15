using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace LeaderEngine
{
    public class Shader : IDisposable
    {
        #region DefaultShader
        public static Shader NoRender;
        public static Shader SpriteShader;
        public static Shader PostProcessing;
        public static Shader SSAO;
        public static Shader SSAOBlur;
        public static Shader Deferred;
        public static Shader Lit;
        public static Shader LitTransparent;
        public static Shader DepthOnly;
        public static Shader Skybox;
        public static Shader TextShader;
        public static Shader ImmediateMode;
        #endregion

        private int handle;

        private readonly Dictionary<string, int> uniformLocations;

        public Shader(string vertSource, string fragSource)
        {
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertSource);
            CompileShader(vertexShader);

            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragSource);
            CompileShader(fragmentShader);

            handle = GL.CreateProgram();

            GL.AttachShader(handle, vertexShader);
            GL.AttachShader(handle, fragmentShader);

            LinkProgram(handle);

            GL.DetachShader(handle, vertexShader);
            GL.DetachShader(handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            uniformLocations = new Dictionary<string, int>();

            for (var i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(handle, i, out _, out _);
                var location = GL.GetUniformLocation(handle, key);

                uniformLocations.Add(key, location);
            }
        }

        ~Shader()
        {
            ThreadManager.ExecuteOnMainThread(() => Dispose());
        }

        public static Shader FromSourceFile(string vertPath, string fragPath)
        {
            return new Shader(File.ReadAllText(vertPath), File.ReadAllText(fragPath));
        }

        public static void InitDefaults()
        {
            NoRender = FromSourceFile(AppContext.BaseDirectory + "DefaultAssets/Shaders/norender-vs.glsl", AppContext.BaseDirectory + "DefaultAssets/Shaders/norender-fs.glsl");
            SpriteShader = FromSourceFile(AppContext.BaseDirectory + "DefaultAssets/Shaders/sprite-vs.glsl", AppContext.BaseDirectory + "DefaultAssets/Shaders/sprite-fs.glsl");
            PostProcessing = FromSourceFile(AppContext.BaseDirectory + "DefaultAssets/Shaders/pp-vs.glsl", AppContext.BaseDirectory + "DefaultAssets/Shaders/pp-fs.glsl");
            SSAO = FromSourceFile(AppContext.BaseDirectory + "DefaultAssets/Shaders/ssao-vs.glsl", AppContext.BaseDirectory + "DefaultAssets/Shaders/ssao-fs.glsl");
            SSAOBlur = FromSourceFile(AppContext.BaseDirectory + "DefaultAssets/Shaders/ssaoblur-vs.glsl", AppContext.BaseDirectory + "DefaultAssets/Shaders/ssaoblur-fs.glsl");
            Deferred = FromSourceFile(AppContext.BaseDirectory + "DefaultAssets/Shaders/deferred-vs.glsl", AppContext.BaseDirectory + "DefaultAssets/Shaders/deferred-fs.glsl");
            Lit = FromSourceFile(AppContext.BaseDirectory + "DefaultAssets/Shaders/lit-vs.glsl", AppContext.BaseDirectory + "DefaultAssets/Shaders/lit-fs.glsl");
            LitTransparent = FromSourceFile(AppContext.BaseDirectory + "DefaultAssets/Shaders/lit-transparent-vs.glsl", AppContext.BaseDirectory + "DefaultAssets/Shaders/lit-transparent-fs.glsl");
            DepthOnly = FromSourceFile(AppContext.BaseDirectory + "DefaultAssets/Shaders/depth-vs.glsl", AppContext.BaseDirectory + "DefaultAssets/Shaders/depth-fs.glsl");
            Skybox = FromSourceFile(AppContext.BaseDirectory + "DefaultAssets/Shaders/skybox-vs.glsl", AppContext.BaseDirectory + "DefaultAssets/Shaders/skybox-fs.glsl");
            TextShader = FromSourceFile(AppContext.BaseDirectory + "DefaultAssets/Shaders/text-vs.glsl", AppContext.BaseDirectory + "DefaultAssets/Shaders/text-fs.glsl");
            ImmediateMode = FromSourceFile(AppContext.BaseDirectory + "DefaultAssets/Shaders/immediatemode-vs.glsl", AppContext.BaseDirectory + "DefaultAssets/Shaders/immediatemode-fs.glsl");

            Logger.Log("Shaders loaded");
        }

        private static void CompileShader(int shader)
        {
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program)
        {
            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                throw new Exception($"Error occurred whilst linking Program({program})");
            }
        }

        public void Use()
        {
            GL.UseProgram(handle);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(handle, attribName);
        }

        /// <summary>
        /// Set a uniform int on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetInt(string name, int data)
        {
            GL.UseProgram(handle);
            if (uniformLocations.ContainsKey(name))
                GL.Uniform1(uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform float on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetFloat(string name, float data)
        {
            GL.UseProgram(handle);
            if (uniformLocations.ContainsKey(name))
                GL.Uniform1(uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform Matrix4 on this shader
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        /// <remarks>
        ///   <para>
        ///   The matrix is transposed before being sent to the shader.
        ///   </para>
        /// </remarks>
        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(handle);
            if (uniformLocations.ContainsKey(name))
                GL.UniformMatrix4(uniformLocations[name], true, ref data);
        }

        /// <summary>
        /// Set a uniform Vector2 on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetVector2(string name, Vector2 data)
        {
            GL.UseProgram(handle);
            if (uniformLocations.ContainsKey(name))
                GL.Uniform2(uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform Vector3 on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(handle);
            if (uniformLocations.ContainsKey(name))
                GL.Uniform3(uniformLocations[name], data);
        }

        public void SetVector4(string name, Vector4 data)
        {
            GL.UseProgram(handle);
            if (uniformLocations.ContainsKey(name))
                GL.Uniform4(uniformLocations[name], data);
        }

        public int GetHandle()
        {
            return handle;
        }

        public void Dispose()
        {
            GL.DeleteShader(handle);

            GC.SuppressFinalize(this);
        }
    }
}
