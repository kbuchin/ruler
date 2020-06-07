using System;
using Util.Geometry;

namespace DotsAndPolygons
{
    // Half-edge
    [Serializable]
    public class DotsHalfEdge // This not not represented in a Unity Object
    {
        // Vertex where this half-edge originates from
        public DotsVertex Origin { get; set; }

        // Vertex where this half-edge ends in, aka the origin of the twin
        public DotsVertex Destination => Twin.Origin;

        // The incident face of this vertex if it exists, null otherwise
        private DotsFace _IncidentFace;

        // The face incident to this Half Edge
        public DotsFace IncidentFace { get; set; }

        // The half-edge twin of this half-edge
        public DotsHalfEdge Twin { get; set; }

        // The previous half-edge in the cycle of the incident face
        public DotsHalfEdge Prev { get; set; }

        // The next half-edge in the cycle of the incident face
        public DotsHalfEdge Next { get; set; }

        public string Name { get; set; }

        // The player that created this half-edge
        public int Player { get; set; }

        public LineSegment Segment => Origin != null && Destination != null
            ? new LineSegment(Origin.Coordinates, Destination.Coordinates)
            : null;

        // Constructor for this half-edge. Needs an origin vertex, a twin half-edge and optionally a previous/next half-edge
        public DotsHalfEdge Constructor(
            int player,
            DotsVertex origin,
            DotsHalfEdge twin,
            DotsHalfEdge prev = null,
            DotsHalfEdge next = null,
            string name = null)
        {
            Player = player;
            Origin = origin;
            Twin = twin;
            Prev = prev;
            Next = next;
            Name = name;

            return this;
        }

        public override string ToString() => $"[{Origin.Coordinates} -> {Destination?.Coordinates}]";
        public DotsHalfEdge Clone() => new DotsHalfEdge().Constructor(Player, Origin, Twin, Prev, Next, Name);
    }
}