using System.Collections.Generic;
using System.IO;

namespace LeaderEngine
{
    public class Scene
    {
        public string Name;

        public List<Entity> SceneRootEntities { get; } = new List<Entity>();

        public Scene(string name)
        {
            Name = name;
        }

        internal void UpdateSceneHierachy()
        {
            for (int i = 0; i < SceneRootEntities.Count; i++)
                if (SceneRootEntities[i].Parent == null)
                    SceneRootEntities[i].RecursivelyUpdate();
        }

        internal void Serialize(BinaryWriter writer)
        {
            //write name
            writer.Write(Name);
        }

        private void RecursivelySerializeEntity(BinaryWriter writer, Entity entity)
        {
            var components = entity.GetComponents<Component>();
        }
    }
}
