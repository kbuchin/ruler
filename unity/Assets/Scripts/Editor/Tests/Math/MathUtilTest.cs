namespace Util.Math.Tests
{
    using NUnit.Framework;
    using System;
    using UnityEngine;
    using Util.Math;

    [TestFixture]
    public class MathUtilTest
    {

        private static double eps = MathUtil.EPS;

        [Test]
        public void AngleTest()
        {
            var a = new Vector2(2f, 0f);
            var x = new Vector2(0f, 0f);
            var b = new Vector2(0f, 5f);
            var c = new Vector2(0f, -5f);
            var d = new Vector2(-1f, -1f);
            var e = new Vector2(2f, (float)-eps);

            Assert.AreEqual(MathUtil.Angle(x, a, a), 0f, eps);
            Assert.AreEqual(MathUtil.Angle(x, a, b), .5f * Math.PI, eps);
            Assert.AreEqual(MathUtil.Angle(x, a, c), 1.5f * Math.PI, eps);
            Assert.AreEqual(MathUtil.Angle(x, a, d), 1.25f * Math.PI, eps);
            Assert.AreEqual(MathUtil.Angle(x, a, e), 2f * Math.PI, eps);
        }

        [Test]
        public void AngleTest2()
        {
            var a = new Vector2(2.5f, 0f);
            var x = new Vector2(1.5f, 0f);
            var b = new Vector2(1f, 0f);

            Assert.AreEqual(MathUtil.Angle(x, a, b), Math.PI, eps);
        }

        [Test]
        public void PositiveModTest()
        {
            Assert.AreEqual(3, MathUtil.PositiveMod(3, 5));
            Assert.AreEqual(0, MathUtil.PositiveMod(5, 5));
            Assert.AreEqual(4, MathUtil.PositiveMod(-1, 5));
        }
    }
}
