namespace Util.Geometry.Polygon
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using UnityEngine;
    using System;
    using Util.Math;
    using Util.Geometry.Duality;
    using Util.Algorithms.Triangulation;
    using Util.Algorithms.Polygon;

    /// <summary>
    /// We represent the polygon internally as a linked list of vertices.
    /// </summary>
    public class Polygon2D : IPolygon2D
    {
        private LinkedList<Vector2> m_vertices;
        private Vector2? m_vertexMean = null;
        private Vector2? m_leftMostVertex = null;

        public ICollection<Vector2> Vertices { get { return m_vertices; } }

        public int VertexCount { get { return m_vertices.Count; } }

        public Vector2? VertexMean
        {
            get { return m_vertexMean; }
        }

        public Vector2? LeftMostVertex
        {
            get { return m_leftMostVertex; }
        }


        public Vector2 Next(Vector2 pos)
        {
            var node = m_vertices.Find(pos).Next;
            if (node == null) node = m_vertices.First;
            return node.Value;
        }

        public Vector2 Prev(Vector2 pos)
        {
            var node = m_vertices.Find(pos).Previous;
            if (node == null) node = m_vertices.Last;
            return node.Value;
        }

        public void AddVertex(Vector2 pos)
        {
            AddVertexAfter(m_vertices.Last, pos);
        }

        public void AddVertexFirst(Vector2 pos)
        {
            AddVertexAfter(m_vertices.First, pos);
        }

        public void AddVertexAfter(Vector2 pos, Vector2 after)
        {
            if (!Contains(after))
            {
                throw new ArgumentException("Polygon does not contain vertex after which to add");
            }
            AddVertexAfter(m_vertices.Find(after), pos);
        }

        private void AddVertexAfter(LinkedListNode<Vector2> node, Vector2 pos)
        {
            m_vertices.AddAfter(node, pos);

            if (!m_leftMostVertex.HasValue || m_leftMostVertex.Value.x > pos.x)
                m_leftMostVertex = pos;

            if (m_vertexMean.HasValue)
                m_vertexMean = (m_vertexMean * m_vertices.Count + pos) / (m_vertices.Count + 1);
            else
                m_vertexMean = pos;

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
            m_leftMostVertex = null;
            m_vertexMean = null;
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

            //add up signed areas allong the edges of the polygon
            var areasum = 0f;
            foreach (LineSegment seg in Segments())
            {
                var v1 = seg.Point1;
                var v2 = seg.Point2;
                areasum += v1.x * v2.y - v2.x * v1.y;
            }

            return Math.Abs(areasum) / 2;
        }

        /// <summary>
        /// Returns a list of all the line segments forming the polygon oriented in a counterclockwise manner.
        /// </summary>
        /// <returns></returns>
        public List<LineSegment> Segments()
        {
            if (m_vertices.Count <= 1) return new List<LineSegment>();

            var result = new List<LineSegment>(Vertices.Count);
            var node = m_vertices.First;
            while (node.Next != null)
            {
                result.Add(new LineSegment(node.Value, node.Next.Value));
                node = node.Next;
            }
            result.Add(new LineSegment(m_vertices.Last.Value, m_vertices.First.Value));
            return result;
        }

        /// <summary>
        /// returns a list of all the tangent lines
        /// </summary>
        /// <returns></returns>
        public List<Line> Lines()
        {
            var result = new List<Line>(Vertices.Count);
            foreach (var seg in Segments()) result.Add(seg.Line);
            return result;
        }

        /// <summary>
        /// Tests wheter this polygon is clockwise and convex by verifying that each tripple of points constitues a right turn
        /// </summary>
        public bool IsConvex()
        {
            if (m_vertices.Count < 3)
            {
                throw new GeomException("Being convex is illdefined for polygons of 2 or less vertices");
            }

            var node = m_vertices.First;
            while(node.Next.Next != null)
            {
                if (MathUtil.Orient2D(node.Value, node.Next.Value, node.Next.Next.Value) < 0)
                    return false;
                node = node.Next;
            }

            return true;
        }

        public bool Contains(Vector2 a_pos)
        {
            if (m_vertices.Count <= 2) return false; // polygon has no area

            if (Area() == 0) //catch case of "flat" triangle
            {
                return false;
            }
            if (IsConvex())
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
                Debug.Assert(m_vertices.Count > 3);

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
        public static IPolygon2D IntersectConvex(Polygon2D a_poly1, Polygon2D a_poly2)
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

            foreach (LineSegment seg1 in a_poly1.Segments())
            {
                foreach (LineSegment seg2 in a_poly2.Segments())
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
            foreach (LineSegment seg in Segments())
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
    }
}