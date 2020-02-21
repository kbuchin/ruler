namespace Util.Geometry.Polygon
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Math;

    /// <summary>
    /// A class representing a general Polygon, with a list of polygonal holes.
    /// Hole polygons cannot contain holes themselves.
    /// </summary>
    public class Polygon2DWithHoles : IPolygon2D
    {
        private readonly List<Polygon2D> m_holes;

        /// <summary>
        /// Outer polygon, defined simply as a 2D polygon without holes.
        /// </summary>
        public Polygon2D Outside { get; private set; }

        /// <summary>
        /// Collection of polygons that form the holes.
        /// Assumed to be disjoint.
        /// </summary>
        public ICollection<Polygon2D> Holes { get { return m_holes; } }

        /// <summary>
        /// Collection of both outer and inner vertices of the polygon.
        /// </summary>
        public ICollection<Vector2> Vertices
        {
            get
            {
                return OuterVertices.Concat(InnerVertices).ToList();
            }
        }

        public ICollection<Vector2> OuterVertices
        {
            get { return Outside.Vertices; }
        }

        public ICollection<Vector2> InnerVertices
        {
            get
            {
                var vertices = new List<Vector2>();
                foreach (var p in m_holes)
                {
                    vertices.AddRange(p.Vertices);
                }
                return vertices;
            }
        }

        public int VertexCount
        {
            get { return Outside.VertexCount + m_holes.Sum(p => p.VertexCount); }
        }

        /// <summary>
        /// Calculates the total area as the outer area minus area of holes.
        /// </summary>
        /// <remarks>
        /// Assumes the hole polygons are disjoint and are contained inside outer polygon
        /// </remarks>
        public float Area
        {
            get
            {
                // find area of outside polygon
                var result = Outside.Area;

                // remove hole area from total
                foreach (var hole in m_holes)
                {
                    result -= hole.Area;
                }

                if (MathUtil.LessEps(result, 0f))
                {
                    throw new GeomException("Holes cannot have more area than outside polygon");
                }

                return result;
            }
        }

        /// <summary>
        /// Collection of both inner and outer segments.
        /// </summary>
        public ICollection<LineSegment> Segments
        {
            get
            {
                // concat outer and inner segments
                var segments = new List<LineSegment>();
                segments.AddRange(OuterSegments);
                foreach (var h in m_holes)
                {
                    segments.AddRange(h.Segments);
                }
                return segments;
            }
        }

        /// <summary>
        /// Collection of segments of the outer polygon.
        /// </summary>
        public ICollection<LineSegment> OuterSegments
        {
            get { return Outside.Segments; }
        }

        /// <summary>
        /// Collection of segments of the inner polygon.
        /// </summary>
        public ICollection<LineSegment> InnerSegments
        {
            get
            {
                // concat outer and inner segments
                var segments = new List<LineSegment>();
                foreach (var h in m_holes)
                {
                    segments.AddRange(h.Segments);
                }
                return segments;
            }
        }

        public Polygon2DWithHoles()
        {
            Outside = null;
            m_holes = new List<Polygon2D>();
        }

        public Polygon2DWithHoles(Polygon2D a_outside) : this()
        {
            Outside = new Polygon2D(a_outside.Vertices);
        }

        public Polygon2DWithHoles(Polygon2D a_outside, IEnumerable<Polygon2D> a_holes)
        {
            Outside = new Polygon2D(a_outside.Vertices);
            m_holes = a_holes.Select(p => new Polygon2D(p.Vertices)).ToList();
        }

        public Vector2? Next(Vector2 pos)
        {
            if (Outside.ContainsVertex(pos)) return Outside.Next(pos);
            foreach (var p in m_holes)
            {
                if (p.ContainsVertex(pos)) return p.Next(pos);
            }
            return null;
        }

        public Vector2? Prev(Vector2 pos)
        {
            if (Outside.ContainsVertex(pos)) return Outside.Prev(pos);
            foreach (var p in m_holes)
            {
                if (p.ContainsVertex(pos)) return p.Prev(pos);
            }
            return null;
        }

        public void AddVertex(Vector2 pos)
        {
            Outside.AddVertex(pos);
        }

        public void AddVertexFirst(Vector2 pos)
        {
            Outside.AddVertexFirst(pos);
        }

        public void AddVertexAfter(Vector2 pos, Vector2 after)
        {
            Outside.AddVertexAfter(pos, after);
        }

        public void AddHole(Polygon2D hole)
        {
            m_holes.Add(hole);
        }

        public void RemoveHole(Polygon2D hole)
        {
            m_holes.RemoveAll(h => h.Equals(hole));
        }

        public void RemoveHoles()
        {
            m_holes.Clear();
        }

        public void RemoveVertex(Vector2 pos)
        {
            Outside.RemoveVertex(pos);
        }

        public void RemoveFirst()
        {
            Outside.RemoveFirst();
        }

        public void RemoveLast()
        {
            Outside.RemoveLast();
        }

        public void Clear()
        {
            Outside.Clear();
            foreach (var p in m_holes) p.Clear();
            m_holes.Clear();
        }

        public bool IsConvex()
        {
            return Outside.IsConvex() && m_holes.Count == 0;
        }

        public bool IsSimple()
        {
            return Outside.IsSimple() && m_holes.Count == 0;
        }

        public bool ContainsInside(Vector2 a_pos)
        {
            return Outside.ContainsInside(a_pos) && !m_holes.Exists(h => h.ContainsInside(a_pos) || h.OnBoundary(a_pos));
        }

        public bool OnBoundary(Vector2 a_pos)
        {
            return Outside.OnBoundary(a_pos) || m_holes.Exists(h => h.OnBoundary(a_pos));
        }

        public bool ContainsVertex(Vector2 pos)
        {
            return Outside.ContainsVertex(pos) || m_holes.Exists(p => p.ContainsVertex(pos));
        }

        /// <summary>
        /// Shifts all polygon points such that the given point lies at origin.
        /// </summary>
        /// <param name="a_point"></param>
        public void ShiftToOrigin(Vector2 a_point)
        {
            Outside.ShiftToOrigin(a_point);
            foreach (var h in m_holes) h.ShiftToOrigin(a_point);
        }

        public bool IsClockwise()
        {
            bool ret = Outside.IsClockwise();
            foreach (var p in m_holes) ret &= p.IsClockwise();  // holes are also clockwise (not counterclockwise)
            return ret;
        }

        public void Reverse()
        {
            Outside.Reverse();
            foreach (var p in m_holes) p.Reverse();
        }

        public Rect BoundingBox(float margin = 0f)
        {
            return BoundingBoxComputer.FromPoints(Vertices, margin);
        }

        public bool Equals(IPolygon2D other)
        {
            var poly = other as Polygon2DWithHoles;
            if (poly == null) return false;

            if (!Outside.Equals(poly.Outside)) return false;
            if (Holes.Count != poly.Holes.Count) return false;

            var holes = Holes.ToList();
            var otherHoles = poly.Holes.ToList();

            for (var i = 0; i < holes.Count; i++)
            {
                if (!holes[i].Equals(otherHoles[i])) return false;
            }

            return true;
        }
    }
}
