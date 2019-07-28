using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Util.DataStructures.Queue;
using System;

namespace Algo.Graph
{
    /// <summary>
    /// Contains three fields, together indicating whether a graph is a spanner. It is gauranteed FalsificationPathsStart and 
    /// FalsificationPathsEnd are nonempty when IsSpanner is false. Furthermore FalsificationPathsStart and  FalsificationPathsEnd are empty when
    /// IsSpanner is true.
    /// </summary>
    public struct SpannerVerification
    {
        /// <summary>
        /// Whether the Graph that created this SpannerVerfication is a spanner
        /// </summary>
        public readonly bool IsSpanner;
        /// <summary>
        /// Startpoint of a falsifcation path (i.e. the length of shorthest path between these two vertices is longer then t times the euclidian distance)
        /// </summary>
        public readonly List<Vertex> FalsificationPathsStart;
        /// <summary>
        /// Endpoint of a falsifcation path (i.e. the length of shorthest path between these two vertices is longer then t times the euclidian distance)
        /// </summary>
        public readonly List<Vertex> FalsificationPathsEnd;

        /// <summary>
        /// The ratio of the current spanner.
        /// </summary>
        public readonly float Ratio;

        internal SpannerVerification(List<Vertex> a_start, List<Vertex> a_end, float a_ratio)
        {
            Debug.Assert(a_start.Count == a_end.Count);
            
            IsSpanner = (a_start.Count == 0);
            FalsificationPathsStart = a_start;
            FalsificationPathsEnd = a_end;
            Ratio = a_ratio;
        }
    }
    
    /// <summary>
    /// Simulates a undirected graph where faces have no meaning. Vertices have a positon, and edges consist of two vertices.
    /// 
    /// </summary>
    public class Graph
    {
        public List<Vertex> Vertices
        {
            get
            {
                return m_vertices;
            }
        }
        public List<Edge> Edges
        {
            get
            {
                return m_edges;
            }
        }

        public List<Vector2> Positions
        {
            get
            {
                return m_vertices.Select(v => v.Pos).ToList();
            }
        }

        private List<Vertex> m_vertices;
        private List<Edge> m_edges;

        /// <summary>
        /// Creates a empty graph
        /// </summary>
        public Graph()
        {
            m_vertices = new List<Vertex>();
            m_edges = new List<Edge>();
        }

        /// <summary>
        /// Creates a empty graph with vertices on the given positions
        /// </summary>
        public Graph(List<Vector2> positions)
        {
            m_vertices = positions.Select(pos => new Vertex(pos)).ToList();
            m_edges = new List<Edge>();

        }

        /// <summary>
        /// Creates a empty graph on the given vertices. 
        /// It is required that these vertices are used in no other graph.
        /// </summary>
        public Graph(IEnumerable<Vertex> a_vertices)
        {
            m_vertices = a_vertices.ToList();
            m_edges = new List<Edge>();
        }


        /// <summary>
        /// Sets the edge set to the complete graph on the currently set vertices
        /// </summary>
        public void MakeCompleteGraph()
        {
            //cleanly remove existing edges
            if (m_edges.Count >0)
            {
                throw new AlgoException("Old edges are not yet nicly removed");
            }

            m_edges = new List<Edge>();

            for (int i = 0; i < m_vertices.Count; i++)
            {
                for (int j = i + 1; j < m_vertices.Count; j++)
                {
                    m_edges.Add(new Edge(m_vertices[i], m_vertices[j], this));
                }
            }
        }

        /// <summary>
        /// Set's the vertices to a deep copy (without incident edges etc.) of the provided list. Also clears all edges.
        /// </summary>
        /// <param name="a_vertices"></param>
        void SetVerticesDeepCopy(List<Vertex> a_vertices)
        {
            m_vertices = new List<Vertex>();
            foreach (var vertex in a_vertices)
            {
                m_vertices.Add(new Vertex(vertex.Pos));
            }
            ClearEdges();
        }

        public SpannerVerification VerifySpanner(float a_t)
        {
            //first determine the possible edges
            var completeGraph = new Graph(Positions);
            completeGraph.MakeCompleteGraph();
            var edges = completeGraph.Edges;
            edges.Sort(new EdgeByMinLengthComparer());


            var startvertices = new List<Vertex>();
            var endvertices = new List<Vertex>();
            var ratio = 1f; //best possible
 
            foreach (var edge in edges)
            {
                var startvertex = Vertex(edge.Vertex1.Pos);
                var endvertex = Vertex(edge.Vertex2.Pos);
                var edgeratio = ShorthestPathLength(startvertex, endvertex) / edge.Length;
                if (edgeratio > a_t)
                {
                    startvertices.Add(startvertex);
                    endvertices.Add(endvertex);
                }
                if (ratio < edgeratio)
                {
                    ratio = edgeratio;
                }
            }
            return new SpannerVerification(startvertices, endvertices, ratio);
        }

        /// <summary>
        /// Add's an edge provided that both vertices are part of the graph, not the samen and the
        /// edge does not exist already.
        /// </summary>
        /// <param name="a_v1"></param>
        /// <param name="a_v2"></param>
        public Edge AddEdge(Vertex a_v1, Vertex a_v2)
        {
            if (a_v1 == a_v2)
            {
                return null;
            }
            if (m_vertices.Contains(a_v1) && m_vertices.Contains(a_v2))
            {
                if(!IsEdge(a_v1, a_v2))
                {
                    var edge = new Edge(a_v1, a_v2, this);
                    m_edges.Add(edge);
                    return edge;
                }
                return null;
            }
            else
            {
                throw new AlgoException("Graph doesn't contain one of the provided vertices");
            }
        }

        /// <summary>
        /// Safly removes and edge
        /// </summary>
        /// <param name="a_edge"></param>
        public void RemoveEdge(Edge a_edge)
        {
            if (m_vertices.Contains(a_edge.Vertex1) && m_vertices.Contains(a_edge.Vertex2))
            {
                a_edge.CleanUp();
                m_edges.Remove(a_edge);
            }
            else
            {
                throw new AlgoException("Graph doesn't contain one of the provided vertices");
            }
        }

        /// <summary>
        /// Checks wheter a (undirected) edge between a_v1 and a_v2 exists
        /// </summary>
        /// <param name="a_v1"></param>
        /// <param name="a_v2"></param>
        /// <returns></returns>
        public bool IsEdge(Vertex a_v1, Vertex a_v2)
        {
            foreach (var edge in m_edges)
            {
                if (edge.Vertex1 == a_v1 && edge.Vertex2 == a_v2) {
                    return true;
                }
                if (edge.Vertex1 == a_v2 && edge.Vertex2 == a_v1)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds the vertex at a_pos 
        /// </summary>
        /// <remarks>This method is O(n)</remarks>
        /// <param name="a_pos">The posiotn at whiche you want to find a vertex</param>
        /// <returns> A vetex at a_pos if available, null otherwise</returns>
        public Vertex Vertex(Vector2 a_pos)
        {
            foreach (var vertex in m_vertices)
            {
                if (vertex.Pos == a_pos)
                {
                    return vertex;
                }
            }
            return null;
        }

        
        public bool IsEdge(Vector2 a_pos1, Vector2 a_pos2)
        {
            
            if (m_edges.Exists(e => e.Vertex1.Pos == a_pos1 && e.Vertex2.Pos == a_pos2 ))
            {
                return true;
            }

            if (m_edges.Exists(e => e.Vertex1.Pos == a_pos2 && e.Vertex2.Pos == a_pos1))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Returns the sum of the length of all edges.
        /// </summary>
        /// <returns></returns>
        public float LengthOfAllEdges()
        {
            var result = 0f;
            foreach (Edge edge in Edges)
            {
                result += edge.Length;
            }
            return result;
        }

        /// <summary>
        /// Returns a list of neihbours of the given vertex
        /// </summary>
        /// <param name="a_vertex"></param>
        /// <returns></returns>
        private List<Vertex> Neighbours(Vertex a_vertex)
        {
            var result = new List<Vertex>();
            foreach (var edge in a_vertex.IncidentEdges)
            {
                 result.Add(edge.OtherVertex(a_vertex));
            }
            return result;
        }


        /// <summary>
        /// Implements Prim's algorithm. We act on the graph in place.
        /// </summary>
        public void MinimumSpanningTree()
        {
            //choose arbitrary starting vertex
            var root = m_vertices[0];

            //initialize data structures
            var visitedVertices = new List<Vertex>() { root };
            var edgesToConsider = new BinaryHeap<Edge>(root.IncidentEdges, new EdgeByMinLengthComparer());
            var edgesToRemove = new List<Edge>(Edges); //shallow copy

            while (visitedVertices.Count < m_vertices.Count)
            {
                var edge = edgesToConsider.Pop();
                var v1visited = visitedVertices.Contains(edge.Vertex1);
                var v2visited = visitedVertices.Contains(edge.Vertex2);
                if (v1visited && v2visited)
                {
                    continue;
                } else if (v1visited)
                {
                    //Keep edge
                    edgesToRemove.Remove(edge);
                    visitedVertices.Add(edge.Vertex2);
                    foreach (Edge newedge in edge.Vertex2.IncidentEdges)
                    {
                        if (visitedVertices.Contains(newedge.Vertex1) && visitedVertices.Contains(newedge.Vertex2)) {
                            continue;
                        }
                        edgesToConsider.Push(newedge);
                    }
                    continue;
                } else if (v2visited)
                {
                    //keep Edge
                    edgesToRemove.Remove(edge);
                    visitedVertices.Add(edge.Vertex1);
                    foreach (Edge newedge in edge.Vertex1.IncidentEdges)
                    {
                        if (visitedVertices.Contains(newedge.Vertex1) && visitedVertices.Contains(newedge.Vertex2))
                        {
                            continue;
                        }
                        edgesToConsider.Push(newedge);
                    }
                    continue;
                }
                else
                {
                    throw new AlgoException("Both v1 and v2 are not visited");
                }
            }
        
            //update graph
            foreach(var edge in edgesToRemove)
            {
                RemoveEdge(edge);
            }
        }
        

        public bool IsTSPTour()
        {
            foreach (var vertex in m_vertices)
            {
                if(vertex.IncidentEdges.Count != 2)
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// Checks wheter graph_1 and graph_2 have the same number of vertices and edges,
        /// the vertices have the same positon and the edges are between the same vertices
        /// </summary>
        /// <param name="graph_1"></param>
        /// <param name="graph_2"></param>
        /// <returns></returns>
        public static bool EqualGraphs(Graph graph_1, Graph graph_2)
        {
            //equal size
            if(graph_1.Vertices.Count != graph_2.Vertices.Count)
            {
                return false;
            }
            if (graph_1.Edges.Count != graph_2.Edges.Count)
            {
                return false;
            }

            //contaimaint of 1 in 2
            foreach (var g1_vertex in graph_1.Vertices)
            {
                if (! graph_2.Vertices.Exists(v => v.Pos == g1_vertex.Pos))
                {
                    return false;
                }
            }
            foreach (Edge edge in graph_1.Edges)
            {
                if(graph_2.IsEdge(edge.Vertex1.Pos, edge.Vertex2.Pos))
                {

                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Creates a t-spanner using a greedy algorithm trying the shortest edges first.
        /// </summary>
        /// <param name="a_postions"> the positions on which we want to construct a t-spanner</param>
        /// <param name="t"> parameter t in the definition of t-spanner. Each pair of vertices should have a path
        ///  of at most length t*eucledian distance</param>
        /// <returns></returns>
        public static Graph GreedySpanner(List<Vector2> a_postions, float a_t)
        {
            var result = new Graph(a_postions);

            //first determine the possible edges
            var completeGraph = new Graph(a_postions);
            completeGraph.MakeCompleteGraph();
            var edges = completeGraph.Edges;
            edges.Sort(new EdgeByMinLengthComparer());

            foreach (var edge in edges)
            {
                var directDistance = edge.Length;

                var startvertex = result.Vertex(edge.Vertex1.Pos);
                var endvertex = result.Vertex(edge.Vertex2.Pos);
                if (result.ShorthestPathLength(startvertex, endvertex) > a_t * directDistance)
                {
                    result.AddEdge(startvertex, endvertex);
                }
            }
            return result;
        }

        internal void ClearEdges()
        {
            m_edges = new List<Edge>();
        }


        /// <summary>
        /// Finds the lenth of a TSPtour, provided by christofides algorithm.
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static float FindTSPLength(List<Vector2> a_positions)
        {
            //first determine a MST
            var mst = new Graph(a_positions);
            mst.MakeCompleteGraph();
            mst.MinimumSpanningTree();

            //find odd degree vertices
            var oddDegreePos = new List<Vector2>();
            foreach (var vertex in mst.Vertices)
            {
                if (vertex.Degree %2 == 1)
                {
                    oddDegreePos.Add(vertex.Pos);
                }
            }

            //find minimum weight perfect matcing
            var oddDegreeMatching = MinimumWeightPerfectMatchingOfCompleteGraph(oddDegreePos);

            Debug.Log("mst length: " + mst.LengthOfAllEdges() + "  om length: " + oddDegreeMatching.LengthOfAllEdges());
            return mst.LengthOfAllEdges() + oddDegreeMatching.LengthOfAllEdges();
        }


        //TODO Currently implements greedy, improve on this
        private static Graph MinimumWeightPerfectMatchingOfCompleteGraph(List<Vector2> a_positions)
        {
            if (a_positions.Count %2 == 1)
            {
                throw new AlgoException("odd number of vertices, perfect matching impossible");
            }

            //first determine the possible edges and sort them on distance
            var result = new Graph(a_positions);
            result.MakeCompleteGraph();
            var edges = result.Edges;
            edges.Sort(new EdgeByMinLengthComparer());

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
                var v1Matched = matched[edge.Vertex1];
                var v2Matched = matched[edge.Vertex2];

                if ( !v1Matched && !v2Matched)
                {
                    //keep edge
                    matched[edge.Vertex1] = true;
                    matched[edge.Vertex2] = true;
                }
                else
                {
                    //remove edge
                    edgesToRemove.Add(edge);
                }
            }
            foreach(var edge in edgesToRemove)
            {
                result.RemoveEdge(edge);
            }

            //test degree
            foreach(var v in result.Vertices)
            {
                if(v.Degree != 1)
                {
                    throw new AlgoException("We have not arrived at a matching");
                }
            }

            return result;
        }
    }
}
