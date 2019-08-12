namespace ArtGallery
{
    using General.Model;
    using UnityEngine;
    using Util.Geometry.Polygon;

    public class ArtGalleryLightHouse : MonoBehaviour
    {
        private ArtGalleryController m_controller;

        public ArtGalleryIsland m_visionAreaMesh { get; set; }

        [SerializeField]
        private GameObject m_visionAreaPrefab;

        public Vector3 Pos
        {
            get
            {
                return gameObject.transform.position;
            }
            set
            {
                gameObject.transform.position = value;
                m_controller.UpdateVision(this);
            }
        }

        public Polygon2D VisionArea { get; set; }

        void Awake()
        {
            m_controller = FindObjectOfType<ArtGalleryController>();
            GameObject go = Instantiate(m_visionAreaPrefab, new Vector3(0,0,-1.5f), Quaternion.identity) as GameObject;
            m_visionAreaMesh = go.GetComponent<ArtGalleryIsland>();

            m_controller.UpdateVision(this);
        }

        void OnDestroy()
        {
            if (m_visionAreaMesh != null)
            {
                Destroy(m_visionAreaMesh.gameObject);
            }
        }
        
        void OnMouseDown()
        {
            m_controller.SelectLighthouse(this);
        }
    }
}
