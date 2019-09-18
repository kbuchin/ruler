namespace Util.Algorithms.Triangulation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry.Triangulation;
    using Util.Geometry;
    using System.Linq;

    public static class Delaunay {

        private const float m_farAway = 999f;

        public static Triangulation Create()
        {
            var v0 = new Vector2(-m_farAway, -m_farAway);
            var v1 = new Vector2(m_farAway, -m_farAway);
            var v2 = new Vector2(0, m_farAway);

            return new Triangulation(v0, v1, v2);
        }

        public static Triangulation Create(IEnumerable<Vector2> vertices)
        {
            var T = Create();
            foreach (var v in vertices)
            {
                AddVertex(T, v);
            }
            return T;
        }

        public static bool AddVertex(Triangulation T, Vector2 X)
        {
            var triangle = FindTriangle(T, X);
            if (triangle == null)
            {
                Debug.LogWarning("Couldn't place Vector2 in triangle; probably placed point colinear.");
                return false;
            }

            AddVertex(T, triangle, X);

            // Flip if needed
            LegalizeEdge(T, X, triangle.E0);
            LegalizeEdge(T, X, triangle.E1);
            LegalizeEdge(T, X, triangle.E2);

            return true;
        }

        private static Triangle FindTriangle(Triangulation T, Vector2 a_Vector2)
        {
            foreach (Triangle triangle in T.Triangles)
            {
                if (triangle.Inside(a_Vector2))
                {
                    return triangle;
                }
            }
            return null;
        }

        private static void AddVertex(Triangulation T, Triangle a_Triangle, Vector2 X)
        {
            if(!a_Triangle.Inside(X))
            {
                throw new ArgumentException("Vector to be added should be inside triangle.");
            }

            T.Remove(a_Triangle);

            var e0x = new TriangleEdge(a_Triangle.P0, X, null, null);
            var ex0 = new TriangleEdge(X, a_Triangle.P0, e0x, null);
            e0x.Twin = ex0;
            var e1x = new TriangleEdge(a_Triangle.P1, X, null, null);
            var ex1 = new TriangleEdge(X, a_Triangle.P1, e1x, null);
            e1x.Twin = ex1;
            var e2x = new TriangleEdge(a_Triangle.P2, X, null, null);
            var ex2 = new TriangleEdge(X, a_Triangle.P2, e2x, null);
            e2x.Twin = ex2;

            var t0 = new Triangle(a_Triangle.E0, e1x, ex0);
            var t1 = new Triangle(a_Triangle.E1, e2x, ex1);
            var t2 = new Triangle(a_Triangle.E2, e0x, ex2);

            T.Add(t0);
            T.Add(t1);
            T.Add(t2);
        }

        private static void LegalizeEdge(Triangulation T, Vector2 a_Vertex, TriangleEdge a_Edge)
        {
            var a_triangle = a_Edge.T;
            if(a_triangle == null)
            {
                throw new GeomException("Invalid TriangleEdge, cannot legalize");
            }

            if (a_Edge.IsOuter)
            {
                // never legalize the initial triangle
                return;
            }

            var a_Twin = a_Edge.Twin.T;
            if (a_Twin == null)
            {
                throw new GeomException("Cannot legalize edge if neighbouring triangles don't exist");
            }

            // Points to test
            var u = (Vector2)a_Edge.T.OtherVertex(a_Edge);
            var v = (Vector2)a_Edge.Twin.T.OtherVertex(a_Edge.Twin);

            if (a_triangle.InsideCircumcircle(v) || a_Twin.InsideCircumcircle(u))
            {
                Flip(T, a_Edge);

                LegalizeEdge(T, a_Vertex, a_Twin.OtherEdge(a_Edge.Twin, a_Edge.Point1));
                LegalizeEdge(T, a_Vertex, a_Twin.OtherEdge(a_Edge.Twin, a_Edge.Point2));
            }
        }

        private static void Flip(Triangulation T, TriangleEdge a_Edge)
        {
            var a_Triangle = a_Edge.T;
            var a_Twin = a_Edge.Twin.T;

            if (a_Triangle == null || a_Twin == null)
            {
                throw new GeomException("Cannot flip edge if neighbouring triangles don't exist");
            }

            // Remove old triangles
            T.Remove(a_Triangle);
            T.Remove(a_Twin);

            var e0 = a_Triangle.OtherEdge(a_Edge, a_Edge.Point1);
            var e1 = a_Triangle.OtherEdge(a_Edge, a_Edge.Point2);
            var e2 = a_Twin.OtherEdge(a_Edge.Twin, a_Edge.Point1);
            var e3 = a_Twin.OtherEdge(a_Edge.Twin, a_Edge.Point2);

            var euv = new TriangleEdge(e0.Point1, e2.Point2, null, null);
            var evu = new TriangleEdge(e2.Point2, e0.Point1, euv, null);
            euv.Twin = evu;

            var t0 = new Triangle(e0, e2, evu);
            var t1 = new Triangle(e3, e1, euv);

            T.Add(t0);
            T.Add(t1);
        }
    }
}

