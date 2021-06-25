using System;

namespace LeaderEngine
{
    public abstract class GameAsset : IDisposable
    {
        private static string nextId;

        public readonly string Name;
        public readonly string ID;

        public abstract GameAssetType AssetType { get; }

        public GameAsset(string name)
        {
            ID = nextId ?? RNG.GetRandomID();
            Name = name;            

            nextId = null;

            AssetManager.Assets.Add(ID, this);
        }

        public virtual void Dispose()
        {
            AssetManager.Assets.Remove(ID);
        }

        internal static void SetNextID(string id)
        {
            nextId = id;
        }
    }
}
