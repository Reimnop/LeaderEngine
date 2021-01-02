using LeaderEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace LeaderEditor.Data
{
    public static class SceneLoad
    {
        public static void LoadScene(string path)
        {
            SceneInfo sceneInfo = JsonSerializer.Deserialize<SceneInfo>(File.ReadAllText(path));

            ProcessScene(sceneInfo);
        }

        private static void ProcessScene(SceneInfo sceneInfo)
        {
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

            gameObject.AddComponent(component);
        }

        private static void ProcessField(Component component, ComponentFieldInfo componentFieldInfo)
        {
            component.GetType()
                .GetField(componentFieldInfo.Name)
                .SetValue(component, componentFieldInfo.Data);
        }

        private static Assembly GetAssemblyByName(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().
                   SingleOrDefault(assembly => assembly.GetName().Name == name);
        }
    }
}
