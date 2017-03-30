using Algo.Polygons;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Algo.Polygons.Tests
{

    [TestFixture]
    class VertexSimplePolygonTest
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

        private VertexSimplePolygon m_arrow = new VertexSimplePolygon(m_arrowVertices);
        private VertexSimplePolygon m_diamond = new VertexSimplePolygon(m_diamondVertices);

        [Test]
        public void ArrowTriangulationTest()
        {

            var expectedTri1 = new List<Vector2> {m_topVertex, m_rightVertex, m_farRightVertex };
            var expectedTri2 = new List<Vector2> { m_botVertex, m_rightVertex, m_farRightVertex };

            var result = m_arrow.Triangulation;
            Assert.AreEqual(2, result.Count());
            Assert.True(AllAreTriangles(result));
            Assert.True(ContainsExpectedTriangle(result, expectedTri1));
            Assert.True(ContainsExpectedTriangle(result, expectedTri2));
        }

        [Test]
        public void DiamondTriangulationTest()
        {
            var expectedTri1 = new List<Vector2> { m_topVertex, m_botVertex, m_leftVertex };
            var expectedTri2 = new List<Vector2> { m_topVertex, m_botVertex, m_rightVertex };

            var result = m_diamond.Triangulation;
            Assert.AreEqual(2, result.Count());
            Assert.True(AllAreTriangles(result));
            Assert.True(ContainsExpectedTriangle(result, expectedTri1));
            Assert.True(ContainsExpectedTriangle(result, expectedTri2));
        }

        //Series of tests build around a bug
        static VertexSimplePolygon containsPoly = new VertexSimplePolygon(new List<Vector2>()
            {
                new Vector2(0,0),
                new Vector2(0,4),
                new Vector2(4,4),
                new Vector2(4,2),
                new Vector2(2,2),
                new Vector2(2,0)
            });
        [Test]
        public void ContainsTest1(){ Assert.True(containsPoly.Contains(new Vector2(1.5f, 1.5f)));}

        [Test]
        public void ContainsTest2() { Assert.True(containsPoly.Contains(new Vector2(2.5f, 2.5f))); }

        //[Test] Edge case  
        //public void ContainsTest3() { Assert.True(containsPoly.Contains(new Vector2(2f, 2f))); }

        [Test]
        public void ContainsTest4() { Assert.False(containsPoly.Contains(new Vector2(2.5f, 1.5f))); }

        [Test]
        public void AreaTest()
        {
            Assert.AreEqual(1, m_arrow.Area());
            Assert.AreEqual(2, m_diamond.Area());
        }





        static List<Vector2> m_horizontalRectVertices = new List<Vector2>()
            {
                new Vector2(-2, 1), new Vector2(2, 1), new Vector2(2, -1), new Vector2(-2,-1)
            };
        static List<Vector2> m_verticalRectVertices = new List<Vector2>()
            {
                new Vector2(-1,2), new Vector2(1, 2), new Vector2(1, 0), new Vector2(-1,0)
            };

        static VertexSimplePolygon m_horizontalRect = new VertexSimplePolygon(m_horizontalRectVertices);
        static VertexSimplePolygon m_verticalRect = new VertexSimplePolygon(m_verticalRectVertices);

        [Test]
        public void CutOutTest1()
        {
            var cutout = VertexSimplePolygon.CutOut(m_verticalRect, m_horizontalRect);
            Assert.AreEqual(2f, cutout.Area());
        }

        [Test]
        public void CutOutTest2()
        {
            var cutout = VertexSimplePolygon.CutOut(m_horizontalRect, m_verticalRect);
            Assert.AreEqual(6f, cutout.Area());
        }

        static List<Vector2> m_2by1RectVertices = new List<Vector2>()
            { new Vector2(0, 1), new Vector2(2, 1), new Vector2(2, 0), new Vector2(0, 0)  };
        static List<Vector2> m_1by2RectVertices = new List<Vector2>()
            { new Vector2(0, 2), new Vector2(1, 2), new Vector2(1, 0), new Vector2(0, 0)  };
        static List<Vector2> m_unitSquareVertices = new List<Vector2>()
            { new Vector2(0,1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0) };

        VertexSimplePolygon m_2by1rect = new VertexSimplePolygon(m_2by1RectVertices);
        VertexSimplePolygon m_1by2rect = new VertexSimplePolygon(m_1by2RectVertices);
        VertexSimplePolygon m_unitSquare = new VertexSimplePolygon(m_unitSquareVertices);


        [Test]
        public void CutOutRectFromSquareCollinearTest1()
        {
            var remainder = VertexSimplePolygon.CutOut(m_unitSquare, m_2by1rect);
            Assert.AreEqual(0f, remainder.Area());
        }

        [Test]
        public void CutOutSquareFromRectCollinearTest1()
        {
            var remainder = VertexSimplePolygon.CutOut(m_2by1rect, m_unitSquare);
            Assert.AreEqual(1f, remainder.Area());
        }

        [Test]
        public void CutOutRectFromSquareCollinearTest2()
        {
            var remainder = VertexSimplePolygon.CutOut(m_unitSquare, m_1by2rect);
            Assert.AreEqual(0f, remainder.Area());
        }

        [Test]
        public void CutOutSquareFromRectCollinearTest2()
        {
            var remainder = VertexSimplePolygon.CutOut(m_1by2rect, m_unitSquare);
            Assert.AreEqual(1f, remainder.Area());
        }

        [Test]
        public void CutOutRectFromRectCollinearTest1()
        {
            var remainder = VertexSimplePolygon.CutOut(m_1by2rect, m_2by1rect);
            Assert.AreEqual(1f, remainder.Area());
        }

        [Test]
        public void CutOutRectFromRectCollinearTest2()
        {
            var remainder = VertexSimplePolygon.CutOut(m_2by1rect, m_1by2rect);
            Assert.AreEqual(1f, remainder.Area());
        }

        [Test]
        public void CutOutNonIntersectingTest()
        {
            List<Vector2> horizontalRectVertices = new List<Vector2>()
            {
                new Vector2(0, 0), new Vector2(2, 0), new Vector2(2, -1), new Vector2(0,-1)
            };
            List<Vector2> squareVertices = new List<Vector2>()
            {
                new Vector2(10,10), new Vector2(11, 10), new Vector2(11, 9), new Vector2(10,9)
            };

            VertexSimplePolygon horizontalRect = new VertexSimplePolygon(horizontalRectVertices);
            VertexSimplePolygon square = new VertexSimplePolygon(squareVertices);

            var cutout = VertexSimplePolygon.CutOut(square, horizontalRect);
            Assert.AreEqual(1f, cutout.Area());

            cutout = VertexSimplePolygon.CutOut(horizontalRect, square);
            Assert.AreEqual(2f, cutout.Area());
        }


        private bool AllAreTriangles(List<VertexSimplePolygon> a_triangulation)
        {
            foreach(var polygon in a_triangulation)
            {
                if(polygon.VertexCount != 3)
                {
                    return false;
                }
            }
            return true;
        }

        private bool ContainsExpectedTriangle(List<VertexSimplePolygon> a_triangulation, List<Vector2> a_expectedTriangleVertices)
        {
            foreach(var polygon in a_triangulation)
            {
                var contained = true;
                foreach (var vertex in a_expectedTriangleVertices)
                {
                    if(polygon.Vertices.Contains(vertex) == false)
                    {
                        contained = false;
                    }
                }
                if (contained)
                {
                    return true;
                }
            }
            return false;  
        }    
    }
}
