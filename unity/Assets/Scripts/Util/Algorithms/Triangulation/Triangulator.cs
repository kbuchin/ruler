namespace Util.Algorithms.Triangulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Geometry;
    using Util.Geometry.DCEL;
    using Util.Geometry.Polygon;
    using Util.Geometry.Triangulation;
    using Util.Math;

    /// <summary>
    /// Collection of algorithms related to the creation of Triangulations for various concepts.
    /// </summary>
    public static class Triangulator
    {
        /// <summary>
        /// Triangulates this dcel by triangulating each inner face.
        /// </summary>
        /// <returns> A list of clockwise triangles whose disjoint union is this dcel</returns>
        public static Triangulation Triangulate(DCEL m_dcel)
        {
            return Triangulate(m_dcel.InnerFaces);
        }

        /// <summary>
        /// Triangulate a collection of dcel faces.
        /// </summary>
        /// <param name="faces"></param>
        /// <returns></returns>
        public static Triangulation Triangulate(IEnumerable<Face> faces)
        {
            var T = new Triangulation();
            foreach (var face in faces)
            {
                T.AddTriangulation(Triangulate(face));
            }
            return T;
        }

        /// <summary>
        /// Triangulate DCEL face
        /// Cannot yet handle faces inside faces.
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public static Triangulation Triangulate(Face face)
        {
            if (face.InnerComponents.Count > 0)
            {
                throw new GeomException("Cannot triangulate DCEL with faces inside faces");
            }

            return Triangulate(face.Polygon.Outside);
        }

        /// <summary>
        /// Triangulates this polygon using the two ears theorem. This is O(n^2).
        /// </summary>
        /// NB: Also see poygontriangulation.pdf in the docs folder
        /// <returns>A list of clockwise triangles whose disjoint union is this polygon</returns>
        public static Triangulation Triangulate(IPolygon2D polygon)
        {
            // cannot yet triangulate non-simple polygons
            if (!polygon.IsSimple())
            {
                throw new ArgumentException("Polygon must be simple: " + polygon);
            }

            if (polygon.VertexCount < 3)
            {
                return new Triangulation();
            }

            return TriangulateRecursive(polygon);
        }

        private static Triangulation TriangulateRecursive(IPolygon2D polygon)
        {
            if (polygon.VertexCount == 3)
            {
                return new Triangulation(polygon.Vertices);
            }

            var triangulation = new Triangulation();

            var vertices = polygon.Vertices.ToList();

            //Find leftmost vertex
            var leftVertex = LeftMost(vertices);
            var index = vertices.IndexOf(leftVertex);
            var prevVertex = vertices[MathUtil.PositiveMod(index - 1, vertices.Count)];
            var nextVertex = vertices[MathUtil.PositiveMod(index + 1, vertices.Count)];

            //Create triangle with diagonal
            Debug.Assert(leftVertex != prevVertex && leftVertex != nextVertex && prevVertex != nextVertex);

            var triangle = new Triangle(prevVertex, leftVertex, nextVertex);

            //check for other vertices inside the candidate triangle
            var baseline = new Line(prevVertex, nextVertex);
            float distance = -1f;
            int diagonalIndex = -1;

            for (int i = 0; i < vertices.Count; i++)
            {
                var v = vertices[i];
                if (v != leftVertex && v != prevVertex && v != nextVertex &&
                    triangle.Contains(v))
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

                triangulation.AddTriangle(triangle);
                vertices.Remove(leftVertex);
                triangulation.AddTriangulation(TriangulateRecursive(new Polygon2D(vertices)));
            }
            else
            {
                var minIndex = Mathf.Min(index, diagonalIndex);
                var maxIndex = Mathf.Max(index, diagonalIndex);

                var poly1List = vertices.Skip(minIndex).Take(maxIndex - minIndex + 1);
                var poly2List = vertices.Skip(maxIndex).Concat(vertices.Take(minIndex + 1));

                triangulation.AddTriangulation(TriangulateRecursive(new Polygon2D(poly1List)));
                triangulation.AddTriangulation(TriangulateRecursive(new Polygon2D(poly2List)));
            }

            return triangulation;
        }

        /// <summary>
        /// Finds the left most vertex.
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        private static Vector2 LeftMost(IEnumerable<Vector2> vertices)
        {
            return vertices.Aggregate(
                (minItem, nextItem) => minItem.x < nextItem.x ? minItem : nextItem
                );
        }
    }
}
