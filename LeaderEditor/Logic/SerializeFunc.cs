using ImGuiNET;
using LeaderEngine;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace LeaderEditor
{
    public class SerializeFunc
    {
        //serialize transform component
        public static void Transform(Transform transform)
        {
            ImGui.PushID("PositionV3");
            ImGuiExtension.DragVector3("Position", ref transform.Position, Vector3.Zero, 0.05f);
            ImGui.PopID();

            ImGui.PushID("RotationV3");
            Vector3 euler = transform.EulerAngles;
            ImGuiExtension.DragVector3("Rotation", ref euler, Vector3.Zero, 0.5f);
            transform.EulerAngles = euler;
            ImGui.PopID();

            ImGui.PushID("ScaleV3");
            ImGuiExtension.DragVector3("Scale", ref transform.Scale, Vector3.One, 0.05f);
            ImGui.PopID();
        }

        internal static void SerializeTextRenderer(Component obj)
        {
            TextRenderer textRenderer = (TextRenderer)obj;
            
            textRenderer.Text = (string)DefaultString("Text", textRenderer.Text);
            textRenderer.Font = (Font)SerializeGameAsset("Font", textRenderer.Font, typeof(Font));

            ImGui.Text("Alignment");
            if (ImGui.BeginCombo("Horizontal", textRenderer.HorizontalAlignment.ToString()))
            {
                if (ImGui.Selectable("Left", textRenderer.HorizontalAlignment == HorizontalAlignment.Left))
                    textRenderer.HorizontalAlignment = HorizontalAlignment.Left;

                if (ImGui.Selectable("Center", textRenderer.HorizontalAlignment == HorizontalAlignment.Center))
                    textRenderer.HorizontalAlignment = HorizontalAlignment.Center;

                if (ImGui.Selectable("Right", textRenderer.HorizontalAlignment == HorizontalAlignment.Right))
                    textRenderer.HorizontalAlignment = HorizontalAlignment.Right;

                ImGui.EndCombo();
            }

            if (ImGui.BeginCombo("Vertical", textRenderer.VerticalAlignment.ToString()))
            {
                if (ImGui.Selectable("Top", textRenderer.VerticalAlignment == VerticalAlignment.Top))
                    textRenderer.VerticalAlignment = VerticalAlignment.Top;

                if (ImGui.Selectable("Center", textRenderer.VerticalAlignment == VerticalAlignment.Center))
                    textRenderer.VerticalAlignment = VerticalAlignment.Center;

                if (ImGui.Selectable("Bottom", textRenderer.VerticalAlignment == VerticalAlignment.Bottom))
                    textRenderer.VerticalAlignment = VerticalAlignment.Bottom;

                ImGui.EndCombo();
            }
        }

        public static void SerializeAudioSource(Component obj)
        {
            AudioSource source = (AudioSource)obj;

            float gain = source.Gain;
            ImGui.DragFloat("Gain", ref gain, 0.1f);
            source.Gain = gain;

            float pitch = source.Pitch;
            ImGui.DragFloat("Pitch", ref pitch, 0.1f);
            source.Pitch = pitch;

            bool loop = source.Looping;
            ImGui.Checkbox("Looping", ref loop);
            source.Looping = loop;

            source.Clip = (AudioClip)SerializeGameAsset("Clip", source.Clip, typeof(AudioClip));

            if (ImGui.Button("Play"))
                source.Play();

            ImGui.SameLine();

            if (ImGui.Button("Pause"))
                source.Pause();

            ImGui.SameLine();

            if (ImGui.Button("Stop"))
                source.Stop();
        }

        public static void DefaultSerializeFunc(Component obj)
        {
            bool guiDrawn = false;

            //serialize fields
            var fields = obj.GetType().GetFields();
            foreach (var field in fields)
            {
                if (typeof(GameAsset).IsAssignableFrom(field.FieldType))
                {
                    ImGui.PushID(field.Name);
                    field.SetValue(obj, SerializeGameAsset(field.Name, (GameAsset)field.GetValue(obj), field.FieldType));
                    ImGui.PopID();
                    continue;
                }

                if (fieldDrawFuncs.TryGetValue(field.FieldType, out var drawFunc) && field.IsPublic && !field.IsStatic)
                {
                    ImGui.PushID(field.Name);
                    field.SetValue(obj, drawFunc.Invoke(field.Name, field.GetValue(obj)));
                    ImGui.PopID();
                    guiDrawn = true;
                }
            }

            //serialize properties
            var props = obj.GetType().GetProperties();
            foreach (var prop in props)
            {
                if (typeof(GameAsset).IsAssignableFrom(prop.PropertyType))
                {
                    ImGui.PushID(prop.Name);
                    prop.SetValue(obj, SerializeGameAsset(prop.Name, (GameAsset)prop.GetValue(obj), prop.PropertyType));
                    ImGui.PopID();
                    continue;
                }

                if (fieldDrawFuncs.TryGetValue(prop.PropertyType, out var drawFunc))
                {
                    ImGui.PushID(prop.Name);
                    prop.SetValue(obj, drawFunc.Invoke(prop.Name, prop.GetValue(obj)));
                    ImGui.PopID();
                    guiDrawn = true;
                }
            }

            if (!guiDrawn)
                ImGui.Text("No property");
        }

        private static GameAsset SerializeGameAsset(string name, GameAsset gameAsset, Type type)
        {
            if (ImGui.BeginCombo(name, gameAsset != null ? gameAsset.Name : "[None]"))
            {
                foreach (GameAsset asset in LeaderEngine.AssetManager.Assets)
                {
                    if (!type.IsAssignableFrom(asset.GetType()))
                        continue;

                    ImGui.PushID(asset.GetHashCode());
                    if (ImGui.Selectable(asset.Name, gameAsset == asset))
                        gameAsset = asset;
                    ImGui.PopID();
                }

                ImGui.EndCombo();
            }

            return gameAsset;
        }

        //default serializers
        private static Dictionary<Type, Func<string, object, object>> fieldDrawFuncs = new Dictionary<Type, Func<string, object, object>>()
        {
            { typeof(int), DefaultInt },
            { typeof(float), DefaultFloat },
            { typeof(string), DefaultString },
            { typeof(Vector4), DefaultV4 },
            { typeof(Vector3), DefaultV3 },
            { typeof(Vector2), DefaultV2 }
        };

        private static object DefaultV2(string name, object obj)
        {
            Vector2 value = (Vector2)obj;
            ImGuiExtension.DragVector2(name, ref value, Vector2.Zero);
            return value;
        }

        private static object DefaultV3(string name, object obj)
        {
            Vector3 value = (Vector3)obj;
            ImGuiExtension.DragVector3(name, ref value, Vector3.Zero);
            return value;
        }

        private static object DefaultV4(string name, object obj)
        {
            Vector4 value = (Vector4)obj;
            ImGuiExtension.DragVector4(name, ref value, Vector4.Zero);
            return value;
        }

        private static object DefaultInt(string name, object obj)
        {
            int value = (int)obj;
            ImGui.DragInt(name, ref value);
            return value;
        }

        private static object DefaultFloat(string name, object obj)
        {
            float value = (float)obj;
            ImGui.DragFloat(name, ref value, 0.05f);
            return value;
        }

        private static object DefaultString(string name, object obj)
        {
            string value = (string)obj ?? string.Empty;
            ImGui.InputText(name, ref value, 65535, ImGuiInputTextFlags.Multiline);
            return value;
        }
    }
}