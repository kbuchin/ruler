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
        public bool IsVertical { get { return MathUtil.EqualsEps(Point1.x, Point2.x); } }

        /// <summary>
        /// Whether the given line is horizontal (with some tolerance).
        /// </summary>
        public bool IsHorizontal { get { return MathUtil.EqualsEps(Point1.y, Point2.y); } }

        /// <summary>
        /// Gives the angle of the line w.r.t the horizontal x-axis. reports in radians
        /// </summary>
        public float Angle { get { return (float)Math.Atan(Slope); } }

        /// <summary>
        /// Gives a 2D normal vector to this line
        /// </summary>
        public Vector2 Normal
        {
            get { return IsVertical ? new Vector2(1f, 0f) : new Vector2(Slope, -1f); }
        }

        /// <summary>
        /// Height at the intersection with y axis, or NaN when line is vertical.
        /// </summary>
        public float HeightAtYAxis
        {
            get
            {
                return IsVertical ? float.NaN : Point1.y - Slope * Point1.x;
            }
        }

        /// <summary>
        /// Height at the intersection with x axis, or NaN when line is horizontal.
        /// </summary>
        public float WidthAtXAxis
        {
            get { return X(0); }
        }

        /// <summary>
        /// Slope of the line (y / x), or infinity when line vertical.
        /// </summary>
        public float Slope
        {
            get
            {
                return IsVertical ? float.PositiveInfinity : (Point1.y - Point2.y) / (Point1.x - Point2.x);
            }
        }

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
        }

        /// <summary>
        /// Creates a line with the given slope and intersection with the y axis.
        /// Line given as y = a*x + b.
        /// </summary>
        /// <param name="a_slope"></param>
        /// <param name="a_heigthatyaxis"></param>
        public Line(float a_slope, float a_heigthatyaxis)
        {
            Point1 = new Vector2(0, a_heigthatyaxis);
            //Point2 = new Vector2(10, a_heigthatyaxis + 10 * a_slope);
            if (MathUtil.IsFinite(a_slope))
            {
                Debug.Log(a_slope);
                Point2 = new Vector2(1, a_heigthatyaxis + a_slope);
            }
            else
            {
                // line vertical
                Point2 = new Vector2(0, HeightAtYAxis + 1);
            }

            m_oriented = false;
        }

        /// <summary>
        /// Create line going trough a certain point and with a certian angle wrt to the positive x-axis
        /// </summary>
        /// <param name="a_point"></param>
        /// <param name="a_angle"></param>
        public Line(Vector2 a_point, float a_angle)
        {
            Point1 = new Vector2(a_point.x, a_point.y);
            Point2 = Point1 + new Vector2(Mathf.Cos(a_angle), Mathf.Sin(a_angle));
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
            if (a_line1.IsVertical && a_line2.IsVertical)
            {
                Debug.Log("Vertical");
                return null;
                //throw new GeomException("Two vertical lines ");
            }

            if (MathUtil.EqualsEps(a_line1.Slope, a_line2.Slope))
            {
                Debug.Log("Parallel");
                return null;
                //throw new GeomException("Two parallel lines");
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
        /// Finds whether the given point lies on the line (with some tolerance).
        /// </summary>
        /// <param name="a_Point"></param>
        /// <returns></returns>
        public bool IsOnLine(Vector2 a_Point)
        {
            return float.IsInfinity(Slope) ? MathUtil.EqualsEps(Point1.x, a_Point.x) 
                : MathUtil.EqualsEps(Y(a_Point.x), a_Point.y);
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

            if (float.IsInfinity(Slope))
            {
                return Point2.y < Point1.y ? a_point.x < Point1.x: a_point.x > Point1.x;
            }

            //consider orientation
            return Point1.x < Point2.x ? !PointAbove(a_point) : PointAbove(a_point);
        }

        /// <summary>
        /// Computes the distance between this line and the given point 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        internal float DistanceToPoint(Vector2 v)
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

        public override string ToString()
        {
            return "Line: (" + Point1 + "," + Point2 + ")";
        }
    }
}