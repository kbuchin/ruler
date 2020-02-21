using System;
using UnityEngine;

namespace Util.Geometry
{
    /// <summary>
    /// An alternative implementation of <see cref="Vector2"/> to support doubles instead of floats. Only functionality
    /// required by one of the algorithms using this class is implemented.
    /// </summary>
    public class Vector2D : IEquatable<Vector2D>
    {
        /// <summary>
        ///   <para>X component of the vector.</para>
        /// </summary>
        public double x;

        /// <summary>
        ///   <para>Y component of the vector.</para>
        /// </summary>
        public double y;

        /// <summary>
        ///   <para>Constructs a new vector with given x, y components.</para>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Vector2D(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2D(Vector2 vector)
        {
            this.x = vector.x;
            this.y = vector.y;
        }

        public Vector2 Vector2
        {
            get { return new Vector2((float)x, (float)y); }
        }

        public static Vector2D operator -(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.x - b.x, a.y - b.y);
        }

        public double Cross(Vector2D other)
        {
            return Cross(this, other);
        }

        public static double Cross(Vector2D a, Vector2D b)
        {
            return (a.x * b.y) - (a.y * b.x);
        }

        public double Dot(Vector2D other)
        {
            return Dot(this, other);
        }

        public static double Dot(Vector2D a, Vector2D b)
        {
            return a.x * b.x + a.y * b.y;
        }

        public Vector2D Interpolate(double tau, Vector2D to)
        {
            return Interpolate(this, tau, to);
        }

        public static Vector2D Interpolate(Vector2D start, double tau, Vector2D end)
        {
            return new Vector2D(start.x + tau * end.x, start.y + tau * end.y);
        }

        public override string ToString()
        {
            return string.Format("({0:F1}, {1:F1})", this.x, this.y);
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode() << 2;
        }

        /// <summary>
        ///   <para>Returns true if the given vector is exactly equal to this vector.</para>
        /// </summary>
        /// <param name="other"></param>
        public override bool Equals(object other)
        {
            var other1 = other as Vector2D;
            if (other1 == null)
            {
                return false;
            }

            return this.Equals(other1);
        }

        public bool Equals(Vector2D other)
        {
            return this.x.Equals(other.x) && this.y.Equals(other.y);
        }
    }
}