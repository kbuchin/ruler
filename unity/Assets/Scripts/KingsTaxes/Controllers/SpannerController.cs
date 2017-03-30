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
    class SpannerController : KingsTaxesController
    {
        [SerializeField]
        private float m_t = 1.5f; //The ratio t in t-spanner
        [SerializeField]
        private GameObject m_hintRoadPrefab = null;

        private float m_thresholdscore;
        private float m_bestPlayerScore;
        private float m_highscore;
        private bool m_thresholdscorebeat;
        private string m_scorekey;
        private int m_numberOfHints;

        private Text m_text;
        private DisableButtonContainer m_advanceButton;
        private DisableButtonContainer m_hintButton;
        private List<GameObject> m_hintRoads;

        protected override void Awake()
        {
            base.Awake();

        }

        protected override void Start()
        {
            base.Start();

            //references
            m_text = GameObject.FindGameObjectWithTag(Tags.ScoreText).GetComponent<Text>();
            m_advanceButton = GameObject.FindGameObjectWithTag(Tags.AdvanceButtonContainer).GetComponent<DisableButtonContainer>();
            m_hintButton = GameObject.FindGameObjectWithTag(Tags.HintButtonContainer).GetComponent<DisableButtonContainer>(); 

            //variables
            m_bestPlayerScore = float.PositiveInfinity;
            m_thresholdscorebeat = false;
            m_scorekey = SceneManager.GetActiveScene().name + "score";
            m_highscore = PlayerPrefs.GetFloat(m_scorekey, float.PositiveInfinity);
            m_numberOfHints = (int) Math.Floor(.1*Math.Pow(m_settlements.Count(), 1.5));
            m_hintRoads = new List<GameObject>();

            //calculate goal
            var greedySpanner = Graph.GreedySpanner(m_settlements.Select<Settlement, Vector2>(go => go.Pos).ToList(), m_t);
            m_thresholdscore = greedySpanner.LengthOfAllEdges() + 0.0001f;

            //init
            UpdateHintButton();
        }

        protected override void Update()
        {
            base.Update();

            if (Input.GetMouseButtonDown(0))
            {
                foreach (GameObject g in m_hintRoads)
                {
                    Destroy(g);
                }
                m_hintRoads.Clear();
            }
        }

        /// <summary>
        /// Updates the text of the textfield
        /// </summary>
        /// <param name="a_spanner"> whether there currently is a valid spanner</param>
        /// <param name="a_tourlength"> The length of a tour. If there is one. Otherwise just provide some value </param>
        private void UpdateTextField(SpannerVerification a_verification, float a_tourlength)
        {
            var text = "";
            text = "Your current graph has ratio " + a_verification.Ratio.ToString("0.##") + " and has length " + a_tourlength.ToString("0.##")+". ";
            if (a_verification.IsSpanner)
            {
                text += "Hence it's a spanner.";
            }
            else
            {
                text += "Hence it's NO spanner.";
            }

            text += "\nThe greedy spanner has length: " + m_thresholdscore.ToString("0.##");
            

            if ( !m_endlessMode)
            {
                text += "\nThe best spanner ever had length: " + m_highscore.ToString("0.##");
            }

            m_text.text = text;
        }

        protected override void CheckVictory()
        {
            var spannerVerifier = m_graph.VerifySpanner(m_t);
            float score = m_graph.LengthOfAllEdges();
            if (spannerVerifier.IsSpanner)
            {
                if (score < m_bestPlayerScore)
                {
                    m_bestPlayerScore = score;
                }
                //victory is also achived by equaling the greedy spanner
                if (score <= m_thresholdscore)
                {
                    m_thresholdscorebeat = true;
                }
                else
                {
                    m_thresholdscorebeat = false;
                }
                if (score < m_highscore)
                {
                    m_highscore = score;
                    PlayerPrefs.SetFloat(m_scorekey, m_highscore);
                }

                if (m_thresholdscorebeat)
                {
                    EnableAdvanceButton();
                }
                else
                {
                    DisableBothButtons();
                }
            }
            else
            {
                UpdateHintButton();
            }

            UpdateTextField(spannerVerifier, score);

        }

        public void showHint()
        {
            if (m_numberOfHints <= 0)
            {
                return;
            }
            var spannerVerifier = m_graph.VerifySpanner(m_t);
            if (spannerVerifier.IsSpanner)
            {
                throw new Exception("Hint button could be clicked while no hint is available");
            } else
            {
                for (int i = 0; i < spannerVerifier.FalsificationPathsStart.Count; i++)
                {
                    displayHintRoad(spannerVerifier.FalsificationPathsStart[i], spannerVerifier.FalsificationPathsEnd[i]);
                }
                m_numberOfHints -= 1;
                UpdateHintButton();
            }
        }

        private void EnableAdvanceButton()
        {
            m_hintButton.Disable();
            m_advanceButton.Enable();
        }

        private void UpdateHintButton()
        {
            if(m_numberOfHints > 0)
            {
                m_hintButton.setText("Hint (" + m_numberOfHints + ")");
                m_hintButton.Enable();

            } else
            {
                m_hintButton.Disable();
            }
            m_advanceButton.Disable();
        }

        private void DisableBothButtons()
        {
            m_hintButton.Disable();
            m_advanceButton.Disable();
        }


        private void displayHintRoad(Vertex a_start, Vertex a_end)
        {
            m_hintRoads.Add(Instantiate(m_hintRoadPrefab, Vector3.forward, Quaternion.identity) as GameObject);
            var roadmeshScript = m_hintRoads.Last().GetComponent<ReshapingMesh>();
            roadmeshScript.CreateNewMesh(a_start.Pos, a_end.Pos);
        }

        protected override List<Vector2> InitEndlessLevel(int level, float width, float height)
        {
            return RandomPos(level + 3, width, height);
        }
    }

}
