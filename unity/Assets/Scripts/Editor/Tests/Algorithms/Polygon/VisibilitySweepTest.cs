namespace Util.Algorithms.Polygon.Tests
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Algorithms.Polygon;
    using Util.Geometry.Polygon;
    using Util.Math;

    [TestFixture]
    public class VisibilitySweepTest
    {
        private readonly Polygon2D rectPoly;
        private readonly Polygon2D diamondPoly;
        private readonly Polygon2DWithHoles holePoly;

        public VisibilitySweepTest()
        {

            rectPoly = new Polygon2D(new List<Vector2>()
            {
                new Vector2(-2, -2),
                new Vector2(-2, 2),
                new Vector2(2, 2),
                new Vector2(2, -2)
            });
            diamondPoly = new Polygon2D(new List<Vector2>()
            {
                new Vector2(0, -1),
                new Vector2(-1, 0),
                new Vector2(0, 1),
                new Vector2(1, 0)
            });

            holePoly = new Polygon2DWithHoles(rectPoly,
                new List<Polygon2D>(){ diamondPoly }
            );
        }

        [Test]
        public void VisionTest()
        {
            var vision = VisibilitySweep.Vision(holePoly, new Vector2(-1.5f, -1.5f));
            Debug.Log(vision);
        }

        [Test]
        public void ColinearTest()
        {
            var vision = VisibilitySweep.Vision(holePoly, new Vector2(-0.5f, -1.5f));
            Debug.Log(vision);
        }

        [Test]
        public void OnVertexTest()
        {
            var vision = VisibilitySweep.Vision(holePoly, new Vector2(-2, -2));
            Debug.Log(vision);
        }

        [Test]
        public void ContainsTest()
        {
            // check if exception is thrown when given point outside polygon
            Assert.Throws<ArgumentException>(() => VisibilitySweep.Vision(holePoly, new Vector2(-5f, 0)));
        }

        [Test]
        public void OnBoundaryTest()
        {
            var vision = VisibilitySweep.Vision(holePoly, new Vector2(0.5f, 0.5f));
            Debug.Log(vision);
        }
    }
}
