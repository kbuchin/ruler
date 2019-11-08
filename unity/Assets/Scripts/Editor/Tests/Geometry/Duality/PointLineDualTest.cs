namespace Util.Geometry.Duality.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Math;

    [TestFixture]
    public class PointLineDualTest
    {
        [Test]
        public void LineToPointTest()
        {
            var l1 = new Line(0.5f, 1.2f);
            var l2 = new Line(-0.2f, -9999.1f);
            var l3 = new Line(new Vector2(0f, 1.2f), new Vector2(1f, 1.7f));

            var ret1 = PointLineDual.Dual(l1);
            var ret2 = PointLineDual.Dual(l2);
            var ret3 = PointLineDual.Dual(l3);

            Assert.IsTrue(MathUtil.EqualsEps(new Vector2(0.5f, -1.2f), ret1));
            Assert.IsTrue(MathUtil.EqualsEps(new Vector2(-0.2f, 9999.1f), ret2));
            Assert.IsTrue(MathUtil.EqualsEps(ret1, ret3));
        }

        [Test]
        public void PointToLineTest()
        {
            var v1 = new Vector2(0.5f, 1.2f);
            var v2 = new Vector2(-0.2f, -9999.1f);

            var ret1 = PointLineDual.Dual(v1);
            var ret2 = PointLineDual.Dual(v2);

            Assert.AreEqual(new Line(0.5f, -1.2f), ret1);
            Assert.AreEqual(new Line(-0.2f, 9999.1f), ret2);
        }

        [Test]
        public void DualOfDualTest()
        {
            var l = new Line(0.5f, -5.2f);
            var l2 = PointLineDual.Dual(PointLineDual.Dual(l));
            Assert.AreEqual(l, l2);
        }

        [Test]
        public void LineCollectionsTest()
        {
            var l1 = new Line(0.5f, 1.2f);
            var l2 = new Line(-0.2f, -9999.1f);

            var ret = PointLineDual.Dual(new List<Line>() { l1, l2 }).ToList();

            Assert.AreEqual(2, ret.Count);
            Assert.IsTrue(MathUtil.EqualsEps(new Vector2(0.5f, -1.2f), ret[0]));
            Assert.IsTrue(MathUtil.EqualsEps(new Vector2(-0.2f, 9999.1f), ret[1]));
        }

        [Test]
        public void PointCollectionsTest()
        {
            var v1 = new Vector2(0.5f, 1.2f);
            var v2 = new Vector2(-0.2f, -9999.1f);

            var ret = PointLineDual.Dual(new List<Vector2>() { v1, v2 }).ToList();

            Assert.AreEqual(2, ret.Count);
            Assert.AreEqual(new Line(0.5f, -1.2f), ret[0]);
            Assert.AreEqual(new Line(-0.2f, 9999.1f), ret[1]);
        }
    }
}
