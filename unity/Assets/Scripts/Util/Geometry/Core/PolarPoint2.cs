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
        public float R { get; set; } // distance to p
        public float Theta { get; set; } // angle with respect to p

        // polar point from distance and angle (in radians)
        public PolarPoint2D(float r, float theta)
        {
            this.R = r;
            this.Theta = theta;
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
            var x = Mathf.Cos(p.Theta) * p.R;
            var y = Mathf.Sin(p.Theta) * p.R;

            return new Vector2(x, y);
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
        public void RotateClockWise(float theta)
        {
            this.Theta -= theta;
            Normalize(2 * Mathf.PI);
        }

        /// <summary>
        /// Keeps theta in [0, period].
        /// </summary>
        /// <param name="period"></param>
        public void Normalize(float period)
        {
            while (Theta <= 0.0)
                Theta += period;

            while (Theta >= period)
                Theta -= period;
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
            int hash = 3;
            hash = 53 * hash + R.GetHashCode() + Theta.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return "[" + R + ", " + Theta + "]";
        }
    }
}
