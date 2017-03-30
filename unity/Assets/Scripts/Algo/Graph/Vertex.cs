using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Algo.Graph
{
    public class Vertex
    {
        private Vector2 m_pos;
        private List<Edge> m_incidentEdges = new List<Edge>();

        public List<Edge> IncidentEdges { get { return m_incidentEdges; }  }
        public int Degree { get { return m_incidentEdges.Count; } }
        public Vector2 Pos { get { return m_pos; } }

        public Vertex(Vector2 a_pos)
        {
            m_pos = a_pos;
        }

        public Vertex(float x, float y)
        {
            m_pos = new Vector2(x, y);
        }

        internal static float Distance(Vertex vertex1, Vertex vertex2)
        {
            var dx = vertex1.m_pos.x - vertex2.m_pos.x;
            var dy = vertex1.m_pos.y - vertex2.m_pos.y;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        internal void AddIncidentEdge(Edge edge)
        {
            m_incidentEdges.Add(edge);
        }

        internal void RemoveIncidentEdge(Edge edge)
        {
            m_incidentEdges.Remove(edge);
        }
    }
}
