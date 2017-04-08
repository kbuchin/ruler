using Algo.Graph;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KingsTaxes
{
    class MSTController:KingsTaxesController
    {
        private Graph m_goalgraph;
        [SerializeField]
        private GameObject m_solutionRoadPrefab;
        private DisableButtonContainer m_advanceButton;

        protected override void Start()
        {
            base.Start();
            m_goalgraph = new Graph(m_settlements.Select<Settlement, Vector2>(go => go.Pos).ToList());
            m_goalgraph.MakeCompleteGraph();
            m_goalgraph.MinimumSpanningTree();

            m_advanceButton = GameObject.FindGameObjectWithTag(Tags.AdvanceButtonContainer).GetComponent<DisableButtonContainer>();
            m_advanceButton.Disable();
        }


        protected override void CheckVictory()
        {
            if (Graph.EqualGraphs(m_graph, m_goalgraph))
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
                roadmeshScript.CreateNewMesh(edge.Vertex1.Pos, edge.Vertex2.Pos);
            }
        }
    }
}
