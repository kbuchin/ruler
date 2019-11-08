namespace Util.Geometry.DCEL.Tests
{
    using NUnit.Framework;
    using Util.Geometry.DCEL;

    [TestFixture]
    public class HalfEdgeTest
    {
        private readonly DCELVertex v, v2;
        private readonly HalfEdge e, eTwin;

        public HalfEdgeTest()
        {
            v = new DCELVertex(0, 1);
            v2 = new DCELVertex(4, 4);
            e = new HalfEdge(v, v2);
            eTwin = new HalfEdge(v2, v);
            e.Twin = eTwin;
            eTwin.Twin = e;
            e.Next = e.Prev = eTwin;
            eTwin.Next = eTwin.Prev = e;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.AreEqual(v, e.From);
            Assert.AreEqual(v2, e.To);
            Assert.AreEqual(v.Pos, e.Segment.Point1);
            Assert.AreEqual(v2.Pos, e.Segment.Point2);
        }

        [Test]
        public void NextPrevTest()
        {
            Assert.AreEqual(eTwin, e.Next);
            Assert.AreEqual(eTwin, e.Prev);
            Assert.AreEqual(e, eTwin.Next);
            Assert.AreEqual(e, eTwin.Prev);
        }

        [Test]
        public void TwinTest()
        {
            Assert.AreEqual(eTwin, e.Twin);
            Assert.AreEqual(e, eTwin.Twin);
        }

        [Test]
        public void MagnitudeTest()
        {
            Assert.AreEqual(5f, e.Magnitude);
            Assert.AreEqual(25f, e.SqrMagnitude);
        }
    }
}
