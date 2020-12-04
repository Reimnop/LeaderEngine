using ImGuiNET;
using LeaderEngine;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using LeaderEditor.Gui;
using System.IO;
using OpenTK.Mathematics;
using System.Reflection;

using Vector2 = System.Numerics.Vector2;

namespace LeaderEditor.Logic
{
    public class SerializeFunc
    {
        //serialize transform component
        public static void Transform(Component obj)
        {
            Transform transform = (Transform)obj;

            ImGuiExtension.DragVector3("Position", ref transform.position, Vector3.Zero, 0.05f);
            ImGuiExtension.DragVector3("Rotation", ref transform.rotationEuler, Vector3.Zero, 0.05f);
            ImGuiExtension.DragVector3("Scale", ref transform.scale, Vector3.One, 0.05f);
        }

        //TODO: finish meshfilter serialization functin
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

        //sprite serialization function
        public static void Sprite(Component obj)
        {
            Sprite sprite = (Sprite)obj;

            //import button
            if (ImGui.Button("Import PNG"))
            {
                //file chooser
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "*.png|*.png";
                    ofd.Multiselect = false;

                    ofd.ShowDialog();

                    if (!string.IsNullOrEmpty(ofd.FileName))
                    {
                        //dispose old texture
                        sprite.Texture?.Dispose();

                        //create new texture
                        sprite.Texture = new LeaderEngine.Texture().FromFile(ofd.FileName);
                    }
                }
            }

            ImGui.SameLine();

            //color edit gui
            System.Numerics.Vector4 col = new System.Numerics.Vector4(sprite.Color.R, sprite.Color.G, sprite.Color.B, sprite.Color.A);
            ImGui.ColorEdit4("Color", ref col);
            sprite.Color = new Color4(col.X, col.Y, col.Z, col.W);

            //draw a preview of the texture
            if (sprite.Texture != null)
                ImGui.Image((IntPtr)sprite.Texture.GetHandle(), new Vector2(sprite.Texture.Size.X, sprite.Texture.Size.Y) / 2.0f);
        }

        //default serializers
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
