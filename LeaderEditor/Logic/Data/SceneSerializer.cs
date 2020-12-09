using LeaderEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace LeaderEditor.Data
{
    public static class SceneSerializer
    {
        public static void SaveScene(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                fs.Write(SerializeScene());

                fs.Close();
            }
        }

        private static byte[] SerializeScene()
        {
            using (var ms = new MemoryStream())
            {
                var sceneObjects = SceneHierachy.SceneObjects;

                foreach (var obj in sceneObjects)
                {
                    //write name length
                    ms.Write(BitConverter.GetBytes(obj.Name.Length));

                    //write name
                    ms.Write(Encoding.ASCII.GetBytes(obj.Name));

                    byte[] allCompsBytes = SerializeComponents(obj.GetAllComponents().ToArray());

                    //write all components size
                    ms.Write(BitConverter.GetBytes(allCompsBytes.Length));

                    //write all components data
                    ms.Write(allCompsBytes);
                }
                return ms.ToArray();
            }
        }

        private static byte[] SerializeComponents(Component[] components)
        {
            using (var ms = new MemoryStream())
            {
                foreach (var comp in components)
                {
                    byte[] compBytes = ComponentSerializer.SerializeComponent(comp);

                    if (compBytes == null)
                        continue;

                    //write component size
                    ms.Write(BitConverter.GetBytes(compBytes.Length));

                    //write component data
                    ms.Write(compBytes);
                }
                return ms.ToArray();
            }
        }
    }
}
