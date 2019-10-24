namespace Util.Geometry.Graph
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Math;

    public class Vertex : IEquatable<Vertex>
    {
        public Vector2 Pos { get; private set; }

        public Vertex() : this(new Vector2())
        { }

        public Vertex(float x, float y) : this(new Vector2(x, y))
        { }

        public Vertex(Vector2 p)
        {
            Pos = p;
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