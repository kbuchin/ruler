namespace Util.Geometry.Graph
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

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
                var edges = new List<Edge>();
                foreach (var edgelist in m_Edges.Values)
                {
                    foreach (var e in edgelist) edges.Add(e);
                }
                return edges;
            }
        }

        public float totalEdgeLength
        {
            get { return Edges.Sum(e => e.Length); }
        }

        public float totalEdgeWeight
        {
            get { return Edges.Sum(e => e.Weight); }
        }

        public void MakeComplete()
        {
            // clear all current edges
            m_Edges.Clear();

            // add edge between all vertices
            foreach (var u in m_Vertices)
            {
                foreach (var v in m_Vertices)
                {
                    if (!u.Equals(v)) AddEdge(u, v);
                }
            }
        }

        public Edge AddEdge(Edge e)
        {
            m_Edges[e.Start].Add(e);

            if (!Type.DIRECTED)
            {
                Edge e_back = new Edge(e.End, e.Start);
                e_back.Twin = e;
                e.Twin = e_back;
                if (!Type.SIMPLE || !ContainsEdge(e_back))
                {
                    m_Edges[e_back.Start].Add(e_back);
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
            return v;
        }

        public void Clear()
        {
            // remove each vertex/edge individually
            // important to not just clear the lists, since remove method might have other hidden effects
            foreach (Vertex v in Vertices) RemoveVertex(v);
            foreach (Edge e in Edges) RemoveEdge(e);

            // clear just to be sure
            m_Vertices.Clear();
            m_Edges.Clear();
        }

        public bool ContainsEdge(Edge e)
        {
            return m_Edges[e.Start].Contains(e);
        }

        public bool ContainsVertex(Vertex v)
        {
            return m_Vertices.Contains(v);
        }

        public int DegreeOf(Vertex v)
        {
            return m_Edges[v].Count;
        }

        public IEnumerable<Edge> EdgesOf(Vertex v)
        {
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
            if (!Type.DIRECTED) return DegreeOf(v);

            int count = 0;
            foreach (var e in Edges)
            {
                if (e.End.Equals(v)) count++;
            }

            return count;
        }

        public IEnumerable<Edge> InEdgesOf(Vertex v)
        {
            if (!Type.DIRECTED) { /* nothing done atm */ }

            foreach (var e in Edges)
            {
                if (e.End.Equals(v)) yield return e;
            }
        }

        public int OutDegreeOf(Vertex v)
        {
            if (!Type.DIRECTED) return DegreeOf(v);

            return m_Edges[v].Count;
        }

        public IEnumerable<Edge> OutEdgesOf(Vertex v)
        {
            if (!Type.DIRECTED) return EdgesOf(v);

            return m_Edges[v];
        }

        public void RemoveAllEdges(ICollection<Edge> E)
        {
            foreach (var e in E)
            {
                RemoveEdge(e);
            }
        }

        public void RemoveAllVertices(ICollection<Vertex> V)
        {
            foreach (var v in V)
            {
                RemoveVertex(v);
            }
        }

        public Edge RemoveEdge(Edge e)
        {
            if (!ContainsEdge(e))
            {
                throw new GeomException("Edge cannot be removed, since it is not contained in the graph");
            }

            m_Edges[e.Start].Remove(e);

            if (!Type.DIRECTED) m_Edges[e.End].Remove(e.Twin);

            return e;
        }

        public void RemoveEdges(Vertex u, Vertex v)
        {
            foreach (var e in m_Edges[u])
            {
                if (e.End.Equals(v)) RemoveEdge(e);
            }

            if (!Type.DIRECTED)
            {
                foreach (var e in m_Edges[v])
                {
                    if (e.End.Equals(u)) RemoveEdge(e);
                }
            }
        }

        public Vertex RemoveVertex(Vertex v)
        {
            m_Vertices.Remove(v);
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
            if (Vertices.Count != other.Vertices.Count)
            {
                return false;
            }
            if (Edges.Count != other.Edges.Count)
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
    }

    public class AdjacencyListGraph<E> : AdjacencyListGraph, IGraph<E>
    {

        private Dictionary<Edge, E> EdgeProp;

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
        {
            EdgeProp = new Dictionary<Edge, E>();
        }

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

        public E getEdgeProp(Edge e)
        {
            if(!ContainsEdge(e))
            {
                throw new GeomException("Graph does not contain edge parameter.");
            }
            return EdgeProp[e];
        }

        public void setEdgeProp(Edge e, E val)
        {
            if (!ContainsEdge(e))
            {
                throw new GeomException("Graph does not contain edge parameter.");
            }
            EdgeProp[e] = val;
        }
    }

    public class AdjacencyListGraph<V,E> : AdjacencyListGraph<E>, IGraph<V,E>
    {

        private Dictionary<Vertex, V> VertexProp;

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
        {
            VertexProp = new Dictionary<Vertex, V>();
        }

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

        public V getVertexProp(Vertex v)
        {
            if (!ContainsVertex(v))
            {
                throw new GeomException("Graph does not contain vertex parameter.");
            }
            return VertexProp[v];
        }

        public void setVertexProp(Vertex v, V val)
        {
            if (!ContainsVertex(v))
            {
                throw new GeomException("Graph does not contain vertex parameter.");
            }
            VertexProp[v] = val;
        }
    }
}

