using MNMatrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using System.Collections.Generic;

namespace VoronoiDCEL
{
    public sealed class Face<T>
    {
        private HalfEdge<T> m_StartingEdge;
        // arbitrary halfedge as starting point for counter-clockwise traversal.
        private List<T> m_Data;

        public HalfEdge<T> StartingEdge
        {
            get { return m_StartingEdge; }
            set { m_StartingEdge = value; }
        }

        public T FirstData
        {
            get
            {
                if (m_Data != null && m_Data.Count > 0)
                {
                    return m_Data[0];
                }
                return default(T);
            }
            set
            {
                if (m_Data == null || m_Data.Count == 0)
                {
                    m_Data = new List<T>(1);
                }
                m_Data[0] = value;
            }
        }

        public T[] Data
        {
            get
            {
                if (m_Data != null && m_Data.Count > 0)
                {
                    return m_Data.ToArray();
                }
                return null;
            }
            set { m_Data = new List<T>(value); }
        }

        public Face()
        {
            m_Data = null;
        }

        public Face(T a_Data)
        {
            m_Data = new List<T>(1);
            m_Data.Add(a_Data);
        }

        public Face(T[] a_Data)
        {
            m_Data = new List<T>(a_Data.Length);
            m_Data.AddRange(a_Data);
        }

        public double ComputeSignedArea()
        {
            double result = 0;
            HalfEdge<T> i = m_StartingEdge;

            double[,] areaArray = new double[,]
            {
                { 0, 0 },
                { 0, 0 },
            };

            while (true)
            {
                areaArray[0, 0] = i.Origin.X;
                areaArray[0, 1] = i.Twin.Origin.X;
                areaArray[1, 0] = i.Origin.Y;
                areaArray[1, 1] = i.Twin.Origin.Y;
                MNMatrix areaMatrix = MNMatrix.Build.DenseOfArray(areaArray);
                result += areaMatrix.Determinant();
                i = i.Next;
                if (i == m_StartingEdge)
                {
                    break;
                }
            }
            return (result * 0.5d);
        }
    }
}

