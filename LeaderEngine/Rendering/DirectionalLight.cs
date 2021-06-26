namespace LeaderEngine
{
    public class DirectionalLight : Component
    {
        public static DirectionalLight Main;

        public float Intensity = 1f;

        private void Start()
        {
            if (Main == null)
                Main = this;
        }

        private void OnRemove()
        {
            if (Main == this)
                Main = null;
        }
    }
}
