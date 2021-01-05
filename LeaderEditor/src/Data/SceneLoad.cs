using LeaderEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace LeaderEditor.Data
{
    public static class SceneLoad
    {
        public static void LoadScene(string path)
        {
            SceneInfo sceneInfo = JsonConvert.DeserializeObject<SceneInfo>(File.ReadAllText(path), SceneCommons.JsonConverters);

            ProcessScene(sceneInfo);
        }

        private static void ProcessScene(SceneInfo sceneInfo)
        {
            for (int i = 0; i < sceneInfo.Models.Length; i++)
                ResourceLoader.LoadModel(Path.Combine(AssetLoader.LoadedProjectDir, "Assets", sceneInfo.Models[i]));

            EditorCamera.Main.transform.LocalPosition = sceneInfo.EditorCamPosition;
            EditorCamera.Main.transform.RotationEuler = sceneInfo.EditorCamRotation;
            GameObjectInfo[] gameObjectInfos = sceneInfo.GameObjects;

            for (int i = 0; i < gameObjectInfos.Length; i++)
                ProcessGameObject(gameObjectInfos[i]);
        }

        private static void ProcessGameObject(GameObjectInfo gameObjectInfo)
        {
            GameObject gameObject = new GameObject(gameObjectInfo.Name, gameObjectInfo.RenderHint);
            gameObject.SetActive(gameObjectInfo.Active);

            ComponentInfo[] componentInfos = gameObjectInfo.Components;

            for (int i = 0; i < componentInfos.Length; i++)
                ProcessComponent(gameObject, componentInfos[i]);

            SceneHierachy.SceneObjects.Add(gameObject);
        }

        private static void ProcessComponent(GameObject gameObject, ComponentInfo componentInfo)
        {
            Assembly asm = GetAssemblyByName(componentInfo.AssemblyName);
            Component component = (Component)Activator.CreateInstance(asm.GetType(componentInfo.Name));

            ComponentFieldInfo[] componentFieldInfos = componentInfo.Fields;

            for (int i = 0; i < componentFieldInfos.Length; i++)
                ProcessField(component, componentFieldInfos[i]);

            if (component.GetType() != typeof(Transform))
                gameObject.AddComponent(component);
            else
                gameObject.ReplaceTransform((Transform)component);
        }

        private static void ProcessField(Component component, ComponentFieldInfo componentFieldInfo)
        {
            Type typeToDeserializeTo = GetAssemblyByName(componentFieldInfo.AssemblyName).GetType(componentFieldInfo.TypeName);
            object data = JsonConvert.DeserializeObject(componentFieldInfo.DataJson, typeToDeserializeTo, SceneCommons.JsonConverters);

            component.GetType()
                .GetField(componentFieldInfo.Name)
                .SetValue(component, data);
        }

        private static Assembly GetAssemblyByName(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().
                   SingleOrDefault(assembly => assembly.GetName().Name == name);
        }
    }
}
