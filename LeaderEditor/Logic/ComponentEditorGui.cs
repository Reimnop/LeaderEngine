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
using LeaderEditor.Logic.Memes;
using System.Reflection;

using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace LeaderEditor.Logic
{
    public class SerializeFunc
    {
        public static void Transform(Component obj)
        {
            Transform transform = (Transform)obj;

            Vector3 posSys = transform.position.ToSystemVector3();
            ImGui.DragFloat3("Position", ref posSys, 0.05f);
            transform.position = posSys.ToOTKVector3();

            Vector3 rotSys = transform.rotationEuler.ToSystemVector3();
            ImGui.DragFloat3("Rotation", ref rotSys, 0.05f);
            transform.rotationEuler = rotSys.ToOTKVector3();

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

        private static Dictionary<Type, Action<Component, FieldInfo>> fieldDrawFuncs = new Dictionary<Type, Action<Component, FieldInfo>>()
        {
            { typeof(int), DefaultInt },
            { typeof(float), DefaultFloat }
        };

        public static void DefaultSerializeFunc(Component obj)
        {
            var fields = obj.GetType().GetFields();
            bool guiDrawn = false;
            foreach (var field in fields)
            {
                if (fieldDrawFuncs.ContainsKey(field.FieldType) && field.IsPublic)
                {
                    fieldDrawFuncs[field.FieldType].Invoke(obj, field);
                    guiDrawn = true;
                }
            }

            if (!guiDrawn)
                ImGui.Text("No property");
        }

        private static void DefaultInt(Component obj, FieldInfo field)
        {
            int value = (int)field.GetValue(obj);
            ImGui.DragInt(field.Name, ref value);
            field.SetValue(obj, value);
        }

        private static void DefaultFloat(Component obj, FieldInfo field)
        {
            float value = (float)field.GetValue(obj);
            ImGui.DragFloat(field.Name, ref value, 0.05f);
            field.SetValue(obj, value);
        }
    }
}
