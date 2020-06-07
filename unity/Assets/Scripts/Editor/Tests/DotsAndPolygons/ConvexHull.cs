﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Util.Geometry;

namespace DotsAndPolygons.Tests
{
    public class ConvexHull
    {
        [Test]
        public void ConvexHull1()
        {
            var a = new Vector2(0, 0);
            var b = new Vector2(1, 0.1f);
            var c = new Vector2(0.2f, 1.2f);
            var d = new Vector2(.5f, .5f);

            var vertices = new List<DotsVertex>
            {
                new DotsVertex(a),
                new DotsVertex(b),
                new DotsVertex(c),
                new DotsVertex(d)
            };

            HashSet<LineSegment> hull = ConvexHullHelper.ComputeHull(vertices);
            HelperFunctions.print($"Hull size: {hull.Count}");
            foreach (string s in hull.Select(it => it.ToString())) HelperFunctions.print(s);

            Assert.True(
                hull.Any(it => it.Equals(new LineSegment(a, b))) || hull.Any(it => it.Equals(new LineSegment(b, a)))
            );
            Assert.True(
                hull.Any(it => it.Equals(new LineSegment(b, c))) || hull.Any(it => it.Equals(new LineSegment(c, b)))
            );
            Assert.True(
                hull.Any(it => it.Equals(new LineSegment(c, a))) || hull.Any(it => it.Equals(new LineSegment(a, c)))
            );

            Assert.False(
                hull.Any(it => it.Equals(new LineSegment(a, d))) || hull.Any(it => it.Equals(new LineSegment(d, a)))
            );

            Assert.False(
                hull.Any(it => it.Equals(new LineSegment(b, d))) || hull.Any(it => it.Equals(new LineSegment(d, b)))
            );

            Assert.False(
                hull.Any(it => it.Equals(new LineSegment(c, d))) || hull.Any(it => it.Equals(new LineSegment(d, c)))
            );
        }


        [Test]
        public void ConvexHull2()
        {
            var a = new Vector2(0, 0);
            var b = new Vector2(1, 0.1f);
            var c = new Vector2(0.2f, 1.2f);
            var d = new Vector2(.56f, .56f);
            var e = new Vector2(.4f, .4f);
            var f = new Vector2(.3f, .3f);
            var g = new Vector2(.55f, .6f);
            var h = new Vector2(.45f, .3f);

            var vertices = new List<DotsVertex>
            {
                new DotsVertex(a),
                new DotsVertex(b),
                new DotsVertex(c),
                new DotsVertex(d),
                new DotsVertex(e),
                new DotsVertex(f),
                new DotsVertex(g),
                new DotsVertex(h
                )
            };

            HashSet<LineSegment> hull = ConvexHullHelper.ComputeHull(vertices);

            Assert.True(
                hull.Any(it => it.Equals(new LineSegment(a, b))) || hull.Any(it => it.Equals(new LineSegment(b, a)))
            );
            Assert.True(
                hull.Any(it => it.Equals(new LineSegment(b, c))) || hull.Any(it => it.Equals(new LineSegment(c, b)))
            );
            Assert.True(
                hull.Any(it => it.Equals(new LineSegment(c, a))) || hull.Any(it => it.Equals(new LineSegment(a, c)))
            );

            Assert.True(hull.Count == 3);
        }

        [Test]
        public void ConvexHull3()
        {
            var a = new Vector2(0, 0);
            var b = new Vector2(1, 0.1f);
            var c = new Vector2(0.2f, 1.2f);
            var d = new Vector2(.56f, .56f);
            var e = new Vector2(.4f, .4f);
            var f = new Vector2(.3f, .3f);
            var g = new Vector2(.55f, .6f);
            var h = new Vector2(.45f, .3f);
            var i = new Vector2(1.2f, 1.3f);

            var vertices = new List<DotsVertex>
            {
                new DotsVertex(a),
                new DotsVertex(b),
                new DotsVertex(c),
                new DotsVertex(d),
                new DotsVertex(e),
                new DotsVertex(f),
                new DotsVertex(g),
                new DotsVertex(h),
                new DotsVertex(i
                )
            };

            HashSet<LineSegment> hull = ConvexHullHelper.ComputeHull(vertices);
            HelperFunctions.print($"Hull size: {hull.Count}");
            foreach (string s in hull.Select(it => it.ToString())) HelperFunctions.print(s);

            Assert.True(
                hull.Any(it => it.Equals(new LineSegment(a, b))) || hull.Any(it => it.Equals(new LineSegment(b, a)))
            );
            Assert.True(
                hull.Any(it => it.Equals(new LineSegment(i, c))) || hull.Any(it => it.Equals(new LineSegment(c, i)))
            );
            Assert.True(
                hull.Any(it => it.Equals(new LineSegment(b, i))) || hull.Any(it => it.Equals(new LineSegment(i, b)))
            );
            Assert.True(
                hull.Any(it => it.Equals(new LineSegment(i, c))) || hull.Any(it => it.Equals(new LineSegment(c, i)))
            );

            Assert.True(hull.Count == 4);
        }
    }
}