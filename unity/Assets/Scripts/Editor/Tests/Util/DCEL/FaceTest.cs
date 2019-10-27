namespace Util.Geometry.DCEL.Tests
{
    using UnityEngine;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;

    [TestFixture]
    public class FaceTest
    {
        [Test]
        public void OuterFaceTest()
        {
            var dcel = new DCEL();

            Assert.Zero(dcel.InnerFaces.Count());
            Assert.Zero(dcel.OuterFace.OuterVertices.Count());
            Assert.IsTrue(dcel.OuterFace.Contains(new Vector2(0, 0)));
        }

        [Test]
        public void ConstructionTest()
        {
            var segs = new List<LineSegment> {
                new LineSegment(new Vector2(-1, -1), new Vector2(1, 1))
            };

            var dcel = new DCEL(segs, new Rect(new Vector2(-1, -1), new Vector2(2, 2)));

            Assert.AreEqual(4, dcel.OuterFace.OuterVertices.Count());

            Assert.AreEqual(2, dcel.InnerFaces.Count());
            foreach (var f in dcel.InnerFaces)
            {
                Assert.AreEqual(3, f.Polygon.Vertices.Count());
            }
        }

        [Test]
        public void VerticalRightOfEdgeTest()
        {
            var downwards = new HalfEdge(new DCELVertex(0, 0), new DCELVertex(0, -10));
            var upwards = new HalfEdge(new DCELVertex(0, -10), new DCELVertex(0, 0));

            var left = new Vector2(-10, 0);
            var right = new Vector2(10, 0);

            Assert.AreEqual(true, downwards.IsRightOf(left));
            Assert.AreEqual(false, downwards.IsRightOf(right));

            Assert.AreEqual(false, upwards.IsRightOf(left));
            Assert.AreEqual(true, upwards.IsRightOf(right));
        }

        [Test]
        public void LeftSlopedVerticalRightOfEdgeTest()
        {
            var downwards = new HalfEdge(new DCELVertex(1, 0), new DCELVertex(0, -10));
            var upwards = new HalfEdge(new DCELVertex(1, -10), new DCELVertex(0, 0));

            var left = new Vector2(-10, 0);
            var right = new Vector2(10, 0);

            Assert.AreEqual(true, downwards.IsRightOf(left));
            Assert.AreEqual(false, downwards.IsRightOf(right));

            Assert.AreEqual(false, upwards.IsRightOf(left));
            Assert.AreEqual(true, upwards.IsRightOf(right));
        }

        [Test]
        public void RigthSlopedVerticalRightOfEdgeTest()
        {
            var downwards = new HalfEdge(new DCELVertex(-1, 0), new DCELVertex(0, -10));
            var upwards = new HalfEdge(new DCELVertex(-1, -10), new DCELVertex(0, 0));

            var left = new Vector2(-10, 0);
            var right = new Vector2(10, 0);

            Assert.AreEqual(true, downwards.IsRightOf(left));
            Assert.AreEqual(false, downwards.IsRightOf(right));

            Assert.AreEqual(false, upwards.IsRightOf(left));
            Assert.AreEqual(true, upwards.IsRightOf(right));
        }
    }
}

