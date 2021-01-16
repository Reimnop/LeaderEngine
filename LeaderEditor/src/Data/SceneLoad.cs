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

            EditorCamera.Main.BaseTransform.LocalPosition = sceneInfo.EditorCamPosition;
            EditorCamera.Main.BaseTransform.RotationEuler = sceneInfo.EditorCamRotation;
            EntityInfo[] entityInfos = sceneInfo.Entities;

            for (int i = 0; i < entityInfos.Length; i++)
                ProcessEntity(entityInfos[i], null);
        }

        private static void ProcessEntity(EntityInfo entityInfo, Entity parent)
        {
            Entity entity = new Entity(entityInfo.Name, entityInfo.RenderHint);
            entity.SetActive(entityInfo.Active);

            ComponentInfo[] componentInfos = entityInfo.Components;

            for (int i = 0; i < componentInfos.Length; i++)
                ProcessComponent(entity, componentInfos[i]);

            if (parent != null)
                entity.Parent = parent;

            for (int i = 0; i < entityInfo.Children.Length; i++)
                ProcessEntity(entityInfo.Children[i], entity);
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
