using ImGuiNET;
using LeaderEngine;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Reflection;

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

        //default serializers
        private static Dictionary<Type, Action<Component, FieldInfo>> fieldDrawFuncs = new Dictionary<Type, Action<Component, FieldInfo>>()
        {
            { typeof(int), DefaultInt },
            { typeof(float), DefaultFloat },
            { typeof(string), DefaultString },
            { typeof(Vector4), DefaultV4 },
            { typeof(Vector3), DefaultV3 },
            { typeof(Vector2), DefaultV2 }
        };

        private static void DefaultV2(Component obj, FieldInfo field)
        {
            Vector2 value = (Vector2)field.GetValue(obj);
            ImGuiExtension.DragVector2(field.Name, ref value, Vector2.Zero);
            field.SetValue(obj, value);
        }

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
                if (fieldDrawFuncs.TryGetValue(field.FieldType, out Action<Component, FieldInfo> drawFunc) && field.IsPublic && !field.IsStatic)
                {
                    ImGui.PushID(field.Name);
                    drawFunc.Invoke(obj, field);
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