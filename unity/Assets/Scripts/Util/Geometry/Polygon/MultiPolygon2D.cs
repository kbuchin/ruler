namespace Util.Geometry.Polygon
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using System;
    using Util.Math;

    /// <summary>
    /// We represent the polygon internally as a linked list of vertices.
    /// </summary>
    public class MultiPolygon2D : IPolygon2D
    {
        private readonly List<Polygon2D> m_Polygons = new List<Polygon2D>();

        public ICollection<Vector2> Vertices {
            get
            {
                var vertices = new List<Vector2>();
                foreach (var p in m_Polygons) vertices.AddRange(p.Vertices);
                return vertices;
            }
        }

        public int VertexCount { get { return m_Polygons.Sum(p => p.VertexCount); } }

        /// <summary>
        /// Computes the area spanned by this multi polygon
        /// </summary>
        /// <remarks>
        /// Assumes the polygons do not overlap.
        /// </remarks>
        /// <returns></returns>
        public float Area
        {
            get { return m_Polygons.Sum(p => p.Area); }
        }

        public ICollection<LineSegment> Segments
        {
            get
            {
                var segments = new List<LineSegment>();
                foreach (var p in m_Polygons) segments.AddRange(p.Segments);
                return segments;
            }
        }

        public ICollection<Polygon2D> Polygons
        {
            get { return m_Polygons; }
        }

        /// <summary>
        /// Simple Constructor 
        /// </summary>
        public MultiPolygon2D()
        { }

        /// <summary>
        /// Simple Constructor 
        /// </summary>
        public MultiPolygon2D(Polygon2D polygon) : this()
        {
            AddPolygon(polygon);
        }

        /// <summary>
        /// Constructs a clockwise polygon with the given vertices.
        /// </summary>
        /// <param name="a_vertices"></param>
        public MultiPolygon2D(IEnumerable<Polygon2D> a_polygons) : this()
        {
            foreach (var p in a_polygons) AddPolygon(p);
        }


        public Vector2? Next(Vector2 pos)
        {
            foreach (var p in m_Polygons)
            {
                if (p.Contains(pos)) return p.Next(pos);
            }
            return null;
        }

        public Vector2? Prev(Vector2 pos)
        {
            foreach (var p in m_Polygons)
            {
                if (p.Contains(pos)) return p.Prev(pos);
            }
            return null;
        }

        public void AddPolygon(Polygon2D polygon)
        {
            if (polygon == null)
            {
                throw new GeomException("Cannot add null polygon");
            }
            m_Polygons.Add(polygon);
        }

        public void AddVertex(Vector2 pos)
        {
            throw new NotSupportedException("Method ill-defined on multi polygon");
        }

        public void AddVertexFirst(Vector2 pos)
        {
            throw new NotSupportedException("Method ill-defined on multi polygon");
        }

        public void AddVertexAfter(Vector2 pos, Vector2 after)
        {
            foreach (var p in m_Polygons)
            {
                if (Contains(after))
                {
                    AddVertexAfter(pos, after);
                }
            }
            throw new ArgumentException("Multi polygon does not contain vertex after which to add");
        }

        public void RemoveVertex(Vector2 pos)
        {
            foreach (var p in m_Polygons)
            {
                p.RemoveVertex(pos);
            }
        }

        public void RemoveFirst()
        {
            throw new NotSupportedException("Method ill-defined on multi polygon");
        }

        public void RemoveLast()
        {
            throw new NotSupportedException("Method ill-defined on multi polygon");
        }

        public void Clear()
        {
            foreach (var p in m_Polygons)
            {
                p.Clear();
            }
            m_Polygons.Clear();
        }

        /// <summary>
        /// Tests wheter this polygon is clockwise and convex by verifying that each tripple of points constitues a right turn
        /// </summary>
        public bool IsConvex()
        {
            return m_Polygons.TrueForAll(p => p.IsConvex());
        }

        public bool Contains(Vector2 a_pos)
        {
            return m_Polygons.Exists(p => p.Contains(a_pos));
        }

        /// <summary>
        /// Determines wheter or not this polygon is clockwise using the Shoelace formula (in O(n))
        /// </summary>
        /// <returns></returns>
        public bool IsClockwise()
        {
            return m_Polygons.TrueForAll(p => p.IsClockwise());
        }

        public void Reverse()
        {
            foreach (var p in m_Polygons) p.Reverse();
        }

        public override string ToString()
        {
            var str = "MultiPolygon: {\n";
            foreach (var p in m_Polygons)
            {
                str += p + "\n";
            }
            return str + "}";
        }

        public bool IsSimple()
        {
            Debug.Log("Simple ill-defined on multiple polygons");
            return m_Polygons.All(p => p.IsSimple()); // assumes polygons disjoint
        }

        public Rect BoundingBox(float margin = 0f)
        {
            return BoundingBoxComputer.FromVector2(Vertices, margin);
        }

        public bool Equals(IPolygon2D other)
        {
            var poly = other as MultiPolygon2D;
            if (poly == null) return false;

            if (Polygons.Count != poly.Polygons.Count) return false;

            var polygons = Polygons.ToList();
            var otherPolygons = poly.Polygons.ToList();

            for (var i = 0; i < Polygons.Count; i++)
            {
                if (!polygons[i].Equals(otherPolygons[i])) return false;
            }

            return true;
        }
    }
}