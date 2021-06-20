using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LeaderEngine
{
    public class Material : GameAsset
    {
        private const int alignment = 4;

        public override GameAssetType AssetType => GameAssetType.Material;

        public Shader Shader => _shader;
        public int UniformBuffer => _uniformBuffer;
        public int BufferSize => _bufferSize;

        public Span<MaterialProperty> MaterialProperties => new Span<MaterialProperty>(_materialProperties);

        private Shader _shader;
        private int _uniformBuffer;
        private int _bufferSize;

        private MaterialProperty[] _materialProperties;

        private Dictionary<string, int> propertyIndices;
        private object[] materialData;

        public Material(string name, Shader shader, params MaterialProperty[] properties) : base(name)
        {
            foreach (MaterialProperty property in properties)
                _bufferSize += GetAlignedPropertySize(property.PropertyType);

            propertyIndices = new Dictionary<string, int>();
            for (int i = 0; i < properties.Length; i++)
            {
                propertyIndices.Add(properties[i].Name, i);
            }

            materialData = new object[properties.Length];
            _materialProperties = properties;

            _uniformBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, _uniformBuffer);
            GL.BufferData(BufferTarget.UniformBuffer, 0, IntPtr.Zero, BufferUsageHint.DynamicCopy);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

            GL.ObjectLabel(ObjectLabelIdentifier.Buffer, _uniformBuffer, name.Length + 4, name + "-UBO");

            _shader = shader;
        }

        #region Setters
        public void SetInt(string name, int value)
        {
            int index = propertyIndices[name];
            materialData[index] = value;
        }
        public void SetFloat(string name, float value)
        {
            int index = propertyIndices[name];
            materialData[index] = value;
        }
        public void SetVector2(string name, Vector2 value)
        {
            int index = propertyIndices[name];
            materialData[index] = value;
        }
        public void SetVector3(string name, Vector3 value)
        {
            int index = propertyIndices[name];
            materialData[index] = value;
        }
        public void SetVector4(string name, Vector4 value)
        {
            int index = propertyIndices[name];
            materialData[index] = value;
        }
        public void SetMatrix2(string name, Matrix2 value)
        {
            int index = propertyIndices[name];
            materialData[index] = value;
        }
        public void SetMatrix3(string name, Matrix3 value)
        {
            int index = propertyIndices[name];
            materialData[index] = value;
        }
        public void SetMatrix4(string name, Matrix4 value)
        {
            int index = propertyIndices[name];
            materialData[index] = value;
        }
        public void SetTexture(string name, Texture value)
        {
            int index = propertyIndices[name];
            materialData[index] = value;
        }
        #endregion
        #region Getters
        public int GetInt(string name)
        {
            int index = propertyIndices[name];
            return (int)materialData[index];
        }
        public float GetFloat(string name)
        {
            int index = propertyIndices[name];
            return (float)materialData[index];
        }
        public Vector2 GetVector2(string name)
        {
            int index = propertyIndices[name];
            return (Vector2)materialData[index];
        }
        public Vector3 GetVector3(string name)
        {
            int index = propertyIndices[name];
            return (Vector3)materialData[index];
        }
        public Vector4 GetVector4(string name)
        {
            int index = propertyIndices[name];
            return (Vector4)materialData[index];
        }
        public Matrix2 GetMatrix2(string name)
        {
            int index = propertyIndices[name];
            return (Matrix2)materialData[index];
        }
        public Matrix3 GetMatrix3(string name)
        {
            int index = propertyIndices[name];
            return (Matrix3)materialData[index];
        }
        public Matrix4 GetMatrix4(string name)
        {
            int index = propertyIndices[name];
            return (Matrix4)materialData[index];
        }
        public Texture GetTexture(string name)
        {
            int index = propertyIndices[name];
            return (Texture)materialData[index];
        }
        #endregion

        public void UpdateBuffer()
        {
            IntPtr data = GenBuffer();
            GL.NamedBufferData(_uniformBuffer, _bufferSize, data, BufferUsageHint.DynamicCopy);

            Marshal.FreeHGlobal(data);
        }

        private IntPtr GenBuffer()
        {
            IntPtr ptr = Marshal.AllocHGlobal(_bufferSize);
            
            unsafe
            {
                Unsafe.InitBlockUnaligned((void*)ptr, 0, (uint)_bufferSize);
            }

            int offset = 0;
            for (int i = 0; i < _materialProperties.Length; i++)
            {
                MaterialProperty property = _materialProperties[i];
                switch (property.PropertyType)
                {
                    case MaterialPropertyType.Int:
                        Marshal.StructureToPtr((int)materialData[i], IntPtr.Add(ptr, offset), false);
                        break;
                    case MaterialPropertyType.Float:
                        Marshal.StructureToPtr((float)materialData[i], IntPtr.Add(ptr, offset), false);
                        break;
                    case MaterialPropertyType.Vector2:
                        Marshal.StructureToPtr((Vector2)materialData[i], IntPtr.Add(ptr, offset), false);
                        break;
                    case MaterialPropertyType.Vector3:
                        Marshal.StructureToPtr((Vector3)materialData[i], IntPtr.Add(ptr, offset), false);
                        break;
                    case MaterialPropertyType.Vector4:
                        Marshal.StructureToPtr((Vector4)materialData[i], IntPtr.Add(ptr, offset), false);
                        break;
                    case MaterialPropertyType.Matrix2:
                        Marshal.StructureToPtr((Matrix2)materialData[i], IntPtr.Add(ptr, offset), false);
                        break;
                    case MaterialPropertyType.Matrix3:
                        Marshal.StructureToPtr((Matrix3)materialData[i], IntPtr.Add(ptr, offset), false);
                        break;
                    case MaterialPropertyType.Matrix4:
                        Marshal.StructureToPtr((Matrix4)materialData[i], IntPtr.Add(ptr, offset), false);
                        break;
                    case MaterialPropertyType.Texture:
                        Texture texture = (Texture)materialData[i];
                        Marshal.StructureToPtr(texture != null ? texture.LongHandle : 0L, IntPtr.Add(ptr, offset), false);
                        break;
                }

                offset += GetAlignedPropertySize(property.PropertyType);
            }

            return ptr;
        }

        private int GetAlignedPropertySize(MaterialPropertyType propertyType)
        {
            switch (propertyType)
            {
                case MaterialPropertyType.Int:
                    return AlignSize(sizeof(int), alignment);
                case MaterialPropertyType.Float:
                    return AlignSize(sizeof(float), alignment);
                case MaterialPropertyType.Vector2:
                    return AlignSize(Vector2.SizeInBytes, alignment);
                case MaterialPropertyType.Vector3:
                    return AlignSize(Vector3.SizeInBytes, alignment);
                case MaterialPropertyType.Vector4:
                    return AlignSize(Vector4.SizeInBytes, alignment);
                case MaterialPropertyType.Matrix2:
                    return AlignSize(sizeof(float) * 4, alignment);
                case MaterialPropertyType.Matrix3:
                    return AlignSize(sizeof(float) * 9, alignment);
                case MaterialPropertyType.Matrix4:
                    return AlignSize(sizeof(float) * 16, alignment);
                case MaterialPropertyType.Texture:
                    return AlignSize(sizeof(long), alignment);
                default:
                    throw new Exception("Invalid property type!");
            }
        }

        private int AlignSize(int size, int alignment)
        {
            size -= 1;
            return size + alignment - size % alignment;
        }

        public override void Dispose()
        {
            base.Dispose();

            GL.DeleteBuffer(_uniformBuffer);
        }
    }
}