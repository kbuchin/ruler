using DotsAndPolygons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotsAndPolygons
{
    public abstract class AiPlayer : DotsPlayer
    {
        public AiPlayer(PlayerNumber player, PlayerType type, HelperFunctions.GameMode mode) : base(player, type, mode)
        {
        }

        public abstract (IDotsVertex, IDotsVertex) NextMove(IEnumerable<IDotsEdge> edges,
            IEnumerable<IDotsHalfEdge> halfEdges,
            HashSet<IDotsFace> faces, IEnumerable<IDotsVertex> vertices);
    }
}
