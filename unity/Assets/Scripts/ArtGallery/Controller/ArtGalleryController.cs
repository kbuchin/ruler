namespace ArtGallery
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

    /// <summary>
    /// Main controller for the art gallery game.
    /// Handles the game update loop, as well as level initialization and advancement.
    /// </summary>
    public class ArtGalleryController : MonoBehaviour, IController
    {
        [SerializeField]
        private List<ArtGalleryLevel> m_levels;

        [SerializeField]
        private string m_victoryScreen = "agVictory";

        [SerializeField]
        private GameObject m_lighthousePrefab;

        [SerializeField]
        private GameObject m_debugPrefab;

        [SerializeField]
        private ButtonContainer m_advanceButton;

        [SerializeField]
        private Text m_lighthouseText;

        [SerializeField]
        private GameObject m_timeLabel;

        [SerializeField]
        private GameObject m_puzzleCounterLabel;

        // stores the current level index
        private int m_levelCounter = -1;

        // specified max number of lighthouses in level
        private int m_maxNumberOfLighthouses;

        // store starting time of level
        private float puzzleStartTime;

        // store relevant art gallery objects
        private ArtGallerySolution m_solution;
        private ArtGalleryIsland m_levelMesh;
        private ArtGalleryLightHouse m_selectedLighthouse;

        private GameObject debugpoint;
        public Polygon2DWithHoles LevelPolygon { get; private set; }

        // Use this for initialization
        void Start()
        {
            m_solution = ScriptableObject.CreateInstance<ArtGallerySolution>();
            m_levelMesh = GetComponentInChildren<ArtGalleryIsland>();

            // go to initial island polygon
            AdvanceLevel();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateTimeText();


            // return if no lighthouse was selected since last update
            if (m_selectedLighthouse == null) return;

            // get current mouseposition
            var worldlocation = Camera.main.ScreenPointToRay(Input.mousePosition).origin;
            worldlocation.z = -2f;

            // move lighthouse to mouse position
            // will update visibility polygon
            m_selectedLighthouse.Pos = worldlocation;

            // see if lighthouse was released 
            if (Input.GetMouseButtonUp(0))
            {
                //check whether lighthouse is over the island
                if (!LevelPolygon.ContainsInside(m_selectedLighthouse.Pos))
                {
                    // destroy the lighthouse
                    m_solution.RemoveLighthouse(m_selectedLighthouse);
                    Destroy(m_selectedLighthouse.gameObject);
                    UpdateLighthouseText();
                }

                // lighthouse no longer selected
                m_selectedLighthouse = null;

                CheckSolution();
            }
        }

        public void InitLevel()
        {
            // clear old level
            m_solution.Clear();
            m_selectedLighthouse = null;
            m_advanceButton.Disable();

            // create new level
            var level = m_levels[m_levelCounter];
            LevelPolygon = level.Polygon;
            m_maxNumberOfLighthouses = level.MaxNumberOfLighthouses;
            m_levelMesh.Polygon = LevelPolygon;

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
                foreach (var lighthouse in m_solution.m_lighthouses)
                {
                    if (lighthouse.VisionPoly.Contains(point))
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


        /// <summary>
        /// Handle a click on the island mesh.
        /// </summary>
        public void HandleIslandClick()
        {
            // return if lighthouse was already selected or player can place no more lighthouses
            if (m_selectedLighthouse != null || m_solution.Count >= m_maxNumberOfLighthouses)
                return;

            // obtain mouse position
            var worldlocation = Camera.main.ScreenPointToRay(Input.mousePosition).origin;
            worldlocation.z = -2f;

            // create a new lighthouse from prefab
            var go = Instantiate(m_lighthousePrefab, worldlocation, Quaternion.identity) as GameObject;

            // add lighthouse to art gallery solution
            m_solution.AddLighthouse(go);
            UpdateLighthouseText();

            CheckSolution();
        }

        /// <summary>
        /// Update the vision polygon for the given lighthouse.
        /// Calculates the visibility polygon.
        /// </summary>
        /// <param name="m_lighthouse"></param>
        public void UpdateVision(ArtGalleryLightHouse m_lighthouse)
        {

            if (LevelPolygon.ContainsInside(m_lighthouse.Pos))
            {
                // calculate new visibility polygon
                var vision = VisibilitySweep.Vision(LevelPolygon, m_lighthouse.Pos);

                if (vision == null)
                {
                    throw new Exception("Vision polygon cannot be null");
                }

                // update lighthouse visibility
                m_lighthouse.VisionPoly = vision;
                m_lighthouse.VisionAreaMesh.Polygon = new Polygon2DWithHoles(vision);
            }
            else
            {
                // remove visibility polygon from lighthouse
                m_lighthouse.VisionPoly = null;
                m_lighthouse.VisionAreaMesh.Polygon = null;
            }
        }

        /// <summary>
        /// Set given lighthouse as selected one
        /// </summary>
        /// <param name="a_select"></param>
        internal void SelectLighthouse(ArtGalleryLightHouse a_select)
        {
            m_selectedLighthouse = a_select;
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