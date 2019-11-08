namespace Util.Geometry.DCEL.Tests
{
    using NUnit.Framework;
    using UnityEngine;
    using Util.Geometry.DCEL;

    [TestFixture]
    public class DCELVertexTest
    {
        [Test]
        public void PosTest()
        {
            var v = new DCELVertex(0, 1);
            var v2 = new DCELVertex(new Vector2(0, 1));
            Assert.AreEqual(new Vector2(0, 1), v.Pos);
            Assert.AreEqual(v.Pos, v2.Pos);
        }

        [Test]
        public void LeavingTest()
        {
            var v = new DCELVertex(0, 1);
            var v2 = new DCELVertex(new Vector2(1, 2));
            var e = new HalfEdge(v, v2);
            var v3 = new DCELVertex(new Vector2(0, 1), e);
            v.Leaving = e;

            Assert.AreEqual(v.Leaving, v3.Leaving);
        }
    }
}
