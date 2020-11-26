using System;
using System.Collections.Generic;
using System.Text;

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
