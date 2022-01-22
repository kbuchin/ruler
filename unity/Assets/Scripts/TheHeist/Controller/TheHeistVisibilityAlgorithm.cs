namespace TheHeist
{

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry;
    using Util.Geometry.Polygon;
    using Util.DataStructures.BST;
    using Util.DataStructures.Queue;
    using Util.Math;
    using System.Linq;
    using System;
    using Util.Algorithms.Polygon;

    public class TheHeistVisibilityAlgorithm : MonoBehaviour
    {

        // position of the player,guard and objects.
        [SerializeField]
        Vector2 playerPos;
        [SerializeField]
        Vector2 guardPos;
        // [SerializeField]
        Vector2 guardOrientation;

        private static Vector2 origin = Vector2.zero;
        private static readonly float coneWidth = 30;

        private Cone cone = new Cone(origin, coneWidth);

        // constructor for the class
        public Polygon2D VisionCone(Polygon2DWithHoles polygon, Vector2 guardPos, Vector2 playerPos, Vector2 guardOrientation)
        {
            // set values
            this.playerPos = playerPos;
            this.guardPos = guardPos;
            this.guardOrientation = Vector2.right; // (guardOrientation.Equals(origin)) ? Vector2.left : guardOrientation;

            // clone polygon
            Polygon2DWithHoles poly = new Polygon2DWithHoles(polygon.Outside, polygon.Holes);

            this.cone = new Cone(origin, coneWidth);

            // shift such that guard is origin of polygon
            poly.ShiftToOrigin(guardPos);

            // cone's polygon for debugging
            var coneList = new List<Vector2>();
            coneList.Add(origin);
            coneList.Add(cone.start.direction);
            coneList.Add(cone.end.direction);
            var conePoly = new Polygon2D(coneList);

            // rotate
            var angle = MathUtil.Angle(origin, MathUtil.Rotate(guardOrientation, coneWidth * Mathf.Deg2Rad), Vector2.right);

            Polygon2D rotatedOutside = RotatePolygon(poly.Outside, angle);

            List<Polygon2D> rotatedHoles = new List<Polygon2D>();
            foreach (var hole in poly.Holes)
            {
                rotatedHoles.Add(RotatePolygon(hole, angle));
            }

            Polygon2DWithHoles rotatedPoly = new Polygon2DWithHoles(rotatedOutside, rotatedHoles);
            
            // create and filter the events
            List<Event> events = GetEvents(rotatedPoly);


            //// create sweepline
            Ray2D sweepline = new Ray2D(origin, Vector2.right);

            //// init status and list for resulting polygon
            AATree<Segment> status = InitStatusTree(events, sweepline); //new AATree<StatusItem>();

            List<Vector2> result = HandleEvents(events, status, sweepline);
            Polygon2D resPoly = RotatePolygon(new Polygon2D(result), -angle);

            //Polygon2D resPoly = RotatePolygon(conePoly, -angle);


            resPoly.ShiftToOrigin(-guardPos);

            //if (!resPoly.IsConvex()) Debug.Log("The vision polygon is nog Convex");

            // print result before rotation
          //  Debug.Log("Result: " + new Polygon2D(result).ToString());

            // return the visiblity polygon
            return resPoly;
        }

        public void PrintState(AATree<Segment> status, Segment top, Event e)
        {
            //Debug.Log("Event: " + e.vertex + ", "+ e.radians + "\n Top: " + top.segment.ToString());
            Segment statusSeg = null;
            status.FindMin(out statusSeg);
            var statusStr = statusSeg.segment.ToString() + "angle: " + statusSeg.angleP1;
            for (int i = 1; i < status.Count; i++)
            {
                Segment newMin = null;
                status.FindNextBiggest(statusSeg, out newMin);
                statusSeg = newMin;
                statusStr += ", " + statusSeg.segment.ToString() + " angle: " + statusSeg.angleP1;
            }
           // Debug.Log("Status: " + statusStr);
        }

        // returns angle between v and cone direction in degrees
        public float CalcAngle(Vector2 vertice)
        {
            float angle = Vector2.SignedAngle(vertice, cone.ray.direction);
            if (angle < 0) angle += 360;
            return angle;
        }

        private bool BetweenAngles(float test, float a1, float a2)
        {
            if (a1 < test && test < a2) return true;
            else return true;
        }

        private bool InConeStart(Segment seg)
        {
            double a1 = MathUtil.Angle(origin, seg.segment.Point1, Vector2.right);
            double a2 = MathUtil.Angle(origin, seg.segment.Point2, Vector2.right);

            //Debug.Log("InLineCheck: " + a1 + ", " + a2);

            // if a1 >= 270 and a2 <= 90
            if (a1 >= 180 * Mathf.Deg2Rad && a2 <= 180 * Mathf.Deg2Rad) return true;
            else if (a2 >= 180 * Mathf.Deg2Rad && a1 <= 180 * Mathf.Deg2Rad) return true;
            else return false;
        }

        public static Vector2 Rotate(Vector2 v, float degrees)
        {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = v.x;
            float ty = v.y;
            v.x = (cos * tx) - (sin * ty);
            v.y = (sin * tx) + (cos * ty);
            return v;
        }

        private Polygon2D RotatePolygon(Polygon2D poly, double angle)
        {
            List<Vector2> rotatedVertices = new List<Vector2>();

            foreach (var v in poly.Vertices)
            {
                var rv = MathUtil.Rotate(v, angle);
                rotatedVertices.Add(rv);
            }

            return new Polygon2D(rotatedVertices);
        }

        private List<Event> CreateEvent(List<LineSegment> segments, bool isHole)
        {
            List<Event> e = new List<Event>();
            for (int i = 0; i < segments.Count; i++)
            {
                Vector2 vertex = segments[i].Point1;
                LineSegment prev = (i != 0) ? segments[i - 1] : segments[segments.Count - 1];
                LineSegment next = segments[i];

                e.Add(new Event(vertex, new Segment(prev), new Segment(next), isHole, cone.ray.direction));
            }
            return e;
        }

        private List<Event> GetEvents(Polygon2DWithHoles polygon)
        {

            List<Event> events = new List<Event>();
            List<LineSegment> segmentsOutside = polygon.Outside.Segments.ToList();
            ICollection<Polygon2D> polyHoles = polygon.Holes;



            // create outside events
            events.AddRange(CreateEvent(segmentsOutside, false));

            // add hole events
            foreach (var hole in polyHoles)
            {
                List<LineSegment> segments = hole.Segments.ToList();
                events.AddRange(CreateEvent(segments, true));
            }

            // filter and sort events
            events.Sort();
            
            return events;
        }

        // Initializes the status structure with all Segments intersecting the sweepline
        private AATree<Segment> InitStatusTree(List<Event> events, Ray2D sweepline)
        {
            AATree<Segment> status = new AATree<Segment>();
            HashSet<Segment> tempSet = new HashSet<Segment>();

            // if the event's previous segment intersects the sweepline, add the segment to the status
            foreach (var e in events)
            {
                foreach ( Segment seg in e.GetSegments()) {
                    var intersection = seg.segment.Intersect(sweepline);
                    if (intersection.HasValue && InConeStart(seg))
                    {
                        var a1 = MathUtil.Angle(origin, Vector2.right, seg.segment.Point1);
                        var a2 = MathUtil.Angle(origin, Vector2.right, seg.segment.Point2);
                        var av = MathUtil.Angle(origin, Vector2.right, e.vertex);
                       // Debug.Log("Status add: " + a1 + ", " + a2 + ", v: " + av + "rad: " + e.radians);
                        tempSet.Add(seg);
                    }

                }
            }

            foreach (var s in tempSet) status.Insert(s);

            return status;
        }

        private List<Vector2> HandleEvents(List<Event> events, AATree<Segment> status, Ray2D sweepline)
        {
            // List of vertices for the resulting visibility polygon
            List<Vector2> result = new List<Vector2>();

            Segment topSeg = null;
            if (status.Count > 0) status.FindMin(out topSeg);
            else print("Status Empty");

            // create a hashset to ensure only adding unique segments
            HashSet<Segment> tempSet = new HashSet<Segment>();

            // handle first top
            // add the origin
            result.Add(origin);
            var intersec = topSeg.segment.Intersect(sweepline);
            if (intersec.HasValue) result.Add(intersec.Value);

            // print("cone end angle: " + MathUtil.Angle(origin, cone.end.direction, Vector2.right));

            for (var i = 0; i < events.Count; i++)
            {
                Event e = events[i];

                // new sweepling
                sweepline = new Ray2D(origin, e.vertex);
                // print("event angle: " + e.radians);

                PrintState(status, topSeg, e);

                // if event
                if (MathUtil.EqualsEps(MathUtil.Angle(origin, cone.end.direction, Vector2.right), e.radians)
                    && topSeg.segment.IsEndpoint(e.vertex))
                {
                    result.Add(e.vertex);
                }
                
                // finish if sweepline falls outside the vision cone
                if (MathUtil.Angle(origin, cone.end.direction, Vector2.right) < e.radians 
                    || MathUtil.EqualsEps(MathUtil.Angle(origin, cone.end.direction, Vector2.right), e.radians))
                {
                    // add final intersection with the current top segment
                    var lastIntersection = topSeg.segment.Intersect(cone.end);
                    if (lastIntersection.HasValue) result.Add(lastIntersection.Value);
                    else print("could not find the last intersection point");
                   
                    // Finished all events, so break out of the for loop
                    break;
                }

                // handle deletion of segments no longer in sweepline
                // if status still contains previous segment, it can be removed since the sweepline has already passed
                foreach (Segment seg in e.GetSegments())
                {
                    if (status.Contains(seg)) status.Delete(seg);
                    else if (tempSet.Contains(seg)) tempSet.Remove(seg);
                    else tempSet.Add(seg);
                }

                // if next event is on the same sweepline, skip adding new segments to the status structure till all are swept
                if (i + 1 < events.Count && Line.Colinear(origin, e.vertex, events[i + 1].vertex)) continue;

                
                // after all all vertices on the same sweepline have been handled, add them to the status
                foreach (Segment seg in tempSet) status.Insert(seg);
                tempSet.Clear();


                // check if there is a new top segment
                Segment newTopSeg = null;
                status.FindMin(out newTopSeg);
                
                if (topSeg == null) topSeg = newTopSeg;

                // if ther is a new top segment
                if (topSeg != newTopSeg && newTopSeg != null) //! topSeg.Equals(newTopSeg))
                {
                    //Debug.Log("New top: " + newTopSeg.segment.ToString());
                    HandleNewTopEvent(ref result, topSeg, newTopSeg, sweepline);
                    topSeg = newTopSeg;
                }
                
                

            }

            return result;
        }

        private void HandleNewTopEvent(ref List<Vector2> result, Segment oldTop, Segment newTop, Ray2D sweepline)
        {
            // check if the sweepline still intersects the old top segment
            var intersectionOld = oldTop.segment.Intersect(sweepline);

            // if the sweepline does not intersect the old top segment anymore
            if (! intersectionOld.HasValue)
            {
                // add the closes point that is still on the segment
                var pointOnSL = oldTop.segment.Line.Intersect(sweepline);
                if (pointOnSL.HasValue && !result.Contains(pointOnSL.Value)) result.Add(oldTop.segment.ClosestPoint(pointOnSL.Value));
                
                // add the first visible point on the new top segment
                var firstVisiblePoint = newTop.segment.Intersect(sweepline);
                if (firstVisiblePoint.HasValue && !result.Contains(firstVisiblePoint.Value)) result.Add(firstVisiblePoint.Value);
            }

            // new top is in front of old top
            if (intersectionOld.HasValue)
            {
                // add the last visible point on the intersection with the old top
                if (!result.Contains(intersectionOld.Value)) result.Add(intersectionOld.Value);

                // get point on new top
                var intersectionNew = newTop.segment.Intersect(sweepline);
                if (intersectionNew.HasValue && !result.Contains(intersectionNew.Value)) result.Add(intersectionNew.Value);
                else print("new top does not have intersection value");
            }
            
            

        }

        private class Cone
        {
            public Ray2D ray;
            public Ray2D start;
            public Ray2D end;
            public double endAngle;

            public Cone(Vector2 origin, float width)
            {
                Vector2 direction = MathUtil.Rotate(Vector2.right, -width * Mathf.Deg2Rad);
                this.ray = new Ray2D(origin, direction);
                
                Vector2 vecEnd = MathUtil.Rotate(direction, -width * Mathf.Deg2Rad);

                start = new Ray2D(origin, Vector2.right);
                end = new Ray2D(origin, vecEnd);
                endAngle = MathUtil.Angle(origin, Vector2.right, end.direction);
            }

            public Cone(Ray2D direction, float width)
            {
                this.ray = direction;

                Vector2 vecStart = Rotate(ray.direction, width);
                Vector2 vecEnd = Rotate(ray.direction, -width);

                this.start = new Ray2D(origin, vecStart);
                this.end = new Ray2D(origin, vecEnd);
            }


        }

        // Event consists of a point and its adjecent line segments
        public class Event : IComparable<Event>, IEquatable<Event>
        {
            public readonly Vector2 vertex;
            // public float degrees;
            public double radians;
            public Segment prevSeg;
            public Segment nextSeg;
            public bool isHole;

            private static readonly VertexComparer vc = new VertexComparer();

            public Event(Vector2 vertex, Segment prevSeg, Segment nextSeg, bool isHole, Vector2 coneDirection)
            {
                this.vertex = vertex;
                this.prevSeg = prevSeg;
                this.nextSeg = nextSeg;
                this.isHole = isHole;

                // this.degrees = CalcAngle(Vector2.zero);
                radians = MathUtil.Angle(origin, vertex, Vector2.right);
            }

            public List<Segment> GetSegments()
            {
                return new List<Segment>() { prevSeg, nextSeg };
            }

            public int CompareTo(Event otherEvent)
            {
                return radians.CompareTo(otherEvent.radians); // vc.Compare(otherEvent.vertex, vertex);
            }

            public bool Equals(Event otherEvent)
            {
                return vertex.Equals(otherEvent.vertex);
            }

            public override int GetHashCode()
            {
                return 51 * vertex.GetHashCode() + 7 * isHole.GetHashCode();
            }
        }

        public class Segment : IComparable<Segment>, IEquatable<Segment>
        {
            public LineSegment segment;
            public double angleP1;

            private static readonly VertexComparer vc = new VertexComparer();

            public Segment(Vector2 p1, Vector2 p2)
            {
                segment = new LineSegment(p1, p2);
                angleP1 = MathUtil.Angle(origin, p2, Vector2.right);
            }

            public Segment(LineSegment linesegment)
            {
                segment = linesegment;
                angleP1 = MathUtil.Angle(origin, linesegment.Point2, Vector2.right);
            }

            public bool Equals(Segment otherSeg)
            {
                return segment.Equals(otherSeg.segment);
            }

            public override int GetHashCode()
            {
                return 29 * segment.GetHashCode();
            }

            public int CompareTo(Segment other)
            {
                LineSegment otherSeg = other.segment;

                if (Equals(other)) return 0;

                List<Vector2> points = new List<Vector2>() { segment.Point1, segment.Point2, otherSeg.Point1, otherSeg.Point2 };
                points.Sort(vc.Compare);

                //// check if segments overlap each other
                foreach (var p in points)
                {
                    if (segment.IsEndpoint(p))
                    {
                        var overlapPoint = otherSeg.Intersect(new Ray2D(origin, p));
                        if (overlapPoint.HasValue)
                            if (!MathUtil.EqualsEps(p, overlapPoint.Value, MathUtil.EPS * 10))
                            {
                                return p.magnitude.CompareTo(overlapPoint.Value.magnitude);
                            }
                    }

                    else if (otherSeg.IsEndpoint(p))
                    {
                        var overlapPoint = segment.Intersect(new Ray2D(origin, p));
                        if (overlapPoint.HasValue)
                            if (!MathUtil.EqualsEps(p, overlapPoint.Value, MathUtil.EPS * 10))
                            {
                                return overlapPoint.Value.magnitude.CompareTo(p.magnitude);
                            }
                    }

                }
                

                // when the segments don't overlap, compare angles of furthes points
                var thisSegAngle = Math.Max(MathUtil.Angle(origin, Vector2.right, segment.Point1), MathUtil.Angle(origin, Vector2.right, segment.Point2));
                var otherSegAngle = Math.Max(MathUtil.Angle(origin, Vector2.right, otherSeg.Point1), MathUtil.Angle(origin, Vector2.right, otherSeg.Point2));

                // if their difference in angle is >= PI
                if (thisSegAngle - otherSegAngle >= Math.PI) otherSegAngle += 2 * Math.PI;
                if (otherSegAngle - thisSegAngle >= Math.PI) thisSegAngle += 2 * Math.PI;

                if (MathUtil.EqualsEps(thisSegAngle, otherSegAngle))
                {
                    // fallback, something arbitrary
                    return segment.GetHashCode().CompareTo(otherSeg.GetHashCode());
                }

                return thisSegAngle.CompareTo(otherSegAngle);

            }

            
        }
    }
}

    
