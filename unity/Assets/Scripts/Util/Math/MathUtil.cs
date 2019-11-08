namespace Util.Math
{
    using System;
    using UnityEngine;
    using Util.Geometry;
    using MNMatrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;

    /// <summary>
    /// Utility class that extends Math and Mathf with additional methods.
    /// Mainly used for floating point comparison and some vector and triangle functionality.
    /// </summary>
    public static class MathUtil
    {
        /// <summary>
        /// Small value used for floating point comparison
        /// </summary>
        public const float EPS = 1e-5f;

        /// <summary>
        /// Some constants (based on Mathf)
        /// </summary>
        public static float PI { get { return Mathf.PI; } }
        public static float PI2 { get { return 2 * Mathf.PI; } }

        /// <summary>
        /// Checks whether a float is finite and not NaN
        /// </summary>
        /// <param name="a_float"></param>
        /// <returns> True when <paramref name="a_val1"/> is not infinite or NaN</returns>
        public static bool IsFinite(this float a_float)
        {
            return !(float.IsInfinity(a_float) || float.IsNaN(a_float));
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
        public static bool EqualsEps(float a_val1, float a_val2, float eps = EPS)
        {
            return Mathf.Abs(a_val1 - a_val2) < eps;
        }

        /// <summary>
        /// Compares two vectors to see if they are within Epsilon distance of each other
        /// </summary>
        /// <param name="a_val1"></param>
        /// <param name="a_val2"></param>
        /// <returns></returns>
        public static bool EqualsEps(Vector2 a_val1, Vector2 a_val2, float eps = EPS)
        {
            return (a_val1 - a_val2).sqrMagnitude < eps;
        }

        /// <summary>
        /// Compares to floats to see if a >= b - eps
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool GEQEps(float a, float b, float eps = EPS)
        {
            return EqualsEps(a, b, eps) || (a > b);
        }

        /// <summary>
        /// Compares to floats to see if a <= b + eps
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool LEQEps(float a, float b, float eps = EPS)
        {
            return EqualsEps(a, b, eps) || (a < b);
        }

        /// <summary>
        /// Compares to floats to see if a > b + eps
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool GreaterEps(float a, float b, float eps = EPS)
        {
            return a > b && !EqualsEps(a, b, eps);
        }

        /// <summary>
        /// Compares to floats to see if a < b - eps
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool LessEps(float a, float b, float eps = EPS)
        {
            return a < b && !EqualsEps(a, b, eps);
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
        public static float Angle(Vector2 x, Vector2 a, Vector2 b)
        {
            var va = a - x;
            var vb = b - x;
            var SignedAngle = Math.Atan2(vb.y, vb.x) - Math.Atan2(va.y, va.x);

            if (SignedAngle < -Math.PI - EPS || SignedAngle > Math.PI + EPS)
            {
                throw new GeomException("Invalid angle");
            }

            if ((float)SignedAngle >= 0) return (float)SignedAngle;
            else return (float)(2.0 * Math.PI + SignedAngle);
        }

        /// <summary>
        /// Rotates the given vector by an angle in radians.
        /// </summary>
        /// <param name="a_Point"></param>
        /// <param name="angleRadians"></param>
        /// <returns></returns>
        public static Vector2 Rotate(Vector2 a_Point, float angleRadians)
        {
            var s = Mathf.Sin(angleRadians);
            var c = Mathf.Cos(angleRadians);

            return new Vector2(
                a_Point.x * c - a_Point.y * s,
                a_Point.y * c + a_Point.x * s
            );
        }

        /// <summary>
        /// Returns a positive value if the points a, b, and c are arranged in
        /// counterclockwise order, a negative value if the points are in clockwise order,
        /// and zero if the points are collinear.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static int Orient2D(Vector2 a, Vector2 b, Vector2 c)
        {
            // get cross product
            var orientArray = new double[,]
                {
                    { a.x - c.x, a.y - c.y },
                    { b.x - c.x, b.y - c.y }
                };

            MNMatrix orientMatrix = MNMatrix.Build.DenseOfArray(orientArray);
            return Math.Sign(orientMatrix.Determinant());
        }

        /// <summary>
        /// Checks whether point X is inside a circle defined by points a, b, c.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="X"></param>
        /// <returns>Whether point X is within a circle defined by points a, b, c.</returns>
        public static bool InsideCircle(Vector2 a, Vector2 b, Vector2 c, Vector2 X)
        {
            // calculate turn orientation
            int orientation = Math.Sign(MathUtil.Orient2D(a, b, c));
            if (orientation == 0)
            {
                // straight line, so degenerate circle
                return false;
            }

            double[,] inCircleArray = new double[,]
            {
                { a.x - X.x, a.y - X.y, Mathf.Pow(a.x - X.x, 2) + Mathf.Pow(a.y - X.y, 2) },
                { b.x - X.x, b.y - X.y, Mathf.Pow(b.x - X.x, 2) + Mathf.Pow(b.y - X.y, 2) },
                { c.x - X.x, c.y - X.y, Mathf.Pow(c.x - X.x, 2) + Mathf.Pow(c.y - X.y, 2) },
            };

            MNMatrix inCircleMatrix = MNMatrix.Build.DenseOfArray(inCircleArray);
            int inside = Math.Sign(inCircleMatrix.Determinant());

            if (inside == 0)
            {
                return false;
            }
            else
            {
                return orientation == inside;
            }
        }

        /// <summary>
        /// Calculates the center point of a circle defined by points a, b, c.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns>the circumcenter of circle abc.</returns>
        public static Vector2 CalculateCircumcenter(Vector2 a, Vector2 b, Vector2 c)
        {
            double[,] numerator = new double[,]
            {
                { Mathf.Pow(a.x - c.x, 2) + Mathf.Pow(a.y - c.y, 2), a.y - c.y },
                { Mathf.Pow(b.x - c.x, 2) + Mathf.Pow(b.y - c.y, 2), b.y - c.y }
            };
            double[,] denomenator = new double[,]
            {
                { a.x - c.x, a.y - c.y },
                { b.x - c.x, b.y - c.y }
            };

            MNMatrix numeratorMatrix = MNMatrix.Build.DenseOfArray(numerator);
            MNMatrix denomenatorMatrix = MNMatrix.Build.DenseOfArray(denomenator);
            double numeratorDeterminant = numeratorMatrix.Determinant();
            double denomenatorDeterminant = denomenatorMatrix.Determinant();
            double Ox = c.x + numeratorDeterminant / (2 * denomenatorDeterminant);

            numerator = new double[,]
            {
                { a.x - c.x, Mathf.Pow(a.x - c.x, 2) + Mathf.Pow(a.y - c.y, 2) },
                { b.x - c.x, Mathf.Pow(b.x - c.x, 2) + Mathf.Pow(b.y - c.y, 2) }
            };

            numeratorMatrix = MNMatrix.Build.DenseOfArray(numerator);
            numeratorDeterminant = numeratorMatrix.Determinant();

            double Oy = c.y + numeratorDeterminant / (2 * denomenatorDeterminant);

            if (!IsFinite((float)Ox) || !IsFinite((float)Oy))
            {
                throw new GeomException("Result of CalculateCircumcenterStable was invalid!");
            }

            return new Vector2((float)Ox, (float)Oy);
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
