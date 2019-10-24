namespace Util.Geometry.DCEL
{
    using System;
    using UnityEngine;
    using Util.Geometry.Graph;
    using Util.Math;

    public class HalfEdge 
    {
        public Face Face { get; internal set; }
        public HalfEdge Next { get; internal set; }
        public HalfEdge Prev { get; internal set; }
        public DCELVertex From { get; internal set; }
        public DCELVertex To { get; internal set; }
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

        public HalfEdge(DCELVertex from, DCELVertex to)
        {
            From = from;
            To = to;
            /*
            if (Magnitude < MathUtil.EPS)
            {
                throw new GeomException(string.Format("Creating edge of length zero: {0}, {1}", from, to));
            }
            */
        }

        public bool PointIsRightOf(Vector2 a_Point)
        {
            var line = new Line(From.Pos, To.Pos);
            return line.PointRightOfLine(a_Point);
        }


        public override string ToString()
        {
            return "(" + From + ", " + To + ")";
        }
    }
}