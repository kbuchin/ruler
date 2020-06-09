using System.Collections.Generic;

namespace DotsAndPolygons
{
    public abstract class AiPlayer : DotsPlayer
    {
        public AiPlayer(PlayerNumber player, PlayerType type, HelperFunctions.GameMode mode) : base(player, type, mode)
        {
        }

        public abstract List<PotentialMove> NextMove(
            HashSet<DotsEdge> dotsEdges,
            HashSet<DotsHalfEdge> dotsHalfEdges,
            HashSet<DotsFace> faces, 
            HashSet<DotsVertex> vertices);
    }
}
