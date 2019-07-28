namespace Util.Geometry
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class LineSegment
    {
        protected Vector2 m_point1;
        protected Vector2 m_point2;

        public Vector2 Point1 { get { return m_point1; } }
        public Vector2 Point2 { get { return m_point2; } }

        public Line Line { get { return new Line(m_point1, m_point2); } }

        public bool IsVertical { get { return m_point1.x == m_point2.x; } }

        public float Magnitude
        {
            get { return (Point2 - Point1).magnitude; }
        }
        public float SqrMagnitude
        {
            get { return (Point2 - Point1).sqrMagnitude; }
        }

        public LineSegment(Vector2 a_point1, Vector2 a_point2)
        {
            m_point1 = a_point1;
            m_point2 = a_point2;
        }

        public bool IsRightOf(Vector2 a_point)
        {
            var line = new Line(m_point1, m_point2);
            return line.PointRightOfLine(a_point);
        }

        public FloatInterval XInterval
        {
            get { return new FloatInterval(m_point1.x, m_point2.x); }
        }

        public FloatInterval YInterval
        {
            get { return new FloatInterval(m_point1.y, m_point2.y); }
        }

        /// <summary>
        /// Returns the unique intersection between a_seg1 and a_seg2. If there is one, if a_seg1 and a_seg2 coincide for a part we return 0
        /// </summary>
        /// <param name="a_seg1"></param>
        /// <param name="a_seg2"></param>
        /// <returns></returns>
        public static Vector2? Intersect(LineSegment a_seg1, LineSegment a_seg2)
        {
            FloatInterval intervalXIntersection = a_seg1.XInterval.Intersect(a_seg2.XInterval);
            FloatInterval intervalYIntersection = a_seg1.YInterval.Intersect(a_seg2.YInterval);
            if (intervalXIntersection == null || intervalYIntersection == null)
            {
                return null;
            }

            if (a_seg1.Line.Slope == a_seg2.Line.Slope)
            {
                return null;
            }

            var intersectionpoint = Line.Intersect(a_seg1.Line, a_seg2.Line);
            if (intervalXIntersection.ContainsEpsilon(intersectionpoint.x) && intervalYIntersection.ContainsEpsilon(intersectionpoint.y))
            {
                return intersectionpoint;
            }
            return null;
        }


        public static Vector2? Intersect(LineSegment a_seg, Line a_line)
        {
            // cf Interset(LineSegment, LineSegment)
            if (a_seg.Line.Slope == a_line.Slope)
            {
                return null;
            }

            var intersectionpoint = Line.Intersect(a_seg.Line, a_line);
            if (a_seg.XInterval.Contains(intersectionpoint.x) && a_seg.YInterval.ContainsEpsilon(intersectionpoint.y)) //Double check to handle single vertical segments
            {
                return intersectionpoint;
            }
            return null;
        }

        internal Vector2? Intersect(LineSegment a_seg)
        {
            return Intersect(this, a_seg);
        }

        internal Vector2? Intersect(Line a_line)
        {
            return Intersect(this, a_line);
        }

        /// <summary>
        /// returns a list of intersections with the given segments. Sorted with the one closest to point1 on top.
        /// </summary>
        /// <param name="a_segments"></param>
        /// <returns></returns>
        public List<Vector2> IntersectionWithSegments(List<LineSegment> a_segments)
        {
            List<Vector2> intersections = new List<Vector2>();

            //find all intersections
            foreach (LineSegment segment in a_segments)
            {
                Vector2? intersection = Intersect(segment);
                if (intersection.HasValue)
                {
                    intersections.Add(intersection.Value);
                }
            }

            //sort them
            intersections.Sort(ClosestToPoint1Comparer);


            return intersections;
        }


        private int ClosestToPoint1Comparer(Vector2 a_1, Vector2 a_2)
        {
            var dist_1 = Vector2.Distance(Point1, a_1);
            var dist_2 = Vector2.Distance(Point1, a_2);

            return dist_1.CompareTo(dist_2);
        }


        /// <summary>
        /// The normal to the right of this segment if we view it as oriented from point1 to point2
        /// </summary>
        /// <returns></returns>
        public Vector2 RightNormal()
        {
            var ori = Orientation();
            return new Vector2(ori.y, -ori.x);
        }

        /// <summary>
        /// The orientation vector of this line segment if we view it as oriented from point1 to point2
        /// </summary>
        /// <returns></returns>
        public Vector2 Orientation()
        {
            return m_point2 - m_point1;
        }

        /// <summary>
        /// Returns X-value on the line segement corresponding to the given y value
        /// </summary>
        /// <param name="a_y"></param>
        /// <returns></returns>
        public float X(float a_y)
        {
            if (YInterval.Contains(a_y))
            {
                return Line.X(a_y);
            }
            else
            {
                throw new GeomException("X-value requested for y:" + a_y + "not in Y-interval" + YInterval.ToString());
            }
        }

        /// <summary>
        ///  Returns Y-value on the line segement corresponding to the given x value
        /// </summary>
        /// <param name="a_x"></param>
        /// <returns></returns>
        public float Y(float a_x)
        {
            if (XInterval.Contains(a_x))
            {
                return Line.Y(a_x);
            }
            else
            {
                throw new GeomException("Y-value requested for x:" + a_x + "not in x-interval" + XInterval.ToString());
            }
        }
    }
}
