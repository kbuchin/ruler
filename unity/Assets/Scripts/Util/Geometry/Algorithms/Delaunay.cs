namespace Util.Geometry.Algorithms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry.DCEL;
    using Util.Geometry.Graph;
    using Util.Geometry.Triangulation;

    public static class Delaunay {

        public static Triangulation Create()
        {
            const float farAway = 10000; // 500000
            var v0 = new Vertex(new Vector2(-farAway, -farAway));
            var v1 = new Vertex(new Vector2(farAway, -farAway));
            var v2 = new Vertex(new Vector2(0, farAway));

            return new Triangulation(v0, v1, v2);
        }

        public static Triangulation Create(IEnumerable<Vertex> vertices)
        {
            var T = Create();
            foreach (var v in vertices)
            {
                AddVertex(T, v);
            }
            return T;
        }

        public static bool AddVertex(Triangulation T, Vertex X)
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

        private static Triangle FindTriangle(Triangulation T, Vertex a_Vector2)
        {
            foreach (Triangle triangle in T.Triangles)
            {
                if (triangle.Inside(a_Vector2.Pos))
                {
                    return triangle;
                }
            }
            return null;
        }

        private static void AddVertex(Triangulation T, Triangle a_Triangle, Vertex X)
        {
            if(!a_Triangle.Inside(X.Pos))
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

            ex0.T = e1x.T = a_Triangle.E0.T = t0;
            ex1.T = e2x.T = a_Triangle.E1.T = t1;
            ex2.T = e0x.T = a_Triangle.E2.T = t2;

            T.Add(t0);
            T.Add(t1);
            T.Add(t2);
        }

        private static void LegalizeEdge(Triangulation T, Vertex a_Vertex, TriangleEdge a_Edge)
        {
            var a_Triangle = a_Edge.T;
            var a_Twin = a_Edge.OtherTriangle();

            if (a_Triangle == null || a_Twin == null)
            {
                throw new GeomException("Cannot legalize edge if neighbouring triangles don't exist");
            }

            // Points to test
            var u = a_Edge.T.OtherVertex(a_Edge);
            var v = a_Edge.Twin.T.OtherVertex(a_Edge.Twin);

            if (a_Triangle.InsideCircumcircle(v.Pos) || a_Twin.InsideCircumcircle(u.Pos))
            {
                Flip(T, a_Edge);

                LegalizeEdge(T, a_Vertex, a_Twin.OtherEdge(a_Edge.Twin, a_Edge.Start));
                LegalizeEdge(T, a_Vertex, a_Twin.OtherEdge(a_Edge.Twin, a_Edge.End));
            }
        }

        private static void Flip(Triangulation T, TriangleEdge a_Edge)
        {
            var a_Triangle = a_Edge.T;
            var a_Twin = a_Edge.OtherTriangle();

            if (a_Triangle == null || a_Twin == null)
            {
                throw new GeomException("Cannot flip edge if neighbouring triangles don't exist");
            }

            // Remove old triangles
            T.Remove(a_Triangle);
            T.Remove(a_Twin);

            var e0 = a_Triangle.OtherEdge(a_Edge, a_Edge.Start);
            var e2 = a_Twin.OtherEdge(a_Edge.Twin, a_Edge.Start);
            var e5 = a_Triangle.OtherEdge(a_Edge, a_Edge.End);
            var e3 = a_Twin.OtherEdge(a_Edge.Twin, a_Edge.End);

            var euv = new TriangleEdge(e0.End, e2.Start, null, null);
            var evu = new TriangleEdge(e2.Start, e0.End, euv, null);
            euv.Twin = evu;

            var t0 = new Triangle(e0, e2, evu);
            var t1 = new Triangle(e3, e5, euv);

            e0.T = e2.T = evu.T = t0;
            e3.T = e5.T = euv.T = t1;
        }
    }
}

