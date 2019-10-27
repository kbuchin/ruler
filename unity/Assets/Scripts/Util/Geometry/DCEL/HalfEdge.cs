namespace Util.Geometry.DCEL
{
    using System;
    using UnityEngine;
    using Util.Geometry.Graph;
    using Util.Math;

    /// <summary>
    /// Represents connection in planar embedding between two vertices.
    /// Main component of the DCEL.
    /// 
    /// Contains pointers to the corresponding face and previous/next edge in cycle.
    /// Also stores pointer to twin edge going opposite direction, belonging to adjacent face (possible same)
    /// </summary>
    public class HalfEdge
    {
        public Face Face { get; internal set; }
        public HalfEdge Next { get; internal set; }
        public HalfEdge Prev { get; internal set; }
        public DCELVertex From { get; internal set; }
        public DCELVertex To { get; internal set; }
        public HalfEdge Twin { get; internal set; }

        public float Magnitude { get { return Segment.Magnitude; } }
        public float SqrMagnitude { get { return Segment.SqrMagnitude; } }

        /// <summary>
        /// Line segment corresponding to the half edge.
        /// </summary>
        public LineSegment Segment
        {
            get { return new LineSegment(From.Pos, To.Pos); }
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

        /// <summary>
        /// Check whether points is right of the half edge. 
        /// </summary>
        /// <param name="a_Point"></param>
        /// <returns></returns>
        public bool IsRightOf(Vector2 a_Point)
        {
            return Segment.IsRightOf(a_Point);
        }


        public override string ToString()
        {
            return "(" + From + ", " + To + ")";
        }
    }
}