namespace Util.Geometry.Duality
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Class generating projective duals from point to line and vice versa.
    /// </summary>
    public static class PointLineDual
    {
        /// <summary>
        /// Calculate line dual from point.
        /// </summary>
        /// <param name="a_point"></param>
        /// <returns></returns>
        public static Line Dual(this Vector2 a_point)
        {
            return new Line(a_point.x, -a_point.y);
        }

        /// <summary>
        /// Calculate point dual from line.
        /// </summary>
        /// <param name="a_line"></param>
        /// <returns></returns>
        public static Vector2 Dual(this Line a_line)
        {
            return new Vector2(a_line.Slope, -a_line.HeightAtYAxis);
        }

        /// <summary>
        /// Generates line duals for each point in collection.
        /// </summary>
        /// <param name="a_list"></param>
        /// <returns></returns>
        public static IEnumerable<Line> Dual(IEnumerable<Vector2> a_list)
        {
            return a_list.Select(p => Dual(p));
        }

        /// <summary>
        /// Generates point duals for each line in collection.
        /// </summary>
        /// <param name="a_list"></param>
        /// <returns></returns>
        public static IEnumerable<Vector2> Dual(IEnumerable<Line> a_list)
        {
            return a_list.Select(l => Dual(l));
        }
    }
}
