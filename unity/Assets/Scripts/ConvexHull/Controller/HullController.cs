namespace ConvexHull
{
    using General.Menu;
    using General.Model;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Geometry.Polygon;
    using Util.Algorithms.Polygon;
    using Util.Geometry;
    using General.Controller;
    using UnityEngine.SceneManagement;

    public class HullController : MonoBehaviour, IController
    {
        public LineRenderer m_line;

        [SerializeField]
        private GameObject m_roadMeshPrefab;
        [SerializeField]
        private GameObject m_pointPrefab;
        [SerializeField]
        private ButtonContainer m_advanceButton;

        [SerializeField]
        private List<HullLevel> m_levels;
        [SerializeField]
        private string m_victoryScene;

        internal HullPoint m_firstPoint;
        internal HullPoint m_secondPoint;
        internal bool m_locked;

        private List<HullPoint> m_points;
        private HashSet<LineSegment> m_segments;
        private Polygon2D m_solutionHull;

        private List<GameObject> instantObjects;

        protected int m_levelCounter = 0;

        void Start()
        {
            // get unity objects
            m_points = new List<HullPoint>();
            m_segments = new HashSet<LineSegment>();
            instantObjects = new List<GameObject>();

            InitLevel();
        }

        void Update()
        {
            if (m_locked && !Input.GetMouseButton(0))
            {
                // create road
                AddSegment(m_firstPoint, m_secondPoint);
            }
            else if (Input.GetMouseButton(0))
            {
                // update road endpoint
                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition + 10 * Vector3.forward);
                m_line.SetPosition(1, pos);
            }

            // clear road creation variables
            if ((m_locked && !Input.GetMouseButton(0)) || Input.GetMouseButtonUp(0))
            {
                m_locked = false;
                m_firstPoint = null;
                m_secondPoint = null;
                m_line.enabled = false;
            }
        }

        public void InitLevel()
        {
            if (m_levelCounter >= m_levels.Count)
            {
                SceneManager.LoadScene(m_victoryScene);
                return;
            }

            // clear old level
            Clear();

            // initialize settlements
            foreach (var point in m_levels[m_levelCounter].Points)
            {
                var obj = Instantiate(m_pointPrefab, point, Quaternion.identity) as GameObject;
                obj.transform.parent = this.transform;
                instantObjects.Add(obj);
            }

            //Make vertex list
            m_points = FindObjectsOfType<HullPoint>().ToList();

            // compute convex hull
            m_solutionHull = ConvexHull.ComputeConvexHull(m_points.Select(v => v.Pos));

            m_advanceButton.Disable();
        }

        public void AdvanceLevel()
        {
            // increase level index
            m_levelCounter++;
            InitLevel();
        }

        public void AddSegment(HullPoint a_point1, HullPoint a_point2)
        {
            var segment = new LineSegment(a_point1.Pos, a_point2.Pos);

            // dont add double segments
            if (m_segments.Contains(segment) || m_segments.Contains(new LineSegment(a_point2.Pos, a_point1.Pos)))
                // also check reverse
                return;

            m_segments.Add(segment);

            // instantiate new road mesh
            var roadmesh = Instantiate(m_roadMeshPrefab, Vector3.forward, Quaternion.identity) as GameObject;
            roadmesh.transform.parent = this.transform;
            instantObjects.Add(roadmesh);

            roadmesh.GetComponent<HullSegment>().Segment = segment;

            var roadmeshScript = roadmesh.GetComponent<ReshapingMesh>();
            roadmeshScript.CreateNewMesh(a_point1.transform.position, a_point2.transform.position);
            
            CheckSolution();
        }

        public void RemoveSegment(HullSegment a_segment)
        {
            m_segments.Remove(a_segment.Segment);
            CheckSolution();
        }

        public void CheckSolution()
        {
            if (CheckHull())
            {
                m_advanceButton.Enable();
            }
            else
            {
                m_advanceButton.Disable();
            }
        }
        
        private bool CheckHull()
        {
            // quick return counts not equal
            if (m_solutionHull.Segments.Count != m_segments.Count) 
                return false;

            return m_solutionHull.Segments.All(seg => m_segments.Contains(seg) || 
                        m_segments.Contains(new LineSegment(seg.Point2, seg.Point1)));  // also check reverse
        }


        /// <summary>
        /// Clears hull and relevant game objects
        /// </summary>
        private void Clear()
        {
            m_solutionHull = null;
            m_points.Clear();
            m_segments.Clear();

            // destroy game objects created in level
            foreach (var obj in instantObjects)
            {
                // destroy immediate
                // since controller will search for existing objects afterwards
                DestroyImmediate(obj);
            }
        }
    }
}
