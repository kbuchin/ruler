using System;
using System.Collections.Generic;
using MNMatrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace VoronoiDCEL
{
    public sealed class Vertex<T> : IComparable<Vertex<T>>, IEquatable<Vertex<T>>
    {
        private readonly double m_x;
        private readonly double m_y;
        private readonly List<HalfEdge<T>> m_IncidentEdges;
        private static readonly double m_Tolerance = Math.Exp(-9);
        // IncidentEdges must have this vertex as origin.

        public Vertex(double a_x, double a_y)
        {
            m_x = a_x;
            m_y = a_y;
            m_IncidentEdges = new List<HalfEdge<T>>();
        }

        public static Vertex<T> Zero
        {
            get { return new Vertex<T>(0, 0); }
        }

        public List<HalfEdge<T>> IncidentEdges
        {
            get { return m_IncidentEdges; }
        }

        public double X
		{ get { return m_x; } }

        public double Y
		{ get { return m_y; } }

        public bool OnLine(Edge<T> a_Edge)
        {
            return Math.Abs(Orient2D(this, a_Edge.LowerEndpoint, a_Edge.UpperEndpoint)) < m_Tolerance;
        }

        public bool LeftOfLine(Edge<T> a_Edge)
        {
            return Orient2D(this, a_Edge.LowerEndpoint, a_Edge.UpperEndpoint) > 0;
        }

        public bool RightOfLine(Edge<T> a_Edge)
        {
            return Orient2D(this, a_Edge.LowerEndpoint, a_Edge.UpperEndpoint) < 0;
        }

        public int CompareTo(Edge<T> a_Edge)
        {
            double result = Orient2D(this, a_Edge.LowerEndpoint, a_Edge.UpperEndpoint);
            if (Math.Abs(result) < m_Tolerance)
            {
                return 0;
            }
            else if (result < 0)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Vertex<T>);
        }

        public bool Equals(Vertex<T> a_Vertex)
        {
            if (a_Vertex != null)
            {
                return Math.Abs(m_y - a_Vertex.Y) < m_Tolerance && Math.Abs(m_x - a_Vertex.X) < m_Tolerance;
            }
            else
            {
                return false;
            }
        }

        public int CompareTo(Vertex<T> a_Vertex)
        {
            if (Math.Abs(m_y - a_Vertex.Y) < m_Tolerance)
            {
                if (Math.Abs(m_x - a_Vertex.X) < m_Tolerance)
                {
                    return 0;
                }
                else if (m_x < a_Vertex.X)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            else if (m_y > a_Vertex.Y)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + m_x.GetHashCode();
                hash = hash * 23 + m_y.GetHashCode();
                return hash;
            }
        }

        public static double Orient2D(Vertex<T> a, Vertex<T> b, Vertex<T> c)
        {
            double[,] orientArray = new double[,]
            {
                { a.X - c.X, a.Y - c.Y },
                { b.X - c.X, b.Y - c.Y }
            };

            MNMatrix orientMatrix = MNMatrix.Build.DenseOfArray(orientArray);
            return orientMatrix.Determinant();
        }

        public override string ToString()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("(");
            builder.Append(m_x);
            builder.Append(",");
            builder.Append(m_y);
            builder.Append(") ");
            builder.Append("Nr incident edges: ");
            builder.Append(m_IncidentEdges.Count);
            return builder.ToString();
        }
    }
}

