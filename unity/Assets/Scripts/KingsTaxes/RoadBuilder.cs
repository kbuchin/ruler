namespace KingsTaxes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    class RoadBuilder : MonoBehaviour
    {
        Settlement m_firstSettlement = null;
        Settlement m_secondSettlement = null;
        bool m_locked = false;
        private LineRenderer m_line;
        private KingsTaxesController m_gameController;

        [SerializeField]
        private GameObject m_roadMeshPrefab = null;

        void Start()
        {
            m_line = GetComponent<LineRenderer>();
            m_gameController = GameObject.FindGameObjectWithTag(Tags.GameController).GetComponent<KingsTaxesController>();
        }

        void Update()
        {
            if (m_locked)
            {
                if (Input.GetMouseButton(0))
                {
                    //do nothing; mouse not yet released
                }
                else
                {
                    //Verify if edge already exists
                    Build();
                }
            } else
            {
                if (Input.GetMouseButton(0))
                {
                    //add forward vector to create distance to the camera
                    var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition + 10 * Vector3.forward);
                    m_line.SetPosition(1, pos);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    Clear();
                }
            }
        }

        internal void MouseDown(Settlement a_target)
        {
            m_line.enabled = true;
            m_firstSettlement = a_target;
            m_line.SetPosition(0, a_target.transform.position);
        }

        internal void MouseEnter(Settlement a_target)
        {
            if (m_firstSettlement != null)
            {
                m_locked = true;
                m_secondSettlement = a_target;
                m_line.SetPosition(1, a_target.transform.position);
            }
        }

        internal void MouseExit(Settlement a_target)
        {
            if (a_target == m_secondSettlement)
            {
                m_locked = false;
                m_secondSettlement = null;
                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition + 10 * Vector3.forward);
                m_line.SetPosition(1, pos);
            }
        }

        /// <summary>
        /// Builds a road
        /// </summary>
        void Build()
        {
            var edge = m_gameController.AddEdge(m_firstSettlement.Vertex, m_secondSettlement.Vertex);
            if (edge != null)
            { 
                var roadmesh = Instantiate(m_roadMeshPrefab, Vector3.forward, Quaternion.identity) as GameObject;
                var roadmeshScript = roadmesh.GetComponent<ReshapingMesh>();
                roadmeshScript.CreateNewMesh(m_firstSettlement.transform.position, m_secondSettlement.transform.position);
                roadmesh.GetComponent<Road>().Edge = edge;
            }

            Clear();
        }


        /// <summary>
        /// resets the linedrawing to the initial state
        /// </summary>
        void Clear()
        {
            m_locked = false;
            m_firstSettlement = null;
            m_secondSettlement = null;

            m_line.enabled = false;

        }
    }
}
