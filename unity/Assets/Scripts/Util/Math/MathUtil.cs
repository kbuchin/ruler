namespace Util.Math
{
    using System;
    using UnityEngine;
    using Util.Geometry;

    /// <summary>
    /// Utility class that extends Math and Mathf with additional methods.
    /// Mainly used for floating point comparison and some vector and triangle functionality.
    /// </summary>
    public static class MathUtil
    {
        /// <summary>
        /// Small value used for floating point comparison
        /// </summary>
        public const double EPS = 1e-4;

        /// <summary>
        /// Some constants (based on Mathf)
        /// </summary>
        public static double PI { get { return Math.PI; } }
        public static double PI2 { get { return 2 * Math.PI; } }

        /// <summary>
        /// Checks whether a float is finite and not NaN
        /// </summary>
        /// <param name="a_float"></param>
        /// <returns> True when <paramref name="a_val1"/> is not infinite or NaN</returns>
        public static bool IsFinite(this double a_float)
        {
            return !(double.IsInfinity(a_float) || double.IsNaN(a_float));
        }

        /// <summary>
        /// Checks whether a vector2 is finite and not NaN
        /// </summary>
        /// <param name="a_float"></param>
        /// <returns> True when <paramref name="a_val1"/> is not infinite or NaN</returns>
        public static bool IsFinite(this Vector2 a_vector)
        {
            return IsFinite(a_vector.x) && IsFinite(a_vector.y);
        }

        /// <summary>
        /// Compares two values to see if they are within Epsilon distance of each other
        /// </summary>
        /// <param name="a_val1"></param>
        /// <param name="a_val2"></param>
        /// <returns> True when the difference between <paramref name="a_val1"/> and <paramref name="a_val2"/> is less then a small Epsilon</returns>
        public static bool EqualsEps(double a_val1, double a_val2, double eps = EPS)
        {
            return Math.Abs(a_val1 - a_val2) < eps;
        }

        /// <summary>
        /// Compares two vectors to see if they are within Epsilon distance of each other
        /// </summary>
        /// <param name="a_val1"></param>
        /// <param name="a_val2"></param>
        /// <returns></returns>
        public static bool EqualsEps(Vector2 a_val1, Vector2 a_val2, double eps = EPS)
        {
            return (a_val1 - a_val2).sqrMagnitude < eps;
        }

        /// <summary>
        /// Compares to floats to see if a >= b - eps
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool GEQEps(double a, double b, double eps = EPS)
        {
            return EqualsEps(a, b, eps) || (a > b);
        }

        /// <summary>
        /// Compares to floats to see if a <= b + eps
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool LEQEps(double a, double b, double eps = EPS)
        {
            return EqualsEps(a, b, eps) || (a < b);
        }

        /// <summary>
        /// Compares to floats to see if a > b + eps
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool GreaterEps(double a, double b, double eps = EPS)
        {
            return a > b && !EqualsEps(a, b, eps);
        }

        /// <summary>
        /// Compares to floats to see if a < b - eps
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool LessEps(double a, double b, double eps = EPS)
        {
            return a < b && !EqualsEps(a, b, eps);
        }

        /// <summary>
        /// A positive modulo operation. (i.e. mod(-3, 4) == 1)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static double PositiveMod(double a, double m)
        {
            return (a % m + m) % m;
        }

        /// <summary>
        /// A positive modulo operation. (i.e. mod(-3, 4) == 1)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static int PositiveMod(int a, int m)
        {
            return (a % m + m) % m;
        }

        /// <summary>
        /// Returns the angle axb in radians from 0 to 2*pi.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns> angle axb in [0, 2*pi) </returns>
        public static double Angle(Vector2 x, Vector2 a, Vector2 b)
        {
            var va = a - x;
            var vb = b - x;
            var SignedAngle = Math.Atan2(vb.y, vb.x) - Math.Atan2(va.y, va.x);

            if (SignedAngle < -PI- EPS || SignedAngle > PI + EPS)
            {
                throw new Exception("Invalid angle");
            }

            if (SignedAngle >= 0) return SignedAngle;
            else return PI2 + SignedAngle;
        }

        /// <summary>
        /// Rotates the given vector by an angle in radians.
        /// </summary>
        /// <param name="a_Point"></param>
        /// <param name="angleRadians"></param>
        /// <returns></returns>
        public static Vector2 Rotate(Vector2 a_Point, double angleRadians)
        {
            var s = Math.Sin(angleRadians);
            var c = Math.Cos(angleRadians);

            return new Vector2(
                (float)(a_Point.x * c - a_Point.y * s),
                (float)(a_Point.y * c + a_Point.x * s)
            );
        }

        /// <summary>
        /// Returns +1 if points abc correspond to a left turn,
        /// -1 if they correspond to a right turn
        /// and zero if the points are collinear.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static int Orient2D(Vector2 a, Vector2 b, Vector2 c)
        {
            var w = b - a;
            var q = c - b;
            return Math.Sign(w.x * q.y - w.y * q.x);
        }

        /// <summary>
        /// Signed area of the triangle (p0, p1, p2)
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double SignedArea(Vector2D p0, Vector2D p1, Vector2D p2)
        {
            return (p0.x - p2.x) * (p1.y - p2.y) - (p1.x - p2.x) * (p0.y - p2.y);
        }
    }
}
