namespace Util.Geometry.Triangulation
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry.Graph;

    public class TriangleEdge : Edge
    {
        public new TriangleEdge Twin { get; set; }
        public Triangle T { get; set; }

        public TriangleEdge(Vertex start, Vertex end, TriangleEdge twin, Triangle t) : base(start, end)
        {
            Twin = Twin;
            T = t;
        }

        public Triangle OtherTriangle()
        {
            if(Twin == null || Twin.T == null)
            {
                throw new GeomException("Neighbouring triangle of TriangleEdge not defined");
            }

            return Twin.T;
        }
    }
}
