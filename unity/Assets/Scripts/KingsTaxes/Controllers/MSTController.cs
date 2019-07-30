namespace KingsTaxes
{
    using Util.Geometry.Graph;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Algorithms;

    class MSTController : KingsTaxesController
    {
        private IGraph m_goalgraph;
        [SerializeField]
        private GameObject m_solutionRoadPrefab;
        private DisableButtonContainer m_advanceButton;

        protected override void Start()
        {
            base.Start();
            m_goalgraph = new AdjacencyListGraph(m_settlements.Select<Settlement, Vertex>(go => new Vertex(go.Pos)).ToList());
            m_goalgraph.MakeComplete();
            MST.MinimumSpanningTree(m_goalgraph);

            m_advanceButton = GameObject.FindGameObjectWithTag(Tags.AdvanceButtonContainer).GetComponent<DisableButtonContainer>();
            m_advanceButton.Disable();
        }


        protected override void CheckVictory()
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

        public void ShowSolution()
        {
            if (m_solutionMode)
            {
                return; // Solution already displayed
            }
            m_solutionMode = true; 

            foreach (var edge in m_goalgraph.Edges)
            {
                var solutionRoad = Instantiate(m_solutionRoadPrefab, Vector3.forward, Quaternion.identity) as GameObject;
                var roadmeshScript = solutionRoad.GetComponent<ReshapingMesh>();
                roadmeshScript.CreateNewMesh(edge.Start.Pos, edge.End.Pos);
            }
        }
    }
}
