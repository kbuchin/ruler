namespace Util.Geometry.Graph
{
    using System;
    using UnityEngine;
    using Util.Math;

    /// <summary>
    /// Simple vertex class that encapsulates a Vector2 point.
    /// </summary>
    public class Vertex : IEquatable<Vertex>
    {
        public Vector2 Pos { get; private set; }

        public Vertex()
        {
            Pos = Vector2.zero;
        }

        public Vertex(float x, float y)
        {
            Pos = new Vector2(x, y);
        }

        public Vertex(Vector2 p)
        {
            Pos = new Vector2(p.x, p.y);
        }

        public bool Equals(Vertex e)
        {
            return MathUtil.EqualsEps(Pos, e.Pos);
        }

        public override string ToString()
        {
            return Pos.ToString();
        }

        public override int GetHashCode()
        {
            return Pos.GetHashCode();
        }
    }
}