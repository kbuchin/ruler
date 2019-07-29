namespace Util.Geometry.Graph
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Edge : IComparer<Edge>, IEquatable<Edge>
    {
        public Vertex Start { get; private set; }

        public Vertex End { get; private set; }

        public float Length { get { return (End.Pos - Start.Pos).magnitude; } }

        public float Weight { get; set; }

        internal Edge Twin { get; set; }

        public Edge(Vertex start, Vertex end)
        {
            Start = start;
            End = end;
            Weight = Length;
        }

        public Edge(Vertex start, Vertex end, float weight)
        {
            Start = start;
            End = end;
            Weight = weight;
        }

        public bool ContainsEndpoint(Vertex v)
        {
            return Start == v || End == v;
        }

        public override string ToString()
        {
            return "(" + Start + ", " + End + ")";
        }

        public bool Equals(Edge e)
        {
            return Start.Equals(e.Start) && End.Equals(e.End) && Weight.Equals(e.Weight);
        }

        public int Compare(Edge x, Edge y)
        {
            return x.Weight.CompareTo(y.Weight);
        }
    }
}

