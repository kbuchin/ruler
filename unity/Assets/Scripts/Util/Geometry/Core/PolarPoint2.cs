namespace Util.Geometry
{
    using System;
    using UnityEngine;
    using Util.Math;

    /// <summary>
    /// Point representation as a distance-angle pair (r, theta) to origin.
    /// </summary>
    public class PolarPoint2D : IEquatable<PolarPoint2D>
    {
        public double R { get; set; } // distance to p
        public double Theta { get; set; } // angle with respect to p

        // polar point from distance and angle (in radians)
        public PolarPoint2D(double r, double theta)
        {
            R = r;
            Theta = theta;
        }

        /// <summary>
        /// Initialize polar point from cartesian point.
        /// </summary>
        /// <param name="p"></param>
        public PolarPoint2D(Vector2 p)
        {
            var temp = CartesianToPolar(p);
            R = temp.R;
            Theta = temp.Theta;
        }

        /// <summary>
        /// Obtain the cartesian representation of the polar point.
        /// </summary>
        public Vector2 Cartesian
        {
            get { return PolarToCartesian(this); }
        }

        /// <summary>
        /// Returns a ray shooting from the origin towards the polar point.
        /// </summary>
        public Ray2D Ray
        {
            get
            {
                return new Ray2D(Vector2.zero, MathUtil.Rotate(new Vector2(1, 0), Theta));
            }
        }

        /// <summary>
        /// Transforms a cartesian point to polar.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static PolarPoint2D CartesianToPolar(Vector2 p)
        {
            var r = Mathf.Sqrt(p.x * p.x + p.y * p.y);
            var theta = Mathf.Atan2(p.y, p.x);

            return new PolarPoint2D(r, theta);
        }

        /// <summary>
        /// Transforms a polar point to cartesian.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Vector2 PolarToCartesian(PolarPoint2D p)
        {
            var x = Math.Cos(p.Theta) * p.R;
            var y = Math.Sin(p.Theta) * p.R;

            return new Vector2((float)x, (float)y);
        }

        /// <summary>
        /// Check whether the polar point is the origin.
        /// </summary>
        /// <returns></returns>
        public bool IsOrigin()
        {
            return R == 0;
        }

        /// <summary>
        /// Rotate the point with a given angle theta in radians.
        /// Normalizes the angle to be between [0, 2 * PI]
        /// </summary>
        /// <param name="theta"></param>
        public void RotateClockWise(double theta)
        {
            Theta = MathUtil.PositiveMod(Theta - theta, MathUtil.PI2);
        }

        /// <summary>
        /// compare based on angle, if angles equal then compare based on distance.
        /// true iff PolarPoints have equal r and theta
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool Equals(PolarPoint2D p)
        {
            return MathUtil.EqualsEps(R, p.R) && MathUtil.EqualsEps(Theta, p.Theta);
        }

        public override int GetHashCode()
        {
            return 53 * R.GetHashCode() + Theta.GetHashCode();
        }

        public override string ToString()
        {
            return "[" + R + ", " + Theta + "]";
        }
    }
}
