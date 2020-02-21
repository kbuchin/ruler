using System.Collections.Generic;
using System.Linq;
using Util.Geometry.Polygon;

namespace Util.Geometry.Contour
{
    public static class ContourExtensions
    {
        /// <summary>
        /// Given a simple polygon, will create a contour polygon from it conforming to the requirements of the
        /// ContourPolygon.
        /// </summary>
        /// <param name="pol"></param>
        /// <returns></returns>
        public static ContourPolygon ToContourPolygon(this Polygon2D pol)
        {
            return new ContourPolygon(new[] {pol.ToContour()});
        }

        private static Contour ToContour(this Polygon2D pol)
        {
            return new Contour((pol.IsClockwise()
                ? pol.Vertices.Reverse()
                : pol.Vertices).Select(v => new Vector2D(v))); // The Polygon2D can be in cw order, while we need it in ccw order
        }

        /// <summary>
        /// Given a non-overlapping multi polygon, will create a contour polygon from it conforming to the requirements
        /// of the ContourPolygon.
        /// </summary>
        /// <param name="pol"></param>
        /// <returns></returns>
        public static ContourPolygon ToContourPolygon(this MultiPolygon2D pol)
        {
            return new ContourPolygon(pol.Polygons.Select(v => v.ToContour()));
        }

        /// <summary>
        /// Given a polygon 2D with holes where none of the holes are overlapping, will create a contour polygon
        /// from it conforming to the requirements of the ContourPolygon.
        /// </summary>
        /// <param name="pol"></param>
        /// <returns></returns>
        public static ContourPolygon ToContourPolygon(this Polygon2DWithHoles pol)
        {
            var outside = new Contour((pol.IsClockwise()
                    ? pol.Vertices.Reverse()
                    : pol.Vertices).Select(v => new Vector2D(v)),
                Enumerable.Range(1,
                    pol.Holes.Count)); // The Polygon2D can be in cw order, while we need it in ccw order
            var result = new ContourPolygon();
            result.Add(outside);
            foreach (var hole in pol.Holes)
            {
                result.Add(new Contour((pol.IsClockwise()
                        ? pol.Vertices
                        : pol.Vertices.Reverse()).Select(v => new Vector2D(v)), Enumerable.Range(1, pol.Holes.Count),
                    false)); // The Polygon2D can be in ccw order, while we need it in cc order for holes;
            }

            return result;
        }
    }
}