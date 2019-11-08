namespace Util.Algorithms.Graph
{
    using System.Collections.Generic;
    using System.Linq;
    using Util.DataStructures.Queue;
    using Util.Geometry;
    using Util.Geometry.Graph;

    /// <summary>
    /// Static collection of algorithms related to minimum spanning trees (MST).
    /// </summary>
    public static class MST
    {
        /// <summary>
        /// Creates a miminum spanning tree of the given list of vertices, where the vertices form a complete graph.
        /// </summary>
        /// <param name="a_vertices"></param>
        /// <returns>A graph representing the minimum spanning tree</returns>
        public static IGraph MinimumSpanningTree(IEnumerable<Vertex> a_vertices)
        {
            var graph = new AdjacencyListGraph(a_vertices);
            graph.MakeComplete();

            return MinimumSpanningTree(graph);
        }

        /// <summary>
        /// Finds a minimum spanning tree of the given graph using Prim's algorithm.
        /// Only uses edges that are present in the given graph.
        /// </summary>
        /// <param name="a_graph"></param>
        /// <returns>A graph representing the minimum spanning tree</returns>
        public static IGraph MinimumSpanningTree(IGraph a_graph)
        {
            if (a_graph.Type.DIRECTED)
            {
                throw new GeomException("Minimum Spanning Tree is not defined on a directed graph.");
            }

            var mst = new AdjacencyListGraph(a_graph.Vertices);

            // do nothing with zero or one verticess
            if (a_graph.VertexCount < 2) return mst;

            //choose arbitrary starting vertex
            var root = a_graph.Vertices.FirstOrDefault();

            //initialize data structures
            var visitedVertices = new HashSet<Vertex>() { root };
            var edgesToConsider = new BinaryHeap<Edge>(a_graph.OutEdgesOf(root));

            while (visitedVertices.Count < a_graph.VertexCount)
            {
                var edge = edgesToConsider.Pop();

                if (!visitedVertices.Contains(edge.Start))
                {
                    // should be impossible
                    throw new GeomException("Start vertex of edge has not been visited");
                }

                if (!visitedVertices.Contains(edge.End))
                {
                    // add edge
                    mst.AddEdge(edge);
                    visitedVertices.Add(edge.End);

                    // consider new edges from endpoint
                    foreach (Edge newedge in a_graph.OutEdgesOf(edge.End))
                    {
                        if (!visitedVertices.Contains(newedge.End))
                        {
                            edgesToConsider.Push(newedge);
                        }
                    }
                }
            }

            return mst;
        }
    }
}

