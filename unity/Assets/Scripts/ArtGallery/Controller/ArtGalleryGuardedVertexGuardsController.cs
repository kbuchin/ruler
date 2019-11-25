using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
