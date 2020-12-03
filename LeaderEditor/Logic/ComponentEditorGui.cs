using ImGuiNET;
using LeaderEngine;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Forms;
using System.Text;
using LeaderEditor.Gui;
using System.IO;
using OpenTK.Mathematics;

using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace LeaderEditor.Logic
{
    public class ComponentEditorGui
    {
        private struct Attribute
        {
            public string Name;
            public object Data;
        }

        private static Dictionary<object, List<Attribute>> attributes = new Dictionary<object, List<Attribute>>();

        private static void AttachAttribute(object o, string name, object data)
        {
            //TODO: attribute system
        }

        public static void Transform(Component obj)
        {
            Transform transform = (Transform)obj;

            Vector3 posSys = transform.position.ToSystemVector3();
            ImGui.DragFloat3("Position", ref posSys, 0.05f);
            transform.position = posSys.ToOTKVector3();

            Vector3 rotSys = new Vector3(
                MathHelper.RadiansToDegrees(transform.rotationEuler.X),
                MathHelper.RadiansToDegrees(transform.rotationEuler.Y),
                MathHelper.RadiansToDegrees(transform.rotationEuler.Z));
            ImGui.DragFloat3("Rotation", ref rotSys, 1.0f);
            transform.rotationEuler = new OpenTK.Mathematics.Vector3(
                MathHelper.DegreesToRadians(rotSys.X),
                MathHelper.DegreesToRadians(rotSys.Y),
                MathHelper.DegreesToRadians(rotSys.Z));

            Vector3 scaleSys = transform.scale.ToSystemVector3();
            ImGui.DragFloat3("Scale", ref scaleSys, 0.05f);
            transform.scale = scaleSys.ToOTKVector3();
        }

        public static void MeshFilter(Component obj)
        {
            MeshFilter meshFilter = (MeshFilter)obj;

            if (ImGui.Button("Import FBX"))
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "*.fbx|*.fbx";
                    ofd.Multiselect = false;

                    ofd.ShowDialog();
                }
            }
        }

        public static void Sprite(Component obj)
        {
            Sprite sprite = (Sprite)obj;

            if (ImGui.Button("Import PNG"))
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "*.png|*.png";
                    ofd.Multiselect = false;

                    ofd.ShowDialog();

                    if (!string.IsNullOrEmpty(ofd.FileName))
                    {
                        sprite.Texture = new LeaderEngine.Texture().FromFile(ofd.FileName);
                    }
                }
            }

            if (sprite.Texture != null)
                ImGui.Image((IntPtr)sprite.Texture.GetHandle(), new Vector2(sprite.Texture.Size.X, sprite.Texture.Size.Y) / 2.0f);
        }
    }
}
