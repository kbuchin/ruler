namespace Util.Algorithms.DCEL.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Algorithms.Triangulation;

    [TestFixture]
    public class VoronoiTest
    {
        private readonly Vector2 m_topVertex, m_botVertex;
        private readonly Vector2 m_leftVertex, m_rightVertex;
        private readonly Vector2 m_farRightVertex, m_weirdVertex;

        private readonly List<Vector2> m_arrowVertices;
        private readonly List<Vector2> m_allVertices;

        public VoronoiTest()
        {
            m_topVertex = new Vector2(0, 2);
            m_botVertex = new Vector2(0, -2);
            m_leftVertex = new Vector2(-1, 0);
            m_rightVertex = new Vector2(1, 0);
            m_farRightVertex = new Vector2(2, 0);
            m_weirdVertex = new Vector2(0, 0.5f);

            m_arrowVertices = new List<Vector2>()
            {
                m_topVertex, m_farRightVertex, m_botVertex, m_rightVertex
            };
            m_allVertices = new List<Vector2>()
            {
                m_topVertex, m_rightVertex, m_botVertex, m_leftVertex, m_farRightVertex, m_weirdVertex
            };
        }

        [Test]
        public void VoronoiFromDelaunayTest1()
        {
            var delaunay = Delaunay.Create(m_arrowVertices);
            var voronoi = Voronoi.Create(delaunay);

            Assert.AreEqual(3, voronoi.VertexCount);
            Assert.AreEqual(3, voronoi.EdgeCount);
            Assert.AreEqual(2, voronoi.FaceCount);
        }

        [Test]
        public void VoronoiFromDelaunayTest2()
        {
            var delaunay = Delaunay.Create(m_allVertices);
            var voronoi = Voronoi.Create(delaunay);

            Assert.AreEqual(6, voronoi.VertexCount);
            Assert.AreEqual(7, voronoi.EdgeCount);
            Assert.AreEqual(3, voronoi.FaceCount);
        }

        [Test]
        public void VoronoiFromVerticesTest1()
        {
            var voronoi = Voronoi.Create(m_arrowVertices);

            Assert.AreEqual(3, voronoi.VertexCount);
            Assert.AreEqual(3, voronoi.EdgeCount); // take into account halfedges
            Assert.AreEqual(2, voronoi.FaceCount);
        }

        [Test]
        public void VoronoiFromVerticesTest2()
        {
            var voronoi = Voronoi.Create(m_allVertices);

            Assert.AreEqual(6, voronoi.VertexCount);
            Assert.AreEqual(7, voronoi.EdgeCount); // take into account halfedges
            Assert.AreEqual(3, voronoi.FaceCount);
        }
    }
}
