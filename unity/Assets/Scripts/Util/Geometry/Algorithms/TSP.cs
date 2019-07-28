﻿namespace Util.Geometry.Algorithms
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry.Graph;

    public static class TSP {

        /// <summary>
        /// Check if the current graph stores a TSP tour
        /// </summary>
        /// <param name="Graph"></param>
        /// <returns>whether the graph represents a TSP tour</returns>
        public static bool IsHamiltonian(IGraph Graph)
        {
            // fewer than 2 vertices is a degenerate case, always a TSP tour
            if (Graph.Vertices.Count <= 1) return true;

            // start with random vertex
            Vertex v = Graph.Vertices.GetEnumerator().Current;
            var visited = new HashSet<Vertex>();

            while(true)
            {
                // each vertex should have degree 2
                if (Graph.DegreeOf(v) != 2) return false;

                visited.Add(v);

                // find a valid edge from the two
                bool change = false;
                foreach (Edge e in Graph.OutEdgesOf(v))
                {
                    if(!change && !visited.Contains(e.End))
                    {
                        v = e.End;
                        change = true;
                    }
                }

                // if no valid edge found, path is terminated
                if (!change) break;
            }

            // check whether all vertices found
            return visited.Count == Graph.Vertices.Count;
        }

        /// <summary>
        /// Finds the lenth of a TSPtour, provided by christofides algorithm.
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static float FindTSPLength(List<Vertex> vertices)
        {
            //first determine a MST
            var mst = new AdjacencyListGraph(vertices);
            mst.MakeComplete();
            MST.MinimumSpanningTree(mst);

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
            var oddDegreeMatching = Matching.MinimumWeightPerfectMatchingOfCompleteGraph(oddDegreePos);

            Debug.Log("mst length: " + mst.totalEdgeWeight + "  om length: " + oddDegreeMatching.totalEdgeWeight);
            return mst.totalEdgeWeight + oddDegreeMatching.totalEdgeWeight;
        }
    }
}