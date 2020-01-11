using System.Linq;

namespace Util.Algorithms.Polygon.Tests
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Algorithms.Polygon;
    using Util.Geometry.Polygon;
    using Util.Math;

    [TestFixture]
    public class VisibilityTest
    {
        private readonly Polygon2D arrowPoly;
        private readonly Polygon2D diamondPoly;
        private readonly Polygon2D LShape;

        public VisibilityTest()
        {
            var m_topVertex = new Vector2(0, 1);
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
        public void AreaTest1()
        {
            var vision = Visibility.Vision(arrowPoly, new Vector2(1.5f, 0));
            Assert.IsTrue(MathUtil.EqualsEps(arrowPoly.Area, vision.Area));

            vision = Visibility.Vision(diamondPoly, Vector2.zero);
            Assert.IsTrue(MathUtil.EqualsEps(diamondPoly.Area, vision.Area));
        }

        [Test]
        public void AreaTest2()
        {
            var shape = new Polygon2D(
                new List<Vector2>()
                {
                    // up arrow
                    new Vector2(-1, 0),
                    new Vector2(0, 2),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                });

            var vision1 = Visibility.Vision(shape, shape.Vertices.ElementAt(0));
            var vision2 = Visibility.Vision(shape, shape.Vertices.ElementAt(1));
            var vision3 = Visibility.Vision(shape, shape.Vertices.ElementAt(2));
            var vision4 = Visibility.Vision(shape, shape.Vertices.ElementAt(3));

            float expectedArea1 = 4f / 6;
            float actualArea1 = vision1.Area;

            float expectedArea2 = 1f;
            float actualArea2 = vision2.Area;

            float expectedArea3 = 4f / 6;
            float actualArea3 = vision3.Area;

            float expectedArea4 = 1f;
            float actualArea4 = vision4.Area;

            Assert.IsTrue(MathUtil.EqualsEps(expectedArea1, actualArea1));
            Assert.IsTrue(MathUtil.EqualsEps(expectedArea2, actualArea2));
            Assert.IsTrue(MathUtil.EqualsEps(expectedArea3, actualArea3));
            Assert.IsTrue(MathUtil.EqualsEps(expectedArea4, actualArea4));
        }

        [Test]
        public void AreaTest3()
        {
            var shape = new Polygon2D(
                new List<Vector2>()
                {
                    //right arrow
                    new Vector2(0, 1),
                    new Vector2(2, 0),
                    new Vector2(0, -1),
                    new Vector2(1, 0),
                });

            var vision1 = Visibility.Vision(shape, shape.Vertices.ElementAt(0));
            var vision2 = Visibility.Vision(shape, shape.Vertices.ElementAt(1));
            var vision3 = Visibility.Vision(shape, shape.Vertices.ElementAt(2));
            var vision4 = Visibility.Vision(shape, shape.Vertices.ElementAt(3));

            float expectedArea1 = 4f / 6;
            float actualArea1 = vision1.Area;

            float expectedArea2 = 1f;
            float actualArea2 = vision2.Area;

            float expectedArea3 = 4f / 6;
            float actualArea3 = vision3.Area;

            float expectedArea4 = 1f;
            float actualArea4 = vision4.Area;

            Assert.IsTrue(MathUtil.EqualsEps(expectedArea1, actualArea1));
            Assert.IsTrue(MathUtil.EqualsEps(expectedArea2, actualArea2));
            Assert.IsTrue(MathUtil.EqualsEps(expectedArea3, actualArea3));
            Assert.IsTrue(MathUtil.EqualsEps(expectedArea4, actualArea4));
        }

        [Test]
        public void AreaTest4()
        {
            var shape = new Polygon2D(
                new List<Vector2>()
                {
                    //down arrow
                    new Vector2(-1, 0),
                    new Vector2(0, -2),
                    new Vector2(1, 0),
                    new Vector2(0, -1),
                });

            var vision1 = Visibility.Vision(shape, shape.Vertices.ElementAt(0));
            var vision2 = Visibility.Vision(shape, shape.Vertices.ElementAt(1));
            var vision3 = Visibility.Vision(shape, shape.Vertices.ElementAt(2));
            var vision4 = Visibility.Vision(shape, shape.Vertices.ElementAt(3));

            float expectedArea1 = 4f / 6;
            float actualArea1 = vision1.Area;

            float expectedArea2 = 1f;
            float actualArea2 = vision2.Area;

            float expectedArea3 = 4f / 6;
            float actualArea3 = vision3.Area;

            float expectedArea4 = 1f;
            float actualArea4 = vision4.Area;

            Assert.IsTrue(MathUtil.EqualsEps(expectedArea1, actualArea1));
            Assert.IsTrue(MathUtil.EqualsEps(expectedArea2, actualArea2));
            Assert.IsTrue(MathUtil.EqualsEps(expectedArea3, actualArea3));
            Assert.IsTrue(MathUtil.EqualsEps(expectedArea4, actualArea4));
        }

        [Test]
        public void AreaTest5()
        {
            var shape = new Polygon2D(
                new List<Vector2>()
                {
                    // left arrow
                    new Vector2(0, 1),
                    new Vector2(-1, 0),
                    new Vector2(0, -1),
                    new Vector2(-2, 0),
                });

            var vision1 = Visibility.Vision(shape, shape.Vertices.ElementAt(0));
            var vision2 = Visibility.Vision(shape, shape.Vertices.ElementAt(1));
            var vision3 = Visibility.Vision(shape, shape.Vertices.ElementAt(2));
            var vision4 = Visibility.Vision(shape, shape.Vertices.ElementAt(3));

            float expectedArea1 = 4f / 6;
            float actualArea1 = vision1.Area;

            float expectedArea2 = 1f;
            float actualArea2 = vision2.Area;

            float expectedArea3 = 4f / 6;
            float actualArea3 = vision3.Area;

            float expectedArea4 = 1f;
            float actualArea4 = vision4.Area;

            Assert.IsTrue(MathUtil.EqualsEps(expectedArea1, actualArea1));
            Assert.IsTrue(MathUtil.EqualsEps(expectedArea2, actualArea2));
            Assert.IsTrue(MathUtil.EqualsEps(expectedArea3, actualArea3));
            Assert.IsTrue(MathUtil.EqualsEps(expectedArea4, actualArea4));
        }

        [Test]
        public void AreaTest6()
        {
            var shape = new Polygon2D(
                new List<Vector2>()
                {
                    // left arrow shifted
                    new Vector2(3, 4),
                    new Vector2(2, 3),
                    new Vector2(3, 2),
                    new Vector2(1, 3),
                });

            var vision1 = Visibility.Vision(shape, shape.Vertices.ElementAt(0));
            var vision2 = Visibility.Vision(shape, shape.Vertices.ElementAt(1));
            var vision3 = Visibility.Vision(shape, shape.Vertices.ElementAt(2));
            var vision4 = Visibility.Vision(shape, shape.Vertices.ElementAt(3));

            float expectedArea1 = 4f / 6;
            float actualArea1 = vision1.Area;

            float expectedArea2 = 1f;
            float actualArea2 = vision2.Area;

            float expectedArea3 = 4f / 6;
            float actualArea3 = vision3.Area;

            float expectedArea4 = 1f;
            float actualArea4 = vision4.Area;

            Assert.IsTrue(MathUtil.EqualsEps(expectedArea1, actualArea1));
            Assert.IsTrue(MathUtil.EqualsEps(expectedArea2, actualArea2));
            Assert.IsTrue(MathUtil.EqualsEps(expectedArea3, actualArea3));
            Assert.IsTrue(MathUtil.EqualsEps(expectedArea4, actualArea4));
        }

        [Test]
        public void ContainsTest()
        {
            // check if exception is thrown when given point outside polygon
            Assert.Throws<ArgumentException>(
                () => Visibility.Vision(arrowPoly, new Vector2(-1f, 0)));
        }

        [Test]
        public void LShapeVisionTest()
        {
            var poly = Visibility.Vision(
                LShape,
                new Vector2(3.427403f, 3.464213f));

            // check percentage
            Assert.Greater(0.88f, poly.Area / LShape.Area);
        }
    }
}