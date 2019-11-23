namespace Util.Algorithms.DCEL
{
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Algorithms.Triangulation;
    using Util.Geometry;
    using Util.Geometry.DCEL;
    using Util.Geometry.Triangulation;

    /// <summary>
    /// Collection of algorithms related to Voronoi diagrams.
    /// </summary>
    public static class Voronoi
    {

        /// <summary>
        /// Create Voronoi DCEL from a collection of vertices.
        /// First creates a delaunay triangulation and then the corresponding Voronoi diagram
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns>DCEL representation of Voronoi diagram</returns>
        public static DCEL Create(IEnumerable<Vector2> vertices)
        {
            // create delaunay triangulation
            // from this the voronoi diagram can be obtained
            var m_Delaunay = Delaunay.Create(vertices);

            return Create(m_Delaunay);
        }

        /// <summary>
        /// Creates a Voronoi DCEL from a triangulation. Triangulation should be Delaunay
        /// </summary>
        /// <param name="m_Delaunay"></param>
        /// <returns></returns>
        public static DCEL Create(Triangulation m_Delaunay)
        {
            if (!Delaunay.IsValid(m_Delaunay))
            {
                throw new GeomException("Triangulation should be delaunay for the Voronoi diagram.");
            }

            var dcel = new DCEL();

            // create vertices for each triangles circumcenter and store them in a dictionary
            Dictionary<Triangle, DCELVertex> vertexMap = new Dictionary<Triangle, DCELVertex>();
            foreach (var triangle in m_Delaunay.Triangles)
            {
                // degenerate triangle, just ignore
                if (!triangle.Circumcenter.HasValue) continue;

                var vertex = new DCELVertex(triangle.Circumcenter.Value);
                dcel.AddVertex(vertex);
                vertexMap.Add(triangle, vertex);
            }

            // remember which edges where visited
            // since each edge has a twin
            var edgesVisited = new HashSet<TriangleEdge>();

            foreach (var edge in m_Delaunay.Edges)
            {
                // either already visited twin edge or edge is outer triangle
                if (edgesVisited.Contains(edge) || edge.IsOuter) continue;

                // add edge between the two adjacent triangles vertices
                // vertices at circumcenter of triangle
                if (edge.T != null && edge.Twin.T != null)
                {
                    var v1 = vertexMap[edge.T];
                    var v2 = vertexMap[edge.Twin.T];

                    dcel.AddEdge(v1, v2);

                    edgesVisited.Add(edge);
                    edgesVisited.Add(edge.Twin);
                }
            }

            return dcel;
        }
    }
}