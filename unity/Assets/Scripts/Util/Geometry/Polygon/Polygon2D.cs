namespace Util.Geometry.Polygon
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Math;
    using Util.Algorithms.Triangulation;
    using Util.Algorithms.Polygon;

    /// <summary>
    /// Simple 2D polygon class without holes.
    /// We represent the polygon internally as a linked list of vertices.
    /// </summary>
    public class Polygon2D : IPolygon2D
    {
        private LinkedList<Vector2> m_vertices;

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
                // no area polygon
                if (VertexCount <= 2) return 0f;

                //Take the origin as arbitrary point P
                //add up signed areas along the edges of the polygon
                var areasum = 0f;
                foreach (LineSegment seg in Segments)
                {
                    var v1 = seg.Point1;
                    var v2 = seg.Point2;
                    areasum += v1.x * v2.y - v2.x * v1.y;
                }

                return Math.Abs(areasum) / 2;
            }
        }


        public ICollection<LineSegment> Segments
        {
            get
            {
                if (VertexCount <= 1) return new List<LineSegment>();

                var result = new List<LineSegment>(VertexCount);
                var node = m_vertices.First;
                while (node.Next != null)
                {
                    result.Add(new LineSegment(node.Value, node.Next.Value));
                    node = node.Next;
                }
                result.Add(new LineSegment(m_vertices.Last.Value, m_vertices.First.Value));
                return result;
            }
        }

        public Vector2? Next(Vector2 pos)
        {
            var node = m_vertices.Find(pos);
            if (node == null) return null;
            node = node.Next;
            if (node == null) node = m_vertices.First;
            return node.Value;
        }

        public Vector2? Prev(Vector2 pos)
        {
            var node = m_vertices.Find(pos);
            if (node == null) return null;
            node = node.Previous;
            if (node == null) node = m_vertices.Last;
            return node.Value;
        }

        public void AddVertex(Vector2 pos)
        {
            m_vertices.AddLast(pos);
        }

        public void AddVertexFirst(Vector2 pos)
        {
            m_vertices.AddFirst(pos);
        }

        public void AddVertexAfter(Vector2 pos, Vector2 after)
        {
            if (!Contains(after))
            {
                throw new ArgumentException("Polygon does not contain vertex after which to add");
            }
            AddVertexAfter(pos, m_vertices.Find(after));
        }

        private void AddVertexAfter(Vector2 pos, LinkedListNode<Vector2> node)
        {
            if (node == null) throw new GeomException("Adding vertex after null node");

            m_vertices.AddAfter(node, pos);
        }

        public void RemoveVertex(Vector2 pos)
        {
            m_vertices.Remove(pos);
        }

        public void RemoveFirst()
        {
            m_vertices.RemoveFirst();
        }

        public void RemoveLast()
        {
            m_vertices.RemoveLast();
        }

        public void Clear()
        {
            m_vertices.Clear();
        }

        /// <summary>
        /// Simple Constructor 
        /// </summary>
        public Polygon2D()
        {
            m_vertices = new LinkedList<Vector2>();
        }

        /// <summary>
        /// Constructs a clockwise polygon with the given vertices.
        /// </summary>
        /// <param name="a_vertices"></param>
        public Polygon2D(IEnumerable<Vector2> a_vertices) : this()
        {
            foreach (var v in a_vertices) AddVertex(v);
        }

        /// <summary>
        /// Tests wheter this polygon is convex by verifying that each triplet of points constitutes a right turn
        /// </summary>
        public bool IsConvex()
        {
            if (VertexCount < 3)
            {
                throw new GeomException("Being convex is illdefined for polygons of 2 or less vertices");
            }

            // flip orientation if polygon counter clockwise 
            var dir = (IsClockwise() ? 1 : -1);

            var node = m_vertices.First;
            while(node.Next.Next != null)
            {
                if (dir * MathUtil.Orient2D(node.Value, node.Next.Value, node.Next.Next.Value) > 0)
                    return false;
                node = node.Next;
            }

            return true;
        }

        public bool Contains(Vector2 a_pos)
        {
            // cannot contain without area
            if (Area == 0) return false;
            
            if (IsConvex())
            {
                bool inv = IsClockwise();
                LineSegment segment;
                var node = m_vertices.First;
                while(node.Next != null)
                {
                    segment = new LineSegment(node.Value, node.Next.Value);
                    if (inv != segment.IsRightOf(a_pos))
                    {
                        return false;
                    }
                    node = node.Next;
                }
                segment = new LineSegment(m_vertices.Last.Value, m_vertices.First.Value);
                if (inv != segment.IsRightOf(a_pos))
                {
                    return false;
                }

                return true;
            }
            else
            {
                Debug.Assert(VertexCount > 3);

                // calculate triangulation
                var poly = RemoveDanglingEdges(this);
                var triangles = Triangulator.Triangulate(poly).Triangles.ToList();

                // check for triangle that contains point
                return triangles.Exists(t => t.Contains(a_pos));
            }
        }

        /// <summary>
        /// Shifts all polygon points such that the given point lies at origin.
        /// </summary>
        /// <param name="a_point"></param>
        public void ShiftToOrigin(Vector2 a_point)
        {
            m_vertices = new LinkedList<Vector2>(m_vertices.Select(v => v - a_point));
        }

        /// <summary>
        /// Dirty method O(n^2)
        /// </summary>
        /// <param name="a_poly1"></param>
        /// <param name="a_poly2"></param>
        /// <returns></returns>
        public static Polygon2D IntersectConvex(Polygon2D a_poly1, Polygon2D a_poly2)
        {
            if (!(a_poly1.IsConvex()))
            {
                throw new GeomException("Method not defined for nonconvex polygons" + a_poly1);
            }
            if (!(a_poly2.IsConvex()))
            {
                throw new GeomException("Method not defined for nonconvex polygons" + a_poly2);
            }

            // obtain vertices that lie inside both polygons
            var resultVertices = a_poly1.Vertices
                .Where(v => a_poly2.Contains(v))
                .Concat(a_poly2.Vertices.Where(v => a_poly1.Contains(v)))
                .ToList();

            // add intersections between two polygon segments
            resultVertices.AddRange(a_poly1.Segments.SelectMany(seg => seg.Intersect(a_poly2.Segments)));

            // remove any duplicates
            var resultVertexSet = new HashSet<Vector2>(resultVertices);

            // retrieve convex hull of relevant vertices
            if (resultVertexSet.Count >= 3)
            {
                var poly = ConvexHull.ComputeConvexHull(resultVertexSet);
                Debug.Assert(poly.IsConvex());
                return poly;
            }

            return null;

        }

        /// <summary>
        /// Determines wheter or not this polygon is clockwise using the Shoelace formula (in O(n))
        /// </summary>
        /// <returns></returns>
        public bool IsClockwise()
        {
            var sum = 0f;
            foreach (LineSegment seg in Segments)
            {
                sum += (seg.Point2.x - seg.Point1.x) * (seg.Point2.y + seg.Point1.y);
            }

            if (MathUtil.GreaterEps(sum, 0f))
            {
                return true;
            }
            //Debug.Assert(sum != 0);
            return false;
        }

        public void Reverse()
        {
            m_vertices = new LinkedList<Vector2>(m_vertices.Reverse());
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
            foreach (var seg1 in Segments)
            {
                foreach (var seg2 in Segments)
                {
                    if (seg1 != seg2 && seg1.IntersectProper(seg2) != null) { Debug.Log(seg1); Debug.Log(seg2); return false; }
                }
            }
            return true;
        }

        public Rect BoundingBox(float margin = 0f)
        {
            return BoundingBoxComputer.FromVector2(Vertices, margin);
        }

        /// <summary>
        /// Removes any parts of the polygon where the boundary reverses (a dangling edge).
        /// </summary>
        /// <remarks>
        /// Useful for easier triangulation, whenever only area or containment matters.
        /// </remarks>
        /// <param name="poly"></param>
        /// <returns></returns>
        public static Polygon2D RemoveDanglingEdges(Polygon2D poly)
        {
            var result = new Polygon2D(poly.Vertices);

            // iterate until no more dangling edges
            bool containsDanglingEdge = true;
            while (containsDanglingEdge)
            {
                containsDanglingEdge = false;

                // find dangling edge
                Vector2? toRemove = null;
                foreach (var vertex in result.Vertices)
                {
                    if (vertex == result.Next(vertex) || vertex == result.Prev(vertex) 
                        || result.Prev(vertex) == result.Next(vertex))
                    {
                        containsDanglingEdge = true;
                        toRemove = vertex;
                        break;
                    }
                }
                
                // remove dangling edge
                if (containsDanglingEdge)
                {
                    result.Vertices.Remove(toRemove.Value);
                }
            }

            return result;
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
                if (vertices[i] != otherVertices[i]) return false;
            }

            return true;
        }
    }
}