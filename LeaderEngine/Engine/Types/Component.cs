namespace LeaderEngine
{
    public abstract class Component
    {
        public abstract void Start();
        public abstract void Update();
        public abstract void LateUpdate();
        public abstract void OnRemove();
        public abstract void OnClosing();
    }
}
