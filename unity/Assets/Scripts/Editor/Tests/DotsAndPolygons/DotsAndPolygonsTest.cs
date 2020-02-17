using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace DotsAndPolygons.Tests
{
    using static HelperFunctions;
    public class DotsAndPolygonsTest
    {
        private readonly HashSet<Vector2> _shortestPointDistancePoints = new HashSet<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(0, 5),
            new Vector2(2, 2),
            new Vector2(5, 5)
        };

        private readonly HashSet<Vector2> _generalPositionPoints = new HashSet<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(0, 5),
            new Vector2(5, 0),
            new Vector2(5, 5)
        };

        [Test]
        public void TestDistance1()
        {
            Assert.AreEqual(
                1,
                Distance(new Vector2(0, 0), new Vector2(0, 1))
            );
        }

        [Test]
        public void TestDistance2()
        {
            Assert.AreEqual(
                1,
                Distance(new Vector2(0, 0), new Vector2(1, 0))
            );
        }

        [Test]
        public void TestDistance3()
        {
            Assert.AreEqual(
                Mathf.Sqrt(26),
                Distance(new Vector2(4, 3), new Vector2(3, -2))
            );
        }

        [Test]
        public void TestShortestPointDistance1()
        {
            var point = new Vector2(2, 1);
            Assert.AreEqual(
                1,
                ShortestPointDistance(point, _shortestPointDistancePoints)
            );
        }

        [Test]
        public void TestShortestPointDistance2()
        {
            var point = new Vector2(4, 4);
            Assert.AreEqual(
                Mathf.Sqrt(2),
                ShortestPointDistance(point, _shortestPointDistancePoints)
            );
        }

        [Test]
        public void TestIsInGeneralPosition1()
        {
            var point = new Vector2(4.9f, 5f);
            Assert.IsFalse(
                IsInGeneralPosition(point, _generalPositionPoints)
            );
        }

        [Test]
        public void TestIsInGeneralPosition2()
        {
            var point = new Vector2(2, 1);
            Assert.IsTrue(
                IsInGeneralPosition(point, _generalPositionPoints)
            );
        }

        [Test]
        public void TestIsInGeneralPosition3()
        {
            var point = new Vector2(10, 5);
            Assert.IsFalse(
                IsInGeneralPosition(point, _generalPositionPoints)
            );
        }
    }
}