namespace Util.Algorithms.Triangulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Geometry;
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
        public static Triangulation Triangulate(IPolygon2D polygon)
        {
            if(!polygon.IsSimple())
            {
                throw new ArgumentException("Polygon must be simple");
            }

            var vertices = polygon.Vertices.ToList();

            if (vertices.Count < 3)
                return new Triangulation();

            if (!polygon.IsClockwise())
            {
                vertices.Reverse();
            }

            if (vertices.Count == 3)
            {
                return new Triangulation(vertices);
            }

            var triangulation = new Triangulation();

            //Find leftmost vertex
            var leftVertex = LeftMost(vertices);
            var index = vertices.IndexOf(leftVertex);
            var prevVertex = vertices[MathUtil.PositiveMod(index - 1, vertices.Count)];
            var nextVertex = vertices[MathUtil.PositiveMod(index + 1, vertices.Count)];

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
                if (!triangle.IsClockwise())
                {
                    // shouldnt happen but in case it does
                    // reverse triangle
                    triangle = new Triangle(triangle.P0, triangle.P2, triangle.P1);
                }

                triangulation.Add(triangle);
                vertices.Remove(leftVertex);
                triangulation.Add(Triangulate(new Polygon2D(vertices)));
            }
            else
            {
                var minIndex = Mathf.Min(index, diagonalIndex);
                var maxIndex = Mathf.Max(index, diagonalIndex);

                var poly1List = polygon.Vertices.Skip(minIndex).Take(maxIndex - minIndex + 1);
                var poly2List = polygon.Vertices.Take(minIndex + 1).Union(vertices.Skip(maxIndex));

                triangulation.Add(Triangulate(new Polygon2D(poly1List)));
                triangulation.Add(Triangulate(new Polygon2D(poly2List)));
            }

            return triangulation;
        }

        private static Vector2 LeftMost(IEnumerable<Vector2> vertices)
        {
            return vertices.Aggregate(
                (minItem, nextItem) => minItem.x < nextItem.x ? minItem : nextItem
                );
        }
    }
}
