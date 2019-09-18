namespace Util.Geometry.Triangulation
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry.Graph;
    using Util.Math;

    public class Triangle : IEquatable<Triangle> {

        public ICollection<Vector2> Vertices {
            get { return new List<Vector2> { P0, P1, P2 }; }
        }

        public ICollection<TriangleEdge> Edges {
            get { return new List<TriangleEdge> { E0, E1, E2 }; }
        }

        public Vector2 P0 { get; private set; }
        public Vector2 P1 { get; private set; }
        public Vector2 P2 { get; private set; }

        public TriangleEdge E0 { get; private set; }
        public TriangleEdge E1 { get; private set; }
        public TriangleEdge E2 { get; private set; }

        public Vector2 Circumcenter { get; private set; }

        public bool IsOuter { get { return E0.IsOuter || E1.IsOuter || E2.IsOuter; } }

        public Triangle() : this(new Vector2(), new Vector2(), new Vector2())
        { }

        public Triangle(Vector2 v0, Vector2 v1, Vector2 v2) : this(
                new TriangleEdge(v0, v1, null, null),
                new TriangleEdge(v1, v2, null, null),
                new TriangleEdge(v2, v0, null, null)
            )
        { }

        public Triangle(TriangleEdge e0, TriangleEdge e1, TriangleEdge e2)
        {
            if(e0.Point2 != e1.Point1 || e1.Point2 != e2.Point1 || e2.Point2 != e0.Point1)
            {
                throw new ArgumentException("Invalid triangle edges given.");
            }
            E0 = e0;
            E1 = e1;
            E2 = e2;
            P0 = e0.Point1;
            P1 = e1.Point1;
            P2 = e2.Point1;
            e0.T = this;
            e1.T = this;
            e2.T = this;
            if (!Degenerate()) {
                if(IsClockwise()) Circumcenter = MathUtil.CalculateCircumcenter(P0, P1, P2);
                else Circumcenter = MathUtil.CalculateCircumcenter(P0, P2, P1);
            }
        }

        private bool Degenerate()
        {
            return !MathUtil.IsFinite(P0) ||
                   !MathUtil.IsFinite(P1) ||
                   !MathUtil.IsFinite(P2) ||
                   MathUtil.Colinear(P0, P1, P2);
        }

        public bool Contains(Vector2 x)
        {
            if(IsClockwise())
            {
                return E0.IsRightOf(x) && E1.IsRightOf(x) && E2.IsRightOf(x);
            }
            else
            {
                return !E0.IsRightOf(x) && !E1.IsRightOf(x) && !E2.IsRightOf(x);
            }
        }

        public bool ContainsEndpoint(Vector2 x)
        {
            return x.Equals(P0) || x.Equals(P1) || x.Equals(P2);
        }

        public bool Inside(Vector2 X)
        {
            int firstSide = Math.Sign(MathUtil.Orient2D(P0, P1, X));
            int secondSide = Math.Sign(MathUtil.Orient2D(P1, P2, X));
            int thirdSide = Math.Sign(MathUtil.Orient2D(P2, P0, X));
            return (firstSide != 0 && firstSide == secondSide && secondSide == thirdSide);
        }

        public bool IsClockwise()
        {
            return MathUtil.Orient2D(P0, P1, P2) < 0;
        }

        public bool InsideCircumcircle(Vector2 X)
        {
            return MathUtil.InsideCircle(P0, P1, P2, X);
        }

        public Vector2? OtherVertex(TriangleEdge a_Edge)
        {
            return OtherVertex(a_Edge.Point1, a_Edge.Point2);
        }
            
        public Vector2? OtherVertex(Vector2 a, Vector2 b)
        {
            foreach (var v in Vertices)
                if(v != a && v != b) return v;
            return null;
        }

        public TriangleEdge OtherEdge(TriangleEdge a, Vector2 b)
        {
            foreach (var e in Edges)
                if (e != a && e.IsEndpoint(b)) return e;
            return null;
        }

        public override string ToString()
        {
            return string.Format("Triangle: <{0}, {1}, {2}>", P0, P1, P2);
        }

        public bool Equals(Triangle other)
        {
            return P0 == other.P0 && P1 == other.P1 && P2 == other.P2;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}

