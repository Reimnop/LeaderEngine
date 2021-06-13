namespace LeaderEngine
{
    public abstract class Material : GameAsset
    {
        public override GameAssetType AssetType => GameAssetType.Material;

        public Shader Shader => _shader;
        public int UniformBuffer => _uniformBuffer;

        protected Shader _shader;
        protected int _uniformBuffer;

        protected Material(string name, Shader shader) : base(name)
        {
            _shader = shader;
        }
    }
}
