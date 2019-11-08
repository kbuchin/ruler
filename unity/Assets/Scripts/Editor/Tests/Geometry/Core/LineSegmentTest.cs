namespace Util.Geometry.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using UnityEngine;

    [TestFixture]
    public class LineSegmentTest
    {
        private readonly LineSegment m_vertSeg;
        private readonly LineSegment m_topHorSeg;
        private readonly LineSegment m_botHorSeg;

        public LineSegmentTest()
        {
            m_vertSeg = new LineSegment(new Vector2(1, 0), new Vector2(1, 1));
            m_topHorSeg = new LineSegment(new Vector2(0, 1), new Vector2(2, 1));
            m_botHorSeg = new LineSegment(new Vector2(2, 0), new Vector2(0, 0));
        }

        [Test]
        public void OrdinarySegmentTest()
        {
            var segment1 = new LineSegment(new Vector2(2, 1), new Vector2(-2, 1));
            var segment2 = new LineSegment(new Vector2(-1, 2), new Vector2(-1, 0));
            var intersection = LineSegment.Intersect(segment1, segment2);
            Assert.AreEqual(new Vector2(-1, 1), intersection.Value);
        }

        [Test]
        public void HorizontalSegmentsTest()
        {
            var inter = LineSegment.Intersect(
                new LineSegment(new Vector2(2, 0), new Vector2(4, 0)),
                new LineSegment(new Vector2(2, 0), new Vector2(4, 0)));

            var inter2 = LineSegment.Intersect(
                new LineSegment(new Vector2(0, 0), new Vector2(4, 0)),
                new LineSegment(new Vector2(0, 0), new Vector2(4, 0)));

            Assert.AreEqual(null, inter);
            Assert.AreEqual(null, inter2);
        }

        [Test]
        public void TopHorVertSegmentTest()
        {
            var intersection = LineSegment.Intersect(m_topHorSeg, m_vertSeg);
            Assert.AreEqual(new Vector2(1, 1), intersection.Value);
        }

        [Test]
        public void BotHorVertSegmentTest()
        {
            var intersection = LineSegment.Intersect(m_botHorSeg, m_vertSeg);
            Assert.AreEqual(new Vector2(1, 0), intersection.Value);
        }

        [Test]
        public void IntersectionWithSegementsTest()
        {
            var intersections = m_vertSeg.Intersect(new List<LineSegment>() { m_botHorSeg, m_topHorSeg });
            Assert.AreEqual(2, intersections.Count);
            Assert.True(intersections.Contains(new Vector2(1, 0)));
            Assert.True(intersections.Contains(new Vector2(1, 1)));
        }

        [Test]
        public void NonIntersectingSegmentsWithIntersectingBoundingBox()
        {
            var seg1 = new LineSegment(Vector2.zero, 4 * Vector2.one);
            var seg2 = new LineSegment(4 * Vector2.right, new Vector2(3, 1));
            var intersection = LineSegment.Intersect(seg1, seg2);
            Assert.AreEqual(null, intersection);
        }
    }
}
