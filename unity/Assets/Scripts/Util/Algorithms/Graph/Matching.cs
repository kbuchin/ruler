namespace Util.Algorithms.Graph
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
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
        /// <remarks>
        /// Currently implements greedy strategy, can be improved
        /// </remarks>
        /// <param name="vertices"></param>
        /// <returns>Matching graph</returns>
        public static IGraph MinimumWeightPerfectMatchingOfCompleteGraph(List<Vertex> vertices)
        {
            if (vertices.Count % 2 == 1)
            {
                throw new GeomException("odd number of vertices, perfect matching impossible");
            }

            // first, create a complete graph of the vertices
            var result = new AdjacencyListGraph(vertices);
            result.MakeComplete();

            // determine the possible edges and sort them on distance
            var edges = result.Edges.ToList();
            edges.Sort();       // edges are by default compared on weight

            //initilize dictionary
            var matched = new Dictionary<Vertex, bool>();
            foreach (var v in result.Vertices)
            {
                matched.Add(v, false);
            }

            // check edges 
            var edgesToRemove = new List<Edge>();
            foreach (var edge in edges)
            {
                if (!matched[edge.Start] && !matched[edge.End])
                {
                    // keep edge
                    matched[edge.Start] = matched[edge.End] = true;
                }
                else
                {
                    // remove edge
                    edgesToRemove.Add(edge);
                }
            }
            foreach (var edge in edgesToRemove)
            {
                result.RemoveEdge(edge);
            }

            return result;
        }
    }
}
