
using General.Mesh;
using UnityEngine;
using Util.Algorithms.Polygon;
using Util.Geometry.Polygon;

namespace ArtGallery
{
    class LightHouse : MonoBehaviour
    {
        private ArtGalleryController m_controller;
        private Polygon2DMesh m_visionAreaMesh;

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
                gameObject.transform.position = value; UpdateVisonArea();
            }
        }

        public Polygon2D VisionArea { get; private set; }

        void Awake()
        {
            m_controller = FindObjectOfType<ArtGalleryController>();
            GameObject go = Instantiate(m_visionAreaPrefab, new Vector3(0,0,-1.5f), Quaternion.identity) as GameObject;
            m_visionAreaMesh = go.GetComponent<Polygon2DMesh>();
            UpdateVisonArea();
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

        private void UpdateVisonArea()
        {
            if (m_controller.LevelPolygon.Contains(Pos))
            {
                VisionArea = Visibility.Vision(m_controller.LevelPolygon, Pos);
                m_visionAreaMesh.Polygon = VisionArea;
            }
        }
    }
}
