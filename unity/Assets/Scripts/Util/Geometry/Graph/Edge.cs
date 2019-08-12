namespace Util.Geometry.Graph
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Edge : IComparable<Edge>, IEquatable<Edge>
    {
        private bool m_isWeightSet = false;
        private float m_weight;

        public Vertex Start { get; private set; }

        public Vertex End { get; private set; }

        public float Length { get { return (End.Pos - Start.Pos).magnitude; } }

        public float Weight
        {
            get
            {
                return (m_isWeightSet ? m_weight : Length);
            }
            set
            {
                m_isWeightSet = true;
                m_weight = value;
            }
        }

        internal Edge Twin { get; set; }

        public Edge(Vertex start, Vertex end)
        {
            Start = start;
            End = end;
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

        public override int GetHashCode()
        {
            return 37 * Start.GetHashCode() + 13 * End.GetHashCode() + 59 * Weight.GetHashCode();
        }

        public bool Equals(Edge e)
        {
            return Start.Equals(e.Start) && End.Equals(e.End) && Weight.Equals(e.Weight);
        }

        public int CompareTo(Edge e)
        {
            return Weight.CompareTo(e.Weight);
        }
    }
}

