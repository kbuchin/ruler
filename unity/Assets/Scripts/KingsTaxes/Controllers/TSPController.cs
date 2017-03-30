using Algo.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KingsTaxes
{
    class TSPController : KingsTaxesController
    {
        private float m_thresholdscore;
        private float m_bestPlayerScore;
        private float m_highscore;
        private string m_scorekey;

        private Text m_text;
        private DisableButtonContainer m_advanceButton;
        

        protected override void Awake()
        {
            base.Awake();

        }

        protected override void Start()
        {
            base.Start();
            //references
            m_text = GameObject.FindGameObjectWithTag(Tags.ScoreText).GetComponent<Text>();
            m_advanceButton = FindObjectOfType<DisableButtonContainer>( );

            //variables
            m_bestPlayerScore = float.PositiveInfinity;
            m_thresholdscore = Graph.FindTSPLength(m_graph.Positions) + 0.0001f;
            m_scorekey = SceneManager.GetActiveScene().name + "score";
            m_highscore = PlayerPrefs.GetFloat(m_scorekey, float.PositiveInfinity);

            //init
            UpdateTextField(false, -1);
            m_advanceButton.Disable();

        }

        /// <summary>
        /// Updates the text of the textfield
        /// </summary>
        /// <param name="a_tsptour"> whether there currently is a TSP tour</param>
        /// <param name="a_tourlength"> The length of a tour. If there is one. Otherwise just provide some value </param>
        private void UpdateTextField(bool a_tsptour, float a_tourlength)
        {
            var text = "";
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

            m_text.text = text;
        }


        protected override void CheckVictory()
        {
            var tour = m_graph.IsTSPTour();
            var score = -1f;
            if (tour)
            {
                score = m_graph.LengthOfAllEdges();
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

                    Debug.Log("We have a tour of " + m_graph.LengthOfAllEdges());
            }
            UpdateTextField(tour, score);
        }

        protected override List<Vector2> InitEndlessLevel(int level, float width, float height)
        {
            return RandomPos(level + 3, width, height);
        }
    }
}
