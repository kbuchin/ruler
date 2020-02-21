namespace Util.Geometry
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Math;

    /// <summary>
    /// Class representing a line segment between two given points.
    /// Has auxiliary methods for intersection, orientation and distance among others.
    /// </summary>
    public class LineSegment : IEquatable<LineSegment>
    {
        public Vector2 Point1 { get; private set; }
        public Vector2 Point2 { get; private set; }

        /// <summary>
        /// Returns point in the middle of the endpoints.
        /// </summary>
        public Vector2 Midpoint { get { return (Point1 + Point2) / 2f; } }

        /// <summary>
        /// Gives the corresponding line through the two points.
        /// </summary>
        public Line Line { get; private set; }

        /// <summary>
        /// A perpendicular line that crosses the segment in the midpoint.
        /// </summary>
        public Line Bissector
        {
            get
            {
                var perp = Vector2.Perpendicular(Point2 - Point1);
                return new Line(Midpoint, Midpoint + perp);
            }
        }

        /// <summary>
        /// Whether the segment is vertical.
        /// </summary>
        public bool IsVertical { get { return Line.IsVertical; } }

        /// <summary>
        /// Whether the segment is horizontal.
        /// </summary>
        public bool IsHorizontal { get { return Line.IsHorizontal; } }

        /// <summary>
        /// Length of the segment.
        /// </summary>
        public float Magnitude { get { return (Point2 - Point1).magnitude; } }

        /// <summary>
        /// Square of the magnitude (length) of the segment.
        /// </summary>
        public float SqrMagnitude { get { return (Point2 - Point1).sqrMagnitude; } }

        /// <summary>
        /// Interval in the x-dimension of the segment.
        /// </summary>
        public FloatInterval XInterval { get; private set; }

        /// <summary>
        /// Interval in the y-dimension of the segment.
        /// </summary>
        public FloatInterval YInterval { get; private set; }

        public LineSegment(Vector2 a_point1, Vector2 a_point2)
        {
            Point1 = new Vector2(a_point1.x, a_point1.y);
            Point2 = new Vector2(a_point2.x, a_point2.y);

            // explicitly calculate variables that are most used
            XInterval = new FloatInterval(a_point1.x, a_point2.x);
            YInterval = new FloatInterval(a_point1.y, a_point2.y);
            Line = new Line(Point1, Point2); 
        }

        public LineSegment(PolarPoint2D a_point1, PolarPoint2D a_point2) 
            : this(a_point1.Cartesian, a_point2.Cartesian)
        { }

        /// <summary>
        /// Checks whether the two lines are parallel.
        /// </summary>
        /// <param name="a_seg"></param>
        /// <returns></returns>
        public bool IsParallel(LineSegment a_seg)
        {
            return Line.IsParallel(a_seg.Line);
        }

        /// <summary>
        /// Checks whether the points lies to the right of the underlying line of the segment.
        /// </summary>
        /// <param name="a_Point"></param>
        /// <returns></returns>
        public bool IsRightOf(Vector2 a_Point)
        {
            return Line.PointRightOfLine(a_Point);
        }

        /// <summary>
        /// Checks whether the points lies on this segment (with some tolerance).
        /// </summary>
        /// <param name="a_Point"></param>
        /// <returns></returns>
        public bool IsOnSegment(Vector2 a_Point)
        {
            return XInterval.ContainsEpsilon(a_Point.x) &&
                YInterval.ContainsEpsilon(a_Point.y)
                && Line.IsOnLine(a_Point);
        }

        /// <summary>
        /// Checks whether the given points is equal to one of the endpoints (with some tolerance).
        /// </summary>
        /// <param name="a_Point"></param>
        /// <returns></returns>
        public bool IsEndpoint(Vector2 a_Point)
        {
            return MathUtil.EqualsEps(Point1, a_Point) || MathUtil.EqualsEps(Point2, a_Point);
        }

        /// <summary>
        /// Returns the unique intersection between a_seg1 and a_seg2. If there is one, if a_seg1 and a_seg2 coincide for a part we return 0
        /// </summary>
        /// <param name="a_seg1"></param>
        /// <param name="a_seg2"></param>
        /// <returns></returns>
        public static Vector2? Intersect(LineSegment a_seg1, LineSegment a_seg2)
        {
            // get intersection of the intervals
            var intervalXIntersection = a_seg1.XInterval.Intersect(a_seg2.XInterval);
            var intervalYIntersection = a_seg1.YInterval.Intersect(a_seg2.YInterval);

            // return quickly if intervals dont overlap
            if (intervalXIntersection == null || intervalYIntersection == null)
            {
                //Debug.Log("null1");
                return null;
            }

            // check for parallel lines
            if (a_seg1.IsParallel(a_seg2))
            {
                return null;
            }

            // get intersection of lines of segments
            // and check if intersection point on both segments
            var intersect = Line.Intersect(a_seg1.Line, a_seg2.Line);
            if (intersect != null &&
                intervalXIntersection.ContainsEpsilon(intersect.Value.x) &&
                intervalYIntersection.ContainsEpsilon(intersect.Value.y))
            {
                return intersect;
            }

            return null;
        }

        /// <summary>
        /// Check for proper intersection, meaning intersection point is not equal to one of the endpoints
        /// </summary>
        /// <param name="a_seg1"></param>
        /// <param name="a_seg2"></param>
        /// <returns></returns>
        public static Vector2? IntersectProper(LineSegment a_seg1, LineSegment a_seg2)
        {
            var eps = MathUtil.EPS * 10;

            // check for overlapping endpoints
            if (MathUtil.EqualsEps(a_seg1.Point1, a_seg2.Point1, eps) || MathUtil.EqualsEps(a_seg1.Point1, a_seg2.Point2, eps) ||
                MathUtil.EqualsEps(a_seg1.Point2, a_seg2.Point1, eps) || MathUtil.EqualsEps(a_seg1.Point2, a_seg2.Point2, eps))
            {
                return null;
            }

            // find (non-proper) intersection of segments
            var intersect = Intersect(a_seg1, a_seg2);

            if (intersect == null) return null;

            // check if intersection point equal to one of the endpoints
            var x = intersect.Value;
            if (MathUtil.EqualsEps(x, a_seg1.Point1, eps) || MathUtil.EqualsEps(x, a_seg1.Point2, eps) ||
                MathUtil.EqualsEps(x, a_seg2.Point1, eps) || MathUtil.EqualsEps(x, a_seg2.Point2, eps))
            {
                return null;
            }

            return x;
        }

        /// <summary>
        /// Intersection of segment and line.
        /// </summary>
        /// <param name="a_seg"></param>
        /// <param name="a_line"></param>
        /// <returns></returns>
        public static Vector2? Intersect(LineSegment a_seg, Line a_line)
        {
            // cf Interset(LineSegment, LineSegment)
            if (a_seg.Line.IsParallel(a_line))
            {
                return null;
            }

            var intersect = Line.Intersect(a_seg.Line, a_line);
            if (!intersect.HasValue) return null;

            if (a_seg.XInterval.ContainsEpsilon(intersect.Value.x) &&
                a_seg.YInterval.ContainsEpsilon(intersect.Value.y)) //Double check to handle single vertical segments
            {
                return intersect;
            }
            else if (MathUtil.EqualsEps(a_seg.Point1, intersect.Value, MathUtil.EPS * 100))
            {
                return a_seg.Point1;
            }
            else if (MathUtil.EqualsEps(a_seg.Point2, intersect.Value, MathUtil.EPS * 100))
            {
                return a_seg.Point2;
            }

            return null;
        }

        /// <summary>
        /// Proper intersection of line segment and line.
        /// </summary>
        /// <param name="a_seg"></param>
        /// <param name="a_line"></param>
        /// <returns></returns>
        public static Vector2? IntersectProper(LineSegment a_seg, Line a_line)
        {
            var intersect = Intersect(a_seg, a_line);
            if (intersect == null) return null;

            var x = (Vector2)intersect;

            if (MathUtil.EqualsEps(x, a_seg.Point1) || MathUtil.EqualsEps(x, a_seg.Point2))
            {
                return null;
            }

            return intersect;
        }

        /// <summary>
        /// Intersection of line segment and ray.
        /// </summary>
        /// <param name="a_seg"></param>
        /// <param name="a_ray"></param>
        /// <returns></returns>
        public static Vector2? Intersect(LineSegment a_seg, Ray2D a_ray)
        {
            var rayTarget = a_ray.origin + a_ray.direction;
            var rayLine = new Line(a_ray.origin, rayTarget);

            Vector2? ret;

            if (rayLine.IsParallel(a_seg.Line))
            {   // lines are parallel

                // check if ray origin is on line of line segment
                if (!a_seg.Line.IsOnLine(a_ray.origin)) return null;

                ret = a_seg.ClosestPoint(a_ray.origin);
            }
            else
            {
                ret = Intersect(a_seg, rayLine);
            }

            if (ret == null) return null;

            // check if intersection in wrong direction
            if (!MathUtil.EqualsEps(ret.Value, a_ray.origin, MathUtil.EPS * 10) &&
                Vector2.Dot((ret.Value - a_ray.origin), a_ray.direction) < 0)
            {
                return null;
            }

            return ret;
        }

        /// <summary>
        /// Intersects the line segment with the rect boundary.
        /// </summary>
        /// <param name="a_seg"></param>
        /// <param name="a_rect"></param>
        /// <returns></returns>
        public static List<Vector2> Intersect(LineSegment a_seg, Rect a_rect)
        {
            var intersections = new List<Vector2>();
            Vector2? intersection;

            // left side
            var left = new LineSegment(new Vector2(a_rect.xMin, a_rect.yMin), new Vector2(a_rect.xMin, a_rect.yMax));
            intersection = Intersect(a_seg, left);
            if (intersection != null) intersections.Add((Vector2)intersection);

            // bottom side
            var bottom = new LineSegment(new Vector2(a_rect.xMin, a_rect.yMin), new Vector2(a_rect.xMax, a_rect.yMin));
            intersection = Intersect(a_seg, bottom);
            if (intersection != null) intersections.Add((Vector2)intersection);

            // right side
            var right = new LineSegment(new Vector2(a_rect.xMax, a_rect.yMin), new Vector2(a_rect.xMax, a_rect.yMax));
            intersection = Intersect(a_seg, right);
            if (intersection != null) intersections.Add((Vector2)intersection);

            // top side
            var top = new LineSegment(new Vector2(a_rect.xMin, a_rect.yMax), new Vector2(a_rect.xMax, a_rect.yMax));
            intersection = Intersect(a_seg, top);
            if (intersection != null) intersections.Add((Vector2)intersection);

            return intersections;
        }

        /// <summary>
        /// returns a list of intersections of a segment with the given segments.
        /// Sorted with the one closest to point1 of given segment.
        /// </summary>
        /// <param name="a_segments"></param>
        /// <returns></returns>
        public static List<Vector2> Intersect(LineSegment a_seg, IEnumerable<LineSegment> a_segments)
        {
            var intersections = new List<Vector2>();

            //find all intersections
            foreach (var segment in a_segments)
            {
                var intersection = Intersect(a_seg, segment);
                if (intersection.HasValue)
                {
                    intersections.Add(intersection.Value);
                }
            }

            return intersections;
        }

        // Some helper methods for easy use that point to static corresponding methods

        internal Vector2? Intersect(LineSegment a_seg)
        {
            return Intersect(this, a_seg);
        }

        internal Vector2? IntersectProper(LineSegment a_seg)
        {
            return IntersectProper(this, a_seg);
        }

        internal Vector2? Intersect(Line a_line)
        {
            return Intersect(this, a_line);
        }

        internal Vector2? IntersectProper(Line a_line)
        {
            return IntersectProper(this, a_line);
        }

        internal Vector2? Intersect(Ray2D a_ray)
        {
            return Intersect(this, a_ray);
        }

        public List<Vector2> Intersect(Rect a_rect)
        {
            return Intersect(this, a_rect);
        }

        public List<Vector2> Intersect(IEnumerable<LineSegment> a_segments)
        {
            return Intersect(this, a_segments);
        }

        /// <summary>
        /// Computes the distance between this line and the given point 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        internal float DistanceToPoint(Vector2 v)
        {
            var closest = ClosestPoint(v);
            return Vector2.Distance(closest, v);
        }

        /// <summary>
        /// Finds the closest point on the segment to the given point
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        internal Vector2 ClosestPoint(Vector2 v)
        {
            var normalLine = new Line(v, v + Line.Normal);
            var intersection = Intersect(normalLine);
            if (intersection == null)
            {
                return Vector2.Distance(Point1, v) < Vector2.Distance(Point2, v) ? Point1 : Point2;
            }
            else
            {
                return (Vector2)intersection;
            }
        }

        /// <summary>
        /// The normal to the right of this segment if we view it as oriented from point1 to point2
        /// </summary>
        /// <returns></returns>
        public Vector2 RightNormal()
        {
            var ori = Orientation();
            return new Vector2(ori.y, -ori.x);
        }

        /// <summary>
        /// The orientation vector of this line segment if we view it as oriented from point1 to point2
        /// </summary>
        /// <returns></returns>
        public Vector2 Orientation()
        {
            return Point2 - Point1;
        }

        /// <summary>
        /// Returns X-value on the line segement corresponding to the given y value
        /// </summary>
        /// <param name="a_y"></param>
        /// <returns></returns>
        public float X(float a_y)
        {
            if (YInterval.ContainsEpsilon(a_y))
            {
                return Line.X(a_y);
            }
            else
            {
                throw new GeomException("X-value requested for y:" + a_y + "not in Y-interval" + YInterval.ToString());
            }
        }

        /// <summary>
        ///  Returns Y-value on the line segement corresponding to the given x value
        /// </summary>
        /// <param name="a_x"></param>
        /// <returns></returns>
        public float Y(float a_x)
        {
            if (XInterval.ContainsEpsilon(a_x))
            {
                return Line.Y(a_x);
            }
            else
            {
                throw new GeomException("Y-value requested for x:" + a_x + "not in x-interval" + XInterval.ToString());
            }
        }

        public bool Equals(LineSegment other)
        {
            return MathUtil.EqualsEps(Point1, other.Point1) &&
                MathUtil.EqualsEps(Point2, other.Point2);
        }

        public override int GetHashCode()
        {
            return 47 * Point1.GetHashCode() + Point2.GetHashCode();
        }

        public override string ToString()
        {
            return "Segment: (" + Point1 + "," + Point2 + ")";
        }
    }
}
