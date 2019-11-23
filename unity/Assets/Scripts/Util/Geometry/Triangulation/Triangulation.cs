namespace Util.Geometry.Triangulation
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Math;

    /// <summary>
    /// Class that holds a triangulation, i.e. a collection of triangles.
    /// Triangles are stored as a tuple of three triangle edges in clockwise order.
    /// 
    /// Similar to a DCEL, each triangle edge is a halfedge, 
    /// which will store a pointer to its twin whenever it exist.
    /// Though for simplicity there is no outer face or next/prev pointers.
    /// </summary>
    public class Triangulation
    {

        private readonly List<Triangle> m_Triangles = new List<Triangle>();

        /// <summary>
        /// Collection of triangles in the triangulation.
        /// </summary>
        public ICollection<Triangle> Triangles { get { return m_Triangles; } }

        /// <summary>
        /// All triangle edges inside the triangulation.
        /// Will return two half edges for every edge in triangulation.
        /// </summary>
        public IEnumerable<TriangleEdge> Edges
        {
            get
            {
                return m_Triangles.SelectMany(t => t.Edges);
            }
        }

        /// <summary>
        /// Returns the unique vertices in the triangulation (no duplicates).
        /// </summary>
        public IEnumerable<Vector2> Vertices
        {
            get
            {
                // removes duplicates
                return m_Triangles.SelectMany(t => t.Vertices).Distinct();
            }
        }

        public float Area
        {
            get
            {
                return m_Triangles.Sum(t => t.Area);
            }
        }

        // Store outer triangle potentially used for initialization. 
        private Vector2 V0;
        private Vector2 V1;
        private Vector2 V2;

        public Triangulation()
        { }

        /// <summary>
        /// Creates "fan"-like triangulation of points, where every triangle includes first point in list.
        /// 
        /// Does not check for crossings!
        /// </summary>
        /// <param name="a_Points"></param>
        public Triangulation(IEnumerable<Vector2> a_Points) : this()
        {
            var Points = a_Points.ToList();
            for (var i = 1; i < Points.Count - 1; i++)
            {
                var t = new Triangle(
                    Points[0],
                    Points[i],
                    Points[i + 1]
                );

                // check triangles are clockwise
                if (!t.IsClockwise())
                {
                    t = new Triangle(
                        Points[0],
                        Points[i + 1],
                        Points[i]
                    );
                }

                AddTriangle(t);
            }
        }

        /// <summary>
        /// Initializes triangulation with starting triangle.
        /// Useful whenever adding individual vertices to triangulation, 
        /// otherwise this is not possible.
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public Triangulation(Vector2 p0, Vector2 p1, Vector2 p2) : this()
        {
            V0 = p0;
            V1 = p1;
            V2 = p2;

            var triangle = new Triangle(V0, V1, V2);
            AddTriangle(triangle);
        }

        public Triangulation(IEnumerable<Triangle> a_triangles)
        {
            foreach (var tr in a_triangles)
            {
                AddTriangle(new Triangle(tr.P0, tr.P1, tr.P2));
            }
        }

        /// <summary>
        /// Add a single triangle to the triangulation.
        /// Assumes triangle's edges are already correctly set.
        /// </summary>
        /// <remarks>
        /// Does not update twin pointers of triangle edges!
        /// Call FixEdges() afterwards or set correctly beforehand.
        /// </remarks>
        /// <param name="t"></param>
        public void AddTriangle(Triangle t, bool setTwinPointers = false)
        {
            m_Triangles.Add(t);

            if (setTwinPointers)
            {
                SetTwinPointers();
            }
        }

        /// <summary>
        /// Add all triangles to this triangulation.
        /// </summary>
        /// <param name="triangles"></param>
        public void AddTriangles(IEnumerable<Triangle> triangles, bool setTwinPointers = false)
        {
            foreach (var t in triangles) AddTriangle(t);

            if (setTwinPointers)
            {
                SetTwinPointers();
            }
        }

        /// <summary>
        /// Add all triangles in given triangulation.
        /// </summary>
        /// <param name="T"></param>
        public void AddTriangulation(Triangulation T, bool setTwinPointers = false)
        {
            AddTriangles(T.Triangles, setTwinPointers);
        }

        /// <summary>
        /// Adds a new vertex into the triangulation.
        /// Splits a triangle intro three, conserving the three triangle edges.
        /// </summary>
        /// <remarks>
        /// Vertex should lie within some triangle 
        /// Tip: (initialize triangulation with large bounding triangle)
        /// </remarks>
        /// <param name="m_vertex"></param>
        public void AddVertex(Vector2 m_vertex)
        {
            var t = FindContainingTriangle(m_vertex);

            if (t == null)
            {
                throw new GeomException("Vertex to be added is outside triangulation");
            }

            // remove old triangle
            RemoveTriangle(t);

            // create new edges
            var e0x = new TriangleEdge(t.P0, m_vertex, null, null);
            var ex0 = new TriangleEdge(m_vertex, t.P0, e0x, null);
            e0x.Twin = ex0;
            var e1x = new TriangleEdge(t.P1, m_vertex, null, null);
            var ex1 = new TriangleEdge(m_vertex, t.P1, e1x, null);
            e1x.Twin = ex1;
            var e2x = new TriangleEdge(t.P2, m_vertex, null, null);
            var ex2 = new TriangleEdge(m_vertex, t.P2, e2x, null);
            e2x.Twin = ex2;

            // create three new triangles
            AddTriangle(new Triangle(t.E0, e1x, ex0));
            AddTriangle(new Triangle(t.E1, e2x, ex1));
            AddTriangle(new Triangle(t.E2, e0x, ex2));
        }

        /// <summary>
        /// Remove triangle from triangulation.
        /// </summary>
        /// <remarks>
        /// Does not update twin pointers of neighbouring half edges.
        /// </remarks>
        /// <param name="t"></param>
        public void RemoveTriangle(Triangle t, bool setTwinPointers = false)
        {
            m_Triangles.RemoveAll(tr => t.Equals(tr));

            if (setTwinPointers)
            {
                SetTwinPointers();
            }
        }

        /// <summary>
        /// Clears the triangulation of all triangles.
        /// </summary>
        public void Clear()
        {
            // clear triangle list
            // leaves triangles for garbage collector
            m_Triangles.Clear();
        }

        /// <summary>
        /// Remove all triangles that contain an endpoint of the initial triangle.
        /// </summary>
        public void RemoveInitialTriangle()
        {
            // get all triangles that contain initial endpoints
            var ToRemove = m_Triangles.Where(t => ContainsInitialPoint(t)).ToList();

            // remove such triangles
            foreach (var t in ToRemove)
            {
                foreach (var edge in t.Edges.Where(e => !e.IsOuter))
                {
                    // clear twin edge pointer of its twin edge
                    edge.Twin.Twin = null;
                }

                m_Triangles.Remove(t);
            }
        }

        /// <summary>
        /// Check whether the triangle contains an endpoint of the initial triangle.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool ContainsInitialPoint(Triangle t)
        {
            return t.ContainsEndpoint(V0) || t.ContainsEndpoint(V1) || t.ContainsEndpoint(V2);
        }

        /// <summary>
        /// Find the triangle that contains the given point.
        /// </summary>
        /// <param name="m_vertex"></param>
        /// <returns></returns>
        public Triangle FindContainingTriangle(Vector2 m_vertex)
        {
            return m_Triangles.FirstOrDefault(t => t.Contains(m_vertex));
        }

        /// <summary>
        /// Fixes edge twin pointers whenever edges share endpoints.
        /// Runs in O(n^2), so avoid when possible
        /// </summary>
        public void SetTwinPointers()
        {
            foreach (var e1 in Edges)
            {
                foreach (var e2 in Edges)
                {
                    if (e1 == e2) continue;

                    if (e1.T == null || e2.T == null || e1.Point1 == e2.Point1 && e1.Point2 == e2.Point2)
                    {
                        //throw new GeomException("Triangulation is misformed");
                        Debug.Log("Triangulation is misformed");
                        continue;
                    }
                    if (MathUtil.EqualsEps(e1.Point1, e2.Point2) || MathUtil.EqualsEps(e1.Point2, e2.Point1))
                    {
                        e1.Twin = e2;
                        e2.Twin = e1;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a Unity mesh object from this triangulation
        /// Mapping between Unity mesh triangulation and this custom triangulation class.
        /// </summary>
        /// <returns></returns>
        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();

            // make indexed map of vertices
            var vertices = new Dictionary<Vector2, int>();
            var index = 0;
            foreach (var t in m_Triangles)
                foreach (var v in t.Vertices)
                    if (!vertices.ContainsKey(v))
                        vertices.Add(v, index++);

            var vertexList = vertices.Keys.ToList();

            // Calculate UV's
            var bbox = BoundingBoxComputer.FromPoints(vertexList, 0.1f);
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

            // set mesh variables
            mesh.vertices = vertexList.Select<Vector2, Vector3>(p => p).ToArray();
            mesh.uv = newUV.ToArray();
            mesh.triangles = tri.ToArray();

            return mesh;
        }
    }
}