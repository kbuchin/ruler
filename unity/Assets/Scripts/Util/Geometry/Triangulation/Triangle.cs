namespace Util.Geometry.Triangulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Math;

    /// <summary>
    /// Simple triangle class for use in triangulations.
    /// Stores triangle edges explicitly while points are inferred from edges.
    /// </summary>
    public class Triangle : IEquatable<Triangle>
    {
        // three edges that define triangle
        public TriangleEdge E0 { get; private set; }
        public TriangleEdge E1 { get; private set; }
        public TriangleEdge E2 { get; private set; }

        // three points are inferred from edges
        public Vector2 P0 { get { return E0.Point1; } }
        public Vector2 P1 { get { return E1.Point1; } }
        public Vector2 P2 { get { return E2.Point1; } }

        private Vector2? m_circumCenter;

        /// <summary>
        /// Collection of the three vertices in the triangle for easy iteration.
        /// </summary>
        public List<Vector2> Vertices
        {
            get { return new List<Vector2> { P0, P1, P2 }; }
        }

        /// <summary>
        /// Collection of the three edges in the triangle for easy iteration.
        /// </summary>
        public List<TriangleEdge> Edges
        {
            get { return new List<TriangleEdge> { E0, E1, E2 }; }
        }

        /// <summary>
        /// Area covered by the triangle
        /// </summary>
        public float Area
        {
            get
            {
                // check degenerate cases
                if (Line.Colinear(P0, P1, P2)) return 0f;
                if (Degenerate) return float.NaN;

                // herons formula
                var s = (E0.Magnitude + E1.Magnitude + E2.Magnitude) / 2f;
                return Mathf.Sqrt(s * (s - E0.Magnitude) * (s - E1.Magnitude) * (s - E2.Magnitude));
            }
        }

        /// <summary>
        /// Center of the circle that goes through all three triangle points.
        /// </summary>
        public Vector2? Circumcenter
        { 
            get
            {
                if (!m_circumCenter.HasValue)
                {
                    m_circumCenter = CalculateCircumcenter(P0, P1, P2);
                }
                return m_circumCenter;
            }
        }

        public Vector2 Centroid
        {
            get
            {
                return new Vector2((P0.x + P1.x + P2.x) / 3f, (P0.y + P1.y + P2.y) / 3f);
            }
        }

        /// <summary>
        /// Whether the given triangle is a degenerate case (area zero or infinite).
        /// </summary>
        public bool Degenerate
        {
            get
            {
                return !MathUtil.IsFinite(P0) ||
                    !MathUtil.IsFinite(P1) ||
                    !MathUtil.IsFinite(P2) ||
                    Line.Colinear(P0, P1, P2);
            }
        }

        /// <summary>
        /// Whether the given triangle has an edge without an adjacent triangle.
        /// Useful for a triangulation.
        /// </summary>
        public bool IsOuter { get { return E0.IsOuter || E1.IsOuter || E2.IsOuter; } }

        public Triangle() : this(new Vector2(), new Vector2(), new Vector2())
        { }

        public Triangle(Vector2 a_vertex0, Vector2 a_vertex1, Vector2 a_vertex2) : this(
                new TriangleEdge(a_vertex0, a_vertex1, null, null),
                new TriangleEdge(a_vertex1, a_vertex2, null, null),
                new TriangleEdge(a_vertex2, a_vertex0, null, null)
            )
        { }

        public Triangle(TriangleEdge a_edge0, TriangleEdge a_edge1, TriangleEdge a_edge2)
        {
            if (a_edge0.Point2 != a_edge1.Point1 || a_edge1.Point2 != a_edge2.Point1 || a_edge2.Point2 != a_edge0.Point1)
            {
                throw new GeomException("Invalid triangle edges given: " + a_edge0 + " " + a_edge1 + " " + a_edge2);
            }

            E0 = a_edge0;
            E1 = a_edge1;
            E2 = a_edge2;

            // set the triangle pointer of the edges
            a_edge0.T = a_edge1.T = a_edge2.T = this;
        }

        /// <summary>
        /// Check whether point is contained inside triangle.
        /// </summary>
        /// <param name="a_pos"></param>
        /// <returns></returns>
        public bool Contains(Vector2 a_pos)
        {
            // edge case when pos is on boundary triangle
            if (E0.IsOnSegment(a_pos) || E1.IsOnSegment(a_pos) || E2.IsOnSegment(a_pos))
            {
                return true;
            }

            int firstSide = MathUtil.Orient2D(P0, P1, a_pos);
            int secondSide = MathUtil.Orient2D(P1, P2, a_pos);
            int thirdSide = MathUtil.Orient2D(P2, P0, a_pos);
            return (firstSide != 0 && firstSide == secondSide && secondSide == thirdSide);
        }

        /// <summary>
        /// Checks whether the point is equal to one of the three endpoints.
        /// </summary>
        /// <param name="a_pos"></param>
        /// <returns></returns>
        public bool ContainsEndpoint(Vector2 a_pos)
        {
            return a_pos.Equals(P0) || a_pos.Equals(P1) || a_pos.Equals(P2);
        }

        /// <summary>
        /// Returns whether the triangle is orientated clockwise.
        /// </summary>
        /// <returns></returns>
        public bool IsClockwise()
        {
            return MathUtil.Orient2D(P0, P1, P2) < 0;
        }

        /// <summary>
        /// Checks whether the given point is inside the circumcircle of the triangle.
        /// </summary>
        /// <param name="a_pos"></param>
        /// <returns></returns>
        public bool InsideCircumcircle(Vector2 a_pos)
        {
            // degenerate triangle
            if (!Circumcenter.HasValue) return false;

            return MathUtil.LessEps(Vector2.Distance(Circumcenter.Value, a_pos), 
                Vector2.Distance(Circumcenter.Value, P0));
        }

        /// <summary>
        /// Find the vertex that is not covered by the given triangle edge.
        /// </summary>
        /// <param name="a_edge"></param>
        /// <returns></returns>
        public Vector2? OtherVertex(TriangleEdge a_edge)
        {
            if (a_edge != E0 && a_edge != E1 && a_edge != E2)
            {
                throw new GeomException(string.Format("{0} not equal to any triangle edge of {1}", a_edge, this));
            }
            return OtherVertex(a_edge.Point1, a_edge.Point2);
        }

        /// <summary>
        /// Find the vertex
        /// </summary>
        /// <param name="a_vertex0"></param>
        /// <param name="a_vertex1"></param>
        /// <returns></returns>
        public Vector2? OtherVertex(Vector2 a_vertex0, Vector2 a_vertex1)
        {
            if (!Vertices.Contains(a_vertex0) || !Vertices.Contains(a_vertex1))
            {
                throw new GeomException("One of the vertices not contained in triangle");
            }
            return Vertices.ToList().Find(v => v != a_vertex0 && v != a_vertex1);
        }

        /// <summary>
        /// Finds the edge that is not equal to given two.
        /// </summary>
        /// <param name="a_edge0"></param>
        /// <param name="a_edge1"></param>
        /// <returns></returns>
        public TriangleEdge OtherEdge(TriangleEdge a_edge0, TriangleEdge a_edge1)
        {
            if (!Edges.Contains(a_edge0) || !Edges.Contains(a_edge1))
            {
                throw new GeomException("One of the edges not contained in triangle");
            }
            return Edges.Find(e => e != a_edge0 && e != a_edge1);
        }

        /// <summary>
        /// Finds edge that connects to the vertex and is not equal to given edge.
        /// </summary>
        /// <param name="a_edge"></param>
        /// <param name="a_vertex"></param>
        /// <returns></returns>
        public TriangleEdge OtherEdge(TriangleEdge a_edge, Vector2 a_vertex)
        {
            if (!Edges.Contains(a_edge) || !Vertices.Contains(a_vertex))
            {
                throw new GeomException("Edge or vertex not contained in triangle");
            }
            return Edges.Find(e => e != a_edge && e.IsEndpoint(a_vertex));
        }

        public override string ToString()
        {
            return string.Format("Triangle: <{0}, {1}, {2}>", P0, P1, P2);
        }

        public bool Equals(Triangle a_triangle)
        {
            // allow relabeling of vertices to be equal
            // as long as orientation equal
            return
            (MathUtil.EqualsEps(P0, a_triangle.P0) && MathUtil.EqualsEps(P1, a_triangle.P1) && MathUtil.EqualsEps(P2, a_triangle.P2)) ||
            (MathUtil.EqualsEps(P0, a_triangle.P1) && MathUtil.EqualsEps(P1, a_triangle.P2) && MathUtil.EqualsEps(P2, a_triangle.P0)) ||
            (MathUtil.EqualsEps(P0, a_triangle.P2) && MathUtil.EqualsEps(P1, a_triangle.P0) && MathUtil.EqualsEps(P2, a_triangle.P1));
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// Calculates the center point of a circle defined by points a, b, c.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns>the circumcenter of circle abc.</returns>
        public static Vector2? CalculateCircumcenter(Vector2 a, Vector2 b, Vector2 c)
        { 
            if (Line.Colinear(a, b, c))
            {
                return null;
            }

            var bisector1 = new LineSegment(a, b).Bissector;
            var bisector2 = new LineSegment(b, c).Bissector;
            var intersect = bisector1.Intersect(bisector2);

            if (!intersect.HasValue)
            {
                // some fallback
                var bisector3 = new LineSegment(c, a).Bissector;
                intersect = bisector2.Intersect(bisector3);
                if (!intersect.HasValue)
                {
                    intersect = bisector3.Intersect(bisector1);
                    if (!intersect.HasValue)
                    {
                        return null;
                    }
                }
            }

            return intersect.Value;
        }
    }
}

