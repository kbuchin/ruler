namespace Util.Geometry.Triangulation
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry.Graph;

    public class TriangleEdge
    {
        public Vector2 Start { get; private set; }

        public Vector2 End { get; private set; }

        public float Length { get { return (End - Start).magnitude; } }

        public TriangleEdge Twin { get; set; }
        public Triangle T { get; set; }

        public TriangleEdge(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
        }

        public TriangleEdge(Vector2 start, Vector2 end, TriangleEdge twin, Triangle t) : this(start, end)
        {
            Twin = Twin;
            T = t;
        }

        public bool ContainsEndpoint(Vector2 pos)
        {
            return Start == pos || End == pos;
        }

        public Triangle OtherTriangle()
        {
            if(Twin == null || Twin.T == null)
            {
                throw new GeomException("Neighbouring triangle of TriangleEdge not defined");
            }

            return Twin.T;
        }

        public override string ToString()
        {
            return "(" + Start + ", " + End + ")";
        }
    }
}
