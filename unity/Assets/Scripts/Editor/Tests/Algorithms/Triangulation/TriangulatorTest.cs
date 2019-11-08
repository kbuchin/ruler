namespace Util.Algorithms.Triangulation.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Geometry;
    using Util.Geometry.DCEL;
    using Util.Geometry.Polygon;
    using Util.Geometry.Triangulation;

    [TestFixture]
    public class TriangulatorTest
    {
        private readonly Vector2 m_topVertex, m_botVertex;
        private readonly Vector2 m_leftVertex, m_rightVertex;
        private readonly Vector2 m_farRightVertex;

        private readonly List<Vector2> m_arrowVertices;
        private readonly List<Vector2> m_diamondVertices;

        private readonly Polygon2D m_arrow;
        private readonly Polygon2D m_diamond;

        private readonly DCEL m_dcel;

        public TriangulatorTest()
        {
            m_topVertex = new Vector2(0, 1);
            m_botVertex = new Vector2(0, -1);
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

            m_arrow = new Polygon2D(m_arrowVertices);
            m_diamond = new Polygon2D(m_diamondVertices);

            var lines = new List<Line>()
            {
                new Line(-1, 3),
                new Line(2, 0)
            };
            var rect = new Rect(-5, -5, 10, 10);
            m_dcel = new DCEL(lines, rect);
        }

        [Test]
        public void ArrowTriangulationTest()
        {
            var expectedTri1 = new Triangle(m_topVertex, m_farRightVertex, m_rightVertex);
            var expectedTri2 = new Triangle(m_botVertex, m_rightVertex, m_farRightVertex);

            var result = Triangulator.Triangulate(m_arrow);
            var triangles = result.Triangles.ToList();
            Assert.AreEqual(2, triangles.Count);
            Assert.IsTrue(triangles.TrueForAll(t => t.IsClockwise()));
            Assert.IsTrue(triangles.Contains(expectedTri1));
            Assert.IsTrue(triangles.Contains(expectedTri2));
        }

        [Test]
        public void DiamondTriangulationTest()
        {
            var expectedTri1 = new Triangle(m_topVertex, m_botVertex, m_leftVertex);
            var expectedTri2 = new Triangle(m_topVertex, m_rightVertex, m_botVertex);

            var result = Triangulator.Triangulate(m_diamond);
            var triangles = result.Triangles.ToList();
            Assert.AreEqual(2, triangles.Count);
            Assert.IsTrue(triangles.TrueForAll(t => t.IsClockwise()));
            Assert.IsTrue(triangles.Contains(expectedTri1));
            Assert.IsTrue(triangles.Contains(expectedTri2));
        }

        [Test]
        public void DCELTriangulationTest()
        {
            var result = Triangulator.Triangulate(m_dcel);
            var triangles = result.Triangles.ToList();
            Assert.AreEqual(8, triangles.Count);
            Assert.IsTrue(triangles.TrueForAll(t => t.IsClockwise()));
        }

        [Test]
        public void FacesTriangulationTest()
        {
            var result = Triangulator.Triangulate(m_dcel.InnerFaces);
            var triangles = result.Triangles.ToList();
            Assert.AreEqual(8, triangles.Count);
            Assert.IsTrue(triangles.TrueForAll(t => t.IsClockwise()));
        }

        [Test]
        public void FaceTriangulationTest()
        {
            Triangulation result = new Triangulation();
            foreach (var face in m_dcel.InnerFaces)
                result.AddTriangulation(Triangulator.Triangulate(face));
            var triangles = result.Triangles.ToList();
            Assert.AreEqual(8, triangles.Count);
            Assert.IsTrue(triangles.TrueForAll(t => t.IsClockwise()));
        }
    }
}
