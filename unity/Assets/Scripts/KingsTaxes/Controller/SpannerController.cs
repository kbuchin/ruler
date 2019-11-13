namespace KingsTaxes
{
    using General.Menu;
    using General.Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using Util.Algorithms.Graph;
    using Util.Geometry.Graph;

    /// <summary>
    /// Game controller for the t-spanner minigame in the Kings Taxes game.
    /// </summary>
    class SpannerController : KingsTaxesController
    {
        [SerializeField]
        private GameObject m_hintRoadPrefab = null;
        [SerializeField]
        private Text m_scoreText;
        [SerializeField]
        private ButtonContainer m_hintButton;

        // score variables
        private float m_thresholdscore;
        private float m_bestPlayerScore;
        private float m_highscore;
        private bool m_thresholdscorebeat;
        private string m_scorekey;
        private int m_numberOfHints;

        // list of hint road objects instantiated
        private List<GameObject> m_hintRoads = new List<GameObject>();

        public SpannerController()
        {
            // set player prefs keys
            m_endlessScoreKey = "spanner_score";
            m_beatKey = "spanner_beat";
        }

        public override void Update()
        {
            base.Update();

            // check for clicks to destroy all hints
            if (Input.GetMouseButtonDown(0))
            {
                foreach (var g in m_hintRoads)
                {
                    Destroy(g);
                }
                m_hintRoads.Clear();
            }
        }

        protected override void FinishLevelSetup()
        {
            // set variables
            m_bestPlayerScore = float.PositiveInfinity;
            m_thresholdscorebeat = false;

            if (!m_endlessMode)
            {
                // set score key for current level
                m_scorekey = SceneManager.GetActiveScene().name + "_score_" + m_levelCounter;
                m_highscore = PlayerPrefs.GetFloat(m_scorekey, float.PositiveInfinity);
            }
            m_numberOfHints = (int)Math.Floor(.5 * Math.Sqrt(m_settlements.Count()));

            //calculate goal
            var greedySpanner = Spanner.GreedySpanner(
                m_settlements.Select(go => new Vertex(go.Pos)).ToList(),
                m_t
            );
            m_thresholdscore = greedySpanner.TotalEdgeWeight + 0.0001f;

            UpdateHintButton();

            m_scoreText.text = "Information on your graph is displayed here.\nGoal: Find a " + m_t + "-spanner";
        }


        protected override List<Vector2> InitEndlessLevel(int level, float width, float height)
        {
            m_t = 1.5f;
            return RandomPos(level + 3, width, height);
        }

        public override void CheckSolution()
        {
            // verify if graph is t-spanner
            var spannerVerifier = Spanner.VerifySpanner(m_graph, m_t);
            float score = m_graph.TotalEdgeWeight;

            if (spannerVerifier.IsSpanner)
            {
                // update best score
                if (score < m_bestPlayerScore)
                {
                    m_bestPlayerScore = score;
                }

                //victory is achieved by equaling the greedy spanner
                if (score <= m_thresholdscore)
                {
                    m_thresholdscorebeat = true;
                }
                else
                {
                    m_thresholdscorebeat = false;
                }

                // update all-time high score
                if (score < m_highscore)
                {
                    m_highscore = score;
                    PlayerPrefs.SetFloat(m_scorekey, m_highscore);
                }

                // enable or disable buttons
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
                // enable hints if still available
                UpdateHintButton();
            }

            UpdateTextField(spannerVerifier, score);
        }

        public new void Clear()
        {
            base.Clear();

            foreach (var obj in m_hintRoads) Destroy(obj);

            m_hintRoads.Clear();
        }

        /// <summary>
        /// Gives a road as a hint that still makes graph an invalid t-spanner.
        /// </summary>
        public void ShowHint()
        {
            // no more hints allowed
            if (m_numberOfHints <= 0)
            {
                return;
            }

            // check if graph spanner
            var spannerVerifier = Spanner.VerifySpanner(m_graph, m_t);
            if (spannerVerifier.IsSpanner)
            {
                throw new Exception("Hint button could be clicked while no hint is available");
            }

            // display the falsification road
            DisplayHintRoad(spannerVerifier.FalsificationStart, spannerVerifier.FalsificationEnd);

            m_numberOfHints -= 1;

            UpdateHintButton();
        }

        /// <summary>
        /// Allow user to advance to next level.
        /// </summary>
        private void EnableAdvanceButton()
        {
            m_hintButton.Disable();
            m_advanceButton.Enable();
        }

        /// <summary>
        /// Update the text on hint button and enable/disable.
        /// </summary>
        private void UpdateHintButton()
        {
            if (m_numberOfHints > 0)
            {
                m_hintButton.SetText("Hint (" + m_numberOfHints + ")");
                m_hintButton.Enable();

            }
            else
            {
                m_hintButton.Disable();
            }
            m_advanceButton.Disable();
        }

        /// <summary>
        /// Disable both hints and advancement
        /// </summary>
        private void DisableBothButtons()
        {
            m_hintButton.Disable();
            m_advanceButton.Disable();
        }

        /// <summary>
        /// Display a hint road between given vertices.
        /// </summary>
        /// <param name="a_start"></param>
        /// <param name="a_end"></param>
        private void DisplayHintRoad(Vertex a_start, Vertex a_end)
        {
            // instantiate hint road
            var hint = Instantiate(m_hintRoadPrefab, Vector3.forward, Quaternion.identity) as GameObject;
            hint.transform.parent = this.transform;

            // store the game object
            m_hintRoads.Add(hint);

            // create the road mesh
            var roadmeshScript = m_hintRoads.Last().GetComponent<ReshapingMesh>();
            roadmeshScript.CreateNewMesh(a_start.Pos, a_end.Pos);
        }

        /// <summary>
        /// Updates the text of the textfield
        /// </summary>
        /// <param name="a_spanner"> whether there currently is a valid spanner</param>
        /// <param name="a_tourlength"> The length of a tour. If there is one. Otherwise just provide some value </param>
        private void UpdateTextField(SpannerVerification a_verification, float a_tourlength)
        {
            string text;
            text = "Your current graph has ratio " + a_verification.Ratio.ToString("0.##") + " and has length " + a_tourlength.ToString("0.##") + ".\n";
            if (a_verification.IsSpanner)
            {
                text += "Hence it's a " + m_t.ToString("0.##") + "-spanner.\n";
            }
            else
            {
                text += "Hence it's NO " + m_t.ToString("0.##") + "-spanner.\n";
            }

            text += "The greedy " + m_t.ToString("0.##") + "-spanner has length: " + m_thresholdscore.ToString("0.##") + "\n";



            if (!m_endlessMode)
            {
                text += "The shorthest 1.5-spanner on this instance had length: " +
                    (m_highscore == float.PositiveInfinity ? "Infinity" : m_highscore.ToString("0.##"));
            }

            m_scoreText.text = text;
        }
    }
}
