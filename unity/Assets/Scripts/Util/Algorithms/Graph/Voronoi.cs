namespace Util.Algorithms.Graph
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry.Triangulation;
    using Util.Geometry.DCEL;
    using Util.Algorithms.Triangulation;
    using Util.Geometry;
    using Util.Math;
    using System.Linq;

    public static class Voronoi {

        public static DCEL Create(IEnumerable<Vector2> vertices)
        {
            // create delaunay triangulation
            // from this the voronoi diagram can be obtained
            var m_Delaunay = Delaunay.Create(vertices);

            return Create(m_Delaunay);
        }

        public static DCEL Create(Triangulation m_Delaunay)
        {
            var dcel = new DCEL();

            Dictionary<Triangle, DCELVertex> vertexMap = new Dictionary<Triangle, DCELVertex>();

            Debug.Log(m_Delaunay.Triangles.ToList().Count + " " + 
                m_Delaunay.Edges.ToList().Count + " " + 
                m_Delaunay.Vertices.ToList().Count);

            foreach (var triangle in m_Delaunay.Triangles)
            {
                var vertex = new DCELVertex(triangle.Circumcenter);
                dcel.AddVertex(vertex);
                vertexMap.Add(triangle, vertex);
            }

            var edgesVisited = new HashSet<TriangleEdge>();

            foreach (var edge in m_Delaunay.Edges)
            {
                // either already visited twin edge or edge is outer triangle
                if (edgesVisited.Contains(edge) || edge.IsOuter) continue;

                if(edge.T != null && edge.Twin.T != null)
                {
                    var v1 = vertexMap[edge.T];
                    var v2 = vertexMap[edge.Twin.T];
                    dcel.AddEdge(v1, v2);
                }

                edgesVisited.Add(edge);
                edgesVisited.Add(edge.Twin);
            }

            return dcel;
        }
    }
}