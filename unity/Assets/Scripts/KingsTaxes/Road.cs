using UnityEngine;
using Algo.Graph;

namespace KingsTaxes
{

    public class Road: MonoBehaviour {
        private KingsTaxesController m_gameController;
        private Edge m_edge;

        public Edge Edge {get{ return m_edge; }internal set { m_edge = value; }   }

        void Awake()
        {
            m_gameController = FindObjectOfType<KingsTaxesController>();
        }

        void OnMouseUpAsButton()
        {
            m_gameController.RemoveEdge(m_edge);
            Destroy(gameObject);
        }
    }

}
