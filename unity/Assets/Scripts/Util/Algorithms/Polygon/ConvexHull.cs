namespace Util.Algorithms.Polygon
{
    using Util.Geometry;
    using Util.Geometry.Polygon;
    using Util.Math;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public static class ConvexHull
    {
        /// <summary>
        /// Does a simple graham scan
        /// </summary>
        /// <param name="a_points"></param>
        public static IPolygon2D ComputeConvexHull(IPolygon2D polygon)
        {
            if (polygon.Vertices.Count <= 2)
            {
                throw new GeomException("Too little points provided");
            }

            var upperhull = ComputeUpperHull(polygon);
            var lowerhull = ComputeLowerHull(polygon);

            //STITCH AND RETURN
            lowerhull.Reverse();
            return new Polygon2D(upperhull.Concat(lowerhull.ToList().GetRange(1, lowerhull.Count - 2)));
        }

        public static ICollection<Vector2> ComputeUpperHull(IPolygon2D polygon)
        {
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
            foreach (Vector2 point in sortedVertices.Skip(2))
            {
                upperhull.Add(point);
                var n = upperhull.Count;
                while (n > 2 && MathUtil.Orient2D(upperhull[n - 3], upperhull[n - 2], upperhull[n - 1]) > 0)
                {
                    upperhull.RemoveAt(n - 2);
                    n = upperhull.Count;
                }
            }

            return upperhull;
        }

        public static ICollection<Vector2> ComputeLowerHull(IPolygon2D polygon)
        {
            //Sort vertices on x-coordinate
            var sortedVertices = polygon.Vertices.ToList().OrderBy(v => v.x).ToList();

            //LOWER HULL
            //add first two points
            var lowerhull = new List<Vector2>
            {
                sortedVertices[0],
                sortedVertices[1]
            };

            //add point and check for removal
            foreach (Vector2 point in sortedVertices.Skip(2))
            {
                lowerhull.Add(point);
                var n = lowerhull.Count;
                while (n > 2 && MathUtil.Orient2D(lowerhull[n - 3], lowerhull[n - 2], lowerhull[n - 1]) < 0)
                {
                    lowerhull.RemoveAt(n - 2);
                    n = lowerhull.Count;

                }
            }

            return lowerhull;
        }
    }
}
