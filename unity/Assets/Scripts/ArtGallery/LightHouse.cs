
using UnityEngine;
using Algo.Polygons;

namespace ArtGallery
{
    class LightHouse:MonoBehaviour
    {
        private ArtGalleryController m_controller;
        private VertexSimplePolygon m_visionArea;
        private VertexSimplePolygonMesh m_visionAreaMesh;

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

        public VertexSimplePolygon VisonArea
        {
            get { return m_visionArea; }
        }

        void Awake()
        {
            m_controller = FindObjectOfType<ArtGalleryController>();
            GameObject go = Instantiate(m_visionAreaPrefab, new Vector3(0,0,-1.5f), Quaternion.identity) as GameObject;
            m_visionAreaMesh = go.GetComponent<VertexSimplePolygonMesh>();
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
                var va = m_controller.LevelPolygon.Vision(Pos);
                m_visionAreaMesh.Polygon = va;
                m_visionArea = va;
            }
        }
    }
}
