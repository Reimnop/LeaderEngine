using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Runtime.InteropServices;

namespace LeaderEngine
{
    internal class SSAO : IDisposable
    {
        public Shader SSAOShader = Shader.SSAO;

        private int FBO, gAlbedoSpec, gPosition, gNormal, depthTexture;

        private Vector2 currentSize;

        private Mesh mesh;

        //SSAO
        private const int kernelSize = 128;
        private const int noiseWidth = 64, noiseHeight = 64;
        private float[] ssaoKernel;

        private Texture noiseTexture;

        public SSAO(int width, int height)
        {
            Setup(new Vector2i(width, height));
        }

        public SSAO(Vector2i vSize)
        {
            Setup(vSize);
        }

        ~SSAO()
        {
            ThreadManager.ExecuteOnMainThread(() => Dispose());
        }

        private void Setup(Vector2i size)
        {
            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

            #region GBUFFER
            //color + specular buffer
            gAlbedoSpec = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gAlbedoSpec);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, gAlbedoSpec, 0);

            //position color buffer
            gPosition = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gPosition);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, gPosition, 0);

            //normals color buffer
            gNormal = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gNormal);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, TextureTarget.Texture2D, gNormal, 0);

            GL.DrawBuffers(3, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2 });
            #endregion

            //depth buffer
            depthTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, depthTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, size.X, size.Y, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthTexture, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            #region QUAD_SETUP
            float[] vertices =
            {
                -1.0f, -1.0f, 0.0f, 0.0f, 0.0f,
                -1.0f,  1.0f, 0.0f, 0.0f, 1.0f,
                 1.0f,  1.0f, 0.0f, 1.0f, 1.0f,
                 1.0f, -1.0f, 0.0f, 1.0f, 0.0f
            };

            uint[] indices =
            {
                0, 1, 3,
                1, 2, 3
            };

            mesh = new Mesh("PPMesh", vertices, indices, new VertexAttrib[]
            {
                new VertexAttrib { location = 0, size = 3 },
                new VertexAttrib { location = 1, size = 2 }
            });
            #endregion

            SSAOSetup();
        }

        private void SSAOSetup()
        {
            //Kernel setup
            ssaoKernel = new float[kernelSize * 3];

            Random rng = new Random();

            for (int i = 0; i < kernelSize; i++)
            {
                int offset = i * 3;

                Vector3 sample = new Vector3(
                    (float)rng.NextDouble() * 2.0f - 1.0f,
                    (float)rng.NextDouble() * 2.0f - 1.0f,
                    (float)rng.NextDouble());

                sample.Normalize();
                sample *= (float)rng.NextDouble();

                float scale = i / 128.0f;
                scale = MathHelper.Lerp(0.1f, 1.0f, scale * scale);

                sample *= scale;

                ssaoKernel[offset + 0] = sample.X;
                ssaoKernel[offset + 1] = sample.Y;
                ssaoKernel[offset + 2] = sample.Z;
            }

            int attLoc = GL.GetUniformLocation(SSAOShader.GetHandle(), "samples");
            GL.ProgramUniform3(SSAOShader.GetHandle(), attLoc, 3, ssaoKernel);

            //kernel rotations
            SSAOShader.SetVector2("nSize", new Vector2(noiseWidth, noiseHeight));

            Vector3[] ssaoNoise = new Vector3[noiseWidth * noiseHeight];

            for (int i = 0; i < noiseWidth * noiseHeight; i++)
            {
                Vector3 noise = new Vector3(
                    (float)rng.NextDouble() * 2.0f - 1.0f,
                    (float)rng.NextDouble() * 2.0f - 1.0f,
                    0.0f);

                ssaoNoise[i] = noise;
            }

            //setup noise texture
            GCHandle handle = GCHandle.Alloc(ssaoNoise, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();

            noiseTexture = new Texture().FromIntPtr(noiseWidth, noiseHeight, ptr, PixelInternalFormat.Rgba16f, PixelFormat.Rgb, PixelType.Float);
            noiseTexture.SetMinFilter(TextureMinFilter.Nearest);
            noiseTexture.SetMagFilter(TextureMagFilter.Nearest);
            noiseTexture.SetWrapS(TextureWrapMode.Repeat);
            noiseTexture.SetWrapT(TextureWrapMode.Repeat);

            handle.Free();
        }

        public void Begin()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void End()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Resize(int width, int height)
        {
            Update(new Vector2i(width, height));
            currentSize = new Vector2(width, height);
        }

        private void Update(Vector2i size)
        {
            GL.BindTexture(TextureTarget.Texture2D, gAlbedoSpec);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2D, gPosition);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2D, gNormal);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2D, depthTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, size.X, size.Y, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Render()
        {
            mesh.Use();
            SSAOShader.Use();

            SSAOShader.SetInt("gAlbedoSpec", 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, gAlbedoSpec);

            SSAOShader.SetInt("gPosition", 1);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, gPosition);

            SSAOShader.SetInt("gNormal", 2);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, gNormal);

            SSAOShader.SetInt("depthMap", 3);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, depthTexture);

            SSAOShader.SetInt("texNoise", 4);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, noiseTexture.GetHandle());

            SSAOShader.SetMatrix4("projection", RenderingGlobals.Projection);
            SSAOShader.SetVector2("vSize", currentSize);

            GL.DrawElements(PrimitiveType.Triangles, mesh.GetIndicesCount(), DrawElementsType.UnsignedInt, 0);
        }

        public void Dispose()
        {
            GL.DeleteFramebuffer(FBO);

            GL.DeleteTexture(gPosition);
            GL.DeleteTexture(gNormal);
            GL.DeleteTexture(gAlbedoSpec);

            GL.DeleteTexture(depthTexture);

            mesh.Dispose();
        }
    }
}
