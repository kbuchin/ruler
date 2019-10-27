namespace Util.Geometry.DCEL
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry.Graph;

    /// <summary>
    /// Simple extension of a normal graph vertex.
    /// Now also stores one additional leaving half edge for easy iteration.
    /// Use DCEL methods for iterating through adjacent edges.
    /// </summary>
    public class DCELVertex : Vertex
    {
        public HalfEdge Leaving { get; set; }

        public DCELVertex(Vertex v) : base(v.Pos)
        { }

        public DCELVertex(float x, float y) : base(x, y)
        { }

        public DCELVertex(Vector2 pos) : base(pos)
        { }

        public DCELVertex(Vertex v, HalfEdge e) : this(v.Pos, e)
        { }

        public DCELVertex(Vector2 pos, HalfEdge e) : base(pos)
        {
            Leaving = e;
        }
    }
}
