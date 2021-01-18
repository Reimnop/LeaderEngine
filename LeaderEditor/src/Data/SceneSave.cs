using LeaderEngine;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Reflection;

namespace LeaderEditor.Data
{
    public static class SceneSave
    {
        private static readonly Type[] SceneSerializableTypes = new Type[]
        {
            typeof(string),
            typeof(int),
            typeof(float),
            typeof(Vector4),
            typeof(Vector3),
            typeof(Mesh)
        };

        public static void SaveScene(string path)
        {
            SceneInfo sceneInfo = ProcessScene();
            string json = JsonConvert.SerializeObject(sceneInfo, Formatting.Indented, SceneCommons.JsonConverters);

            File.WriteAllText(path, json);
        }

        private static SceneInfo ProcessScene()
        {
            SceneInfo sceneInfo = new SceneInfo();
            sceneInfo.Models = ResourceLoader.LoadedMeshes.Select(x => x.Key).ToArray();
            sceneInfo.EditorCamPosition = EditorCamera.Main.BaseTransform.LocalPosition;
            sceneInfo.EditorCamRotation = EditorCamera.Main.BaseTransform.RotationEuler;

            var sceneObjectsSurf = SceneHierachy.SceneObjects.Where(x => x.Parent == null).ToArray();

            sceneInfo.Entities = new EntityInfo[sceneObjectsSurf.Length];

            for (int i = 0; i < sceneInfo.Entities.Length; i++)
                sceneInfo.Entities[i] = ProcessEntity(SceneHierachy.SceneObjects[i]);

            return sceneInfo;
        }

        private static EntityInfo ProcessEntity(Entity entity)
        {
            EntityInfo entityInfo = new EntityInfo();
            entityInfo.Name = entity.Name;
            entityInfo.Active = entity.ActiveSelf;
            entityInfo.RenderHint = entity.RenderHint;

            var components = entity.GetAllComponents();

            entityInfo.Components = new ComponentInfo[components.Count];

            for (int i = 0; i < components.Count; i++)
                entityInfo.Components[i] = ProcessComponent(components[i]);

            entityInfo.Children = new EntityInfo[entity.Children.Count];

            for (int i = 0; i < entity.Children.Count; i++)
                entityInfo.Children[i] = ProcessEntity(entity.Children[i]);

            return entityInfo;
        }

        private static ComponentInfo ProcessComponent(Component component)
        {
            ComponentInfo componentInfo = new ComponentInfo();
            componentInfo.Name = component.GetType().FullName;
            componentInfo.AssemblyName = component.GetType().Assembly.GetName().Name;

            FieldInfo[] fields = component.GetType()
                .GetFields()
                .Where(x => x.IsPublic && SceneSerializableTypes.Contains(x.FieldType))
                .ToArray();

            componentInfo.Fields = new ComponentFieldInfo[fields.Length];

            for (int i = 0; i < fields.Length; i++)
                componentInfo.Fields[i] = ProcessField(component, fields[i]);

            return componentInfo;
        }

        private static ComponentFieldInfo ProcessField(Component comp, FieldInfo fieldInfo)
        {
            ComponentFieldInfo componentFieldInfo = new ComponentFieldInfo();
            componentFieldInfo.Name = fieldInfo.Name;
            componentFieldInfo.TypeName = fieldInfo.FieldType.FullName;
            componentFieldInfo.AssemblyName = fieldInfo.FieldType.Assembly.GetName().Name;
            componentFieldInfo.DataJson = JsonConvert.SerializeObject(fieldInfo.GetValue(comp), SceneCommons.JsonConverters);

            return componentFieldInfo;
        }
    }
}
