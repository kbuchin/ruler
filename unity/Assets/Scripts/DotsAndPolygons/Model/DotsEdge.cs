using Util.Geometry;

namespace DotsAndPolygons
{
    public class DotsEdge : IDotsEdge
    {
        public LineSegment Segment { get; set; }

        public IDotsHalfEdge RightPointingHalfEdge { get; set; } = null;
        public IDotsHalfEdge LeftPointingHalfEdge { get; set; } = null;

        public int Player { get; set; }


        public DotsEdge(IDotsHalfEdge leftPointingHalfEdge, IDotsHalfEdge rightPointingHalfEdge)
        {
            LeftPointingHalfEdge = leftPointingHalfEdge;
            RightPointingHalfEdge = rightPointingHalfEdge;
            Segment = new LineSegment(rightPointingHalfEdge.Origin.Coordinates,
                leftPointingHalfEdge.Origin.Coordinates);
        }

        public DotsEdge(LineSegment segment)
        {
            Segment = segment;
        }

        public override string ToString() => $"{Segment}, Player: {Player}";
    }
}