using UnityEngine;

namespace DotsAndPolygons
{
    public enum PlayerType
    {
        MinMaxAi,
        GreedyAi,
        Player
        // tbd
    }

    public enum PlayerNumber
    {
        Player1 = 1,
        Player2 = 2
    }

    public static class ExtensionMethods
    {
        public static string ValueOf(this PlayerType playerType) {
           switch(playerType)
            {
                case PlayerType.MinMaxAi:
                    return "Min Max AI";
                case PlayerType.GreedyAi:
                    return "Greedy AI";
                case PlayerType.Player:
                    return "Player";
                default:
                    return "Player";
            }
        }

        public static DotsPlayer CreatePlayer(this PlayerType playerType, PlayerNumber playerNumber, HelperFunctions.GameMode gameMode)
        {
            switch(playerType)
            {
                case PlayerType.MinMaxAi:
                    return new MinMaxAi(playerNumber, gameMode);
                case PlayerType.GreedyAi:
                    return new GreedyAi(playerNumber, gameMode);
                case PlayerType.Player:
                    return new DotsPlayer(playerNumber, PlayerType.Player, gameMode);
                default:
                    return new DotsPlayer(playerNumber, PlayerType.Player, gameMode);
            }
        }

        public static PlayerNumber Switch(this PlayerNumber playerNumber)
        {
            return playerNumber == PlayerNumber.Player1 ? PlayerNumber.Player2 : PlayerNumber.Player1;
        }
        
    }
}