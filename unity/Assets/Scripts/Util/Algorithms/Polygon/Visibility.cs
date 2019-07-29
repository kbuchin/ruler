namespace Util.Algorithms.Polygon
{
    using System;
    using Util.Geometry;
    using Util.Geometry.Polygon;
    using System.Linq;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Math;

    public static class Visibility
    {
        private static readonly Polygon2D VisibilityPolygon = new Polygon2D();
        private static LineSegment TopSegment;

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
            TopSegment = null;

            // We will apply a sweepline algorithm

            // First Gather all line segments
            var linesegments = new List<LineSegment>();
            linesegments.AddRange(polygon.Outside.Segments());
            foreach (var hole in polygon.Holes)
            {
                linesegments.AddRange(hole.Segments());
            }
            var statuslines = new List<StatusLine>();
            foreach (var l in linesegments) statuslines.Add(new StatusLine(l));

            // set static variables 
            SweepSegment.Pos = StatusLine.Pos = a_pos;

            // create SweepLine object
            var SweepLine = new SweepLine<StatusLine>();

            // create initial status
            var initLines = statuslines
                .FindAll(item => item.Segment.YInterval.Contains(a_pos.y) && item.Segment.X(a_pos.y) > a_pos.x);

            var initStatus = new List<StatusLine>();
            foreach (var item in initLines) initStatus.Add(item);

            // add initial lines that cross the x-axis to the right of a_pos
            SweepLine.InitializeStatus(initStatus);

            //fill eventlist with begin and end points of segments.
            var events = new List<ISweepEvent<StatusLine>>();
            foreach (var item in statuslines)
            {
                var ev1 = new SweepSegment(item, item.Segment.Point1);
                var ev2 = new SweepSegment(item, item.Segment.Point2);

                if (ev1.CompareTo(ev2) < 0)
                {
                    ev1.SetStart(true);
                    ev2.SetStart(false);
                }
                else
                {
                    ev1.SetStart(false);
                    ev2.SetStart(true);
                }

                events.Add(ev1);
                events.Add(ev2);
            }

            SweepLine.Sweep(events, HandleEvent);

            return VisibilityPolygon;
        }

        static void HandleEvent(StatusLine minItem, ISweepEvent<StatusLine> ev)
        {
            if(minItem == null)
            {
                throw new GeomException("Status should always contain some line segment.");
            }

            var sweepEv = ev as SweepSegment;
            if(sweepEv == null)
            {
                throw new GeomException("Event should be of type SweepSegment");
            }

            if (minItem.Segment != TopSegment)
            {
                VisibilityPolygon.AddVertex(sweepEv.Vertex);
                TopSegment = minItem.Segment;
            }
        }
    }

    internal class StatusLine : IComparable<StatusLine>, IEquatable<StatusLine>
    {
        internal static Vector2 Pos { get; set; }

        internal LineSegment Segment { get; set; }

        internal StatusLine(LineSegment seg)
        {
            Segment = seg;
        }

        public int CompareTo(StatusLine other)
        {
            var d1 = Segment.DistanceToPoint(Pos);
            var d2 = other.Segment.DistanceToPoint(Pos);
            return d1.CompareTo(d2);
        }

        public bool Equals(StatusLine other)
        {
            return Segment.Equals(other.Segment);
        }
    }

    internal class SweepSegment : ISweepEvent<StatusLine>
    {
        internal static Vector2 Pos { get; set; }

        public Vector2 Vertex { get; private set; }

        public StatusLine StatusItem { get; private set; }

        public bool IsStart { get; private set; }

        public bool IsEnd { get; private set; }

        internal SweepSegment(StatusLine line, Vector2 vertex) : this(line, vertex, true)
        { }

        internal SweepSegment(StatusLine line, Vector2 vertex, bool start)
        {
            StatusItem = line;
            Vertex = vertex;
            IsStart = start;
            IsEnd = !start;
        }

        public void SetStart(bool start)
        {
            IsStart = start;
            IsEnd = !start;
        }

        public int CompareTo(object obj)
        {
            var ev = obj as SweepSegment;
            if (ev != null) return 1;

            var a1 = MathUtil.Angle(Pos, Pos + new Vector2(1f, 0), Vertex);
            var a2 = MathUtil.Angle(Pos, Pos + new Vector2(1f, 0), ev.Vertex);

            var comp = a1.CompareTo(a2);
            if (comp == 0)
                // if angle is same 
                return (Vertex - Pos).magnitude.CompareTo((ev.Vertex - Pos).magnitude);
            else
                return comp;
        }
    }
}
