using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LeaderEngine
{
    public static class AssetManager
    {
        private struct AssetSerializerPair
        {
            public GameAsset Asset;
            public GameAssetSerializer Serializer;

            public AssetSerializerPair(GameAsset asset, GameAssetSerializer serializer)
            {
                Asset = asset;
                Serializer = serializer;
            }
        }

        private static GameAssetSerializer[] gameAssetSerializers = new GameAssetSerializer[]
        {
            new MeshSerializer(),
            new TextureSerializer(),
            new MaterialSerializer(),
            new PrefabSerializer()
        };

        public static Dictionary<string, GameAsset> Assets { get; } = new Dictionary<string, GameAsset>();

        internal static void Init()
        {
            new Font("Inconsolata", "EngineAssets/Fonts/Inconsolata.fnt");
            new Font("Impact", "EngineAssets/Fonts/Impact.fnt");
        }

        public static void SaveAssetsToFile(string path)
        {
            AssetSerializerPair[] serializableAssets = GetSerializableAssets();

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                BinaryWriter writer = new BinaryWriter(stream);

                writer.Write(serializableAssets.Length); //assets count
                for (int i = 0; i < serializableAssets.Length; i++)
                {
                    AssetSerializerPair assetSerializerPair = serializableAssets[i];
                    GameAsset asset = assetSerializerPair.Asset;

                    writer.Write(asset.ID);
                    writer.Write((int)asset.AssetType); //asset type
                    assetSerializerPair.Serializer.WriteToStream(writer, asset); //asset data
                }
            }
        }

        public static void LoadAssetsFromFile(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                BinaryReader reader = new BinaryReader(stream);

                int assetsCount = reader.ReadInt32();
                for (int i = 0; i < assetsCount; i++)
                {
                    string id = reader.ReadString();
                    GameAssetType assetType = (GameAssetType)reader.ReadInt32();

                    GameAssetSerializer serializer = GetSuitableSerializer(assetType);
                    if (serializer == null)
                        throw new Exception("No suitable serializer found!");

                    GameAsset.SetNextID(id);
                    serializer.ReadFromStream(reader);
                }
            }
        }

        private static AssetSerializerPair[] GetSerializableAssets()
        {
            List<AssetSerializerPair> assetSerializerPairs = new List<AssetSerializerPair>();

            GameAsset[] assets = Assets.Values.ToArray();
            foreach (GameAsset asset in assets)
            {
                GameAssetSerializer serializer = GetSuitableSerializer(asset.AssetType);
                if (serializer == null)
                    continue;

                assetSerializerPairs.Add(new AssetSerializerPair(asset, serializer));
            }

            return assetSerializerPairs.ToArray();
        }

        private static GameAssetSerializer GetSuitableSerializer(GameAssetType assetType)
        {
            foreach (GameAssetSerializer serializer in gameAssetSerializers)
            {
                if (serializer.CanSerialize(assetType))
                {
                    return serializer;
                }
            }

            return null;
        }
    }
}
