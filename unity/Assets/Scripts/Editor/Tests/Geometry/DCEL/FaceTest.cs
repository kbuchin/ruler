namespace Util.Geometry.DCEL.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    [TestFixture]
    public class FaceTest
    {
        [Test]
        public void OuterFaceContainsTest()
        {
            var dcel = new DCEL();
            Assert.IsTrue(dcel.OuterFace.Contains(new Vector2(0, 0)));
        }

        [Test]
        public void ConstructionTest()
        {
            var segs = new List<LineSegment> {
                new LineSegment(new Vector2(-1, -1), new Vector2(1, 1))
            };

            var dcel = new DCEL(segs, new Rect(-1, -1, 2, 2));

            Assert.AreEqual(4, dcel.OuterFace.InnerHalfEdges.Count());

            Assert.AreEqual(2, dcel.InnerFaces.Count());
            foreach (var f in dcel.InnerFaces)
            {
                Assert.AreEqual(3, f.Polygon.Vertices.Count());
            }
        }


        [Test]
        public void AreaTest()
        {
            var segs = new List<LineSegment> {
                new LineSegment(new Vector2(-1, 0), new Vector2(1, 0))
            };

            var dcel = new DCEL(segs, new Rect(-1, -1, 2, 2));

            foreach (var face in dcel.InnerFaces)
            {
                Assert.AreEqual(2f, face.Area);
            }
        }

        [Test]
        public void BoundingBoxTest()
        {

        }
    }
}

