namespace Util.Geometry.Graph.Tests
{
    using UnityEngine;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;
    using Util.Algorithms.Graph;
    using Util.Math;

    [TestFixture]
    public class GraphTest
    {
        private readonly List<Vertex> m_complete4pos;
        private readonly IGraph m_complete4;
        private readonly List<Vertex> m_level2pos;

        private static float eps = MathUtil.EPS;

        public GraphTest()
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

        [Test]
        public void Complete4Test()
        {
            Assert.AreEqual(4, m_complete4.VertexCount);
            Assert.AreEqual(6, m_complete4.EdgeCount);
        }

        [Test]
        public void CompleteContainsEdgeTest()
        {
            foreach (var e in m_complete4.Edges)
            {
                Assert.True(m_complete4.ContainsEdge(e));
            }
            foreach (var u in m_complete4pos)
            {
                foreach (var v in m_complete4pos)
                {
                    if (u == v) continue;
                    Assert.True(m_complete4.ContainsEdge(u, v));
                }
            }            
        }

        [Test]
        public void GraphLengthTest()
        {
            var expected = (4 + 2 * Mathf.Sqrt(2));
            Assert.AreEqual(expected, m_complete4.TotalEdgeLength, eps);
        }


        [Test]
        public void EqualGraphTest()
        {
            var otherGraph = new AdjacencyListGraph();

            var lv2complete = new AdjacencyListGraph(m_level2pos);
            lv2complete.MakeComplete();

            Assert.True(m_complete4.Equals(m_complete4));

            Assert.False(otherGraph.Equals(m_complete4));

            // Graph on the same vertices
            Assert.False(lv2complete.Equals(otherGraph));

            //Same graphs on different vertex set 
            Assert.True(lv2complete.Equals(lv2complete));

        }
    }
}

