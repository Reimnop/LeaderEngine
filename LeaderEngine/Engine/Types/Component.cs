namespace LeaderEngine
{
    public class Component
    {
        public virtual void Start() { }
        public virtual void Update() { }
        public virtual void LateUpdate() { }
        public virtual void OnRemove() { }
        public virtual void OnClosing() { }
    }
}
