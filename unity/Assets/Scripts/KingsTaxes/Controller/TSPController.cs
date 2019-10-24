namespace KingsTaxes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using Util.Geometry.Graph;
    using Util.Algorithms.Graph;

    class TSPController : KingsTaxesController
    {

        [SerializeField]
        private Text m_scoreText;

        private float m_thresholdscore;
        private float m_bestPlayerScore;
        private float m_highscore;
        private string m_scorekey;
        
        public TSPController()
        {
            m_endlessScoreKey = "tsp_score";
            m_beatKey = "tsp_beat";
        }

        public override void Awake()
        {
            base.Awake();
        }

        public override void FinishLevelSetup()
        {
            m_bestPlayerScore = float.PositiveInfinity;
            m_thresholdscore = TSP.FindTSPLength(m_graph) + 0.0001f;
            m_scorekey = SceneManager.GetActiveScene().name + "score" + m_levelCounter;
            m_highscore = PlayerPrefs.GetFloat(m_scorekey, float.PositiveInfinity);

            UpdateTextField(false, -1);
        }

        public override void CheckSolution()
        {
            var tour = TSP.IsHamiltonian(m_graph);
            var score = -1f;
            if (tour)
            {
                score = m_graph.TotalEdgeWeight;
                if (score< m_bestPlayerScore)
                {
                    m_bestPlayerScore = score;
                }
                if (score < m_thresholdscore)
                {
                    m_advanceButton.Enable();
                }
                if (score < m_highscore)
                {
                    m_highscore = score;
                    PlayerPrefs.SetFloat(m_scorekey, m_highscore);
                }

                    Debug.Log("We have a tour of " + m_graph.TotalEdgeWeight);
            }
            UpdateTextField(tour, score);
        }

        public override List<Vector2> InitEndlessLevel(int level, float width, float height)
        {
            return RandomPos(level + 3, width, height);
        }

        /// <summary>
        /// Updates the text of the textfield
        /// </summary>
        /// <param name="a_tsptour"> whether there currently is a TSP tour</param>
        /// <param name="a_tourlength"> The length of a tour. If there is one. Otherwise just provide some value </param>
        private void UpdateTextField(bool a_tsptour, float a_tourlength)
        {
            string text;
            if (a_tsptour)
            {
                text = "The current tour has length: " + a_tourlength.ToString("0.##");
            }
            else
            {
                text = "Your best tour so far had length: " + m_bestPlayerScore.ToString("0.##");
            }

            text += "\nChristofides computed length: " + m_thresholdscore.ToString("0.##");

            if (!m_endlessMode)
            {
                text += "\nThe best score achieved in this level is: " + m_highscore.ToString("0.##");
            }

            m_scoreText.text = text;
        }
    }
}
