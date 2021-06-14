using System.Collections.Generic;

namespace LeaderEngine
{
    internal static class SceneGlobals
    {
        internal static BinarySerializer[] Serializers = new BinarySerializer[]
        {
            new IntSerializer(),
            new FloatSerializer(),
            new StringSerializer(),
            //new MeshSerializer(),
            //new MaterialSerializer(),
            //new TextureSerializer()
            //TODO: FIX ME!
        };
    }

    public class Scene
    {
        public string Name;

        public List<Entity> SceneEntities { get; } = new List<Entity>();

        public Scene(string name)
        {
            Name = name;
        }

        internal void UpdateScene()
        {
            foreach (Entity entity in SceneEntities)
                entity.Update();
        }
    }
}
