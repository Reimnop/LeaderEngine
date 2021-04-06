using System.Collections.Generic;

namespace LeaderEngine
{
    public class Scene
    {
        public string Name;

        public List<Entity> SceneRootEntities = new List<Entity>();

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
    }

    public static class DataManager
    {
        public static Scene CurrentScene { get; private set; } = new Scene("Untitled Scene");
    }
}
