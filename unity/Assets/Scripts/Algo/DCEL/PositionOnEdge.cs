using UnityEngine;

namespace Algo.DCEL
{
    public class PositionOnEdge
    {
        private Halfedge m_edge;
        private Vector2 m_pos;

        public Vector2 Pos { get { return m_pos; } }
        public Halfedge Edge { get { return m_edge; } }


        public PositionOnEdge(Vector2 a_pos, Halfedge a_edge)
        {
            m_pos = a_pos;
            m_edge = a_edge;
        }
    }
}