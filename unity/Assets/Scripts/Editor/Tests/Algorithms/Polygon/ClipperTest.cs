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
    public class ClipperTest
    {
        private readonly Polygon2D subjectPoly;
        private readonly Polygon2D clipPoly;
        private readonly MultiPolygon2D resultPoly;

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
                m_topVertex, m_botVertex, m_leftVertex
            }));
        }

        [Test]
        public void OverlappingClipTest()
        {
            var clipResult = Clipper.CutOut(subjectPoly, clipPoly);
            Assert.IsTrue(MathUtil.EqualsEps(resultPoly.Area, clipResult.Area));
            //Assert.AreEqual(resultPoly, clipResult);
        }

        [Test]
        public void NonOverlappingClipTest()
        {
            var clipResult = Clipper.CutOut(resultPoly, clipPoly);
            Assert.IsTrue(MathUtil.EqualsEps(resultPoly.Area, clipResult.Area));
            //Assert.IsTrue(resultPoly.Equals(clipResult));
        }

        [Test]
        public void RepeatCutTest()
        {
            var clipResult = Clipper.CutOut(subjectPoly, clipPoly);
            var nextResult = Clipper.CutOut(clipResult, clipPoly);
            Assert.IsTrue(clipResult.Equals(nextResult));
        }
    }
}
