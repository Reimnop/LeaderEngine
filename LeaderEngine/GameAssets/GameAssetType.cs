namespace LeaderEngine
{
    public enum GameAssetType
    {
        AudioClip = 1,
        Material  = 2,
        Mesh      = 4,
        Shader    = 8,
        Sprite    = 16,
        Texture   = 32,
        Cubemap   = 64,
        Font      = 128,
        Prefab    = 256,

        All = AudioClip | Material | Mesh | Shader | Sprite | Texture | Cubemap | Font | Prefab
    }
}
