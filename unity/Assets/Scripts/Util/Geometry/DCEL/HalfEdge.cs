namespace Util.Geometry.DCEL
{
    /// <summary>
    /// Represents connection in planar embedding between two vertices.
    /// Main component of the DCEL.
    /// 
    /// Contains pointers to the corresponding face and previous/next edge in cycle.
    /// Also stores pointer to twin edge going opposite direction, belonging to adjacent face (possible same)
    /// </summary>
    public class HalfEdge
    {
        public Face Face { get; set; }
        public HalfEdge Next { get; set; }
        public HalfEdge Prev { get; set; }
        public DCELVertex From { get; set; }
        public DCELVertex To { get; set; }
        public HalfEdge Twin { get; set; }

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
        }

        public override string ToString()
        {
            return "(" + From + ", " + To + ")";
        }
    }
}