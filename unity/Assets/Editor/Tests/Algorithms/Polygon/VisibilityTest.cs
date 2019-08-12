namespace Util.Algorithms.Polygon.Tests
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using UnityEngine;
    using Util.Geometry.Polygon;
    using Util.Algorithms.Polygon;
    using System;

    [TestFixture]
    public class VisibilityTest
    {
        private readonly Polygon2DWithHoles arrowPoly;
        private readonly Polygon2DWithHoles diamondPoly;
        private readonly Polygon2DWithHoles holePoly;

        public VisibilityTest()
        {
            var m_topVertex = new Vector2(0, 1);
            var m_botVertex = new Vector2(0, -1);
            var m_leftVertex = new Vector2(-1, 0);
            var m_rightVertex = new Vector2(1, 0);
            var m_farRightVertex = new Vector2(2, 0);

            arrowPoly = new Polygon2DWithHoles(new Polygon2D(new List<Vector2>()
            {
                m_topVertex, m_farRightVertex, m_botVertex, m_rightVertex
            }));
            diamondPoly = new Polygon2DWithHoles(new Polygon2D(new List<Vector2>()
            {
                m_topVertex, m_rightVertex, m_botVertex, m_leftVertex
            }));

            holePoly = new Polygon2DWithHoles(
                new Polygon2D(new List<Vector2>() {
                    new Vector2(0f, 0f),
                    new Vector2(0f, 6f),
                    new Vector2(6f, 6f),
                    new Vector2(6f, 0f)
                }),
                new List<Polygon2D>()
                {
                    new Polygon2D(new List<Vector2>()
                    {
                        new Vector2(2f, 2f),
                        new Vector2(4f, 2f),
                        new Vector2(4f, 4f),
                        new Vector2(2f, 4f)
                    })
                });
        }
       
        [Test]
        public void AreaTest()
        {
            var vision = Visibility.Vision(arrowPoly, new Vector2(1.5f, 0));
            Debug.Log(arrowPoly.Outside);
            Debug.Log(vision);
            Assert.AreEqual(arrowPoly.Area(), vision.Area());

            vision = Visibility.Vision(diamondPoly, Vector2.zero);
            Assert.AreEqual(diamondPoly.Area(), vision.Area());
        }

        [Test]
        public void HoleTest()
        {
            var vision = Visibility.Vision(holePoly, new Vector2(1f, 3f));
            Debug.Log(vision);
            var expected = new Polygon2D(new List<Vector2>()
            {
                new Vector2(2f, 2f),
                new Vector2(4f, 0f),
                new Vector2(0f, 0f),
                new Vector2(0f, 6f),
                new Vector2(4f, 6f),
                new Vector2(2f, 4f)
            });
            Assert.True(vision.Equals(expected));

            vision = Visibility.Vision(holePoly, new Vector2(1f, 2f));
            Debug.Log(vision);
            expected = new Polygon2D(new List<Vector2>()
            {
                new Vector2(6f, 0f),
                new Vector2(0f, 0f),
                new Vector2(0f, 6f),
                new Vector2(3f, 6f),
                new Vector2(2f, 4f),
                new Vector2(2f, 2f),
                new Vector2(6f, 2f)
            });
            Assert.True(vision.Equals(expected));
        }

        [Test]
        public void ContainsTest()
        {
            // check if exception is thrown when given point outside polygon
            Assert.Throws<ArgumentException>(() => Visibility.Vision(arrowPoly, new Vector2(-1f, 0)));
        }
    }
}
