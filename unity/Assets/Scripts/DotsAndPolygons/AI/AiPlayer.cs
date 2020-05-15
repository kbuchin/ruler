using DotsAndPolygons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.DotsAndPolygons.AI
{
    public class AiPlayer : DotsPlayer
    {
        public AiPlayer(PlayerNumber player, PlayerType type, HelperFunctions.GameMode mode) : base(player, type, mode)
        {
        }
    }
}
