namespace Util.Algorithms.Polygon.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Algorithms.Polygon;
    using Util.Geometry.Polygon;
    using Util.Math;

    [TestFixture]
    public class ClipperTest
    {
        private readonly Polygon2D subjectPoly;
        private readonly Polygon2D clipPoly;
        private readonly MultiPolygon2D resultPoly;

        private readonly List<Vector2> m_horizontalRectVertices;
        private readonly List<Vector2> m_verticalRectVertices;
        private readonly Polygon2D m_horizontalRect;
        private readonly Polygon2D m_verticalRect;

        private readonly List<Vector2> m_2by1RectVertices;
        private readonly List<Vector2> m_1by2RectVertices;
        private readonly List<Vector2> m_unitSquareVertices;

        private readonly Polygon2D m_2by1rect;
        private readonly Polygon2D m_1by2rect;
        private readonly Polygon2D m_unitSquare;
        public ClipperTest()
        {
            var m_topVertex = new Vector2(0, 1);
            var m_botVertex = new Vector2(0, -1);
            var m_leftVertex = new Vector2(-1, 0);
            var m_rightVertex = new Vector2(1, 0);
            var m_farRightVertex = new Vector2(5, 0);

            subjectPoly = new Polygon2D(new List<Vector2>()
            {
                m_topVertex, m_rightVertex, m_botVertex, m_leftVertex
            });
            clipPoly = new Polygon2D(new List<Vector2>()
            {
                m_topVertex, m_farRightVertex, m_botVertex
            });
            resultPoly = new MultiPolygon2D(new Polygon2D(new List<Vector2>()
            {
                m_botVertex, m_leftVertex, m_topVertex
            }));

            m_horizontalRectVertices = new List<Vector2>()
            {
                new Vector2(-2, 1), new Vector2(2, 1), new Vector2(2, -1), new Vector2(-2,-1)
            };
            m_verticalRectVertices = new List<Vector2>()
            {
                new Vector2(-1,2), new Vector2(1, 2), new Vector2(1, 0), new Vector2(-1,0)
            };

            m_horizontalRect = new Polygon2D(m_horizontalRectVertices);
            m_verticalRect = new Polygon2D(m_verticalRectVertices);

            m_2by1RectVertices = new List<Vector2>()
            {
                new Vector2(0, 1), new Vector2(2, 1), new Vector2(2, 0), new Vector2(0, 0)
            };
            m_1by2RectVertices = new List<Vector2>()
            {
                new Vector2(0, 2), new Vector2(1, 2), new Vector2(1, 0), new Vector2(0, 0)
            };
            m_unitSquareVertices = new List<Vector2>()
            {
                new Vector2(0,1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0)
            };

            m_2by1rect = new Polygon2D(m_2by1RectVertices);
            m_1by2rect = new Polygon2D(m_1by2RectVertices);
            m_unitSquare = new Polygon2D(m_unitSquareVertices);
        }


        [Test]
        public void CutOutTest1()
        {
            var cutout = Clipper.CutOut(m_verticalRect, m_horizontalRect);
            Assert.AreEqual(2f, cutout.Area, MathUtil.EPS);
        }

        [Test]
        public void CutOutTest2()
        {
            var cutout = Clipper.CutOut(m_horizontalRect, m_verticalRect);
            Assert.AreEqual(6f, cutout.Area, MathUtil.EPS);
        }

        [Test]
        public void OverlappingClipTest()
        {
            var clipResult = Clipper.CutOut(subjectPoly, clipPoly);
            Assert.AreEqual(resultPoly, clipResult);
        }

        [Test]
        public void NonOverlappingClipTest()
        {
            var clipResult = Clipper.CutOut(resultPoly, clipPoly);
            Assert.AreEqual(resultPoly, clipResult);
        }

        [Test]
        public void RepeatCutTest()
        {
            var clipResult = Clipper.CutOut(subjectPoly, clipPoly);
            var nextResult = Clipper.CutOut(clipResult, clipPoly);
            Assert.AreEqual(clipResult, nextResult);
        }


        [Test]
        public void CutOutRectFromSquareCollinearTest1()
        {
            var remainder = Clipper.CutOut(m_unitSquare, m_2by1rect);
            Assert.AreEqual(0f, remainder.Area, MathUtil.EPS);
        }

        [Test]
        public void CutOutSquareFromRectCollinearTest1()
        {
            var remainder = Clipper.CutOut(m_2by1rect, m_unitSquare);
            Assert.AreEqual(1f, remainder.Area, MathUtil.EPS);
        }

        [Test]
        public void CutOutRectFromSquareCollinearTest2()
        {
            var remainder = Clipper.CutOut(m_unitSquare, m_1by2rect);
            Assert.AreEqual(0f, remainder.Area, MathUtil.EPS);
        }

        [Test]
        public void CutOutSquareFromRectCollinearTest2()
        {
            var remainder = Clipper.CutOut(m_1by2rect, m_unitSquare);
            Assert.AreEqual(1f, remainder.Area, MathUtil.EPS);
        }

        [Test]
        public void CutOutRectFromRectCollinearTest1()
        {
            var remainder = Clipper.CutOut(m_1by2rect, m_2by1rect);
            Assert.AreEqual(1f, remainder.Area, MathUtil.EPS);
        }

        [Test]
        public void CutOutRectFromRectCollinearTest2()
        {
            var remainder = Clipper.CutOut(m_2by1rect, m_1by2rect);
            Assert.AreEqual(1f, remainder.Area, MathUtil.EPS);
        }

        [Test]
        public void CutOutNonIntersectingTest()
        {
            List<Vector2> horizontalRectVertices = new List<Vector2>()
            {
                new Vector2(0, 0), new Vector2(2, 0), new Vector2(2, -1), new Vector2(0,-1)
            };
            List<Vector2> squareVertices = new List<Vector2>()
            {
                new Vector2(10,10), new Vector2(11, 10), new Vector2(11, 9), new Vector2(10,9)
            };

            Polygon2D horizontalRect = new Polygon2D(horizontalRectVertices);
            Polygon2D square = new Polygon2D(squareVertices);

            var cutout = Clipper.CutOut(square, horizontalRect);
            Assert.AreEqual(1f, cutout.Area, MathUtil.EPS);

            cutout = Clipper.CutOut(horizontalRect, square);
            Assert.AreEqual(2f, cutout.Area, MathUtil.EPS);
        }
    }
}