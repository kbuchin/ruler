namespace Util.Geometry.Triangulation
{
    using UnityEngine;

    /// <summary>
    /// Half edge in triangulation that connects two points (extension of line segment).
    /// Stores pointers to corresponding triangle and twin edge, which belongs to adjacent triangle.
    /// </summary>
    public class TriangleEdge : LineSegment
    {
        /// <summary>
        /// Twin half edge belonging to adjacent triangle.
        /// </summary>
        public TriangleEdge Twin { get; set; }

        /// <summary>
        /// Triangle this edge belongs to.
        /// </summary>
        public Triangle T { get; set; }

        /// <summary>
        /// Whether this edge is on the outer boundary of a triangulation.
        /// In this case it will have no twin halfedge.
        /// </summary>
        public bool IsOuter { get { return Twin == null; } }

        public TriangleEdge(Vector2 a_point1, Vector2 a_point2) : base(a_point1, a_point2)
        { }

        public TriangleEdge(Vector2 a_point1, Vector2 a_point2, TriangleEdge twin, Triangle t) : this(a_point1, a_point2)
        {
            Twin = twin;
            T = t;
        }

        public override string ToString()
        {
            return "(" + Point1 + ", " + Point2 + ")";
        }
    }
}
