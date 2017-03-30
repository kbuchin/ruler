using System;
using System.Collections.Generic;
using UnityEngine;

namespace Algo  {
    public class Line : IComparable<Line>
        //Comparing lines sorts them on Slope
    {
        private Vector2 m_point1;
        private Vector2 m_point2;
        private bool m_oriented;

        public Vector2 Point1 { get { return m_point1; } }
        public Vector2 Point2 { get { return m_point2; } }
        
        /// <summary>
        /// Gives the angle of the line w.r.t the horizontal x-axis. reports in radians
        /// </summary>
        public float Angle { get { return (float) Math.Atan(Slope); } }

        /// <summary>
        /// Gives a 2D normal vector to this line
        /// </summary>
        public Vector2 Normal { get { return new Vector2(Slope, -1); } }

        public float HeightAtYAxis { get {
                if (m_point1.x == m_point2.x)
                {
                    return float.NaN;
                }
                else
                {
                    return m_point1.y - Slope * m_point1.x ;
                }
            } }
        public float Slope { get
            {
                if (m_point1.x == m_point2.x)
                {
                    return float.PositiveInfinity;
                }
                else
                {
                    return (m_point1.y - m_point2.y) / (m_point1.x - m_point2.x);
                }
            }}


        public Line(Vector2 a_point1, Vector2 a_point2)
        {
            m_point1 = a_point1;
            m_point2 = a_point2;
            m_oriented = true;
        }

        public Line(float a_slope, float a_heigthatyaxis)
        {
            m_point1 = new Vector2(0, a_heigthatyaxis);
            m_point2 = new Vector2(10, a_heigthatyaxis + 10 * a_slope);
            m_oriented = false;
        }


        /// <summary>
        /// Create line going trough a certain point and with a certian angle wrt to the positive x-axis
        /// </summary>
        /// <param name="a_point"></param>
        /// <param name="a_angle"></param>
        public Line(Vector2 a_point, float a_angle)
        {
            m_point1 = a_point;
            m_point2 = a_point + new Vector2(Mathf.Cos(a_angle), Mathf.Sin(a_angle))    ;
        }

        public float X(float a_y)
        {
            if (Slope == 0)
            {
                throw new AlgoException("Method not supported for horizontal lines");
            }
            if (float.IsInfinity(Slope))
            {
                return m_point1.x;
            }
            return (a_y - HeightAtYAxis) / Slope;
        }

        public float Y(float a_x)
        {
            if (float.IsInfinity(Slope)) { 
                throw new AlgoException("Method not supported for vertical lines");
            }
            return HeightAtYAxis + Slope * a_x;
        }

        public static Vector2 Intersect(Line a_line1, Line a_line2)
        {
            if( float.IsInfinity(a_line1.Slope) && float.IsInfinity(a_line2.Slope))
            {
                throw new AlgoException("Two vertical lines ");
            }

            if (! (float.IsInfinity(a_line1.Slope) || float.IsInfinity(a_line2.Slope) ) )
            {
                var dy = a_line1.HeightAtYAxis - a_line2.HeightAtYAxis;
                var comparitiveslope = a_line1.Slope - a_line2.Slope;
                var x = -dy / comparitiveslope;
                return new Vector2(x, a_line1.Y(x));
            }
            else
            {
                Line verticalLine;
                Line normalLine;
                if (float.IsInfinity(a_line1.Slope))
                {
                    verticalLine = a_line1;
                    normalLine = a_line2;
                }
                else
                {
                    verticalLine = a_line2;
                    normalLine = a_line1;
                }
                var x = verticalLine.m_point1.x;
                return new Vector2(x, normalLine.Y(x));
            }
        }

        public Vector2 Intersect(Line a_otherline)
        {
            return Intersect(this, a_otherline);
        }

        public int CompareTo(Line a_other)
        {
            return Slope.CompareTo(a_other.Slope);
        }

        internal bool PointAbove(Vector2 a_point)
        {
            //Returns true when point is above line (or left in the vertical case)
            if (float.IsInfinity(Slope))
            {
                return a_point.x < m_point1.x;
            }
            else
            {
                var liney = Y(a_point.x);
                return a_point.y > liney;
            }


        }

        internal int NumberOfPointsAbove(List<Vector2> a_points)
        {
            var result = 0;
            foreach (Vector2 point in a_points)
            {
                if (PointAbove(point))
                {
                    result++;
                }
            }
            return result;
        }

        internal bool PointRightOfLine(Vector2 a_point)
        {
            if (!m_oriented)
            {
                throw new AlgoException("Can't test rightness on unoriented line");
            }

            if (float.IsInfinity(Slope))
            {
                if (m_point2.y < m_point1.y)
                {
                    //edge oriented top-down, so right of edge is smaller x
                    return a_point.x < m_point1.x;
                }
                else
                {
                    return a_point.x > m_point1.x;
                }
            }

            //consider orientation
            if (m_point1.x < m_point2.x)
            {
                //left-right orientation, right of the edge is below it
                return !PointAbove(a_point);
            }
            else 
            {
                //right-left orientation, right of the edge is above it
                return PointAbove(a_point);
            }
        }
        
        /// <summary>
        /// Computes the distance between this line and the given point 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        internal float DistanceToPoint(Vector2 v)
        {
            var normalLine = new Line(v, v + Normal);
            var intersection = Intersect(normalLine);
            return Vector2.Distance(v, intersection);
        }
    }
}