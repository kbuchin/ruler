namespace ArtGallery.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NUnit.Framework;
    using UnityEngine;
    using Util.Geometry;
    using Util.Geometry.Polygon;
    using Util.Math;
    using Util.Algorithms.Polygon;

    [TestFixture]
    public class NaiveLighthouseToLighthouseVisibilityTest
    {

        private readonly Polygon2D arrowPoly;
        private readonly Polygon2D diamondPoly;
        private readonly Polygon2D LShape;

        public NaiveLighthouseToLighthouseVisibilityTest()
        {
            var m_topVertex = new Vector2(0, 1);
            var m_botVertex = new Vector2(0, -1);
            var m_leftVertex = new Vector2(-1, 0);
            var m_rightVertex = new Vector2(1, 0);
            var m_farRightVertex = new Vector2(2, 0);

            arrowPoly = new Polygon2D(new List<Vector2>()
            {
                m_topVertex, m_farRightVertex, m_botVertex, m_rightVertex
            });

            diamondPoly = new Polygon2D(new List<Vector2>()
            {
                m_topVertex, m_rightVertex, m_botVertex, m_leftVertex
            });

            LShape = new Polygon2D(new List<Vector2>()
            {
                new Vector2(0,0), new Vector2(0,4), new Vector2(4,4),
                new Vector2(4,2), new Vector2(2,2), new Vector2(2,0)
            });
        }

        [Test]
        public void VisibleVerticesTest1()
        {
            foreach (var vertex in diamondPoly.Vertices)
            {
                var actual = NaiveLighthouseToLighthouseVisibility.VisibleVertices(
                    diamondPoly,
                    vertex);

                Assert.AreEqual(diamondPoly.VertexCount, actual.Count);
            }
        }

        [Test]
        public void VisibleVerticesTest2()
        {
            var vertex = arrowPoly.Vertices.First();
            var actual = NaiveLighthouseToLighthouseVisibility.VisibleVertices(
                arrowPoly,
                vertex);

            Assert.AreEqual(arrowPoly.VertexCount - 1, actual.Count);
        }

        [Test]
        public void VisibleVerticesTest3()
        {
            var vertex = LShape.Vertices.First();
            var actual = NaiveLighthouseToLighthouseVisibility.VisibleVertices(
                LShape,
                vertex);

            Assert.AreEqual(LShape.VertexCount - 1, actual.Count);
        }

        [Test]
        public void VisibleVerticesTest4()
        {
            var polygon = new Polygon2D(new List<Vector2>()
            {
                new Vector2(0,0), new Vector2(0,4), new Vector2(4,4),
                new Vector2(6,4), new Vector2(8,4), new Vector2(10,4),
                new Vector2(10,2), new Vector2(8,2), new Vector2(6,2),
                new Vector2(4,2), new Vector2(2,2), new Vector2(2,0)
            });

            int visibleVertexes = 5;
            var vertex = polygon.Vertices.First();
            var actual = NaiveLighthouseToLighthouseVisibility.VisibleVertices(
                polygon,
                vertex);

            Assert.AreEqual(visibleVertexes, actual.Count);

             visibleVertexes = polygon.Vertices.Count;
             vertex = polygon.Vertices.ElementAt(1);
             actual = NaiveLighthouseToLighthouseVisibility.VisibleVertices(
                polygon,
                vertex);

            Assert.AreEqual(visibleVertexes, actual.Count);

        }
    }
}
