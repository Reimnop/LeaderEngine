using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LeaderEngine
{
    public static class CommandProcessor
    {
        private static int lastShader;

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
            { CommandEnum.SetUniformMatrix4, ExecuteSetUniformMatrix4 },

            { CommandEnum.UniformBlockBinding, ExecuteUniformBlockBinding },
            { CommandEnum.BindBufferBase, ExecuteBindBufferBase }
        };

        private static void ExecuteBindBufferBase(object obj)
        {
            var args = (ValueTuple<BufferRangeTarget, int, int>)obj;
            GL.BindBufferBase(args.Item1, args.Item2, args.Item3);
        }

        private static void ExecuteUniformBlockBinding(object obj)
        {
            var args = (ValueTuple<int, int, int>)obj;
            GL.UniformBlockBinding(args.Item1, args.Item2, args.Item3);
        }

        private static void ExecuteSetUniformMatrix4(object obj)
        {
            var args = (ValueTuple<int, Matrix4>)obj;
            GL.UniformMatrix4(args.Item1, true, ref args.Item2);
        }

        private static void ExecuteSetUniformMatrix3(object obj)
        {
            var args = (ValueTuple<int, Matrix3>)obj;
            GL.UniformMatrix3(args.Item1, true, ref args.Item2);
        }

        private static void ExecuteSetUniformVector4(object obj)
        {
            var args = (ValueTuple<int, Vector4>)obj;
            GL.Uniform4(args.Item1, args.Item2);
        }

        private static void ExecuteSetUniformVector3(object obj)
        {
            var args = (ValueTuple<int, Vector3>)obj;
            GL.Uniform3(args.Item1, args.Item2);
        }

        private static void ExecuteSetUniformFloat(object obj)
        {
            var args = (ValueTuple<int, float>)obj;
            GL.Uniform1(args.Item1, args.Item2);
        }

        private static void ExecuteSetUniformInt(object obj)
        {
            var args = (ValueTuple<int, int>)obj;
            GL.Uniform1(args.Item1, args.Item2);
        }

        private static void ExecuteMultiDrawElements(object obj)
        {
            var args = (ValueTuple<PrimitiveType, int[], DrawElementsType, int[], int>)obj;
            GL.MultiDrawElements(args.Item1, args.Item2, args.Item3, args.Item4, args.Item5);
        }

        private static void ExecuteMultiDrawArrays(object obj)
        {
            var args = (ValueTuple<PrimitiveType, int[], int[], int>)obj;
            GL.MultiDrawArrays(args.Item1, args.Item2, args.Item3, args.Item4);
        }

        private static void ExecuteDrawElementsInstanced(object obj)
        {
            var args = (ValueTuple<PrimitiveType, int, DrawElementsType, int, int>)obj;
            GL.DrawElementsInstanced(args.Item1, args.Item2, args.Item3, (IntPtr)args.Item4, args.Item5);
        }

        private static void ExecuteDrawArraysInstanced(object obj)
        {
            var args = (ValueTuple<PrimitiveType, int, int, int>)obj;
            GL.DrawArraysInstanced(args.Item1, args.Item2, args.Item3, args.Item4);
        }

        private static void ExecuteDrawElements(object obj)
        {
            var args = (ValueTuple<PrimitiveType, int, DrawElementsType, int>)obj;
            GL.DrawElements(args.Item1, args.Item2, args.Item3, args.Item4);
        }

        private static void ExecuteDrawArrays(object obj)
        {
            var args = (ValueTuple<PrimitiveType, int, int>)obj;
            GL.DrawArrays(args.Item1, args.Item2, args.Item3);
        }

        private static void ExecuteBindBuffer(object obj)
        {
            var args = (ValueTuple<BufferTarget, int>)obj;
            GL.BindBuffer(args.Item1, args.Item2);
        }

        private static void ExecuteBindVertexArray(object obj)
        {
            int handle = (int)obj;
            GL.BindVertexArray(handle);
        }

        private static void ExecuteBindTexture(object obj)
        {
            var args = (ValueTuple<TextureUnit, int>)obj;
            GL.BindTextureUnit((int)args.Item1 - (int)TextureUnit.Texture0, args.Item2);
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
