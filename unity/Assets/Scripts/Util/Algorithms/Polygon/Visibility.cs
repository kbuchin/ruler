namespace Util.Algorithms.Polygon
{
    using System;
    using Util.Geometry;
    using Util.Geometry.Polygon;
    using System.Linq;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Math;
    using Util.DataStructures.BST;

    public static class Visibility
    {
        internal static Vector2 VisionPos;

        private static readonly Polygon2D VisibilityPolygon = new Polygon2D();
        private static StatusLine TopStatus;

        public static Polygon2D Vision(Polygon2D polygon, Vector2 a_pos)
        {
            return Vision(new Polygon2DWithHoles(polygon), a_pos);
        }

        public static Polygon2D Vision(Polygon2DWithHoles polygon, Vector2 a_pos)
        {
            if (!polygon.Contains(a_pos))
            {
                throw new ArgumentException("Point should be contained inside polygon");
            }

            // clear old visibility polygon
            VisibilityPolygon.Clear();
            TopStatus = null;
            VisionPos = a_pos;

            // First Gather all line segments
            var linesegments = new List<LineSegment>();
            linesegments.AddRange(polygon.Outside.Segments);
            foreach (var hole in polygon.Holes)
            {
                linesegments.AddRange(hole.Segments);
            }
            var statuslines = new List<StatusLine>();
            foreach (var l in linesegments) statuslines.Add(new StatusLine(l));
            
            // create SweepLine object
            var SweepLine = new SweepLine<StatusLine>(new Line(a_pos, a_pos + new Vector2(1f, 0f)));

            // create initial status
            var initLines = new HashSet<StatusLine>(
                statuslines
                    .FindAll(item => 
                        !item.Segment.IsHorizontal &&
                        item.Segment.YInterval.ContainsEpsilon(a_pos.y) && 
                        item.Segment.X(a_pos.y) > a_pos.x &&
                        (item.Segment.Point1.y < a_pos.y || item.Segment.Point2.y < a_pos.y))
                    .ToList()
            );
            var initStatus = new List<StatusLine>();
            foreach (var item in initLines) initStatus.Add(item);

            // add initial lines that cross the x-axis to the right of a_pos
            var status = SweepLine.InitializeStatus(initStatus);

            // initialize top segment
            if (!status.FindMin(out TopStatus))
            {
                throw new GeomException("Status should always contain some line segment.");
            }

            //fill eventlist with begin and end points of segments.
            var events = new List<ISweepEvent<StatusLine>>();
            foreach (var item in statuslines)
            {
                var ev1 = new SweepSegment(item, item.Segment.Point1);
                var ev2 = new SweepSegment(item, item.Segment.Point2);

                bool start1 = initLines.Contains(item) ? (ev1.CompareTo(ev2) > 0) : (ev1.CompareTo(ev2) < 0);

                ev1.SetStart(start1);
                ev2.SetStart(!start1);

                events.Add(ev1);
                events.Add(ev2);
            }

            // insert all events
            SweepLine.InitializeEvents(events);

            // perform the radial sweep
            SweepLine.RadialSweep(a_pos, HandleEvent);

            // reverse visibility polygon to make clockwise
            VisibilityPolygon.Reverse();

            return VisibilityPolygon;
        }

        static void HandleEvent(IBST<ISweepEvent<StatusLine>> events, IBST<StatusLine> status, ISweepEvent<StatusLine> ev)
        {
            var sweepEv = ev as SweepSegment;
            if(sweepEv == null)
            {
                throw new GeomException("Event should be of type SweepSegment");
            }

            Debug.Log(sweepEv);

            StatusLine minItem;
            if (!status.FindMin(out minItem))
            {
                throw new GeomException("Status should always contain some line segment.");
            }

            if (minItem.Segment != TopStatus.Segment)
            {
                // find closest intersection point on sweepline
                Vector2? p1 = IntersectionOrHorizontal(SweepLine<StatusLine>.Line, TopStatus.Segment);
                Vector2? p2 = IntersectionOrHorizontal(SweepLine<StatusLine>.Line, minItem.Segment);

                if (p1 == null || p2 == null)
                {
                    throw new GeomException("Segments should intersect sweep line");
                }

                // insert intersection with old top segment and new one
                // avoid adding a vertex twice
                if (VisibilityPolygon.VertexCount == 0 || VisibilityPolygon.Vertices.Last() != p1)
                    VisibilityPolygon.AddVertex((Vector2)p1);
                if (VisibilityPolygon.VertexCount == 0 || VisibilityPolygon.Vertices.Last() != p2)
                    VisibilityPolygon.AddVertex((Vector2)p2);
                
                TopStatus = minItem;
            }
        }

        internal static Vector2? IntersectionOrHorizontal(Line a_line, LineSegment a_seg)
        {
            if (MathUtil.EqualsEps(a_seg.Line.Slope, a_line.Slope))
            {   // parallel lines
                // if segment is part of line, return closest point to vision point
                if (a_line.IsOnLine(a_seg.Point1)) return a_seg.ClosestPoint(Visibility.VisionPos);
                else return null;
            }
            else
            {
                return a_seg.Intersect(a_line);
            }
        }
    }

    internal class StatusLine : IComparable<StatusLine>, IEquatable<StatusLine>
    {
        internal LineSegment Segment { get; set; }

        internal StatusLine(LineSegment seg)
        {
            Segment = seg;
        }

        // sort on shortest distance to vision point on sweepline
        public int CompareTo(StatusLine other)
        {
            if (other == null) return 1;
            if (this.Equals(other)) return 0;

            var sweep = SweepLine<StatusLine>.Line;

            // find closest intersection point on sweepline
            var p1 = Visibility.IntersectionOrHorizontal(sweep, Segment);
            var p2 = Visibility.IntersectionOrHorizontal(sweep, other.Segment);

            if (p1 == null || p2 == null)
            {
                Debug.Log("p1: " + p1 + ", p2: " + p2);
                Debug.Log(sweep + " " + Segment + " " + other.Segment);
                throw new GeomException("Lines should intersect sweepline");
            }
            var d1 = ((Vector2)p1 - Visibility.VisionPos).sqrMagnitude;
            var d2 = ((Vector2)p2 - Visibility.VisionPos).sqrMagnitude;

            if (!MathUtil.EqualsEps(d1, d2)) return d1.CompareTo(d2);

            // intersections are equal distance on sweepline so segments share a vertex 
            // (assuming no intersecting segments)
            var otherVertex1 = Segment.Point1 != other.Segment.Point1 && Segment.Point1 != other.Segment.Point2 ?
                Segment.Point1 : Segment.Point2;
            var otherVertex2 = other.Segment.Point1 != Segment.Point1 && other.Segment.Point1 != Segment.Point2 ?
                other.Segment.Point1 : other.Segment.Point2;

            var line1 = new Line(Visibility.VisionPos, otherVertex1);
            var line2 = new Line(Visibility.VisionPos, otherVertex2);

            p1 = Segment.Intersect(line2);
            p2 = other.Segment.Intersect(line1);

            if (p1 == null && p2 == null)
            {
                return Segment.Line.Slope.CompareTo(other.Segment.Line.Slope);
            }
            else if (p2 == null)
            {
                return 1;
            }
            else if (p1 == null)
            {
                return -1;
            }
            else
            {
                Debug.Log("two intersections");
                // just compare on something arbitrary
                return Segment.Line.Slope.CompareTo(other.Segment.Line.Slope);
            }
        }

        public bool Equals(StatusLine other)
        {
            if (other == null) return false;
            if (Segment == null) return other.Segment == null;
            return Segment.Equals(other.Segment);
        }

        public override string ToString()
        {
            return Segment.ToString();
        }
    }

    internal class SweepSegment : ISweepEvent<StatusLine>
    {
        public Vector2 Pos { get; private set; }
        public StatusLine StatusItem { get; private set; }
        public bool IsStart { get; private set; }
        public bool IsEnd { get; private set; }

        internal SweepSegment(StatusLine line, Vector2 vertex) : this(line, vertex, true)
        { }

        internal SweepSegment(StatusLine line, Vector2 vertex, bool start)
        {
            StatusItem = line;
            Pos = vertex;
            IsStart = start;
            IsEnd = !start;
        }

        public void SetStart(bool start)
        {
            IsStart = start;
            IsEnd = !start;
        }

        public int CompareTo(ISweepEvent<StatusLine> other)
        {
            var ev = other as SweepSegment;
            if (ev == null) return 1;

            var a1 = MathUtil.Angle(Visibility.VisionPos, Visibility.VisionPos + new Vector2(1f, 0), Pos);
            var a2 = MathUtil.Angle(Visibility.VisionPos, Visibility.VisionPos + new Vector2(1f, 0), ev.Pos);

            if (MathUtil.EqualsEps(a1, a2))
            {
                // do starting events first (to ensure status always contains line segments)
                var ret = IsEnd.CompareTo(ev.IsEnd);
                if (ret != 0) return ret;

                // else do closer event first
                ret = (Pos - Visibility.VisionPos).magnitude.CompareTo((ev.Pos - Visibility.VisionPos).magnitude);
                if (ret != 0) return ret;

                // otherwise just do something arbitrary
                return StatusItem.Segment.Line.Slope.CompareTo(ev.StatusItem.Segment.Line.Slope);
            }
            else
            {
                return a1.CompareTo(a2); 
            }            
        }

        public bool Equals(ISweepEvent<StatusLine> other)
        {
            var ev = other as SweepSegment;
            if (ev == null) return false;

            return StatusItem == ev.StatusItem &&
                Pos == ev.Pos &&
                IsStart == ev.IsStart &&
                IsEnd == ev.IsEnd;
        }

        public override string ToString()
        {
            return "(" + Pos + "," + StatusItem + "," + IsStart + "," + IsEnd + ")";
        }
    }
}
