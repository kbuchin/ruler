namespace Util.Math
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using MNMatrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;

    public static class MathUtil
    {
        /// <summary>
        /// Checks whether a float is finite and not NaN
        /// </summary>
        /// <param name="a_float"></param>
        /// <returns> True when <paramref name="a_val1"/> is not infinite or NaN</returns>
        public static bool isFinite(this float a_float)
        {
            return !(float.IsInfinity(a_float) || float.IsNaN(a_float));
        }

        /// <summary>
        /// Compares two values to see if they are within Epsilon distance of each other
        /// </summary>
        /// <param name="a_val1"></param>
        /// <param name="a_val2"></param>
        /// <returns> True when the difference between <paramref name="a_val1"/> and <paramref name="a_val2"/> is less then a small Epsilon</returns>
        public static bool EqualsEps(float a_val1, float a_val2)
        {
            return Mathf.Approximately(a_val1, a_val2);
        }

        /// <summary>
        /// A positive modulo operation. (i.e. mod(-3, 4) == 1)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static float PositiveMod(float a, float m)
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
        public static float Angle(Vector2 x, Vector2 a, Vector2 b)
        {
            var va = a - x;
            var vb = b - x;
            var SignedAngle = Math.Atan2(vb.y, vb.x) - Math.Atan2(va.y, va.x);
            if (SignedAngle >= 0) return (float)SignedAngle;
            else return (float)(2f * Math.PI + SignedAngle);
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
        public static double Orient2D(Vector2 a, Vector2 b, Vector2 c)
        {
            var orientArray = new double[,]
                {
                    { a.x - c.x, a.y - c.y },
                    { b.x - c.x, b.y - c.y }
                };

            MNMatrix orientMatrix = MNMatrix.Build.DenseOfArray(orientArray);
            return orientMatrix.Determinant();
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
            int orientation = Math.Sign(MathUtil.Orient2D(a, b, c));
            if (orientation == 0)
            {
                Debug.LogWarning("Tried to compute InCircle on degenerate circle");
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

            if (!isFinite((float)Ox) || !isFinite((float)Oy))
            {
                throw new Exception("Result of CalculateCircumcenterStable was invalid!");
            }

            return new Vector2((float)Ox, (float)Oy);
        }
    }
}
