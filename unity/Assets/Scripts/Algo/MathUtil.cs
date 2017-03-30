using System;


namespace Algo
{
    static class MathUtil
    {
        public const float Epsilon = 0.0001f;

        public static bool isFinite(this float a_float)
        {
            return !(float.IsInfinity(a_float) || float.IsNaN(a_float));
        }

        internal static bool Equals(float a_val1, float a_val2, float a_epsilon)
        {
            return Math.Abs(a_val1 - a_val2) < a_epsilon;
        }

        /// <summary>
        /// Compares two values to see if they are within Epsilon distance of each other
        /// </summary>
        /// <param name="a_val1"></param>
        /// <param name="a_val2"></param>
        /// <returns> True when the difference between <paramref name="a_val1"/> and <paramref name="a_val2"/> is less then a small Epsilon</returns>
        internal static bool EqualsEps(float a_val1, float a_val2)
        {
            return Equals(a_val1, a_val2, Epsilon);
        }

        /// <summary>
        /// A positive modulo operation. (i.e. mod(-3, 4) == 1)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        internal static int Mod(int a, int m)
        {
            return (a % m + m) % m;
        }

        /// <summary>
        /// A positive modulo operation. (i.e. mod(-3, 4) == 1)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        internal static float Mod(float a, float m)
        {
            return (a % m + m) % m;
        }

    }
}
