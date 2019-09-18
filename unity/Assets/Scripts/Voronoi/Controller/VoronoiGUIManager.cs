namespace Voronoi.Controller
{
    using UnityEngine;

    public sealed class VoronoiGUIManager : MonoBehaviour {

	    public GameObject m_StartPanel;
	    public GameObject m_RedPanel;
	    public GameObject m_BluePanel;
	    public UnityEngine.UI.Text m_BlueScoreText;
	    public UnityEngine.UI.Text m_RedScoreText;
        public UnityEngine.UI.Text m_BlueText;

        public void OnStartClicked()
	    {
		    m_StartPanel.SetActive(false);
	    }

	    public void OnRedTurnStart()
	    {
		    m_BluePanel.SetActive(false);
		    m_RedPanel.SetActive(true);
	    }

	    public void OnBlueTurnStart()
	    {
		    m_RedPanel.SetActive(false);
		    m_BluePanel.SetActive(true);
	    }

        public void OnLastMove()
        {
            m_RedPanel.SetActive(false);
            m_BluePanel.SetActive(true);
            m_BlueText.text = "Game Over";
        }

        public void SetPlayerAreaOwned(float a_Player1Area, float a_Player2Area)
	    {
		    float totalArea = a_Player1Area + a_Player2Area;
		    int player1Percentage = Mathf.RoundToInt((a_Player1Area / totalArea) * 100);
		    int player2Percentage = Mathf.RoundToInt((a_Player2Area / totalArea) * 100);
		    m_BlueScoreText.text = player1Percentage.ToString() + "%";
		    m_RedScoreText.text = player2Percentage.ToString() + "%";
	    }
    }
}
