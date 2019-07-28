using System;
using UnityEngine;
using MNMatrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using MNVector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace Voronoi
{
    public sealed class Triangle
    {
        private bool m_Drawn = false;
        private Vertex m_Circumcenter;
        private bool m_CalculatedCircumcenter = false;
        private float circumcenterRangeSquared;
        private bool calculatedCircumcenterRangeSquared = false;
        private readonly Vertex[] m_Vertices;
        private Color m_Color;
        private HalfEdge m_HalfEdge;

        public Vertex[] Vertices { get { return m_Vertices; } }

        public HalfEdge HalfEdge { get { return m_HalfEdge; } }

        public Color Color { get { return m_Color; } }

        public bool Drawn
        {
            get { return m_Drawn; }
            set { m_Drawn = value; }
        }

        public Triangle(HalfEdge a_HalfEdge)
        {
            m_Vertices = new Vertex[3];
            m_HalfEdge = a_HalfEdge;
            m_Color = Color.red;

            Vertex v1 = m_HalfEdge.Origin;
            Vertex v2 = m_HalfEdge.Next.Origin;
            Vertex v3 = m_HalfEdge.Next.Next.Origin;
            Vertex v4 = m_HalfEdge.Next.Next.Next.Origin;

            if (v1 == v2 || v2 == v3 || v1 == v3 || v1 != v4)
            {
                throw new Exception("Triangle does not have a correct 3 vertex loop.");
            }

            if (v1.IsInvalid() || v2.IsInvalid() || v3.IsInvalid())
            {
                throw new Exception("Invalid triangle vertex coordinates!");
            }

            HalfEdge h1 = m_HalfEdge;
            HalfEdge h2 = m_HalfEdge.Next.Twin.Next;
            HalfEdge h3 = m_HalfEdge.Next.Twin.Next.Next.Twin.Next;

            if (h1 == null || h2 == null || h3 == null)
            {
                throw new Exception("Associated halfedges of triangle are invalid!");
            }

            // Fix halfedges to this triangle
            m_HalfEdge.Triangle = this;
            m_HalfEdge.Next.Triangle = this;
            m_HalfEdge.Next.Next.Triangle = this;

            // Add vertices to the array
            m_Vertices[0] = v1;
            m_Vertices[1] = v2;
            m_Vertices[2] = v3;
        }

        private Vertex CalculateCircumcenter()
        {
            Vertex a = m_Vertices[0];
            Vertex b = m_Vertices[1];
            Vertex c = m_Vertices[2];

            double[,] numerator = new double[,]
            {
                { Mathf.Pow(a.X - c.X, 2) + Mathf.Pow(a.Y - c.Y, 2), a.Y - c.Y },
                { Mathf.Pow(b.X - c.X, 2) + Mathf.Pow(b.Y - c.Y, 2), b.Y - c.Y }
            };
            double[,] denomenator = new double[,]
            {
                { a.X - c.X, a.Y - c.Y },
                { b.X - c.X, b.Y - c.Y }
            };

            MNMatrix numeratorMatrix = MNMatrix.Build.DenseOfArray(numerator);
            MNMatrix denomenatorMatrix = MNMatrix.Build.DenseOfArray(denomenator);
            double numeratorDeterminant = numeratorMatrix.Determinant();
            double denomenatorDeterminant = denomenatorMatrix.Determinant();
            double Ox = c.X + numeratorDeterminant / (2 * denomenatorDeterminant);

            numerator = new double[,]
            {
                { a.X - c.X, Mathf.Pow(a.X - c.X, 2) + Mathf.Pow(a.Y - c.Y, 2) },
                { b.X - c.X, Mathf.Pow(b.X - c.X, 2) + Mathf.Pow(b.Y - c.Y, 2) }
            };

            numeratorMatrix = MNMatrix.Build.DenseOfArray(numerator);
            numeratorDeterminant = numeratorMatrix.Determinant();

            double Oy = c.Y + numeratorDeterminant / (2 * denomenatorDeterminant);

            Vertex circumCenter = new Vertex((float)Ox, (float)Oy);
            if (!circumCenter.IsInvalid())
            {
                return circumCenter;
            }
            else
            {
                throw new Exception("Result of CalculateCircumcenterStable was invalid!");
            }
        }

        public bool InsideTriangle(Vertex a_Vertex)
        {
            int firstSide = Math.Sign(Orient2D(m_HalfEdge.Origin, m_HalfEdge.Next.Origin, a_Vertex));
            int secondSide = Math.Sign(Orient2D(m_HalfEdge.Next.Origin, m_HalfEdge.Next.Next.Origin, a_Vertex));
            int thirdSide = Math.Sign(Orient2D(m_HalfEdge.Next.Next.Origin, m_HalfEdge.Origin, a_Vertex));
            return (firstSide != 0 && firstSide == secondSide && secondSide == thirdSide);
        }

        private bool InCircle(Vertex a, Vertex b, Vertex c, Vertex d)
        {
            int orientation = Math.Sign(Orient2D(a, b, c));
            if (orientation == 0)
            {
                Debug.LogWarning("Tried to compute InCircle on degenerate circle");
                return false;
            }

            double[,] inCircleArray = new double[,]
            {
                { a.X - d.X, a.Y - d.Y, Mathf.Pow(a.X - d.X, 2) + Mathf.Pow(a.Y - d.Y, 2) },
                { b.X - d.X, b.Y - d.Y, Mathf.Pow(b.X - d.X, 2) + Mathf.Pow(b.Y - d.Y, 2) },
                { c.X - d.X, c.Y - d.Y, Mathf.Pow(c.X - d.X, 2) + Mathf.Pow(c.Y - d.Y, 2) },
            };

            MNMatrix inCircleMatrix = MNMatrix.Build.DenseOfArray(inCircleArray);
            int inside = Math.Sign(inCircleMatrix.Determinant());

            if (inside == 0)
            {
                return false;
            }
            else
            {
                return orientation == inside;
            }
        }

        // returns a positive value if the points a, b, and c are arranged in
        // counterclockwise order, a negative value if the points are in clockwise order,
        // and zero if the points are collinear.
        private double Orient2D(Vertex a, Vertex b, Vertex c)
        {
            double[,] orientArray = new double[,]
            {
                { a.X - c.X, a.Y - c.Y },
                { b.X - c.X, b.Y - c.Y }
            };

            MNMatrix orientMatrix = MNMatrix.Build.DenseOfArray(orientArray);
            return orientMatrix.Determinant();
        }

        public bool InsideCircumcenter(Vertex a_Vertex)
        {
            return InCircle(m_Vertices[0], m_Vertices[1], m_Vertices[2], a_Vertex);
        }

        public Vertex Circumcenter
        {
            get
            {
                if (m_CalculatedCircumcenter)
                {
                    return m_Circumcenter;
                }

                m_Circumcenter = CalculateCircumcenter();
                m_CalculatedCircumcenter = true;

                return m_Circumcenter;
            }
        }

        public float CircumcenterRangeSquared
        {
            get
            {
                if (calculatedCircumcenterRangeSquared)
                {
                    return circumcenterRangeSquared;
                }

                circumcenterRangeSquared = m_Vertices[0].DeltaSquaredXY(Circumcenter);
                calculatedCircumcenterRangeSquared = true;

                return circumcenterRangeSquared;
            }
        }
    }
}