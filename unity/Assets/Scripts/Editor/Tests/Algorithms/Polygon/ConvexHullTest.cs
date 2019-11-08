namespace Util.Algorithms.Polygon.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry.Polygon;

    [TestFixture]
    public class ConvexHullTest
    {
        private readonly List<Vector2> m_vertices;
        private readonly IPolygon2D m_polygon;

        private readonly List<Vector2> m_upperhull;
        private readonly List<Vector2> m_lowerhull;
        private readonly Polygon2D m_hull;

        public ConvexHullTest()
        {
            m_vertices = new List<Vector2>()
            {
                new Vector2(0, 0),
                new Vector2(1, 1),
                new Vector2(0, 2),
                new Vector2(3, 2),
                new Vector2(2, -1),
                new Vector2(0, -1),
                new Vector2(-1, -2),
                new Vector2(-3, -1),
                new Vector2(-1, -0.5f),
                new Vector2(-4, 1),
                new Vector2(-2, 3),
            };

            m_polygon = new Polygon2D(m_vertices);

            m_upperhull = new List<Vector2>()
            {
                m_vertices[9], m_vertices[10], m_vertices[3]
            };
            m_lowerhull = new List<Vector2>()
            {
                m_vertices[9], m_vertices[7], m_vertices[6], m_vertices[4], m_vertices[3]
            };

            m_hull = new Polygon2D(new List<Vector2>()
            {
                m_vertices[9], m_vertices[10], m_vertices[3], // upperhull
                m_vertices[4], m_vertices[6], m_vertices[7] // lowerhull
            });
        }

        [Test]
        public void ComputeUpperHullTest()
        {
            var result = ConvexHull.ComputeUpperHull(m_vertices);
            Assert.AreEqual(m_upperhull, result);

            var result2 = ConvexHull.ComputeUpperHull(m_polygon);
            Assert.AreEqual(result, result2);
        }

        [Test]
        public void ComputeLowerHullTest()
        {
            var result = ConvexHull.ComputeLowerHull(m_vertices);
            Assert.AreEqual(m_lowerhull, result);

            var result2 = ConvexHull.ComputeLowerHull(m_polygon);
            Assert.AreEqual(result, result2);
        }

        [Test]
        public void ComputeConvexHullTest()
        {
            var result = ConvexHull.ComputeConvexHull(m_vertices);
            Assert.AreEqual(m_hull, result);

            var result2 = ConvexHull.ComputeConvexHull(m_polygon);
            Assert.AreEqual(result, result2);
        }
    }
}
