namespace Util.Geometry.Polygon
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Algorithms.Triangulation;
    using Util.Geometry.Triangulation;
    using Util.Math;

    /// <summary>
    /// Simple 2D polygon class without holes.
    /// We represent the polygon internally as a linked list of vertices.
    /// </summary>
    public class Polygon2D : IPolygon2D
    {
        private LinkedList<Vector2> m_vertices;

        // some cache variables for speedup
        private List<LineSegment> m_segments;
        private float m_area = -1f;
        private Polygon2D m_removedDanglingEdgesPoly;
        private Triangulation m_triangulation;
        private bool? m_simple;
        private bool? m_convex;
        private bool? m_clockwise;

        public ICollection<Vector2> Vertices { get { return m_vertices; } }

        public int VertexCount { get { return m_vertices.Count; } }

        /// <summary>
        /// Computes the area spanned by this polygon
        /// </summary>
        /// The theory behind this method is documented in the docs folder.
        /// <returns></returns>
        public float Area
        {
            get
            {
                if (m_area != -1f) return m_area;

                // no area polygon
                if (VertexCount <= 2) return 0f;

                //Take the origin as arbitrary point P
                //add up signed areas along the edges of the polygon
                double areasum = 0.0;
                foreach (LineSegment seg in Segments)
                {
                    var v1 = seg.Point1;
                    var v2 = seg.Point2;
                    areasum += v1.x * v2.y - v2.x * v1.y;
                }

                m_area = (float)Math.Abs(areasum) / 2f;
                return m_area;
            }
        }

        public ICollection<LineSegment> Segments
        {
            get
            {
                if (m_segments != null) return m_segments;

                if (VertexCount <= 1) return new List<LineSegment>();

                m_segments = new List<LineSegment>();
                for (var node = m_vertices.First; node != null; node = node.Next)
                {
                    var nextNode = node.Next ?? m_vertices.First;
                    m_segments.Add(new LineSegment(node.Value, nextNode.Value));
                }
                return m_segments;
            }
        }

        public Vector2? Next(Vector2 pos)
        {
            var node = m_vertices.Find(pos);
            if (node == null) return null;
            return node.Next != null ? node.Next.Value : m_vertices.First.Value;
        }

        public Vector2? Prev(Vector2 pos)
        {
            var node = m_vertices.Find(pos);
            if (node == null) return null;
            return node.Previous != null ? node.Previous.Value : m_vertices.Last.Value;
        }

        public void AddVertex(Vector2 pos)
        {
            m_vertices.AddLast(pos);
            ClearCache();
        }

        public void AddVertexFirst(Vector2 pos)
        {
            m_vertices.AddFirst(pos);
            ClearCache();
        }

        public void AddVertexAfter(Vector2 pos, Vector2 after)
        {
            if (!ContainsVertex(after))
            {
                throw new ArgumentException("Polygon does not contain vertex after which to add");
            }
            AddVertexAfter(pos, m_vertices.Find(after));
            ClearCache();
        }

        private void AddVertexAfter(Vector2 pos, LinkedListNode<Vector2> node)
        {
            if (node == null) throw new GeomException("Adding vertex after null node");

            m_vertices.AddAfter(node, pos);
            ClearCache();
        }

        public void RemoveVertex(Vector2 pos)
        {
            m_vertices.Remove(pos);
            ClearCache();
        }

        public void RemoveFirst()
        {
            m_vertices.RemoveFirst();
            ClearCache();
        }

        public void RemoveLast()
        {
            m_vertices.RemoveLast();
            ClearCache();
        }

        public void Clear()
        {
            m_vertices.Clear();
            ClearCache();
        }

        /// <summary>
        /// Clears some cache variable that are stored for performance.
        /// </summary>
        private void ClearCache()
        {
            m_segments = null;
            m_area = -1f;
            m_removedDanglingEdgesPoly = null;
            m_triangulation = null;
            m_convex = null;
            m_clockwise = null;
            m_simple = null;
        }

        /// <summary>
        /// Simple Constructor 
        /// </summary>
        public Polygon2D()
        {
            m_vertices = new LinkedList<Vector2>();
            ClearCache();
        }

        /// <summary>
        /// Constructs a clockwise polygon with the given vertices.
        /// </summary>
        /// <param name="a_vertices"></param>
        public Polygon2D(IEnumerable<Vector2> a_vertices) : this()
        {
            foreach (var v in a_vertices)
                AddVertex(new Vector2(v.x, v.y));
        }

        /// <summary>
        /// Tests wheter this polygon is convex by verifying that each triplet of points constitutes a right turn
        /// </summary>
        public bool IsConvex()
        {
            if (m_convex.HasValue) return m_convex.Value;

            if (VertexCount < 3)
            {
                throw new GeomException("Being convex is illdefined for polygons of 2 or less vertices");
            }

            // flip orientation if polygon counter clockwise 
            var dir = (IsClockwise() ? 1 : -1);

            for (var node = m_vertices.First; node != null; node = node.Next)
            {
                var prevNode = node.Previous ?? m_vertices.Last;
                var nextNode = node.Next ?? m_vertices.First;

                // do not consider degenerate case with two equal nodes
                if (MathUtil.EqualsEps(node.Value, nextNode.Value)) continue;

                // check for dangling edges or illegal turn
                if (MathUtil.EqualsEps(prevNode.Value, nextNode.Value) ||
                    dir * MathUtil.Orient2D(prevNode.Value, node.Value, nextNode.Value) > 0)
                {
                    m_convex = false;
                    return false;
                }
            }

            m_convex = true;
            return true;
        }

        public bool ContainsInside(Vector2 a_pos)
        {
            // cannot contain without area
            if (Area == 0 || OnBoundary(a_pos)) return false;

            if (IsConvex())
            {
                bool inv = IsClockwise();

                for (var node = m_vertices.First; node != null; node = node.Next)
                {
                    var nextNode = node.Next ?? m_vertices.First;
                    var segment = new LineSegment(node.Value, nextNode.Value);
                    if (inv != segment.IsRightOf(a_pos))
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                Debug.Assert(VertexCount > 3);

                // calculate triangulation
                if (m_triangulation == null)
                {
                    var poly = RemoveDanglingEdges();
                    m_triangulation = Triangulator.Triangulate(poly, false);
                }

                // check for triangle that contains point
                return m_triangulation.Triangles.Any(t => t.Contains(a_pos));
            }
        }

        public bool OnBoundary(Vector2 pos)
        {
            return Segments.ToList().Exists(seg => seg.IsOnSegment(pos));
        }

        public bool ContainsVertex(Vector2 pos)
        {
            return m_vertices.Find(pos) != null;
        }

        public bool Contains(Vector2 pos)
        {
            // ContainsInside(pos) || 
            return ContainsInside(pos) || ContainsVertex(pos) || OnBoundary(pos);
        }

        /// <summary>
        /// Shifts all polygon points such that the given point lies at origin.
        /// </summary>
        /// <param name="a_point"></param>
        public void ShiftToOrigin(Vector2 a_point)
        {
            m_vertices = new LinkedList<Vector2>(m_vertices.Select(v => v - a_point));
            ClearCache();
        }

        /// <summary>
        /// Determines wheter or not this polygon is clockwise using the Shoelace formula (in O(n))
        /// </summary>
        /// <returns></returns>
        public bool IsClockwise()
        {
            if (m_clockwise.HasValue) return m_clockwise.Value;

            var sum = 0f;
            foreach (var seg in Segments)
            {
                sum += (seg.Point2.x - seg.Point1.x) * (seg.Point2.y + seg.Point1.y);
            }

            if (MathUtil.GreaterEps(sum, 0f))
            {
                m_clockwise = true;
                return true;
            }

            m_clockwise = false;
            return false;
        }

        public void Reverse()
        {
            m_vertices = new LinkedList<Vector2>(m_vertices.Reverse());
            ClearCache();
        }

        public override string ToString()
        {
            var str = "Face: {";
            foreach (var vertex in Vertices)
            {
                str += vertex + ", ";
            }
            return str + "}";
        }

        public bool IsSimple()
        {
            if (m_simple.HasValue) return m_simple.Value;

            foreach (var seg1 in Segments)
            {
                foreach (var seg2 in Segments)
                {
                    if (seg1 != seg2 && seg1.IntersectProper(seg2) != null)
                    {
                        Debug.Log(seg1 + " " + seg2 + " " + seg1.IntersectProper(seg2));
                        m_simple = false;
                        return false;
                    }
                }
            }
            m_simple = true;
            return true;
        }

        public Rect BoundingBox(float margin = 0f)
        {
            return BoundingBoxComputer.FromPoints(Vertices, margin);
        }

        /// <summary>
        /// Removes any parts of the polygon where the boundary reverses (a dangling edge).
        /// </summary>
        /// <remarks>
        /// Useful for easier triangulation, whenever only area or containment matters.
        /// </remarks>
        /// <param name="poly"></param>
        /// <returns></returns>
        public Polygon2D RemoveDanglingEdges()
        {
            if (m_removedDanglingEdgesPoly != null) return m_removedDanglingEdgesPoly;

            var result = new List<Vector2>(Vertices);

            // iterate until no more dangling edges
            bool containsDanglingEdge = true;
            while (containsDanglingEdge)
            {
                containsDanglingEdge = false;

                // find dangling edge
                Vector2? toRemove = null;
                for (var i = 0; i < result.Count; i++)
                {
                    var prev = MathUtil.PositiveMod(i - 1, result.Count);
                    var next = MathUtil.PositiveMod(i + 1, result.Count);
                    if (MathUtil.EqualsEps(result[i], result[next]) || MathUtil.EqualsEps(result[i], result[prev])
                        || MathUtil.EqualsEps(result[prev], result[next]))
                    {
                        containsDanglingEdge = true;
                        toRemove = result[i];
                        break;
                    }
                }

                // remove dangling edge
                if (containsDanglingEdge)
                {
                    result.Remove(toRemove.Value);
                }
            }

            m_removedDanglingEdgesPoly = new Polygon2D(result);
            return m_removedDanglingEdgesPoly;
        }

        public bool Equals(IPolygon2D other)
        {
            var poly = other as Polygon2D;
            if (poly == null) return false;

            if (VertexCount != poly.VertexCount) return false;

            var vertices = Vertices.ToList();
            var otherVertices = poly.Vertices.ToList();

            for (var i = 0; i < VertexCount; i++)
            {
                if (!vertices[i].Equals(otherVertices[i])) return false;
            }

            return true;
        }
    }
}