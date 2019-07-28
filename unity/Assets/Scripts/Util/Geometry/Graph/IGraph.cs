namespace Util.Geometry.Graph
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public interface IGraph
    {
        GraphType Type { get; }
        ICollection<Vertex> Vertices { get; }
        ICollection<Edge> Edges { get; }

        float totalEdgeLength { get; }
        float totalEdgeWeight { get; }

        void MakeComplete();

        Vertex AddVertex();
        Vertex AddVertex(Vertex v);

        Edge AddEdge(Edge e);
        Edge AddEdge(Vertex u, Vertex v);

        int DegreeOf(Vertex v);
        int OutDegreeOf(Vertex v);
        int InDegreeOf(Vertex v);

        IEnumerable<Edge> EdgesOf(Vertex v);
        IEnumerable<Edge> InEdgesOf(Vertex v);
        IEnumerable<Edge> OutEdgesOf(Vertex v);

        bool ContainsVertex(Vertex v);
        bool ContainsEdge(Edge e);

        Vertex RemoveVertex(Vertex v);
        void RemoveAllVertices(ICollection<Vertex> V);
        Edge RemoveEdge(Edge e);
        void RemoveEdges(Vertex u, Vertex v);
        void RemoveAllEdges(ICollection<Edge> E);

        void Clear();
    }

    public interface IGraph<E> : IGraph
    {
        E getEdgeProp(Edge e);
        void setEdgeProp(Edge e, E val);
    }

    public interface IGraph<V,E> : IGraph<E>
    {
        V getVertexProp(Vertex v);
        void setVertexProp(Vertex v, V val);
    }
}
