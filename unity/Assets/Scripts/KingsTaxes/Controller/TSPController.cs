namespace KingsTaxes
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using Util.Algorithms.Graph;

    /// <summary>
    /// Game controller for the TSP minigame of the King Taxes game.
    /// </summary>
    class TSPController : KingsTaxesController
    {
        [SerializeField]
        private Text m_scoreText;

        // score variables
        private float m_thresholdscore;
        private float m_bestPlayerScore;
        private float m_highscore;
        private string m_scorekey;

        public TSPController()
        {
            m_endlessScoreKey = "tsp_score";
            m_beatKey = "tsp_beat";
        }

        protected override void FinishLevelSetup()
        {
            // set score variables
            m_bestPlayerScore = float.PositiveInfinity;
            m_thresholdscore = TSP.FindTSPLength(m_graph.Vertices) + 0.0001f;        // use greedy tsp length as threshold
            m_scorekey = SceneManager.GetActiveScene().name + "score" + m_levelCounter;
            m_highscore = PlayerPrefs.GetFloat(m_scorekey, float.PositiveInfinity);

            UpdateTextField(false, -1);
        }

        protected override List<Vector2> InitEndlessLevel(int level, float width, float height)
        {
            return RandomPos(level + 3, width, height);
        }

        public override void CheckSolution()
        {
            var score = -1f;

            // check if tour is hamiltonian, otherwise invalid
            var tour = TSP.IsHamiltonian(m_graph);
            if (tour)
            {
                // calculate TSP score
                score = m_graph.TotalEdgeWeight;

                // update best score by player
                if (score < m_bestPlayerScore)
                {
                    m_bestPlayerScore = score;
                }

                // check if score below theshold
                if (score < m_thresholdscore)
                {
                    m_advanceButton.Enable();
                }

                // update all-time high score
                if (score < m_highscore)
                {
                    m_highscore = score;
                    PlayerPrefs.SetFloat(m_scorekey, m_highscore);
                }
            }

            UpdateTextField(tour, score);
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
                text += "\nThe best score achieved in this level is: " +
                    (m_highscore == float.PositiveInfinity ? "Infinity" : m_highscore.ToString("0.##"));
            }

            m_scoreText.text = text;
        }
    }
}
