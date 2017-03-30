using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Algo.Graph
{
    public class Edge
    {
        private Vertex m_vertex1;
        private Vertex m_vertex2;

        /// <summary>
        /// Creates an edge, when removing the edge cleanup should be called
        /// </summary>
        /// <param name="a_vertex1"></param>
        /// <param name="a_vertex2"></param>
        public Edge(Vertex a_vertex1, Vertex a_vertex2, Graph a_graph)
        {
            m_vertex1 = a_vertex1;
            m_vertex2 = a_vertex2;
            a_vertex1.AddIncidentEdge(this);
            a_vertex2.AddIncidentEdge(this);
        }

        public float Length { get
            {
                return Vertex.Distance(m_vertex1, m_vertex2);
            }
        }

        public Vertex Vertex1
        {
            get
            {
                return m_vertex1;
            }
        }

        public Vertex Vertex2
        {
            get
            {
                return m_vertex2;
            }
        }

        /// <summary>
        /// After calling cleanup a edge can be safely removed
        /// </summary>
        internal void CleanUp()
        {
            m_vertex1.RemoveIncidentEdge(this);
            m_vertex2.RemoveIncidentEdge(this);
        }
        
        /// <summary>
        /// Returns the other vertex in this edge (i.e. when provided with Vertex1 it returns Vertex2)
        /// </summary>
        internal Vertex OtherVertex(Vertex a_vertex)
        {
            if (a_vertex == Vertex1)
            {
                return Vertex2;
            }
            else if (a_vertex == Vertex2)
            {
                return Vertex1;
            } else
            {
                throw new AlgoException("Unexpected argument: " + a_vertex);
            }
        }
    }
}
