namespace KingsTaxes
{
    using UnityEngine;

    class RoadBuilder : MonoBehaviour
    {
        // points to settlement the user is drawing an edge between
        // otherwise null
        private Settlement m_firstSettlement = null;
        private Settlement m_secondSettlement = null;

        // whether the end of the edge is currently locked onto a settlement
        private bool m_locked = false;

        private LineRenderer m_line;
        private KingsTaxesController m_gameController;

        // Use this for initialization
        void Awake()
        {
            m_line = GetComponent<LineRenderer>();
            m_gameController = FindObjectOfType<KingsTaxesController>();
        }

        // Update is called once per frame
        void Update()
        {
            if (m_locked)
            {
                if (!Input.GetMouseButton(0))
                {
                    // create road
                    m_gameController.AddRoad(m_firstSettlement, m_secondSettlement);

                    // clear current road line
                    Clear();
                }
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    //add forward vector to create distance to the camera
                    var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition + 10 * Vector3.forward);
                    m_line.SetPosition(1, pos);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    // clear current road line
                    Clear();
                }
            }
        }

        /// <summary>
        /// Handles a mouse down event on the given settlement.
        /// </summary>
        /// <param name="a_target"></param>
        public void MouseDown(Settlement a_target)
        {
            // enable line drawing
            m_line.enabled = true;

            // set first settlement of road
            m_firstSettlement = a_target;

            // update road line start
            m_line.SetPosition(0, a_target.transform.position);
        }

        /// <summary>
        /// Handles a mouse up event on the given settlement.
        /// </summary>
        /// <param name="a_target"></param>
        public void MouseEnter(Settlement a_target)
        {
            // do nothing if no start settlement has been selected
            if (m_firstSettlement == null) return;

            // add lock to target settlement
            m_locked = true;
            m_secondSettlement = a_target;

            // update road line end to target settlement
            m_line.SetPosition(1, a_target.transform.position);
        }

        /// <summary>
        /// Handles a mouse exit event for the given settlement.
        /// </summary>
        /// <param name="a_target"></param>
        public void MouseExit(Settlement a_target)
        {
            // do nothing if settlement was not current target
            if (a_target != m_secondSettlement) return;

            // remove lock and target settlement
            m_locked = false;
            m_secondSettlement = null;

            // update road line to mouse position
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition + 10 * Vector3.forward);
            m_line.SetPosition(1, pos);
        }

        /// <summary>
        /// Resets the line drawing to the initial state
        /// </summary>
        void Clear()
        {
            // remove lock and settlements
            m_locked = false;
            m_firstSettlement = null;
            m_secondSettlement = null;

            // disable line drawer
            m_line.enabled = false;
        }   
    }
}
