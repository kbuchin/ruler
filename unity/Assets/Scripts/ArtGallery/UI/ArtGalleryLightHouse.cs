namespace ArtGallery
{
    using General.Model;
    using UnityEngine;
    using Util.Geometry.Polygon;

    /// <summary>
    /// Represents the lighthouse object in the game.
    /// Holds its position as well as the corresponding visibility polygon.
    /// Handles user clicks and drags.
    /// </summary>
    public class ArtGalleryLightHouse : MonoBehaviour
    {
        // stores a prefab object for the vision polygon
        [SerializeField]
        private GameObject m_visionAreaPrefab;

        private ArtGalleryController m_controller;

        /// <summary>
        /// 
        /// </summary>
        public ArtGalleryIsland m_visionAreaMesh { get; set; }

        /// <summary>
        /// Stores lighthouse position. Updates vision after a change in position.
        /// </summary>
        public Vector3 Pos
        {
            get
            {
                return gameObject.transform.position;
            }
            set
            {
                gameObject.transform.position = value;

                // update vision polygon
                m_controller.UpdateVision(this);
            }
        }

        /// <summary>
        /// Holds the visibility polygon.
        /// </summary>
        public Polygon2D VisionPoly { get; set; }

        // Use this for initialization
        void Start()
        {
            m_controller = FindObjectOfType<ArtGalleryController>();
            
            // initialize the vision polygon
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
