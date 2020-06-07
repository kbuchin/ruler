using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DotsAndPolygons.Tests
{
    using static HelperFunctions;

    public class FaceLoop
    {
        private static HashSet<DotsHalfEdge> GetFullTriangle()
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

            var allHalfEdges = new HashSet<DotsHalfEdge>();
            var allVertices = new HashSet<DotsVertex> {a, b, c};

            AddEdge(a, b, 1, allHalfEdges, allVertices, GameMode.GameMode2);
            AddEdge(b, c, 1, allHalfEdges, allVertices, GameMode.GameMode2);
            AddEdge(c, a, 1, allHalfEdges, allVertices, GameMode.GameMode2);

            return allHalfEdges;
        }

        private static DotsHalfEdge GetFullTriangleAB()
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

            var allHalfEdges = new HashSet<DotsHalfEdge>();
            var allVertices = new HashSet<DotsVertex> {a, b, c};

            AddEdge(a, b, 1, allHalfEdges, allVertices, GameMode.GameMode2);
            AddEdge(b, c, 1, allHalfEdges, allVertices, GameMode.GameMode2);
            AddEdge(c, a, 1, allHalfEdges, allVertices, GameMode.GameMode2);

            return allHalfEdges.First(it => it.Origin == a);
        }

        [Test]
        public void FaceLoop1()
        {
            Assert.True(GetFullTriangleAB().IncidentFace == null ^ GetFullTriangleAB().Twin.IncidentFace == null);
        }

        [Test]
        public void FaceLoop2()
        {
            HashSet<DotsHalfEdge> edges = GetFullTriangle();

            DotsHalfEdge ab = edges.First(it =>
                Math.Abs(it.Origin.Coordinates[0]) < BIETJE &&
                Math.Abs(it.Origin.Coordinates[1]) < BIETJE);


            Assert.True(ab.IncidentFace == null ^ ab.Twin.IncidentFace == null); // Calculate face
            DotsVertex a = ab.Origin;

            var alpha = new DotsVertex
            (
                new Vector2(-1, 0)
            );

            AddEdge(alpha, a, 1, edges, new List<DotsVertex>(), GameMode.GameMode2);

            Assert.AreEqual(
                8,
                edges.Count
            );

            Assert.AreEqual(
                ab,
                edges.First(it => it.Origin == alpha).Next
            );
        }

        [Test]
        public void FaceLoop3()
        {
            DotsHalfEdge ab = GetFullTriangleAB();
            Assert.True(ab.IncidentFace == null ^ ab.Twin.IncidentFace == null); // Calculate face
            DotsVertex a = ab.Origin;

            var alpha = new DotsVertex
            (
                new Vector2(-1, 0)
            );

            var edges = new HashSet<DotsHalfEdge>();
            AddEdge(a, alpha, 1, edges, new List<DotsVertex>(), GameMode.GameMode2);

            Assert.AreEqual(
                2,
                edges.Count
            );
        }
    }
}