namespace Util.Geometry.Triangulation.Tests
{
    using NUnit.Framework;
    using UnityEngine;
    using Util.Math;

    [TestFixture]
    public class TriangleTest
    {
        private readonly Vector2 v1, v2, v3, v4;
        private readonly TriangleEdge e1, e2, e3;
        private readonly TriangleEdge e4, e5, e6;
        private readonly Triangle t1, t2;

        public TriangleTest()
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
        public void ConstructionTest()
        {
            Assert.AreEqual(3, t1.Vertices.Count);
            Assert.AreEqual(3, t1.Edges.Count);
        }

        [Test]
        public void VerticesTest()
        {
            Assert.AreEqual(v1, t1.P0);
            Assert.AreEqual(v2, t1.P1);
            Assert.AreEqual(v3, t1.P2);
        }

        [Test]
        public void TriangleEdgesTest()
        {
            Assert.AreEqual(e1, t1.E0);
            Assert.AreEqual(e2, t1.E1);
            Assert.AreEqual(e3, t1.E2);
        }

        [Test]
        public void AreaTest()
        {
            Assert.AreEqual(1f, t1.Area, MathUtil.EPS);
            Assert.AreEqual(t1.Area, t2.Area, MathUtil.EPS);
            Assert.AreEqual(0f, new Triangle().Area);
        }

        [Test]
        public void CircumcenterTest()
        {
            Assert.IsTrue(MathUtil.EqualsEps(new Vector2(1, 0), t1.Circumcenter.Value));
        }

        [Test]
        public void IsOuterTest()
        {
            Assert.IsTrue(t1.IsOuter);
            Assert.IsTrue(t2.IsOuter);
        }

        [Test]
        public void OtherEdgeTest()
        {
            Assert.AreEqual(e1, t1.OtherEdge(e2, e3));
            Assert.AreEqual(e2, t1.OtherEdge(e1, e3));
            Assert.AreEqual(e3, t1.OtherEdge(e1, v1));
            Assert.Throws<GeomException>(() => t1.OtherEdge(e1, e5));
        }

        [Test]
        public void OtherVertexTest()
        {
            Assert.AreEqual(v1, t1.OtherVertex(v3, v3));
            Assert.AreEqual(v2, t1.OtherVertex(v1, v3));
            Assert.AreEqual(v3, t1.OtherVertex(e1));
            Assert.Throws<GeomException>(() => t1.OtherVertex(v1, v4));
        }

        [Test]
        public void DegenerateTest()
        {
            Assert.IsFalse(t1.Degenerate);
            Assert.IsFalse(t2.Degenerate);
            Assert.IsTrue(new Triangle().Degenerate);
        }
    }
}
