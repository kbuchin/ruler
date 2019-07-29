namespace Util.Geometry.DCEL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Geometry.Polygon;
    using Util.Geometry.Graph;

    public class Face
    {
        public HalfEdge OuterComponent { get; set; }
        public List<HalfEdge> InnerComponents { get; private set; }

        public bool IsOuter { get; set; }

        public Face(HalfEdge a_outerComponent)
        {
            OuterComponent = a_outerComponent;
            IsOuter = false;
        }

        public Face(HalfEdge a_outerComponent, List<HalfEdge> a_innerComponents)
        {
            OuterComponent = a_outerComponent;
            InnerComponents = a_innerComponents;
            IsOuter = false;
        }

        /// <summary>
        /// A list of the vertices occuring in the outer component of the face
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Vertex> OuterVertices()
        {
            var result = new List<Vertex>();
            result.Add(OuterComponent.From);

            var workinedge = OuterComponent.Next;
            while (OuterComponent != workinedge)
            {
                result.Add(workinedge.From);
                workinedge = workinedge.Next;
            }
            return result;
        }

        /// <summary>
        /// A list of the halfedges occuring in the outer component of the face
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HalfEdge> OuterHalfedges()
        {
            var result = new List<HalfEdge>();
            result.Add(OuterComponent);

            var workinedge = OuterComponent.Next;
            while (OuterComponent != workinedge)
            {
                result.Add(workinedge);
                workinedge = workinedge.Next;
            }
            return result;
        }

        public IEnumerable<Vector2> OuterPoints()
        {
            return OuterVertices()
                .Select(v => v.Pos);
        }

        /// <summary>
        ///  Returns a the Polygon given by the endpoints of the edges of the face
        /// </summary>
        /// <returns></returns>
        public Polygon2D Polygon
        {
            get
            {
                // The vertices of a face are mutable a polygon is not 
                return new Polygon2D(OuterPoints());
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
            return Polygon.Contains(a_pos);
        }

        public List<Vector2> GridPoints(float a_xspacing, float a_xmultiple, float a_yspacingBase)
        {
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
            for (var i = 0; i < xcoords.Count; i++)
            {
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
            var bBox = new Rect(OuterComponent.From.Pos, Vector2.zero);

            var workingedge = OuterComponent.Next;

            while (workingedge != OuterComponent)
            {
                bBox.xMin = Math.Min(bBox.xMin, workingedge.From.Pos.x);
                bBox.xMax = Math.Max(bBox.xMax, workingedge.From.Pos.x);
                bBox.yMin = Math.Min(bBox.yMin, workingedge.From.Pos.y);
                bBox.yMax = Math.Max(bBox.yMax, workingedge.From.Pos.y);

                workingedge = workingedge.Next;
            }
            return bBox;
        }

        public override string ToString()
        {
            return Polygon.ToString();
        }
    }
}