using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace LeaderEngine
{
    public enum MaterialPropType
    {
        Int,
        Float,
        Vector3,
        Vector4
    }

    public struct MaterialProp
    {
        public MaterialPropType PropType;
        public object Data;
    }

    public sealed class Material : IDisposable
    {
        public readonly string Name;
        public readonly string ID;

        private static Dictionary<MaterialPropType, int> propSizes { get; } = new Dictionary<MaterialPropType, int>()
        {
            { MaterialPropType.Int, sizeof(int) },
            { MaterialPropType.Float, sizeof(float) },
            { MaterialPropType.Vector3, sizeof(float) * 3 },
            { MaterialPropType.Vector4, sizeof(float) * 4 }
        };

        internal Dictionary<string, MaterialProp> MaterialProps = new Dictionary<string, MaterialProp>();
        internal Dictionary<TextureUnit, Texture> MaterialTextures = new Dictionary<TextureUnit, Texture>();

        public Material(string name, string id = null)
        {
            Name = name;

            ID = id ?? RNG.GetRandomID();

            GlobalData.Materials.Add(ID, this);
        }

        #region SetMethods
        public void SetInt(string name, int value)
        {
            MaterialProps.SetOrAdd(name, new MaterialProp
            {
                PropType = MaterialPropType.Int,
                Data = value
            });
        }

        public void SetFloat(string name, float value)
        {
            MaterialProps.SetOrAdd(name, new MaterialProp
            {
                PropType = MaterialPropType.Float,
                Data = value
            });
        }

        public void SetVector3(string name, Vector3 value)
        {
            MaterialProps.SetOrAdd(name, new MaterialProp
            {
                PropType = MaterialPropType.Vector3,
                Data = value
            });
        }

        public void SetVector4(string name, Vector4 value)
        {
            MaterialProps.SetOrAdd(name, new MaterialProp
            {
                PropType = MaterialPropType.Vector4,
                Data = value
            });
        }

        public void SetTexture2D(TextureUnit unit, Texture texture)
        {
            MaterialTextures.SetOrAdd(unit, texture);
        }
        #endregion

        public void Serialize(BinaryWriter writer)
        {
            //write name
            writer.Write(Name);
            //write id
            writer.Write(ID);
            //write prop count
            writer.Write(MaterialProps.Count);
            //write mat props
            foreach (var prop in MaterialProps)
            {
                //write name
                writer.Write(prop.Key);

                int size = propSizes[prop.Value.PropType];

                //write prop type
                writer.Write((int)prop.Value.PropType);

                //convert prop data to ptr
                IntPtr dataPtr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(prop.Value.Data, dataPtr, true);

                //convert ptr to byte[]
                byte[] dataArr = new byte[size];

                //copy ptr to byte[]
                Marshal.Copy(dataPtr, dataArr, 0, size);

                //write bytes
                writer.Write(dataArr);

                //free memory
                Marshal.FreeHGlobal(dataPtr);
            }

            //write textures count
            writer.Write(MaterialTextures.Count);
            //write mat textures
            foreach (var tex in MaterialTextures)
            {
                //write unit
                writer.Write((int)tex.Key);
                //write texture
                writer.Write(tex.Value.ID);
            }
        }

        public static Material Deserialize(BinaryReader reader)
        {
            //read name
            string name = reader.ReadString();
            //read id
            string id = reader.ReadString();

            //read props
            int propCount = reader.ReadInt32();

            Dictionary<string, MaterialProp> materialProps = new Dictionary<string, MaterialProp>();
            for (int i = 0; i < propCount; i++)
            {
                //read name
                string pName = reader.ReadString();
                //read prop type
                MaterialPropType propType = (MaterialPropType)reader.ReadInt32();

                int size = propSizes[propType];

                //read bytes
                byte[] data = reader.ReadBytes(size);

                GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);

                object pData = null;
                switch (propType)
                {
                    case MaterialPropType.Int:
                        pData = Marshal.PtrToStructure<int>(handle.AddrOfPinnedObject());
                        break;
                    case MaterialPropType.Float:
                        pData = Marshal.PtrToStructure<float>(handle.AddrOfPinnedObject());
                        break;
                    case MaterialPropType.Vector3:
                        pData = Marshal.PtrToStructure<Vector3>(handle.AddrOfPinnedObject());
                        break;
                    case MaterialPropType.Vector4:
                        pData = Marshal.PtrToStructure<Vector4>(handle.AddrOfPinnedObject());
                        break;
                }

                handle.Free();

                materialProps.Add(pName, new MaterialProp
                {
                    PropType = propType,
                    Data = pData
                });
            }

            //read textures
            int textureCount = reader.ReadInt32();

            Dictionary<TextureUnit, Texture> textures = new Dictionary<TextureUnit, Texture>();
            for (int i = 0; i < textureCount; i++)
            {
                textures.Add(
                    (TextureUnit)reader.ReadInt32(),
                    GlobalData.Textures[reader.ReadString()]
                );
            }

            Material mat = new Material(name, id);
            mat.MaterialProps = materialProps;
            mat.MaterialTextures = textures;

            return mat;
        }

        public void Use(Shader shader)
        {
            foreach (var prop in MaterialProps)
            {
                switch (prop.Value.PropType)
                {
                    case MaterialPropType.Int:
                        shader.SetInt(prop.Key, (int)prop.Value.Data);
                        break;
                    case MaterialPropType.Float:
                        shader.SetFloat(prop.Key, (float)prop.Value.Data);
                        break;
                    case MaterialPropType.Vector3:
                        shader.SetVector3(prop.Key, (Vector3)prop.Value.Data);
                        break;
                    case MaterialPropType.Vector4:
                        shader.SetVector4(prop.Key, (Vector4)prop.Value.Data);
                        break;
                }
            }

            foreach (var tex in MaterialTextures)
            {
                tex.Value.Use(tex.Key);
            }
        }

        public void Dispose()
        {
            GlobalData.Materials.Remove(ID);
        }
    }
}
