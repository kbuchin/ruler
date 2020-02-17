using UnityEngine;
using Util.Geometry;

namespace DotsAndPolygons
{
    public class LineSegmentWithDotsEdge
    {
        public IDotsEdge DotsEdge { get; set; }
        public LineSegment Segment { get; set; }

        public LineSegmentWithDotsEdge(LineSegment segment, IDotsEdge dotsEdge)
        {
            Segment = segment;
            DotsEdge = dotsEdge;
        }

        public LineSegmentWithDotsEdge(Vector2 point1, Vector2 point2, IDotsEdge dotsEdge)
        {
            Segment = new LineSegment(point1, point2);
            DotsEdge = dotsEdge;
        }
    }
}