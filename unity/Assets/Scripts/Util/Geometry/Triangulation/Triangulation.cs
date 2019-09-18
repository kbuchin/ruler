namespace Util.Geometry.Triangulation
{
    using System.Linq;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Algorithms;

    public class Triangulation
    {

        private readonly LinkedList<Triangle> m_Triangles;

        public IEnumerable<Triangle> Triangles { get { return m_Triangles; } }

        public IEnumerable<TriangleEdge> Edges {
            get
            {
                return m_Triangles
                    .Select(t => t.E0)
                    .Union(m_Triangles.Select(t => t.E1))
                    .Union(m_Triangles.Select(t => t.E2));
            }
        }

        public IEnumerable<Vector2> Vertices {
            get
            {
                var vertices = m_Triangles
                    .Select(t => t.P0)
                    .Union(m_Triangles.Select(t => t.P1))
                    .Union(m_Triangles.Select(t => t.P2));

                // remove duplicates
                return vertices.GroupBy(v => v.GetHashCode()).Select(x => x.First());
            }
        }

        public Vector2 V0 { get; private set; }
        public Vector2 V1 { get; private set; }
        public Vector2 V2 { get; private set; }

        public Triangulation()
        {
            m_Triangles = new LinkedList<Triangle>();
        }

        public Triangulation(IEnumerable<Vector2> a_Points) : this()
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

        public Triangulation(Vector2 p0, Vector2 p1, Vector2 p2) : this()
        {
            V0 = p0;
            V1 = p1;
            V2 = p2;

            var triangle = new Triangle(V0, V1, V2);
            Add(triangle);

            triangle.E0.IsOuter = true;
            triangle.E1.IsOuter = true;
            triangle.E2.IsOuter = true;
        }

        public void Add(Triangle t)
        {
            m_Triangles.AddLast(t);
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

        public void Add(Vector2 m_vertex)
        {
            Triangle t = FindContainingTriangle(m_vertex);

            if(t != null)
            {
                Remove(t);

                // create three clockwise triangles
                if(t.IsClockwise())
                {
                    Add(new Triangle(t.P0, m_vertex, t.P2));
                    Add(new Triangle(t.P0, t.P1, m_vertex));
                    Add(new Triangle(t.P1, t.P2, m_vertex));
                }
                else
                {
                    Add(new Triangle(t.P2, m_vertex, t.P1));
                    Add(new Triangle(t.P1, t.P0, m_vertex));
                    Add(new Triangle(t.P2, t.P1, m_vertex));
                }
            }
            else
            {
                throw new GeomException("Vertex to be added is outside triangulation");
            }
        }

        public void Remove(Triangle t)
        {
            m_Triangles.Remove(t);
        }

        public void Clear()
        {
            m_Triangles.Clear();
        }

        public void RemoveInitialTriangle()
        {
            var ToRemove = m_Triangles
                .Where(t => t.ContainsEndpoint(V0) || t.ContainsEndpoint(V1) || t.ContainsEndpoint(V2));

            foreach (var t in ToRemove)
                m_Triangles.Remove(t);
        }

        public bool ContainsInitialPoint(Triangle t)
        {
            return t.ContainsEndpoint(V0) || t.ContainsEndpoint(V1) || t.ContainsEndpoint(V2);
        }

        public Triangle FindContainingTriangle(Vector2 m_vertex)
        {
            return m_Triangles.First(t => t.Contains(m_vertex));
        }

        private void FixEdges()
        {
            foreach (var e1 in Edges)
            {
                foreach (var e2 in Edges)
                {
                    if (e1 == e2) continue;

                    if(e1.T == null || e2.T == null || (e1.Point1 == e2.Point1 && e1.Point2 == e2.Point2))
                    {
                        throw new GeomException("Triangulation is misformed");
                    }

                    if(e1.Point1 == e2.Point2 || e1.Point2 == e2.Point1)
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
            foreach (var t in m_Triangles)
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