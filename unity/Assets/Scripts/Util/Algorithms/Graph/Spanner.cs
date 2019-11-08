namespace Util.Algorithms.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Util.Geometry.Graph;

    /// <summary>
    /// Static collection of algorithms related to t-spanners.
    /// </summary>
    public static class Spanner
    {
        /// <summary>
        /// Verifies if the given graph is a t-spanner for the given ratio a_t.
        /// </summary>
        /// <param name="a_graph"></param>
        /// <param name="a_t"></param>
        /// <returns>A vertification struct which holds the ratio and possibly a falsification pair. </returns>
        public static SpannerVerification VerifySpanner(IGraph a_graph, float a_t)
        {
            //first determine the possible edges
            var completeGraph = new AdjacencyListGraph(new List<Vertex>(a_graph.Vertices));
            completeGraph.MakeComplete();
            var edges = completeGraph.Edges.ToList();
            edges.Sort();

            Vertex Start = null;
            Vertex End = null;
            var ratio = 1f; // best possible ratio

            foreach (var edge in edges)
            {
                // find distance in given graph
                // TODO all-pair shortest path
                var dist = ShortestPath.ShorthestDistance(a_graph, edge.Start, edge.End);

                // compare ratios
                float edgeratio = dist / edge.Weight;
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
        /// <param name="a_vertices"> the vertices on which we want to construct a t-spanner</param>
        /// <param name="t"> parameter t in the definition of t-spanner. Each pair of vertices should have a path
        ///  of at most length t*eucledian distance</param>
        /// <returns></returns>
        public static IGraph GreedySpanner(List<Vertex> a_vertices, float a_t)
        {
            var completeGraph = new AdjacencyListGraph(a_vertices);
            completeGraph.MakeComplete();

            // return t-spanner of complete graph
            return GreedySpanner(completeGraph, a_t);
        }

        /// <summary>
        /// Creates a t-spanner using a greedy algorithm trying the shortest edges first.
        /// </summary>
        /// <param name="a_graph"> the graph on which we want to construct a t-spanner</param>
        /// <param name="a_t"> parameter t in the definition of t-spanner. Each pair of vertices should have a path
        ///  of at most length t*eucledian distance</param>
        /// <returns></returns>
        public static IGraph GreedySpanner(IGraph a_graph, float a_t)
        {
            var result = new AdjacencyListGraph(a_graph.Vertices);

            var edges = a_graph.Edges.ToList();
            edges.Sort();   // by default sorts on weight

            foreach (var edge in edges)
            {
                // add edge if t-spanner criteria not satisfied
                if (ShortestPath.ShorthestDistance(result, edge.Start, edge.End) > a_t * edge.Weight)
                {
                    result.AddEdge(edge);
                }
            }
            return result;
        }
    }


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

        public SpannerVerification(Vertex a_start, Vertex a_end, float a_ratio)
        {
            if ((a_start == null && a_end != null) || (a_start != null && a_end == null))
                throw new ArgumentException("Either start and end of falsification path are null or both are not null.");

            IsSpanner = a_start == null;
            FalsificationStart = a_start;
            FalsificationEnd = a_end;
            Ratio = a_ratio;
        }
    }
}

