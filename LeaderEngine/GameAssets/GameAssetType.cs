namespace LeaderEngine
{
    public enum GameAssetType
    {
        AudioClip = (1 << 0),
        Material  = (1 << 1),
        Mesh      = (1 << 2),
        Shader    = (1 << 3),
        Sprite    = (1 << 4),
        Texture   = (1 << 5),
        Cubemap   = (1 << 6),
        Font      = (1 << 7),
        Prefab    = (1 << 8),

        All = AudioClip | Material | Mesh | Shader | Sprite | Texture | Cubemap | Font | Prefab
    }
}
