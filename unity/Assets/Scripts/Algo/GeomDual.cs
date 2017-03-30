using UnityEngine;
using System.Collections.Generic;

namespace Algo
{
    //class generating projective duals
    static class GeomDual
    {
        public static Line Dual(this Vector2 a_point)
        {
            return new Line(a_point.x, -a_point.y);
        }

        public static Vector2 Dual(this Line a_line)
        {
            return new Vector2(a_line.Slope, -a_line.HeightAtYAxis);
        }

        public static List<Line> Dual(IList<Vector2> a_list)
        {
            var result = new List<Line>();
            foreach (Vector2 point in a_list)
                {
                result.Add(Dual(point));
                }
            return result;
        }

        static List<Vector2> Dual(IList<Line> a_list)
        {
            var result = new List<Vector2>();
            foreach (Line line in a_list)
            {
                result.Add(Dual(line));
            }
            return result;
        }
    }
}
