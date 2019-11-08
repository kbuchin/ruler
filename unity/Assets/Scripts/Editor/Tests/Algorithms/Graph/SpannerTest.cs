namespace Util.Algorithms.Graph.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using Util.Algorithms.Graph;
    using Util.Geometry.Graph;

    [TestFixture]
    public class SpannerTest
    {
        private readonly List<Vertex> m_complete4pos;
        private readonly IGraph m_complete4;

        public SpannerTest()
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
        public void GreedySpannerCompleteTest()
        {
            Assert.True(m_complete4.Equals(Spanner.GreedySpanner(m_complete4pos, 1)));

            //large test case
            var pos = new List<Vertex> {  new Vertex(9,3), new Vertex(5,6), new Vertex(4,7), new Vertex(-2, 5), new Vertex(6,-3), new Vertex(23,3), new Vertex(22,4),
                                           new Vertex (9.5f,-3.4f), new Vertex (5.5f, -6.3f), new Vertex(-4.5f,7.4f), new Vertex(-2.5f, -5.3f), new Vertex(6.5f,-3.3f), new Vertex(23.5f, -4.3f), new Vertex(22.5f,-5.3f)};
            var completegraph = new AdjacencyListGraph(pos);
            completegraph.MakeComplete();
            var spanner = Spanner.GreedySpanner(pos, 1);
            Assert.True(completegraph.Equals(spanner));
        }

        [Test]
        public void GreedySpannerLineGraphTest()
        {
            //Line graph test case
            var vert = new List<Vertex> { new Vertex(0, 0), new Vertex(1, 0), new Vertex(2, 0) };

            var lineGraph = new AdjacencyListGraph(vert);

            lineGraph.AddEdge(vert[0], vert[1]);
            lineGraph.AddEdge(vert[1], vert[2]);

            var spanner = Spanner.GreedySpanner(vert, 20);

            Assert.True(lineGraph.Equals(spanner));
        }

        /// <summary>
        /// Test GreedySpanner and IsSpanner against each other
        /// </summary>
        [Test]
        public void SpannerIsSpannerTest()
        {
            var pos = new List<Vertex> {  new Vertex(9,3), new Vertex(5,6), new Vertex(4,7), new Vertex(-2, 5), new Vertex(6,-3), new Vertex(23,3), new Vertex(22,4),
                                           new Vertex (9.5f,-3.4f), new Vertex (5.5f, -6.3f), new Vertex(-4.5f,7.4f), new Vertex(-2.5f, -5.3f), new Vertex(6.5f,-3.3f), new Vertex(23.5f, -4.3f), new Vertex(22.5f,-5.3f)};
            var tlist = new List<float> { 1f, 1.1f, 1.5f, 2f, 3f, 5f, 10f };

            foreach (var t in tlist)
            {
                var spanner = Spanner.GreedySpanner(pos, t);
                Assert.True(Spanner.VerifySpanner(spanner, t).IsSpanner, "we failed for t is: " + t);
            }
        }
    }
}
