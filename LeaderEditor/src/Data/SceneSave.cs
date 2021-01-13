using LeaderEngine;
using Newtonsoft.Json;
using OpenTK.Mathematics;
using System;
using System.IO;
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
            sceneInfo.EditorCamPosition = EditorCamera.Main.Transform.LocalPosition;
            sceneInfo.EditorCamRotation = EditorCamera.Main.Transform.RotationEuler;

            var sceneEntitiesSurf = SceneHierachy.SceneEntities.Where(x => x.Parent == null).ToArray();

            sceneInfo.Entities = new EntityInfo[sceneEntitiesSurf.Length];

            for (int i = 0; i < sceneInfo.Entities.Length; i++)
                sceneInfo.Entities[i] = ProcessEntity(SceneHierachy.SceneEntities[i]);

            return sceneInfo;
        }

        private static EntityInfo ProcessEntity(Entity Entity)
        {
            EntityInfo EntityInfo = new EntityInfo();
            EntityInfo.Name = Entity.Name;
            EntityInfo.Active = Entity.ActiveSelf;
            EntityInfo.RenderHint = Entity.RenderHint;

            var components = Entity.GetAllComponents();

            EntityInfo.Components = new ComponentInfo[components.Count];

            for (int i = 0; i < components.Count; i++)
                EntityInfo.Components[i] = ProcessComponent(components[i]);

            EntityInfo.Children = new EntityInfo[Entity.Children.Count];

            for (int i = 0; i < Entity.Children.Count; i++)
                EntityInfo.Children[i] = ProcessEntity(Entity.Children[i]);

            return EntityInfo;
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
