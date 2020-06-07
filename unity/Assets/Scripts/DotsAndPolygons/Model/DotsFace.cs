using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util.Algorithms.Triangulation;
using Util.Geometry.Polygon;
using Util.Geometry.Triangulation;

namespace DotsAndPolygons
{
    [Serializable]
    public class DotsFace
    {
        public DotsHalfEdge OuterComponent { get; set; }

        public IEnumerable<DotsHalfEdge> OuterComponentHalfEdges
        {
            get
            {
                var visitedHalfEdges = new List<DotsHalfEdge>();
                DotsHalfEdge currentHalfEdge = OuterComponent.Next;
                int count = 0;
                while (count < 10000)
                {
                    count++;
                    if (currentHalfEdge == OuterComponent)
                    {
                        visitedHalfEdges.Add(currentHalfEdge);
                        return visitedHalfEdges;
                    }

                    if (currentHalfEdge.Next != null)
                    {
                        visitedHalfEdges.Add(currentHalfEdge);
                        currentHalfEdge = currentHalfEdge.Next;
                    }
                    else return null;
                }
                return null;
            }
        }

        public DotsFace(DotsHalfEdge outerComponent, List<DotsHalfEdge> innerComponents = null)
        {
            this.OuterComponent = outerComponent;
            this.InnerComponents = innerComponents;
        }

        public DotsFace()
        {
        }

        public IEnumerable<DotsVertex> OuterComponentVertices => OuterComponentHalfEdges.Select(edge => edge.Origin);

        public List<DotsHalfEdge> InnerComponents { get; set; }
        public int Player { get; set; }
        public float Area { get; set; }

        public float AreaMinusInner => Area - this.GetAreaOfAllInnerComponents();

        public void Constructor(DotsHalfEdge outerComponent, List<DotsHalfEdge> innerComponents = null,
            List<Vector2> testVertices = null)
        {
            OuterComponent = outerComponent;
            InnerComponents = innerComponents ?? new List<DotsHalfEdge>();

            List<Vector2> vertices =
                testVertices ?? OuterComponentVertices.Select(vertex => vertex.Coordinates).ToList();
            var polygon = new Polygon2D(vertices);
            Triangulation triangulation = Triangulator.Triangulate(polygon);
            Area = Math.Abs(triangulation.Area);
        }

        public override string ToString() =>
            $"Player: {Player}, Face: {string.Join(", ", OuterComponentVertices.Select(it => it.ToString()))}";

        public DotsFace Clone() => new DotsFace(this.OuterComponent, this.InnerComponents);
    }
}