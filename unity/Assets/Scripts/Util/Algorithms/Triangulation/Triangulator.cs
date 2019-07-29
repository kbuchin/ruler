namespace Util.Algorithms.Triangulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Geometry;
    using Util.Geometry.Graph;
    using Util.Geometry.Polygon;
    using Util.Geometry.Triangulation;
    using Util.Math;

    public static class Triangulator
    {
        /// <summary>
        /// Triangulates this polygon using the two ears theorem. This is O(n^2).
        /// </summary>
        /// NB: Also see poygontriangulation.pdf in the docs folder
        /// <returns>A list of clockwise triangles whose disjoint union is this polygon</returns>
        public static Triangulation Triangulate(Polygon2D polygon)
        {
            var vertices = polygon.Vertices.ToList();

            if (vertices.Count < 3)
                return new Triangulation();

            //PERF we can do this faster as a sweepline algorithm
            if (vertices.Count == 3)
            {
                if (polygon.IsConvex())
                {
                    return new Triangulation(vertices);
                }
                else
                {
                    // make clockwise
                    vertices.Reverse();
                    return new Triangulation(vertices);
                }
            }

            var triangulation = new Triangulation();

            //Find leftmost vertex
            var leftVertex = (Vector2)polygon.LeftMostVertex;
            var index = vertices.IndexOf(leftVertex);
            var prevVertex = vertices[MathUtil.PositiveMod(index - 1, vertices.Count)];
            var nextVertex = vertices[MathUtil.PositiveMod(index - 1, vertices.Count)];

            //Create triangle with diagonal
            var triangle = new Triangle(prevVertex, leftVertex, nextVertex);

            //check for other vertices inside the candidate triangle
            var baseline = new Line(prevVertex, nextVertex);
            float distance = 0;
            int diagonalIndex = -1;

            for (int i = 0; i < vertices.Count; i++)
            {
                var v = vertices[i];

                if (triangle.Inside(v))
                {
                    if (baseline.DistanceToPoint(v) > distance)
                    {
                        distance = baseline.DistanceToPoint(v);
                        diagonalIndex = i;
                    }
                }
            }


            //Do Recursive call
            if (diagonalIndex == -1) //didn't change
            {
                if (triangle.IsClockwise())
                {
                    // reverse triangle
                    triangle = new Triangle(triangle.P0, triangle.P2, triangle.P1);
                    triangulation.Add(triangle);
                }
                else
                {
                    triangulation.Add(triangle);
                }
                vertices.Remove(leftVertex);
                vertices.Remove(prevVertex);
                vertices.Remove(nextVertex);

                triangulation.Add(Triangulate(new Polygon2D(vertices)));
            }
            else
            {
                IEnumerable<Vector2> poly1List, poly2List;
                if (diagonalIndex < index)
                {
                    poly1List = polygon.Vertices.Skip(diagonalIndex).Take(index - diagonalIndex + 1);
                    poly2List = polygon.Vertices.Take(diagonalIndex + 1).Union(vertices.Skip(index));
                }
                else
                {
                    poly1List = vertices.Skip(index).Take(diagonalIndex - index + 1);
                    poly2List = vertices.Take(index + 1).Union(vertices.Skip(diagonalIndex));
                }
                triangulation.Add(Triangulate(new Polygon2D(poly1List)));
                triangulation.Add(Triangulate(new Polygon2D(poly2List)));
            }

            return triangulation;
        }
    }
}
