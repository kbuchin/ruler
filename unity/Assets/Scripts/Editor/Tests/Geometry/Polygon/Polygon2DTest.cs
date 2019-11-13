namespace Util.Geometry.Polygon.Tests
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Math;

    [TestFixture]
    class Polygon2DTest
    {
        private readonly List<Vector2> m_arrowVertices;
        private readonly List<Vector2> m_diamondVertices;

        private readonly Polygon2D m_arrow;
        private readonly Polygon2D m_diamond;

        //Series of tests build around a bug
        private readonly Polygon2D m_containsPoly;

        public Polygon2DTest()
        {
            var m_topVertex = new Vector2(0, 1);
            var m_botVertex = new Vector2(0, -1);
            var m_leftVertex = new Vector2(-1, 0);
            var m_rightVertex = new Vector2(1, 0);
            var m_farRightVertex = new Vector2(2, 0);

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

            m_containsPoly = new Polygon2D(new List<Vector2>()
            {
                new Vector2(0,0),
                new Vector2(0,4),
                new Vector2(4,4),
                new Vector2(4,2),
                new Vector2(2,2),
                new Vector2(2,0)
            });
        }

        [Test]
        public void ConstructionTest()
        {
            Assert.AreEqual(m_arrowVertices.Count, m_arrow.VertexCount);
            Assert.AreEqual(m_diamondVertices.Count, m_diamond.VertexCount);
            Assert.AreEqual(6, m_containsPoly.VertexCount);
        }

        [Test]
        public void NextTest()
        {
            Assert.AreEqual(m_arrowVertices[1], m_arrow.Next(m_arrowVertices[0]));
            Assert.AreEqual(m_arrowVertices[0], m_arrow.Next(m_arrowVertices[3]));
            Assert.AreEqual(m_diamondVertices[0], m_diamond.Next(m_diamondVertices[3]));
        }

        [Test]
        public void PrevTest()
        {
            Assert.AreEqual(m_arrowVertices[0], m_arrow.Prev(m_arrowVertices[1]));
            Assert.AreEqual(m_arrowVertices[3], m_arrow.Prev(m_arrowVertices[0]));
            Assert.AreEqual(m_diamondVertices[3], m_diamond.Prev(m_diamondVertices[0]));
        }

        [Test]
        public void AddVertexTest()
        {
            var poly = new Polygon2D(m_arrowVertices);
            var pos = new Vector2(0, 1);
            poly.AddVertex(pos);
            Assert.AreEqual(5, poly.VertexCount);
            Assert.AreEqual(pos, poly.Vertices.ToList()[4]);
        }

        [Test]
        public void AddVertexAfterTest()
        {
            var poly = new Polygon2D(m_arrowVertices);
            var pos = new Vector2(0, 1);
            poly.AddVertexAfter(pos, m_arrowVertices[0]);
            Assert.AreEqual(5, poly.VertexCount);
            Assert.AreEqual(pos, poly.Vertices.ToList()[1]);

            Assert.Throws<ArgumentException>(() => poly.AddVertexAfter(pos, new Vector2(-99, 99)));
        }

        [Test]
        public void BoundingBoxTest()
        {
            var exp = new Rect(0, -1, 2, 2);
            Assert.AreEqual(exp, m_arrow.BoundingBox());
            exp = new Rect(-1, -2, 4, 4);
            Assert.AreEqual(exp, m_arrow.BoundingBox(1f));
        }

        [Test]
        public void ContainsInsideTest()
        {
            Assert.IsTrue(m_arrow.ContainsInside(new Vector2(1.5f, 0)));
            Assert.IsFalse(m_arrow.ContainsInside(new Vector2(0, 0)));
            Assert.IsTrue(m_diamond.ContainsInside(new Vector2(0, 0)));
            Assert.IsFalse(m_diamond.ContainsInside(new Vector2(-999, 999)));
            Assert.IsTrue(m_containsPoly.ContainsInside(new Vector2(1, 3)));
            Assert.IsFalse(m_containsPoly.ContainsInside(new Vector2(3, 1)));
        }

        [Test]
        public void ContainsVertexTest()
        {
            Assert.IsTrue(m_arrow.ContainsVertex(m_arrowVertices[0]));
            Assert.IsTrue(m_arrow.ContainsVertex(m_arrowVertices[1]));
            Assert.IsTrue(m_arrow.ContainsVertex(m_arrowVertices[2]));
            Assert.IsFalse(m_arrow.ContainsVertex(m_diamondVertices[3]));
        }

        [Test]
        public void RemoveVertexTest()
        {
            var poly = new Polygon2D(m_arrowVertices);
            poly.RemoveVertex(m_arrowVertices[1]);
            Assert.AreEqual(3, poly.VertexCount);
            Assert.IsFalse(poly.ContainsVertex(m_arrowVertices[1]));
        }

        [Test]
        public void RemoveFirstTest()
        {
            var poly = new Polygon2D(m_arrowVertices);
            poly.RemoveFirst();
            Assert.AreEqual(3, poly.VertexCount);
            Assert.IsFalse(poly.ContainsVertex(m_arrowVertices[0]));
        }

        [Test]
        public void RemoveLastTest()
        {
            var poly = new Polygon2D(m_arrowVertices);
            poly.RemoveLast();
            Assert.AreEqual(3, poly.VertexCount);
            Assert.IsFalse(poly.ContainsVertex(m_arrowVertices[3]));
        }

        [Test]
        public void AreaTest()
        {
            Assert.AreEqual(1, m_arrow.Area, MathUtil.EPS);
            Assert.AreEqual(2, m_diamond.Area, MathUtil.EPS);
            Assert.AreEqual(12, m_containsPoly.Area, MathUtil.EPS);
        }

        [Test]
        public void ClearTest()
        {
            var poly = new Polygon2D(m_arrowVertices);
            poly.Clear();
            Assert.AreEqual(0, poly.VertexCount);
            Assert.IsEmpty(poly.Vertices);
        }

        [Test]
        public void SegmentsTest()
        {
            var segs = (System.Collections.ICollection)m_arrow.Segments;
            Assert.AreEqual(4, segs.Count);
            Assert.Contains(new LineSegment(m_arrowVertices[0], m_arrowVertices[1]), segs);
            Assert.Contains(new LineSegment(m_arrowVertices[1], m_arrowVertices[2]), segs);
            Assert.Contains(new LineSegment(m_arrowVertices[2], m_arrowVertices[3]), segs);
            Assert.Contains(new LineSegment(m_arrowVertices[3], m_arrowVertices[0]), segs);
        }

        [Test]
        public void IsConvexTest()
        {
            Assert.IsTrue(m_diamond.IsConvex());
            Assert.IsFalse(m_arrow.IsConvex());
            Assert.IsFalse(m_containsPoly.IsConvex());
        }

        [Test]
        public void ClockwiseTest()
        {
            Assert.IsTrue(m_arrow.IsClockwise());
            var poly = new Polygon2D(m_arrowVertices);
            poly.Reverse();
            Assert.IsFalse(poly.IsClockwise());
        }

        [Test]
        public void AddVertexFirstTest()
        {
            var poly = new Polygon2D(m_arrowVertices);
            var pos = new Vector2(-5, 5);
            poly.AddVertexFirst(pos);
            Assert.AreEqual(5, poly.VertexCount);
            var vertices = (System.Collections.ICollection)poly.Vertices;
            Assert.Contains(pos, vertices);
        }
    }
}
