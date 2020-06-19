
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DotsAndPolygons
{
    public class Menu : MonoBehaviour
    {
        [SerializeField] public Text Player1Text;

        [SerializeField] public Text Player2Text;

        private void UpdateText()
        {
            Player1Text.text = $"P1: {Settings.Player1.ValueOf()}";
            Player2Text.text = $"P2: {Settings.Player2.ValueOf()}";
        }

        private static PlayerType CyclePlayerType(PlayerType current)
        {
            switch (current)
            {
                case PlayerType.Player:
                    return PlayerType.GreedyAi;
                case PlayerType.GreedyAi:
                    return PlayerType.MinMaxAi;
                case PlayerType.MinMaxAi:
                    return PlayerType.Player;
                default:
                    return PlayerType.Player;
            }
        }

        public void Start()
        {
            UpdateText();
        }

        public void TogglePlayer1()
        {
            if(Settings.Player2 != PlayerType.Player && !Settings.MultiThreaded)
            {
                Settings.Player1 = PlayerType.Player;
                UpdateText();
            }
            else
            {
                Settings.Player1 = CyclePlayerType(Settings.Player1);
                UpdateText();
            }

        }

        public void TogglePlayer2()
        {
            if (Settings.Player1 != PlayerType.Player && !Settings.MultiThreaded)
            {
                Settings.Player2 = PlayerType.Player;
                UpdateText();
            }
            else
            {
                Settings.Player2 = CyclePlayerType(Settings.Player2);
                UpdateText();
            }
            
        }
    }
}