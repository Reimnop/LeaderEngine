namespace LeaderEngine
{
    public class Component
    {
        public Entity BaseEntity = null;
        public Transform Transform => BaseEntity.Transform;

        public bool Enabled = true;

        public virtual void Start() { return; }
        public virtual void Update() { return; }
        public virtual void LateUpdate() { return; }
        public virtual void OnRender() { return; }
        public virtual void OnRenderGui() { return; }
        public virtual void OnRemove() { return; }
    }
}
