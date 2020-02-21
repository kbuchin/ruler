namespace Util.Algorithms.Polygon
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.DataStructures.BST;
    using Util.Geometry;
    using Util.Geometry.Polygon;
    using Util.Math;

    public class VisibilitySweep
    {
        public static Polygon2D Vision(Polygon2D polygon, Vector2 x)
        {
            return Vision(new Polygon2DWithHoles(polygon), x);
        }

        public static Polygon2D Vision(Polygon2DWithHoles polygon, Vector2 x)
        {
            if (!(polygon.ContainsInside(x) || polygon.OnBoundary(x)))
            {
                throw new ArgumentException(x + " is not inside polygon: " + polygon);
            }

            float initAngle;
            var events = Preprocess(polygon, x, out initAngle);

            var status = new AATree<StatusItem>();
            var visibility = new List<Vector2>();

            // create ray in positive x direction
            var ray = new Ray2D(Vector2.zero, new Vector2(1f, 0f));

            VisibilityEvent xEvent = null;

            // initialize the status
            foreach (var v in events)
            {
                if (MathUtil.EqualsEps(v.vertex, Vector2.zero))
                {
                    xEvent = v;
                    continue;
                }

                var seg = v.item1.seg;
                if (!seg.IsEndpoint(x))
                {
                    var intersect = seg.Intersect(ray);
                    if (intersect.HasValue && intersect.Value.x > 0 && (seg.Point1.y >= 0 != seg.Point2.y >= 0) 
                        && !MathUtil.EqualsEps(intersect.Value, Vector2.zero))
                    {  
                        status.Insert(v.item1);
                    }
                }
            }
            
            if (xEvent != null)
            {
                if (!xEvent.isHole) status.Insert(xEvent.item1);
                else status.Delete(xEvent.item1);

                if (!xEvent.isHole) status.Delete(xEvent.item2);   
                else status.Insert(xEvent.item2);

                if (!xEvent.isHole)
                {
                    visibility.Add(xEvent.item2.seg.Point2);
                    visibility.Add(Vector2.zero);
                    visibility.Add(xEvent.item1.seg.Point1);
                }
                else
                {
                    visibility.Add(xEvent.item2.seg.Point1);
                    visibility.Add(Vector2.zero);
                    visibility.Add(xEvent.item1.seg.Point2);
                }
            }
            
            // handle events
            StatusItem top = null;
            var insertions = new HashSet<StatusItem>();
            if (status.Count > 0) status.FindMin(out top);
            for (var i = 0; i < events.Count; i++)
            {
                var v = events[i];

                if (MathUtil.EqualsEps(v.vertex, Vector2.zero)) continue;

                ray = new Ray2D(Vector2.zero, v.vertex);

                // first handle deletions

                // handle first segment
                if (status.Contains(v.item1))
                    status.Delete(v.item1);
                else if (insertions.Contains(v.item1))
                    insertions.Remove(v.item1);
                else
                    insertions.Add(v.item1);

                // handle second segment
                if (status.Contains(v.item2))
                    status.Delete(v.item2);
                else if (insertions.Contains(v.item2))
                    insertions.Remove(v.item2);
                else
                    insertions.Add(v.item2);

                // skip if next event colinear with current
                if (i < events.Count - 1 && Line.Colinear(Vector2.zero, v.vertex, events[i + 1].vertex))
                {
                    // skip until all colinear events are handled
                    continue;
                }

                // handle insertions (after potential skip for colinear events
                foreach (var item in insertions)
                {
                    status.Insert(item);
                }
                insertions.Clear();

                StatusItem newTop;
                status.FindMin(out newTop);

                // do stuff if current top different from previous
                if (top != newTop)
                {
                    // add intersections with previous top segment
                    if (top != null)    HandleTopSegment(ref visibility, top.seg, ray);

                    // add intersections with new top segment
                    if (newTop != null) HandleTopSegment(ref visibility, newTop.seg, ray);

                    top = newTop;
                }
            }

            return Postprocess(visibility, x, initAngle);
        }

        private static List<VisibilityEvent> Preprocess(Polygon2DWithHoles poly, Vector2 x, out float initAngle)
        {
            // copy polygon
            var polygon = new Polygon2DWithHoles(poly.Outside, poly.Holes);

            // shift such that x is at origin
            polygon.ShiftToOrigin(x);
            x = Vector2.zero;

            var angle = 0f;
            bool rot = false;
            var segments = polygon.Outside.Segments.ToList();
            var holeSegments = polygon.Holes.Select(h => h.Segments.ToList()).ToList();

            // insert x if on segment
            var xSeg = segments.FirstOrDefault(seg => seg.IsOnSegment(x) && !seg.IsEndpoint(x));
            if (xSeg != null)
            {
                var i = segments.IndexOf(xSeg);
                segments.Insert(i + 1, new LineSegment(x, xSeg.Point2));
                segments[i] = new LineSegment(xSeg.Point1, x);
                xSeg = segments[i];
            }
            else
            {
                for (var i=0; i<holeSegments.Count; i++)
                {
                    var hsegs = holeSegments[i];
                    xSeg = hsegs.FirstOrDefault(seg => seg.IsOnSegment(x) && !seg.IsEndpoint(x));
                    if (xSeg != null)
                    {
                        var j = hsegs.IndexOf(xSeg);
                        hsegs.Insert(j + 1, new LineSegment(x, xSeg.Point2));
                        xSeg = new LineSegment(xSeg.Point1, x);
                        break;
                    }
                }
            }

            // find if rotation is needed
            xSeg = segments.FirstOrDefault(seg => MathUtil.EqualsEps(seg.Point2, x));
            if (xSeg != null)
            {
                angle = (float)new PolarPoint2D(xSeg.Point1).Theta;
                rot = true;
            }
            else
            {
                for (var i = 0; i < holeSegments.Count; i++)
                {
                    var hsegs = holeSegments[i];
                    xSeg = hsegs.FirstOrDefault(seg => MathUtil.EqualsEps(seg.Point1, x));
                    if (xSeg != null)
                    {
                        angle = (float)new PolarPoint2D(xSeg.Point2).Theta;
                        rot = true;
                        break;
                    }
                }
            }

            if (rot)
            {
                segments = segments.Select(seg => new LineSegment(MathUtil.Rotate(seg.Point1, -angle), 
                    MathUtil.Rotate(seg.Point2, -angle))).ToList();

                for (var i = 0; i < holeSegments.Count; i++)
                {
                    holeSegments[i] = holeSegments[i].Select(seg => new LineSegment(MathUtil.Rotate(seg.Point1, -angle),
                        MathUtil.Rotate(seg.Point2, -angle))).ToList();
                }
            }

            // initialize events
            // one for each Vector2 in the Polygon2D (+ holes)
            var events = CreateEvents(segments, false);
            foreach (var hsegs in holeSegments)
            {
                events.AddRange(CreateEvents(hsegs, true));
            }

            events.Sort();

            initAngle = angle;
            return events;
        }

        private static List<VisibilityEvent> CreateEvents(List<LineSegment> segments, bool isHole)
        {
            var ret = new List<VisibilityEvent>();
            for (int i = 0; i < segments.Count; i++)
            {
                var v = segments[i].Point1;

                var prev = i > 0 ? segments[i - 1] : segments.Last();
                var next = segments[i];
                ret.Add(new VisibilityEvent(v, isHole, new StatusItem(prev), new StatusItem(next)));
            }
            return ret;
        }

        private static void HandleTopSegment(ref List<Vector2> visibility, LineSegment seg, Ray2D ray)
        {
            // dont handle colinear lines
            if (seg == null || Line.Colinear(ray.origin, seg.Point1, seg.Point2)) return;

            var intersect = seg.Intersect(ray);

            if (!intersect.HasValue)
            {
                // robustness issues if ray near endpoint
                var lineIntersect = seg.Line.Intersect(ray);
                if (lineIntersect.HasValue)
                {
                    intersect = seg.ClosestPoint(lineIntersect.Value);
                }
            }

            if (intersect.HasValue &&
                !MathUtil.EqualsEps(Vector2.zero, intersect.Value) &&
                (visibility.Count == 0 || !MathUtil.EqualsEps(intersect.Value, visibility.Last())))
            {
                visibility.Add(intersect.Value);
            }
        }

        private static Polygon2D Postprocess(List<Vector2> vertices, Vector2 x, float initAngle)
        {
            // remove possible duplicate at endpoints
            if (vertices.Count > 2 && MathUtil.EqualsEps(vertices.First(), vertices.Last())) 
                vertices.RemoveAt(vertices.Count - 1);

            // rotate back and then reverse to get clockwise polygon
            return new Polygon2D(vertices.Select(v => x + MathUtil.Rotate(v, initAngle)).Reverse());
        }
    }

    public class VertexComparer : IComparer<Vector2>
    {
        public int Compare(Vector2 x, Vector2 y)
        {
            var a = MathUtil.Angle(Vector2.zero, new Vector2(1, 0), x);
            var b = MathUtil.Angle(Vector2.zero, new Vector2(1, 0), y);
            return a.CompareTo(b);
        }
    }

    public class VisibilityEvent : IComparable<VisibilityEvent>, IEquatable<VisibilityEvent>
    {
        private static readonly VertexComparer vc = new VertexComparer();

        public readonly Vector2 vertex;
        public readonly bool isHole;
        public StatusItem item1;
        public StatusItem item2;

        public VisibilityEvent(Vector2 vertex, bool isHole, StatusItem item1, StatusItem item2)
        {
            //copy
            this.vertex = new Vector2(vertex.x, vertex.y);
            this.isHole = isHole;
            this.item1 = item1;
            this.item2 = item2;
        }

        public int CompareTo(VisibilityEvent other)
        {
            return vc.Compare(vertex, other.vertex);
        }

        public bool Equals(VisibilityEvent other)
        {
            return vertex.Equals(other.vertex) && isHole == other.isHole;
        }
        public override int GetHashCode()
        {
            return 51 * vertex.GetHashCode() + 7 * isHole.GetHashCode();
        }
    }

    public class StatusItem : IComparable<StatusItem>, IEquatable<StatusItem>
    {
        private static readonly VertexComparer vc = new VertexComparer();

        public static Vector2 origin = Vector2.zero;

        public LineSegment seg;
        public bool insert;

        public StatusItem(LineSegment seg)
        {
            // copy
            this.seg = new LineSegment(seg.Point1, seg.Point2);
        }
        
        public int CompareTo(StatusItem other)
        {
            var seg2 = other.seg;

            if (Equals(other))
                return 0;
            
            var points = new List<Vector2>() { seg.Point1, seg.Point2, seg2.Point1, seg2.Point2 };
            
            // sort to make comparison always consistent 
            points.Sort(vc.Compare);

            foreach (var p in points)
            {
                var ray = new Ray2D(origin, p);

                var p1 = p == seg.Point1 ? seg.Point1 : p == seg.Point2 ? seg.Point2 : seg.Intersect(ray);
                var p2 = p == seg2.Point1 ? seg2.Point1 : p == seg2.Point2 ? seg2.Point2 : seg2.Intersect(ray);

                if (p1.HasValue && p2.HasValue && !MathUtil.EqualsEps(p1.Value, p2.Value, MathUtil.EPS * 10)) 
                {
                    //Debug.Log(seg + " " + seg2 + " " + p + " " + p1 + " " + p2);
                    return p1.Value.magnitude.CompareTo(p2.Value.magnitude);
                }
            }

            // two segments have no overlap (from origin) except possibly endpoints
            // return line with endpoint with larger angle

            var other1 = seg.Point1 == seg2.Point1 || seg.Point1 == seg2.Point2 ? seg.Point2 : seg.Point1;
            var other2 = seg.Point1 == seg2.Point1 || seg.Point2 == seg2.Point1 ? seg2.Point2 : seg2.Point1;

            var angle1 = MathUtil.Angle(Vector2.zero, new Vector2(1, 0), other1);
            var angle2 = MathUtil.Angle(Vector2.zero, new Vector2(1, 0), other2);

            if (angle1 - angle2 >= Math.PI) angle2 += 2 * Math.PI;
            if (angle2 - angle1 >= Math.PI) angle1 += 2 * Math.PI;

            if (MathUtil.EqualsEps(angle1, angle2))
            {
                // fallback, something arbitrary
                return seg.GetHashCode().CompareTo(seg2.GetHashCode());
            }

            return angle1.CompareTo(angle2);
        }

        public bool Equals(StatusItem other)
        {
            return seg.Equals(other.seg);
        }

        public override int GetHashCode()
        {
            return 29 * seg.GetHashCode();
        }
    }
}
