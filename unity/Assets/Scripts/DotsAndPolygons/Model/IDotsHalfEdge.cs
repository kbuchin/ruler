using JetBrains.Annotations;
using Util.Geometry;

namespace DotsAndPolygons
{
    public interface IDotsHalfEdge
    {
        // Vertex where this half-edge originates from
        IDotsVertex Origin { get; set; }

        string Name { get; set; }

        // Vertex where this half-edge ends in, aka the origin of the twin
        IDotsVertex Destination { get; }

        // The face incident to this Half Edge
        IDotsFace IncidentFace { set; get; }

        // The half-edge twin of this half-edge
        IDotsHalfEdge Twin { get; set; }

        // The previous half-edge in the cycle of the incident face
        IDotsHalfEdge Prev { get; set; }

        // The next half-edge in the cycle of the incident face
        IDotsHalfEdge Next { get; set; }

        // The player that created this half-edge
        int Player { get; set; }

        LineSegment Segment { get; }

        DotsController GameController { get; set; }

        IDotsHalfEdge Constructor(
            [CanBeNull] DotsController mGameController,
            int player,
            IDotsVertex origin,
            IDotsHalfEdge twin,
            IDotsHalfEdge prev = null,
            IDotsHalfEdge next = null,
            string name = null);

        string ToString();
    }
}