namespace Util.Geometry.Polygon
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// A class representing a general Polygon (possibly with holes)
    /// </summary>
    public class Polygon2DWithHoles : IPolygon2D
    {
        private readonly List<Polygon2D> m_holes;

        public Polygon2D Outside { get; private set; }

        public ICollection<Polygon2D> Holes { get { return m_holes; } }

        public ICollection<Vector2> Vertices
        {
            get
            {
                var vertices = Outside.Vertices;
                foreach (var p in m_holes)
                {
                    vertices.Concat(p.Vertices);
                }
                return vertices;
            }
        }

        public int VertexCount { get { return Outside.VertexCount + m_holes.Sum(p => p.VertexCount); } }


        /// <summary>
        /// Computes the area of this polygon minus it's holes
        /// </summary>
        /// <returns></returns>
        public float Area
        {
            get
            {
                var result = Outside.Area;
                foreach (var hole in m_holes)
                {
                    result -= hole.Area;
                }
                if (result < 0)
                {
                    throw new GeomException("Somehow ended up with negative area");
                }
                return result;
            }
        }

        public ICollection<LineSegment> Segments
        {
            get
            {
                var segments = Outside.Segments;
                foreach (var h in m_holes)
                {
                    segments.Concat(h.Segments);
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
            Outside = a_outside;
        }

        public Polygon2DWithHoles(Polygon2D a_outside, List<Polygon2D> a_holes)
        {
            Outside = a_outside;
            m_holes = a_holes;
        }

        public Vector2? Next(Vector2 pos)
        {
            if (Outside.Contains(pos)) return Outside.Next(pos);
            foreach (var p in m_holes)
            {
                if (p.Contains(pos)) return p.Next(pos);
            }
            return null;
        }

        public Vector2? Prev(Vector2 pos)
        {
            if (Outside.Contains(pos)) return Outside.Prev(pos);
            foreach (var p in m_holes)
            {
                if (p.Contains(pos)) return p.Prev(pos);
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

        public void AddVertexAfter(Vector2 after, Vector2 pos)
        {
            Outside.AddVertexAfter(pos, after);
        }

        public void AddHole(Polygon2D hole)
        {
            m_holes.Add(hole);
        }

        public void RemoveHole(Polygon2D hole)
        {
            m_holes.Remove(hole);
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
        }

        public bool IsConvex()
        {
            return Outside.IsConvex() && m_holes.Count == 0;
        }

        public bool IsSimple()
        {
            return false; // TODO
        }

        /// <summary>
        /// Returns whether a position is contained in the polygon
        /// </summary>
        /// <param name="a_pos"></param>
        /// <returns></returns>
        public bool Contains(Vector2 a_pos)
        {
            foreach (var hole in m_holes)
            {
                if (hole.Contains(a_pos)) return false;
            }

            return Outside.Contains(a_pos);
        }

        public bool IsClockwise()
        {
            bool ret = Outside.IsClockwise();
            foreach (var p in m_holes) ret &= !p.IsClockwise();
            return ret;
        }


        public void Reverse()
        {
            Outside.Reverse();
            foreach (var p in m_holes) p.Reverse();
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
