namespace KingsTaxes
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using Util.Geometry.Graph;

    public abstract class KingsTaxesController : MonoBehaviour {
        public string m_nextlevel = "lv";
        public bool m_endlessMode = false;
        public string m_endlessScoreKey = "";
        public GameObject m_castlePrefab;
        public GameObject m_villagePrefab;

        private RoadBuilder m_roadBuilder;
        private int m_endlessLevel;
        protected float m_settlementRadius = 0.33f; // estimated radius of a settlement in unity units 
        protected bool m_solutionMode = false;
        protected IGraph m_graph;
        protected Settlement[] m_settlements;
            

        protected virtual void Awake ()
        {
            m_roadBuilder = FindObjectOfType<RoadBuilder>();
        }

	    // Use this for initialization
	    protected virtual void Start () {
            if (m_endlessMode)
            {
                m_endlessLevel = PlayerPrefs.GetInt(m_endlessScoreKey);
                Camera.main.orthographicSize = 2 * (1+ m_settlementRadius * Mathf.Sqrt(m_endlessLevel)) ;
                Debug.Log(2 * (1 + m_settlementRadius * Mathf.Sqrt(m_endlessLevel)));
                Debug.Log(Camera.main.aspect);

                var height = Camera.main.orthographicSize * 2;
                var width = height * Camera.main.aspect;
                List<Vector2> positions = InitEndlessLevel(m_endlessLevel, width, height);
                foreach(var positon in positions)
                {
                    if (Random.Range(0f, 1f) < .75f)
                    {
                        Instantiate(m_villagePrefab, positon, Quaternion.identity);
                    } else
                    {
                        Instantiate(m_castlePrefab, positon, Quaternion.identity);
                    }
                }
            }
            //Make vertex list
            m_settlements = FindObjectsOfType<Settlement>();
            //init Empty grap
            m_graph = new AdjacencyListGraph(m_settlements.Select<Settlement, Vertex>(go => go.Vertex));
        }
	
	    // Update is called once per frame
	    protected virtual void Update () {
	
	    }

        /// <summary>
        /// Is called after removing or adding an edge
        /// </summary>
        protected abstract void CheckVictory();

        /// <summary>
        /// Gives the position for a random endlesss level. The positons are centered around 0,0 
        /// </summary>
        /// <param name="level">The difficulty level, in range 0 -- infty</param>
        protected abstract List<Vector2> InitEndlessLevel(int level, float width, float height);


        /// <summary>
        /// Advances to the next level
        /// </summary>
        public void AdvanceLevel()
        {
            if (m_endlessMode)
            {
                if(!m_solutionMode)
                {
                    PlayerPrefs.SetInt(m_endlessScoreKey, m_endlessLevel + 1);
                }
            }
            SceneManager.LoadScene(m_nextlevel);
        }

        internal void MouseDown(Settlement a_target)
        {
            m_roadBuilder.MouseDown(a_target);
        }

        internal void MouseEnter(Settlement a_target)
        {
            m_roadBuilder.MouseEnter(a_target);
        }


        internal void MouseExit(Settlement a_target)
        {
            m_roadBuilder.MouseExit(a_target);
        }

        /// <summary>
        /// Adds edge to game graph if it's not already there.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <returns> The edge that was added (or null)</returns>
        internal Edge AddEdge(Vertex vertex1, Vertex vertex2)
        {
            var edge = m_graph.AddEdge(vertex1, vertex2);
            if (edge == null)
            {
                return null;
            }
            CheckVictory();
            return edge;

        }

        internal void RemoveEdge(Edge edge)
        {
            m_graph.RemoveEdge(edge);
            CheckVictory();
        }

        /// <summary>
        /// Returns count non-overlaping random positions not on the boundary
        /// </summary>
        /// <param name="count"> The number of positions returned</param>
        /// <param name="width"> The widht of the rectangle in which the positions should lie</param>
        /// <param name="height"> The height of the rectangle in which the positions should lie</param>
        /// <returns></returns>
        protected List<Vector2> RandomPos(int count, float width, float height)
        {
            var result = new List<Vector2>();

            while (result.Count < count)
            {
                var xpos = Random.Range(-width / 2 + 2 * m_settlementRadius, width / 2 - 2 * m_settlementRadius);
                var ypos = Random.Range(-height / 2 + 2 * m_settlementRadius, height / 2 - 2 * m_settlementRadius);
                Debug.Log(ypos);
                var pos = new Vector2(xpos, ypos);
                var accepted = true;
                foreach (var r in result)
                {
                    if (Vector2.Distance(r, pos) < 2 * m_settlementRadius)
                    {
                        accepted = false;
                    }
                }
                if (accepted)
                {
                    result.Add(pos);
                }
            }

            return result;
        }
    }
}
