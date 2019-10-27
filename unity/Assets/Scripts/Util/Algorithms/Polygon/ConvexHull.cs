namespace Util.Algorithms.Polygon
{
    using Util.Geometry;
    using Util.Geometry.Polygon;
    using Util.Math;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

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
            var upperhull = ComputeUpperHull(polygon);
            var lowerhull = ComputeLowerHull(polygon);

            //STITCH AND RETURN
            lowerhull = lowerhull.Reverse();

            return new Polygon2D(upperhull.Concat(lowerhull.ToList().GetRange(1, lowerhull.Count() - 2)));
        }

        /// <summary>
        /// Computes the upper hull of the given polygon
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static IEnumerable<Vector2> ComputeUpperHull(IPolygon2D polygon)
        {
            return ComputeHull(polygon, 1);
        }

        /// <summary>
        /// Computes the lower hull of the given polygon
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static IEnumerable<Vector2> ComputeLowerHull(IPolygon2D polygon)
        {
            return ComputeHull(polygon, -1);
        }

        /// <summary>
        /// Computes either the upper or lower hull of the polygon, depending on the sign of dir.
        /// Looks at the turns of three points.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        private static IEnumerable<Vector2> ComputeHull(IPolygon2D polygon, int dir)
        {
            if (polygon.VertexCount <= 2)
            {
                throw new GeomException("Too little points provided");
            }

            //Sort vertices on x-coordinate
            var sortedVertices = polygon.Vertices.ToList().OrderBy(v => v.x).ToList();

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
                upperhull.Add(point);
                var n = upperhull.Count;
                while (n > 2 && dir * MathUtil.Orient2D(upperhull[n - 3], upperhull[n - 2], upperhull[n - 1]) > 0)
                {
                    upperhull.RemoveAt(n - 2);
                    n = upperhull.Count;
                }
            }

            return upperhull;
        }
    }
}
