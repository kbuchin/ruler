using UnityEngine;
using NUnit.Framework;


namespace Util.Geometry.DCEL.Tests
{
    [TestFixture]
    public class FaceTest
    {
        [Test]
        public void ContainsTest()
        {
            var dcel = new DCEL(new Rect(-10, -10, 20, 20));

            var innerFace = dcel.Faces[1];

            var test = innerFace.Contains(new Vector2(0, 0));

            Assert.AreNotEqual(dcel.OuterFace, innerFace);
            Assert.AreEqual(true, test);
        }

        [Test]
        public void VerticalRightOfEdgeTest()
        {
            var downwards = new HalfEdge(new Vertex(0, 0), new Vertex(0, -10));
            var upwards = new HalfEdge(new Vertex(0, -10), new Vertex(0, 0));

            var left = new Vector2(-10, 0);
            var right = new Vector2(10, 0);

            Assert.AreEqual(true, downwards.PointIsRightOf(left));
            Assert.AreEqual(false, downwards.PointIsRightOf(right));

            Assert.AreEqual(false, upwards.PointIsRightOf(left));
            Assert.AreEqual(true, upwards.PointIsRightOf(right));
        }

        [Test]
        public void LeftSlopedVerticalRightOfEdgeTest()
        {
            var downwards = new HalfEdge(new Vertex(1, 0), new Vertex(0, -10));
            var upwards = new HalfEdge(new Vertex(1, -10), new Vertex(0, 0));

            var left = new Vector2(-10, 0);
            var right = new Vector2(10, 0);

            Assert.AreEqual(true, downwards.PointIsRightOf(left));
            Assert.AreEqual(false, downwards.PointIsRightOf(right));

            Assert.AreEqual(false, upwards.PointIsRightOf(left));
            Assert.AreEqual(true, upwards.PointIsRightOf(right));
        }



        [Test]
        public void RigthSlopedVerticalRightOfEdgeTest()
        {
            var downwards = new HalfEdge(new Vertex(-1, 0), new Vertex(0, -10));
            var upwards = new HalfEdge(new Vertex(-1, -10), new Vertex(0, 0));

            var left = new Vector2(-10, 0);
            var right = new Vector2(10, 0);

            Assert.AreEqual(true, downwards.PointIsRightOf(left));
            Assert.AreEqual(false, downwards.PointIsRightOf(right));

            Assert.AreEqual(false, upwards.PointIsRightOf(left));
            Assert.AreEqual(true, upwards.PointIsRightOf(right));
        }
    }
}

