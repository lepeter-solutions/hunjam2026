using System.Collections.Generic;
using UnityEngine;

namespace HunJam
{
    // Planar XY paper. Triangle clipping interpolates the original UVs at every new vertex.
    public static class PaperMesh
    {
        private struct Vertex
        {
            public Vector3 p; public Vector2 uv;
            public Vertex(Vector3 point, Vector2 tex) { p = point; uv = tex; }
        }
        public static Mesh Create(float width = 0.42f, float height = 0.3f, int divisions = 12)
        {
            var vertices = new List<Vector3>(); var uv = new List<Vector2>(); var triangles = new List<int>();
            for (int y = 0; y <= divisions; y++) for (int x = 0; x <= divisions; x++)
            {
                var t = new Vector2((float)x / divisions, (float)y / divisions);
                vertices.Add(new Vector3((t.x - 0.5f) * width, (t.y - 0.5f) * height, 0)); uv.Add(t);
            }
            for (int y = 0; y < divisions; y++) for (int x = 0; x < divisions; x++)
            {
                int a = y * (divisions + 1) + x, b = a + divisions + 1;
                triangles.AddRange(new[] { a, a + 1, b + 1, a, b + 1, b });
            }
            return Build(vertices, uv, triangles, "Subdivided contract");
        }
        public static Mesh Clip(Mesh source, Vector2 point, Vector2 normal, bool positive)
        {
            var vertices = new List<Vector3>(); var uv = new List<Vector2>(); var triangles = new List<int>();
            var input = source.vertices; var tex = source.uv; var indices = source.triangles;
            float side = positive ? 1 : -1;
            for (int i = 0; i < indices.Length; i += 3)
            {
                var polygon = new List<Vertex>(4);
                for (int k = 0; k < 3; k++)
                {
                    int a = indices[i + k], b = indices[i + (k + 1) % 3];
                    float da = Vector2.Dot((Vector2)input[a] - point, normal) * side;
                    float db = Vector2.Dot((Vector2)input[b] - point, normal) * side;
                    if (da >= 0) polygon.Add(new Vertex(input[a], tex[a]));
                    if ((da >= 0) != (db >= 0))
                    {
                        float t = da / (da - db);
                        polygon.Add(new Vertex(Vector3.Lerp(input[a], input[b], t), Vector2.Lerp(tex[a], tex[b], t)));
                    }
                }
                for (int k = 1; k + 1 < polygon.Count; k++)
                {
                    if (Vector3.Cross(polygon[k].p - polygon[0].p, polygon[k + 1].p - polygon[0].p).sqrMagnitude < 1e-14f) continue;
                    foreach (int index in new[] { 0, k, k + 1 })
                    { triangles.Add(vertices.Count); vertices.Add(polygon[index].p); uv.Add(polygon[index].uv); }
                }
            }
            return Build(vertices, uv, triangles, positive ? "Paper positive fragment" : "Paper negative fragment");
        }
        public static Mesh ColliderHull(Mesh mesh, float thickness = 0.004f)
        {
            // PhysX builds a convex hull from the extruded planar mesh; the cut boundary stays exact.
            var points = new List<Vector3>(); var triangles = new List<int>();
            var source = mesh.vertices; var indices = mesh.triangles;
            foreach (var p in source) points.Add(p + Vector3.forward * thickness * 0.5f);
            foreach (var p in source) points.Add(p - Vector3.forward * thickness * 0.5f);
            triangles.AddRange(indices);
            for (int i = indices.Length - 1; i >= 0; i--) triangles.Add(indices[i] + source.Length);
            return Build(points, new List<Vector2>(new Vector2[points.Count]), triangles, "Paper collision hull");
        }
        private static Mesh Build(List<Vector3> p, List<Vector2> uv, List<int> triangles, string name)
        {
            var mesh = new Mesh { name = name };
            mesh.SetVertices(p); mesh.SetUVs(0, uv); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }
    }
}
