namespace Util.Geometry.Algorithms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.DataStructures.Queue;
    using Util.Geometry.Graph;

    struct VertexDist : IComparable<VertexDist>
    {
        public Vertex V { get; private set; }
        public float Dis { get; private set; }

        public VertexDist(Vertex a_V, float a_Dis)
        {
            this.V = a_V;
            this.Dis = a_Dis;
        }

        public int CompareTo(VertexDist obj)
        {
            return Dis.CompareTo(obj.Dis);
        }
    }

    public static class ShortestPath {

        // auxilliary variables used in the dijkstra algorithm
        private readonly static Dictionary<Vertex, VertexDist> ParentDist = new Dictionary<Vertex, VertexDist>();

        /// <summary>
        /// Runs Dijkstra's algorithm on the given Graph from the Start vertex.
        /// Stops when End vertex is explored.
        /// Stores distance and parent information in the ParentDist variable.
        /// </summary>
        /// <param name="Graph"></param>
        /// <param name="Start"></param>
        /// <param name="End"></param>
        private static void Dijkstra(IGraph Graph, Vertex Start, Vertex End)
        {
            ParentDist.Clear();
            var Queue = new BinaryHeap<VertexDist>();

            foreach (Vertex v in Graph.Vertices)
            {
                ParentDist.Add(v, new VertexDist(v, float.PositiveInfinity));
            }

            //set start vertex O(n)
            Queue.Push(new VertexDist(Start, 0));
            ParentDist[Start] = new VertexDist(Start, 0f);

            while (Queue.Count > 0)
            {
                //get cheapest vertex O( n log n) in total
                var distance = Queue.Peek().Dis;
                var currentVertex = Queue.Peek().V;
                Queue.Pop();

                if (ParentDist[currentVertex].Dis < distance) continue;

                //Check for termination
                if (currentVertex == End) return;

                //update neighbours O(m n) in total
                foreach (var edge in Graph.OutEdgesOf(currentVertex))
                {
                    var updateVertex = edge.End;
                    if (ParentDist[updateVertex].Dis < distance + edge.Length)
                    {
                        ParentDist[updateVertex] = new VertexDist(currentVertex, distance + edge.Length);
                        Queue.Push(new VertexDist(updateVertex, distance + edge.Length));
                    }
                }
            }
        }

        /// <summary>
        /// Finds the length of the shorthest path betweeen two points.
        /// </summary>
        /// <remarks>
        /// Implements dijkstra's algorithm in O(m n +  n log n) time.
        /// Does not update nodes in priority Queue.
        /// </remarks>
        /// <param name="Start"></param>
        /// <param name="End"></param>
        /// <returns>Shortest distance between Start and End.</returns>
        public static float ShorthestDistance(IGraph Graph, Vertex Start, Vertex End)
        {
            if (!(Graph.ContainsVertex(Start) && Graph.ContainsVertex(End)))
            {
                throw new GeomException("Graph does not contain both start and end vertices.");
            }

            // run dijkstra search until End found
            Dijkstra(Graph, Start, End);

            // return distance to end found (possibly infinite)
            return ParentDist[End].Dis;
        }

        /// <summary>
        /// Returns a shortest path from Start to End as an Enumerable.
        /// </summary>
        /// <remarks>
        /// Implements dijkstra's algorithm in O(m n +  n log n) time.
        /// Does not update nodes in priority Queue.
        /// </remarks>
        /// <param name="Graph"></param>
        /// <param name="Start"></param>
        /// <param name="End"></param>
        /// <returns>Shortest path between Start and End.</returns>
        public static IEnumerable<Vertex> ShorthestPath(IGraph Graph, Vertex Start, Vertex End)
        {
            if (!(Graph.ContainsVertex(Start) && Graph.ContainsVertex(End)))
            {
                throw new GeomException("Graph does not contain both start and end vertices.");
            }

            // run dijkstra search until End found
            Dijkstra(Graph, Start, End);

            var path = new LinkedList<Vertex>();
            var cur = End;

            // iterate back towards Start and construct path
            path.AddFirst(cur);
            while(cur != Start)
            {
                cur = ParentDist[cur].V;
                path.AddFirst(cur);
            }

            return path;
        }
    }
}
