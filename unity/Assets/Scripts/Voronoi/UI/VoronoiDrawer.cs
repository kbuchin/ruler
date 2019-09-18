namespace Voronoi.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Geometry.Triangulation;

    public static class VoronoiDrawer
    {
        public static bool CircleOn { get; set; }
        public static bool EdgesOn { get; set; }
        public static bool VoronoiOn { get; set; }

        private static Material m_lineMaterial;

        public static void CreateLineMaterial()
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            m_lineMaterial = new Material(shader);
            m_lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            m_lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m_lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            m_lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            m_lineMaterial.SetInt("_ZWrite", 0);
        }

        private static void DrawEdges(Triangulation m_Delaunay)
        {
            GL.Color(Color.green);
            GL.Begin(GL.LINES);

            foreach (var halfEdge in m_Delaunay.Edges)
            {
                // dont draw edges to outer vertices
                if (m_Delaunay.ContainsInitialPoint(halfEdge.T))
                {
                    continue;
                }

                GL.Vertex3(halfEdge.Point1.x, 0, halfEdge.Point1.y);
                GL.Vertex3(halfEdge.Point2.x, 0, halfEdge.Point2.y);
            }
            GL.End();
        }

        private static void DrawCircles(Triangulation m_Delaunay)
        {
            GL.Begin(GL.LINES);

            //const float extra = (360 / 100);

            foreach (Triangle triangle in m_Delaunay.Triangles)
            {
                // dont draw circles for triangles to outer vertices
                if (m_Delaunay.ContainsInitialPoint(triangle))
                {
                    continue;
                }

                //GL.Color(triangle.Color);
                var radius = (triangle.Circumcenter - triangle.P0).magnitude;
                var prevA = 0f;
                for (var a = 0f; a <= 2 * Mathf.PI; a += 0.05f)
                {
                    //the circle.
                    GL.Vertex3(Mathf.Cos(prevA) * radius + triangle.Circumcenter.x, 0, Mathf.Sin(prevA) * radius + triangle.Circumcenter.y);
                    GL.Vertex3(Mathf.Cos(a) * radius + triangle.Circumcenter.x, 0, Mathf.Sin(a) * radius + triangle.Circumcenter.y);

                    //midpoint of the circle.
                    GL.Vertex3(Mathf.Cos(prevA) * 0.1f + triangle.Circumcenter.x, 0, Mathf.Sin(prevA) * 0.1f + triangle.Circumcenter.y);
                    GL.Vertex3(Mathf.Cos(a) * 0.1f + triangle.Circumcenter.x, 0, Mathf.Sin(a) * 0.1f + triangle.Circumcenter.y);

                    prevA = a;
                }
            }

            GL.End();
        }

        private static void DrawVoronoi(Triangulation m_Delaunay)
        {
            GL.Begin(GL.LINES);
            foreach (var halfEdge in m_Delaunay.Edges)
            {
                if (m_Delaunay.ContainsInitialPoint(halfEdge.T))
                {
                    continue;
                }

                Triangle t1 = halfEdge.T;
                Triangle t2 = halfEdge.Twin.T;

                if (t1 != null && t2 != null)
                {
                    var v1 = t1.Circumcenter;
                    var v2 = t2.Circumcenter;

                    GL.Vertex3(v1.x, 0, v1.y);
                    GL.Vertex3(v2.x, 0, v2.y);
                }
            }
            GL.End();
        }

        public static void Draw(Triangulation m_Delaunay)
        {
            m_lineMaterial.SetPass(0);

            if (EdgesOn)
            {
                DrawEdges(m_Delaunay);
            }

            if (CircleOn)
            {
                DrawCircles(m_Delaunay);
            }

            if (VoronoiOn)
            {
                DrawVoronoi(m_Delaunay);
            }
        }
    }
}
