namespace KingsTaxes
{
    using Util.Geometry.Graph;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using Util.Algorithms.Graph;
    using General.Model;

    class SpannerController : KingsTaxesController
    {
        [SerializeField]
        private GameObject m_hintRoadPrefab = null;
        [SerializeField]
        private Text m_scoreText;
        [SerializeField]
        private ButtonContainer m_hintButton;

        private float m_thresholdscore;
        private float m_bestPlayerScore;
        private float m_highscore;
        private bool m_thresholdscorebeat;
        private string m_scorekey;
        private int m_numberOfHints;

        private List<GameObject> m_hintRoads = new List<GameObject>();

        public SpannerController()
        {
            m_endlessScoreKey = "spannerscore";
        }

        public override void Awake()
        {
            base.Awake();

        }
        public override void Start()
        {
            base.Start();
        }

        public override void Update()
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

        public override void FinishLevelSetup()
        {
            // set variabls
            m_bestPlayerScore = float.PositiveInfinity;
            m_thresholdscorebeat = false;

            if (!m_endlessMode)
            {
                m_scorekey = SceneManager.GetActiveScene().name + "score" + m_levelCounter;
                m_highscore = PlayerPrefs.GetFloat(m_scorekey, float.PositiveInfinity);
            }
            m_numberOfHints = (int)Math.Floor(.5 * Math.Sqrt(m_settlements.Count()));

            //calculate goal
            var greedySpanner = Spanner.GreedySpanner(
                m_settlements.Select<Settlement, Vertex>(go => new Vertex(go.Pos)).ToList(),
                m_t
            );
            m_thresholdscore = greedySpanner.TotalEdgeWeight + 0.0001f;

            //init
            UpdateHintButton();

            m_scoreText.text = "Information on your graph is displayed here.\nGoal: Find a " + m_t + "-spanner";
        }

        /// <summary>
        /// Updates the text of the textfield
        /// </summary>
        /// <param name="a_spanner"> whether there currently is a valid spanner</param>
        /// <param name="a_tourlength"> The length of a tour. If there is one. Otherwise just provide some value </param>
        private void UpdateTextField(SpannerVerification a_verification, float a_tourlength)
        {
            string text;
            text = "Your current graph has ratio " + a_verification.Ratio.ToString("0.##") + " and has length " + a_tourlength.ToString("0.##")+".\n";
            if (a_verification.IsSpanner)
            {
                text += "Hence it's a " + m_t.ToString("0.##") + "-spanner.\n";
            }
            else
            {
                text += "Hence it's NO " + m_t.ToString("0.##") + "-spanner.\n";
            }

            text += "The greedy 1.5-spanner has length: " + m_thresholdscore.ToString("0.##") + "\n";



            if ( !m_endlessMode)
            {
                text += "The shorthest 1.5-spanner on this instance had length: " + m_highscore.ToString("0.##");
            }

            m_scoreText.text = text;
        }

        public override void CheckSolution()
        {
            var spannerVerifier = Spanner.VerifySpanner(m_graph, m_t);
            float score = m_graph.TotalEdgeWeight;

            Debug.Log("check");

            if (spannerVerifier.IsSpanner)
            {
                Debug.Log("correct");
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

        public void ShowHint()
        {
            if (m_numberOfHints <= 0)
            {
                return;
            }
            var spannerVerifier = Spanner.VerifySpanner(m_graph, m_t);
            if (spannerVerifier.IsSpanner)
            {
                throw new Exception("Hint button could be clicked while no hint is available");
            } else
            {
                DisplayHintRoad(spannerVerifier.FalsificationStart, spannerVerifier.FalsificationEnd);
                m_numberOfHints -= 1;
                UpdateHintButton();
            }
        }

        private void EnableAdvanceButton()
        {
            Debug.Log("advance button");
            m_hintButton.Disable();
            m_advanceButton.Enable();
        }

        private void UpdateHintButton()
        {
            if(m_numberOfHints > 0)
            {
                m_hintButton.SetText("Hint (" + m_numberOfHints + ")");
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

        private void DisplayHintRoad(Vertex a_start, Vertex a_end)
        {
            var hint = Instantiate(m_hintRoadPrefab, Vector3.forward, Quaternion.identity) as GameObject;
            hint.transform.parent = this.transform;
            m_hintRoads.Add(hint);
            var roadmeshScript = m_hintRoads.Last().GetComponent<ReshapingMesh>();
            roadmeshScript.CreateNewMesh(a_start.Pos, a_end.Pos);
        }

        public override List<Vector2> InitEndlessLevel(int level, float width, float height)
        {
            m_t = 1.5f;
            return RandomPos(level + 3, width, height);
        }

        public new void Clear()
        {
            base.Clear();

            foreach (var obj in m_hintRoads) Destroy(obj);

            m_hintRoads.Clear();
        }
    }
}
