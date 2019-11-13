namespace Voronoi
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Manages the GUI for the Voronoi game.
    /// Displays different panels based on turns and updates score text fields.
    /// </summary>
    public class VoronoiGUIManager : MonoBehaviour
    {
        // panels to activate/deactivate
        public GameObject m_StartPanel;
        public GameObject m_RedPanel;
        public GameObject m_BluePanel;

        // text fields for displaying scores
        public Text m_BlueScoreText;
        public Text m_RedScoreText;
        public Text m_BlueText;

        /// <summary>
        /// Called when game has just been started.
        /// </summary>
        public void OnStartClicked()
        {
            m_StartPanel.SetActive(false);
        }

        /// <summary>
        /// Called when a new turn has started.
        /// </summary>
        /// <param name="m_blueStart"></param>
        public void OnTurnStart(bool m_blueStart)
        {
            m_BluePanel.SetActive(m_blueStart);
            m_RedPanel.SetActive(!m_blueStart);
        }

        /// <summary>
        /// Called when last move has been played.
        /// </summary>
        public void OnLastMove()
        {
            m_RedPanel.SetActive(false);
            m_BluePanel.SetActive(true);
            m_BlueText.text = "Game Over";
        }

        /// <summary>
        /// Sets the text fields with percentages of the given areas.
        /// </summary>
        /// <param name="a_Player1Area"></param>
        /// <param name="a_Player2Area"></param>
        public void SetPlayerAreaOwned(float a_Player1Area, float a_Player2Area)
        {
            // find player's percentage of area owned
            float totalArea = a_Player1Area + a_Player2Area;
            int player1Percentage = Mathf.RoundToInt((a_Player1Area / totalArea) * 100);
            int player2Percentage = Mathf.RoundToInt((a_Player2Area / totalArea) * 100);

            // update text field with percentages
            m_BlueScoreText.text = string.Format("{0}%", player1Percentage);
            m_RedScoreText.text = string.Format("{0}%", player2Percentage);
        }
    }
}
