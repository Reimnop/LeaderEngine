using LeaderEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LeaderEditor.Data
{
    public static class ComponentSerializer
    {
        private static Dictionary<Type, Func<Component, byte[]>> serializeFuncs = new Dictionary<Type, Func<Component, byte[]>>()
        {
            { typeof(Transform), SerializeTransform }
        };

        public static byte[] SerializeComponent(Component component)
        {
            if (!serializeFuncs.ContainsKey(component.GetType()))
            {
                DebugConsole.Log($"Type {component.GetType().Name} not serializable!", LogType.Warning);
                return new byte[0];
            }
            return serializeFuncs[component.GetType()](component);
        }

        private static byte[] SerializeTransform(Component component)
        {
            Transform transform = (Transform)component;

            using (var ms = new MemoryStream())
            {
                //write position
                ms.Write(BitConverter.GetBytes(transform.position.X));
                ms.Write(BitConverter.GetBytes(transform.position.Y));
                ms.Write(BitConverter.GetBytes(transform.position.Z));

                //write rotation euler
                ms.Write(BitConverter.GetBytes(transform.rotation.X));
                ms.Write(BitConverter.GetBytes(transform.rotation.Y));
                ms.Write(BitConverter.GetBytes(transform.rotation.Z));

                //write scale
                ms.Write(BitConverter.GetBytes(transform.scale.X));
                ms.Write(BitConverter.GetBytes(transform.scale.Y));
                ms.Write(BitConverter.GetBytes(transform.scale.Z));

                return ms.ToArray();
            }
        }
    }
}
