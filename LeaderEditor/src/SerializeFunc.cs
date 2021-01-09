using ImGuiNET;
using LeaderEditor.Data;
using LeaderEditor.Gui;
using LeaderEngine;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using Vector2 = System.Numerics.Vector2;

namespace LeaderEditor
{
    public class SerializeFunc
    {
        //serialize transform component
        public static void Transform(Component obj)
        {
            Transform transform = (Transform)obj;

            ImGui.PushID("PositionV3");
            ImGuiExtension.DragVector3("Position", ref transform.LocalPosition, Vector3.Zero, 0.05f);
            ImGui.PopID();

            ImGui.PushID("RotationV3");
            ImGuiExtension.DragVector3("Rotation", ref transform.RotationEuler, Vector3.Zero, 0.5f);
            ImGui.PopID();

            ImGui.PushID("ScaleV3");
            ImGuiExtension.DragVector3("Scale", ref transform.Scale, Vector3.One, 0.05f);
            ImGui.PopID();
        }

        //serialize uitext
        public static void UIText(Component obj)
        {
            UIText uitext = (UIText)obj;

            ImGui.PushID("UITextString");
            string s = uitext.Text;
            ImGui.InputText("Text", ref s, uint.MaxValue);
            uitext.Text = s;
            ImGui.PopID();
        } 

        public static void MeshFilter(Component obj)
        {
            MeshFilter meshFilter = (MeshFilter)obj;

            string preview = meshFilter.Mesh == null ? "[None]" : meshFilter.Mesh.Name;

            if (ImGui.BeginCombo("Select Model", preview))
            {
                foreach (var item in ResourceLoader.LoadedMeshes) 
                {
                    if (ImGui.Selectable(item.Key, meshFilter.Mesh == item.Value))
                        meshFilter.Mesh = item.Value;
                }
                ImGui.EndCombo();
            }
        }

        public static void Skybox(Component obj)
        {
            Skybox skybox = (Skybox)obj;

            if (ImGui.Button("Import Cubemap"))
            {
                using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                {
                    fbd.ShowDialog();

                    if (!string.IsNullOrEmpty(fbd.SelectedPath))
                    {
                        skybox.LoadSkybox(fbd.SelectedPath);
                    }
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
                        string fPath = AssetLoader.LoadAsset(ofd.FileName);

                        if (string.IsNullOrEmpty(fPath))
                            fPath = ofd.FileName;

                        //dispose old texture
                        sprite.Texture?.Dispose();

                        //create new texture
                        sprite.Texture = new LeaderEngine.Texture().FromFile(fPath);
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
            { typeof(float), DefaultFloat },
            { typeof(string), DefaultString },
            { typeof(Vector4), DefaultV4 },
            { typeof(Vector3), DefaultV3 },
        };

        private static void DefaultV3(Component obj, FieldInfo field)
        {
            Vector3 value = (Vector3)field.GetValue(obj);
            ImGuiExtension.DragVector3(field.Name, ref value, Vector3.Zero);
            field.SetValue(obj, value);
        }

        private static void DefaultV4(Component obj, FieldInfo field)
        {
            Vector4 value = (Vector4)field.GetValue(obj);
            ImGuiExtension.DragVector4(field.Name, ref value, Vector4.Zero);
            field.SetValue(obj, value);
        }

        public static void DefaultSerializeFunc(Component obj)
        {
            var fields = obj.GetType().GetFields();
            bool guiDrawn = false;
            foreach (var field in fields)
            {
                if (fieldDrawFuncs.ContainsKey(field.FieldType) && field.IsPublic && !field.IsStatic)
                {
                    ImGui.PushID(field.Name);
                    fieldDrawFuncs[field.FieldType].Invoke(obj, field);
                    ImGui.PopID();
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

        private static void DefaultString(Component obj, FieldInfo field)
        {
            string value = (string)field.GetValue(obj) ?? string.Empty;
            ImGui.InputText(field.Name, ref value, 65535, ImGuiInputTextFlags.Multiline);
            field.SetValue(obj, value);
        }
    }
}
