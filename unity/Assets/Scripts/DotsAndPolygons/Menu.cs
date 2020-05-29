using System;
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
            Settings.Player1 = CyclePlayerType(Settings.Player1);
            UpdateText();
        }

        public void TogglePlayer2()
        {
            Settings.Player2 = CyclePlayerType(Settings.Player2);
            UpdateText();
        }
    }
}