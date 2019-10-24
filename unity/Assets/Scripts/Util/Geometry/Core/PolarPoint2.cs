namespace Util.Geometry.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Math;

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

        // initlialize polar point from cartesian
        public PolarPoint2D(Vector2 p)
        {
            var temp = CartesianToPolar(p);
            R = temp.R;
            Theta = temp.Theta;
        }

        public Vector2 Cartesian
        {
            get { return PolarToCartesian(this); }
        }

        public Ray2D Ray
        {
            get
            {
                return new Ray2D(Vector2.zero, MathUtil.Rotate(new Vector2(1, 0), Theta));
            }
        }

        private PolarPoint2D CartesianToPolar(Vector2 p)
        {
            var r = Mathf.Sqrt(p.x * p.x + p.y * p.y);
            var theta = Mathf.Atan2(p.y, p.x);

            return new PolarPoint2D(r, theta);
        }

        private Vector2 PolarToCartesian(PolarPoint2D p)
        {
            var x = Mathf.Cos(p.Theta) * p.R;
            var y = Mathf.Sin(p.Theta) * p.R;

            return new Vector2(x, y);
        }

        public bool IsOrigin()
        {
            return R == 0;
        }

        public void RotateClockWise(float theta)
        {
            this.Theta -= theta;
            Normalize(2 * Mathf.PI);
        }

        // keeps theta in [0, period]
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
