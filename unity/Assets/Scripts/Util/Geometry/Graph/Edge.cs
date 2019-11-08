namespace Util.Geometry.Graph
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Simple edge class for graphs embedded in the plane,
    /// meaning the edge is defined between two vertices which are mapped to points.
    /// The edge carries a weight, which by default is equal to its length.
    /// </summary>
    public class Edge : IComparable<Edge>, IEquatable<Edge>
    {
        public Vertex Start { get; private set; }
        public Vertex End { get; private set; }

        /// <summary>
        /// Length of the edge
        /// </summary>
        public float Length { get { return Vector2.Distance(Start.Pos, End.Pos); } }

        /// <summary>
        /// Weight of the edge.
        /// By default its length unless specified otherwise.
        /// </summary>
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

        /// <summary>
        /// Edge twin used for undirected graphs, 
        /// where each undirected edge is stored as two directed ones.
        /// </summary>
        public Edge Twin { get; set; }

        // stores whether the user gave a set weight
        // otherwise the length is used for weight
        private bool m_isWeightSet = false;

        // store the custom weight value
        private float m_weight;

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

        /// <summary>
        /// Check whether the given vertex is equal to one of the endpoints.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
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

