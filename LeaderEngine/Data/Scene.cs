using System.Collections.Generic;

namespace LeaderEngine
{
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
