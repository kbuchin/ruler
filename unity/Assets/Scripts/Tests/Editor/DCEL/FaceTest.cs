using System;
using Algo;
using Algo.DCEL;
using UnityEngine;
using NUnit.Framework;


namespace Algo.DCEL.Tests
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

            Assert.AreNotEqual(dcel.outerface, innerFace);
            Assert.AreEqual(true, test);
        }

        [Test]
        public void VerticalRightOfEdgeTest()
        {
            var downwards = new Halfedge(new Vector2(0, 0), new Vector2(0, -10));
            var upwards = new Halfedge(new Vector2(0, -10), new Vector2(0, 0));

            var left = new Vector2(-10, 0);
            var right = new Vector2(10, 0);

            Assert.AreEqual(true, downwards.pointIsRightOf(left));
            Assert.AreEqual(false, downwards.pointIsRightOf(right));

            Assert.AreEqual(false, upwards.pointIsRightOf(left));
            Assert.AreEqual(true, upwards.pointIsRightOf(right));
        }

        [Test]
        public void LeftSlopedVerticalRightOfEdgeTest()
        {
            var downwards = new Halfedge(new Vector2(1, 0), new Vector2(0, -10));
            var upwards = new Halfedge(new Vector2(1, -10), new Vector2(0, 0));

            var left = new Vector2(-10, 0);
            var right = new Vector2(10, 0);

            Assert.AreEqual(true, downwards.pointIsRightOf(left));
            Assert.AreEqual(false, downwards.pointIsRightOf(right));

            Assert.AreEqual(false, upwards.pointIsRightOf(left));
            Assert.AreEqual(true, upwards.pointIsRightOf(right));
        }



        [Test]
        public void RigthSlopedVerticalRightOfEdgeTest()
        {
            var downwards = new Halfedge(new Vector2(-1, 0), new Vector2(0, -10));
            var upwards = new Halfedge(new Vector2(-1, -10), new Vector2(0, 0));

            var left = new Vector2(-10, 0);
            var right = new Vector2(10, 0);

            Assert.AreEqual(true, downwards.pointIsRightOf(left));
            Assert.AreEqual(false, downwards.pointIsRightOf(right));

            Assert.AreEqual(false, upwards.pointIsRightOf(left));
            Assert.AreEqual(true, upwards.pointIsRightOf(right));
        }
    }
}

