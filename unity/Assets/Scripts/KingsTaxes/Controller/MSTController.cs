namespace KingsTaxes
{
    using Util.Geometry.Graph;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Algorithms.Graph;
    using General.Model;

    class MSTController : KingsTaxesController
    {
        [SerializeField]
        private GameObject m_solutionRoadPrefab;

        private IGraph m_goalgraph;

        public MSTController()
        {
            m_endlessScoreKey = "mstscore";
        }

        public override void FinishLevelSetup()
        {
            m_goalgraph = new AdjacencyListGraph(m_settlements.Select<Settlement, Vertex>(go => new Vertex(go.Pos)).ToList());
            m_goalgraph.MakeComplete();
            m_goalgraph = MST.MinimumSpanningTree((AdjacencyListGraph)m_goalgraph);

            Debug.Log(m_goalgraph);
        }


        public override void CheckSolution()
        {
            if (m_graph.Equals(m_goalgraph))
            {
                m_advanceButton.Enable();
            }   
        }


        public override List<Vector2> InitEndlessLevel(int level, float width, float height)
        {
            return RandomPos(level + 3, width, height);
        }

        public void ShowSolution()
        {
            if (m_solutionMode)
            {
                return; // DivideSolution already displayed
            }
            m_solutionMode = true; 

            foreach (var edge in m_goalgraph.Edges)
            {
                var solutionRoad = Instantiate(m_solutionRoadPrefab, Vector3.forward, Quaternion.identity) as GameObject;
                solutionRoad.transform.parent = this.transform;
                instantObjects.Add(solutionRoad);
                var roadmeshScript = solutionRoad.GetComponent<ReshapingMesh>();
                roadmeshScript.CreateNewMesh(edge.Start.Pos, edge.End.Pos);
            }
        }
    }
}
