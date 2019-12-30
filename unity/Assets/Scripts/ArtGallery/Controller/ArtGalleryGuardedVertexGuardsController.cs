using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArtGallery;
using General.Controller;
using UnityEngine;

namespace Assets.Scripts.ArtGallery.Controller
{
    //Will be implemented by Karina and Job
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
    /// Main controller for the art gallery guarded vertex guards game.
    /// Handles the game update loop, as well as level initialization and
    /// advancement.
    /// </summary>
    public class ArtGalleryGuardedVertexGuardsController : MonoBehaviour, IController
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

        // stores the current level index
        private int m_levelCounter = -1;

        // specified max number of lighthouses in level
        private int m_maxNumberOfLighthouses;

        // store relevant art gallery objects
        private ArtGallerySolution m_solution;
        private ArtGalleryIsland m_levelMesh;
        private ArtGalleryLightHouse m_selectedLighthouse;

        public Polygon2D LevelPolygon { get; private set; }


        /// <inheritdoc />
        public void InitLevel()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void CheckSolution()
        {
            throw new NotImplementedException();
        }



        /// <summary>
        /// Handle a click on the island mesh.
        /// </summary>
        public void HandleIslandClick()
        {
        	// TODO KARINA
            // return if lighthouse was already selected or player can place no more lighthouses
            if (m_selectedLighthouse != null || m_solution.Count >= m_maxNumberOfLighthouses)
                return;

            // obtain mouse position
            var worldlocation = Camera.main.ScreenPointToRay(Input.mousePosition).origin;
            worldlocation.z = -2f;
            Vector2 worldlocation2D = worldlocation;


            //find closest vertex. CHeck if it is occupied. If not, place the guard there.
            var closestVertex2D = LevelPolygon.Vertices.ElementAt(0);
            var minMagnitude = (worldlocation2D-closestVertex2D).magnitude;
            foreach (var vtx2D in LevelPolygon.Vertices) 
            {
                var currentMagnitude = (worldlocation2D-vtx2D).magnitude;
                if (currentMagnitude < minMagnitude)
                {
                    minMagnitude = currentMagnitude;
                    closestVertex2D = vtx2D;
                }
            }
            //TODO: check if closestVertex already holds a lighthouse. If yes, return. 
            //LevelPolygon.
            
            // create a new lighthouse from prefab
            var go = Instantiate(m_lighthousePrefab, worldlocation, Quaternion.identity) as GameObject;

            // add lighthouse to art gallery solution
            m_solution.AddLighthouse(go);
            UpdateLighthouseText();

            CheckSolution();
        }
        
        /// <summary>
        /// Checks if the current placed lighthouses completely illuminate
        /// the room
        /// </summary>
        /// <returns>
        /// True if the complete room is illuminated, false otherwise
        /// </returns>
        public bool CheckVisibility()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if the current placed lighthouses can each see at lease one
        /// other lighthouse
        /// </summary>
        /// <returns>
        /// True if each lighthouse can see at least one other lighthouse,
        /// false otherwise
        /// </returns>
        public bool CheckGuardedGuards()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void AdvanceLevel()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Update the text field with max number of lighthouses which can still be placed
        /// </summary>
        private void UpdateLighthouseText()
        {
            m_lighthouseText.text = "Torches left: " + (m_maxNumberOfLighthouses - m_solution.Count);
        }
    }
}
