namespace ConvexHull
{
    using UnityEngine;

    public class HullPoint : MonoBehaviour
    {
        public Vector2 Pos { get; private set; }

        private HullController m_controller;

        void Awake()
        {
            Pos = new Vector2(transform.position.x, transform.position.y);
            m_controller = FindObjectOfType<HullController>();
        }

        void OnMouseDown()
        {
            m_controller.m_line.enabled = true;
            m_controller.m_firstPoint = this;
            m_controller.m_line.SetPosition(0, Pos);
        }

        void OnMouseEnter()
        {
            if (m_controller.m_firstPoint == null) return;

            m_controller.m_locked = true;
            m_controller.m_secondPoint = this;
            m_controller.m_line.SetPosition(1, Pos);
        }

        void OnMouseExit()
        {
            if (this != m_controller.m_secondPoint) return;

            m_controller.m_locked = false;
            m_controller.m_secondPoint = null;
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition + 10 * Vector3.forward);
            m_controller.m_line.SetPosition(1, pos);
        }
    }
}
