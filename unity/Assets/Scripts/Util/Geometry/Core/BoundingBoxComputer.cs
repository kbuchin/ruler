namespace Util.Geometry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Math;

    /// <summary>
    /// Class for computing the bounding box (Rect) around various structures.
    /// </summary>
    public static class BoundingBoxComputer
    {
        /// <summary>
        /// Compute bounding box from collection of segments.
        /// Extend with given margin.
        /// </summary>
        /// <param name="a_segments"></param>
        /// <param name="margin"></param>
        /// <returns></returns>
        public static Rect FromSegments(IEnumerable<LineSegment> a_segments, float margin = 0f)
        {
            var vertices = new List<Vector2>();
            foreach (var seg in a_segments)
            {
                vertices.Add(seg.Point1);
                vertices.Add(seg.Point2);
            }
            return FromPoints(vertices, margin);
        }

        /// <summary>
        /// Computes bounding box around the line intersections.
        /// Extend with given margin.
        /// </summary>
        /// <param name="a_lines"></param>
        /// <returns> A bounding box</returns>
        public static Rect FromLines(IEnumerable<Line> a_lines, float margin = 0f)
        {
            var lines = a_lines.ToList();

            if (lines.Count < 2)
            {
                Debug.Log("Bounding box not defined for less than 2 lines");
                return new Rect();
            }

            lines.Sort(); //Sorts on slope by implementation of compareTo in line 

            //Outermost (in both x and y) intersections are between lines of adjecent slope

            var candidatepoints = new List<Vector2>();
            for (var i = 0; i < lines.Count; i++)
            {
                var intersect = Line.Intersect(lines[i], lines[(i + 1) % lines.Count]);
                if (intersect.HasValue)
                {
                    candidatepoints.Add(intersect.Value);
                }
            }

            return FromPoints(candidatepoints, margin);
        }

        /// <summary>
        /// Compute bounding box from collection of points.
        /// Extend with given margin.
        /// </summary>
        /// <param name="a_points"></param>
        /// <param name="margin"></param>
        /// <returns></returns>
        public static Rect FromPoints(IEnumerable<Vector2> a_points, float margin = 0f)
        {
            if (a_points.Count() == 0)
            {
                Debug.Log("Bounding box not defined on empty vector list");
                return new Rect();
            }

            var result = new Rect(a_points.FirstOrDefault(), Vector2.zero);
            foreach (var candidatepoint in a_points)
            {
                result.xMin = Math.Min(result.xMin, candidatepoint.x - margin);
                result.xMax = Math.Max(result.xMax, candidatepoint.x + margin);
                result.yMin = Math.Min(result.yMin, candidatepoint.y - margin);
                result.yMax = Math.Max(result.yMax, candidatepoint.y + margin);
            }

            if (!MathUtil.IsFinite(result.xMin) ||
                !MathUtil.IsFinite(result.xMax) ||
                !MathUtil.IsFinite(result.yMax) ||
                !MathUtil.IsFinite(result.yMin))
            {
                throw new GeomException("Bounding box has nonfinite values");
            }

            return result;
        }
    }
}
