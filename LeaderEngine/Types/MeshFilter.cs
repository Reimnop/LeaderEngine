namespace LeaderEngine
{
    public class MeshFilter : Component
    {
        public VertexArray VertexArray;

        public MeshFilter() { }

        public MeshFilter(VertexArray vertexArray)
        {
            VertexArray = vertexArray;
        }
    }
}
