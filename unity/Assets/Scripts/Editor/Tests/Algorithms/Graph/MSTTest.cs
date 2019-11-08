namespace Util.Algorithms.Graph.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;
    using Util.Algorithms.Graph;
    using Util.Geometry.Graph;
    using Util.Math;

    [TestFixture]
    public class MSTTest
    {
        private readonly IGraph m_graph;
        private readonly List<Vertex> m_level1pos, m_level2pos;

        public MSTTest()
        {
            m_level1pos = new List<Vertex>() {
                new Vertex(0f,   0f),
                new Vertex(.5f, .5f),
                new Vertex(0f, 1.5f),
                new Vertex(.5f,  1f)
            };
            m_graph = new AdjacencyListGraph(m_level1pos);
            m_graph.MakeComplete();
            m_graph.RemoveEdges(m_level1pos[1], m_level1pos[3]);

            m_level2pos = new List<Vertex>() {
                new Vertex(1.4f, -1.1f),
                new Vertex(1.3f,  -.2f),
                new Vertex(.8f,    .7f),
                new Vertex(-.2f,    1f),
                new Vertex(-1.2f, 1.2f)
            };
        }

        [Test]
        public void MSTWeightTest()
        {
            var mst = MST.MinimumSpanningTree(m_level2pos);
            var level2vertices = mst.Vertices.ToList();
            var cost = new Edge(level2vertices[0], level2vertices[1]).Weight +
                       new Edge(level2vertices[2], level2vertices[1]).Weight +
                       new Edge(level2vertices[2], level2vertices[3]).Weight +
                       new Edge(level2vertices[3], level2vertices[4]).Weight;

            Assert.AreEqual(cost, mst.TotalEdgeWeight, MathUtil.EPS);
        }

        [Test]
        public void Level2MSTTest()
        {
            var level2mst = MST.MinimumSpanningTree(m_level2pos);

            var expected = new AdjacencyListGraph(m_level2pos);
            var level2vertices = expected.Vertices.ToList();
            for (int i = 0; i < level2vertices.Count - 1; i++)
            {
                expected.AddEdge(level2vertices[i], level2vertices[i + 1]);
            }

            Assert.True(expected.Equals(level2mst));
        }

        [Test]
        public void ContainsEdgeTest()
        {
            var level2mst = MST.MinimumSpanningTree(m_level2pos);

            var level2vertices = level2mst.Vertices.ToList();

            Assert.True(level2mst.ContainsEdge(level2vertices[0], level2vertices[1]));
            Assert.True(level2mst.ContainsEdge(level2vertices[2], level2vertices[1]));
            Assert.True(level2mst.ContainsEdge(level2vertices[2], level2vertices[3]));
            Assert.True(level2mst.ContainsEdge(level2vertices[3], level2vertices[4]));

            Assert.False(level2mst.ContainsEdge(level2vertices[0], level2vertices[2]));
            Assert.False(level2mst.ContainsEdge(level2vertices[3], level2vertices[1]));
            Assert.False(level2mst.ContainsEdge(level2vertices[4], level2vertices[1]));
            Assert.False(level2mst.ContainsEdge(level2vertices[4], level2vertices[2]));
            Assert.False(level2mst.ContainsEdge(level2vertices[1], level2vertices[3]));
        }

        [Test]
        public void NotContainsEdgeTest()
        {
            var mst = MST.MinimumSpanningTree(m_graph);

            Assert.False(mst.ContainsEdge(m_level1pos[1], m_level1pos[2]));
        }
    }
}
