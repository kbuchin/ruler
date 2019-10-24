namespace Util.Geometry.Duality
{
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;

    //class generating projective duals
    public static class PointLineDual
    {
        public static Line Dual(this Vector2 a_point)
        {
            return new Line(a_point.x, -a_point.y);
        }

        public static Vector2 Dual(this Line a_line)
        {
            return new Vector2(a_line.Slope, -a_line.HeightAtYAxis);
        }

        public static IEnumerable<Line> Dual(IList<Vector2> a_list)
        {
            return a_list.Select(p => Dual(p));
        }

        public static IEnumerable<Vector2> Dual(IList<Line> a_list)
        {
            return a_list.Select(l => Dual(l));
        }
    }
}
