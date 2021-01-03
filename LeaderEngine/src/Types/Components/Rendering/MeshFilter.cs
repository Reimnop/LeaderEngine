namespace LeaderEngine
{
    public class MeshFilter : EditorComponent
    {
        public Mesh Mesh;

        public MeshFilter() { }

        public MeshFilter(Mesh mesh)
        {
            Mesh = mesh;
        }

        public override void EditorStart()
        {
            MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();

            if (mr != null)
                mr.MeshFilter = this;
        }
    }
}
