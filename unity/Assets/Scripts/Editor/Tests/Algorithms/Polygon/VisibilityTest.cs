namespace Util.Algorithms.Polygon.Tests
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using UnityEngine;
    using Util.Geometry.Polygon;
    using Util.Algorithms.Polygon;
    using System;
    using Util.Math;

    [TestFixture]
    public class VisibilityTest
    {
        private readonly Polygon2D arrowPoly;
        private readonly Polygon2D diamondPoly;

        public VisibilityTest()
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
        }
       
        [Test]
        public void AreaTest()
        {
            var vision = Visibility.Vision(arrowPoly, new Vector2(1.5f, 0));
            Debug.Log(arrowPoly);
            Debug.Log(vision);
            Assert.IsTrue(MathUtil.EqualsEps(arrowPoly.Area, vision.Area));

            vision = Visibility.Vision(diamondPoly, Vector2.zero);
            Assert.IsTrue(MathUtil.EqualsEps(diamondPoly.Area, vision.Area));
        }

        [Test]
        public void ContainsTest()
        {
            // check if exception is thrown when given point outside polygon
            Assert.Throws<ArgumentException>(() => Visibility.Vision(arrowPoly, new Vector2(-1f, 0)));
        }
    }
}
