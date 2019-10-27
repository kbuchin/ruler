namespace KingsTaxes
{
    using UnityEngine;
    using Util.Geometry.Graph;

    /// <summary>
    /// Handles interaction with road object, namely user clicking.
    /// </summary>
    public class Road : MonoBehaviour
    {
        /// <summary>
        /// The edge this road corresponds to in the graph
        /// </summary>
        public Edge Edge { get; internal set; }

        private KingsTaxesController m_gameController;

        // Use this for initialization
        void Awake()
        {
            // find gamecontroller in scene
            m_gameController = FindObjectOfType<KingsTaxesController>();
        }

        void OnMouseUpAsButton()
        {
            // destroy the road object
            m_gameController.RemoveRoad(this);
            Destroy(gameObject);
        }
    }

}
