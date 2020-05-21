using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util.Algorithms.Triangulation;
using Util.Geometry.Polygon;
using Util.Geometry.Triangulation;

namespace DotsAndPolygons
{
    public class DotsFace : IDotsFace
    {
        public IDotsHalfEdge OuterComponent { get; set; }

        public IEnumerable<IDotsHalfEdge> OuterComponentHalfEdges
        {
            get
            {
                var visitedHalfEdges = new List<IDotsHalfEdge>();
                IDotsHalfEdge currentHalfEdge = OuterComponent.Next;
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

        public IEnumerable<IDotsVertex> OuterComponentVertices => OuterComponentHalfEdges.Select(edge => edge.Origin);

        public List<IDotsHalfEdge> InnerComponents { get; set; }
        public int Player { get; set; }
        public float Area { get; set; }

        public float AreaMinusInner => Area - this.GetAreaOfAllInnerComponents();

        public void Constructor(IDotsHalfEdge outerComponent, List<IDotsHalfEdge> innerComponents = null,
            List<Vector2> testVertices = null)
        {
            OuterComponent = outerComponent;
            InnerComponents = innerComponents ?? new List<IDotsHalfEdge>();

            List<Vector2> vertices =
                testVertices ?? OuterComponentVertices.Select(vertex => vertex.Coordinates).ToList();
            var polygon = new Polygon2D(vertices);
            Triangulation triangulation = Triangulator.Triangulate(polygon);
            Area = Math.Abs(triangulation.Area);
        }

        public override string ToString() =>
            $"Player: {Player}, Face: {string.Join(", ", OuterComponentVertices.Select(it => it.ToString()))}";
    }
}