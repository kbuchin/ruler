using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Algo.Polygons;

namespace Algo.DCEL.Vision.Tests
{
    [TestFixture]
    public class VisionTest
    {

        private static Vector2 m_topVertex = new Vector2(0, 1);
        private static Vector2 m_botVertex = new Vector2(0, -1);
        private static Vector2 m_leftVertex = new Vector2(-1, 0);
        private static Vector2 m_rightVertex = new Vector2(1, 0);
        private static Vector2 m_farRightVertex = new Vector2(2, 0);


        private static List<Vector2> m_arrowVertices = new List<Vector2>()
        {
            m_topVertex, m_farRightVertex, m_botVertex, m_rightVertex
        };
        private static List<Vector2> m_diamondVertices = new List<Vector2>()
        {
            m_topVertex, m_rightVertex, m_botVertex, m_leftVertex
        };

       
        [Test]
        public void ConstructorTest()
        {

            var poly = new VertexHolePolygon(new VertexSimplePolygon(m_arrowVertices));
            Assert.AreEqual(poly.Area(), poly.VisibleArea(new Vector2(1.5f , 0)));

            poly = new VertexHolePolygon(new VertexSimplePolygon(m_diamondVertices));
            Assert.AreEqual(poly.Area(), poly.VisibleArea(Vector2.zero));
        }

    }
}
