namespace ArtGallery
{
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using KingsTaxes;
    using Util.Math;
    using Util.Geometry.Polygon;
    using Util.Algorithms.Polygon;
    using General.Controller;
    using General.Model;

    public class ArtGalleryController : MonoBehaviour, IController
    {
        [SerializeField]
        private List<ArtGalleryLevel> m_levels;

        [SerializeField]
        private string m_victoryScreen = "agVictory";

        [SerializeField]
        private GameObject m_lighthousePrefab;

        [SerializeField]
        private ButtonContainer m_advanceButton;

        [SerializeField]
        private Text m_lighthouseText;


        private int m_levelCounter = 0;
        
        private readonly float m_eps = MathUtil.EPS * 10;

        private int m_maxNumberOfLighthouses;
        private ArtGallerySolution m_solution;
        private ArtGalleryIsland m_levelMesh;
        private ArtGalleryLightHouse m_selectedLighthouse;

        internal Polygon2D LevelPolygon { get; private set; }

        public void Start()
        {
            m_solution = ScriptableObject.CreateInstance<ArtGallerySolution>();
            m_levelMesh = GetComponentInChildren<ArtGalleryIsland>();
            InitLevel();
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

        internal void SelectLighthouse(ArtGalleryLightHouse a_select)
        {
            m_selectedLighthouse = a_select;
        }

        public void Update()
        {
            if (m_selectedLighthouse == null) return;

            var worldlocation = Camera.main.ScreenPointToRay(Input.mousePosition).origin;
            worldlocation.z = -2f;

            m_selectedLighthouse.Pos = worldlocation;

            if (Input.GetMouseButtonUp(0))
            {
                //check whether lighthouse is over the island
                if (!LevelPolygon.Contains(m_selectedLighthouse.Pos))
                {
                    m_solution.RemoveLighthouse(m_selectedLighthouse);
                    Destroy(m_selectedLighthouse.gameObject);
                    UpdateLighthouseText();
                }

                m_selectedLighthouse = null;
                CheckSolution();
            }
        }

        internal void IslandClick()
        {
            if (m_selectedLighthouse != null || m_solution.Count >= m_maxNumberOfLighthouses)
                return;

            var worldlocation = Camera.main.ScreenPointToRay(Input.mousePosition).origin;
            worldlocation.z = -2f;

            var go = Instantiate(m_lighthousePrefab, worldlocation, Quaternion.identity) as GameObject;

            m_solution.AddLighthouse(go);
            UpdateLighthouseText();

            CheckSolution();
        }

        public void UpdateVision(ArtGalleryLightHouse m_lighthouse)
        {
            if (LevelPolygon.Contains(m_lighthouse.Pos))
            {
                var vision = Visibility.Vision(LevelPolygon, m_lighthouse.Pos);
                m_lighthouse.VisionArea = vision;
                m_lighthouse.m_visionAreaMesh.Polygon = vision;
            }
            else
            {
                m_lighthouse.VisionArea = null;
                m_lighthouse.m_visionAreaMesh.Polygon = null;
            }
        }

        public void CheckSolution()
        {
            var ratio = m_solution.Area / LevelPolygon.Area;
            Debug.Log(ratio + " part is visible");
            if (ratio > 1 - m_eps)
            {
                m_advanceButton.Enable();
            }
        }

        /// <summary>
        /// Advances to the next level
        /// </summary>
        public void AdvanceLevel()
        {
            if (m_levelCounter < m_levels.Count)
            {
                m_levelCounter++;
                InitLevel();
            }
            else
            {
                SceneManager.LoadScene(m_victoryScreen);
            }
        }

        private void UpdateLighthouseText()
        {
            m_lighthouseText.text = "Torches left: " + (m_maxNumberOfLighthouses - m_solution.Count);
        }
    }
}