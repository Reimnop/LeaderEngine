namespace LeaderEngine
{
    public class MeshRenderer : Renderer
    {
        public Mesh Mesh;
        public Shader Shader;

        private void Start()
        {
            BaseEntity.Renderers.Add(this);
        }

        private void OnRemove()
        {
            BaseEntity.Renderers.Remove(this);
        }

        public override void Render()
        {
            Engine.Renderer.PushDrawData(DrawType.Opaque, new GLDrawData
            {
                Mesh = Mesh,
                Shader = Shader,
                Texture = null
            });
        }
    }
}
