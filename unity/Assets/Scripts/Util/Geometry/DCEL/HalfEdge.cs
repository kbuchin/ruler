namespace Util.Geometry.DCEL
{
    using System;
    using UnityEngine;
    using Util.Geometry.Graph;

    public class HalfEdge
    {
        public Face Face { get; internal set; }
        public HalfEdge Next { get; internal set; }
        public HalfEdge Prev { get; internal set; }
        public Vertex From { get; internal set; }
        public Vertex To { get; internal set; }
        public HalfEdge Twin { get; internal set; }

        public LineSegment Segment
        {
            get { return new LineSegment(From.Pos, To.Pos); }
        }

        public bool IsBorder
        {
            get { return Face.IsOuter || Twin.Face.IsOuter; }
        }

        public float Magnitude { get { return Segment.Magnitude; } }
        public float SqrMagnitude { get { return Segment.SqrMagnitude; } }

        private FloatInterval XInterval
        {
            get { return Segment.XInterval; }
        }
        private FloatInterval YInterval
        {
            get { return Segment.YInterval; }
        }

        public HalfEdge(Vertex from, Vertex to)
        {
            From = from;
            To = to;
            if (SqrMagnitude < Mathf.Epsilon)
            {
                throw new GeomException("Creating edge of length zero.");
            }
        }

        public bool PointIsRightOf(Vector2 a_Point)
        {
            var line = new Line(From.Pos, To.Pos);
            return line.PointRightOfLine(a_Point);
        }

        public Vector2? IntersectLine(Line a_Line)
        {
            return Segment.Intersect(a_Line);
        }

        public override string ToString()
        {
            return "(" + From + ", " + To + ")";
        }
    }
}