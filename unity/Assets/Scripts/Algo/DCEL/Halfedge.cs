using System;
using UnityEngine;
using Algo;

namespace Algo.DCEL
{

    public class Halfedge:LineSegment
    {
        private Halfedge m_twin;
        private Halfedge m_prev;
        private Halfedge m_next;
        private Face m_face;

        public float lengthSquared
        {
            get
            {
                var dx = m_point1.x - m_point2.x;
                var dy = m_point1.y - m_point2.y;
                return (dx * dx + dy * dy);
            }
        }

        public Face Face { get { return m_face; } internal set { m_face = value; } }
        public Halfedge Next { get { return m_next; } internal set { m_next = value; }  }
        public Halfedge Prev { get { return m_prev; } internal set { m_prev = value; } }
        public Vector2 Fromvertex { get { return m_point1; } internal set { m_point1 = value; } }
        public Vector2 Tovertex { get { return m_point2; } internal set { m_point2 = value; } }
        public Halfedge Twin { get { return m_twin; } internal set { m_twin = value; } }

        public Halfedge(Vector2 a_fromvertex, Vector2 a_tovertex):base(a_fromvertex, a_tovertex)
        {
            if (lengthSquared == 0)
            {
                throw new AlgoException("creating edge of length zero");
            }
        }

        public PositionOnEdge intersectLine(Line a_line)
        {
            //returns intersection point (if any) otherwise returns null
            if (m_point2.x != m_point1.x)
            {
                //create line for edge
                var slope = (m_point1.y - m_point2.y) / (m_point1.x - m_point2.x);
                var heightatyaxis = m_point1.y - slope * m_point1.x;
                var edgeline = new Line(slope, heightatyaxis);

                var intersection = a_line.Intersect(edgeline);

                var dy = Math.Abs(m_point1.y - m_point2.y);
                var dx = Math.Abs(m_point1.x - m_point2.x);
        

                if (dx > dy)
                {
                    if (XInterval.Contains(intersection.x))
                    {
                        return new PositionOnEdge(intersection, this);
                      }
                }
                else {
                    if (YInterval.Contains(intersection.y))
                    {
                        return new PositionOnEdge(intersection, this);
                      }
                }
                return null;
            }
            else {
                var edgex = m_point1.x;
                var liney = a_line.Y(edgex);
                if (YInterval.Contains(liney)) 
                {
                    var pos = new Vector2(edgex, liney);
                    return new PositionOnEdge(pos, this);
                }
                return null;
            }


        }

        public override string ToString()
        {
            return "From: " + m_point1.ToString() + " To: " + m_point2.ToString();
        }

    }
}