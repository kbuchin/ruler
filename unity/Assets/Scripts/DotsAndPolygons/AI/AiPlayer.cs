using System.Collections.Generic;

namespace DotsAndPolygons
{
    public abstract class AiPlayer : DotsPlayer
    {
        public AiPlayer(PlayerNumber player, PlayerType type, HelperFunctions.GameMode mode) : base(player, type, mode)
        {
        }

        public abstract (IDotsVertex, IDotsVertex) NextMove(
            HashSet<IDotsEdge> dotsEdges,
            HashSet<IDotsHalfEdge> dotsHalfEdges,
            HashSet<IDotsFace> faces, 
            IEnumerable<IDotsVertex> vertices);
    }
}
