namespace Util.Geometry.Triangulation
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry.Graph;
    using Util.Math;

    public class Triangle {

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

        public Triangle() : this(new Vector2(), new Vector2(), new Vector2())
        { }

        public Triangle(Vector2 v0, Vector2 v1, Vector2 v2) : this(
                new TriangleEdge(v0, v1, null, null),
                new TriangleEdge(v1, v2, null, null),
                new TriangleEdge(v2, v0, null, null)
            )
        {
            E0.T = this;
            E1.T = this;
            E2.T = this;
        }

        public Triangle(TriangleEdge e0, TriangleEdge e1, TriangleEdge e2)
        {
            if(e0.End != e1.Start || e1.End != e2.Start || e2.End != e0.Start)
            {
                throw new ArgumentException("Invalid triangle edges given.");
            }
            E0 = e0;
            E1 = e1;
            E2 = e2;
            P0 = e0.Start;
            P1 = e1.Start;
            P2 = e2.Start;
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
            return OtherVertex(a_Edge.Start, a_Edge.End);
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
                if (e != a && e.ContainsEndpoint(b)) return e;
            return null;
        }

        public override string ToString()
        {
            return "Triangle: {" + P0 + ", " + P1 + ", " + P2 + "}";
        }
    }
}

