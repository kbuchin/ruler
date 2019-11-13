namespace KingsTaxes
{
    using General.Model;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Algorithms.Graph;
    using Util.Geometry.Graph;

    /// <summary>
    /// Game controller for the MST minigame of the Kings Taxes game.
    /// </summary>
    class MSTController : KingsTaxesController
    {
        [SerializeField]
        private GameObject m_solutionRoadPrefab;

        // goal graph calculated by Prim's algorithm
        private IGraph m_goalgraph;

        public MSTController()
        {
            // player prefs keys
            m_endlessScoreKey = "mst_score";
            m_beatKey = "mst_beat";
        }

        public override void CheckSolution()
        {
            if (m_graph.Equals(m_goalgraph))
            {
                m_advanceButton.Enable();
            }
        }

        protected override List<Vector2> InitEndlessLevel(int level, float width, float height)
        {
            return RandomPos(level + 3, width, height);
        }
        protected override void FinishLevelSetup()
        {
            // create vertices from settlement objects
            var vertices = m_settlements.Select(go => new Vertex(go.Pos));

            // calculate MST
            m_goalgraph = MST.MinimumSpanningTree(vertices);
        }

        /// <summary>
        /// Shows MST solution to the player.
        /// </summary>
        public void ShowSolution()
        {
            // Divide solution already displayed
            if (m_solutionMode) return;

            m_solutionMode = true;

            // create solution roads for given graph
            foreach (var edge in m_goalgraph.Edges)
            {
                // instantiate solution road
                var solutionRoad = Instantiate(m_solutionRoadPrefab, Vector3.forward, Quaternion.identity) as GameObject;
                solutionRoad.transform.parent = this.transform;

                // store instantiated object
                instantObjects.Add(solutionRoad);

                // create road mesh
                var roadmeshScript = solutionRoad.GetComponent<ReshapingMesh>();
                roadmeshScript.CreateNewMesh(edge.Start.Pos, edge.End.Pos);
            }
        }
    }
}
