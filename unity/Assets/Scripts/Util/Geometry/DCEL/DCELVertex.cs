namespace Util.Geometry.DCEL
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry.Graph;

    public class DCELVertex : Vertex
    {
        public HalfEdge Leaving { get; set; }

        public DCELVertex(Vertex v) : base(v.Pos)
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
