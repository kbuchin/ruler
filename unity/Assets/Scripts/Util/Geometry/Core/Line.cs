namespace Util.Geometry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Math;

    /// <summary>
    /// Default line class including some intersection and point-line auxiliary methods.
    /// Lines are by default sorted on slope.
    /// </summary>
    public class Line : IComparable<Line>, IEquatable<Line>
    {
        public Vector2 Point1 { get; private set; }
        public Vector2 Point2 { get; private set; }

        // whether the line has a given orientation/direction
        // used for right of line check
        private bool m_oriented;

        /// <summary>
        /// Whether the given line is vertical (with some tolerance).
        /// </summary>
        public bool IsVertical { get; private set; }

        /// <summary>
        /// Whether the given line is horizontal (with some tolerance).
        /// </summary>
        public bool IsHorizontal { get; private set; }

        /// <summary>
        /// Gives the angle of the line w.r.t the horizontal x-axis. reports in radians
        /// </summary>
        public float Angle { get { return (float)Math.Atan(Slope); } }

        /// <summary>
        /// Gives a 2D normal vector to this line.
        /// </summary>
        public Vector2 Normal
        {
            get { return IsVertical ? new Vector2(1f, 0f) : new Vector2(Slope, -1f); }
        }

        /// <summary>
        /// Height at the intersection with y axis, or NaN when line is vertical.
        /// </summary>
        public float HeightAtYAxis { get; private set; }

        /// <summary>
        /// Height at the intersection with x axis, or NaN when line is horizontal.
        /// </summary>
        public float WidthAtXAxis { get; private set; }

        /// <summary>
        /// Slope of the line (y / x), or infinity when line vertical.
        /// </summary>
        public float Slope { get; private set; }

        /// <summary>
        /// Creates a line through the given points
        /// </summary>
        /// <param name="a_point1"></param>
        /// <param name="a_point2"></param>
        public Line(Vector2 a_point1, Vector2 a_point2)
        {
            // create copy
            Point1 = new Vector2(a_point1.x, a_point1.y);
            Point2 = new Vector2(a_point2.x, a_point2.y);
            m_oriented = true;

            // explicitly calculate variables that are most used, for speedup
            IsVertical = MathUtil.EqualsEps(Point1.x, Point2.x, MathUtil.EPS * 100);
            IsHorizontal = MathUtil.EqualsEps(Point1.y, Point2.y, MathUtil.EPS * 100);
            var p1 = a_point1.x < a_point2.x ? a_point1 : a_point2;
            var p2 = a_point1.x < a_point2.x ? a_point2 : a_point1;
            Slope =  IsVertical ? float.PositiveInfinity : (p1.y - p2.y) / (p1.x - p2.x);
            HeightAtYAxis = IsVertical ? float.NaN : Point1.y - Slope * Point1.x;
            WidthAtXAxis = X(0);
        }

        /// <summary>
        /// Creates a line with the given slope and intersection with the y axis.
        /// Line given as y = a*x + b.
        /// </summary>
        /// <param name="a_slope"></param>
        /// <param name="a_heigthatyaxis"></param>
        public Line(float a_slope, float a_heigthatyaxis)
            : this(new Vector2(0, a_heigthatyaxis), MathUtil.IsFinite(a_slope) ? 
                  new Vector2(1, a_heigthatyaxis + a_slope) : new Vector2(0, a_heigthatyaxis + 1))
        {
            m_oriented = false;
        }

        /// <summary>
        /// Create line going trough a certain point and with a certian angle wrt to the positive x-axis
        /// </summary>
        /// <param name="a_point"></param>
        /// <param name="a_angle"></param>
        public Line(Vector2 a_point, float a_angle)
            : this(a_point, a_point + new Vector2(Mathf.Cos(a_angle), Mathf.Sin(a_angle)))
        {
            m_oriented = false;
        }

        /// <summary>
        /// Returns the x value at the given y, or NaN when horizontal
        /// </summary>
        /// <param name="a_y"></param>
        /// <returns></returns>
        public float X(float a_y)
        {
            if (IsHorizontal)
            {
                return float.NaN;
            }
            return IsVertical ? Point1.x : (a_y - HeightAtYAxis) / Slope;
        }

        /// <summary>
        /// Returns the y value at the given x, or NaN when vertical
        /// </summary>
        /// <param name="a_x"></param>
        /// <returns></returns>
        public float Y(float a_x)
        {
            if (IsVertical)
            {
                return float.NaN;
            }
            return HeightAtYAxis + Slope * a_x;
        }

        /// <summary>
        /// Finds the intersection point of the two lines.
        /// </summary>
        /// <param name="a_line1"></param>
        /// <param name="a_line2"></param>
        /// <returns></returns>
        public static Vector2? Intersect(Line a_line1, Line a_line2)
        {
            if (a_line1.IsParallel(a_line2))
            {
                return null;
            }

            if (!a_line1.IsVertical && !a_line2.IsVertical)
            {
                // two non-vertical lines, can use regular method based on slope

                var dy = a_line1.HeightAtYAxis - a_line2.HeightAtYAxis;
                var comparitiveslope = a_line1.Slope - a_line2.Slope;
                var x = -dy / comparitiveslope;

                return new Vector2(x, a_line1.Y(x));
            }
            else
            {
                // one vertical and one "normal"
                var verticalLine = a_line1.IsVertical ? a_line1 : a_line2;
                var normalLine = a_line1.IsVertical ? a_line2 : a_line1;

                var x = verticalLine.Point1.x;

                return new Vector2(x, normalLine.Y(x));
            }
        }

        /// <summary>
        /// Intersect this line with given other line.
        /// </summary>
        /// <param name="a_otherline"></param>
        /// <returns></returns>
        public Vector2? Intersect(Line a_otherline)
        {
            return Intersect(this, a_otherline);
        }

        /// <summary>
        /// Intersect this line with given other line.
        /// </summary>
        /// <param name="a_otherline"></param>
        /// <returns></returns>
        public Vector2? Intersect(Ray2D a_ray)
        {
            return Intersect(this, a_ray);
        }

        public Vector2? Intersect(Line a_line, Ray2D a_ray)
        {
            var rayTarget = a_ray.origin + a_ray.direction;
            var rayLine = new Line(a_ray.origin, rayTarget);

            Vector2? ret;

            if (rayLine.IsParallel(a_line))
            {
                // check if ray origin is on line of line segment
                if (a_line.IsOnLine(a_ray.origin)) return a_ray.origin;
                else return null;
            }
            else
            {
                ret = Intersect(a_line, rayLine);
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

        public bool IsParallel(Line a_otherLine)
        {
            // check specifically for verticality
            // tolerance will break if slopes close to infinity
            return IsVertical && a_otherLine.IsVertical || MathUtil.EqualsEps(Slope, a_otherLine.Slope);
        }

        /// <summary>
        /// Finds whether the given point lies on the line (with some tolerance).
        /// </summary>
        /// <param name="a_Point"></param>
        /// <returns></returns>
        public bool IsOnLine(Vector2 a_Point)
        {
            return DistanceToPoint(a_Point) < MathUtil.EPS * 100;
        }

        /// <summary>
        /// Checks whether the given points lies above the line.
        /// </summary>
        /// <param name="a_point"></param>
        /// <returns></returns>
        public bool PointAbove(Vector2 a_point)
        {
            //Returns true when point is above line (or left in the vertical case)
            if (IsVertical)
            {
                return a_point.x < Point1.x;
            }
            else
            {
                var lineY = Y(a_point.x);
                return a_point.y > lineY;
            }
        }

        /// <summary>
        /// Counts the number of points above the line.
        /// </summary>
        /// <param name="a_points"></param>
        /// <returns></returns>
        public int NumberOfPointsAbove(IEnumerable<Vector2> a_points)
        {
            return a_points.Where(p => PointAbove(p)).Count();
        }

        /// <summary>
        /// Returns whether the point is on the right of the line.
        /// </summary>
        /// <param name="a_point"></param>
        /// <returns></returns>
        public bool PointRightOfLine(Vector2 a_point)
        {
            if (!m_oriented)
            {
                throw new GeomException("Can't test rightness on unoriented line");
            }

            if (IsVertical)
            {
                return Point2.y < Point1.y ? a_point.x < Point1.x : a_point.x > Point1.x;
            }
            
            //consider orientation
            return Point1.x < Point2.x ? !PointAbove(a_point) : PointAbove(a_point);
        }

        /// <summary>
        /// Computes the distance between this line and the given point 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public float DistanceToPoint(Vector2 v)
        {
            var normalLine = new Line(v, v + Normal);
            var intersection = Intersect(normalLine);       // cannot be null
            return Vector2.Distance(v, intersection.Value);
        }

        public int CompareTo(Line a_other)
        {
            return Slope.CompareTo(a_other.Slope);
        }

        public bool Equals(Line other)
        {
            return MathUtil.EqualsEps(Point1, other.Point1) &&
                MathUtil.EqualsEps(Point2, other.Point2);
        }

        public override int GetHashCode()
        {
            return 71 * Point1.GetHashCode() + Point2.GetHashCode();
        }

        public override string ToString()
        {
            return "Line: (" + Point1 + "," + Point2 + ")";
        }

        /// <summary>
        /// Checks if three points lie on a single line
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns>whether the three points are colinear</returns>
        public static bool Colinear(Vector2 a, Vector2 b, Vector2 c)
        {
            return new Line(a, b).IsOnLine(c);
        }
    }
}