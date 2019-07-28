namespace Util.Geometry.Algorithms
{
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry.Triangulation;
    using Util.Geometry.DCEL;
    using Util.Geometry.Graph;

    public static class Voronoi {
        public static DCEL Create(IEnumerable<Vertex> vertices)
        {
            var BBox = Rect.MinMaxRect(
                vertices.Min(item => item.Pos.x),
                vertices.Min(item => item.Pos.y),
                vertices.Max(item => item.Pos.x),
                vertices.Max(item => item.Pos.y)
            );

            var lines = new List<Line>();
            Triangulation m_Delaunay = Delaunay.Create(vertices);
            foreach (var edge in m_Delaunay.Edges)
            {
                Triangle t1 = edge.T;
                Triangle t2 = edge.Twin.T;
                if (t1 != null && t2 != null)
                {
                    lines.Add(new Line(t1.Circumcenter, t2.Circumcenter));
                }
            }

            return new DCEL(lines, BBox);
        }
    }
}