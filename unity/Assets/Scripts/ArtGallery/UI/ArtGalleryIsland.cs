using System;
using UnityEngine;

namespace ArtGallery
{
    using General.Model;

    /// <summary>
    /// Represents the level island (2D polygon).
    /// Handles user clicks on the polygon
    /// </summary>
    public class ArtGalleryIsland : Polygon2DMesh
    {
        private AbstractArtGalleryController m_controller;

        public ArtGalleryIsland()
        {
            m_scale = 4f;
        }

        // Use this for initialization
        public new void Awake()
        {
            base.Awake();
            m_controller = FindObjectOfType<AbstractArtGalleryController>();
        }

        void OnMouseUpAsButton()
        {
            // call the relevant controller method
            m_controller.HandleIslandClick();
        }
    }
}
