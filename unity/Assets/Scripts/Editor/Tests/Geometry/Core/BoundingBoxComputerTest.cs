namespace Util.Geometry.Test
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Math;

    [TestFixture]
    public class BoundingBoxComputerTest
    {
        private readonly List<Vector2> m_points;
        private readonly List<LineSegment> m_segments;
        private readonly List<Line> m_lines;
        private readonly Rect expRect, expRectMargin;

        public BoundingBoxComputerTest()
        {
            m_points = new List<Vector2>()
            {
                new Vector2(1, 2),
                new Vector2(-2f / 3f, 2),
                new Vector2(-1f / 3f, 2f / 3f),
                new Vector2(6, 7),
                new Vector2(1, -3),
                new Vector2(3.5f, 2)
            };
            m_segments = new List<LineSegment>()
            {
                new LineSegment(m_points[0], m_points[1]),
                new LineSegment(m_points[2], m_points[3]),
                new LineSegment(m_points[1], m_points[4]),
                new LineSegment(m_points[0], m_points[5])
            };
            m_lines = new List<Line>()
            {
                new Line(1, 1),
                new Line(2, -5),
                new Line(-3, 0),
                new Line(0, 2)
            };

            expRect = new Rect(-2f / 3f, -3, 20f / 3f, 10);
            expRectMargin = new Rect(-5f / 3f, -4, 26f / 3f, 12);
        }

        [Test]
        public void FromSegmentsTest()
        {
            var ret = BoundingBoxComputer.FromSegments(m_segments);
            Assert.IsTrue(CmpRect(expRect, ret));
            ret = BoundingBoxComputer.FromSegments(m_segments, 1f);
            Assert.IsTrue(CmpRect(expRectMargin, ret));

            Assert.AreEqual(BoundingBoxComputer.FromSegments(new List<LineSegment>()), new Rect());
        }

        [Test]
        public void FromLinesTest()
        {
            var ret = BoundingBoxComputer.FromLines(m_lines);
            Assert.IsTrue(CmpRect(expRect, ret));
            ret = BoundingBoxComputer.FromLines(m_lines, 1f);
            Assert.IsTrue(CmpRect(expRectMargin, ret));

            Assert.AreEqual(BoundingBoxComputer.FromLines(new List<Line>()), new Rect());
        }

        [Test]
        public void FromVector2Test()
        {
            var ret = BoundingBoxComputer.FromPoints(m_points);
            Assert.IsTrue(CmpRect(expRect, ret));
            ret = BoundingBoxComputer.FromPoints(m_points, 1f);
            Assert.IsTrue(CmpRect(expRectMargin, ret));

            Assert.AreEqual(BoundingBoxComputer.FromPoints(new List<Vector2>()), new Rect());
        }

        private bool CmpRect(Rect a, Rect b)
        {
            return MathUtil.EqualsEps(a.xMin, b.xMin) &&
                MathUtil.EqualsEps(a.yMin, b.yMin) &&
                MathUtil.EqualsEps(a.width, b.width) &&
                MathUtil.EqualsEps(a.height, b.height);
        }
    }
}
