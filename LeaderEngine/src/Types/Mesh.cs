using System.Collections.Generic;

namespace LeaderEngine
{
    public class Mesh
    {
        private List<VertexArray> vertexArrays = new List<VertexArray>();

        public string Name;

        public Mesh(string name)
            => Name = name;

        public Mesh(string name, VertexArray[] vertexArrays)
        {
            this.vertexArrays.AddRange(vertexArrays);
            Name = name;
        }

        public void AddVertexArray(VertexArray vertexArray)
        {
            vertexArrays.Add(vertexArray);
        }

        public List<VertexArray> GetAllVertexArrays()
        {
            return vertexArrays;
        }
    }
}
