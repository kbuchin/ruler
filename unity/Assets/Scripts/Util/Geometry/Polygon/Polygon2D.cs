namespace Util.Geometry.Polygon
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using System;
    using Util.Math;
    using Util.Algorithms.Triangulation;
    using Util.Algorithms.Polygon;

    /// <summary>
    /// We represent the polygon internally as a linked list of vertices.
    /// </summary>
    public class Polygon2D : IPolygon2D
    {
        private LinkedList<Vector2> m_vertices;

        public ICollection<Vector2> Vertices { get { return m_vertices; } }

        public int VertexCount { get { return m_vertices.Count; } }

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

        public void AddVertexAfter(Vector2 after, Vector2 pos)
        {
            if (!Contains(after))
            {
                throw new ArgumentException("Polygon does not contain vertex after which to add");
            }
            AddVertexAfter(m_vertices.Find(after), pos);
        }

        private void AddVertexAfter(LinkedListNode<Vector2> node, Vector2 pos)
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

        private Vector2 FindLeftMostVertex()
        {
            //init
            var minVertex = m_vertices.First.Value;
            var minX = minVertex.x;

            foreach (var v in m_vertices)
            {
                if (v.x < minX)
                {
                    minX = v.x;
                    minVertex = v;
                }
            }

            return minVertex;
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
        /// Computes the area spanned by this polygon
        /// </summary>
        /// The theory behind this method is documented in the docs folder.
        /// <returns></returns>
        public float Area()
        {
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

        /// <summary>
        /// Tests wheter this polygon is convex by verifying that each tripplet of points constitues a right turn
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
            if (VertexCount <= 2) return false; // polygon has no area

            if (Area() == 0) //catch case of "flat" triangle
            {
                return false;
            }
            if (IsConvex() && IsClockwise())
            {
                LineSegment segment;
                var node = m_vertices.First;
                while(node.Next != null)
                {
                    segment = new LineSegment(node.Value, node.Next.Value);
                    if (!segment.IsRightOf(a_pos))
                    {
                        return false;
                    }
                    node = node.Next;
                }
                segment = new LineSegment(m_vertices.Last.Value, m_vertices.First.Value);
                if (!segment.IsRightOf(a_pos))
                {
                    return false;
                }

                return true;
            }
            else
            {
                Debug.Assert(VertexCount > 3);

                foreach (var triangle in Triangulator.Triangulate(this).Triangles)
                {
                    if (triangle.Inside(a_pos))
                    {
                        return true;
                    }
                }
                return false;
            }
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

            var resultVertices = new List<Vector2>();

            foreach (Vector2 vertex in a_poly1.Vertices)
            {
                if (a_poly2.Contains(vertex))
                {
                    resultVertices.Add(vertex);
                }
            }

            foreach (Vector2 vertex in a_poly2.Vertices)
            {
                if (a_poly1.Contains(vertex))
                {
                    resultVertices.Add(vertex);
                }
            }

            foreach (LineSegment seg1 in a_poly1.Segments)
            {
                foreach (LineSegment seg2 in a_poly2.Segments)
                {
                    var intersection = seg1.Intersect(seg2);
                    if (intersection.HasValue)
                    {
                        resultVertices.Add(intersection.Value);
                    }
                }
            }
            if (resultVertices.Count >= 3)
            {
                return ConvexHull.ComputeConvexHull(new Polygon2D(resultVertices));
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

            if (sum > 0)
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
            return true; // TODO
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