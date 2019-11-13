namespace Util.Geometry.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Basic implementation of the graph interface, using adjacency lists to store vertex-edge information 
    /// (as opposed to an adjacency matrix).
    /// </summary>
    public class AdjacencyListGraph : IGraph
    {
        private readonly List<Vertex> m_Vertices;
        private readonly Dictionary<Vertex, List<Edge>> m_Edges;

        public AdjacencyListGraph() : this(new GraphType(false, true))
        { }

        public AdjacencyListGraph(GraphType typ) : this(new List<Vertex>(), typ)
        { }

        public AdjacencyListGraph(IEnumerable<Vertex> vertices) : this(vertices, new GraphType(false, true))
        { }

        public AdjacencyListGraph(IEnumerable<Vertex> vertices, GraphType typ) : this(vertices, new List<Edge>(), typ)
        { }

        public AdjacencyListGraph(IEnumerable<Vertex> vertices, IEnumerable<Edge> edges)
            : this(vertices, edges, new GraphType(false, true))
        { }

        public AdjacencyListGraph(IEnumerable<Vertex> vertices, IEnumerable<Edge> edges, GraphType typ)
        {
            m_Vertices = new List<Vertex>();
            m_Edges = new Dictionary<Vertex, List<Edge>>();
            Type = typ;
            EdgeCount = 0;

            // add given vertices and edges in order
            foreach (var v in vertices) AddVertex(v);
            foreach (var e in edges) AddEdge(e);
        }

        public GraphType Type { get; internal set; }

        public ICollection<Vertex> Vertices
        {
            get { return m_Vertices; }
        }

        public ICollection<Edge> Edges
        {
            get
            {
                var edges = new HashSet<Edge>();
                foreach (var edgelist in m_Edges.Values)
                {
                    foreach (var e in edgelist)
                        if (Type.DIRECTED || !edges.Contains(e.Twin))
                            edges.Add(e);
                }
                return edges;
            }
        }

        public int VertexCount { get { return m_Vertices.Count; } }

        public int EdgeCount { get; private set; }

        public float TotalEdgeLength
        {
            get
            {
                var sum = Edges.Sum(e => e.Length);
                //if (!Type.DIRECTED) sum /= 2f;
                return sum;
            }
        }

        public float TotalEdgeWeight
        {
            get
            {
                var sum = Edges.Sum(e => e.Weight);
                //if (!Type.DIRECTED) sum /= 2f;
                return sum;
            }
        }

        public void MakeComplete()
        {
            // add edge between all vertices
            foreach (var u in m_Vertices)
            {
                if (!m_Edges.ContainsKey(u))
                {
                    throw new GeomException("Vertex is not present in edge dictionary");
                }

                // clear current edges
                RemoveAllEdges(m_Edges[u]);

                foreach (var v in m_Vertices)
                {
                    if (!u.Equals(v)) AddEdge(u, v);
                }
            }
        }

        public Edge AddEdge(Edge e)
        {
            if (!m_Edges.ContainsKey(e.Start) || !m_Edges.ContainsKey(e.End))
            {
                throw new GeomException("Edge contains vertices not in the graph");
            }

            if (Type.SIMPLE && e.Start == e.End)
            {
                throw new GeomException("Simple graph cannot have self-loops");
            }

            // edge already exists
            if (Type.SIMPLE && ContainsEdge(e)) return e;

            // add edge to graph
            m_Edges[e.Start].Add(e);
            EdgeCount++;

            // check if back edge is necessary
            // only add back edge in undirected graph
            if (!Type.DIRECTED)
            {
                Edge e_back = new Edge(e.End, e.Start, e.Weight)
                {
                    Twin = e
                };
                e.Twin = e_back;

                // if back edge does not exist or graph isnt simple, add back edge
                if (!(Type.SIMPLE && ContainsEdge(e_back)))
                {
                    m_Edges[e.End].Add(e_back);
                }
            }

            return e;
        }

        public Edge AddEdge(Vertex u, Vertex v)
        {
            return AddEdge(new Edge(u, v));
        }

        public Vertex AddVertex()
        {
            return AddVertex(new Vertex());
        }

        public Vertex AddVertex(Vertex v)
        {
            m_Vertices.Add(v);
            m_Edges.Add(v, new List<Edge>());
            return v;
        }

        public void Clear()
        {
            // remove each vertex/edge individually
            // important to not just clear the lists, since remove method might have other hidden effects
            var toRemoveVertices = Vertices.ToList();
            foreach (var v in toRemoveVertices) RemoveVertex(v);
            var toRemoveEdges = Edges.ToList();
            foreach (var e in toRemoveEdges) RemoveEdge(e);

            // clear just to be sure
            m_Vertices.Clear();
            m_Edges.Clear();
        }

        public bool ContainsVertex(Vertex v)
        {
            if (v == null)
            {
                throw new ArgumentException("Vertex cannot be null");
            }
            return m_Vertices.Contains(v);
        }

        public bool ContainsEdge(Edge e)
        {
            if (e == null)
            {
                throw new ArgumentException("Edge cannot be null");
            }
            if (!m_Edges.ContainsKey(e.Start)) return false;
            else return m_Edges[e.Start].Contains(e);
        }

        public bool ContainsEdge(Vertex u, Vertex v)
        {
            if (u == null || v == null)
            {
                throw new ArgumentException("Vertices cannot be null");
            }
            return ContainsEdge(new Edge(u, v));
        }

        public int DegreeOf(Vertex v)
        {
            if (v == null || !m_Edges.ContainsKey(v))
            {
                throw new ArgumentException("Graph does not contain vertex");
            }
            return m_Edges[v].Count;
        }

        public IEnumerable<Edge> EdgesOf(Vertex v)
        {
            if (v == null || !m_Edges.ContainsKey(v))
            {
                throw new ArgumentException("Graph does not contain vertex");
            }

            if (Type.DIRECTED)
            {
                foreach (var e in Edges)
                {
                    if (e.Start.Equals(v) || e.End.Equals(v)) yield return e;
                }
            }
            else
            {
                foreach (var e in m_Edges[v])
                {
                    yield return e;
                }
            }
        }

        public int InDegreeOf(Vertex v)
        {
            if (v == null || !m_Edges.ContainsKey(v))
            {
                throw new ArgumentException("Graph does not contain vertex");
            }

            if (!Type.DIRECTED) return DegreeOf(v);

            return Edges.Where(e => e.End.Equals(v)).Count();
        }

        public IEnumerable<Edge> InEdgesOf(Vertex v)
        {
            if (v == null || !m_Edges.ContainsKey(v))
            {
                throw new ArgumentException("Graph does not contain vertex");
            }

            if (!Type.DIRECTED) { /* nothing done atm */ }

            foreach (var e in Edges)
            {
                if (e.End.Equals(v)) yield return e;
            }
        }

        public int OutDegreeOf(Vertex v)
        {
            if (v == null || !m_Edges.ContainsKey(v))
            {
                throw new ArgumentException("Graph does not contain vertex");
            }

            if (!Type.DIRECTED) return DegreeOf(v);

            return m_Edges[v].Count;
        }

        public IEnumerable<Edge> OutEdgesOf(Vertex v)
        {
            if (v == null || !m_Edges.ContainsKey(v))
            {
                throw new ArgumentException("Graph does not contain vertex");
            }

            if (!Type.DIRECTED) return EdgesOf(v);

            return m_Edges[v];
        }

        public void RemoveAllEdges(IEnumerable<Edge> E)
        {
            // create copy to avoid any concurrent modification error
            var toRemove = new List<Edge>(E);

            foreach (var e in toRemove)
            {
                RemoveEdge(e);
            }
        }

        public void RemoveAllVertices(IEnumerable<Vertex> V)
        {
            // create copy to avoid any concurrent modification error
            var toRemove = new List<Vertex>(V);

            foreach (var v in toRemove)
            {
                RemoveVertex(v);
            }
        }

        public Edge RemoveEdge(Edge e)
        {
            // nothing to do
            if (!ContainsEdge(e)) return e;

            m_Edges[e.Start].Remove(e);
            EdgeCount--;

            if (!Type.DIRECTED) m_Edges[e.End].Remove(e.Twin);

            return e;
        }

        public void RemoveEdges(Vertex u, Vertex v)
        {
            var toRemove = m_Edges[u].FindAll(e => e.End.Equals(v));

            if (!Type.DIRECTED)
            {
                toRemove.AddRange(m_Edges[v].FindAll(e => e.End.Equals(u)));
            }

            RemoveAllEdges(toRemove);
        }

        public Vertex RemoveVertex(Vertex v)
        {
            m_Vertices.Remove(v);
            m_Edges.Remove(v);
            return v;
        }

        /// <summary>
        /// Checks wheter graph_1 and other have the same number of vertices and edges,
        /// the vertices have the same positon and the edges are between the same vertices
        /// </summary>
        /// <param name="graph_1"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(IGraph other)
        {

            //equal size
            if (VertexCount != other.VertexCount)
            {
                return false;
            }
            if (EdgeCount != other.EdgeCount)
            {
                return false;
            }

            //contaimaint of 1 in 2
            foreach (var vertex in Vertices)
            {
                if (!other.ContainsVertex(vertex))
                {
                    return false;
                }
            }
            foreach (Edge edge in Edges)
            {
                if (!other.ContainsEdge(edge))
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            var str = "Edges: {";
            foreach (var e in Edges) str += e + ", ";
            return str + "}";
        }
    }

    public class AdjacencyListGraph<E> : AdjacencyListGraph, IGraph<E>
    {

        private readonly Dictionary<Edge, E> EdgeProp = new Dictionary<Edge, E>();

        public AdjacencyListGraph() : this(new GraphType(false, true))
        { }

        public AdjacencyListGraph(GraphType typ) : this(new List<Vertex>(), typ)
        { }

        public AdjacencyListGraph(IEnumerable<Vertex> vertices) : this(vertices, new GraphType(false, true))
        { }

        public AdjacencyListGraph(IEnumerable<Vertex> vertices, GraphType typ) : this(vertices, new List<Edge>(), typ)
        { }

        public AdjacencyListGraph(IEnumerable<Vertex> vertices, IEnumerable<Edge> edges)
            : this(vertices, edges, new GraphType(false, true))
        { }

        public AdjacencyListGraph(IEnumerable<Vertex> vertices, IEnumerable<Edge> edges, GraphType typ)
            : base(vertices, edges, typ)
        { }

        public new Edge AddEdge(Edge e)
        {
            EdgeProp.Add(e, default(E));
            base.AddEdge(e);
            if (!Type.DIRECTED) EdgeProp.Add(e.Twin, default(E));
            return e;
        }

        public new Edge RemoveEdge(Edge e)
        {
            EdgeProp.Remove(e);
            if (!Type.DIRECTED) EdgeProp.Remove(e.Twin);
            return base.RemoveEdge(e);
        }

        public new void Clear()
        {
            base.Clear();
            EdgeProp.Clear();
        }

        public E GetEdgeProp(Edge e)
        {
            if (!ContainsEdge(e))
            {
                throw new GeomException("Graph does not contain edge parameter.");
            }
            return EdgeProp[e];
        }

        public void SetEdgeProp(Edge e, E val)
        {
            if (!ContainsEdge(e))
            {
                throw new GeomException("Graph does not contain edge parameter.");
            }
            EdgeProp[e] = val;
        }
    }

    public class AdjacencyListGraph<V, E> : AdjacencyListGraph<E>, IGraph<V, E>
    {

        private readonly Dictionary<Vertex, V> VertexProp = new Dictionary<Vertex, V>();

        public AdjacencyListGraph() : this(new GraphType(false, true))
        { }

        public AdjacencyListGraph(GraphType typ) : this(new List<Vertex>(), typ)
        { }

        public AdjacencyListGraph(IEnumerable<Vertex> vertices) : this(vertices, new GraphType(false, true))
        { }

        public AdjacencyListGraph(IEnumerable<Vertex> vertices, GraphType typ) : this(vertices, new List<Edge>(), typ)
        { }

        public AdjacencyListGraph(IEnumerable<Vertex> vertices, IEnumerable<Edge> edges)
            : this(vertices, edges, new GraphType(false, true))
        { }

        public AdjacencyListGraph(IEnumerable<Vertex> vertices, IEnumerable<Edge> edges, GraphType typ)
            : base(vertices, edges, typ)
        { }

        public new Vertex AddVertex()
        {
            Vertex v = base.AddVertex();
            VertexProp.Add(v, default(V));
            return v;
        }

        public new Vertex AddVertex(Vertex v)
        {
            VertexProp.Add(v, default(V));
            return base.AddVertex(v);
        }

        public new Vertex RemoveVertex(Vertex v)
        {
            VertexProp.Remove(v);
            return base.RemoveVertex(v);
        }

        public new void Clear()
        {
            base.Clear();
            VertexProp.Clear();
        }

        public V GetVertexProp(Vertex v)
        {
            if (!ContainsVertex(v))
            {
                throw new GeomException("Graph does not contain vertex parameter.");
            }
            return VertexProp[v];
        }

        public void SetVertexProp(Vertex v, V val)
        {
            if (!ContainsVertex(v))
            {
                throw new GeomException("Graph does not contain vertex parameter.");
            }
            VertexProp[v] = val;
        }
    }
}

