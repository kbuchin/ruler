namespace Util.Geometry
{
    using System;
    using Util.Math;

    /// <summary>
    /// Represents the interval [Min, Max] given by the two floats.
    /// </summary>
    public class FloatInterval : IComparable<FloatInterval>, IEquatable<FloatInterval>
    {
        public float Max { get; private set; }
        public float Min { get; private set; }

        public FloatInterval(float a_val1, float a_val2)
        {
            Min = Math.Min(a_val1, a_val2);
            Max = Math.Max(a_val1, a_val2);
        }

        /// <summary>
        /// Check whether the value is inside the given interval.
        /// </summary>
        /// <remarks>
        /// Use ContainsEpsilon whenever some tolerance is needed.
        /// </remarks>
        /// <param name="a_val"></param>
        /// <returns></returns>
        public bool Contains(float a_val)
        {
            return Min <= a_val && a_val <= Max;
        }

        /// <summary>
        /// Version of contians with a small build in tolerance
        /// </summary>
        /// <param name="a_val"></param>
        /// <returns></returns>
        public bool ContainsEpsilon(float a_val)
        {
            var eps = MathUtil.EPS * 100;
            return MathUtil.GEQEps(a_val, Min, eps) && MathUtil.LEQEps(a_val, Max, eps);
        }

        /// <summary>
        /// Intersects two Intervals. Returns null when there is no intersection
        /// </summary>
        /// <param name="a_1"></param>
        /// <param name="a_2"></param>
        /// <returns> The intersection of the two intervals. Null when there is no intersection</returns>
        public static FloatInterval Intersect(FloatInterval a_1, FloatInterval a_2)
        {
            if (MathUtil.LessEps(a_1.Max, a_2.Min) || MathUtil.LessEps(a_2.Max, a_1.Min))
            {
                return null;
            }
            return new FloatInterval(Math.Max(a_1.Min, a_2.Min), Math.Min(a_1.Max, a_2.Max));
        }

        /// <summary>
        /// Finds the intersection between this interval and the given one.
        /// </summary>
        /// <param name="a_other"></param>
        /// <returns></returns>
        public FloatInterval Intersect(FloatInterval a_other)
        {
            return Intersect(this, a_other);
        }

        public override string ToString()
        {
            return "Min: " + Min + " Max: " + Max;
        }

        public int CompareTo(FloatInterval other)
        {
            // by default sort on start of interval
            return Min.CompareTo(other.Min);
        }

        public bool Equals(FloatInterval other)
        {
            return MathUtil.EqualsEps(Min, other.Min) && MathUtil.EqualsEps(Max, other.Max);
        }
        public override int GetHashCode()
        {
            return 37 * Min.GetHashCode() + Max.GetHashCode();
        }
    }
}