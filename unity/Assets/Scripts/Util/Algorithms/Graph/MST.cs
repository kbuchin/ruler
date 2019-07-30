namespace Util.Algorithms
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.DataStructures.Queue;
    using Util.Geometry.Graph;
    using Util.Geometry;

    public static class MST {
        public static void MinimumSpanningTree(IGraph Graph)
        {
            if(Graph.Type.DIRECTED)
            {
                throw new GeomException("Minimum Spanning Tree is not defined on a directed graph.");
            }
            
            // check to see if graph contains any vertices at all
            if (Graph.Vertices.Count <= 0) return;
            
            //choose arbitrary starting vertex
            var root = Graph.Vertices.GetEnumerator().Current;

            //initialize data structures
            var visitedVertices = new HashSet<Vertex>() { root };
            var edgesToConsider = new BinaryHeap<Edge>(Graph.EdgesOf(root));
            var edgesToRemove = new List<Edge>(Graph.Edges); //shallow copy

            while (visitedVertices.Count < Graph.Vertices.Count)
            {
                var edge = edgesToConsider.Pop();
                var v1visited = visitedVertices.Contains(edge.Start);
                var v2visited = visitedVertices.Contains(edge.End);

                if (v1visited && v2visited)
                {
                    continue;
                }
                else if (v1visited)
                {
                    //Keep edge
                    edgesToRemove.Remove(edge);
                    visitedVertices.Add(edge.End);
                    foreach (Edge newedge in Graph.EdgesOf(edge.End))
                    {
                        if (visitedVertices.Contains(newedge.Start) && visitedVertices.Contains(newedge.End))
                        {
                            continue;
                        }
                        edgesToConsider.Push(newedge);
                    }
                    continue;
                }
                else if (v2visited)
                {
                    //keep Edge
                    edgesToRemove.Remove(edge);
                    visitedVertices.Add(edge.Start);
                    foreach (Edge newedge in Graph.EdgesOf(edge.Start))
                    {
                        if (visitedVertices.Contains(newedge.Start) && visitedVertices.Contains(newedge.End))
                        {
                            continue;
                        }
                        edgesToConsider.Push(newedge);
                    }
                    continue;
                }
                else
                {
                    throw new GeomException("Both v1 and v2 are not visited");
                }
            }

            //update graph
            foreach (var edge in edgesToRemove)
            {
                Graph.RemoveEdge(edge);
            }
        }
    }
}
   
