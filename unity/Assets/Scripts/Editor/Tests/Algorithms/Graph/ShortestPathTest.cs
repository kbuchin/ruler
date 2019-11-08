namespace Util.Algorithms.Graph.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Geometry.Graph;
    using Util.Math;

    [TestFixture]
    public class ShortestPathTest
    {
        private readonly List<Vertex> vertices;

        private readonly IGraph m_graph;
        private readonly IGraph m_graphDirected;

        public ShortestPathTest()
        {
            m_graph = new AdjacencyListGraph();
            m_graphDirected = new AdjacencyListGraph(new GraphType(true, true));

            vertices = new List<Vertex>()
            {
                new Vertex(0, 0),
                new Vertex(1, 0),
                new Vertex(1, 1),
                new Vertex(2, 2),
                new Vertex(2, 3),
                new Vertex(-1, -2)
            };

            foreach (var v in vertices)
            {
                m_graph.AddVertex(v);
                m_graphDirected.AddVertex(v);
            }

            m_graph.AddEdge(vertices[0], vertices[1]);
            m_graph.AddEdge(vertices[1], vertices[2]);
            m_graph.AddEdge(vertices[0], vertices[2]);
            m_graph.AddEdge(vertices[2], vertices[3]);
            m_graph.AddEdge(vertices[1], vertices[3]);
            m_graph.AddEdge(vertices[3], vertices[4]);
            m_graph.AddEdge(vertices[0], vertices[5]);

            m_graphDirected.AddEdge(vertices[0], vertices[1]);
            m_graphDirected.AddEdge(vertices[1], vertices[2]);
            m_graphDirected.AddEdge(vertices[0], vertices[2]);
            m_graphDirected.AddEdge(vertices[2], vertices[3]);
            m_graphDirected.AddEdge(vertices[1], vertices[3]);
            m_graphDirected.AddEdge(vertices[3], vertices[4]);
            m_graphDirected.AddEdge(vertices[0], vertices[5]);
        }

        [Test]
        public void ShortestDistanceTest1()
        {
            var dis = ShortestPath.ShorthestDistance(m_graph, vertices[0], vertices[4]);
            var exp = Vector2.Distance(vertices[0].Pos, vertices[2].Pos) +
                Vector2.Distance(vertices[2].Pos, vertices[3].Pos) +
                Vector2.Distance(vertices[3].Pos, vertices[4].Pos);
            Assert.AreEqual(exp, dis, MathUtil.EPS);

            // check if opposite direction is equal (undirected graph)
            var dis2 = ShortestPath.ShorthestDistance(m_graph, vertices[4], vertices[0]);
            Assert.AreEqual(dis, dis2, MathUtil.EPS);
        }

        [Test]
        public void ShortestDistanceTest2()
        {
            var dis = ShortestPath.ShorthestDistance(m_graph, vertices[3], vertices[5]);
            var exp = Vector2.Distance(vertices[3].Pos, vertices[2].Pos) +
                Vector2.Distance(vertices[2].Pos, vertices[0].Pos) +
                Vector2.Distance(vertices[0].Pos, vertices[5].Pos);
            Assert.AreEqual(exp, dis, MathUtil.EPS);

            // check if opposite direction is equal (undirected graph)
            var dis2 = ShortestPath.ShorthestDistance(m_graph, vertices[5], vertices[3]);
            Assert.AreEqual(dis, dis2, MathUtil.EPS);
        }

        [Test]
        public void ShortestDistanceTestDirected()
        {
            var dis = ShortestPath.ShorthestDistance(m_graphDirected, vertices[0], vertices[4]);
            var exp = Vector2.Distance(vertices[0].Pos, vertices[2].Pos) +
                Vector2.Distance(vertices[2].Pos, vertices[3].Pos) +
                Vector2.Distance(vertices[3].Pos, vertices[4].Pos);
            Assert.AreEqual(exp, dis, MathUtil.EPS);

            var dis2 = ShortestPath.ShorthestDistance(m_graphDirected, vertices[4], vertices[0]);
            Assert.AreEqual(float.PositiveInfinity, dis2);
        }

        [Test]
        public void ShortestPathTest1()
        {
            var path = ShortestPath.ShorthestPath(m_graph, vertices[0], vertices[4]).ToList();
            var exp = new List<Vertex>() { vertices[0], vertices[2], vertices[3], vertices[4] };
            Assert.AreEqual(exp, path);
        }
    }
}
