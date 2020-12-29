namespace LeaderEngine
{
    public class MeshFilter : Component
    {
        public Mesh Mesh;

        public MeshFilter() { }

        public MeshFilter(Mesh mesh)
        {
            Mesh = mesh;
        }
    }
}
