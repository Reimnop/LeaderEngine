using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace LeaderEngine
{
    public static class RenderCommandProcessor
    {
        private static Dictionary<CommandEnum, Action<object>> commandProcessors = new Dictionary<CommandEnum, Action<object>>()
        {
            { CommandEnum.BindShader, ExecuteBindShader },
            { CommandEnum.BindTexture, ExecuteBindTexture },
            { CommandEnum.BindVertexArray, ExecuteBindVertexArray },
            { CommandEnum.BindBuffer, ExecuteBindBuffer },

            { CommandEnum.DrawArrays, ExecuteDrawArrays },
            { CommandEnum.DrawElements, ExecuteDrawElements },
            { CommandEnum.DrawArraysInstanced, ExecuteDrawArraysInstanced },
            { CommandEnum.DrawElementsInstanced, ExecuteDrawElementsInstanced },
            { CommandEnum.MultiDrawArrays, ExecuteMultiDrawArrays },
            { CommandEnum.MultiDrawElements, ExecuteMultiDrawElements },

            { CommandEnum.SetUniformInt, ExecuteSetUniformInt },
            { CommandEnum.SetUniformFloat, ExecuteSetUniformFloat },
            { CommandEnum.SetUniformVector3, ExecuteSetUniformVector3 },
            { CommandEnum.SetUniformVector4, ExecuteSetUniformVector4 },
            { CommandEnum.SetUniformMatrix3, ExecuteSetUniformMatrix3 },
            { CommandEnum.SetUniformMatrix4, ExecuteSetUniformMatrix4 }
        };

        private static int lastShader;

        private static void ExecuteSetUniformMatrix4(object obj)
        {
            var args = ((int location, Matrix4 value))obj;
            GL.UniformMatrix4(args.location, true, ref args.value);
        }

        private static void ExecuteSetUniformMatrix3(object obj)
        {
            var args = ((int location, Matrix3 value))obj;
            GL.UniformMatrix3(args.location, true, ref args.value);
        }

        private static void ExecuteSetUniformVector4(object obj)
        {
            var args = ((int location, Vector4 value))obj;
            GL.Uniform4(args.location, args.value);
        }

        private static void ExecuteSetUniformVector3(object obj)
        {
            var args = ((int location, Vector3 value))obj;
            GL.Uniform3(args.location, args.value);
        }

        private static void ExecuteSetUniformFloat(object obj)
        {
            var args = ((int location, float value))obj;
            GL.Uniform1(args.location, args.value);
        }

        private static void ExecuteSetUniformInt(object obj)
        {
            var args = ((int location, int value))obj;
            GL.Uniform1(args.location, args.value);
        }

        private static void ExecuteMultiDrawElements(object obj)
        {
            var args = ((PrimitiveType primitiveType, int[] counts, DrawElementsType drawElementsType, int[] indices, int drawCount))obj;
            GL.MultiDrawElements(args.primitiveType, args.counts, args.drawElementsType, args.indices, args.drawCount);
        }

        private static void ExecuteMultiDrawArrays(object obj)
        {
            var args = ((PrimitiveType primitiveType, int[] firsts, int[] counts, int drawCount))obj;
            GL.MultiDrawArrays(args.primitiveType, args.firsts, args.counts, args.drawCount);
        }

        private static void ExecuteDrawElementsInstanced(object obj)
        {
            var args = ((PrimitiveType primitiveType, int count, DrawElementsType drawElementsType, int indices, int instanceCount))obj;
            GL.DrawElementsInstanced(args.primitiveType, args.count, args.drawElementsType, (IntPtr)args.indices, args.instanceCount);
        }

        private static void ExecuteDrawArraysInstanced(object obj)
        {
            var args = ((PrimitiveType primitiveType, int offset, int count, int instanceCount))obj;
            GL.DrawArraysInstanced(args.primitiveType, args.offset, args.count, args.instanceCount);
        }

        private static void ExecuteDrawElements(object obj)
        {
            var args = ((PrimitiveType primitiveType, int count, DrawElementsType drawElementsType, int indices))obj;
            GL.DrawElements(args.primitiveType, args.count, args.drawElementsType, args.indices);
        }

        private static void ExecuteDrawArrays(object obj)
        {
            var args = ((PrimitiveType primitiveType, int offset, int count))obj;
            GL.DrawArrays(args.primitiveType, args.offset, args.count);
        }

        private static void ExecuteBindBuffer(object obj)
        {
            var args = ((BufferTarget bufferTarget, int handle))obj;
            GL.BindBuffer(args.bufferTarget, args.handle);
        }

        private static void ExecuteBindVertexArray(object obj)
        {
            int handle = (int)obj;
            GL.BindVertexArray(handle);
        }

        private static void ExecuteBindTexture(object obj)
        {
            var args = ((TextureUnit unit, int handle))obj;
            GL.BindTextureUnit((int)args.unit - (int)TextureUnit.Texture0, args.handle);
        }

        private static void ExecuteBindShader(object obj)
        {
            int handle = (int)obj;

            if (lastShader == handle)
                return;

            GL.UseProgram(handle);
            lastShader = handle;
        }

        public static void ExecuteCommand(GLCommand command)
        {
            if (commandProcessors.TryGetValue(command.Command, out var processor))
                processor(command.Arguments);
        }

        public static void Reset()
        {
            lastShader = -1;
        }
    }
}
