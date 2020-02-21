namespace Util.Algorithms.Triangulation
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Geometry;
    using Util.Geometry.Triangulation;

    /// <summary>
    /// Collection of algorithms related to Delaunay triangulation.
    /// </summary>
    public static class Delaunay
    {

        private const float m_farAway = 999f;

        /// <summary>
        /// Create initial delaunay triangulation
        /// </summary>
        /// <returns></returns>
        public static Triangulation Create()
        {
            var v0 = new Vector2(-m_farAway, -m_farAway);
            var v1 = new Vector2(0, m_farAway);
            var v2 = new Vector2(m_farAway, -m_farAway);

            return new Triangulation(v0, v1, v2);
        }

        /// <summary>
        /// Create a Delaunay triangulation of the given vertices.
        /// Adds vertices iteratively, while keeping Delaunay condition.
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static Triangulation Create(IEnumerable<Vector2> vertices)
        {
            var T = Create();
            foreach (var v in vertices)
            {
                AddVertex(T, v);
            }

            T.RemoveInitialTriangle();

            return T;
        }

        /// <summary>
        /// Checks if the Delaunay property holds for the triangulation.
        /// </summary>
        /// <param name="T"></param>
        /// <returns>Whether the triangulation is a Delaunay triangulation.</returns>
        public static bool IsValid(Triangulation T)
        {
            return T.Edges.All(e => IsValid(e));
        }

        /// <summary>
        /// Checks if the Delaunay property holds for the edge.
        /// </summary>
        /// <param name="T"></param>
        /// <returns>Whether the triangle edge is valid for a Delaunay triangulation.</returns>
        public static bool IsValid(TriangleEdge a_Edge)
        {
            // outer edge always valid
            if (a_Edge != null && a_Edge.IsOuter) return true;

            if (a_Edge == null || a_Edge.T == null || a_Edge.Twin == null || a_Edge.Twin.T == null)
            {
                throw new GeomException("Invalid triangle edge - null pointers");
            }

            var a_triangle = a_Edge.T;
            var a_Twin = a_Edge.Twin.T;

            // Points to test
            var u = a_Edge.T.OtherVertex(a_Edge).Value;
            var v = a_Edge.Twin.T.OtherVertex(a_Edge.Twin).Value;

            return !a_triangle.InsideCircumcircle(v) && !a_Twin.InsideCircumcircle(u);
        }

        /// <summary>
        /// Adds a vertex at the given position to the Delaunay triangulation.
        /// Legalizes relevant edges in order to maintain Delaunay condition.
        /// </summary>
        /// <param name="T"></param>
        /// <param name="a_vertex"></param>
        /// <returns></returns>
        public static void AddVertex(Triangulation T, Vector2 a_vertex)
        {
            // find triangle that contains X
            var triangle = T.FindContainingTriangle(a_vertex);

            T.AddVertex(a_vertex);

            // Flip if needed
            LegalizeEdge(T, a_vertex, triangle.E0);
            LegalizeEdge(T, a_vertex, triangle.E1);
            LegalizeEdge(T, a_vertex, triangle.E2);
        }

        /// <summary>
        /// Makes an edge legal accorinding to the Delaunay condition.
        /// Recurses whenever flipping an edge since adjacent edges can be made illegal.
        /// </summary>
        /// <param name="T"></param>
        /// <param name="a_Vertex"></param>
        /// <param name="a_Edge"></param>
        public static void LegalizeEdge(Triangulation T, Vector2 a_Vertex, TriangleEdge a_Edge)
        {
            // do not legalize outer edge
            if (a_Edge != null && a_Edge.IsOuter) return;

            if (a_Edge == null || a_Edge.T == null || a_Edge.Twin == null || a_Edge.Twin.T == null)
            {
                throw new GeomException("Invalid triangle edge - Cannot legalize edge");
            }

            var a_triangle = a_Edge.T;
            var a_Twin = a_Edge.Twin.T;

            if (!IsValid(a_Edge))
            {
                Flip(T, a_Edge);

                LegalizeEdge(T, a_Vertex, a_Twin.OtherEdge(a_Edge.Twin, a_Edge.Point1));
                LegalizeEdge(T, a_Vertex, a_Twin.OtherEdge(a_Edge.Twin, a_Edge.Point2));
            }
        }

        /// <summary>
        /// Flips the given triangle edge in the triangulation.
        /// Flipping for Delaunay means changing the edge for the adjacent two triangles to the other possibility.
        /// </summary>
        /// <param name="T"></param>
        /// <param name="a_Edge"></param>
        public static void Flip(Triangulation T, TriangleEdge a_Edge)
        {
            var a_Triangle = a_Edge.T;
            var a_Twin = a_Edge.Twin.T;

            if (a_Triangle == null || a_Twin == null)
            {
                throw new GeomException("Cannot flip edge if neighbouring triangles don't exist");
            }

            // retrieve other adjacent edges to edge vertices
            // e3  .  e1
            //   / | \
            //  .  |  .
            //   \ | /
            // e2  .  e0
            var e0 = a_Triangle.OtherEdge(a_Edge, a_Edge.Point1);
            var e1 = a_Triangle.OtherEdge(a_Edge, a_Edge.Point2);
            var e2 = a_Twin.OtherEdge(a_Edge.Twin, a_Edge.Point1);
            var e3 = a_Twin.OtherEdge(a_Edge.Twin, a_Edge.Point2);

            // create new triangle edges
            var euv = new TriangleEdge(e0.Point1, e2.Point2, null, null);
            var evu = new TriangleEdge(e2.Point2, e0.Point1, euv, null);
            euv.Twin = evu;

            // Remove old triangles
            T.RemoveTriangle(a_Triangle);
            T.RemoveTriangle(a_Twin);

            // add new triangles
            T.AddTriangle(new Triangle(e0, e2, evu));
            T.AddTriangle(new Triangle(e3, e1, euv));
        }
    }
}

