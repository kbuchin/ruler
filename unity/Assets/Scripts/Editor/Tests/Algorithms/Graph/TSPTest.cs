namespace Util.Algorithms.Graph.Tests
{
    using UnityEngine;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Util.Math;
    using Util.Geometry.Graph;
    using Util.Algorithms.Graph;

    [TestFixture]
    class TSPTest
    {
        private readonly List<Vertex> m_complete4pos;
        private readonly IGraph m_complete4;

        public TSPTest()
        {
            m_complete4pos = new List<Vertex>() {
                new Vertex(0, 0),
                new Vertex(1, 0),
                new Vertex(0, 1),
                new Vertex(1, 1)
            };
            m_complete4 = new AdjacencyListGraph(m_complete4pos);
            m_complete4.MakeComplete();
        }
        [Test]
        public void IsHamiltonianTest()
        {
            var graph = new AdjacencyListGraph(m_complete4pos.Take(3).ToList());
            graph.MakeComplete();
            Assert.True(TSP.IsHamiltonian(graph));

            Assert.False(TSP.IsHamiltonian(m_complete4));
        }
    }
}
