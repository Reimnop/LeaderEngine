using System.Collections.Generic;

namespace LeaderEngine
{
    public static class DataManager
    {
        public static Scene CurrentScene { get; set; } = new Scene("Untitled Scene");
        internal static List<Entity> UnlistedEntities { get; } = new List<Entity>();
    }
}
