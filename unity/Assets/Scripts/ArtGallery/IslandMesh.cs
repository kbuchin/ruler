namespace ArtGallery
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using General.Mesh;

    class IslandMesh : Polygon2DMesh
    {
        [SerializeField]
        private ArtGalleryController m_controller;

        protected IslandMesh()
        {
            m_scale = 4;
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
