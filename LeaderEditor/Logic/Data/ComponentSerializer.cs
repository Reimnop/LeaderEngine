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
                return null;
            }
            using (var ms = new MemoryStream())
            {
                byte[] compData = serializeFuncs[component.GetType()](component);

                string fullName = component.GetType().FullName;

                //write component name length
                ms.Write(BitConverter.GetBytes(fullName.Length));

                //write component full name
                ms.Write(Encoding.ASCII.GetBytes(fullName));

                //write component data length
                ms.Write(BitConverter.GetBytes(compData.Length));

                //write component data
                ms.Write(compData);

                return ms.ToArray();
            }
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
