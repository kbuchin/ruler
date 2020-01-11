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
            var m_topVertex = new Vector2(1, 1);
            var m_botVertex = new Vector2(0, -1);
            var m_leftVertex = new Vector2(-1, 0);
            var m_rightVertex = new Vector2(1, 0);
            var m_farRightVertex = new Vector2(2, 0);

            arrowPoly = new Polygon2D(
                new List<Vector2>()
                {
                    m_topVertex,
                    m_farRightVertex,
                    m_botVertex,
                    m_rightVertex
                });

            diamondPoly = new Polygon2D(
                new List<Vector2>()
                {
                    m_topVertex,
                    m_rightVertex,
                    m_botVertex,
                    m_leftVertex
                });

            LShape = new Polygon2D(
                new List<Vector2>()
                {
                    new Vector2(0, 0),
                    new Vector2(0, 4),
                    new Vector2(4, 4),
                    new Vector2(4, 2),
                    new Vector2(2, 2),
                    new Vector2(2, 0)
                });
        }

        [Test]
        public void VisibleToEachOtherTest()
        {
            var polygon = arrowPoly;

            var vertex1 = polygon.Vertices.First();
            var vertex2 = polygon.Vertices.ElementAt(2);

            bool canSeeEachOther =
                NaiveLighthouseToLighthouseVisibility.VisibleToOtherVertex(
                    vertex1,
                    vertex2,
                    polygon);

            Assert.IsFalse(canSeeEachOther);
        }

        [Test]
        public void VisibleToEachOtherTest1()
        {
            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    new Vector2(0, 0),
                    new Vector2(0, 4),
                    new Vector2(4, 4),
                    new Vector2(6, 4),
                    new Vector2(8, 4),
                    new Vector2(10, 4),
                    new Vector2(10, 2),
                    new Vector2(8, 2),
                    new Vector2(6, 2),
                    new Vector2(4, 2),
                    new Vector2(2, 2),
                    new Vector2(2, 0)
                });

            var vertex1 = polygon.Vertices.First();
            var vertex2 = polygon.Vertices.ElementAt(1);

            bool canSeeEachOther =
                NaiveLighthouseToLighthouseVisibility.VisibleToOtherVertex(
                    vertex1,
                    vertex2,
                    polygon);

            Assert.IsTrue(canSeeEachOther);
        }


        [Test]
        public void VisibleToEachOtherTest2()
        {
            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    new Vector2(0, 0),
                    new Vector2(0, 4),
                    new Vector2(4, 4),
                    new Vector2(6, 4),
                    new Vector2(8, 4),
                    new Vector2(10, 4),
                    new Vector2(10, 2),
                    new Vector2(8, 2),
                    new Vector2(6, 2),
                    new Vector2(4, 2),
                    new Vector2(2, 2),
                    new Vector2(2, 0)
                });

            var vertex1 = new Vector2(0, 4);
            var vertex2 = new Vector2(10, 4);

            bool canSeeEachOther =
                NaiveLighthouseToLighthouseVisibility.VisibleToOtherVertex(
                    vertex1,
                    vertex2,
                    polygon);

            Assert.IsTrue(canSeeEachOther);
        }

        [Test]
        public void VisibleToEachOtherTest3()
        {
            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    new Vector2(0, 0),
                    new Vector2(0, 4),
                    new Vector2(4, 4),
                    new Vector2(6, 4),
                    new Vector2(8, 4),
                    new Vector2(10, 4),
                    new Vector2(10, 2),
                    new Vector2(8, 2),
                    new Vector2(6, 2),
                    new Vector2(4, 2),
                    new Vector2(2, 2),
                    new Vector2(2, 0)
                });

            var vertex1 = new Vector2(0, 0);
            var vertex2 = new Vector2(10, 2);

            bool canSeeEachOther =
                NaiveLighthouseToLighthouseVisibility.VisibleToOtherVertex(
                    vertex1,
                    vertex2,
                    polygon);

            Assert.IsFalse(canSeeEachOther);
        }

        [Test]
        public void VisibleToEachOtherTest4()
        {
            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    new Vector2(0, 0),
                    new Vector2(0, 4),
                    new Vector2(4, 4),
                    new Vector2(6, 4),
                    new Vector2(8, 4),
                    new Vector2(10, 4),
                    new Vector2(10, 2),
                    new Vector2(8, 2),
                    new Vector2(6, 2),
                    new Vector2(4, 2),
                    new Vector2(2, 2),
                    new Vector2(2, 0)
                });

            var vertex1 = new Vector2(0, 0);

            var otherVertexes = new List<Vector2>()
            {
                new Vector2(10, 2),
                new Vector2(0, 4)
            };

            bool canSeeEachOther =
                NaiveLighthouseToLighthouseVisibility.VisibleToOtherVertex(
                    vertex1,
                    otherVertexes,
                    polygon);

            Assert.IsTrue(canSeeEachOther);
        }

        [Test]
        public void VisibleToEachOtherTest5()
        {
            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    new Vector2(0, 0),
                    new Vector2(0, 4),
                    new Vector2(4, 4),
                    new Vector2(6, 4),
                    new Vector2(8, 4),
                    new Vector2(10, 4),
                    new Vector2(10, 2),
                    new Vector2(8, 2),
                    new Vector2(6, 2),
                    new Vector2(4, 2),
                    new Vector2(2, 2),
                    new Vector2(2, 0)
                });

            var vertex1 = new Vector2(0, 0);

            var otherVertexes = new List<Vector2>()
            {
                new Vector2(10, 2),
                new Vector2(0, 4)
            };

            bool canSeeEachOther =
                NaiveLighthouseToLighthouseVisibility.VisibleToOtherVertex(
                    vertex1,
                    otherVertexes,
                    polygon);

            Assert.IsTrue(canSeeEachOther);
        }


        [Test]
        public void VisibleToEachOtherTest6()
        {
            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    new Vector2(0, 0),
                    new Vector2(0, 4),
                    new Vector2(4, 4),
                    new Vector2(6, 4),
                    new Vector2(8, 4),
                    new Vector2(10, 4),
                    new Vector2(10, 2),
                    new Vector2(8, 2),
                    new Vector2(6, 2),
                    new Vector2(4, 2),
                    new Vector2(2, 2),
                    new Vector2(2, 0)
                });

            var vertexes = new List<Vector2>()
            {
                new Vector2(0, 0),
                new Vector2(10, 2),
                new Vector2(0, 4)
            };

            bool canSeeEachOther =
                NaiveLighthouseToLighthouseVisibility.VisibleToOtherVertex(
                    vertexes,
                    polygon);

            Assert.IsTrue(canSeeEachOther);
        }

        [Test]
        public void VisibleToEachOtherTest7()
        {
            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    new Vector2(0, 0),
                    new Vector2(0, 4),
                    new Vector2(4, 4),
                    new Vector2(6, 4),
                    new Vector2(8, 4),
                    new Vector2(10, 4),
                    new Vector2(10, 2),
                    new Vector2(8, 2),
                    new Vector2(6, 2),
                    new Vector2(4, 2),
                    new Vector2(2, 2),
                    new Vector2(2, 0)
                });

            var vertexes = new List<Vector2>()
            {
                new Vector2(0, 0),
                new Vector2(10, 2),
                new Vector2(8, 2),
                new Vector2(6, 2),
                new Vector2(4, 2),
            };

            bool canSeeEachOther =
                NaiveLighthouseToLighthouseVisibility.VisibleToOtherVertex(
                    vertexes,
                    polygon);

            Assert.IsFalse(canSeeEachOther);
        }

        [Test]
        public void VisibleToOtherVertexesTest1()
        {
            var m_topVertex = new Vector2(1, 1);
            var m_botVertex = new Vector2(0, -1);
            var m_rightVertex = new Vector2(1, 0);
            var m_farRightVertex = new Vector2(2, 0);

            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    m_topVertex,
                    m_farRightVertex,
                    m_botVertex,
                    m_rightVertex
                });

            var vertexes = new List<Vector2>()
            {
                m_botVertex
            };

            var visibleVertexes = new List<Vector2>();

            var vertex = m_topVertex;

            var actual =
                NaiveLighthouseToLighthouseVisibility.VisibleToOtherVertices(
                    vertex,
                    vertexes,
                    polygon);

            Assert.AreEqual(visibleVertexes.Count, actual.Count);
        }


        [Test]
        public void VisibleToOtherVertexesTest2()
        {
            var m_topVertex = new Vector2(1, 1);
            var m_botVertex = new Vector2(0, -1);
            var m_rightVertex = new Vector2(1, 0);
            var m_farRightVertex = new Vector2(2, 0);

            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    m_topVertex,
                    m_farRightVertex,
                    m_botVertex,
                    m_rightVertex
                });

            var vertexes = new List<Vector2>()
            {
                m_farRightVertex,
                m_botVertex,
                m_rightVertex
            };

            var visibleVertexes = new List<Vector2>()
            {
                m_farRightVertex,
                m_rightVertex
            };

            var vertex = m_topVertex;

            var actual =
                NaiveLighthouseToLighthouseVisibility.VisibleToOtherVertices(
                    vertex,
                    vertexes,
                    polygon);

            Assert.AreEqual(visibleVertexes.Count, actual.Count);
        }

        [Test]
        public void VisibleToOtherVertexesTest3()
        {
            var m_topVertex = new Vector2(1, 1);
            var m_botVertex = new Vector2(0, -1);
            var m_rightVertex = new Vector2(1, 0);
            var m_farRightVertex = new Vector2(2, 0);

            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    m_topVertex,
                    m_farRightVertex,
                    m_botVertex,
                    m_rightVertex
                });

            var vertexes = new List<Vector2>()
            {
                m_topVertex,
                m_botVertex,
                m_rightVertex
            };

            var actual =
                NaiveLighthouseToLighthouseVisibility.VisibleToOtherVertices(
                    vertexes,
                    polygon);

            Assert.AreEqual(1, actual[m_topVertex].Count);
            Assert.AreEqual(1, actual[m_botVertex].Count);
            Assert.AreEqual(2, actual[m_rightVertex].Count);
        }

        [Test]
        public void VisibleToOtherVertexesTest4()
        {
            var polygon = new Polygon2D(
                new List<Vector2>()
                {
                    new Vector2(0, 0),
                    new Vector2(0, 4),
                    new Vector2(4, 4),
                    new Vector2(6, 4),
                    new Vector2(8, 4),
                    new Vector2(10, 4),
                    new Vector2(10, 2),
                    new Vector2(8, 2),
                    new Vector2(6, 2),
                    new Vector2(4, 2),
                    new Vector2(2, 2),
                    new Vector2(2, 0)
                });

            var vertexes = new List<Vector2>()
            {
                new Vector2(0, 0),
                new Vector2(10, 2),
                new Vector2(8, 2),
            };

            var actual =
                NaiveLighthouseToLighthouseVisibility.VisibleToOtherVertices(
                    vertexes,
                    polygon);

            Assert.AreEqual(0, actual[new Vector2(0, 0)].Count);
            Assert.AreEqual(1, actual[new Vector2(10, 2)].Count);
            Assert.AreEqual(1, actual[new Vector2(8, 2)].Count);
        }
    }
}