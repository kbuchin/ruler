namespace Util.Algorithms.Graph
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.DataStructures.Queue;
    using Util.Geometry.Graph;
    using Util.Geometry;
    using System;
    using System.Linq;

    public static class MST
    {
        /// <summary>
        /// Creates a miminum spanning tree of the given list of vertices
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns>A graph representing the minimum spanning tree</returns>
        public static IGraph MinimumSpanningTree(IEnumerable<Vertex> vertices)
        {
            var graph = new AdjacencyListGraph(vertices);
            graph.MakeComplete();
            return MinimumSpanningTree(graph);
        }

        /// <summary>
        /// Finds a minimum spanning tree of the given graph.
        /// Only uses edges that are present in the given graph.
        /// </summary>
        /// <param name="Graph"></param>
        /// <returns>A graph representing the minimum spanning tree</returns>
        public static IGraph MinimumSpanningTree(IGraph Graph)
        {
            if(Graph.Type.DIRECTED)
            {
                throw new GeomException("Minimum Spanning Tree is not defined on a directed graph.");
            }

            var mst = new AdjacencyListGraph(Graph.Vertices);

            // do nothing with zero or one verticess
            if (Graph.VertexCount < 2) return mst;

            //choose arbitrary starting vertex
            var root = Graph.Vertices.First();

            //initialize data structures
            var visitedVertices = new HashSet<Vertex>() { root };
            var edgesToConsider = new BinaryHeap<Edge>( Graph.OutEdgesOf(root) );

            while (visitedVertices.Count < Graph.VertexCount)
            {
                //Debug.Log("go");
                var edge = edgesToConsider.Pop();

                if(!visitedVertices.Contains(edge.Start))
                {
                    // should be impossible
                    throw new GeomException("Start vertex of edge has not been visited");
                }

                if (!visitedVertices.Contains(edge.End))
                {
                    // add edge
                    mst.AddEdge(edge);
                    visitedVertices.Add(edge.End);
                    foreach (Edge newedge in Graph.OutEdgesOf(edge.End))
                    {
                        if (visitedVertices.Contains(newedge.End))
                        {
                            continue;
                        }
                        edgesToConsider.Push(newedge);
                    }
                }
            }

            return mst;
        }
    }
}
   
