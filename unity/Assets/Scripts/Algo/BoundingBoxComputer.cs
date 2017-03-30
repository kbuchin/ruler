using System.Collections.Generic;
using UnityEngine;


namespace Algo
{
    class BoundingBoxComputer
    {
        public static Rect FromLines(List<Line> a_lines, int a_xmargin, int a_ymargin )
        {
            if (a_lines.Count < 2)
            {
                throw new System.Exception("Not enough lines provided");
            }
            a_lines.Sort(); //Sorts on slope by implementation of compareTo in line 



            //Outermost (in both x and y) intersections are between lines of adjecent slope

            var candidatepoints = new List<Vector2>();
            for (var i = 0; i < a_lines.Count - 1; i++)
            {
                candidatepoints.Add(Line.Intersect(a_lines[i], a_lines[i + 1]));


            }

            var result = FromVector2(candidatepoints);
                       

            //relax bounding box a little (such that no intersections are actually on the boundingBox)
            result.xMin = result.xMin - a_xmargin; //More extra x such that we get all vertical lines
            result.xMax = result.xMax + a_xmargin; //More extra x such that we get all vertical lines
            result.yMin = result.yMin - a_ymargin; //More extra y such that we get all vertical lines
            result.yMax = result.yMax + a_ymargin; //More extra y such that we get all vertical lines
            return result;
        }

        public static Rect FromVector2(List<Vector2> a_points)
        {
            var result = new Rect(a_points[0], Vector2.zero);
            foreach (var candidatepoint in a_points)
            {
                if (candidatepoint.x < result.xMin)
                {
                    result.xMin = candidatepoint.x;
                }
                else if (candidatepoint.x > result.xMax)
                {
                    result.xMax = candidatepoint.x;
                }

                if (candidatepoint.y < result.yMin)
                {
                    result.yMin = candidatepoint.y;
                }
                else if (candidatepoint.y > result.yMax)
                {
                    result.yMax = candidatepoint.y;
                }
            }

            if (!(result.xMin.isFinite() && result.xMax.isFinite() && result.yMax.isFinite() && result.yMin.isFinite()))
            {
                throw new AlgoException("Bounding box has nonfinite values");
            }

            return result;
        }
    }
}
