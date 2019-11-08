namespace Util.Algorithms.Polygon
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Geometry;
    using Util.Geometry.Polygon;
    using Util.Math;

    /// <summary>
    /// Collection of algorithms related to convex hulls.
    /// </summary>
    public static class ConvexHull
    {
        /// <summary>
        /// Does a simple graham scan.
        /// </summary>
        /// <param name="a_points"></param>
        public static Polygon2D ComputeConvexHull(IPolygon2D polygon)
        {
            return ComputeConvexHull(polygon.Vertices);
        }

        /// <summary>
        /// Performs a simple graham scan of the given vertices
        /// </summary>
        /// <param name="a_vertices"></param>
        /// <returns></returns>
        public static Polygon2D ComputeConvexHull(IEnumerable<Vector2> a_vertices)
        {
            var vertices = a_vertices.Distinct();

            var upperhull = ComputeUpperHull(vertices);
            var lowerhull = ComputeLowerHull(vertices);

            //STITCH AND RETURN
            lowerhull.Reverse();

            var convexhull = new Polygon2D(upperhull.Concat(lowerhull.GetRange(1, lowerhull.Count - 2)));

            Debug.Assert(convexhull.IsConvex(), convexhull);
            Debug.Assert(!MathUtil.EqualsEps(convexhull.Vertices.First(), convexhull.Vertices.Last()),
                convexhull);


            return convexhull;
        }

        /// <summary>
        /// Computes the upper hull of the given polygon.
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static List<Vector2> ComputeUpperHull(IPolygon2D polygon)
        {
            return ComputeHull(polygon.Vertices, 1);
        }

        /// <summary>
        /// Computes the upper hull of the given vertices.
        /// </summary>
        /// <param name="a_vertices"></param>
        /// <returns></returns>
        public static List<Vector2> ComputeUpperHull(IEnumerable<Vector2> a_vertices)
        {
            return ComputeHull(a_vertices, 1);
        }

        /// <summary>
        /// Computes the lower hull of the given polygon
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static List<Vector2> ComputeLowerHull(IPolygon2D polygon)
        {
            return ComputeHull(polygon.Vertices, -1);
        }

        /// <summary>
        /// Computes the lower hull of the given vertices.
        /// </summary>
        /// <param name="a_vertices"></param>
        /// <returns></returns>
        public static List<Vector2> ComputeLowerHull(IEnumerable<Vector2> a_vertices)
        {
            return ComputeHull(a_vertices, -1);
        }

        /// <summary>
        /// Computes either the upper or lower hull of the polygon, depending on the sign of dir.
        /// Looks at the turns of three points.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        private static List<Vector2> ComputeHull(IEnumerable<Vector2> a_vertices, int dir)
        {
            if (a_vertices.Count() <= 2)
            {
                throw new GeomException("Too little points provided");
            }

            //Sort vertices on x-coordinate
            var sortedVertices = a_vertices.ToList().OrderBy(v => v.x).ToList();

            //UPPER HULL
            //add first two points
            var upperhull = new List<Vector2>
            {
                sortedVertices[0],
                sortedVertices[1]
            };

            //add point and check for removal
            foreach (var point in sortedVertices.Skip(2))
            {
                // skip duplicate vertices
                if (MathUtil.EqualsEps(point, upperhull.Last())) continue;

                upperhull.Add(point);
                var n = upperhull.Count;
                while (n > 2 && (dir * MathUtil.Orient2D(upperhull[n - 3], upperhull[n - 2], upperhull[n - 1]) >= 0))
                {
                    upperhull.RemoveAt(n - 2);
                    n = upperhull.Count;
                }
            }

            return upperhull;
        }
    }
}
