using OpenTK.Mathematics;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LeaderEngine
{
    public class MaterialSerializer : GameAssetSerializer
    {
        public override bool CanSerialize(GameAssetType assetType)
        {
            return assetType == GameAssetType.Material;
        }

        public override void WriteToStream(BinaryWriter writer, GameAsset asset)
        {
            Material material = (Material)asset;

            writer.Write(material.Name);
            writer.Write(material.Shader.ID);

            Span<MaterialProperty> properties = material.MaterialProperties;
            writer.Write(properties.Length);
            foreach (MaterialProperty property in properties)
            {
                writer.Write(property.Name);
                writer.Write((int)property.PropertyType);
                switch (property.PropertyType)
                {
                    case MaterialPropertyType.Int:
                        writer.Write(material.GetInt(property.Name));
                        break;
                    case MaterialPropertyType.Float:
                        writer.Write(material.GetFloat(property.Name));
                        break;
                    case MaterialPropertyType.Vector2:
                        WriteStruct(writer, material.GetVector2(property.Name));
                        break;
                    case MaterialPropertyType.Vector3:
                        WriteStruct(writer, material.GetVector3(property.Name));
                        break;
                    case MaterialPropertyType.Vector4:
                        WriteStruct(writer, material.GetVector4(property.Name));
                        break;
                    case MaterialPropertyType.Matrix2:
                        WriteStruct(writer, material.GetMatrix2(property.Name));
                        break;
                    case MaterialPropertyType.Matrix3:
                        WriteStruct(writer, material.GetMatrix3(property.Name));
                        break;
                    case MaterialPropertyType.Matrix4:
                        WriteStruct(writer, material.GetMatrix4(property.Name));
                        break;
                    case MaterialPropertyType.Texture:
                        Texture texture = material.GetTexture(property.Name);
                        writer.Write(texture != null ? texture.ID : string.Empty);
                        break;
                }
            }
        }

        public override GameAsset ReadFromStream(BinaryReader reader)
        {
            string name = reader.ReadString();
            Shader shader = (Shader)AssetManager.Assets[reader.ReadString()];

            int propertiesCount = reader.ReadInt32();

            MaterialProperty[] properties = new MaterialProperty[propertiesCount];
            object[] propData = new object[propertiesCount];
            for (int i = 0; i < propertiesCount; i++)
            {
                string propName = reader.ReadString();
                MaterialPropertyType propertyType = (MaterialPropertyType)reader.ReadInt32();

                switch (propertyType)
                {
                    case MaterialPropertyType.Int:
                        propData[i] = reader.ReadInt32();
                        break;
                    case MaterialPropertyType.Float:
                        propData[i] = reader.ReadSingle();
                        break;
                    case MaterialPropertyType.Vector2:
                        propData[i] = ReadStruct<Vector2>(reader);
                        break;
                    case MaterialPropertyType.Vector3:
                        propData[i] = ReadStruct<Vector3>(reader);
                        break;
                    case MaterialPropertyType.Vector4:
                        propData[i] = ReadStruct<Vector4>(reader);
                        break;
                    case MaterialPropertyType.Matrix2:
                        propData[i] = ReadStruct<Matrix2>(reader);
                        break;
                    case MaterialPropertyType.Matrix3:
                        propData[i] = ReadStruct<Matrix3>(reader);
                        break;
                    case MaterialPropertyType.Matrix4:
                        propData[i] = ReadStruct<Matrix4>(reader);
                        break;
                    case MaterialPropertyType.Texture:
                        string texID = reader.ReadString();
                        propData[i] = !string.IsNullOrEmpty(texID) ? AssetManager.Assets[texID] : null;
                        break;
                }

                properties[i] = new MaterialProperty(propName, propertyType);
            }

            Material material = new Material(name, shader, properties);
            for (int i = 0; i < propertiesCount; i++)
            {
                MaterialProperty property = properties[i];

                switch (properties[i].PropertyType)
                {
                    case MaterialPropertyType.Int:
                        material.SetInt(property.Name, (int)propData[i]);
                        break;
                    case MaterialPropertyType.Float:
                        material.SetFloat(property.Name, (float)propData[i]);
                        break;
                    case MaterialPropertyType.Vector2:
                        material.SetVector2(property.Name, (Vector2)propData[i]);
                        break;
                    case MaterialPropertyType.Vector3:
                        material.SetVector3(property.Name, (Vector3)propData[i]);
                        break;
                    case MaterialPropertyType.Vector4:
                        material.SetVector4(property.Name, (Vector4)propData[i]);
                        break;
                    case MaterialPropertyType.Matrix2:
                        material.SetMatrix2(property.Name, (Matrix2)propData[i]);
                        break;
                    case MaterialPropertyType.Matrix3:
                        material.SetMatrix3(property.Name, (Matrix3)propData[i]);
                        break;
                    case MaterialPropertyType.Matrix4:
                        material.SetMatrix4(property.Name, (Matrix4)propData[i]);
                        break;
                    case MaterialPropertyType.Texture:
                        material.SetTexture(property.Name, (Texture)propData[i]);
                        break;
                }
            }

            material.UpdateBuffer();

            return material;
        }

        private T ReadStruct<T>(BinaryReader reader) where T : struct
        {
            int size = Unsafe.SizeOf<T>();
            IntPtr ptr = Marshal.AllocHGlobal(size);
            byte[] bytes = reader.ReadBytes(size);
            Marshal.Copy(bytes, 0, ptr, size);
            T value = Marshal.PtrToStructure<T>(ptr);
            Marshal.FreeHGlobal(ptr);
            return value;
        }

        private void WriteStruct<T>(BinaryWriter writer, T value) where T : struct
        {
            int size = Unsafe.SizeOf<T>();
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(value, ptr, false);
            byte[] bytes = new byte[size];
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);

            writer.Write(bytes);
        }
    }
}
