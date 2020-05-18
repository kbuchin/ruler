using Util.Geometry;

namespace DotsAndPolygons
{
    // Half-edge
    public class DotsHalfEdge : IDotsHalfEdge // This not not represented in a Unity Object
    {
        // Vertex where this half-edge originates from
        public IDotsVertex Origin { get; set; }

        // Vertex where this half-edge ends in, aka the origin of the twin
        public IDotsVertex Destination => Twin.Origin;

        // The incident face of this vertex if it exists, null otherwise
        private IDotsFace _IncidentFace;

        // The face incident to this Half Edge
        public IDotsFace IncidentFace { get; set; }

        // The half-edge twin of this half-edge
        public IDotsHalfEdge Twin { get; set; }

        // The previous half-edge in the cycle of the incident face
        public IDotsHalfEdge Prev { get; set; }

        // The next half-edge in the cycle of the incident face
        public IDotsHalfEdge Next { get; set; }

        public string Name { get; set; }

        // The player that created this half-edge
        public int Player { get; set; }

        // Reference to the main controller class of the game
        public DotsController GameController { get; set; }

        public LineSegment Segment => Origin != null && Destination != null
            ? new LineSegment(Origin.Coordinates, Destination.Coordinates)
            : null;

        // Constructor for this half-edge. Needs an origin vertex, a twin half-edge and optionally a previous/next half-edge
        public IDotsHalfEdge Constructor(
            DotsController mGameController,
            int player,
            IDotsVertex origin,
            IDotsHalfEdge twin,
            IDotsHalfEdge prev = null,
            IDotsHalfEdge next = null,
            string name = null)
        {
            Player = player;
            Origin = origin;
            Twin = twin;
            Prev = prev;
            Next = next;
            Name = name;
            GameController = mGameController;

            return this;
        }

        public override string ToString() => $"[{Origin.Coordinates} -> {Destination?.Coordinates}]";
        public IDotsHalfEdge Clone() => new DotsHalfEdge().Constructor(GameController, Player, Origin, Twin, Prev, Next, Name);
    }
}