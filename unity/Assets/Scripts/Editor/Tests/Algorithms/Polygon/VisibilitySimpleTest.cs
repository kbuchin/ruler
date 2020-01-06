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
    public class VisibilitySimpleTest
    {
        private readonly Polygon2D arrowPoly;
        private readonly Polygon2D diamondPoly;
        private readonly Polygon2D LShape;

        public VisibilitySimpleTest()
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
        public void AreaTest()
        {
            var vision = VisibilitySimple.Vision(arrowPoly, new Vector2(1.5f, 0));
            Assert.IsTrue(MathUtil.EqualsEps(arrowPoly.Area, vision.Area));

            vision = VisibilitySimple.Vision(diamondPoly, Vector2.zero);
            Assert.IsTrue(MathUtil.EqualsEps(diamondPoly.Area, vision.Area));
        }

        [Test]
        public void ContainsTest()
        {
            // check if exception is thrown when given point outside polygon
            Assert.Throws<ArgumentException>(() => VisibilitySimple.Vision(arrowPoly, new Vector2(-1f, 0)));
        }

        [Test]
        public void LShapeVisionTest()
        {
            var poly = VisibilitySimple.Vision(LShape, new Vector2(3.427403f, 3.464213f));

            // check percentage
            Assert.Greater(0.88f, poly.Area / LShape.Area);
        }
    }
}
