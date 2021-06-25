using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LeaderEngine
{
    public static class DataManager
    {
        public static Scene CurrentScene { get; set; } = new Scene("Untitled Scene");
        internal static List<Entity> UnlistedEntities { get; } = new List<Entity>();

        public static void SaveSceneToFile(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                BinaryWriter writer = new BinaryWriter(stream);

                Entity[] entitiesToSerialize = CurrentScene.SceneEntities.Where(e => e.Parent == null).ToArray();

                writer.Write(entitiesToSerialize.Length);
                foreach (Entity entity in entitiesToSerialize)
                {
                    RecursivelySerializeEntities(entity, writer);
                }
            }
        }

        public static void LoadSceneFromFile(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                BinaryReader reader = new BinaryReader(stream);

                int entitiesCount = reader.ReadInt32();
                for (int i = 0; i < entitiesCount; i++)
                    RecursivelyDeserializeEntities(reader);
            }
        }

        private static Entity RecursivelyDeserializeEntities(BinaryReader reader)
        {
            string name = reader.ReadString();
            string tag = reader.ReadString();

            bool active = reader.ReadBoolean();

            Entity entity = new Entity(name, tag);
            entity.Active = active;

            Transform transform = entity.Transform;
            transform.Position = ReadStruct<Vector3>(reader);
            transform.EulerAngles = ReadStruct<Vector3>(reader);
            transform.Scale = ReadStruct<Vector3>(reader);

            Component[] components = ReadComponents(reader);
            foreach (Component component in components)
                entity.AddComponent(component);

            int childrenCount = reader.ReadInt32();
            for (int i = 0; i < childrenCount; i++)
                RecursivelyDeserializeEntities(reader).Parent = entity;

            return entity;
        }

        private static Component[] ReadComponents(BinaryReader reader)
        {
            int componentsCount = reader.ReadInt32();
            Component[] components = new Component[componentsCount];

            for (int i = 0; i < componentsCount; i++)
            {
                string typeName = reader.ReadString();
                Type type = Type.GetType(typeName);

                Component component = (Component)Activator.CreateInstance(type);

                DeserializeFields(component, reader);

                components[i] = component;
            }

            return components;
        }

        private static void DeserializeFields(object obj, BinaryReader reader)
        {
            int fieldsCount = reader.ReadInt32();

            for (int i = 0; i < fieldsCount; i++)
            {
                string name = reader.ReadString();
                string typeName = reader.ReadString();

                Type type = Type.GetType(typeName);
                FieldInfo field = obj.GetType().GetField(name);

                if (field.FieldType != type)
                    throw new Exception("Field type mismatch!");

                if (type == typeof(int))
                {
                    field.SetValue(obj, reader.ReadInt32());
                }
                else if (type == typeof(float))
                {
                    field.SetValue(obj, reader.ReadSingle());
                }
                else if (!type.IsClass)
                {
                    object value =
                        typeof(DataManager)
                            .GetMethod("ReadStruct", BindingFlags.Static | BindingFlags.NonPublic)
                            .MakeGenericMethod(type)
                            .Invoke(null, new object[] { reader });

                    field.SetValue(obj, value);
                }
                else if (typeof(GameAsset).IsAssignableFrom(type)) 
                {
                    string id = reader.ReadString();
                    field.SetValue(obj, AssetManager.Assets[id]);
                }
                else if (field.FieldType.IsClass)
                {
                    throw new NotImplementedException();
                }
            }
        }

        private static void RecursivelySerializeEntities(Entity entity, BinaryWriter writer)
        {
            writer.Write(entity.Name);
            writer.Write(entity.Tag);

            writer.Write(entity.Active);

            Transform transform = entity.Transform;
            WriteStruct(writer, transform.Position);
            WriteStruct(writer, transform.EulerAngles);
            WriteStruct(writer, transform.Scale);

            WriteComponents(entity.GetComponents<Component>(), writer);

            writer.Write(entity.Children.Count);
            foreach (Entity child in entity.Children)
                RecursivelySerializeEntities(child, writer);
        }

        private static void WriteComponents(Component[] components, BinaryWriter writer)
        {
            writer.Write(components.Length);

            foreach (Component component in components)
            {
                Type type = component.GetType();

                writer.Write(type.AssemblyQualifiedName);

                SerializeFields(type.GetFields(BindingFlags.Public | BindingFlags.Instance), component, writer);
            }
        }

        private static void SerializeFields(FieldInfo[] fields, object obj, BinaryWriter writer)
        {
            writer.Write(fields.Length);

            foreach (FieldInfo field in fields)
            {
                writer.Write(field.Name);
                writer.Write(field.FieldType.AssemblyQualifiedName);

                //yandere dev moment
                if (field.FieldType == typeof(int))
                {
                    writer.Write((int)field.GetValue(obj));
                }
                else if (field.FieldType == typeof(float))
                {
                    writer.Write((float)field.GetValue(obj));
                }
                else if (!field.FieldType.IsClass)
                {
                    typeof(DataManager)
                        .GetMethod("WriteStruct", BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(field.FieldType)
                        .Invoke(null, new object[] { writer, field.GetValue(obj) });
                }
                else if (typeof(GameAsset).IsAssignableFrom(field.FieldType))
                {
                    GameAsset asset = (GameAsset)field.GetValue(obj);
                    writer.Write(asset.ID);
                }
                else if (field.FieldType.IsClass)
                {
                    throw new NotImplementedException();
                }
            }
        }

        private static T ReadStruct<T>(BinaryReader reader) where T : struct
        {
            int size = Unsafe.SizeOf<T>();
            IntPtr ptr = Marshal.AllocHGlobal(size);
            byte[] bytes = reader.ReadBytes(size);
            Marshal.Copy(bytes, 0, ptr, size);
            T value = Marshal.PtrToStructure<T>(ptr);
            Marshal.FreeHGlobal(ptr);
            return value;
        }

        private static void WriteStruct<T>(BinaryWriter writer, T value) where T : struct
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
