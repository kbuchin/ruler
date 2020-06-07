using System.Collections.Generic;

namespace DotsAndPolygons
{
    public abstract class AiPlayer : DotsPlayer
    {
        public AiPlayer(PlayerNumber player, PlayerType type, HelperFunctions.GameMode mode) : base(player, type, mode)
        {
        }

        public abstract (DotsVertex, DotsVertex) NextMove(
            HashSet<DotsEdge> dotsEdges,
            HashSet<DotsHalfEdge> dotsHalfEdges,
            HashSet<DotsFace> faces, 
            IEnumerable<DotsVertex> vertices);
    }
}
