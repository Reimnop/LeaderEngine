using System.Collections.Generic;

namespace LeaderEngine
{
    public class Scene
    {
        public string Name;

        public List<Entity> SceneEntities = new List<Entity>();

        public Scene(string name)
        {
            Name = name;
        }

        internal void UpdateSceneHierachy()
        {
            for (int i = 0; i < SceneEntities.Count; i++)
                if (SceneEntities[i].Parent == null)
                    SceneEntities[i].Update();
        }
    }

    public static class DataManager
    {
        public static Scene CurrentScene { get; private set; } = new Scene("Untitled Scene");
    }
}
