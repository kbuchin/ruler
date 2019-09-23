namespace KingsTaxes
{
    using General.Controller;
    using General.Model;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using Util.Geometry.Graph;
    using KingsTaxes.Model;

    public abstract class KingsTaxesController : MonoBehaviour, IController
    {
        [SerializeField]
        private GameObject m_roadMeshPrefab;
        [SerializeField]
        private GameObject m_castlePrefab;
        [SerializeField]
        private GameObject m_villagePrefab;
        [SerializeField]
        private List<KingsTaxesLevel> m_levels;
        [SerializeField]
        private string m_victoryScene;
        [SerializeField]
        protected ButtonContainer m_advanceButton;

        protected int m_levelCounter = 0;

        [SerializeField]
        protected bool m_endlessMode = false;
        protected string m_endlessScoreKey = "taxesscore";

        protected int m_endlessDifficulty;
        protected readonly float m_settlementRadius = 0.5f; // estimated radius of a settlement in unity units 
        protected bool m_solutionMode = false;

        protected List<GameObject> instantObjects = new List<GameObject>();

        protected IGraph m_graph;
        protected Settlement[] m_settlements;

        protected float m_t = 1f;

        public virtual void Awake ()
        { }

        // Use this for initialization
        public virtual void Start () {
            InitLevel();
        }

        public void InitLevel()
        {
            if (m_levelCounter >= m_levels.Count && m_endlessMode)
            {
                m_endlessDifficulty = PlayerPrefs.GetInt(m_endlessScoreKey);
                Camera.main.orthographicSize = 2 * (1 + m_settlementRadius * Mathf.Sqrt(m_endlessDifficulty));

                var height = Camera.main.orthographicSize * 2;
                var width = height * Camera.main.aspect;
                List<Vector2> positions = InitEndlessLevel(m_endlessDifficulty, width, height);

                foreach (var positon in positions)
                {
                    GameObject obj;
                    if (Random.Range(0f, 1f) < .75f)
                    {
                        obj = Instantiate(m_villagePrefab, positon, Quaternion.identity);
                    }
                    else
                    {
                        obj = Instantiate(m_castlePrefab, positon, Quaternion.identity);
                    }
                    instantObjects.Add(obj);
                }

                m_t = 1.5f;
            }
            else
            {
                // initialize settlements
                foreach (var village in m_levels[m_levelCounter].Villages)
                {
                    var obj = Instantiate(m_villagePrefab, village, Quaternion.identity);
                    obj.transform.parent = this.transform;
                    instantObjects.Add(obj);
                }
                foreach (var castle in m_levels[m_levelCounter].Castles)
                {
                    var obj = Instantiate(m_castlePrefab, castle, Quaternion.identity);
                    obj.transform.parent = this.transform;
                    instantObjects.Add(obj);
                }

                m_t = m_levels[m_levelCounter].TSpannerRatio;
            }

            //Make vertex list
            m_settlements = FindObjectsOfType<Settlement>();

            //init empty grap
            m_graph = new AdjacencyListGraph(m_settlements.Select<Settlement, Vertex>(go => go.Vertex));

            FinishLevelSetup();

            m_advanceButton.Disable();
        }

        // Update is called once per frame
        public virtual void Update ()
        { }

        /// <summary>
        /// Is called to finish level creation, e.g. for creating a solution for a new level
        /// </summary>
        public abstract void FinishLevelSetup();

        /// <summary>
        /// Is called after removing or adding an edge
        /// </summary>
        public abstract void CheckSolution();

        /// <summary>
        /// Gives the position for a random endlesss level. The positons are centered around 0,0 
        /// </summary>
        /// <param name="level">The difficulty level, in range 0 -- infty</param>
        public abstract List<Vector2> InitEndlessLevel(int level, float width, float height);


        /// <summary>
        /// Advances to the next level
        /// </summary>
        public void AdvanceLevel()
        {
            if (m_levelCounter >= m_levels.Count && m_endlessMode)
            {
                if(!m_solutionMode)
                {
                    PlayerPrefs.SetInt(m_endlessScoreKey, m_endlessDifficulty + 1);
                }
            }
            else
            {
                m_levelCounter++;
            }

            Clear();

            if (m_levelCounter >= m_levels.Count && !m_endlessMode)
            {
                SceneManager.LoadScene(m_victoryScene);
            }
            else
            {
                InitLevel();
            }
        }

        /// <summary>
        /// Builds a road between given settlements
        /// </summary>
        /// <param name="settlement1"></param>
        /// <param name="settlement2"></param>
        public void AddRoad(Settlement settlement1, Settlement settlement2)
        {
            // dont add road to itself
            if (settlement1 == settlement2) return;

            var edge = m_graph.AddEdge(settlement1.Vertex, settlement2.Vertex);

            if (edge != null)
            {
                var roadmesh = Instantiate(m_roadMeshPrefab, Vector3.forward, Quaternion.identity) as GameObject;
                roadmesh.transform.parent = this.transform;
                instantObjects.Add(roadmesh);
                var roadmeshScript = roadmesh.GetComponent<ReshapingMesh>();
                roadmeshScript.CreateNewMesh(settlement1.transform.position, settlement2.transform.position);
                roadmesh.GetComponent<Road>().Edge = edge;

                CheckSolution();
            }
        }

        public void RemoveRoad(Road road)
        {
            m_graph.RemoveEdge(road.Edge);
            CheckSolution();
        }

        /// <summary>
        /// Returns count non-overlaping random positions not on the boundary
        /// </summary>
        /// <param name="count"> The number of positions returned</param>
        /// <param name="width"> The widht of the rectangle in which the positions should lie</param>
        /// <param name="height"> The height of the rectangle in which the positions should lie</param>
        /// <returns></returns>
        public List<Vector2> RandomPos(int count, float width, float height)
        {
            var result = new List<Vector2>();

            while (result.Count < count)
            {
                var xpos = Random.Range(-width / 2 + 2 * m_settlementRadius, width / 2 - 2 * m_settlementRadius);
                var ypos = Random.Range(-height / 2 + 2 * m_settlementRadius, height / 2 - 2 * m_settlementRadius);
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

        protected void Clear()
        {
            m_graph.Clear();

            foreach (var obj in instantObjects)
            {
                DestroyImmediate(obj);
            }

            instantObjects.Clear();

            m_settlements = null;
        }
    }
}
