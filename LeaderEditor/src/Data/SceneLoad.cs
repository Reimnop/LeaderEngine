using LeaderEngine;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

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

            EditorCamera.Main.Transform.LocalPosition = sceneInfo.EditorCamPosition;
            EditorCamera.Main.Transform.RotationEuler = sceneInfo.EditorCamRotation;
            EntityInfo[] entityInfos = sceneInfo.Entities;

            for (int i = 0; i < entityInfos.Length; i++)
                ProcessEntity(entityInfos[i], null);
        }

        private static void ProcessEntity(EntityInfo entityInfo, Entity parent)
        {
            Entity Entity = new Entity(entityInfo.Name, entityInfo.RenderHint);
            Entity.SetActive(entityInfo.Active);

            ComponentInfo[] componentInfos = entityInfo.Components;

            for (int i = 0; i < componentInfos.Length; i++)
                ProcessComponent(Entity, componentInfos[i]);

            if (parent != null)
                Entity.Parent = parent;

            for (int i = 0; i < entityInfo.Children.Length; i++)
                ProcessEntity(entityInfo.Children[i], Entity);
        }

        private static void ProcessComponent(Entity entity, ComponentInfo componentInfo)
        {
            Assembly asm = GetAssemblyByName(componentInfo.AssemblyName);
            Component component = (Component)Activator.CreateInstance(asm.GetType(componentInfo.Name));

            ComponentFieldInfo[] componentFieldInfos = componentInfo.Fields;

            for (int i = 0; i < componentFieldInfos.Length; i++)
                ProcessField(component, componentFieldInfos[i]);

            if (component.GetType() != typeof(Transform))
                entity.AddComponent(component);
            else
                entity.ReplaceTransform((Transform)component);
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
