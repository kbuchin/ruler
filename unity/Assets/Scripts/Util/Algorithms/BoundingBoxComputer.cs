namespace Util.Algorithms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Geometry;
    using Util.Geometry.Polygon;
    using Util.Math;

    public static class BoundingBoxComputer
    {
        public static Rect FromPolygon(IPolygon2D polygon)
        {
            return FromPolygon(polygon, 0f);
        }

        public static Rect FromPolygon(IPolygon2D polygon, float margin)
        {
            return FromVector2(polygon.Vertices, margin);
        }

        public static Rect FromSegments(IEnumerable<LineSegment> a_segments)
        {
            return FromSegments(a_segments, 0f);
        }

        public static Rect FromSegments(IEnumerable<LineSegment> a_segments, float margin)
        {
            var vertices = new List<Vector2>();
            foreach (var seg in a_segments)
            {
                vertices.Add(seg.Point1);
                vertices.Add(seg.Point2);
            }
            return FromVector2(vertices, margin);
        }

        public static Rect FromLines(IEnumerable<Line> a_lines)
        {
            return FromLines(a_lines, 0f);
        }

        /// <summary>
        /// Computes bounding box around the line intersections.
        /// </summary>
        /// <param name="a_lines"></param>
        /// <returns> A bounding box</returns>
        public static Rect FromLines(IEnumerable<Line> a_lines, float margin)
        {
            var lines = a_lines.ToList();

            if (lines.Count < 2)
            {
                throw new ArgumentException("Not enough lines provided");
            }

            lines.Sort(); //Sorts on slope by implementation of compareTo in line 

            //Outermost (in both x and y) intersections are between lines of adjecent slope
            
            var candidatepoints = new List<Vector2>();
            for (var i = 0; i < lines.Count - 1; i++)
            {
                candidatepoints.Add(Line.Intersect(lines[i], lines[i + 1]));
            }

            return FromVector2(candidatepoints, margin);
        }

        public static Rect FromVector2(IEnumerable<Vector2> a_points)
        {
            return FromVector2(a_points, 0.1f);
        }

        public static Rect FromVector2(IEnumerable<Vector2> a_points, float margin)
        {
            if(a_points.Count() == 0)
            {
                throw new ArgumentException("Bounding box not defined on empty vector list");
            }

            var result = new Rect(a_points.First(), Vector2.zero);
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
