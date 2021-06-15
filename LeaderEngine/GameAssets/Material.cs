using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LeaderEngine
{
    public class Material<T> : Material where T : struct
    {
        public T Data => _data;

        private T _data;

        public Material(string name, Shader shader) : base(name, shader)
        {
            _uniformBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, _uniformBuffer);
            GL.BufferData(BufferTarget.UniformBuffer, 0, IntPtr.Zero, BufferUsageHint.DynamicCopy);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, _uniformBuffer, name.Length + 4, name + "-UBO");
        }

        public void UpdateMaterial(T data)
        {
            int size = Unsafe.SizeOf<T>();
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(data, ptr, true);
            GL.NamedBufferData(_uniformBuffer, size, ptr, BufferUsageHint.DynamicCopy);
            Marshal.FreeHGlobal(ptr);

            _data = data;
        }

        public override void Dispose()
        {
            base.Dispose();

            GL.DeleteBuffer(_uniformBuffer);
        }
    }
}
