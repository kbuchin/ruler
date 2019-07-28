namespace Util.Geometry.Algorithms
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry.Graph;

    public static class Matching
    {

        //TODO Currently implements greedy, improve on this
        public static IGraph MinimumWeightPerfectMatchingOfCompleteGraph(List<Vertex> vertices)
        {
            if (vertices.Count % 2 == 1)
            {
                throw new GeomException("odd number of vertices, perfect matching impossible");
            }

            //first determine the possible edges and sort them on distance
            var result = new AdjacencyListGraph(vertices);
            result.MakeComplete();
            var edges = (List<Edge>) result.Edges;
            edges.Sort();

            //initilize dictiornary
            var matched = new Dictionary<Vertex, bool>();
            foreach (var v in result.Vertices)
            {
                matched.Add(v, false);
            }

            //check edges 
            var edgesToRemove = new List<Edge>();
            foreach (var edge in edges)
            {
                var v1Matched = matched[edge.Start];
                var v2Matched = matched[edge.End];

                if (!v1Matched && !v2Matched)
                {
                    //keep edge
                    matched[edge.Start] = matched[edge.End] = true;
                }
                else
                {
                    //remove edge
                    edgesToRemove.Add(edge);
                }
            }
            foreach (var edge in edgesToRemove)
            {
                result.RemoveEdge(edge);
            }

            //test degree
            foreach (var v in result.Vertices)
            {
                if (result.DegreeOf(v) != 1)
                {
                    throw new GeomException("We have not arrived at a matching");
                }
            }

            return result;
        }
    }
}
