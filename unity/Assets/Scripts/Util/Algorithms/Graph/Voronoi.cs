namespace Util.Algorithms.Graph
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry.Triangulation;
    using Util.Geometry.DCEL;
    using Util.Algorithms.Triangulation;
    using Util.Geometry;

    public static class Voronoi {
        public static DCEL Create(IEnumerable<Vector2> vertices)
        {
            // create delaunay triangulation
            // from this the voronoi diagram can be obtained
            var m_Delaunay = Delaunay.Create(vertices);

            // retrieve all edge/lines
            var lines = new List<LineSegment>();
            foreach (var edge in m_Delaunay.Edges)
            {
                Triangle t1 = edge.T;
                Triangle t2 = edge.Twin.T;
                if (t1 != null && t2 != null)
                {
                    lines.Add(new LineSegment(t1.Circumcenter, t2.Circumcenter));
                }
            }

            return new DCEL(lines);
        }
    }
}