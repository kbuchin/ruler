namespace Util.Geometry.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Math;

    [TestFixture]
    public class LineTest
    {
        private readonly Line m_line1, m_line2, m_line3;
        private readonly Line m_horLine, m_vertLine;
        private readonly Line m_parallelLine;

        public LineTest()
        {
            m_line1 = new Line(1, 2);
            m_line2 = new Line(-2, 5);
            m_line3 = new Line(new Vector2(0, 0), new Vector2(5, 3));
            m_horLine = new Line(0, -1);
            m_vertLine = new Line(new Vector2(3, 0), new Vector2(3, 3));
            m_parallelLine = new Line(1, -2);
        }

        [Test]
        public void IsHorizontalTest()
        {
            Assert.IsTrue(m_horLine.IsHorizontal);

            Assert.IsFalse(m_line1.IsHorizontal);
            Assert.IsFalse(m_line2.IsHorizontal);
            Assert.IsFalse(m_line3.IsHorizontal);
            Assert.IsFalse(m_vertLine.IsHorizontal);
        }

        [Test]
        public void IsVerticalTest()
        {
            Assert.IsTrue(m_vertLine.IsVertical);

            Assert.IsFalse(m_line1.IsVertical);
            Assert.IsFalse(m_line2.IsVertical);
            Assert.IsFalse(m_line3.IsVertical);
            Assert.IsFalse(m_horLine.IsVertical);
        }

        [Test]
        public void HeightAtYAxisTest()
        {
            Assert.AreEqual(2, m_line1.HeightAtYAxis);
            Assert.AreEqual(5, m_line2.HeightAtYAxis);
            Assert.AreEqual(0, m_line3.HeightAtYAxis);
            Assert.AreEqual(-1, m_horLine.HeightAtYAxis);
            Assert.AreEqual(float.NaN, m_vertLine.HeightAtYAxis);
        }

        [Test]
        public void WidthAtXAxisTest()
        {
            Assert.AreEqual(-2, m_line1.WidthAtXAxis);
            Assert.AreEqual(2.5f, m_line2.WidthAtXAxis);
            Assert.AreEqual(0, m_line3.WidthAtXAxis);
            Assert.AreEqual(float.NaN, m_horLine.WidthAtXAxis);
            Assert.AreEqual(3, m_vertLine.WidthAtXAxis);
        }

        [Test]
        public void SlopeTest()
        {
            Assert.AreEqual(1, m_line1.Slope); ;
            Assert.AreEqual(-2, m_line2.Slope);
            Assert.AreEqual(3f / 5f, m_line3.Slope);
            Assert.AreEqual(0, m_horLine.Slope);
            Assert.AreEqual(float.PositiveInfinity, m_vertLine.Slope);
        }

        [Test]
        public void AngleTest()
        {
            Assert.AreEqual(.25f * Mathf.PI, m_line1.Angle);
            Assert.AreEqual(0f, m_horLine.Angle);
            Assert.AreEqual(.5f * Mathf.PI, m_vertLine.Angle);
        }

        [Test]
        public void IntersectLineTest()
        {
            var result = m_line1.Intersect(m_line2);
            var exp = new Vector2(1, 3);
            Assert.NotNull(result);
            Assert.IsTrue(MathUtil.EqualsEps(exp, result.Value));
            result = m_horLine.Intersect(m_vertLine);
            exp = new Vector2(3, -1);
            Assert.NotNull(result);
            Assert.IsTrue(MathUtil.EqualsEps(exp, result.Value));
            result = m_line1.Intersect(m_parallelLine);
            Assert.IsNull(result);
        }

        [Test]
        public void IsOnLineTest()
        {
            var point = new Vector2(1, 3);
            Assert.IsTrue(m_line1.IsOnLine(point));
            Assert.IsTrue(m_line2.IsOnLine(point));
            Assert.IsFalse(m_line3.IsOnLine(point));
            point = new Vector2(999, -1);
            Assert.IsTrue(m_horLine.IsOnLine(point));
            Assert.IsFalse(m_vertLine.IsOnLine(point));

            // robustness check
            point = new Vector2(3, (float)MathUtil.EPS / 10f);
            Assert.IsTrue(m_vertLine.IsOnLine(point));
        }

        [Test]
        public void PointAboveTest()
        {
            var point = new Vector2(0, 3);
            Assert.IsTrue(m_line1.PointAbove(point));
            Assert.IsFalse(m_line2.PointAbove(point));
            Assert.IsTrue(m_line3.PointAbove(point));
            Assert.IsTrue(m_horLine.PointAbove(point));

            // check point on line
            point = new Vector2(0, 0);
            Assert.IsFalse(m_line3.PointAbove(point));

            // check left is above for vertical
            Assert.IsTrue(m_vertLine.PointAbove(point));
            point = new Vector2(5, 0);
            Assert.IsFalse(m_vertLine.PointAbove(point));
        }

        [Test]
        public void NumberOfPointsAboveTest()
        {
            var points = new List<Vector2>()
            {
                new Vector2(0, 3), new Vector2(0, 0), new Vector2(0, -5)
            };
            Assert.AreEqual(1, m_line1.NumberOfPointsAbove(points));
            Assert.AreEqual(0, m_line2.NumberOfPointsAbove(points));
            Assert.AreEqual(1, m_line3.NumberOfPointsAbove(points));
        }

        [Test]
        public void PointRightOfLineTest()
        {
            var point = new Vector2(1, 0);
            Assert.Throws<GeomException>(() => m_line1.PointRightOfLine(point));
            Assert.Throws<GeomException>(() => m_line2.PointRightOfLine(point));
            Assert.IsTrue(m_line3.PointRightOfLine(point));
            Assert.IsFalse(m_vertLine.PointRightOfLine(point));
            Assert.Throws<GeomException>(() => m_horLine.PointRightOfLine(point));
        }

        [Test]
        public void DistanceToPointTest()
        {
            var point = new Vector2(2, 0);
            Assert.AreEqual(Mathf.Sqrt(8), m_line1.DistanceToPoint(point));
            Assert.AreEqual(1f, m_horLine.DistanceToPoint(point));
            Assert.AreEqual(1f, m_vertLine.DistanceToPoint(point));
        }
    }
}
