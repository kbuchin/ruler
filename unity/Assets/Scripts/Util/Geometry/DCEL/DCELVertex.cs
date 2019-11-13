namespace Util.Geometry.DCEL
{
    using System;
    using UnityEngine;
    using Util.Math;

    /// <summary>
    /// Simple class similar to a normal graph vertex.
    /// Now also stores one additional leaving half edge for easy iteration.
    /// Use DCEL methods for iterating through adjacent edges.
    /// </summary>
    public class DCELVertex : IEquatable<DCELVertex>
    {
        /// <summary>
        /// Position vector of the vertex.
        /// </summary>
        public Vector2 Pos { get; private set; }

        /// <summary>
        /// Halfedge pointing away from the vertex.
        /// Can be null.
        /// </summary>
        public HalfEdge Leaving { get; set; }

        public DCELVertex()
        {
            Pos = Vector2.zero;
        }

        public DCELVertex(float x, float y)
        {
            Pos = new Vector2(x, y);
        }

        public DCELVertex(Vector2 p)
        {
            Pos = new Vector2(p.x, p.y);
        }

        public DCELVertex(Vector2 pos, HalfEdge e) : this(pos)
        {
            Leaving = e;
        }

        public bool Equals(DCELVertex e)
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
