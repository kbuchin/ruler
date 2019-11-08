namespace Util.Geometry.Polygon.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Geometry.Polygon;
    using Util.Math;

    [TestFixture]
    class Polygon2DWithHolesTest
    {
        private readonly List<Vector2> m_diamondVertices;
        private readonly List<Vector2> m_largeSquareVertices;

        private readonly Polygon2D m_diamond;
        private readonly Polygon2D m_largeSquare;

        //4x4 square with 1x1 diamond hole
        private readonly Polygon2DWithHoles m_squareWithHole;

        public Polygon2DWithHolesTest()
        {
            m_diamondVertices = new List<Vector2>()
            {
                new Vector2(0, 1),
                new Vector2(1, 0),
                new Vector2(0, -1),
                new Vector2(-1, 0),
            };
            m_largeSquareVertices = new List<Vector2>()
            {
                new Vector2(2, 2),
                new Vector2(2, -2),
                new Vector2(-2, -2),
                new Vector2(-2, 2)
            };

            m_diamond = new Polygon2D(m_diamondVertices);
            m_largeSquare = new Polygon2D(m_largeSquareVertices);

            //4x4 square with 1x1 diamond hole
            m_squareWithHole = new Polygon2DWithHoles(m_largeSquare,
                new List<Polygon2D>() { m_diamond });
        }

        [Test]
        public void ConstructionTest()
        {
            Assert.AreEqual(m_largeSquareVertices.Count + m_diamondVertices.Count,
                m_squareWithHole.VertexCount);
            Assert.AreEqual(1, m_squareWithHole.Holes.Count);
            Assert.NotNull(m_squareWithHole.Outside);
            Assert.AreEqual(4, m_squareWithHole.OuterSegments.Count);
            Assert.AreEqual(4, m_squareWithHole.InnerSegments.Count);
            Assert.AreEqual(4, m_squareWithHole.OuterVertices.Count);
            Assert.AreEqual(4, m_squareWithHole.InnerVertices.Count);
        }

        [Test]
        public void NextTest()
        {
            Assert.AreEqual(m_largeSquareVertices[1], m_squareWithHole.Next(m_largeSquareVertices[0]));
            Assert.AreEqual(m_largeSquareVertices[0], m_squareWithHole.Next(m_largeSquareVertices[3]));
            Assert.AreEqual(m_diamondVertices[0], m_squareWithHole.Next(m_diamondVertices[3]));
        }

        [Test]
        public void PrevTest()
        {
            Assert.AreEqual(m_largeSquareVertices[0], m_squareWithHole.Prev(m_largeSquareVertices[1]));
            Assert.AreEqual(m_largeSquareVertices[3], m_squareWithHole.Prev(m_largeSquareVertices[0]));
            Assert.AreEqual(m_diamondVertices[3], m_squareWithHole.Prev(m_diamondVertices[0]));
        }

        [Test]
        public void AddVertexTest()
        {
            var poly = new Polygon2DWithHoles(m_squareWithHole.Outside, m_squareWithHole.Holes);
            var pos = new Vector2(0, 1);
            poly.AddVertex(pos);
            Assert.AreEqual(5, poly.OuterVertices.Count);
            Assert.AreEqual(pos, poly.Outside.Vertices.ToList()[4]);
        }

        [Test]
        public void AddVertexAfterTest()
        {
            var poly = new Polygon2DWithHoles(m_squareWithHole.Outside, m_squareWithHole.Holes);
            var pos = new Vector2(0, 1);
            poly.AddVertexAfter(pos, m_largeSquareVertices[0]);
            Assert.AreEqual(5, poly.OuterVertices.Count);
            Assert.AreEqual(pos, poly.Outside.Vertices.ToList()[1]);
        }

        [Test]
        public void BoundingBoxTest()
        {
            var exp = new Rect(-2, -2, 4, 4);
            Assert.AreEqual(exp, m_squareWithHole.BoundingBox());
            exp = new Rect(-3, -3, 6, 6);
            Assert.AreEqual(exp, m_squareWithHole.BoundingBox(1f));
        }

        [Test]
        public void ContainsInsideTest()
        {
            Assert.IsTrue(m_squareWithHole.ContainsInside(new Vector2(1.5f, 0)));
            Assert.IsFalse(m_squareWithHole.ContainsInside(new Vector2(0, 0)));
        }

        [Test]
        public void ContainsVertexTest()
        {
            Assert.IsTrue(m_squareWithHole.ContainsVertex(m_largeSquareVertices[0]));
            Assert.IsTrue(m_squareWithHole.ContainsVertex(m_largeSquareVertices[1]));
            Assert.IsTrue(m_squareWithHole.ContainsVertex(m_largeSquareVertices[2]));
            Assert.IsTrue(m_squareWithHole.ContainsVertex(m_largeSquareVertices[3]));
            Assert.IsTrue(m_squareWithHole.ContainsVertex(m_diamondVertices[0]));
            Assert.IsTrue(m_squareWithHole.ContainsVertex(m_diamondVertices[1]));
            Assert.IsTrue(m_squareWithHole.ContainsVertex(m_diamondVertices[2]));
            Assert.IsTrue(m_squareWithHole.ContainsVertex(m_diamondVertices[3]));
            Assert.IsFalse(m_squareWithHole.ContainsVertex(new Vector2(-99, 99)));
        }

        [Test]
        public void RemoveVertexTest()
        {
            var poly = new Polygon2DWithHoles(m_squareWithHole.Outside, m_squareWithHole.Holes);
            poly.RemoveVertex(m_largeSquareVertices[1]);
            Assert.AreEqual(7, poly.VertexCount);
            Assert.IsFalse(poly.ContainsVertex(m_largeSquareVertices[1]));
        }

        [Test]
        public void RemoveFirstTest()
        {
            var poly = new Polygon2DWithHoles(m_squareWithHole.Outside, m_squareWithHole.Holes);
            poly.RemoveFirst();
            Assert.AreEqual(7, poly.VertexCount);
            Assert.IsFalse(poly.ContainsVertex(m_largeSquareVertices[0]));
        }

        [Test]
        public void RemoveLastTest()
        {
            var poly = new Polygon2DWithHoles(m_squareWithHole.Outside, m_squareWithHole.Holes);
            poly.RemoveLast();
            Assert.AreEqual(7, poly.VertexCount);
            Assert.IsFalse(poly.ContainsVertex(m_largeSquareVertices[3]));
        }

        [Test]
        public void AreaTest()
        {
            Assert.AreEqual(14f, m_squareWithHole.Area, MathUtil.EPS);
        }

        [Test]
        public void ClearTest()
        {
            var poly = new Polygon2DWithHoles(m_squareWithHole.Outside, m_squareWithHole.Holes);
            poly.Clear();
            Assert.AreEqual(0, poly.VertexCount);
            Assert.IsEmpty(poly.Vertices);
            Assert.IsEmpty(poly.Holes);
        }

        [Test]
        public void SegmentsTest()
        {
            Assert.AreEqual(4, m_squareWithHole.OuterSegments.Count);

            var segs = (System.Collections.ICollection)m_squareWithHole.Segments;
            Assert.AreEqual(8, segs.Count);

            Assert.Contains(new LineSegment(m_largeSquareVertices[0], m_largeSquareVertices[1]), segs);
            Assert.Contains(new LineSegment(m_largeSquareVertices[1], m_largeSquareVertices[2]), segs);
            Assert.Contains(new LineSegment(m_largeSquareVertices[2], m_largeSquareVertices[3]), segs);
            Assert.Contains(new LineSegment(m_largeSquareVertices[3], m_largeSquareVertices[0]), segs);

            Assert.Contains(new LineSegment(m_diamondVertices[0], m_diamondVertices[1]), segs);
            Assert.Contains(new LineSegment(m_diamondVertices[1], m_diamondVertices[2]), segs);
            Assert.Contains(new LineSegment(m_diamondVertices[2], m_diamondVertices[3]), segs);
            Assert.Contains(new LineSegment(m_diamondVertices[3], m_diamondVertices[0]), segs);
        }

        [Test]
        public void ClockwiseTest()
        {
            Assert.IsTrue(m_squareWithHole.IsClockwise());
            var poly = new Polygon2DWithHoles(m_squareWithHole.Outside, m_squareWithHole.Holes);
            poly.Reverse();
            Assert.IsFalse(poly.IsClockwise());
        }

        [Test]
        public void AddVertexFirstTest()
        {
            var poly = new Polygon2DWithHoles(m_squareWithHole.Outside);
            var pos = new Vector2(-5, 5);
            poly.AddVertexFirst(pos);
            Assert.AreEqual(5, poly.VertexCount);
            var vertices = (System.Collections.ICollection)poly.Vertices;
            Assert.Contains(pos, vertices);
        }

        [Test]
        public void AddHoleTest()
        {
            var poly = new Polygon2DWithHoles();
            poly.AddHole(m_diamond);
            Assert.AreEqual(1, m_squareWithHole.Holes.Count);
        }

        [Test]
        public void RemoveHoleTest()
        {
            var poly = new Polygon2DWithHoles(m_squareWithHole.Outside, m_squareWithHole.Holes);
            poly.RemoveHole(m_diamond);
            Assert.AreEqual(0, poly.Holes.Count);
        }

        public void RemoveHolesTest()
        {
            var poly = new Polygon2DWithHoles(m_squareWithHole.Outside, m_squareWithHole.Holes);
            poly.RemoveHoles();
            Assert.AreEqual(0, poly.Holes.Count);
        }

        [Test]
        public void IsConvexTest()
        {
            Assert.IsFalse(m_squareWithHole.IsConvex());
            var poly = new Polygon2DWithHoles(m_squareWithHole.Outside, m_squareWithHole.Holes);
            poly.RemoveHoles();
            Assert.IsTrue(poly.IsConvex());
        }
    }
}
