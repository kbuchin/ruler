namespace KingsTaxes
{
    using General.Controller;
    using General.Menu;
    using General.Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using Util.Geometry.Graph;

    /// <summary>
    /// Parent controller for all game controllers related to the Kings Taxes game
    /// </summary>
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

        // Player prefs keys for endless levels beat and regular games beat
        protected string m_endlessScoreKey = "taxes_score";
        protected string m_beatKey = "taxes_beat";

        // estimated radius of a settlement in unity units 
        protected readonly float m_settlementRadius = 0.6f;
        protected bool m_solutionMode = false;

        // list of game objects instantiated, for removal
        protected List<GameObject> instantObjects = new List<GameObject>();

        // graph information
        protected IGraph m_graph;
        protected Settlement[] m_settlements;

        // t-spanner ratio for the current level
        // only relevant for spanner, but stored with each level object
        protected float m_t = 1f;

        // Use this for initialization
        public virtual void Start()
        {
            InitLevel();
        }

        // Update called every frame
        public virtual void Update()
        { }

        public void InitLevel()
        {
            // clear old level
            Clear();

            if (m_levelCounter >= m_levels.Count && m_endlessMode)
            {
                var m_endlessDifficulty = PlayerPrefs.GetInt(m_endlessScoreKey);
                Camera.main.orthographicSize = 2 * (1 + m_settlementRadius * Mathf.Sqrt(m_endlessDifficulty));

                var height = Camera.main.orthographicSize * 2;
                var width = height * Camera.main.aspect;
                List<Vector2> positions = InitEndlessLevel(m_endlessDifficulty, width, height);

                foreach (var position in positions)
                {
                    GameObject obj;
                    if (UnityEngine.Random.Range(0f, 1f) < .75f)
                    {
                        obj = Instantiate(m_villagePrefab, position, Quaternion.identity);
                    }
                    else
                    {
                        obj = Instantiate(m_castlePrefab, position, Quaternion.identity);
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

            //init empty graph
            m_graph = new AdjacencyListGraph(m_settlements.Select(go => go.Vertex));

            FinishLevelSetup();

            m_advanceButton.Disable();
        }

        /// <summary>
        /// Is called to finish level creation, e.g. for creating a solution for a new level
        /// </summary>
        protected abstract void FinishLevelSetup();

        /// <summary>
        /// Is called after removing or adding an edge
        /// </summary>
        public abstract void CheckSolution();

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
            if (m_levelCounter >= m_levels.Count && m_endlessMode)
            {
                if (!m_solutionMode)
                {
                    // update number of endless levels solved
                    var m_endlessDifficulty = PlayerPrefs.GetInt(m_endlessScoreKey);
                    PlayerPrefs.SetInt(m_endlessScoreKey, m_endlessDifficulty + 1);
                }
            }
            else
            {
                // increase level index
                m_levelCounter++;
            }

            if (m_levelCounter >= m_levels.Count && !m_endlessMode)
            {
                // all levels beat, load victory
                PlayerPrefs.SetInt(m_beatKey, 1);
                SceneManager.LoadScene(m_victoryScene);
            }
            else
            {
                // initialize new level
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
            if (settlement1 == settlement2 || m_graph.ContainsEdge(settlement1.Vertex, settlement2.Vertex))
            {
                return;
            }

            // instantiate a road object in game
            var roadmesh = Instantiate(m_roadMeshPrefab, Vector3.forward, Quaternion.identity) as GameObject;
            roadmesh.transform.parent = this.transform;

            // remember road for destroyal later
            instantObjects.Add(roadmesh);

            // create road mesh
            var roadmeshScript = roadmesh.GetComponent<ReshapingMesh>();
            roadmeshScript.CreateNewMesh(settlement1.transform.position, settlement2.transform.position);

            // create road edge
            var edge = m_graph.AddEdge(settlement1.Vertex, settlement2.Vertex);

            // error check
            if (edge == null)
            {
                throw new InvalidOperationException("Road could not be added to graph");
            }

            // link edge to road
            roadmesh.GetComponent<Road>().Edge = edge;

            // check if solution present
            CheckSolution();
        }

        /// <summary>
        /// Removes the given road from the graph
        /// </summary>
        /// <param name="road"></param>
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
        protected List<Vector2> RandomPos(int count, float width, float height)
        {
            var result = new List<Vector2>();

            while (result.Count < count)
            {
                // find uniform random position centered around (0,0) within width and height
                // taking into account settlement radius
                var xpos = UnityEngine.Random.Range(-width / 2 + 2 * m_settlementRadius, width / 2 - 2 * m_settlementRadius);
                var ypos = UnityEngine.Random.Range(-height / 2 + 2 * m_settlementRadius, height / 2 - 2 * m_settlementRadius);
                var pos = new Vector2(xpos, ypos);

                // add if not too close to other settlement
                if (!result.Exists(r => Vector2.Distance(r, pos) < 2 * m_settlementRadius))
                {
                    result.Add(pos);
                }
            }

            return result;
        }

        /// <summary>
        /// Clears graph and relevant game objects
        /// </summary>
        protected void Clear()
        {
            // clear graph if exists
            if (m_graph != null) m_graph.Clear();

            // destroy game objects related to graph
            foreach (var obj in instantObjects)
            {
                // destroy immediate
                // since controller will search for existing objects afterwards
                DestroyImmediate(obj);
            }
            instantObjects.Clear();

            m_settlements = null;
        }
    }
}
