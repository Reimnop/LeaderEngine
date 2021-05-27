using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace LeaderEngine
{
    public enum CommandEnum
    {
        BindShader,
        BindTexture,
        BindVertexArray,
        BindBuffer,

        DrawArrays,
        DrawElements,
        DrawArraysInstanced,
        DrawElementsInstanced,
        MultiDrawArrays,
        MultiDrawElements,

        SetUniformInt,
        SetUniformFloat,
        SetUniformVector3,
        SetUniformVector4,
        SetUniformMatrix3,
        SetUniformMatrix4
    }

    public struct GLCommand
    {
        public CommandEnum Command;
        public object Arguments;
    }

    public class CommandBuffer
    {
        public GLCommand[] Commands { get; } = new GLCommand[128];
        public int Count { get; private set; }

        private void AddCommand(GLCommand command)
        {
            Commands[Count] = command;
            Count++;
        }

        public void Clear()
        {
            Count = 0;
        }

        public void BindShader(int shaderHandle) //basic
        {
            AddCommand(new GLCommand
            {
                Command = CommandEnum.BindShader,
                Arguments = shaderHandle
            });
        }

        public void BindShader(Shader shader)
        {
            BindShader(shader.GetHandle());
        }

        public void BindTexture(TextureUnit unit, int handle) //basic
        {
            AddCommand(new GLCommand
            {
                Command = CommandEnum.BindTexture,
                Arguments = ValueTuple.Create(unit, handle)
            });
        }

        public void BindTexture(TextureUnit unit, Texture texture)
        {
            BindTexture(unit, texture.GetHandle());
        }

        public void BindVertexArray(int handle)
        {
            AddCommand(new GLCommand
            {
                Command = CommandEnum.BindVertexArray,
                Arguments = handle
            });
        }

        public void BindBuffer(BufferTarget bufferTarget, int handle)
        {
            AddCommand(new GLCommand
            {
                Command = CommandEnum.BindBuffer,
                Arguments = ValueTuple.Create(bufferTarget, handle)
            });
        }

        public void DrawArrays(PrimitiveType primitiveType, int offset, int count)
        {
            AddCommand(new GLCommand
            {
                Command = CommandEnum.DrawArrays,
                Arguments = ValueTuple.Create(primitiveType, offset, count)
            });
        }

        public void DrawElements(PrimitiveType primitiveType, int count, DrawElementsType drawElementsType, int indices)
        {
            AddCommand(new GLCommand
            {
                Command = CommandEnum.DrawElements,
                Arguments = ValueTuple.Create(primitiveType, count, drawElementsType, indices)
            });
        }

        public void DrawArraysInstanced(PrimitiveType primitiveType, int offset, int count, int instanceCount)
        {
            AddCommand(new GLCommand
            {
                Command = CommandEnum.DrawArraysInstanced,
                Arguments = ValueTuple.Create(primitiveType, offset, count, instanceCount)
            });
        }

        public void DrawElementsInstanced(PrimitiveType primitiveType, int count, DrawElementsType drawElementsType, int indices, int instanceCount)
        {
            AddCommand(new GLCommand
            {
                Command = CommandEnum.DrawElementsInstanced,
                Arguments = ValueTuple.Create(primitiveType, drawElementsType, count, indices, instanceCount)
            });
        }

        public void MultiDrawArrays(PrimitiveType primitiveType, int[] firsts, int[] counts, int drawCount)
        {
            AddCommand(new GLCommand
            {
                Command = CommandEnum.MultiDrawArrays,
                Arguments = ValueTuple.Create(primitiveType, firsts, counts, drawCount)
            });
        }

        public void MultiDrawElements(PrimitiveType primitiveType, int[] counts, DrawElementsType drawElementsType, int[] indices, int drawCount)
        {
            AddCommand(new GLCommand
            {
                Command = CommandEnum.MultiDrawElements,
                Arguments = ValueTuple.Create(primitiveType, counts, drawElementsType, indices, drawCount)
            });
        }

        public void SetUniformInt(int location, int value)
        {
            AddCommand(new GLCommand
            {
                Command = CommandEnum.SetUniformInt,
                Arguments = ValueTuple.Create(location, value)
            });
        }

        public void SetUniformInt(Shader shader, string name, int value)
        {
            SetUniformInt(shader.GetAttribLocation(name), value);
        }

        public void SetUniformFloat(int location, float value)
        {
            AddCommand(new GLCommand
            {
                Command = CommandEnum.SetUniformFloat,
                Arguments = ValueTuple.Create(location, value)
            });
        }

        public void SetUniformFloat(Shader shader, string name, float value)
        {
            SetUniformFloat(shader.GetAttribLocation(name), value);
        }

        public void SetUniformVector3(int location, Vector3 value)
        {
            AddCommand(new GLCommand
            {
                Command = CommandEnum.SetUniformVector3,
                Arguments = ValueTuple.Create(location, value)
            });
        }

        public void SetUniformVector3(Shader shader, string name, Vector3 value)
        {
            SetUniformVector3(shader.GetAttribLocation(name), value);
        }

        public void SetUniformVector4(int location, Vector4 value)
        {
            AddCommand(new GLCommand
            {
                Command = CommandEnum.SetUniformVector4,
                Arguments = ValueTuple.Create(location, value)
            });
        }

        public void SetUniformVector4(Shader shader, string name, Vector4 value)
        {
            SetUniformVector4(shader.GetAttribLocation(name), value);
        }

        public void SetUniformMatrix3(int location, Matrix3 value)
        {
            AddCommand(new GLCommand
            {
                Command = CommandEnum.SetUniformMatrix3,
                Arguments = ValueTuple.Create(location, value)
            });
        }

        public void SetUniformMatrix3(Shader shader, string name, Matrix3 value)
        {
            SetUniformMatrix3(shader.GetAttribLocation(name), value);
        }

        public void SetUniformMatrix4(int location, Matrix4 value)
        {
            AddCommand(new GLCommand
            {
                Command = CommandEnum.SetUniformMatrix4,
                Arguments = ValueTuple.Create(location, value)
            });
        }

        public void SetUniformMatrix4(Shader shader, string name, Matrix4 value)
        {
            SetUniformMatrix4(shader.GetAttribLocation(name), value);
        }

        //methods for the lazy
        public void BindMesh(Mesh mesh)
        {
            BindVertexArray(mesh.VAO);
        }

        public void DrawMesh(Mesh mesh)
        {
            DrawElements(mesh.PrimitiveType, mesh.IndicesCount, DrawElementsType.UnsignedInt, 0);
        }

        public void BindMaterial(Material material, Shader shader)
        {
            foreach (var prop in material.MaterialProps)
            {
                switch (prop.Value.PropType)
                {
                    case MaterialPropType.Int:
                        SetUniformInt(shader, prop.Key, (int)prop.Value.Data);
                        break;
                    case MaterialPropType.Float:
                        SetUniformFloat(shader, prop.Key, (float)prop.Value.Data);
                        break;
                    case MaterialPropType.Vector3:
                        SetUniformVector3(shader, prop.Key, (Vector3)prop.Value.Data);
                        break;
                    case MaterialPropType.Vector4:
                        SetUniformVector4(shader, prop.Key, (Vector4)prop.Value.Data);
                        break;
                }
            }

            foreach (var tex in material.MaterialTextures)
            {
                BindTexture(tex.Key, tex.Value);
            }
        }
    }
}
