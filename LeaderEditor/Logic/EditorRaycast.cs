using System;
using LeaderEngine;
using OpenTK.Mathematics;

namespace LeaderEditor
{
    internal struct Ray
    {
        public Vector3 Origin;
        public Vector3 Direction;
    }

    internal static class EditorRaycast
    {
        public static bool MeshRaycast(Mesh mesh, Ray ray, Matrix4 model, out float t)
        {
            t = float.PositiveInfinity;

            Span<Vector3> vertices = mesh.Vertices;
            Span<uint> indices = mesh.Indices;

            bool intersect = false;
            for (int i = 0; i < mesh.IndicesCount / 3; i++)
            {
                int index = i * 3;
                Vector3 v0 = (new Vector4(vertices[(int)indices[index + 0]], 1f) * model).Xyz;
                Vector3 v1 = (new Vector4(vertices[(int)indices[index + 1]], 1f) * model).Xyz;
                Vector3 v2 = (new Vector4(vertices[(int)indices[index + 2]], 1f) * model).Xyz;

                if (IntersectTriangle(ray, v0, v1, v2, out float dist, out _, out _))
                {
                    intersect = true;
                    if (t > dist)
                        t = dist;
                }
            }

            return intersect;
        }

        private static bool IntersectTriangle(
            Ray ray, Vector3 vert0, Vector3 vert1, Vector3 vert2, 
            out float t, out float u, out float v)
        {
            const float EPSILON = 1e-8f;

            t = float.PositiveInfinity;
            u = 0;
            v = 0;

            // find vectors for two edges sharing vert0
            Vector3 edge1 = vert1 - vert0;
            Vector3 edge2 = vert2 - vert0;
            // begin calculating determinant - also used to calculate U parameter
            Vector3 pvec = Vector3.Cross(ray.Direction, edge2);
            // if determinant is near zero, ray lies in plane of triangle
            float det = Vector3.Dot(edge1, pvec);
            // use backface culling
            if (det < EPSILON)
                return false;

            float inv_det = 1f / det;
            // calculate distance from vert0 to ray origin
            Vector3 tvec = ray.Origin - vert0;
            // calculate U parameter and test bounds
            u = Vector3.Dot(tvec, pvec) * inv_det;
            if (u < 0.0 || u > 1f)
                return false;
            // prepare to test V parameter
            Vector3 qvec = Vector3.Cross(tvec, edge1);
            // calculate V parameter and test bounds
            v = Vector3.Dot(ray.Direction, qvec) * inv_det;
            if (v < 0.0 || u + v > 1f)
                return false;
            // calculate t, ray intersects triangle
            t = Vector3.Dot(edge2, qvec) * inv_det;
            return true;
        }

    }
}
