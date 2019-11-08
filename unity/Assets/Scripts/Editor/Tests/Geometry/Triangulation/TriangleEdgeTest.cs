namespace Util.Geometry.Triangulation.Tests
{
    using NUnit.Framework;
    using UnityEngine;

    [TestFixture]
    public class TriangleEdgeTest
    {
        private readonly Vector2 v1, v2, v3, v4;
        private readonly TriangleEdge e1, e2, e3;
        private readonly TriangleEdge e4, e5, e6;
        private readonly Triangle t1, t2;

        public TriangleEdgeTest()
        {
            v1 = new Vector2(0, 0);
            v2 = new Vector2(1, 1);
            v3 = new Vector2(2, 0);
            v4 = new Vector2(-1, -1);

            e1 = new TriangleEdge(v1, v2);
            e2 = new TriangleEdge(v2, v3);
            e3 = new TriangleEdge(v3, v1);
            t1 = new Triangle(e1, e2, e3);

            e4 = new TriangleEdge(v1, v3);
            e5 = new TriangleEdge(v3, v4);
            e6 = new TriangleEdge(v4, v1);
            t2 = new Triangle(e4, e5, e6);

            e3.Twin = e4;
            e4.Twin = e3;
        }

        [Test]
        public void IsOuterTest()
        {
            Assert.IsTrue(e1.IsOuter);
            Assert.IsTrue(e2.IsOuter);
            Assert.IsFalse(e3.IsOuter);
        }

        [Test]
        public void TriangleTest()
        {
            Assert.AreEqual(t1, e1.T);
            Assert.AreEqual(t1, e2.T);
            Assert.AreEqual(t1, e3.T);
            Assert.AreEqual(t2, e4.T);
            Assert.AreEqual(t2, e5.T);
            Assert.AreEqual(t2, e6.T);
        }
    }
}
