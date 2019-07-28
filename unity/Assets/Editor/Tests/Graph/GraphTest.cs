using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Util.Geometry.Graph.Tests
{
    [TestFixture]
    public class GraphTest
    {
        private List<Vertex> m_complete4pos;
        private IGraph m_complete4;
        private List<Vertex> m_level2pos;


        public GraphTest()
        {
            m_complete4pos = new List<Vertex>() {
                new Vertex(new Vector2(0, 0)),
                new Vertex(new Vector2(1, 0)),
                new Vertex(new Vector2(0, 1)),
                new Vertex(new Vector2(1, 1))
            };
            m_complete4 = new AdjacencyListGraph(m_complete4pos);
            m_complete4.MakeComplete();

            m_level2pos = new List<Vertex>() {
                new Vertex(new Vector2(1.4f, -1.1f)),
                new Vertex(new Vector2(1.3f, -0.2f)),
                new Vertex(new Vector2(0.8f, 0.7f)),
                new Vertex(new Vector2(-0.2f, 1f)),
                new Vertex(new Vector2(-1.2f, 1.2f))
            };
        }

        [Test]
        public void Complete4Test()
        {
            Assert.AreEqual(m_complete4.Vertices.Count, 4);
            Assert.AreEqual(m_complete4.Edges.Count, 6);
        }

        [Test]
        public void GraphLengthTest()
        {
            var expected = (4 + 2 * Mathf.Sqrt(2));
            Assert.AreEqual(expected, m_complete4.totalEdgeLength);
        }

        [Test]
        public void MSTTest()
        {
            var mst = new AdjacencyListGraph(m_complete4pos);
            mst.MakeComplete();
            mst.MinimumSpanningTree();
            Assert.AreEqual(3, mst.LengthOfAllEdges());
        }

        [Test]
        public void Level2MSTTest()
        {
            var level2mst = new Graph(m_level2pos);
            level2mst.MakeCompleteGraph();
            level2mst.MinimumSpanningTree();

            var expected = new Graph(m_level2pos);
            var level2vertices = expected.Vertices;
            for (int i = 0; i < level2vertices.Count -1 ; i++)
            {
                expected.AddEdge(level2vertices[i], level2vertices[i + 1]);
            }

            Assert.True(Graph.EqualGraphs(expected, level2mst));
        }

        [Test]
        public void IsEdgeTest()
        {
            var level2mst = new Graph(m_level2pos);
            level2mst.MakeCompleteGraph();
            level2mst.MinimumSpanningTree();

            var level2vertices = level2mst.Vertices;

            Assert.AreEqual(true, level2mst.IsEdge(level2vertices[0], level2vertices[1]));
            Assert.AreEqual(true, level2mst.IsEdge(level2vertices[2], level2vertices[1]));
            Assert.AreEqual(true, level2mst.IsEdge(level2vertices[2], level2vertices[3]));
            Assert.AreEqual(true, level2mst.IsEdge(level2vertices[4], level2vertices[3]));

            Assert.AreEqual(false, level2mst.IsEdge(level2vertices[0], level2vertices[2]));
            Assert.AreEqual(false, level2mst.IsEdge(level2vertices[3], level2vertices[1]));
            Assert.AreEqual(false, level2mst.IsEdge(level2vertices[4], level2vertices[1]));
            Assert.AreEqual(false, level2mst.IsEdge(level2vertices[4], level2vertices[2]));
            Assert.AreEqual(false, level2mst.IsEdge(level2vertices[1], level2vertices[3]));
        }

        [Test]
        public void EqualGraphTest()
        {
            var level2mst = new Graph(m_level2pos);
            level2mst.MakeCompleteGraph();
            level2mst.MinimumSpanningTree();

            var lv2complete = new Graph(m_level2pos);
            lv2complete.MakeCompleteGraph();

            var lv2complete2 = new Graph(m_level2pos);
            lv2complete2.MakeCompleteGraph();

            Assert.AreEqual(true, Graph.EqualGraphs(m_complete4, m_complete4));
            Assert.AreEqual(true, Graph.EqualGraphs(level2mst, level2mst));

            Assert.AreEqual(false, Graph.EqualGraphs(level2mst, m_complete4));
            Assert.AreEqual(false, Graph.EqualGraphs(m_complete4, level2mst));

            // Graph on the same vertices
            Assert.False(Graph.EqualGraphs(lv2complete, level2mst));

            //Same graphs on different vertex set 
            Assert.True(Graph.EqualGraphs(lv2complete, lv2complete2));

        }

        [Test]
        public void IsTSPTest()
        {
            var graph = new Graph(m_complete4pos.Take(3).ToList());
            graph.MakeCompleteGraph();
            Assert.True(graph.IsTSPTour());

            Assert.False(m_complete4.IsTSPTour());
        }

        [Test]
        public void SpannerTest()
        {
            Assert.True(Graph.EqualGraphs(m_complete4, Graph.GreedySpanner(m_complete4pos, 1)));


            //Line graph test case
            var vert = new List<Vertex> { new Vertex(0, 0), new Vertex(1, 0), new Vertex(2, 0) };
            var lineGraph = new Graph(vert);
            lineGraph.AddEdge(vert[0], vert[1]);
            lineGraph.AddEdge(vert[1], vert[2]);
            var spanner = Graph.GreedySpanner(vert.Select(v => v.Pos).ToList(), 20);
            Assert.True(Graph.EqualGraphs(lineGraph, spanner ));

            //large test case
            var pos = new List<Vector2> {  new Vector2(9,3), new Vector2(5,6), new Vector2(4,7), new Vector2(-2, 5), new Vector2(6,-3), new Vector2(23,3), new Vector2(22,4),
                                           new Vector2 (9.5f,-3.4f), new Vector2 (5.5f, -6.3f), new Vector2(-4.5f,7.4f), new Vector2(-2.5f, -5.3f), new Vector2(6.5f,-3.3f), new Vector2(23.5f, -4.3f), new Vector2(22.5f,-5.3f)};
            var completegraph = new Graph(pos);
            completegraph.MakeCompleteGraph();
            spanner = Graph.GreedySpanner(pos, 1);
            Assert.True(Graph.EqualGraphs(completegraph, spanner)); 
        }

        /// <summary>
        /// Test GreedySpanner and IsSpanner against each other
        /// </summary>
        [Test]
        public void SpannerIsSpannerTest()
        {
            var pos = new List<Vector2> {  new Vector2(9,3), new Vector2(5,6), new Vector2(4,7), new Vector2(-2, 5), new Vector2(6,-3), new Vector2(23,3), new Vector2(22,4),
                                           new Vector2 (9.5f,-3.4f), new Vector2 (5.5f, -6.3f), new Vector2(-4.5f,7.4f), new Vector2(-2.5f, -5.3f), new Vector2(6.5f,-3.3f), new Vector2(23.5f, -4.3f), new Vector2(22.5f,-5.3f)};
            var tlist = new List<float> { 1f, 1.1f, 1.5f, 2f, 3f, 5f, 10f };

            foreach(var t in tlist)
            {
                var spanner = Graph.GreedySpanner(pos, t);
                Assert.True(spanner.VerifySpanner(t).IsSpanner, "we failed for t is: "+t);
            }


        }
    }
}

