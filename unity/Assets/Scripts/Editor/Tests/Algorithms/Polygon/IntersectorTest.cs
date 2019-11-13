namespace Util.Algorithms.Polygon.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry;
    using Util.Geometry.Polygon;

    [TestFixture]
    class IntersectorTest
    {
        private readonly List<Vector2> m_arrowVertices;
        private readonly List<Vector2> m_diamondVertices;

        private readonly Polygon2D m_arrow;
        private readonly Polygon2D m_diamond;
        private readonly Polygon2D m_squarePoly;

        public IntersectorTest()
        {
            var m_topVertex = new Vector2(0, 1);
            var m_botVertex = new Vector2(0, -1);
            var m_leftVertex = new Vector2(-1, 0);
            var m_rightVertex = new Vector2(1, 0);
            var m_farRightVertex = new Vector2(2, 0);

            m_arrowVertices = new List<Vector2>()
            {
                m_topVertex, m_farRightVertex, m_botVertex, m_rightVertex
            };
            m_diamondVertices = new List<Vector2>()
            {
                m_topVertex, m_rightVertex, m_botVertex, m_leftVertex
            };

            m_arrow = new Polygon2D(m_arrowVertices);
            m_diamond = new Polygon2D(m_diamondVertices);

            m_squarePoly = new Polygon2D(new List<Vector2>()
            {
                new Vector2(0, 0), new Vector2(0, 2), new Vector2(2, 2), new Vector2(2, 0)
            });
        }

        [Test]
        public void IntersectConvexTest()
        {
            
            var intersect = Intersector.IntersectConvex(m_diamond, m_squarePoly);
            Assert.AreEqual(0.5f, intersect.Area);

            Assert.Throws<GeomException>(() => Intersector.IntersectConvex(m_diamond, m_arrow));
        }
    }
}
