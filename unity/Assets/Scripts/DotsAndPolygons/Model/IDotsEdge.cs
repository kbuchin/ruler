using Util.Geometry;

namespace DotsAndPolygons
{
    public interface IDotsEdge
    {
        // The line segment representing this Edge
        LineSegment Segment { get; set; }

        IDotsHalfEdge RightPointingHalfEdge { get; set; }
        IDotsHalfEdge LeftPointingHalfEdge { get; set; }

        // The player that created this edge, either 1 or 2
        int Player { get; set; }

        string ToString();
    }
}