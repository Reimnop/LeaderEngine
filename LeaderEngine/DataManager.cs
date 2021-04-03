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
            SceneEntities.ForEach(x =>
            {
                if (x.Parent == null)
                    x.Update();
            });
        }
    }

    public static class DataManager
    {
        public static Scene CurrentScene { get; private set; } = new Scene("Untitled Scene");
    }
}
