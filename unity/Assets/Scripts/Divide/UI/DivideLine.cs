namespace Divide
{
    using UnityEngine;
    using Util.Geometry;

    /// <summary>
    /// Deals with user drawn lines.
    /// Updates a line renderer and calls the controller when line is drawn.
    /// </summary>
    public class DivideLine : MonoBehaviour
    {
        /// <summary>
        /// Holds the last line drawn by user
        /// </summary>
        public Line Line { get; private set; }

        private LineRenderer m_lineRenderer;
        private DivideController m_gameController;

        // holds start and end positions for line
        private Vector3 startPos, curPos;

        // holds whether line needs to be updated
        private bool m_updating = false;

        // Use this for initialization
        void Awake()
        {
            m_lineRenderer = GetComponentInChildren<LineRenderer>();
            m_gameController = FindObjectOfType<DivideController>();
            Line = null;
        }

        // Update is called once per frame
        void Update()
        {
            //If mouse is relead and the mouse is no longer held
            if (!Input.GetMouseButton(0) && m_updating)
            {
                // stop update line
                m_updating = false;

                // calculate difference vector
                var d = startPos - curPos;

                if (d.sqrMagnitude > .25)
                {
                    // move positions to increase size of line to cover entire screen
                    startPos -= 10 * d;
                    curPos += 10 * d;

                    // update line renderer
                    m_lineRenderer.SetPositions(new[] { startPos, curPos });

                    // update line
                    Line = new Line(startPos, curPos);
                    m_gameController.CheckSolution();
                }
                else
                {
                    // line too small, disable it
                    DisableLine();
                }
            }

            if (m_updating)
            {
                //add forward vector to create distance to the camera
                curPos = Camera.main.ScreenToWorldPoint(Input.mousePosition + 10 * Vector3.forward);

                // update line renderer
                m_lineRenderer.SetPosition(1, curPos);
            }
        }

        //we use a event to prevent drawing a line when switching things
        void OnMouseDown()
        {
            m_lineRenderer.enabled = true;
            m_updating = true;
            startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition + 10 * Vector3.forward);
            m_lineRenderer.SetPosition(0, startPos);
        }

        /// <summary>
        /// Disable line renderer
        /// </summary>
        public void DisableLine()
        {
            m_lineRenderer.enabled = false;
        }
    }
}