using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Runtime.InteropServices;

namespace LeaderEngine
{
    public class SSAO : IDisposable
    {
        private Shader SSAOShader = Shader.SSAO;

        private int FBO, gAlbedo, gPosition, gNormal, gPositionViewSpace, gNormalViewSpace, depthTexture;

        private Vector2 currentSize;

        private Mesh mesh;

        //SSAO
        private const int kernelSize = 64;
        private const int noiseWidth = 64, noiseHeight = 64;
        private float[] ssaoKernel;

        private Texture noiseTexture;

        public SSAOBlur SSAOBlur;

        public float Power = 5.0f;
        public float Radius = 0.5f;
        public float Bias = 0.0005f;

        public DeferredProcessor DeferredProcessor;

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
            currentSize = size;

            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

            #region GBUFFER
            //color + specular buffer
            gAlbedo = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gAlbedo);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, gAlbedo, 0);

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

            //position color buffer view space
            gPositionViewSpace = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gPositionViewSpace);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment3, TextureTarget.Texture2D, gPositionViewSpace, 0);

            //normals color buffer view space
            gNormalViewSpace = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gNormalViewSpace);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment4, TextureTarget.Texture2D, gNormalViewSpace, 0);

            GL.DrawBuffers(5, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2, DrawBuffersEnum.ColorAttachment3, DrawBuffersEnum.ColorAttachment4 });
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
            SSAOBlur = new SSAOBlur(size);
            DeferredProcessor = new DeferredProcessor(size, gAlbedo, gPosition, gNormal, depthTexture);
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
            SSAOBlur.Resize(width, height);
            DeferredProcessor.Resize(width, height);
            currentSize = new Vector2(width, height);
        }

        private void Update(Vector2i size)
        {
            GL.BindTexture(TextureTarget.Texture2D, gAlbedo);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2D, gPosition);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2D, gNormal);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2D, gPositionViewSpace);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2D, gNormalViewSpace);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2D, depthTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, size.X, size.Y, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Render()
        {
            GL.Disable(EnableCap.DepthTest);

            mesh.Use();
            SSAOShader.Use();

            SSAOShader.SetInt("gAlbedoSpec", 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, gAlbedo);

            SSAOShader.SetInt("gPosition", 1);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, gPositionViewSpace);

            SSAOShader.SetInt("gNormal", 2);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, gNormalViewSpace);

            SSAOShader.SetInt("depthMap", 3);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, depthTexture);

            SSAOShader.SetInt("texNoise", 4);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, noiseTexture.GetHandle());

            SSAOShader.SetMatrix4("projection", RenderingGlobals.Projection);
            SSAOShader.SetVector2("vSize", currentSize);

            SSAOShader.SetFloat("power", Power);
            SSAOShader.SetFloat("radius", Radius);
            SSAOShader.SetFloat("bias", Bias);

            SSAOBlur.Begin();
            GL.DrawElements(PrimitiveType.Triangles, mesh.GetIndicesCount(), DrawElementsType.UnsignedInt, 0);
            SSAOBlur.End();
        }

        public void RenderBlurPass()
        {
            DeferredProcessor.Begin();
            SSAOBlur.Render();
            DeferredProcessor.End();
        }

        public void RenderLightPass()
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            DeferredProcessor.Render();
        }

        public void Dispose()
        {
            GL.DeleteFramebuffer(FBO);

            GL.DeleteTexture(gPosition);
            GL.DeleteTexture(gNormal);
            GL.DeleteTexture(gAlbedo);

            GL.DeleteTexture(depthTexture);

            mesh.Dispose();
        }
    }
}
