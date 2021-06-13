using System;

namespace LeaderEngine
{
    public abstract class GameAsset : IDisposable
    {
        public readonly string Name;

        public abstract GameAssetType AssetType { get; }

        public GameAsset(string name)
        {
            AssetManager.Assets.Add(this);

            Name = name;
        }

        public virtual void Dispose()
        {
            AssetManager.Assets.Remove(this);
        }
    }
}
