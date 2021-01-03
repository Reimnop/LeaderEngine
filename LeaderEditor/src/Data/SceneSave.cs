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
            string json = JsonConvert.SerializeObject(sceneInfo, Formatting.Indented,
                new Vector4Converter(),
                new Vector3Converter(),
                new MeshConverter());

            File.WriteAllText(path, json);
        }

        private static SceneInfo ProcessScene()
        {
            SceneInfo sceneInfo = new SceneInfo();
            sceneInfo.Models = ResourceLoader.LoadedModels.Select(x => x.Key).ToArray();
            sceneInfo.EditorCamPosition = EditorCamera.Main.transform.Position;
            sceneInfo.EditorCamRotation = EditorCamera.Main.transform.RotationEuler;
            sceneInfo.GameObjects = new GameObjectInfo[SceneHierachy.SceneObjects.Count];

            for (int i = 0; i < sceneInfo.GameObjects.Length; i++)
                sceneInfo.GameObjects[i] = ProcessGameObject(SceneHierachy.SceneObjects[i]);

            return sceneInfo;
        }

        private static GameObjectInfo ProcessGameObject(GameObject gameObject)
        {
            GameObjectInfo gameObjectInfo = new GameObjectInfo();
            gameObjectInfo.Name = gameObject.Name;
            gameObjectInfo.Active = gameObject.ActiveSelf;
            gameObjectInfo.RenderHint = gameObject.RenderHint;

            var components = gameObject.GetAllComponents();

            gameObjectInfo.Components = new ComponentInfo[components.Count];

            for (int i = 0; i < components.Count; i++)
                gameObjectInfo.Components[i] = ProcessComponent(components[i]);

            return gameObjectInfo;
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
            componentFieldInfo.Data = fieldInfo.GetValue(comp);

            return componentFieldInfo;
        }
    }
}
