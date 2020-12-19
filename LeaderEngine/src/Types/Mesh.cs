using System;
using System.Collections.Generic;
using System.Text;

namespace LeaderEngine
{
    public class Mesh
    {
        private List<VertexArray> vertexArrays = new List<VertexArray>();

        public Mesh() { }

        public Mesh(VertexArray[] vertexArrays)
        {
            this.vertexArrays.AddRange(vertexArrays);
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
