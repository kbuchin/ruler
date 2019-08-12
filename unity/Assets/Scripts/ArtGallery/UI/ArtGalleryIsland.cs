namespace ArtGallery
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using General.Model;

    public class ArtGalleryIsland : Polygon2DMesh
    {
        private ArtGalleryController m_controller;

        protected ArtGalleryIsland()
        {
            m_scale = 4f;
        }

        new void Awake()
        {
            base.Awake();
            m_controller = FindObjectOfType<ArtGalleryController>();
        }

        void OnMouseUpAsButton()
        {
            m_controller.IslandClick();
        }
    }
}
