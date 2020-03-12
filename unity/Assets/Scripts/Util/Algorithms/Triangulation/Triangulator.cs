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
        /// <param name="m_dcel"></param>
        /// <param name="setTwinPointers"> whether the triangulation should set twin pointers </param>
        /// <returns> A list of clockwise triangles whose disjoint union is this dcel</returns>
        public static Triangulation Triangulate(DCEL m_dcel, bool setTwinPointers = true)
        {
            return Triangulate(m_dcel.InnerFaces, setTwinPointers);
        }

        /// <summary>
        /// Triangulate a collection of dcel faces.
        /// </summary>
        /// <param name="faces"></param>
        /// <param name="setTwinPointers"> whether the triangulation should set twin pointers </param>
        /// <returns></returns>
        public static Triangulation Triangulate(IEnumerable<Face> faces, bool setTwinPointers = true)
        {
            var T = new Triangulation();
            foreach (var face in faces)
            {
                T.AddTriangulation(Triangulate(face, false));
            }

            if (setTwinPointers)
            {
                T.SetTwinPointers();
            }

            return T;
        }

        /// <summary>
        /// Triangulate DCEL face
        /// Cannot yet handle faces inside faces.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="setTwinPointers"> whether the triangulation should set twin pointers </param>
        /// <returns></returns>
        public static Triangulation Triangulate(Face face, bool setTwinPointers = true)
        {
            if (face.InnerComponents.Count > 0)
            {
                throw new GeomException("Cannot triangulate DCEL with faces inside faces");
            }

            return Triangulate(face.Polygon.Outside, setTwinPointers);
        }

        /// <summary>
        /// Triangulates this polygon using the two ears theorem. This is O(n^2).
        /// </summary>
        /// <remarks>
        /// Currently runs in O(n^2) time.
        /// TODO improve this to O(n log n) or O(n log log n).
        /// </remarks>
        /// <param name="polygon"></param>
        /// <param name="setTwinPointers"> whether the triangulation should set twin pointers </param>
        /// <returns>A list of clockwise triangles whose disjoint union is this polygon</returns>
        public static Triangulation Triangulate(Polygon2D polygon, bool setTwinPointers = true)
        {
            // cannot yet triangulate non-simple polygons
            /* assume it to be, checks takes too long
            if (!polygon.IsSimple())
            {
                throw new ArgumentException("Polygon must be simple: " + polygon);
            }
            */

            if (polygon.VertexCount < 3)
            {
                return new Triangulation();
            }

            var T = TriangulateRecursive(polygon.Vertices.ToList());
            if (setTwinPointers)
            {
                T.SetTwinPointers();
            }
            return T;
        }

        /*
         * Triangulation algorithm is faulty
         * 

        /// <summary>
        /// Triangulates this polygon using the two ears theorem. This is O(n^2).
        /// </summary>
        /// <remarks>
        /// Currently runs in O(n^2) time.
        /// TODO improve this to O(n log n) or O(n log log n).
        /// </remarks>
        /// <param name="polygon"></param>
        /// <param name="setTwinPointers"> whether the triangulation should set twin pointers </param>
        /// <returns>A list of clockwise triangles whose disjoint union is this polygon</returns>
        public static Triangulation Triangulate(Polygon2DWithHoles polygon, bool setTwinPointers = true)
        {
            if (polygon.VertexCount < 3)
            {
                return new Triangulation();
            }

            if (polygon.Holes.Count == 0)
            {
                return Triangulate(polygon.Outside, setTwinPointers);
            }

            var T = Delaunay.Create(polygon.Vertices);

            var holeSegments = polygon.Holes.SelectMany(h => h.Segments).ToList();
            var flipSegments = new List<TriangleEdge>();
            foreach (var t in T.Triangles)
            {
                foreach (var seg in holeSegments)
                {
                    if (seg.Intersect(t.E0).HasValue && !(seg.IsEndpoint(t.P0) || seg.IsEndpoint(t.P1)))
                        flipSegments.Add(t.E0);
                    if (seg.Intersect(t.E1).HasValue && !(seg.IsEndpoint(t.P1) || seg.IsEndpoint(t.P2)))
                        flipSegments.Add(t.E1);
                    if (seg.Intersect(t.E2).HasValue && !(seg.IsEndpoint(t.P2) || seg.IsEndpoint(t.P0)))
                        flipSegments.Add(t.E2);
                }
            }

            for (var i=0; i<flipSegments.Count; i++)
            {
                Delaunay.Flip(T, flipSegments[i]);
            }

            // remove triangles inside holes
            T = new Triangulation(T.Triangles.Where(tr => polygon.ContainsInside(tr.Centroid)));

            if (setTwinPointers)
            {
                T.SetTwinPointers();
            }

            return T;
        }
        */

        /// <summary>
        /// Triangulates the polygon recursively.
        /// Finds the leftmost point and creates a triangle with that or splits polygon in two.
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        private static Triangulation TriangulateRecursive(List<Vector2> vertices)
        {
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
                triangulation.AddTriangulation(TriangulateRecursive(vertices));
            }
            else
            {
                var minIndex = Mathf.Min(index, diagonalIndex);
                var maxIndex = Mathf.Max(index, diagonalIndex);

                var poly1List = vertices.Skip(minIndex).Take(maxIndex - minIndex + 1).ToList();
                var poly2List = vertices.Skip(maxIndex).Concat(vertices.Take(minIndex + 1)).ToList();

                triangulation.AddTriangulation(TriangulateRecursive(poly1List));
                triangulation.AddTriangulation(TriangulateRecursive(poly2List));
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
