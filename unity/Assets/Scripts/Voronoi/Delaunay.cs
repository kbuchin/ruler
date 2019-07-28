using System;
using System.Collections.Generic;
using UnityEngine;

namespace Voronoi
{
    public sealed class Delaunay
    {
        private List<Triangle> m_Triangles = new List<Triangle>();
        private List<Vertex> m_Vertices = new List<Vertex>();
        private List<HalfEdge> m_HalfEdges = new List<HalfEdge>();
        private Material m_LineMaterial;

        public List<Triangle> Triangles { get { return m_Triangles; } }

        public List<HalfEdge> HalfEdges { get { return m_HalfEdges; } }

        public Material LineMaterial { get { return m_LineMaterial; } }

        public void Create()
        {
            const float farAway = 1000; // 500000
            Vertex v1 = new Vertex(-farAway, -farAway);
            Vertex v2 = new Vertex(farAway, farAway);
            Vertex v3 = new Vertex(-farAway, farAway);
            Vertex v4 = new Vertex(farAway, -farAway);
            m_Vertices.AddRange(new List<Vertex>() { v1, v2, v3, v4 });

            HalfEdge h1 = new HalfEdge(v1);
            HalfEdge h2 = new HalfEdge(v2);
            HalfEdge h3 = new HalfEdge(v3);
            m_HalfEdges.AddRange(new List<HalfEdge>() { h1, h2, h3 });

            h1.Next = h2;
            h2.Next = h3;
            h3.Next = h1;

            h2.Prev = h1;
            h3.Prev = h2;
            h1.Prev = h3;

            HalfEdge h4 = new HalfEdge(v2);
            HalfEdge h5 = new HalfEdge(v1);
            HalfEdge h6 = new HalfEdge(v4);
            m_HalfEdges.AddRange(new List<HalfEdge>() { h4, h5, h6 });

            h4.Twin = h1;
            h1.Twin = h4;

            h4.Next = h5;
            h5.Next = h6;
            h6.Next = h4;

            h5.Prev = h4;
            h6.Prev = h5;
            h4.Prev = h6;

            HalfEdge h7 = new HalfEdge(v1);
            HalfEdge h8 = new HalfEdge(v2);
            HalfEdge h9 = new HalfEdge(v3);
            HalfEdge h10 = new HalfEdge(v4);
            m_HalfEdges.AddRange(new List<HalfEdge>() { h7, h8, h9, h10 });

            h10.Next = h7;
            h7.Prev = h10;
            h8.Next = h10;
            h10.Prev = h8;
            h9.Next = h8;
            h8.Prev = h9;
            h7.Next = h9;
            h9.Prev = h7;

            h3.Twin = h7;
            h7.Twin = h3;
            h8.Twin = h6;
            h6.Twin = h8;
            h2.Twin = h9;
            h9.Twin = h2;

            h10.Twin = h5;
            h5.Twin = h10;

            m_Triangles.Add(new Triangle(h1));
            m_Triangles.Add(new Triangle(h4));

            CreateLineMaterial();
        }

        public bool AddVertex(Vertex a_Vertex)
        {
            Triangle triangle = FindTriangle(a_Vertex);
            if (triangle == null)
            {
                Debug.LogWarning("Couldn't place vertex in triangle; probably placed point colinear.");
                return false;
            }

            AddVertex(triangle, a_Vertex);

            // Find halfedges of triangle
            HalfEdge h1 = triangle.HalfEdge;
            HalfEdge h2 = triangle.HalfEdge.Next.Twin.Next;
            HalfEdge h3 = triangle.HalfEdge.Next.Twin.Next.Next.Twin.Next;

            // Flip if needed
            LegalizeEdge(a_Vertex, h1, h1.Triangle);
            LegalizeEdge(a_Vertex, h2, h2.Triangle);
            LegalizeEdge(a_Vertex, h3, h3.Triangle);

            return true;
        }

        private Triangle FindTriangle(Vertex a_Vertex)
        {
            foreach (Triangle triangle in m_Triangles)
            {
                if (triangle.InsideTriangle(a_Vertex))
                {
                    return triangle;
                }
            }
            return null;
        }

        private void AddVertex(Triangle a_Triangle, Vertex a_Vertex)
        {
            m_Vertices.Add(a_Vertex);

            m_Triangles.Remove(a_Triangle);

            HalfEdge h1 = a_Triangle.HalfEdge;
            HalfEdge h2 = h1.Next;
            HalfEdge h3 = h2.Next;

            HalfEdge h4 = new HalfEdge(h1.Origin);
            HalfEdge h5 = new HalfEdge(h2.Origin);
            HalfEdge h6 = new HalfEdge(h3.Origin);
            HalfEdge h7 = new HalfEdge(a_Vertex);
            HalfEdge h8 = new HalfEdge(a_Vertex);
            HalfEdge h9 = new HalfEdge(a_Vertex);
            m_HalfEdges.AddRange(new List<HalfEdge>() { h4, h5, h6, h7, h8, h9 });

            h4.Twin = h7;
            h7.Twin = h4;
            h5.Twin = h8;
            h8.Twin = h5;
            h6.Twin = h9;
            h9.Twin = h6;

            // Set all next
            h1.Next = h5;
            h5.Prev = h1;
            h5.Next = h7;
            h7.Prev = h5;
            h7.Next = h1;
            h1.Prev = h7;

            h2.Next = h6;
            h6.Prev = h2;
            h6.Next = h8;
            h8.Prev = h6;
            h8.Next = h2;
            h2.Prev = h8;

            h3.Next = h4;
            h4.Prev = h3;
            h4.Next = h9;
            h9.Prev = h4;
            h9.Next = h3;
            h3.Prev = h9;

            m_Triangles.Add(new Triangle(h1));
            m_Triangles.Add(new Triangle(h2));
            m_Triangles.Add(new Triangle(h3));
        }

        private void LegalizeEdge(Vertex a_Vertex, HalfEdge a_HalfEdge, Triangle a_Triangle)
        {
            // Points to test
            Vertex v1 = a_HalfEdge.Twin.Next.Next.Origin;
            Vertex v2 = a_HalfEdge.Next.Twin.Next.Next.Origin;
            Vertex v3 = a_HalfEdge.Next.Next.Twin.Next.Next.Origin;
            
            if (a_Triangle.InsideCircumcenter(v1) || a_Triangle.InsideCircumcenter(v2) || a_Triangle.InsideCircumcenter(v3))
            {
                HalfEdge h1 = a_HalfEdge.Twin.Next.Twin;
                HalfEdge h2 = a_HalfEdge.Twin.Prev.Twin;

                if (h1.Twin.Triangle != null && h2.Twin.Triangle != null)
                {
                    Flip(a_HalfEdge);

                    LegalizeEdge(a_Vertex, h1.Twin, h1.Twin.Triangle);
                    LegalizeEdge(a_Vertex, h2.Twin, h2.Twin.Triangle);
                }
                else
                {
                    throw new Exception("Tried to call LegalizeEdge on invalid triangle!");
                }
            }
        }

        private void Flip(HalfEdge a_HalfEdge)
        {
            HalfEdge h1 = a_HalfEdge;
            HalfEdge h2 = h1.Next;
            HalfEdge h3 = h2.Next;
            HalfEdge h4 = a_HalfEdge.Twin;
            HalfEdge h5 = h4.Next;
            HalfEdge h6 = h5.Next;

            if (h1.Triangle == null || h4.Triangle == null)
            {
                return;
            }
            
            // Remove old triangles
            m_Triangles.Remove(a_HalfEdge.Triangle);
            m_Triangles.Remove(a_HalfEdge.Twin.Triangle);

            h1.Next = h6;
            h6.Prev = h1;
            h6.Next = h2;
            h2.Prev = h6;
            h2.Next = h1;
            h1.Prev = h2;
            h1.Origin = h3.Origin;

            h4.Next = h3;
            h3.Prev = h4;
            h3.Next = h5;
            h5.Prev = h3;
            h5.Next = h4;
            h4.Prev = h5;
            h4.Origin = h6.Origin;

            m_Triangles.Add(new Triangle(h1));
            m_Triangles.Add(new Triangle(h1.Twin));
        }

        private void CreateLineMaterial()
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            m_LineMaterial = new Material(shader);
            m_LineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            m_LineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m_LineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            m_LineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            m_LineMaterial.SetInt("_ZWrite", 0);
        }
    }
}
