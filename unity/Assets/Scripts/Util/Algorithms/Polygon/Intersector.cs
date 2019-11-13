namespace Util.Algorithms.Polygon
{
    using System.Linq;
    using UnityEngine;
    using Util.Geometry;
    using Util.Geometry.Polygon;

    public static class Intersector
    {
        /// <summary>
        /// Dirty method O(n^2)
        /// </summary>
        /// <param name="a_poly1"></param>
        /// <param name="a_poly2"></param>
        /// <returns></returns>
        public static Polygon2D IntersectConvex(Polygon2D a_poly1, Polygon2D a_poly2)
        {
            if (!(a_poly1.IsConvex()))
            {
                throw new GeomException("Method not defined for nonconvex polygons" + a_poly1);
            }
            if (!(a_poly2.IsConvex()))
            {
                throw new GeomException("Method not defined for nonconvex polygons" + a_poly2);
            }

            // obtain vertices that lie inside both polygons
            var resultVertices = a_poly1.Vertices
                .Where(v => a_poly2.ContainsInside(v))
                .Concat(a_poly2.Vertices.Where(v => a_poly1.ContainsInside(v)))
                .ToList();

            // add intersections between two polygon segments
            resultVertices.AddRange(a_poly1.Segments.SelectMany(seg => seg.Intersect(a_poly2.Segments)));

            // remove any duplicates
            resultVertices = resultVertices.Distinct().ToList();

            // retrieve convex hull of relevant vertices
            if (resultVertices.Count() >= 3)
            {
                var poly = ConvexHull.ComputeConvexHull(resultVertices);
                Debug.Assert(poly.IsConvex());
                return poly;
            }

            return null;
        }
    }
}
