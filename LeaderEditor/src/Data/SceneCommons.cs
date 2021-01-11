using LeaderEngine;
using Newtonsoft.Json;
using OpenTK.Mathematics;

namespace LeaderEditor.Data
{
    public static class SceneCommons
    {
        public static JsonConverter[] JsonConverters = new JsonConverter[]
        {
            new Vector4Converter(),
            new Vector3Converter(),
            new MeshConverter()
        };
    }

    public struct ComponentFieldInfo
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string AssemblyName { get; set; }
        public string DataJson { get; set; }
    }

    public struct ComponentInfo
    {
        public string Name { get; set; }
        public string AssemblyName { get; set; }
        public ComponentFieldInfo[] Fields { get; set; }
    }

    public struct GameObjectInfo
    {
        public string Name { get; set; }
        public bool Active { get; set; }
        public RenderHint RenderHint { get; set; }
        public GameObjectInfo[] Children;
        public ComponentInfo[] Components { get; set; }
    }

    public struct SceneInfo
    {
        public string[] Models { get; set; }
        public Vector3 EditorCamPosition { get; set; }
        public Vector3 EditorCamRotation { get; set; }
        public GameObjectInfo[] GameObjects { get; set; }
    }
}