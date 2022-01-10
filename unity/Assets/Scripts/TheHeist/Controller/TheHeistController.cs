namespace TheHeist
{
    using General.Controller;
    using General.Menu;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using Util.Algorithms.Polygon;
    using Util.Geometry.Polygon;
    using Util.Math;
    using UnityEngine.Experimental.UIElements;

    /// <summary>
    /// Main controller for the art gallery game.
    /// Handles the game update loop, as well as level initialization and advancement.
    /// </summary>
    public class TheHeistController : MonoBehaviour, IController
    {
        [SerializeField]
        private List<TheHeistLevel> m_levels;

        [SerializeField]
        private string m_victoryScreen = "thVictory";

        [SerializeField]
        private GameObject m_playerPrefab;

        [SerializeField]
        private GameObject m_guardPrefab;

        [SerializeField]
        private GameObject m_debugPrefab;

        [SerializeField]
        private ButtonContainer m_advanceButton;

        [SerializeField]
        private Text m_lighthouseText;

        [SerializeField]
        private Text m_guardText;

        [SerializeField]
        private GameObject m_timeLabel;

        [SerializeField]
        private GameObject m_puzzleCounterLabel;

        // list of game objects instantiated, for removal
        protected List<GameObject> instantObjects = new List<GameObject>();

        // stores the current level index
        private int m_levelCounter = -1;

        // specified max number of guards in level
        private int m_maxNumberOfGuards;


        private int m_maxNumberOfLighthouses;

        // store starting time of level
        private float puzzleStartTime;

        // store relevant art gallery objects
        private TheHeistSolution m_solution;
        private TheHeistIsland m_levelMesh;
        private TheHeistLightHouse m_SelectedLighthouse;

        protected List<TheHeistGuard> m_guards = new List<TheHeistGuard>();
        protected TheHeistPlayer m_playerScript;
        protected float timer = 0;
        protected float delay = 0.75f;
        protected string state = "player";
        protected Vector3 playerStart;
        protected List<Vector3> guardStarts = new List<Vector3>();
        float t = 0;
        float speed = 1;
        int i = 0;
        // x and y for player input
        float x = 0;
        float y = 0;
        bool inputCheck = true;


        private TheHeistGuard m_SelectedGuard;

        private GameObject debugpoint;
        public Polygon2DWithHoles LevelPolygon { get; private set; }

        // Use this for initialization
        void Start()
        {
            m_solution = ScriptableObject.CreateInstance<TheHeistSolution>();
            m_levelMesh = GetComponentInChildren<TheHeistIsland>();

            // go to initial island polygon
            AdvanceLevel();
            playerStart = m_playerScript.Pos;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            UpdateTimeText();

            float stepsize = 0.75f;

            // State design pattern for turns
            switch (state)
            {
                case "player":
                    if (inputCheck)
                    {
                        GetInput(stepsize); 
                       
                    } else
                    {
                        if (t >= 1)
                        {
                            foreach (var guard in m_guards)
                            {
                                guardStarts.Add(guard.transform.position);
                            }
                            t = 0;
                            state = "guard";
                        }
                        else
                        {
                            LerpPlayer(stepsize);
                        }
                    }                     
                break;
                case "guard":

                    playerStart = m_playerScript.transform.position;
                    MoveGuards(stepsize);
                    if (t >= 1)
                    {
                        guardStarts.Clear();
                        t = 0;
                        inputCheck = true;
                        state = "player";
                    }
                break;
                case "pause":
                    break;

            }

            //// return if no lighthouse was selected since last update
            //if (m_SelectedLighthouse == null) return;

            //// get current mouseposition
            //var worldlocation = Camera.main.ScreenPointToRay(Input.mousePosition).origin;
            //worldlocation.z = -2f;

            //// move lighthouse to mouse position
            //// will update visibility polygon
            //m_SelectedLighthouse.Pos = worldlocation;

            //// see if lighthouse was released 
            //if (Input.GetMouseButtonUp(0))
            //{
            //    //check whether lighthouse is over the island
            //    if (!LevelPolygon.ContainsInside(m_SelectedLighthouse.Pos))
            //    {
            //        // destroy the lighthouse
            //        m_solution.RemoveLighthouse(m_SelectedGuard);
            //        Destroy(m_SelectedLighthouse.gameObject);
            //        UpdateLighthouseText();
            //    }

            //    // lighthouse no longer selected
            //    m_SelectedLighthouse = null;

            //    CheckSolution();
            //}
        }
        
        public void MoveGuards(float stepsize)
        {
            int i = 0;
            t += Time.deltaTime;
            //float distCovered = (Time.time - t) * 0.1f;
            foreach (var guard in m_guards)
            {
                if (CheckLegal(guardStarts[i] + new Vector3(0, stepsize, 0f)))
                {
                    guard.Pos = Vector3.Lerp(guardStarts[i], guardStarts[i] + new Vector3(0, stepsize, 0f), t * speed);
                }
                else if (CheckLegal(guardStarts[i] + new Vector3(-stepsize, 0f, 0f)))
                {
                    guard.Pos = Vector3.Lerp(guardStarts[i], guardStarts[i] + new Vector3(-stepsize, 0f, 0f), t * speed);
                }
                i++;
            }
        }

        /// <summary>
        /// Handle keyboard input.
        /// </summary> 
        public void GetInput(float stepsize)
        {
            if (Input.GetKey(KeyCode.A) && CheckLegal(playerStart + new Vector3(-stepsize, 0f, 0f)))
            {
                inputCheck = false;
                //m_playerScript.Pos += new Vector3(-stepsize, 0f, 0f);
                x = -stepsize;
                y = 0;
            }
            if (Input.GetKey(KeyCode.D) && CheckLegal(playerStart + new Vector3(stepsize, 0f, 0f)))
            {
                inputCheck = false;
                x = stepsize;
                y = 0;
            }

            if (Input.GetKey(KeyCode.W) && CheckLegal(playerStart + new Vector3(0f, stepsize, 0f)))
            {
                inputCheck = false;
                y = stepsize;
                x = 0;
            }
            if (Input.GetKey(KeyCode.S) && CheckLegal(playerStart + new Vector3(0f, -stepsize, 0f)))
            {
                inputCheck = false;
                y = -stepsize;
                x = 0;
            }

            //print("playerpos: " + m_playerScript.Pos);
        }

        public void LerpPlayer(float stepsize)
        {
            t += Time.deltaTime;
            if (CheckLegal(playerStart + new Vector3(x*stepsize, y*stepsize, 0f))) {
                //print("LerpPlayer start:"+ playerStart );
                m_playerScript.Pos = Vector3.Lerp(playerStart, playerStart + new Vector3(x, y, 0f), t * speed);
            }
        }

        public void InitLevel()
        {
            // clear old level
            m_solution.Clear();
            m_SelectedLighthouse = null;
            m_advanceButton.Disable();

            // create new level
            var level = m_levels[m_levelCounter];
            LevelPolygon = level.Polygon;
            m_maxNumberOfLighthouses = level.MaxNumberOfLighthouses;
            m_levelMesh.Polygon = LevelPolygon;


            // initialize guards
            foreach (var guard in m_levels[m_levelCounter].Guards)
            {
                //print("guards: " + guard);
                var obj = Instantiate(m_guardPrefab, new Vector3(guard.x, guard.y, -2f), Quaternion.identity);
                m_guards.Add(obj.GetComponent<TheHeistGuard>());
                m_solution.AddGuard(obj);
            }
            //print(m_levels[m_levelCounter].Player.Count);
            foreach (var player in m_levels[m_levelCounter].Player)
            {
                var playerobj = Instantiate(m_playerPrefab, new Vector3(player.x, player.y, -2f), Quaternion.identity);
                m_playerScript = playerobj.GetComponent<TheHeistPlayer>();
                m_solution.AddPlayer(playerobj);
                
            }
            ////initialize one guard
            //var obje = Instantiate(m_guardPrefab, new Vector3(3.5f, -0.85f, -2f) , Quaternion.identity) as GameObject;
            //m_solution.AddGuard(obje);

            // update text box
            UpdateLighthouseText();
        }

        public bool CheckContainmentPoints()
        {
            Destroy(debugpoint);
            var level = m_levels[m_levelCounter];
            foreach (var point in level.CheckPoints)
            {
                var found = false;
                foreach (var guard in m_solution.m_guards)
                {
                    if (guard.VisionPoly.Contains(point))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    var placement = new Vector3(point.x, point.y, -2f);
                    debugpoint = Instantiate(m_debugPrefab, placement, Quaternion.identity);
                    return false;
                }
            }

            return true;
        }

        /**
         * TODO: replace this by a check if the player is in vision of the guard.
         * */
        public void CheckSolution()
        {
            if (m_levels[m_levelCounter].MaxNumberOfLighthouses != m_solution.Count || !CheckContainmentPoints()) return;

            // calculate ratio of area visible
            var ratio = m_solution.Area / LevelPolygon.Area;

            Debug.Log(ratio + " part is visible");

            // see if entire polygon is covered
            // only check if no solution yet found
            if (MathUtil.GEQEps(ratio, 1f, 0.001f * m_levels[m_levelCounter].MaxNumberOfLighthouses))
            {
                m_advanceButton.Enable();
            }
        }

        /// <summary>
        /// Advances to the next level
        /// </summary>
        public void AdvanceLevel()
        {
            m_levelCounter++;

            if (m_levelCounter < m_levels.Count)
            {
                UpdatePuzzleCounter();
                InitLevel();
            }
            else
            {
                SceneManager.LoadScene(m_victoryScreen);
            }

            puzzleStartTime = Time.time;
        }

        public bool CheckLegal(Vector3 input)
        {
            return LevelPolygon.ContainsInside(input);
        }
        /// <summary>
        /// Handle a click on the island mesh.
        /// </summary>
        public void HandleIslandClick()
        {
            //// return if lighthouse was already selected or player can place no more lighthouses
            //if (m_SelectedLighthouse != null || m_solution.Count >= m_maxNumberOfLighthouses)
            //    return;

            //// obtain mouse position
            //var worldlocation = Camera.main.ScreenPointToRay(Input.mousePosition).origin;
            //worldlocation.z = -2f;

            //// create a new lighthouse from prefab
            //var go = Instantiate(m_LighthousePrefab, worldlocation, Quaternion.identity) as GameObject;

            //// add lighthouse to art gallery solution
            //m_solution.PlacePlayer(go);
            //UpdateLighthouseText();

            //CheckSolution();
        }

        /// <summary>
        /// Update the vision polygon for the given lighthouse. TODO: guard, also remove from update loop only do this after 'end turn'
        /// Calculates the visibility polygon.
        /// </summary>
        /// <param name="m_guard"></param>
        public void UpdateVision(TheHeistGuard m_guard)
        {

            if (LevelPolygon.ContainsInside(m_guard.Pos))
            {
                // calculate new visibility polygon
                var vision = VisibilitySweep.Vision(LevelPolygon, m_guard.Pos);

                if (vision == null)
                {
                    throw new Exception("Vision polygon cannot be null");
                }

                // update lighthouse visibility
                m_guard.VisionPoly = vision;
                m_guard.VisionAreaMesh.Polygon = new Polygon2DWithHoles(vision);
            }
            else
            {
                // remove visibility polygon from lighthouse
                m_guard.VisionPoly = null;
                m_guard.VisionAreaMesh.Polygon = null;
            }
        }

        /// <summary>
        /// Set given lighthouse as selected one
        /// </summary>
        /// <param name="a_select"></param>
        internal void SelectLighthouse(TheHeistLightHouse a_select)
        {
            m_SelectedLighthouse = a_select;
        }

        /// <summary>
        /// Set given guard as selected one
        /// </summary>
        /// <param name="a_select"></param>
        internal void SelectGuard(TheHeistGuard a_select)
        {
            m_SelectedGuard = a_select;
        }

        /// <summary>
        /// Update the text field with max number of lighthouses which can still be placed
        /// </summary>
        private void UpdateLighthouseText()
        {
            m_lighthouseText.text = "Torches left: " + (m_maxNumberOfLighthouses - m_solution.Count);
        }

        /// <summary>
        /// Update the text field with max number of lighthouses which can still be placed
        /// </summary>
        private void UpdateTimeText()
        {
            m_timeLabel.GetComponentInChildren<Text>().text = string.Format("Time: {0:0.}s", Time.time - puzzleStartTime);
        }

        /// <summary>
        /// Updates the label with puzzle counter
        /// </summary>
        private void UpdatePuzzleCounter()
        {
            m_puzzleCounterLabel.GetComponentInChildren<Text>().text = string.Format("Puzzle {0} / {1}", m_levelCounter + 1, m_levels.Count);
        }
    }
}