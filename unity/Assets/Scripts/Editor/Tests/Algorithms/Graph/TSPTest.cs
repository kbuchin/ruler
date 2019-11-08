namespace Util.Algorithms.Graph.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Algorithms.Graph;
    using Util.Geometry;
    using Util.Geometry.Graph;
    using Util.Math;

    [TestFixture]
    class TSPTest
    {
        private readonly List<Vertex> m_points;
        private readonly IGraph m_graph, m_complete4;
        private readonly float expTSP;

        public TSPTest()
        {
            m_points = new List<Vertex>() {
                new Vertex(0, 0),
                new Vertex(2, 0),
                new Vertex(0, 1),
                new Vertex(1.5f, -1)
            };

            m_graph = new AdjacencyListGraph(m_points);
            m_graph.AddEdge(m_points[0], m_points[2]);
            m_graph.AddEdge(m_points[2], m_points[1]);
            m_graph.AddEdge(m_points[1], m_points[3]);
            m_graph.AddEdge(m_points[3], m_points[0]);

            m_complete4 = new AdjacencyListGraph(m_points);
            m_complete4.MakeComplete();

            expTSP = 1 + Mathf.Sqrt(5) + Mathf.Sqrt(1.25f) + Mathf.Sqrt(3.25f);
        }

        [Test]
        public void IsHamiltonianTest()
        {
            Assert.True(TSP.IsHamiltonian(m_graph));
            Assert.False(TSP.IsHamiltonian(m_complete4));

            // check edge case with <= 1 vertex
            var smallGraph = new AdjacencyListGraph();
            smallGraph.AddVertex(new Vertex());
            Assert.IsTrue(TSP.IsHamiltonian(smallGraph));
        }

        [Test]
        public void ComputeTSPLengthTest()
        {
            var tsp = TSP.ComputeTSPLength(m_graph);
            Assert.AreEqual(expTSP, tsp, MathUtil.EPS);

            Assert.Throws<GeomException>(() => TSP.ComputeTSPLength(m_complete4));
        }

        [Test]
        public void FindTSPLengthVerticesTest()
        {
            var tsp = TSP.FindTSPLength(m_graph.Vertices);
            Assert.AreEqual(expTSP, tsp, MathUtil.EPS);
        }
    }
}
