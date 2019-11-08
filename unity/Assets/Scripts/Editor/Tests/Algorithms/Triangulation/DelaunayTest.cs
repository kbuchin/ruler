namespace Util.Algorithms.Triangulation.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry.Polygon;
    using Util.Geometry.Triangulation;
    using Util.Math;

    [TestFixture]
    public class DelaunayTest
    {
        private readonly Vector2 m_topVertex, m_botVertex;
        private readonly Vector2 m_leftVertex, m_rightVertex;
        private readonly Vector2 m_farRightVertex;

        private readonly List<Vector2> m_arrowVertices;
        private readonly List<Vector2> m_diamondVertices;

        private readonly Polygon2D m_diamond;

        public DelaunayTest()
        {
            m_topVertex = new Vector2(0, 2);
            m_botVertex = new Vector2(0, -2);
            m_leftVertex = new Vector2(-1, 0);
            m_rightVertex = new Vector2(1, 0);
            m_farRightVertex = new Vector2(2, 0);

            m_arrowVertices = new List<Vector2>()
            {
                m_topVertex, m_farRightVertex, m_botVertex, m_rightVertex
            };
            m_diamondVertices = new List<Vector2>()
            {
                m_topVertex, m_rightVertex, m_botVertex, m_leftVertex
            };

            m_diamond = new Polygon2D(m_diamondVertices);
        }

        [Test]
        public void DelaunayTriangulation1()
        {
            var m_delaunay = Delaunay.Create(m_diamondVertices);
            Assert.IsTrue(Delaunay.IsValid(m_delaunay));
            Assert.AreEqual(m_diamond.Area, m_delaunay.Area, MathUtil.EPS);
            var triangles = (System.Collections.ICollection)m_delaunay.Triangles;
            Assert.Contains(new Triangle(m_topVertex, m_rightVertex, m_leftVertex), triangles);
            Assert.Contains(new Triangle(m_leftVertex, m_rightVertex, m_botVertex), triangles);
        }

        [Test]
        public void DelaunayTriangulation2()
        {
            var m_delaunay = Delaunay.Create(m_arrowVertices);
            Assert.IsTrue(Delaunay.IsValid(m_delaunay));
            var triangles = (System.Collections.ICollection)m_delaunay.Triangles;
            Assert.Contains(new Triangle(m_topVertex, m_rightVertex, m_botVertex), triangles);
            Assert.Contains(new Triangle(m_topVertex, m_farRightVertex, m_rightVertex), triangles);
            Assert.Contains(new Triangle(m_rightVertex, m_farRightVertex, m_botVertex), triangles);
        }

        [Test]
        public void InValidTriangulationTest()
        {
            var m_delaunay = new Triangulation();
            var t1 = new Triangle(m_topVertex, m_rightVertex, m_botVertex);
            var t2 = new Triangle(m_topVertex, m_botVertex, m_leftVertex);
            t1.E2.Twin = t2.E0;
            t2.E0.Twin = t1.E2;
            m_delaunay.AddTriangle(t1);
            m_delaunay.AddTriangle(t2);
            Assert.IsFalse(Delaunay.IsValid(m_delaunay));
        }

        [Test]
        public void DelaunayAddVertexTest()
        {
            var m_delaunay = Delaunay.Create();
            Delaunay.AddVertex(m_delaunay, m_topVertex);
            Delaunay.AddVertex(m_delaunay, m_botVertex);
            Delaunay.AddVertex(m_delaunay, m_leftVertex);
            Delaunay.AddVertex(m_delaunay, m_farRightVertex);
            Delaunay.AddVertex(m_delaunay, m_rightVertex);
            m_delaunay.RemoveInitialTriangle();
            Assert.IsTrue(Delaunay.IsValid(m_delaunay));

            // check for triangles
            var triangles = (System.Collections.ICollection)m_delaunay.Triangles;
            Assert.Contains(new Triangle(m_topVertex, m_rightVertex, m_leftVertex), triangles);
            Assert.Contains(new Triangle(m_leftVertex, m_rightVertex, m_botVertex), triangles);
            Assert.Contains(new Triangle(m_topVertex, m_farRightVertex, m_rightVertex), triangles);
            Assert.Contains(new Triangle(m_rightVertex, m_farRightVertex, m_botVertex), triangles);
        }

        [Test]
        public void TestColinearPoints()
        {
            var m_Delaunay = Delaunay.Create();
            m_Delaunay.AddVertex(new Vector2(1, 1));
            m_Delaunay.AddVertex(new Vector2(2, 2));
            m_Delaunay.AddVertex(new Vector2(3, 3));
            m_Delaunay.AddVertex(new Vector2(1, 4));
            m_Delaunay.AddVertex(new Vector2(2, 8));
            m_Delaunay.AddVertex(new Vector2(3, 12));
        }

        [Test]
        public void TestCocircularPoints()
        {
            var m_Delaunay = Delaunay.Create();
            m_Delaunay.AddVertex(new Vector2(1, 1));
            m_Delaunay.AddVertex(new Vector2(1, 2));
            m_Delaunay.AddVertex(new Vector2(2, 2));
            m_Delaunay.AddVertex(new Vector2(2, 1));
            m_Delaunay.AddVertex(new Vector2(1, 3));
            m_Delaunay.AddVertex(new Vector2(2, 3));
        }
    }
}
