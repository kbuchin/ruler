using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Util.Geometry;
using Util.Math;

namespace DotsAndPolygons.Tests
{
    using static HelperFunctions;

    public class HalfEdge
    {
        private static HashSet<IDotsHalfEdge> SimpleEdge()
        {
            var halfEdges = new HashSet<IDotsHalfEdge>();

            var a = new DotsVertex
            (
                new Vector2(0, 0)
            );
            var b = new DotsVertex
            (
                new Vector2(1, 0)
            );

            AddEdge(a, b, 1, halfEdges, new[] {a, b}, GameMode.GameMode2);

            IDotsHalfEdge ab = halfEdges.First(it => it.Origin == a);
            IDotsHalfEdge ba = halfEdges.First(it => it.Origin == b);

            ab.Name = "ab";
            ba.Name = "ba";

            return halfEdges;
        }

        [Test]
        public void AngleTest()
        {
            var a = new DotsVertex
            (
                new Vector2(0, 0)
            );
            var b = new DotsVertex
            (
                new Vector2(1, 0)
            );
            var c = new DotsVertex
            (
                new Vector2(0, 1)
            );

            var edges = new HashSet<IDotsHalfEdge>();

            AddEdge(a, b, 1, edges, new List<DotsVertex>(), GameMode.GameMode2);

            IDotsHalfEdge ab = edges.First(it => it.Origin == a);
            IDotsHalfEdge ba = edges.First(it => it.Origin == b);

            ab.Name = "ab";
            ba.Name = "ba";

            AddEdge(b, c, 1, edges, new List<DotsVertex>(), GameMode.GameMode2);

            IDotsHalfEdge bc = edges.First(it => it.Origin == b && it.Name == null);
            IDotsHalfEdge cb = edges.First(it => it.Origin == c && it.Name == null);

            bc.Name = "bc";
            cb.Name = "cb";

            AddEdge(a, c, 1, edges, new List<DotsVertex>(), GameMode.GameMode2);

            IDotsHalfEdge ac = edges.First(it => it.Origin == a && it.Name == null);
            IDotsHalfEdge ca = edges.First(it => it.Origin == c && it.Name == null);

            ac.Name = "ac";
            ca.Name = "ca";


            Assert.AreEqual(
                315,
                AngleVertices(ab, bc)
            );

            Assert.AreEqual(
                45,
                AngleVertices(cb, ba)
            );

            Assert.AreEqual(
                90,
                AngleVertices(ba, ac)
            );

            Assert.AreEqual(
                45,
                AngleVertices(ac, cb)
            );
        }

        [Test]
        public void SimpleTest()
        {
            HashSet<IDotsHalfEdge> edges = SimpleEdge();

            Assert.AreEqual(
                2,
                edges.Count
            );

            IDotsHalfEdge ab = edges.First(it => it.Name == "ab");
            IDotsHalfEdge ba = edges.First(it => it.Name == "ba");

            Assert.AreEqual(
                ba,
                ab.Prev
            );
            Assert.AreEqual(
                ba,
                ab.Next
            );
            Assert.AreEqual(
                ab,
                ba.Prev
            );
            Assert.AreEqual(
                ab,
                ba.Next
            );
        }

        [Test]
        public void ComplexerTest()
        {
            HashSet<IDotsHalfEdge> edges = SimpleEdge();

            IDotsHalfEdge ab = edges.First(it => it.Name == "ab");
            IDotsHalfEdge ba = edges.First(it => it.Name == "ba");

            var a = (DotsVertex) ab.Origin;
            var b = (DotsVertex) ba.Origin;

            var c = new DotsVertex
            (
                new Vector2(0, 1)
            );

            AddEdge(b, c, 1, edges, new List<DotsVertex>(), GameMode.GameMode2);

            IDotsHalfEdge bc = edges.First(it => it.Origin == b && it.Name == null);
            IDotsHalfEdge cb = edges.First(it => it.Origin == c && it.Name == null);

            bc.Name = "bc";
            cb.Name = "cb";

            Assert.AreEqual(
                ba,
                cb.Next
            );
            Assert.AreEqual(
                ab,
                bc.Prev
            );
            Assert.AreEqual(
                bc,
                ab.Next
            );
            Assert.AreEqual(
                cb,
                ba.Prev
            );
        }

        [Test]
        public void EvenComplexerTest()
        {
            HashSet<IDotsHalfEdge> edges = SimpleEdge();

            IDotsHalfEdge ab = edges.First(it => it.Name == "ab");
            IDotsHalfEdge ba = edges.First(it => it.Name == "ba");

            var a = (DotsVertex) ab.Origin;
            var b = (DotsVertex) ba.Origin;

            var c = new DotsVertex
            (
                new Vector2(0, 1)
            );

            AddEdge(b, c, 1, edges, new List<DotsVertex>(), GameMode.GameMode2);

            IDotsHalfEdge bc = edges.First(it => it.Origin == b && it.Name == null);
            IDotsHalfEdge cb = edges.First(it => it.Origin == c && it.Name == null);

            bc.Name = "bc";
            cb.Name = "cb";

            AddEdge(a, c, 1, edges, new List<DotsVertex>(), GameMode.GameMode2);

            IDotsHalfEdge ac = edges.First(it => it.Origin == a && it.Name == null);
            IDotsHalfEdge ca = edges.First(it => it.Origin == c && it.Name == null);

            ac.Name = "ac";
            ca.Name = "ca";

            Assert.AreEqual(
                6,
                edges.Count
            );

            //a and b
            Assert.AreEqual(
                ca,
                ab.Prev
            );
            Assert.AreEqual(
                bc,
                ab.Next
            );
            Assert.AreEqual(
                cb,
                ba.Prev
            );
            Assert.AreEqual(
                ac,
                ba.Next
            );

            //b and c
            Assert.AreEqual(
                ca,
                bc.Next
            );
            Assert.AreEqual(
                ab,
                bc.Prev
            );
            Assert.AreEqual(
                ba,
                cb.Next
            );
            Assert.AreEqual(
                ac,
                cb.Prev
            );

            //a and c
            Assert.AreEqual(
                cb,
                ac.Next
            );
            Assert.AreEqual(
                ba,
                ac.Prev
            );
            Assert.AreEqual(
                ab,
                ca.Next
            );
            Assert.AreEqual(
                bc,
                ca.Prev
            );
        }

        [Test]
        public void ComplexestTest()
        {
            HashSet<IDotsHalfEdge> edges = SimpleEdge();

            IDotsHalfEdge ab = edges.First(it => it.Name == "ab");
            IDotsHalfEdge ba = edges.First(it => it.Name == "ba");

            var a = (DotsVertex) ab.Origin;
            var b = (DotsVertex) ba.Origin;

            var c = new DotsVertex
            (
                new Vector2(0, 1)
            );

            AddEdge(b, c, 1, edges, new List<DotsVertex>(), GameMode.GameMode2);

            IDotsHalfEdge bc = edges.First(it => it.Origin == b && it.Name == null);
            IDotsHalfEdge cb = edges.First(it => it.Origin == c && it.Name == null);

            bc.Name = "bc";
            cb.Name = "cb";

            AddEdge(a, c, 1, edges, new List<DotsVertex>(), GameMode.GameMode2);

            IDotsHalfEdge ac = edges.First(it => it.Origin == a && it.Name == null);
            IDotsHalfEdge ca = edges.First(it => it.Origin == c && it.Name == null);

            ac.Name = "ac";
            ca.Name = "ca";

            var d = new DotsVertex
            (
                new Vector2(-1, 0)
            );

            AddEdge(d, a, 1, edges, new List<DotsVertex>(), GameMode.GameMode2);

            IDotsHalfEdge ad = edges.First(it => it.Origin == a && it.Name == null);
            IDotsHalfEdge da = edges.First(it => it.Origin == d && it.Name == null);

            ad.Name = "ad";
            da.Name = "da";

            Assert.AreEqual(
                8,
                edges.Count
            );


            //a and b
            Assert.AreEqual(
                da.Name,
                ab.Prev.Name
            );
            Assert.AreEqual(
                bc,
                ab.Next
            );
            Assert.AreEqual(
                cb,
                ba.Prev
            );
            Assert.AreEqual(
                ac,
                ba.Next
            );

            //a and c
            Assert.AreEqual(
                cb,
                ac.Next
            );
            Assert.AreEqual(
                ba,
                ac.Prev
            );
            Assert.AreEqual(
                ad.Name,
                ca.Next.Name
            );
            Assert.AreEqual(
                bc,
                ca.Prev
            );

            //a and d
            Assert.AreEqual(
                da,
                ad.Next
            );
            Assert.AreEqual(
                ca,
                ad.Prev
            );
            Assert.AreEqual(
                ab,
                da.Next
            );
        }
        
        [Test]
        public void EvenComplexerThanMostComplexestTest()
        {
            var o = new DotsVertex
            (
                new Vector2(0, 0)
            );

            var a1 = new DotsVertex
            (
                new Vector2(2, 1)
            );

            var a2 = new DotsVertex
            (
                new Vector2(1, 2)
            );

            var b1 = new DotsVertex
            (
                new Vector2(3, 0)
            );

            var b2 = new DotsVertex
            (
                new Vector2(3, 3)
            );

            var b3 = new DotsVertex
            (
                new Vector2(0, 3)
            );

            var allVertices = new List<IDotsVertex> {o, a1, a2, b1, b2, b3};
            var allHalfEdges = new HashSet<IDotsHalfEdge>();
            Assert.IsFalse(AddEdge(o, a1, 0, allHalfEdges, allVertices, GameMode.GameMode2));
            Assert.IsFalse(AddEdge(a1, a2, 0, allHalfEdges, allVertices, GameMode.GameMode2));
            Assert.IsTrue(AddEdge(a2, o, 0, allHalfEdges, allVertices, GameMode.GameMode2));


            Assert.IsFalse(AddEdge(o, b3, 0, allHalfEdges, allVertices, GameMode.GameMode2));
            Assert.IsFalse(AddEdge(b3, b2, 0, allHalfEdges, allVertices, GameMode.GameMode2));
            Assert.IsFalse(AddEdge(b2, b1, 0, allHalfEdges, allVertices, GameMode.GameMode2));
            Assert.IsFalse(AddEdge(b1, o, 0, allHalfEdges, allVertices, GameMode.GameMode2));
        }

        [Test]
        public void EvenComplexerThanMostComplexestTest2()
        {
            var o = new DotsVertex
            (
                new Vector2(-5, 0)
            );

            var a1 = new DotsVertex
            (
                new Vector2(-4, -1)
            );

            var a2 = new DotsVertex
            (
                new Vector2(-4, 0)
            );

            var b1 = new DotsVertex
            (
                new Vector2(-2, 1)
            );

            var b2 = new DotsVertex
            (
                new Vector2(-2, -2)
            );

            var b3 = new DotsVertex
            (
                new Vector2(-4, -2)
            );

            var allVertices = new List<IDotsVertex> {o, a1, a2, b1, b2, b3};
            var allHalfEdges = new HashSet<IDotsHalfEdge>();
            Assert.IsFalse(AddEdge(o, a1, 0, allHalfEdges, allVertices, GameMode.GameMode2));
            Assert.IsFalse(AddEdge(a1, a2, 0, allHalfEdges, allVertices, GameMode.GameMode2));
            Assert.IsTrue(AddEdge(a2, o, 0, allHalfEdges, allVertices, GameMode.GameMode2));


            Assert.IsFalse(AddEdge(o, b1, 0, allHalfEdges, allVertices, GameMode.GameMode2));
            Assert.IsFalse(AddEdge(b1, b2, 0, allHalfEdges, allVertices, GameMode.GameMode2));
            Assert.IsFalse(AddEdge(b2, b3, 0, allHalfEdges, allVertices, GameMode.GameMode2));
            Assert.IsFalse(AddEdge(b3, o, 0, allHalfEdges, allVertices, GameMode.GameMode2));
        }

        [Test]
        public void SimpleAngleTest()
        {
            var top = new Vector2(0, 1);
            var center = new Vector2(0, 0);
            var right = new Vector2(1, 0);

            var topCenter = new DotsHalfEdge
            {
                Origin = new DotsVertex
                (
                    top
                )
            };
            var centerTop = new DotsHalfEdge
            {
                Origin = new DotsVertex
                (
                    center
                ),
                Twin = topCenter
            };
            topCenter.Twin = centerTop;
            var centerRight = new DotsHalfEdge
            {
                Origin = new DotsVertex
                (
                    center
                )
            };
            var rightCenter = new DotsHalfEdge
            {
                Origin = new DotsVertex
                (
                    right
                ),
                Twin = centerRight
            };
            centerRight.Twin = rightCenter;

            // top -> center -> right
            double angle = MathUtil.Angle(center, top, right);
            double angle2 = AngleVertices(topCenter, centerRight);
            double angle2a = AngleVertices(top, center, right);

            Assert.AreEqual(Math.Round(angle * 180.0 / Math.PI), Math.Round(angle2));
            Assert.AreEqual(Math.Round(angle), Math.Round(angle2 * Math.PI / 180.0));
            Assert.AreEqual(angle2, angle2a);

            // right -> center -> top
            double angle3 = MathUtil.Angle(center, right, top);
            double angle4 = AngleVertices(rightCenter, centerTop);
            double angle4a = AngleVertices(right, center, top);

            Assert.AreEqual(Math.Round(angle3 * 180.0 / Math.PI), Math.Round(angle4));
            Assert.AreEqual(Math.Round(angle3), Math.Round(angle4 * Math.PI / 180.0));
            Assert.AreEqual(angle4, angle4a);

            // right -> center -> top alternative
            double angle5 = MathUtil.Angle(center, right, top);
            double angle6 = AngleVertices(centerRight, centerTop);

            Assert.AreEqual(Math.Round(angle5 * 180.0 / Math.PI), Math.Round(angle6));
            Assert.AreEqual(Math.Round(angle5), Math.Round(angle6 * Math.PI / 180.0));
        }
    }
}