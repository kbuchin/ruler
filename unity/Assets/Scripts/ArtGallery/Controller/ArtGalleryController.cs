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
    public class ArtGalleryController : AbstractArtGalleryController
    {
        /// <inheritdoc />
        public override void CheckSolution()
        {
            // calculate ratio of area visible
            var ratio = m_solution.Area / LevelPolygon.Area;

            Debug.Log(ratio + " part is visible");

            // see if entire polygon is covered
            if (MathUtil.GEQEps(ratio, 1f, 0.001f))
            {
                m_advanceButton.Enable();
            }
        }

        protected override void Update()
        {
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


        /// <inheritdoc />
        public override void HandleIslandClick()
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
    }
}