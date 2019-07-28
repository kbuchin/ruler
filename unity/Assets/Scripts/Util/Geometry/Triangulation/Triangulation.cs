using Util.Geometry.Graph;

namespace Util.Geometry.Triangulation
{
    using System.Linq;
    using System.Collections.Generic;
    using UnityEngine;

    public class Triangulation
    {
        private readonly LinkedList<Triangle> m_Triangles;
        private readonly LinkedList<TriangleEdge> m_Edges;

        public ICollection<Triangle> Triangles { get { return m_Triangles; } }

        public ICollection<TriangleEdge> Edges { get { return m_Edges; } }

        private readonly Vertex V0, V1, V2;

        public Triangulation()
        {
            m_Triangles = new LinkedList<Triangle>();
        }

        public Triangulation(Vertex v0, Vertex v1, Vertex v2)
        {
            m_Triangles = new LinkedList<Triangle>(new Triangle[]
            {
                new Triangle(v0, v1, v2)
            });
            V0 = v0;
            V1 = v1;
            V2 = v2;
        }

        public void Add(Triangle t)
        {
            Triangles.Add(t);
        }

        public void Add(IEnumerable<Triangle> triangles)
        {
            foreach (var t in triangles) Add(t);
        }

        public void Remove(Triangle t)
        {
            Triangles.Remove(t);
        }

        public void Clear()
        {
            Triangles.Clear();
        }

        public void RemoveInitialTriangle()
        {
            var ToRemove = Triangles
                .Where(t => t.ContainsEndpoint(V0) || t.ContainsEndpoint(V1) || t.ContainsEndpoint(V2));

            foreach (var t in ToRemove)
                Triangles.Remove(t);
        }
    }
}