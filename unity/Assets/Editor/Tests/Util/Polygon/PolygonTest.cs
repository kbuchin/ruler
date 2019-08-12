namespace Util.Geometry.Polygons.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry.Polygons;

    [TestFixture]
    class PolygonTest
    {
        /*
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
        private static List<Vector2> m_largeSquareVertices = new List<Vector2>()
        {
            new Vector2(-2, -2),
            new Vector2(2,-2),
            new Vector2(2,2),
            new Vector2(-2, 2)
        };

        private VertexHolePolygon m_arrow = new VertexHolePolygon( new VertexSimplePolygon(m_arrowVertices));
        private VertexHolePolygon m_diamond = new VertexHolePolygon ( new VertexSimplePolygon(m_diamondVertices));

        //4x4 square with 1x1 diamond hole
        private VertexHolePolygon m_squareWithHole = new VertexHolePolygon(new VertexSimplePolygon(m_largeSquareVertices), new List<VertexSimplePolygon>() {new VertexSimplePolygon(m_diamondVertices)}  );

        [Test]
        public void VertexMeanTest()
        {
            var poly = new VertexSimplePolygon(new Vector2[] {m_topVertex, m_leftVertex, m_botVertex, m_rightVertex});
            Assert.AreEqual(new Vector2(0, 0), poly.VertexMean); 
        }

        [Test]
        public void DiamondVisionTest()
        {
            var visionarea = m_diamond.VisibleArea(Vector2.zero);
            Assert.GreaterOrEqual(visionarea, 1.999f);
        }

        [Test]
        public void DiamondVisionVerticalOffCenterTest()
        {
            var visibleArea = m_diamond.VisibleArea(new Vector2(0f, .5f));
            Assert.GreaterOrEqual(visibleArea, 1.999f);
        }

        [Test]
        public void DiamondVisionHorizontalOffCenterTest()
        {
            var visibleArea = m_diamond.VisibleArea(new Vector2(.5f, 0f));
            Assert.GreaterOrEqual(visibleArea, 1.999f);
        }

        [Test]
        public void DiamondVisionDiagonalOffCenterTest()
        {
            var visibleArea = m_diamond.VisibleArea(.4f * Vector2.one);
            Assert.GreaterOrEqual(visibleArea, 1.999f);
        }

        [Test] 
        public void ArrowVisionTest()
        {
            var visibleArea = m_arrow.VisibleArea(new Vector2(1.5f, 0));
            Assert.GreaterOrEqual(visibleArea, .999f);
        }

        [Test]
        public void SquareWithHoleVisionTest()
        {
            var visionPolygon = m_squareWithHole.VisibleArea(new Vector2(-1f, -1f));
            Assert.GreaterOrEqual(visionPolygon, 7.499f);
            Assert.GreaterOrEqual(7.501f, visionPolygon);

        }

        [Test]
        public void LShapeVisionTest()
        {
            var LShape = new VertexHolePolygon(new VertexSimplePolygon(new List<Vector2>()
            {
                new Vector2(0,0), new Vector2(0,4), new Vector2(4,4),
                new Vector2(4,2), new Vector2(2,2), new Vector2(2,0)
            }));

            var area = LShape.VisibleArea(new Vector2(3.427403f, 3.464213f));

            //Debug.Log(area / 12f);
            Assert.Greater(0.88f, area/12f);
        }

        [Test]
        public void LShapeTwoTowerVisionTest()
        {
            var LShape = new VertexHolePolygon(new VertexSimplePolygon(new List<Vector2>()
            {
                new Vector2(0,0), new Vector2(0,4), new Vector2(4,4),
                new Vector2(4,2), new Vector2(2,2), new Vector2(2,0)
            }));

            var area = LShape.Vision(new Vector2(3.499f, 3.5f));
            var area2 = LShape.Vision(new Vector2(.4342f, .43f));

            var result = new VertexMultiPolygon(area);
            result.CutOut(area2);
            result.Add(area2);

            Assert.AreEqual(12f, result.Area());
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

        [Test]
        public void PolygonTriangulation()
        {
            VertexSimplePolygon p = new VertexSimplePolygon(new List<Vector2>() { 
                    new Vector2(1, 1),
                    new Vector2(1, 2),
                    new Vector2(2,2),
                    new Vector2(2,-1),
                    new Vector2(-1, -1),
                    new Vector2(-1,-2),
                    new Vector2(-2, -2),
                    new Vector2(-2, -1)
                });
            var test = p.Triangulation;

            Assert.NotNull(test);
        } 
        */
    }
}
