namespace Util.Algorithms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry.Graph;

    /// <summary>
    /// Contains three fields, together indicating whether a graph is a spanner. It is gauranteed FalsificationPathsStart and 
    /// FalsificationPathsEnd are nonempty when IsSpanner is false. Furthermore FalsificationPathsStart and  FalsificationPathsEnd are empty when
    /// IsSpanner is true.
    /// </summary>
    public struct SpannerVerification
    {
        /// <summary>
        /// Whether the Graph that created this SpannerVerfication is a spanner
        /// </summary>
        public readonly bool IsSpanner;
        /// <summary>
        /// Startpoint of a falsifcation path (i.e. the length of shorthest path between these two vertices is longer then t times the euclidian distance)
        /// </summary>
        public readonly Vertex FalsificationStart;
        /// <summary>
        /// Endpoint of a falsifcation path (i.e. the length of shorthest path between these two vertices is longer then t times the euclidian distance)
        /// </summary>
        public readonly Vertex FalsificationEnd;
        /// <summary>
        /// The ratio of the current spanner.
        /// </summary>
        public readonly float Ratio;

        internal SpannerVerification(Vertex a_start, Vertex a_end, float a_ratio)
        {
            if ((a_start == null && a_end != null) || (a_start != null && a_end == null))
                throw new ArgumentException("Either start and end of falsification path are null or both are not null.");

            IsSpanner = a_start == null;
            FalsificationStart = a_start;
            FalsificationEnd = a_end;
            Ratio = a_ratio;
        }
    }

    public static class Spanner  {
        public static SpannerVerification VerifySpanner(IGraph Graph, float a_t)
        {
            //first determine the possible edges
            var completeGraph = new AdjacencyListGraph(new List<Vertex>(Graph.Vertices));
            completeGraph.MakeComplete();
            List<Edge> edges = (List<Edge>) completeGraph.Edges;
            edges.Sort();

            Vertex Start = null;
            Vertex End = null;
            var ratio = 1f; // best possible ratio

            foreach (var edge in edges)
            {
                var dist = ShortestPath.ShorthestDistance(Graph, edge.Start, edge.End);
                if (dist == float.PositiveInfinity)
                    throw new GeomException("Path does not exist in graph");

                float edgeratio = dist / edge.Length;  // TODO all-pairs shortest path
                if (edgeratio > a_t)
                {
                    Start = edge.Start;
                    End = edge.End;
                }
                if (ratio <= edgeratio)
                {
                    ratio = edgeratio;
                }
            }
            return new SpannerVerification(Start, End, ratio);
        }

        /// <summary>
        /// Creates a t-spanner using a greedy algorithm trying the shortest edges first.
        /// </summary>
        /// <param name="vertices"> the vertices on which we want to construct a t-spanner</param>
        /// <param name="t"> parameter t in the definition of t-spanner. Each pair of vertices should have a path
        ///  of at most length t*eucledian distance</param>
        /// <returns></returns>
        public static IGraph GreedySpanner(List<Vertex> vertices, float a_t)
        {
            var result = new AdjacencyListGraph(vertices);
            var completeGraph = new AdjacencyListGraph(vertices);
                
            completeGraph.MakeComplete();
            var edges = (List<Edge>) completeGraph.Edges;
            edges.Sort();

            foreach (var edge in edges)
            {
                var directDistance = edge.Length;

                if (ShortestPath.ShorthestDistance(result, edge.Start, edge.End) > a_t * directDistance)
                {
                    result.AddEdge(edge.Start, edge.End);
                }
            }
            return result;
        }
    }
}

