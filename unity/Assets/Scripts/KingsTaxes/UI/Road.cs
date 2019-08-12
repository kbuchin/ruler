namespace KingsTaxes
{
    using UnityEngine;
    using Util.Geometry.Graph;

    public class Road : MonoBehaviour {
        private KingsTaxesController m_gameController;

        public Edge Edge { get; internal set; }

        void Awake()
        {
            m_gameController = FindObjectOfType<KingsTaxesController>();
        }

        void OnMouseUpAsButton()
        {
            m_gameController.RemoveRoad(this);
            Destroy(gameObject);
        }
    }

}
