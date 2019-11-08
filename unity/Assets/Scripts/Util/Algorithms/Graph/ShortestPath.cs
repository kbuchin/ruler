namespace Util.Algorithms.Graph
{
    using System;
    using System.Collections.Generic;
    using Util.DataStructures.Queue;
    using Util.Geometry;
    using Util.Geometry.Graph;

    /// <summary>
    /// Static collection of algorithms related to shortest path
    /// </summary>
    public static class ShortestPath
    {

        // auxilliary variables used in the dijkstra algorithm
        private readonly static Dictionary<Vertex, VertexDist> ParentDist = new Dictionary<Vertex, VertexDist>();

        /// <summary>
        /// Finds the length of the shorthest path betweeen two points.
        /// </summary>
        /// <remarks>
        /// Implements dijkstra's algorithm in O(m n +  n log n) time.
        /// </remarks>
        /// <param name="a_start"></param>
        /// <param name="a_end"></param>
        /// <returns>Shortest distance between Start and End.</returns>
        public static float ShorthestDistance(IGraph a_graph, Vertex a_start, Vertex a_end)
        {
            if (!(a_graph.ContainsVertex(a_start) && a_graph.ContainsVertex(a_end)))
            {
                throw new GeomException("Graph does not contain both start and end vertices.");
            }

            // run dijkstra search until End found
            Dijkstra(a_graph, a_start, a_end);

            // return distance to end found (possibly infinite)
            return ParentDist[a_end].Dis;
        }

        /// <summary>
        /// Returns a shortest path from Start to End as an Enumerable.
        /// </summary>
        /// <remarks>
        /// Implements dijkstra's algorithm in O(m n +  n log n) time.
        /// </remarks>
        /// <param name="a_graph"></param>
        /// <param name="a_start"></param>
        /// <param name="a_end"></param>
        /// <returns>Shortest path between Start and End.</returns>
        public static IEnumerable<Vertex> ShorthestPath(IGraph a_graph, Vertex a_start, Vertex a_end)
        {
            if (!(a_graph.ContainsVertex(a_start) && a_graph.ContainsVertex(a_end)))
            {
                throw new GeomException("Graph does not contain both start and end vertices.");
            }

            // run dijkstra search until End found
            Dijkstra(a_graph, a_start, a_end);

            var path = new LinkedList<Vertex>();
            var cur = a_end;

            // iterate back towards Start and construct path
            path.AddFirst(cur);
            while (cur != a_start)
            {
                cur = ParentDist[cur].V;
                path.AddFirst(cur);
            }

            return path;
        }

        /// <summary>
        /// Runs Dijkstra's algorithm on the given Graph from the Start vertex.
        /// Stops when End vertex is explored.
        /// Stores distance and parent information in the ParentDist variable.
        /// </summary>
        /// <param name="a_graph"></param>
        /// <param name="a_start"></param>
        /// <param name="a_end"></param>
        private static void Dijkstra(IGraph a_graph, Vertex a_start, Vertex a_end)
        {
            ParentDist.Clear();
            var queue = new BinaryHeap<VertexDist>();

            foreach (Vertex v in a_graph.Vertices)
            {
                ParentDist.Add(v, new VertexDist(v, float.PositiveInfinity));
            }

            //set start vertex O(n)
            queue.Push(new VertexDist(a_start, 0));
            ParentDist[a_start] = new VertexDist(a_start, 0f);

            while (queue.Count > 0)
            {
                //get cheapest vertex, O( n log n) in total
                var distance = queue.Peek().Dis;
                var currentVertex = queue.Peek().V;
                queue.Pop();

                //Check for termination
                if (currentVertex == a_end) return;

                // old node distance pair, better distance already found so disregard
                if (ParentDist[currentVertex].Dis < distance) continue;

                //update neighbours O(m n) in total
                foreach (var edge in a_graph.OutEdgesOf(currentVertex))
                {
                    if (ParentDist[edge.End].Dis > distance + edge.Length)
                    {
                        // insert new vertex-distance pair into priority queue
                        ParentDist[edge.End] = new VertexDist(currentVertex, distance + edge.Length);
                        queue.Push(new VertexDist(edge.End, distance + edge.Length));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Struct for storing vertex distance pairs
    /// </summary>
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
}
