using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace DotsAndPolygons.Tests.SoCG
{
    using static HelperFunctions;

    public class WinningStrategy
    {
        private static IEnumerable<IDotsVertex> GetVerticesInConvexPosition(int amount, Vector2? center = null,
            float radius = 1f)
        {
            Vector2 _center = center ?? Vector2.zero;
            var angleBetweenVertices = 2f * Mathf.PI / amount;
            var vertices = new List<IDotsVertex>();

            for (var i = 0; i < amount; i++)
            {
                var angle = i * angleBetweenVertices;
                Vector2 pos = _center + new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
                vertices.Add(new DotsVertex(pos));
            }

            return vertices;
        }

        private static Tuple<IDotsVertex, IDotsVertex> FindAMiddleLine(
            IEnumerable<IDotsVertex> vertices)
        {
            IDotsVertex vertexA = vertices.First();

            IDotsVertex vertexB = vertices.OrderBy(
                it => Mathf.Abs(Distance(vertexA.Coordinates, it.Coordinates))
            ).Last();
            
            return new Tuple<IDotsVertex, IDotsVertex>(vertexA, vertexB);
        }


        [Test]
        public void TestVerticesInConvexPos()
        {
            var vertices = GetVerticesInConvexPosition(4, new Vector2(0, 0), 2);
            var segments = new List<IDotsEdge>();

            var middleVertices = FindAMiddleLine(vertices);

            // if (vertices.Count() % 2 == 0)

            Assert.IsEmpty(string.Join("; ", middleVertices));
        }
    }
}