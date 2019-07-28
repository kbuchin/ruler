namespace Util.Geometry.Polygon
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    struct BeginVertex
    {
        private readonly Vector2 m_vertex;
        private readonly LineSegment m_segment;

        internal LineSegment Segment { get { return m_segment; } }
        internal Vector2 Vertex { get { return m_vertex; } }

        internal BeginVertex(Vector2 a_vertex, LineSegment a_segment)
        {
            m_vertex = a_vertex;
            m_segment = a_segment;
        }
    }

    struct EndVertex
    {
        private readonly Vector2 m_vertex;
        private readonly LineSegment m_segment;

        internal LineSegment Segment { get { return m_segment; } }
        internal Vector2 Vertex { get { return m_vertex; } }


        internal EndVertex(Vector2 a_vertex, LineSegment a_segment)
        {
            m_vertex = a_vertex;
            m_segment = a_segment;
        }
    }


    /// <summary>
    /// A class representing a general Polygon (possibly with holes)
    /// </summary>
    public class Polygon2DWithHoles
    {

        Polygon2D m_outside;
        List<Polygon2D> m_holes;

        public Polygon2D Outside { get { return m_outside; } }

        public Polygon2DWithHoles(Polygon2D a_outside)
        {
            m_outside = a_outside;
            m_holes = new List<Polygon2D>();
        }

        public Polygon2DWithHoles(Polygon2D a_outside, List<Polygon2D> a_holes)
        {
            //TODO Check no overlap in holes (or does that take to much time?)
            m_outside = a_outside;
            m_holes = a_holes;
        }

        /// <summary>
        /// Computes the area of this polygon minus it's holes
        /// </summary>
        /// <returns></returns>
        public float Area()
        {
            var result = m_outside.Area();
            foreach (var hole in m_holes)
            {
                result -= hole.Area();
            }
            if (result < 0)
            {
                throw new GeomException("somehow ended up with negative area");
            }
            return result;
        }

        /// <summary>
        /// Returns the size of the area visible from a given point
        /// </summary>
        /// <param name="a_pos"></param>
        /// <returns></returns>
        public float VisibleArea(Vector2 a_pos)
        {
            return Vision(a_pos).Area();
        }

        /// <summary>
        /// Returns the vision area of a observer at a_pos
        /// </summary>
        /// NB Since we are working with polygons we know that the linesegments we are 
        /// working with won't cross
        /// <param name="a_pos">The position of the observer</param>
        /// <returns></returns>
        public Polygon2D Vision(Vector2 a_pos)
        {
            throw new NotSupportedException();
            /*
            // We will apply a sweepline algorithm

            // First Gather all line segments
            var linesegments = new List<LineSegment>();
            linesegments.AddRange(m_outside.Segments());
            foreach (var hole in m_holes)
            {
                linesegments.AddRange(hole.Segments());
            }

            //remove those segments collinear with the guard position. These disturb the algorithm and are unnecessary since the algorithm automatically continues at to the foremost wall.
            var removeSegments = new List<LineSegment>();
            //kevin: dist, minDist for debugging
            //double minDist = 10.0;
            //double dist;
            foreach (var seg in linesegments)
            {
                if (seg.Line.DistanceToPoint(a_pos) <= Mathf.Epsilon)
                {
                    removeSegments.Add(seg);
                }
                //dist = seg.Line.DistanceToPoint(a_pos);
                //if (dist < minDist) minDist = dist;
            }
            //Debug.Log("smallest distance: " + minDist);
            foreach (var seg in removeSegments)
            {
                //kevin: removing edges doesn't seem to solve the problem, so lets not do it for now
                //Debug.Log("removed a segment");
                //linesegments.Remove(seg);
            }

            //fill eventlist with begin and end points of segments. Also set up status structure.
            List<LineSegment> status = new List<LineSegment>(); //abuse sorted list, we are only interested in the key
            SimplePriorityQueue<BeginVertex> begin = new SimplePriorityQueue<BeginVertex>();
            SimplePriorityQueue<EndVertex> end = new SimplePriorityQueue<EndVertex>();
            foreach (var segment in linesegments)
            {
                //should it be in the status structure (when the segement crosses the positive x-axis ray from a_pos)
                if (segment.YInterval.Contains(a_pos.y) && segment.X(a_pos.y) > a_pos.x)
                {
                    status.Add(segment);
                }

                //normalize segment start points
                var v1 = segment.Point1 - a_pos;
                var v2 = segment.Point2 - a_pos;

                //angles  start at the positive x-axis and proceed counterclokwise
                var angle1 = Mathf.Atan2(v1.y, v1.x);
                var angle2 = Mathf.Atan2(v2.y, v2.x);

                //and are in the 0..2Pi range
                if (angle1 < 0) { angle1 += 2 * Mathf.PI; }
                if (angle2 < 0) { angle2 += 2 * Mathf.PI; }


                //we determine the begin and endpoint using the fact that the angle spanned by a line segment is never more then 180deg
                if (MathUtil.PositiveMod(angle2 - angle1, 2 * Mathf.PI) < Mathf.PI)
                {
                    if (angle1 == 0) { angle1 = Mathf.PI * 2; } //move begin events with angle 0 to 2Pi since they are already in the status

                    begin.Enqueue(new BeginVertex(segment.Point1, segment), angle1);
                    end.Enqueue(new EndVertex(segment.Point2, segment), angle2);
                }
                else
                {
                    if (angle2 == 0) { angle2 = Mathf.PI * 2; } //move begin events with angle 0 to 2Pi since they are already in the status
                    begin.Enqueue(new BeginVertex(segment.Point2, segment), angle2);
                    end.Enqueue(new EndVertex(segment.Point1, segment), angle1);
                }
            }


            //Start actual sweep
            var angle = 0f;
            var sweepline = new Line(a_pos, angle);
            var segmentComparer = new SegmentComparer(a_pos, sweepline);

            status.Sort(segmentComparer);
            var resultVertices = new List<Vector2>();

            while (begin.Count + end.Count > 0)
            {
                //update sweepline
                double beginPrio, endPrio;
                if (begin.Count > 0) { beginPrio = begin.LowestPriority(); } else { beginPrio = float.PositiveInfinity; }
                if (end.Count > 0) { endPrio = end.LowestPriority(); } else { endPrio = float.PositiveInfinity; }

                var newAngle = Mathf.Min((float)beginPrio, (float)endPrio);
                if (angle != newAngle)
                {
                    angle = newAngle;
                    sweepline = new Line(a_pos, angle);
                    segmentComparer.UpdateSweepline(sweepline);

                    //Strictly speaking this sort is unnecessary, however if two segments start at the same vertex we can have them in the wrong order. This solves that.
                    //kevin: I am not convinced
                    status.Sort(segmentComparer);
                }

                //prefer beginning an edge over ending edge (to prevent seeing through a vertex)
                if (endPrio < beginPrio)
                {
                    //end a segment
                    var endpoint = end.Dequeue();
                    if (status[0] == endpoint.Segment)
                    {
                        resultVertices.Add(endpoint.Vertex);
                        status.Remove(endpoint.Segment);
                        //determine the point where we need to continue now the previous segment terminated

                        var continuePoint = status[0].ForgivingIntersection(sweepline, a_pos);
                        resultVertices.Add(continuePoint);
                    }
                    else
                    {
                        status.Remove(endpoint.Segment);
                    }

                }
                else
                {
                    //begin a segment
                    var beginpoint = begin.Dequeue();

                    status.Add(beginpoint.Segment);


                    //did we add a segment in front of previous segment
                    status.Sort(segmentComparer);
                    if (status[0] == beginpoint.Segment)
                    {
                        resultVertices.Add(status[1].ForgivingIntersection(sweepline, a_pos)); //end of ray on previous segment
                        resultVertices.Add(beginpoint.Vertex);
                    }
                }
            }

            //Clear subsequent duplicates from resultvertices
            var i = 0;
            while (resultVertices.Count > i + 1)
            {
                if (resultVertices[i] == resultVertices[i + 1])
                {
                    resultVertices.RemoveAt(i + 1);
                }
                else
                {
                    i++;
                }
            }

            if (resultVertices[0] == resultVertices[resultVertices.Count - 1] && resultVertices.Count != 1)
            {
                resultVertices.RemoveAt(0);
            }


            //the sweepline yields a counterclockwise polygon (traitonal for increasing angle), we want out polygons to be oriented clockwise
            resultVertices.Reverse();
            return new Polygon2D(resultVertices);
            */
        }


        /// <summary>
        /// Returns whether a position is contained in the polygon
        /// </summary>
        /// <param name="a_pos"></param>
        /// <returns></returns>
        public bool Contains(Vector2 a_pos)
        {
            foreach (var hole in m_holes)
            {
                if (hole.Contains(a_pos))
                {
                    return false;
                }
            }

            if (m_outside.Contains(a_pos))
            {
                return true;
            }

            return false;
        }
    }

    internal static class LineSegmentExtension
    {

        /// <summary>
        /// An extension method that wiggles the intersectionline a bit, in order to find the correct intersection point.
        /// </summary>
        /// <param name="a_seg"></param>
        /// <param name="a_line"></param>
        /// <param name="a_pivot"></param>
        /// <returns></returns>
        internal static Vector2 ForgivingIntersection(this LineSegment a_seg, Line a_line, Vector2 a_pivot)
        {
            var intersection = a_seg.Intersect(a_line);

            if (intersection != null)
            {
                return intersection.Value;
            }


            var altLine = new Line(a_pivot, a_line.Angle - .0001f);

            intersection = a_seg.Intersect(altLine);
            if (intersection != null)
            {
                if (Vector2.Distance(intersection.Value, a_seg.Point1) < Vector2.Distance(intersection.Value, a_seg.Point2))
                {
                    return a_seg.Point1;
                }
                else
                {
                    return a_seg.Point2;
                }
            }

            altLine = new Line(a_pivot, a_line.Angle + .0001f);
            intersection = a_seg.Intersect(altLine);
            if (intersection == null)
            {
                throw new GeomException("No intersection");
            }
            if (Vector2.Distance(intersection.Value, a_seg.Point1) < Vector2.Distance(intersection.Value, a_seg.Point2))
            {
                return a_seg.Point1;
            }
            else
            {
                return a_seg.Point2;
            }
        }
    }

    internal class SegmentComparer : IComparer<LineSegment>
    {
        private Vector2 m_pos;
        private Line m_line;

        public SegmentComparer(Vector2 a_pos, Line a_sweepline)
        {
            m_pos = a_pos;
            m_line = a_sweepline;
        }

        public void UpdateSweepline(Line a_line)
        {
            m_line = a_line;
        }



        public int Compare(LineSegment seg1, LineSegment seg2)
        {
            var intersection1 = seg1.ForgivingIntersection(m_line, m_pos);
            var intersection2 = seg2.ForgivingIntersection(m_line, m_pos);

            return Vector2.Distance(m_pos, intersection1).CompareTo(Vector2.Distance(m_pos, intersection2));
        }
    }
}
