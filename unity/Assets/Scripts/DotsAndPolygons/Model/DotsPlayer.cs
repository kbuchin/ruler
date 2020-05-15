using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util.Algorithms.Triangulation;
using Util.Geometry.Polygon;
using Util.Geometry.Triangulation;

namespace DotsAndPolygons
{
    public class DotsPlayer
    {
        public float TotalArea { get; set; }
        public PlayerType PlayerType { get; private set; }
        public PlayerNumber PlayerNumber { get; private set; }
        public HelperFunctions.GameMode GameMode { get; private set; }

        public DotsPlayer(PlayerNumber player, PlayerType type, HelperFunctions.GameMode gameMode)
        {
            this.PlayerNumber = player;
            this.PlayerType = type;
            this.GameMode = gameMode;
        }
    }
}