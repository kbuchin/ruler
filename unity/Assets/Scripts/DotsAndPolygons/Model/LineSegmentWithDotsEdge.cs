using UnityEngine;
using Util.Geometry;

namespace DotsAndPolygons
{
    public class LineSegmentWithDotsEdge
    {
        public DotsEdge DotsEdge { get; set; }
        public LineSegment Segment { get; set; }

        public LineSegmentWithDotsEdge(LineSegment segment, DotsEdge dotsEdge)
        {
            Segment = segment;
            DotsEdge = dotsEdge;
        }

        public LineSegmentWithDotsEdge(Vector2 point1, Vector2 point2, DotsEdge dotsEdge)
        {
            Segment = new LineSegment(point1, point2);
            DotsEdge = dotsEdge;
        }
    }
}