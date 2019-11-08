namespace Util.Algorithms.Graph
{
    using System.Collections.Generic;
    using System.Linq;
    using Util.Geometry;
    using Util.Geometry.Graph;

    /// <summary>
    /// Static class for implementation of graph matching algorithms
    /// </summary>
    public static class Matching
    {
        /// <summary>
        /// Tries to find a perfect matching with minimum weight of the given vertices, where the vertices form a complete graph
        /// </summary>
        /// <param name="a_vertices"></param>
        /// <returns>Collection of matching edges.</returns>
        public static IEnumerable<Edge> MinimumWeightPerfectMatching(List<Vertex> a_vertices)
        {
            // first, create a complete graph of the vertices
            var result = new AdjacencyListGraph(a_vertices);
            result.MakeComplete();

            return GreedyMinimumWeightPerfectMatching(result);
        }

        /// <summary>
        /// Tries to find a perfect matching with minimum weight of the given graph
        /// </summary>
        /// <remarks>
        /// Implements a greedy strategy, where edges are sorted on weight
        /// </remarks>
        /// <param name="a_result"></param>
        /// <returns></returns>
        public static IEnumerable<Edge> GreedyMinimumWeightPerfectMatching(IGraph a_result)
        {
            if (a_result.VertexCount % 2 == 1)
            {
                throw new GeomException("odd number of vertices, perfect matching impossible");
            }

            // determine the possible edges and sort them on distance
            var edges = a_result.Edges.ToList();
            edges.Sort();       // edges are by default compared on weight

            //initilize dictionary
            var matched = new Dictionary<Vertex, bool>();
            foreach (var v in a_result.Vertices)
            {
                matched.Add(v, false);
            }

            // check edges 
            var matching = new List<Edge>();
            foreach (var edge in edges)
            {
                if (!matched[edge.Start] && !matched[edge.End])
                {
                    // keep edge
                    matched[edge.Start] = matched[edge.End] = true;
                    matching.Add(edge);
                }
            }

            return matching;
        }
    }
}
