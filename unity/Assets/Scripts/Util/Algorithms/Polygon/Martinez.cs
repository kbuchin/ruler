using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Util.DataStructures.BST;
using Util.Geometry;
using Util.Geometry.Contour;
using Util.Geometry.Polygon;
using Util.Math;

namespace Util.Algorithms.Polygon
{
    /// <summary>
    /// Martinez is an implementation of a binary operation on polygons using the algorithm published by Martinez
    /// et al. in "A simple algorithm for Boolean operations on polygons" (https://doi.org/10.1016/j.advengsoft.2013.04.004)
    ///
    /// The implementation is based on the public domain C++ implementation published by the authors of the paper
    /// at http://www4.ujaen.es/~fmartin/bool_op.html with modifications to improve robustness, since the published
    /// C++ implementation is not robust.
    ///
    /// To improve robustness, the algorithm uses doubles instead of floats, as this reduces the number of edge
    /// cases significantly. Moreover, it might seem counterintuitive that EqualsEps is never used and always != 0
    /// and the like are used, but this is on purpose; finding an extra intersection is much less problematic than
    /// not finding an intersection where there should not be one.
    ///
    /// To use this algorithm for the union of <see cref="Polygon2D"/>, see <see cref="UnionSweepLine"/>.
    /// </summary>
    public class Martinez : SweepLine<Martinez.SweepEvent, Martinez.StatusItem>
    {
        /// <summary>
        /// The operation that is being executed. Based on this, the resulting polygon changes.
        /// </summary>
        private OperationType Operation { get; set; }

        /// <summary>
        /// The subject polygon.
        /// </summary>
        private ContourPolygon Subject { get; set; }

        /// <summary>
        /// The clipping polygon.
        /// </summary>
        private ContourPolygon Clipping { get; set; }

        /// <summary>
        /// The ResultEvents contains all events in-order that they were encountered.
        /// </summary>
        private List<SweepEvent> ResultEvents { get; set; }

        /// <summary>
        /// The bounding box of the subject polygon.
        /// </summary>
        private Rect SubjectBoundingBox { get; set; }

        /// <summary>
        /// The bounding box of the clipping polygon.
        /// </summary>
        private Rect ClippingBoundingBox { get; set; }

        /// <summary>
        /// The minimum right bound of the subject and clipping bounding boxes.
        /// </summary>
        private double RightBound { get; set; }

        /// <summary>
        /// Creates a new object for executing a single boolean operation using the Martinez algorithm. Usually, the
        /// next call will be a call to <see cref="Run"/>. The created object can only be used for executing the algorithm
        /// once.
        /// </summary>
        /// <param name="subject">The subject polygon. It must adhere to the orientation as specified in the documentation of ContourPolygon.</param>
        /// <param name="clipping">The clipping polygon. It must adhere to the orientation as specified in the documentation of ContourPolygon.</param>
        /// <param name="operation"></param>
        public Martinez(ContourPolygon subject, ContourPolygon clipping, OperationType operation = OperationType.Union)
        {
            Subject = subject;
            Clipping = clipping;
            Operation = operation;
        }

        /// <summary>
        /// Run will run the algorithm and return the result.
        /// </summary>
        /// <returns></returns>
        public ContourPolygon Run()
        {
            ResultEvents = new List<SweepEvent>();

            SubjectBoundingBox = Subject.BoundingBox();
            ClippingBoundingBox = Clipping.BoundingBox();
            RightBound = System.Math.Min(SubjectBoundingBox.xMax, ClippingBoundingBox.xMax);

            ContourPolygon result;
            if (ComputeTrivialResult(out result)) // Trivial cases can be quickly resolved without sweeping the plane
            {
                return result;
            }

            var events = CreateEvents();

            InitializeEvents(events);
            InitializeStatus(new List<StatusItem>());

            VerticalSweep(HandleEvent);

            return ConnectEdges();
        }

        /// <summary>
        /// Computes a trivial result
        /// </summary>
        /// <returns>Whether a trivial result could be computed</returns>
        private bool ComputeTrivialResult(out ContourPolygon result)
        {
            // When one of the polygons is empty, the result is trivial
            if (Subject.VertexCount == 0 || Clipping.VertexCount == 0)
            {
                switch (Operation)
                {
                    case OperationType.Difference:
                        result = Subject;
                        break;
                    case OperationType.Union:
                    case OperationType.Xor:
                        result = Subject.VertexCount > 0 ? Subject : Clipping;
                        break;
                    case OperationType.Intersection:
                        result = new ContourPolygon();
                        break;
                    default:
                        throw new ArgumentException("Invalid Operation");
                }

                return true;
            }

            // Optimization 1: When the polygons do not overlap, the result is trivial
            if (!SubjectBoundingBox.Overlaps(ClippingBoundingBox))
            {
                // The bounding boxes do not overlap
                switch (Operation)
                {
                    case OperationType.Difference:
                        result = Subject;
                        break;
                    case OperationType.Union:
                    case OperationType.Xor:
                        result = Subject;
                        result.Join(Clipping);
                        break;
                    case OperationType.Intersection:
                        result = new ContourPolygon();
                        break;
                    default:
                        throw new ArgumentException("Invalid Operation");
                }

                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Creates the events for the polygons.
        /// </summary>
        /// <returns>A list containing all events</returns>
        private List<SweepEvent> CreateEvents()
        {
            var events = new List<SweepEvent>();
            for (int i = 0; i < Subject.NumberOfContours; i++)
            {
                for (int j = 0; j < Subject.Contours[i].VertexCount; j++)
                {
                    CreateEvents(Subject.Contours[i].Segment(j), PolygonType.Subject, events);
                }
            }

            for (int i = 0; i < Clipping.NumberOfContours; i++)
            {
                for (int j = 0; j < Clipping.Contours[i].VertexCount; j++)
                {
                    CreateEvents(Clipping.Contours[i].Segment(j), PolygonType.Clipping, events);
                }
            }

            return events;
        }

        /// <summary>
        /// Creates the events for a segment.
        /// </summary>
        /// <param name="segment">A list consisting of two points defining an edge</param>
        /// <param name="polygonType"></param>
        /// <param name="list">The list to add the events to</param>
        private void CreateEvents(IList<Vector2D> segment, PolygonType polygonType,
            ICollection<SweepEvent> list)
        {
            var point1 = segment[0];
            var point2 = segment[1];

            var event1 = new SweepEvent(point1, false, null, polygonType);
            var event2 = new SweepEvent(point2, false, event1, polygonType);
            event1.OtherEvent = event2;

            if (point1.Equals(point2))
            {
                // 0-length segments are irrelevant for us since they will never result in intersections
                return;
            }

            // The segment could be ordered the wrong way around, so we need to set the IsStart field properly
            if (SweepEvent.CompareTo(event1, event2) > 0)
            {
                event2.IsStart = true;
            }
            else
            {
                event1.IsStart = true;
            }

            list.Add(event1);
            list.Add(event2);
        }

        private void HandleEvent(IBST<SweepEvent> events, IBST<StatusItem> status, SweepEvent ev)
        {
            ResultEvents.Add(ev);

            // Optimization 2
            if ((Operation == OperationType.Intersection && ev.Point.x > RightBound) ||
                (Operation == OperationType.Difference && ev.Point.x > SubjectBoundingBox.xMax))
            {
                // We need to connect edges now, so just clear all events. This will result in us immediately
                // going to ConnectEdges() since there are no more events to handle.
                InitializeEvents(new List<SweepEvent>());
                return;
            }

            if (ev.IsStart) // The line segment must be inserted into status
            {
                ev.StatusItem = new StatusItem(ev);
                if (!status.Insert(ev.StatusItem))
                {
                    throw new ArgumentException("Failed to insert into state");
                }

                StatusItem prev;
                var prevFound = status.FindNextSmallest(ev.StatusItem, out prev);

                ComputeFields(ev, prev, prevFound);

                StatusItem next;
                if (status.FindNextBiggest(ev.StatusItem, out next))
                {
                    // Process a possible intersection between "ev" and its next neighbor in status
                    if (PossibleIntersection(ev, next.SweepEvent, events) == 2)
                    {
                        ComputeFields(ev, prev, prevFound);
                        ComputeFields(next.SweepEvent, ev.StatusItem, true);
                    }
                }

                // Process a possible intersection between "ev" and its previous neighbor in status
                if (prevFound)
                {
                    if (PossibleIntersection(prev.SweepEvent, ev, events) == 2)
                    {
                        StatusItem prevprev;
                        var prevprevFound = status.FindNextSmallest(prev, out prevprev);

                        ComputeFields(prev.SweepEvent, prevprev, prevprevFound);
                        ComputeFields(ev, prev, prevFound);
                    }
                }
            }
            else
            {
                // The line segment must be removed from status
                ev = ev.OtherEvent; // We work with the left event

                StatusItem prev, next;
                var prevFound = status.FindNextSmallest(ev.StatusItem, out prev);
                var nextFound = status.FindNextBiggest(ev.StatusItem, out next);

                // Delete line segment associated to "ev" from status and check for intersection between the neighbors of "ev" in status
                status.Delete(ev.StatusItem);

                if (nextFound && prevFound)
                {
                    PossibleIntersection(prev.SweepEvent, next.SweepEvent, events);
                }
            }
        }

        private void ComputeFields(SweepEvent ev, StatusItem prev, bool prevFound)
        {
            // Compute InOut and OtherInOut fields
            if (!prevFound)
            {
                ev.InOut = false;
                ev.OtherInOut = true;
            }
            else if (ev.PolygonType == prev.SweepEvent.PolygonType
            ) // Previous line segment in status belongs to the same polygon that "ev" belongs to
            {
                ev.InOut = !prev.SweepEvent.InOut;
                ev.OtherInOut = prev.SweepEvent.OtherInOut;
            }
            else // Previous line segment in status belongs to a different polygon that "ev" belongs to
            {
                ev.InOut = !prev.SweepEvent.OtherInOut;
                ev.OtherInOut = prev.SweepEvent.Vertical ? !prev.SweepEvent.InOut : prev.SweepEvent.InOut;
            }

            // Compute PreviousInResult field
            if (prevFound)
            {
                ev.PreviousInResult = (!InResult(prev.SweepEvent) || prev.SweepEvent.Vertical)
                    ? prev.SweepEvent.PreviousInResult
                    : prev.SweepEvent;
            }

            // Check if the line segment belongs to the boolean operation
            ev.InResult = InResult(ev);
        }

        private bool InResult(SweepEvent ev)
        {
            switch (ev.EdgeType)
            {
                case EdgeType.Normal:
                    switch (Operation)
                    {
                        case OperationType.Intersection:
                            return !ev.OtherInOut;
                        case OperationType.Union:
                            return ev.OtherInOut;
                        case OperationType.Difference:
                            return (ev.PolygonType == PolygonType.Subject && ev.OtherInOut) ||
                                   (ev.PolygonType == PolygonType.Clipping && !ev.OtherInOut);
                        case OperationType.Xor:
                            return true;
                    }

                    break;
                case EdgeType.SameTransition:
                    return Operation == OperationType.Intersection || Operation == OperationType.Union;
                case EdgeType.DifferentTransition:
                    return Operation == OperationType.Difference;
                case EdgeType.NonContributing:
                    return false;
            }

            // Just to make it compile
            return false;
        }

        private int PossibleIntersection(SweepEvent ev1, SweepEvent ev2, IBST<SweepEvent> events)
        {
            Vector2D intersectionPoint;
            var nIntersections = FindIntersections(ev1.Point, ev1.OtherEvent.Point, ev2.Point, ev2.OtherEvent.Point,
                out intersectionPoint);

            if (nIntersections == 0)
            {
                return 0; // no intersection
            }

            // If the intersection is between two endpoints
            if (nIntersections == 1 && (ev1.Point.Equals(ev2.Point) ||
                                        ev1.OtherEvent.Point.Equals(ev2.OtherEvent.Point)))
            {
                return 0; // the line segments intersect at an endpoint of both line segments
            }

            if (nIntersections == 2 && ev1.PolygonType == ev2.PolygonType)
            {
                // The line segments overlap, but they belong to the same polygon
                throw new ArgumentException(string.Format("Sorry, edges of the same polygon overlap ({0} and {1})", ev1,
                    ev2));
            }

            // The line segments associated to ev1 and ev2 intersect
            if (nIntersections == 1)
            {
                if (!ev1.Point.Equals(intersectionPoint) && !ev1.OtherEvent.Point.Equals(intersectionPoint)
                ) // If the intersection point is not an endpoint of ev1.Segment
                {
                    DivideSegment(ev1, intersectionPoint, events);
                }

                if (!ev2.Point.Equals(intersectionPoint) && !ev2.OtherEvent.Point.Equals(intersectionPoint)
                ) // If the intersection point is not an endpoint of ev2.Segment
                {
                    DivideSegment(ev2, intersectionPoint, events);
                }

                return 1;
            }

            // The line segments associated to ev1 and ev2 overlap
            var sortedEvents = new List<SweepEvent>();
            var leftEqual = false;
            var rightEqual = false;
            if (ev1.Point.Equals(ev2.Point))
            {
                leftEqual = true;
            }
            else if (SweepEvent.CompareTo(ev1, ev2) == 1)
            {
                sortedEvents.Add(ev2);
                sortedEvents.Add(ev1);
            }
            else
            {
                sortedEvents.Add(ev1);
                sortedEvents.Add(ev2);
            }

            if (ev1.OtherEvent.Point.Equals(ev2.OtherEvent.Point))
            {
                rightEqual = true;
            }
            else if (SweepEvent.CompareTo(ev1.OtherEvent, ev2.OtherEvent) == 1)
            {
                sortedEvents.Add(ev2.OtherEvent);
                sortedEvents.Add(ev1.OtherEvent);
            }
            else
            {
                sortedEvents.Add(ev1.OtherEvent);
                sortedEvents.Add(ev2.OtherEvent);
            }

            if (leftEqual)
            {
                // Both line segments are equal or share the left endpoint
                ev2.EdgeType = EdgeType.NonContributing;
                ev1.EdgeType = (ev2.InOut == ev1.InOut) ? EdgeType.SameTransition : EdgeType.DifferentTransition;
                if (!rightEqual)
                {
                    DivideSegment(sortedEvents[1].OtherEvent, sortedEvents[0].Point, events);
                }

                return 2;
            }

            if (rightEqual)
            {
                // The line segments share the right endpoint
                DivideSegment(sortedEvents[0], sortedEvents[1].Point, events);
                return 3;
            }

            if (sortedEvents[0] != sortedEvents[3].OtherEvent)
            {
                // No line segment includes totally the other one
                DivideSegment(sortedEvents[0], sortedEvents[1].Point, events);
                DivideSegment(sortedEvents[1], sortedEvents[2].Point, events);
                return 3;
            }

            // One line segment includes the other one
            DivideSegment(sortedEvents[0], sortedEvents[1].Point, events);
            DivideSegment(sortedEvents[3].OtherEvent, sortedEvents[2].Point, events);
            return 3;
        }

        private void DivideSegment(SweepEvent ev, Vector2D pos, IBST<SweepEvent> events)
        {
            // "Right event" of the "left line segment" resulting from dividing ev.Segment
            var r = new SweepEvent(pos, false, ev, ev.PolygonType);
            // "Left event" of the "right line segment" resulting from dividing ev.Segment
            var l = new SweepEvent(pos, true, ev.OtherEvent, ev.PolygonType);

            if (SweepEvent.CompareTo(l, ev.OtherEvent) > 0
            ) // Avoid a rounding error. The left event would be processed after the right event
            {
                ev.OtherEvent.IsStart = true;
                l.IsStart = false;
            }

            ev.OtherEvent.OtherEvent = l;
            ev.OtherEvent = r;
            events.Insert(l);
            events.Insert(r);
        }

        private ContourPolygon ConnectEdges()
        {
            var result = new ContourPolygon();

            var resultEvents = ResultEvents
                .Where(it => (it.IsStart && it.InResult) || (!it.IsStart && it.OtherEvent.InResult)).ToList();

            // Due to overlapping edges the resultEvents list can be not wholly sorted
            var sorted = false;
            while (!sorted)
            {
                sorted = true;
                for (int i = 0; i < resultEvents.Count; i++)
                {
                    if (i + 1 < resultEvents.Count && SweepEvent.CompareTo(resultEvents[i], resultEvents[i + 1]) == 1)
                    {
                        var tmp = resultEvents[i];
                        resultEvents[i] = resultEvents[i + 1];
                        resultEvents[i + 1] = tmp;
                        sorted = false;
                    }
                }
            }

            // We cannot do a foreach because we need to set PositionInResult
            for (int i = 0; i < resultEvents.Count; i++)
            {
                var resultEvent = resultEvents[i];
                resultEvent.PositionInResult = i;
            }

            foreach (var resultEvent in resultEvents)
            {
                if (!resultEvent.IsStart)
                {
                    var tmp = resultEvent.PositionInResult;
                    resultEvent.PositionInResult = resultEvent.OtherEvent.PositionInResult;
                    resultEvent.OtherEvent.PositionInResult = tmp;
                }
            }

            var processed = new BitArray(resultEvents.Count);
            var depth = new List<int>();
            var holeOf = new List<int>();
            for (int i = 0; i < resultEvents.Count; i++)
            {
                if (processed[i])
                {
                    continue;
                }

                var contour = new Contour();
                result.Add(contour);
                var contourId = result.NumberOfContours - 1;
                depth.Add(0);
                holeOf.Add(-1);
                if (resultEvents[i].PreviousInResult != null)
                {
                    var lowerContourId = resultEvents[i].PreviousInResult.ContourId;
                    if (!resultEvents[i].PreviousInResult.ResultInOut)
                    {
                        result[lowerContourId].AddHole(contourId);
                        holeOf[contourId] = lowerContourId;
                        depth[contourId] = depth[lowerContourId] + 1;
                        contour.External = false;
                    }
                    else if (!result[lowerContourId].External)
                    {
                        result[holeOf[lowerContourId]].AddHole(contourId);
                        holeOf[contourId] = holeOf[lowerContourId];
                        depth[contourId] = depth[lowerContourId];
                        contour.External = false;
                    }
                }

                var pos = i;
                var initial = resultEvents[i].Point;
                contour.AddVertex(initial);
                while (pos >= i)
                {
                    processed[pos] = true;
                    if (resultEvents[pos].IsStart)
                    {
                        resultEvents[pos].ResultInOut = false;
                        resultEvents[pos].ContourId = contourId;
                    }
                    else
                    {
                        resultEvents[pos].OtherEvent.ResultInOut = true;
                        resultEvents[pos].OtherEvent.ContourId = contourId;
                    }

                    pos = resultEvents[pos].PositionInResult;
                    processed[pos] = true;
                    contour.AddVertex(resultEvents[pos].Point);
                    pos = NextPos(pos, resultEvents, processed, i);
                }

                pos = pos == -1 ? i : pos;

                processed[pos] = processed[resultEvents[pos].PositionInResult] = true;
                resultEvents[pos].OtherEvent.ResultInOut = true;
                resultEvents[pos].OtherEvent.ContourId = contourId;
                if ((depth[contourId] & 1) != 0)
                {
                    contour.ChangeOrientation();
                }
            }

            return result;
        }

        private int NextPos(int pos, List<SweepEvent> resultEvents, BitArray processed, int origIndex)
        {
            var newPos = pos + 1;
            while (newPos < resultEvents.Count && resultEvents[newPos].Point.Equals(resultEvents[pos].Point))
            {
                if (!processed[newPos])
                {
                    return newPos;
                }

                newPos++;
            }

            newPos = pos - 1;
            while (newPos >= origIndex && processed[newPos])
            {
                newPos--;
            }

            return newPos;
        }

        /// <summary>
        /// Finds the number of intersections between two line segments. If there is exactly 1 intersection point, will
        /// return that intersection point in intersectionPoint.
        ///
        /// Note that we are not using <see cref="LineSegment.Intersect(LineSegment, LineSegment)"/>'s method because
        /// it is far too imprecise due to using a threshold of being vertical of 100*EPS, while we need exact calculations
        /// which do not use an epsilon at all.
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <param name="intersectionPoint"></param>
        /// <returns></returns>
        private static int FindIntersections(Vector2D a1, Vector2D a2, Vector2D b1, Vector2D b2,
            out Vector2D intersectionPoint)
        {
            intersectionPoint = null;

            // First, we'll create vectors that point in the direction our line segments are pointing
            var va = a2 - a1;
            var vb = b2 - b1;

            // Difference between the two supporting points
            var e = b1 - a1;
            var kross = va.Cross(vb);
            var sqrLenA = va.Dot(va);
            if (kross != 0) // The cross product of va and vb is zero only when the lines are parallel
            {
                // These lines are thus not parallel, but they might still not intersect because they are line segments.

                var s = e.Cross(vb) / kross;
                if (s < 0 || s > 1)
                {
                    // It's not on line segment a
                    return 0;
                }

                var t = e.Cross(va) / kross;
                if (t < 0 || t > 1)
                {
                    // It's not on line segment b
                    return 0;
                }

                if (t == 0 || t == 1)
                {
                    // The intersection is on an endpoint of line segment b
                    intersectionPoint = b1.Interpolate(t, vb);
                    return 1;
                }

                // The check for line segment a is not required because then the following will just work.

                intersectionPoint = a1.Interpolate(s, va);
                return 1;
            }

            // When the vector between the two supporting points and the vector of segment a are parallel,
            // they are the same line. Otherwise, they are merely parallel. So, if the cross product is non-zero,
            // they are just parallel and not the same line.
            if (e.Cross(va) != 0)
            {
                return 0;
            }

            var sa = va.Dot(e) / sqrLenA;
            var sb = sa + va.Dot(vb) / sqrLenA;
            var smin = System.Math.Min(sa, sb);
            var smax = System.Math.Max(sa, sb);

            if (smin <= 1 || smax >= 0)
            {
                if (smin == 1)
                {
                    // Intersection on an endpoint of line segment a
                    intersectionPoint = a1.Interpolate(smin, va);
                    return 1;
                }

                if (smax == 0)
                {
                    // Intersection on an endpoint of line segment b
                    intersectionPoint = a1.Interpolate(smax, va);
                    return 1;
                }

                // There are two intersection points, but for us it's irrelevant where those intersection points
                // are. Just FYI, to get the intersection points:
                // a1.Interpolate(smin > 0 ? smin : 0, va),
                // a1.Interpolate(smax < 1 ? smax : 1, va)
                return 2;
            }

            // The line segments are on the same line, but have no overlap.
            return 0;
        }

        public class SweepEvent : ISweepEvent<StatusItem>, IComparable<SweepEvent>, IEquatable<SweepEvent>
        {
            internal SweepEvent(Vector2D pos, bool isStart, SweepEvent otherEvent, PolygonType polygonType,
                EdgeType edgeType = EdgeType.Normal)
            {
                Point = pos;
                IsStart = isStart;
                OtherEvent = otherEvent;
                PolygonType = polygonType;
                EdgeType = edgeType;

                InOut = true;
                OtherInOut = true;
            }

            // Point associated with the event
            internal Vector2D Point { get; private set; }

            public Vector2 Pos
            {
                get { return Point.Vector2; }
            }

            public StatusItem StatusItem { get; set; }

            /// <summary>
            /// Is Pos the left endpoint of the edge 
            /// </summary>
            public bool IsStart { get; set; }

            /// <summary>
            /// Is Pos the right endpoint of the edge. Note: not actually used, but required by ISweepEvent
            /// </summary>
            public bool IsEnd
            {
                get { return !IsStart; }
            }

            /// <summary>
            /// Other endpoint of the edge. When the edge is subdivided, this is updated.
            /// </summary>
            internal SweepEvent OtherEvent { get; set; }

            /// <summary>
            /// Is this event associated to the Subject or Clipping polygon?
            /// </summary>
            internal PolygonType PolygonType { get; private set; }

            internal EdgeType EdgeType { get; set; }

            /// <summary>
            /// Indicates if this segment determines an inside-outside transition into the polygon for
            /// a vertical ray that starts below the polygon and intersects the segment.
            /// </summary>
            internal bool InOut { get; set; }

            /// <summary>
            /// See InOut, but referred to the closest segment to this segment downwards in status that
            /// belongs to the other polygon. 
            /// </summary>
            internal bool OtherInOut { get; set; }

            /// <summary>
            /// The closest edge to the segment downwards in status that belongs to the result polygon.
            /// This field is used in the second stage of the algorithm to compute child contours.
            /// </summary>
            internal SweepEvent PreviousInResult { get; set; }

            /// <summary>
            /// Whether this segment is in the result.
            /// </summary>
            internal bool InResult { get; set; }

            /// <summary>
            /// Stores the position of the segment in the Result.
            /// </summary>
            internal int PositionInResult { get; set; }

            /// <summary>
            /// Indicates if the segment determines an in-out transition into C for a vertical ray that starts
            /// below C and intersects the segment associated to this segment.
            /// </summary>
            internal bool ResultInOut { get; set; }

            /// <summary>
            /// The ID that identifies the contour C.
            /// </summary>
            internal int ContourId { get; set; }

            /// <summary>
            /// Determines whether the line segment is below point p
            /// </summary>
            /// <param name="p"></param>
            /// <returns>Whether the line segment is below point p</returns>
            internal bool Below(Vector2D p)
            {
                return IsStart
                    ? MathUtil.SignedArea(Point, OtherEvent.Point, p) > 0
                    : MathUtil.SignedArea(OtherEvent.Point, Point, p) > 0;
            }

            /// <summary>
            /// Determines whether the line segment is above point p
            /// </summary>
            /// <param name="p"></param>
            /// <returns>Whether the line segment is above point p</returns>
            internal bool Above(Vector2D p)
            {
                return !Below(p);
            }

            /// <summary>
            /// Indicates whether the line segment is a vertical line segment
            /// </summary>
            internal bool Vertical
            {
                get { return Point.x.Equals(OtherEvent.Point.x); }
            }

            /// <summary>
            /// CompareTo is used for sorting the sweep events in the event BST.
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentException"></exception>
            public int CompareTo(SweepEvent other)
            {
                // This method is different to the static CompareTo because we require this equality checks, whereas
                // the other method does not.
                if (this == other)
                {
                    return 0;
                }

                return CompareTo(this, other);
            }

            /// <summary>
            /// Compare two sweep events.
            /// </summary>
            /// <param name="e1"></param>
            /// <param name="e2"></param>
            /// <returns>True when e1 should be placed before e2, i.e. e1 should be handled after e2</returns>
            public static int CompareTo(SweepEvent e1, SweepEvent e2)
            {
                if (e1.Point.x > e2.Point.x) // Different x-coordinate
                {
                    return 1;
                }

                if (e1.Point.x < e2.Point.x) // Different x-coordinate
                {
                    return -1;
                }

                if (!e1.Point.y.Equals(e2.Point.y)
                ) // Different points, but same x-coordinate. The event with lower y-coordinate is processed first
                {
                    return e1.Point.y > e2.Point.y ? 1 : -1;
                }

                if (e1.IsStart != e2.IsStart
                ) // Same point, but one is a left endpoint and the other a right endpoint. The right endpoint is processed first.
                {
                    return e1.IsStart ? 1 : -1;
                }

                // Same point, but events are left endpoints or both are right endpoints.
                if (MathUtil.SignedArea(e1.Point, e1.OtherEvent.Point, e2.OtherEvent.Point) != 0)
                {
                    // Not collinear
                    return
                        e1.Above(e2.OtherEvent.Point)
                            ? 1
                            : -1; // The event associated to the bottom segment is processed first
                }

                // Collinear
                return e1.PolygonType > e2.PolygonType ? 1 : -1;
            }

            public bool Equals(SweepEvent other)
            {
                throw new NotImplementedException();
            }

            public override string ToString()
            {
                return string.Format(
                    "{0} ({1}) S:[{2} - {3}] ({4}) ({5})",
                    Point, IsStart ? "left" : "right", Point, OtherEvent.Point,
                    PolygonType.ToString().ToUpper(),
                    EdgeType.ToString().ToUpper()
                );
            }
        }

        public class StatusItem : IComparable<StatusItem>, IEquatable<StatusItem>
        {
            internal SweepEvent SweepEvent { get; private set; }

            internal StatusItem(SweepEvent sweepEvent)
            {
                SweepEvent = sweepEvent;
            }

            public int CompareTo(StatusItem other)
            {
                if (ReferenceEquals(this, other))
                {
                    return 0;
                }

                var le1 = this.SweepEvent;
                var le2 = other.SweepEvent;

                if (le1 == le2)
                {
                    return 0;
                }

                if (MathUtil.SignedArea(le1.Point, le1.OtherEvent.Point, le2.Point) != 0 ||
                    MathUtil.SignedArea(le1.Point, le1.OtherEvent.Point, le2.OtherEvent.Point) != 0)
                {
                    // Segments are not collinear
                    // If they share their left endpoint use the right endpoint to sort
                    if (le1.Point.Equals(le2.Point))
                    {
                        return le1.Below(le2.OtherEvent.Point) ? -1 : 1;
                    }

                    // Different left endpoint: use the left endpoint to sort
                    if (le1.Point.x.Equals(le2.Point.x))
                    {
                        return le1.Point.y < le2.Point.y ? -1 : 1;
                    }

                    if (le1.CompareTo(le2) == 1
                    ) // Has the line segment associated to this been inserted into S after the line segment associated to other
                    {
                        return le2.Above(le1.Point) ? -1 : 1;
                    }

                    // The line segment associated to other has been inserted into S after the line segment associated to this
                    return le1.Below(le2.Point) ? -1 : 1;
                }

                // Segments are collinear
                if (le1.PolygonType != le2.PolygonType)
                {
                    return le1.PolygonType == PolygonType.Subject ? -1 : 1;
                }

                // Same polygon

                // Just a consistent criterion is used
                if (le1.Point.Equals(le2.Point))
                {
                    if (le1.OtherEvent.Point.Equals(le2.OtherEvent.Point))
                    {
                        return 0;
                    }

                    return RuntimeHelpers.GetHashCode(le1) > RuntimeHelpers.GetHashCode(le2) ? 1 : -1;
                }

                return le1.CompareTo(le2) == 1 ? 1 : -1;
            }

            public bool Equals(StatusItem other)
            {
                throw new NotImplementedException();
            }

            public override string ToString()
            {
                return string.Format("StatusItem ({0})", SweepEvent);
            }
        }

        public enum PolygonType
        {
            Subject,
            Clipping
        }

        internal enum EdgeType
        {
            Normal,
            NonContributing,
            SameTransition,
            DifferentTransition
        }

        public enum OperationType
        {
            Intersection,
            Union,
            Difference,
            Xor
        }
    }
}