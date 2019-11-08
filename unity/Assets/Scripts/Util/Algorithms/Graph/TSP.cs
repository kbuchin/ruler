namespace Util.Algorithms.Graph
{
    using System.Collections.Generic;
    using System.Linq;
    using Util.Geometry;
    using Util.Geometry.Graph;

    /// <summary>
    /// Collection of algorithms related to TSP and Hamiltonicity.
    /// </summary>
    public static class TSP
    {

        /// <summary>
        /// Check if the current graph stores a TSP tour
        /// </summary>
        /// <param name="Graph"></param>
        /// <returns>whether the graph represents a TSP tour</returns>
        public static bool IsHamiltonian(IGraph Graph)
        {
            // fewer than 2 vertices is a degenerate case, always a TSP tour
            if (Graph.VertexCount <= 1) return true;

            // start with random vertex
            Vertex v = Graph.Vertices.FirstOrDefault();
            var visited = new HashSet<Vertex>();

            while (true)
            {
                // each vertex should have degree 2
                if (Graph.DegreeOf(v) != 2) return false;

                visited.Add(v);

                // find a valid edge from the two
                bool change = false;
                foreach (Edge e in Graph.OutEdgesOf(v))
                {
                    if (!change && !visited.Contains(e.End))
                    {
                        v = e.End;
                        change = true;
                    }
                }

                // if no valid edge found, path is terminated
                if (!change) break;
            }

            // check whether all vertices found
            return visited.Count == Graph.VertexCount;
        }

        /// <summary>
        /// Compute the length of the TSP tour in the graph.
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        public static float ComputeTSPLength(IGraph graph)
        {
            if (!IsHamiltonian(graph))
            {
                throw new GeomException("Graph should be hamiltonian");
            }
            return graph.TotalEdgeWeight;
        }

        /// <summary>
        /// Finds the lenth of a TSPtour, provided by christofides algorithm.
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static float FindTSPLength(IEnumerable<Vertex> vertices)
        {
            //first determine a MST
            var mst = MST.MinimumSpanningTree(vertices);

            //find odd degree vertices
            var oddDegreePos = new List<Vertex>();
            foreach (var vertex in mst.Vertices)
            {
                if (mst.DegreeOf(vertex) % 2 == 1)
                {
                    oddDegreePos.Add(vertex);
                }
            }

            //find minimum weight perfect matcing
            var oddDegreeMatching = Matching.MinimumWeightPerfectMatching(oddDegreePos);
            var matchWeight = oddDegreeMatching.Sum(e => e.Weight);

            return mst.TotalEdgeWeight + matchWeight;
        }
    }
}
