using System.IO;

namespace LeaderEngine
{
    public abstract class GameAssetSerializer
    {
        public abstract bool CanSerialize(GameAssetType assetType);
        public abstract void WriteToStream(BinaryWriter stream, GameAsset asset);
        public abstract GameAsset ReadFromStream(BinaryReader reader);
    }
}
