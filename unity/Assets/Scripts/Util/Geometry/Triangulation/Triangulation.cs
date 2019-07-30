namespace Util.Geometry.Triangulation
{
    using System.Linq;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Algorithms;

    public class Triangulation
    {
        private readonly LinkedList<Triangle> m_Triangles;
        private readonly LinkedList<TriangleEdge> m_Edges;

        public ICollection<Triangle> Triangles { get { return m_Triangles; } }

        public ICollection<TriangleEdge> Edges { get { return m_Edges; } }

        private readonly Vector2 V0, V1, V2;

        public Triangulation()
        {
            m_Triangles = new LinkedList<Triangle>();
        }

        public Triangulation(IEnumerable<Vector2> a_Points)
        { 
            var Points = a_Points.ToList();
            for (var i = 0; i < Points.Count - 2; i++)
            {
                Add(new Triangle(
                    Points[i],
                    Points[i + 1],
                    Points[i + 2]
                ));
            }
        }

        public Triangulation(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            V0 = p0;
            V1 = p1;
            V2 = p2;
            m_Triangles = new LinkedList<Triangle>(new Triangle[]
            {
                new Triangle(V0, V1, V2)
            });
        }

        public void Add(Triangle t)
        {
            Triangles.Add(t);
        }

        public void Add(IEnumerable<Triangle> triangles)
        {
            foreach (var t in triangles) Add(t);

            FixEdges();
        }

        public void Add(Triangulation T)
        {
            Add(T.Triangles);
        }

        public void Remove(Triangle t)
        {
            Triangles.Remove(t);
        }

        public void Clear()
        {
            Triangles.Clear();
        }

        public void RemoveInitialTriangle()
        {
            var ToRemove = Triangles
                .Where(t => t.ContainsEndpoint(V0) || t.ContainsEndpoint(V1) || t.ContainsEndpoint(V2));

            foreach (var t in ToRemove)
                Triangles.Remove(t);
        }

        private void FixEdges()
        {
            foreach (var e1 in m_Edges)
            {
                foreach (var e2 in m_Edges)
                {
                    if (e1 == e2) continue;

                    if(e1.T == null || e2.T == null || e1.Start == e2.Start || e1.End == e2.End)
                    {
                        throw new GeomException("Triangulation is misformed");
                    }

                    if(e1.Start == e2.End || e1.End == e2.Start)
                    {
                        e1.Twin = e2;
                        e2.Twin = e1;
                    }
                }
            }
        }

        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();

            // make indexed map of vertices
            var vertices = new Dictionary<Vector2, int>();
            var index = 0;
            foreach (var t in m_Triangles)
                foreach(var v in t.Vertices)
                    if (!vertices.ContainsKey(v))
                        vertices.Add(v, index++);

            var vertexList = vertices.Keys.ToList();
            
            // Calculate UV's
            var bbox = BoundingBox.FromVector2(vertexList);
            var newUV = vertexList.Select<Vector2, Vector2>(p => Rect.PointToNormalized(bbox, p));

            // Calculate mesh triangles
            var tri = new List<int>();
            foreach (var t in Triangles)
            {
                tri.AddRange(new int[3] {
                    vertices[t.P0],
                    vertices[t.P1],
                    vertices[t.P2]
                });
            }

            mesh.vertices = vertexList.Select<Vector2, Vector3>(p => p).ToArray();
            mesh.uv = newUV.ToArray();
            mesh.triangles = tri.ToArray();

            return mesh;
        }
    }
}