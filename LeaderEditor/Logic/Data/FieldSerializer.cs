using LeaderEngine;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace LeaderEditor.Data
{
    public static class FieldSerializer
    {
        private static Dictionary<Type, Func<FieldInfo, Component, byte[]>> fieldSerializeFuncs = new Dictionary<Type, Func<FieldInfo, Component, byte[]>>()
        {
            { typeof(float), SerializeFloat },
            { typeof(int), SerializeInt },
            { typeof(Vector3), SerializeVector3 },
            { typeof(Vector2), SerializeVector2 },
            { typeof(Color4), SerializeColor4 }
        };

        public static byte[] SerializeFields(Component component)
        {
            var fields = component.GetType().GetFields();

            using (var ms = new MemoryStream())
            {
                foreach (var field in fields)
                {
                    if (!field.IsPublic || field.IsStatic)
                        continue;

                    var serializeFieldFunc = GetFieldSerializeFunc(field.FieldType);

                    if (serializeFieldFunc == null)
                        continue;

                    var fieldBytes = serializeFieldFunc(field, component);

                    //write data length
                    ms.Write(BitConverter.GetBytes(fieldBytes.Length));

                    //write data
                    ms.Write(fieldBytes);
                }

                return ms.ToArray();
            }
        }

        private static Func<FieldInfo, Component, byte[]> GetFieldSerializeFunc(Type type)
        {
            if (!fieldSerializeFuncs.ContainsKey(type))
            {
                DebugConsole.Log($"No suitable method found to serialize {type.Name}, skipping", LogType.Warning);
                return null;
            }
            return fieldSerializeFuncs[type];
        }

        private static byte[] SerializeFloat(FieldInfo fieldInfo, Component component)
        {
            using (var ms = new MemoryStream())
            {
                //write field name length
                ms.Write(BitConverter.GetBytes(fieldInfo.Name.Length));

                //write field name
                ms.Write(Encoding.ASCII.GetBytes(fieldInfo.Name));

                //write field data
                ms.Write(BitConverter.GetBytes((float)fieldInfo.GetValue(component)));

                return ms.ToArray();
            }
        }

        private static byte[] SerializeInt(FieldInfo fieldInfo, Component component)
        {
            using (var ms = new MemoryStream())
            {
                //write field name length
                ms.Write(BitConverter.GetBytes(fieldInfo.Name.Length));

                //write field name
                ms.Write(Encoding.ASCII.GetBytes(fieldInfo.Name));

                //write field data
                ms.Write(BitConverter.GetBytes((int)fieldInfo.GetValue(component)));

                return ms.ToArray();
            }
        }

        private static byte[] SerializeVector3(FieldInfo fieldInfo, Component component)
        {
            using (var ms = new MemoryStream())
            {
                //write field name length
                ms.Write(BitConverter.GetBytes(fieldInfo.Name.Length));

                //write field name
                ms.Write(Encoding.ASCII.GetBytes(fieldInfo.Name));

                Vector3 data = (Vector3)fieldInfo.GetValue(component);

                //write data
                ms.Write(BitConverter.GetBytes(data.X));
                ms.Write(BitConverter.GetBytes(data.Y));
                ms.Write(BitConverter.GetBytes(data.Z));

                return ms.ToArray();
            }
        }

        private static byte[] SerializeVector2(FieldInfo fieldInfo, Component component)
        {
            using (var ms = new MemoryStream())
            {
                //write field name length
                ms.Write(BitConverter.GetBytes(fieldInfo.Name.Length));

                //write field name
                ms.Write(Encoding.ASCII.GetBytes(fieldInfo.Name));

                Vector2 data = (Vector2)fieldInfo.GetValue(component);

                //write data
                ms.Write(BitConverter.GetBytes(data.X));
                ms.Write(BitConverter.GetBytes(data.Y));

                return ms.ToArray();
            }
        }

        private static byte[] SerializeColor4(FieldInfo fieldInfo, Component component)
        {
            using (var ms = new MemoryStream())
            {
                //write field name length
                ms.Write(BitConverter.GetBytes(fieldInfo.Name.Length));

                //write field name
                ms.Write(Encoding.ASCII.GetBytes(fieldInfo.Name));

                Color4 data = (Color4)fieldInfo.GetValue(component);

                //write data
                ms.Write(BitConverter.GetBytes(data.R));
                ms.Write(BitConverter.GetBytes(data.G));
                ms.Write(BitConverter.GetBytes(data.B));
                ms.Write(BitConverter.GetBytes(data.A));

                return ms.ToArray();
            }
        }
    }
}
