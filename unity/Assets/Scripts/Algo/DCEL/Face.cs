using System;
using System.Collections.Generic;
using UnityEngine;
using Algo.Polygons;

namespace Algo.DCEL
{
    public class Face
    {
        private Halfedge m_outerComponent;
        private List<Halfedge> m_innerComponents;

        public Halfedge OuterComponent { get { return m_outerComponent; } set { m_outerComponent = value; } }
        public List<Halfedge> innerComponents { get { return m_innerComponents; } }

        public Face(Halfedge a_outerComponent)
        {
            m_outerComponent = a_outerComponent;
        }

        public Face(Halfedge a_outerComponent, List<Halfedge> a_innerComponents) {
            m_outerComponent = a_outerComponent;
            m_innerComponents = a_innerComponents;
        }

        /// <summary>
        /// A list of the vertices occuring in the outer component of the face
        /// </summary>
        /// <returns></returns>
        public List<Vector2> OuterVertices()
        {
            var result = new List<Vector2>();
            result.Add(m_outerComponent.Fromvertex);

            var workinedge = m_outerComponent.Next;
            while(m_outerComponent!= workinedge)
            {
                result.Add(workinedge.Fromvertex);
                workinedge = workinedge.Next;
            }
            return result;
        }

        /// <summary>
        /// A list of the halfedges occuring in the outer component of the face
        /// </summary>
        /// <returns></returns>
        public List<Halfedge> OuterHalfedges()
        {
            var result = new List<Halfedge>();
            result.Add(m_outerComponent);

            var workinedge = m_outerComponent.Next;
            while (m_outerComponent != workinedge)
            {
                result.Add(workinedge);
                workinedge = workinedge.Next;
            }
            return result;
        }

        /// <summary>
        ///  Returns a the Polygon given by the endpoints of the edges of the face
        /// </summary>
        /// <returns></returns>
        public VertexSimplePolygon Polygon
        {
            get
            {
                // The vertices of a face are mutable a polygon is not 
                return new VertexSimplePolygon(OuterVertices());
            }
        }

        public float Area { get { return Polygon.Area(); } }

        /// <summary>
        /// This assumes the face is convex
        /// </summary>
        /// <param name="a_pos"></param>
        /// <returns></returns>
        public bool Contains(Vector2 a_pos)
        {
            var startedge = OuterComponent;
            var workingedge = startedge;
            do
            {
                if (!workingedge.pointIsRightOf(a_pos))
                {
                    return false;
                }
                workingedge = workingedge.Next;
            } while (workingedge != startedge);
            return true;
        }

        public List<Vector2> GridPoints(float a_xspacing, float a_xmultiple, float a_yspacingBase) {
            //first compute a boundingBox
            var bBox = BoundingBox();

            //Then create gridPointsInFace
            List<float> xcoords = new List<float>();
            xcoords.Add(0);

            var xit = a_xspacing;
            while (xit < bBox.xMax)
            {
                xcoords.Add(xit);
                xit = xit * a_xmultiple;
            }

            xit = -a_xspacing;
            while (xit > bBox.xMin)
            {
                xcoords.Add(xit);
                xit = xit * a_xmultiple;
            }

            var gridpoints = new List<Vector2>();
            for (var i = 0; i < xcoords.Count; i++) {
                for (float yit = 0; yit < bBox.height; yit += (a_yspacingBase * Math.Abs(xcoords[i]) + .05f))
                {
                    //+.2 to prevent trouble when the slope is 0
                    var pos = new Vector2(xcoords[i], bBox.yMin + yit);
                    if (Contains(pos))
                    {
                        gridpoints.Add(pos);
                    }
                }
            }
            return gridpoints;
    }

        public Rect BoundingBox()
        {
            var startedge = OuterComponent;
            var bBox = new Rect(startedge.Fromvertex, Vector2.zero);


            var workingedge = startedge.Next;

            while (workingedge != startedge)
            {
                if (workingedge.Fromvertex.x < bBox.xMin)
                {
                    bBox.xMin = workingedge.Fromvertex.x;
                }
                else if (workingedge.Fromvertex.x > bBox.xMax)
                {
                    bBox.xMax = workingedge.Fromvertex.x;
                }

                if (workingedge.Fromvertex.y < bBox.yMin)
                {
                    bBox.yMin = workingedge.Fromvertex.y;
                }
                else if (workingedge.Fromvertex.y > bBox.yMax)
                {
                    bBox.yMax = workingedge.Fromvertex.y;
                }

                workingedge = workingedge.Next;
            }
            return bBox;
        }
    }
}