using LeaderEngine;
using System;
using System.IO;
using System.Text;

namespace LeaderEditor.Data
{
    public static class ComponentSerializer
    {
        public static byte[] SerializeComponent(Component component)
        {
            using (var ms = new MemoryStream())
            {
                byte[] compData = FieldSerializer.SerializeFields(component);

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
    }
}
