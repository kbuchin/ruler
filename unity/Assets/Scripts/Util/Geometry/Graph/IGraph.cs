namespace Util.Geometry.Graph
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Graph interface that defines several methods related to adding/removing vertices/edges,
    /// calculating degrees andfinding neighbours
    /// </summary>
    public interface IGraph : IEquatable<IGraph>
    {
        /// <summary>
        /// Defines type of graph (directed/undirected, etc)
        /// </summary>
        GraphType Type { get; }

        ICollection<Vertex> Vertices { get; }
        ICollection<Edge> Edges { get; }

        int VertexCount { get; }
        int EdgeCount { get; }

        float TotalEdgeLength { get; }
        float TotalEdgeWeight { get; }

        /// <summary>
        /// Generates a complete graph between the present vertices.
        /// </summary>
        void MakeComplete();

        /// <summary>
        /// Add a new vertex to the graph, at initial position (0, 0)
        /// </summary>
        /// <returns>The added vertex</returns>
        Vertex AddVertex();

        /// <summary>
        /// Add the given vertex to the graph.
        /// </summary>
        /// <param name="v"></param>
        /// <returns>The added vertex</returns>
        Vertex AddVertex(Vertex v);

        /// <summary>
        /// Add given edge to the graph.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>The added edge</returns>
        Edge AddEdge(Edge e);

        /// <summary>
        /// Creates an edge between the vertices.
        /// If edge already exists, return existing edge.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns>the created edge</returns>
        Edge AddEdge(Vertex u, Vertex v);

        /// <summary>
        /// Calculates the degree of the vertex.
        /// In case of directed graph this is equal to OutDegreeOf
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        int DegreeOf(Vertex v);

        /// <summary>
        /// Calculates the outgoing degree of the vertex.
        /// In case of undirected, this is equal to the DegreeOf.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        int OutDegreeOf(Vertex v);

        /// <summary>
        /// Calculates the ingoing degree of the vertex.
        /// In case of undirected, this is equal to the DegreeOf.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        int InDegreeOf(Vertex v);

        /// <summary>
        /// Find all edges adjacent to the vertex.
        /// In case of a directed graph, this is equal to OutEdgesOf.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        IEnumerable<Edge> EdgesOf(Vertex v);

        /// <summary>
        /// Find all edges going into the vertex.
        /// In case of an undirected graph, this is similar to EdgesOf.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        IEnumerable<Edge> InEdgesOf(Vertex v);

        /// <summary>
        /// Find all edges going out of the vertex.
        /// In case of an undirected graph, this is similar to EdgesOf.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        IEnumerable<Edge> OutEdgesOf(Vertex v);

        /// <summary>
        /// Checks whether the graph contains the given vertex.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        bool ContainsVertex(Vertex v);

        /// <summary>
        /// Checks whether the graph contains the given edge.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        bool ContainsEdge(Edge e);

        /// <summary>
        /// Checks whether there exists an edge between the given vertices.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        bool ContainsEdge(Vertex u, Vertex v);

        /// <summary>
        /// Remove the given vertex from the graph.
        /// Includes all edges adjacent to this vertex.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        Vertex RemoveVertex(Vertex v);

        /// <summary>
        /// Remove all vertices in the given collection.
        /// </summary>
        /// <param name="V"></param>
        void RemoveAllVertices(IEnumerable<Vertex> V);

        /// <summary>
        /// Remove the given edge from the graph.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        Edge RemoveEdge(Edge e);

        /// <summary>
        /// Remove all edges between the given vertices u and v.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        void RemoveEdges(Vertex u, Vertex v);

        /// <summary>
        /// Removes all edges in the given collection.
        /// </summary>
        /// <param name="E"></param>
        void RemoveAllEdges(IEnumerable<Edge> E);

        /// <summary>
        /// Clears the graph of all vertices and edges.
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// Graph interface where edges have an additional property E.
    /// Use whenever the default edges need to carry additional information.
    /// </summary>
    /// <typeparam name="E"></typeparam>
    public interface IGraph<E> : IGraph
    {
        E GetEdgeProp(Edge e);
        void SetEdgeProp(Edge e, E val);
    }

    /// <summary>
    /// Graph interface where both vertices and edges carry additional properties,
    /// V and E respectively.
    /// </summary>
    /// <typeparam name="V"></typeparam>
    /// <typeparam name="E"></typeparam>
    public interface IGraph<V, E> : IGraph<E>
    {
        V GetVertexProp(Vertex v);
        void SetVertexProp(Vertex v, V val);
    }
}
