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
                var vertices = Outside.Vertices.ToList();
                foreach (var p in m_holes)
                {
                    vertices.AddRange(p.Vertices);
                }
                return vertices;
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

        public Vector2 Next(Vector2 pos)
        {
            return Outside.Next(pos);
        }

        public Vector2 Prev(Vector2 pos)
        {
            return Outside.Prev(pos);
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
            return m_holes.Count == 0;
        }

        /// <summary>
        /// Computes the area of this polygon minus it's holes
        /// </summary>
        /// <returns></returns>
        public float Area()
        {
            var result = Outside.Area();
            foreach (var hole in m_holes)
            {
                result -= hole.Area();
            }
            if (result < 0)
            {
                throw new GeomException("Somehow ended up with negative area");
            }
            return result;
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
                if (hole.Contains(a_pos))
                {
                    return false;
                }
            }

            if (Outside.Contains(a_pos))
            {
                return true;
            }

            return false;
        }

        public bool IsClockwise()
        {
            bool ret = Outside.IsClockwise();
            foreach (var p in m_holes) ret &= Outside.IsClockwise();
            return ret;
        }
    }
}
