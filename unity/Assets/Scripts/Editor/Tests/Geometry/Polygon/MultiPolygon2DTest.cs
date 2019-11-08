namespace Util.Geometry.Polygon.Tests
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Math;

    [TestFixture]
    public class MultiPolygon2DTest
    {
        private readonly List<Vector2> m_vertices1, m_vertices2;
        private readonly Polygon2D m_poly1, m_poly2;
        private readonly MultiPolygon2D m_multi;

        public MultiPolygon2DTest()
        {
            m_vertices1 = new List<Vector2>()
            {
                new Vector2(0, 0), new Vector2(1, 1), new Vector2(1, 0)
            };
            m_vertices2 = new List<Vector2>()
            {
                new Vector2(1, 0), new Vector2(1, 1), new Vector2(2, 1)
            };
            m_poly1 = new Polygon2D(m_vertices1);
            m_poly2 = new Polygon2D(m_vertices2);
            m_multi = new MultiPolygon2D(new List<Polygon2D>() { m_poly1, m_poly2 });
        }

        [Test]
        public void ConstructionTest()
        {
            Assert.AreEqual(6, m_multi.VertexCount);
        }

        [Test]
        public void AddPolygonTest()
        {
            var multi2 = new MultiPolygon2D(m_multi.Polygons);
            var poly3 = new Polygon2D();
            multi2.AddPolygon(poly3);
            Assert.Contains(poly3, (System.Collections.ICollection)multi2.Polygons);
        }

        [Test]
        public void NextTest()
        {
            Assert.AreEqual(m_vertices1[1], m_multi.Next(m_vertices1[0]));
            Assert.AreEqual(m_vertices1[0], m_multi.Next(m_vertices1[2]));
            Assert.AreEqual(m_vertices2[0], m_multi.Next(m_vertices2[2]));
        }

        [Test]
        public void PrevTest()
        {
            Assert.AreEqual(m_vertices1[0], m_multi.Prev(m_vertices1[1]));
            Assert.AreEqual(m_vertices1[2], m_multi.Prev(m_vertices1[0]));
            Assert.AreEqual(m_vertices2[1], m_multi.Prev(m_vertices2[2]));
        }

        [Test]
        public void AddVertexAfterTest()
        {
            var multi2 = new MultiPolygon2D(m_multi.Polygons);
            var pos = new Vector2(0, 1);
            multi2.AddVertexAfter(pos, m_vertices1[0]);
            Assert.AreEqual(7, multi2.VertexCount);
            Assert.AreEqual(pos, multi2.Polygons.First().Vertices.ToList()[1]);
        }

        [Test]
        public void BoundingBoxTest()
        {
            var exp = new Rect(0, 0, 2, 1);
            Assert.AreEqual(exp, m_multi.BoundingBox());
            exp = new Rect(-1, -1, 4, 3);
            Assert.AreEqual(exp, m_multi.BoundingBox(1f));
        }

        [Test]
        public void ContainsInsideTest()
        {
            Assert.IsTrue(m_multi.ContainsInside(new Vector2(.75f, .1f)));
            Assert.IsTrue(m_multi.ContainsInside(new Vector2(1.5f, .9f)));
            Assert.IsFalse(m_multi.ContainsInside(new Vector2(-1, -1)));
        }

        [Test]
        public void ContainsVertexTest()
        {
            Assert.IsTrue(m_multi.ContainsVertex(m_vertices1[0]));
            Assert.IsTrue(m_multi.ContainsVertex(m_vertices1[2]));
            Assert.IsTrue(m_multi.ContainsVertex(m_vertices2[2]));
            Assert.IsFalse(m_multi.ContainsVertex(new Vector2(-1, -1)));
        }

        [Test]
        public void RemoveVertexTest()
        {
            var multi2 = new MultiPolygon2D(m_multi.Polygons);
            multi2.RemoveVertex(m_vertices1[0]);
            Assert.AreEqual(5, multi2.VertexCount);
        }

        [Test]
        public void AreaTest()
        {
            Assert.AreEqual(1f, m_multi.Area, MathUtil.EPS);
        }

        [Test]
        public void ClearTest()
        {
            var multi2 = new MultiPolygon2D(m_multi.Polygons);
            multi2.Clear();
            Assert.AreEqual(0, multi2.VertexCount);
            Assert.IsEmpty(multi2.Vertices);
        }

        [Test]
        public void SegmentsTest()
        {
            var segs = (System.Collections.ICollection)m_multi.Segments;
            Assert.AreEqual(6, segs.Count);
            Assert.Contains(new LineSegment(m_vertices1[0], m_vertices1[1]), segs);
            Assert.Contains(new LineSegment(m_vertices1[1], m_vertices1[2]), segs);
            Assert.Contains(new LineSegment(m_vertices1[2], m_vertices1[0]), segs);
            Assert.Contains(new LineSegment(m_vertices2[0], m_vertices2[1]), segs);
            Assert.Contains(new LineSegment(m_vertices2[1], m_vertices2[2]), segs);
            Assert.Contains(new LineSegment(m_vertices2[2], m_vertices2[0]), segs);
        }

        [Test]
        public void ClockwiseTest()
        {
            Assert.IsTrue(m_multi.IsClockwise());
            var m_multi2 = new MultiPolygon2D(m_multi.Polygons);
            m_multi2.Reverse();
            Assert.IsFalse(m_multi2.IsClockwise());
        }

        [Test]
        public void IllDefinedMethodsTest()
        {
            Assert.Throws<NotSupportedException>(() => m_multi.AddVertex(new Vector2()));
            Assert.Throws<NotSupportedException>(() => m_multi.AddVertexFirst(new Vector2()));
            Assert.Throws<NotSupportedException>(() => m_multi.RemoveFirst());
            Assert.Throws<NotSupportedException>(() => m_multi.RemoveLast());
        }
    }
}
