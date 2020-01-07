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
    public abstract class AbstractArtGalleryController : MonoBehaviour, IController
    {

        [SerializeField]
        protected List<ArtGalleryLevel> m_levels;

        [SerializeField]
        protected string m_victoryScreen = "agVictory";

        [SerializeField]
        protected GameObject m_lighthousePrefab;

        [SerializeField]
        protected ButtonContainer m_advanceButton;

        [SerializeField]
        protected Text m_lighthouseText;

        // stores the current level index
        protected int m_levelCounter = -1;

        // specified max number of lighthouses in level
        protected int m_maxNumberOfLighthouses;

        // store relevant art gallery objects
        protected ArtGallerySolution m_solution;
        protected ArtGalleryIsland m_levelMesh;
        protected ArtGalleryLightHouse m_selectedLighthouse;
        private IController _controllerImplementation;

        public Polygon2D LevelPolygon { get; protected set; }

        /// <inheritdoc />
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

        // Use this for initialization
        void Start()
        {
            m_solution = ScriptableObject.CreateInstance<ArtGallerySolution>();
            m_levelMesh = GetComponentInChildren<ArtGalleryIsland>();

            // go to initial island polygon
            AdvanceLevel();
        }

        // Update is called once per frame
        protected abstract void Update();

        /// <inheritdoc />
        public abstract void CheckSolution();


        /// <inheritdoc />
        public void AdvanceLevel()
        {
            Debug.Log("advance to next level");
            m_levelCounter++;

            if (m_levelCounter < m_levels.Count)
            {
                InitLevel();
            }
            else
            {
                SceneManager.LoadScene(m_victoryScreen);
            }
        }

        /// <summary>
        /// Update the text field with max number of lighthouses which can still be placed
        /// </summary>
        protected void UpdateLighthouseText()
        {
            m_lighthouseText.text = "Torches left: " + (m_maxNumberOfLighthouses - m_solution.Count);
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
        /// Update the vision polygon for the given lighthouse.
        /// Calculates the visibility polygon.
        /// </summary>
        /// <param name="m_lighthouse"></param>
        public void UpdateVision(ArtGalleryLightHouse m_lighthouse)
        {

            if (LevelPolygon.ContainsInside(m_lighthouse.Pos))
            {
                // calculate new visibility polygon
                var vision = Visibility.Vision(LevelPolygon, m_lighthouse.Pos);

                if (vision == null)
                {
                    throw new Exception("Vision polygon cannot be null");
                }

                // update lighthouse visibility
                m_lighthouse.VisionPoly = vision;
                m_lighthouse.VisionAreaMesh.Polygon = vision;
            }
            else
            {
                // remove visibility polygon from lighthouse
                m_lighthouse.VisionPoly = null;
                m_lighthouse.VisionAreaMesh.Polygon = null;
            }
        }

        /// <summary>
        /// Handle a click on the island mesh.
        /// </summary>
        public abstract void HandleIslandClick();
    }
}
