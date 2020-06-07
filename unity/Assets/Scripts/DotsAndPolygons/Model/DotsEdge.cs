using System;
using Util.Geometry;

namespace DotsAndPolygons
{
    [Serializable]
    public class DotsEdge
    {
        private SerializableLineSegment SerializableLineSegment { get; set; }
        public LineSegment Segment
        {
            get => SerializableLineSegment.LineSegment;
            set => SerializableLineSegment = new SerializableLineSegment(value);
        }

        public DotsHalfEdge RightPointingHalfEdge { get; set; } = null;
        public DotsHalfEdge LeftPointingHalfEdge { get; set; } = null;

        public int Player { get; set; }


        public DotsEdge(DotsHalfEdge leftPointingHalfEdge, DotsHalfEdge rightPointingHalfEdge)
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

        public DotsEdge Clone() => new DotsEdge(this.Segment);
    }
}