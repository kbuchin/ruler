namespace Util.Math.Tests
{
    using System;
    using NUnit.Framework;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Math;

    [TestFixture]
    class MathUtilTest
    {
        [Test]
        public void AngleTest()
        {
            Vector2 a = new Vector2(2f, 0f);
            Vector2 x = new Vector2();
            Vector2 b = new Vector2(0f, 5f);

            Assert.AreEqual(MathUtil.Angle(x, a, b), .5f * Math.PI, Mathf.Epsilon);
        }
    }
}
