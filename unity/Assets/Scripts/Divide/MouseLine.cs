namespace Divide
{
    using UnityEngine;
    using Util.Geometry;

    public class MouseLine : MonoBehaviour
    {

        private LineRenderer m_line;
        private bool m_updating = false;
        private Vector3 m_pos1;
        private Vector3 m_pos0;
        private GameController m_gameController;

        // Use this for initialization
        void Awake()
        {
            m_line = GetComponentInChildren<LineRenderer>();
            m_gameController = GameObject.FindGameObjectWithTag(Tags.GameController).GetComponent<GameController>();
        }

        // Update is called once per frame
        void Update()
        {
            //If mouse is relead and the mouse is no longer held
            if (!Input.GetMouseButton(0) && m_updating)
            {
                m_updating = false;
                var d = m_pos0 - m_pos1;
                if (d.sqrMagnitude > .25)
                {
                    m_pos0 = m_pos0 - 10 * d;
                    m_pos1 = m_pos1 + 10 * d;
                    m_line.SetPositions(new[] { m_pos0, m_pos1 });
                    m_gameController.processSolution(new Line(m_pos0, m_pos1));
                }
                else
                {
                    DisableLine();
                }
            }

            if (m_updating)
            {
                //add forward vector to create distance to the camera
                m_pos1 = Camera.main.ScreenToWorldPoint(Input.mousePosition + 10 * Vector3.forward);
                m_line.SetPosition(1, m_pos1);
            }
        }

        //we use a event to prevent drawing a line when switching things
        void OnMouseDown()
        {
            m_line.enabled = true;
            m_updating = true;
            m_pos0 = Camera.main.ScreenToWorldPoint(Input.mousePosition + 10* Vector3.forward);
            m_line.SetPosition(0, m_pos0);
        }

        public void DisableLine()
        {
            m_line.enabled = false;
        }
    }
}