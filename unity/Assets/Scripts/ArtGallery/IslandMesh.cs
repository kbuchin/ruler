using UnityEngine;

namespace ArtGallery
{
    class IslandMesh:VertexSimplePolygonMesh
    {
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
