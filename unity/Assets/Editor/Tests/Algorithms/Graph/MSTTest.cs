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
    public class MSTTest
    {
        private readonly List<Vertex> m_complete4pos;
        private readonly IGraph m_complete4;
        private readonly List<Vertex> m_level2pos;

        private static float eps = MathUtil.EPS;

        public MSTTest()
        {
            m_complete4pos = new List<Vertex>() {
                new Vertex(0, 0),
                new Vertex(1, 0),
                new Vertex(0, 1),
                new Vertex(1, 1)
            };
            m_complete4 = new AdjacencyListGraph(m_complete4pos);
            m_complete4.MakeComplete();

            m_level2pos = new List<Vertex>() {
                new Vertex(1.4f, -1.1f),
                new Vertex(1.3f, -0.2f),
                new Vertex(0.8f, 0.7f),
                new Vertex(-0.2f, 1f),
                new Vertex(-1.2f, 1.2f)
            };
        }

        private IGraph MakeLevel2MST()
        {
            IGraph mst = new AdjacencyListGraph(m_level2pos);
            mst.MakeComplete();
            mst = MST.MinimumSpanningTree(mst);
            return mst;
        }


        [Test]
        public void MSTWeightTest()
        {
            var mst = MakeLevel2MST();
            var level2vertices = mst.Vertices.ToList();
            var cost = new Edge(level2vertices[0], level2vertices[1]).Weight +
                       new Edge(level2vertices[2], level2vertices[1]).Weight +
                       new Edge(level2vertices[2], level2vertices[3]).Weight +
                       new Edge(level2vertices[3], level2vertices[4]).Weight;

            Assert.AreEqual(cost, mst.TotalEdgeWeight);
        }

        [Test]
        public void Level2MSTTest()
        {
            var level2mst = MakeLevel2MST();

            var expected = new AdjacencyListGraph(m_level2pos);
            var level2vertices = expected.Vertices.ToList();
            for (int i = 0; i < level2vertices.Count - 1; i++)
            {
                expected.AddEdge(level2vertices[i], level2vertices[i + 1]);
            }

            Assert.True(expected.Equals(level2mst));
        }

        [Test]
        public void ContainsEdge()
        {
            var level2mst = MakeLevel2MST();

            var level2vertices = level2mst.Vertices.ToList();

            Assert.True(level2mst.ContainsEdge(new Edge(level2vertices[0], level2vertices[1])));
            Assert.True(level2mst.ContainsEdge(new Edge(level2vertices[2], level2vertices[1])));
            Assert.True(level2mst.ContainsEdge(new Edge(level2vertices[2], level2vertices[3])));
            Assert.True(level2mst.ContainsEdge(new Edge(level2vertices[3], level2vertices[4])));

            Assert.False(level2mst.ContainsEdge(new Edge(level2vertices[0], level2vertices[2])));
            Assert.False(level2mst.ContainsEdge(new Edge(level2vertices[3], level2vertices[1])));
            Assert.False(level2mst.ContainsEdge(new Edge(level2vertices[4], level2vertices[1])));
            Assert.False(level2mst.ContainsEdge(new Edge(level2vertices[4], level2vertices[2])));
            Assert.False(level2mst.ContainsEdge(new Edge(level2vertices[1], level2vertices[3])));
        }
    }
}
