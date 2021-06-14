using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LeaderEngine
{
    public static class AssetManager
    {
        public static List<GameAsset> Assets { get; } = new List<GameAsset>();

        internal static Dictionary<string, Type> ComponentTypes { get; } = new Dictionary<string, Type>();

        internal static void Init()
        {
            var kvps =
                AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(t => typeof(Component).IsAssignableFrom(t))
                .Select(x => KeyValuePair.Create(x.AssemblyQualifiedName, x));

            foreach (var kvp in kvps)
                ComponentTypes.Add(kvp.Key, kvp.Value);

            new Font("Inconsolata", Path.Combine(AppContext.BaseDirectory, "EngineAssets/Fonts/Inconsolata.fnt"));
            new Font("Impact", Path.Combine(AppContext.BaseDirectory, "EngineAssets/Fonts/Impact.fnt"));
        }
    }
}
