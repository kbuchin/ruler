namespace Util.Geometry.Triangulation
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry.Graph;

    public class TriangleEdge : LineSegment
    {
        public float Length { get { return (m_Point2 - m_Point1).magnitude; } }

        public TriangleEdge Twin { get; set; }
        public Triangle T { get; set; }

        public bool IsOuter { get; set; }

        public TriangleEdge(Vector2 a_point1, Vector2 a_point2) : base(a_point1, a_point2)
        {
            IsOuter = false;
        }

        public TriangleEdge(Vector2 a_point1, Vector2 a_point2, TriangleEdge twin, Triangle t) : this(a_point1, a_point2)
        {
            Twin = twin;
            T = t;
        }

        public override string ToString()
        {
            return "(" + m_Point1 + ", " + m_Point2 + ")";
        }
    }
}
