namespace Util.Geometry.Graph
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Vertex : IEquatable<Vertex>
    {
        public Vector2 Pos { get; private set; }

        public Vertex() : this(new Vector2())
        { }

        public Vertex(Vector2 p)
        {
            Pos = p;
        }

        public bool Equals(Vertex e)
        {
            return Pos.Equals(e.Pos);
        }

        public override string ToString()
        {
            return Pos.ToString();
        }
    }
}
