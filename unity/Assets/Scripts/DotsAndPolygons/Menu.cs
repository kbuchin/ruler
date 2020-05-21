using UnityEngine;
using UnityEngine.UI;

namespace DotsAndPolygons
{
    public class Menu : MonoBehaviour
    {
        [SerializeField] public Text Player1Text;

        [SerializeField] public Text Player2Text;

        public void Start()
        {
            switch (Settings.Player1)
            {
                case PlayerType.GreedyAi:
                    Player1Text.text = "P1: Greedy AI";
                    break;
                case PlayerType.Player:
                    Player1Text.text = "P1: Player";
                    break;
                default:
                    Player1Text.text = "P1: Player";
                    break;

            }

            switch (Settings.Player2)
            {
                case PlayerType.GreedyAi:
                    Player2Text.text = "P2: Greedy AI";
                    break;
                case PlayerType.Player:
                    Player2Text.text = "P2: Player";
                    break;
                default:
                    Player2Text.text = "P2: Player";
                    break;
            }
        }

        public void TogglePlayer1()
        {
            switch (Settings.Player1)
            {
                case PlayerType.Player:
                    Settings.Player1 = PlayerType.GreedyAi;
                    Player1Text.text = "P1: Greedy AI";
                    break;
                case PlayerType.GreedyAi:
                    Settings.Player1 = PlayerType.Player;
                    Player1Text.text = "P1: Player";
                    break;
                default:
                    Settings.Player1 = PlayerType.Player;
                    Player1Text.text = "P1: Player";
                    break;
            }
        }

        public void TogglePlayer2()
        {
            switch (Settings.Player2)
            {
                case PlayerType.Player:
                    Settings.Player2 = PlayerType.GreedyAi;
                    Player2Text.text = "P2: Greedy AI";
                    break;
                case PlayerType.GreedyAi:
                    Settings.Player2 = PlayerType.Player;
                    Player2Text.text = "P2: Player";
                    break;
                default:
                    Settings.Player2 = PlayerType.Player;
                    Player2Text.text = "P2: Player";
                    break;
            }
        }
    }
}