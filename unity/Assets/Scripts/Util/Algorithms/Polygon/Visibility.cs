namespace Util.Algorithms.Polygon
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Geometry;
    using Util.Geometry.Polygon;
    using Util.Math;

    /// <summary>
    /// Collection of algorithms related to visibility polygons.
    /// </summary>
    public static class Visibility
    {
        /// <summary>
        /// Computes the visibility polygon from the given point 
        /// inside of a simple polygon (given as n vertices in CCW order) in O(n) time.
        /// Based on: https://cs.uwaterloo.ca/research/tr/1985/CS-85-38.pdf
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Polygon2D Vision(Polygon2D polygon, Vector2 z)
        {
            // check for invalid polygon
            if (polygon.VertexCount < 3)
            {
                return null;
            }

            if (!polygon.ContainsInside(z))
            {
                throw new ArgumentException("Visibility point must be inside polygon");
            }

            // list v, satisfies assumptions made in paper (section 2, paragraph 1
            // and 2).
            float initAngle;

            var vs = Preprocess(polygon, z, out initAngle);

            
            var s = new Stack<VertDispl>();
            var i = 0;
            VertDispl w = null;
            var ccw = true;

            var v0 = vs.Get(0);
            s.Push(v0);

            Debug.Assert(vs.n > 1);

            NextCall m_nextCall;
            if (MathUtil.GEQEps(vs.Get(1).alpha, v0.alpha))
                m_nextCall = Advance(ref vs, ref s, ref i, ref w, ref ccw);
            else
                m_nextCall = Scan(ref vs, ref s, ref i, ref w, ref ccw);  // CounterClockWise

            while (m_nextCall != NextCall.STOP)
            {
                switch (m_nextCall)
                {
                    case NextCall.ADVANCE:
                        m_nextCall = Advance(ref vs, ref s, ref i, ref w, ref ccw);
                        break;
                    case NextCall.RETARD:
                        m_nextCall = Retard(ref vs, ref s, ref i, ref w, ref ccw); 
                        break;    
                    case NextCall.SCAN: 
                        m_nextCall = Scan(ref vs, ref s, ref i, ref w, ref ccw);
                        break;    
                }
            }

            var sList = s.ToList();

            // error occurred due to robustness
            if (sList.Count == 0) return new Polygon2D();

            Debug.Assert(MathUtil.EqualsEps(sList[s.Count - 1].p.Cartesian, v0.p.Cartesian));

            var poly = Postprocess(sList, vs, z, initAngle);

            return poly;
        }

        /// <summary>
        /// Preprocess polygon such that z is at origin and
        /// vertices are stored as polar points with angular displacements.
        /// Orders vertices based on angle to z.
        /// </summary>
        /// <param name="pol"></param>
        /// <param name="z"></param>
        /// <param name="initAngle"></param>
        /// <returns></returns>
        private static VsRep Preprocess(Polygon2D pol, Vector2 z, out float initAngle)
        {
            // make polygon counter clockwise
            if (pol.IsClockwise())
            {
                pol = new Polygon2D(pol.Vertices.Reverse());
            }
            else
            {
                pol = new Polygon2D(pol.Vertices);
            }

            // shift such that z is at origin
            pol.ShiftToOrigin(z);

            // check if z is a vertex or not
            var zIsVertex = pol.Vertices.Contains(Vector2.zero);

            // determines v0
            var v0 = GetInitialVertex(pol, zIsVertex);

            Debug.Assert(!v0.Cartesian.Equals(z));

            // converts cartesian vertices of polygon to polar and stores them in
            // list l
            var l = pol.Vertices.Select(x => new PolarPoint2D(x)).ToList();

            // adjusts positions such that v0 at the beginning
            var index = l.IndexOf(v0);
            l = l.Skip(index).Concat(l.Take(index)).ToList();

            Debug.Assert(l[0].Equals(v0));
            Debug.Assert(l.Count == pol.Vertices.Count);

            // if z is a vertex then [v0, v1, ..., vk, z] -> [z, v0, v1, ..., vk]
            if (zIsVertex)
            {
                // removes z from list (z is origin because we shifted the polygon)
                var temp = l.Remove(new PolarPoint2D(0, 0));
                Debug.Assert(temp);

                l.Insert(0, new PolarPoint2D(0, 0));
            }
            else
            {
                l.Add(new PolarPoint2D(v0.R, v0.Theta));
            }

            // remember original angle
            initAngle = v0.Theta;

            // rotate all points of the shifted polygon clockwise such that v0 lies
            // on the x axis
            foreach (var curr in l)
            {
                if (!curr.IsOrigin())
                {
                    curr.RotateClockWise(initAngle);
                }
            }

            Debug.Assert(l[0].Theta == 0);

            return new VsRep(l, zIsVertex);
        }

        /// <summary>
        /// Pushes a new vertex on the stack and calls the appropriated function
        /// (advance, retard, scan) depending on the next vertex on the polygon.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="s"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static NextCall Advance(ref VsRep v, ref Stack<VertDispl> s, ref int i, ref VertDispl w, ref bool ccw)
        {
            var n = v.n - 1;

            Debug.Assert(i + 1 <= n);

            if (MathUtil.LEQEps(v.Get(i + 1).alpha, MathUtil.PI2))
            {
                i++;
                s.Push(v.Get(i));

                // TODO check order of returned list
                if (i == n)
                {
                    return NextCall.STOP;
                }

                if (MathUtil.LessEps(v.Get(i + 1).alpha, v.Get(i).alpha)
                   && MathUtil.Orient2D(v.Get(i - 1).p.Cartesian, v.Get(i).p.Cartesian,
                         v.Get(i + 1).p.Cartesian) < 0)
                { // -1 is RightTurn
                    w = null;
                    ccw = true;
                    return NextCall.SCAN;
                }
                else if (MathUtil.LessEps(v.Get(i + 1).alpha, v.Get(i).alpha)
                      && MathUtil.Orient2D(v.Get(i - 1).p.Cartesian, v.Get(i).p.Cartesian,
                              v.Get(i + 1).p.Cartesian) > 0)
                { // 1 is LeftTurn
                    return NextCall.RETARD;
                }
                else
                {
                    return NextCall.ADVANCE;
                }
            }
            else
            {
                var v0 = v.Get(0);

                if (MathUtil.LEQEps(s.Peek().alpha, MathUtil.PI2))
                {
                    var isect = (new LineSegment(v.Get(i).p.Cartesian, v.Get(i + 1).p.Cartesian)).Intersect(v0.p.Ray);

                    Debug.Assert(isect != null);

                    var st = DisplacementInBetween(new PolarPoint2D(isect.Value), v.Get(i), v.Get(i + 1));
                    s.Push(st);
                }

                w = v0;
                ccw = false;
                return NextCall.SCAN;
            }
        }

        /// <summary>
        /// Pops all vertices from the stack that have become invisible after the addition
        /// of a new vertex.
        /// Calls appropriate method (advance, scan, retard) after being done based on next vertex.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="sOld"></param>
        /// <param name="iprev"></param>
        /// <returns></returns>
        public static NextCall Retard(ref VsRep v, ref Stack<VertDispl> s, ref int i, ref VertDispl w, ref bool ccw)
        {
            // LocateSj will pop vertices from the stack
            // until appropriated s_j is found
            // see paper
            var sjNext = LocateSj(v.Get(i), v.Get(i + 1), s);

            if (s.Count == 0)
            {
                return NextCall.STOP;
            }

            var sj = s.Peek();

            if (sj.alpha < v.Get(i + 1).alpha)
            {
                i++;

                var vi = v.Get(i);
                var p = (new LineSegment(sj.p.Cartesian, sjNext.p.Cartesian)).Intersect(vi.p.Ray);

                if (p == null) return NextCall.STOP;

                //Debug.Assert(p != null, new LineSegment(sj.p.Cartesian, sjNext.p.Cartesian) + "\n" + vi.p.Ray);

                var st1 = DisplacementInBetween(new PolarPoint2D(p.Value), sj, sjNext);

                if (st1 != null)
                    s.Push(st1);

                s.Push(vi);

                // paper does i == v.n
                if (i == v.n - 1)
                {
                    // TODO order of returned list correct? (check stack to list conversion)
                    return NextCall.STOP;
                }
                else if (MathUtil.GEQEps(v.Get(i + 1).alpha, vi.alpha) &&
                    MathUtil.Orient2D(v.Get(i - 1).p.Cartesian, vi.p.Cartesian, v.Get(i + 1).p.Cartesian) <= 0)
                { // -1 is RighTurn
                    return NextCall.ADVANCE;
                }
                else if (MathUtil.GreaterEps(v.Get(i + 1).alpha, vi.alpha) &&
                    MathUtil.Orient2D(v.Get(i - 1).p.Cartesian, vi.p.Cartesian, v.Get(i + 1).p.Cartesian) > 0)
                {  // 1 is LeftTurn
                    s.Pop();
                    w = vi;
                    ccw = false;
                    return NextCall.SCAN;
                }
                else
                {
                    s.Pop();
                    return NextCall.RETARD;
                }
            }
            else
            {
                if (MathUtil.EqualsEps(v.Get(i + 1).alpha, sj.alpha) &&
                    MathUtil.GreaterEps(v.Get(i + 2).alpha, v.Get(i + 1).alpha) &&
                    MathUtil.Orient2D(v.Get(i).p.Cartesian, v.Get(i + 1).p.Cartesian, v.Get(i + 2).p.Cartesian) <= 0)
                {  // -1 is RightTurn
                    s.Push(v.Get(i + 1));
                    return NextCall.ADVANCE;
                }
                else
                {
                    w = IntersectWithWindow(v.Get(i), v.Get(i + 1), sj, sjNext);
                    ccw = true;
                    
                    if (w == null) return NextCall.STOP;
                    else return NextCall.SCAN;
                }
            }
        }

        /// <summary>
        /// Scans the vertices on the polygon until either an advance or retard step can be made.
        /// Used when iterating over a section of the polygon not visible from z.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="s"></param>
        /// <param name="iprev"></param>
        /// <param name="windowEnd"></param>
        /// <param name="ccw"></param>
        /// <returns> </returns>
        public static NextCall Scan(ref VsRep v, ref Stack<VertDispl> s, ref int i, ref VertDispl windowEnd, ref bool ccw)
        {
            while (i < v.n)
            {
                i++;

                if (ccw &&        // CounterClockWise
                    MathUtil.GreaterEps(v.Get(i + 1).alpha, s.Peek().alpha) &&
                    MathUtil.GEQEps(s.Peek().alpha, v.Get(i).alpha))
                {
                    VertDispl intersec = IntersectWithWindow(v.Get(i), v.Get(i + 1), s.Peek(), windowEnd);

                    if (intersec != null && !(windowEnd != null && MathUtil.EqualsEps(intersec.p.Cartesian, windowEnd.p.Cartesian)))
                    {
                        s.Push(intersec);
                        return NextCall.ADVANCE;
                    }
                }
                else if (!ccw &&        // ClockWise
                    MathUtil.LEQEps(v.Get(i + 1).alpha, s.Peek().alpha) &&
                    s.Peek().alpha < v.Get(i).alpha)
                {
                    if (IntersectWithWindow(v.Get(i), v.Get(i + 1), s.Peek(), windowEnd) != null)
                    {
                        return NextCall.RETARD;
                    }
                }
            }

            return NextCall.STOP;
        }

        /// <summary>
        /// Either intersects two line segments defined by the given four polar points
        /// or if endpoint is null, a line segment (a,b) and ray from point orig in the direction 
        /// of its adjacent polygon segment.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="orig"></param>
        /// <param name="endpoint"></param>
        /// <returns> A vertex-displacement pair associated with the intersection. </returns>
        public static VertDispl IntersectWithWindow(VertDispl a, VertDispl b, VertDispl orig, VertDispl endpoint)
        {
            if (a == null || b == null || orig == null) return null;

            var s1 = new LineSegment(a.p, b.p);

            // extra checks related to robustness issues
            if (s1.IsOnSegment(orig.p.Cartesian)) return orig;

            Vector2? res;
            if (endpoint != null)
            {
                var s2 = new LineSegment(orig.p, endpoint.p);

                // check for parallel slopes
                if (s1.IsParallel(s2))
                {
                    res = s1.ClosestPoint(orig.p.Cartesian);

                    if (res.HasValue && !s2.IsOnSegment(res.Value))
                    {
                        res = null;
                    }
                }
                else
                {
                    res = s1.Intersect(s2);
                }
            }
            else
            {
                var ray = new Ray2D(orig.p.Cartesian, orig.Direction);
                res = s1.Intersect(ray);
            }

            if (!res.HasValue)
            {
                return null;
            }

            return DisplacementInBetween(new PolarPoint2D(res.Value), a, b);
        }

        /// <summary>
        /// Gives a displacement a(s) value between a(v1) and a(v2).
        /// Increments/decrements a(s) with 2 * PI until between a(v1) and a(v2).
        /// </summary>
        /// <param name="s"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns> The generated vertex-displacement pair. </returns>
        public static VertDispl DisplacementInBetween(PolarPoint2D s, VertDispl v1, VertDispl v2)
        {
            var bot = Mathf.Min(v1.alpha, v2.alpha);
            var top = Mathf.Max(v1.alpha, v2.alpha);
            if (MathUtil.EqualsEps(bot, top))
                return new VertDispl(s, bot);

            var temp = s.Theta;
            while (MathUtil.GreaterEps(temp, top))
                temp -= MathUtil.PI2;

            while (MathUtil.LessEps(temp, bot))
                temp += MathUtil.PI2;

            //Debug.Assert(MathUtil.LEQEps(bot, temp) && MathUtil.LEQEps(temp, top));

            return new VertDispl(s, temp);
        }

        /// <summary>
        /// Locates the vertex s_j on the stack with either a(s_j) <= a(v_i+1) <= a(s_j+1)
        /// or a(v_i+1) <= a(s_j) and a(s_j) = a(s_j+1) and segments (v_i, v_i+1) and (s_j, s_j+1) intersect.
        /// Pops nodes from the stack until s_j found.
        /// </summary>
        /// <param name="vi"></param>
        /// <param name="vi1"></param>
        /// <param name="ss"></param>
        /// <param name="outSj"></param>
        /// <returns> The last node popped from the stack, s_j+1. </returns>
        public static VertDispl LocateSj(VertDispl vi, VertDispl vi1, Stack<VertDispl> ss)
        {
            var sj1 = ss.Pop();

            while (ss.Count > 0)
            {
                VertDispl sj = ss.Peek();

                if (MathUtil.LEQEps(sj.alpha, vi1.alpha) && MathUtil.LEQEps(vi1.alpha, sj1.alpha))
                {
                    // TODO check if order is correct
                    return sj1;
                }

                if (MathUtil.LEQEps(vi1.alpha, sj.alpha) &&
                    MathUtil.EqualsEps(sj.alpha, sj1.alpha))
                {
                    var y = (new LineSegment(vi.p.Cartesian, vi1.p.Cartesian)).Intersect(new LineSegment(sj.p.Cartesian, sj1.p.Cartesian));

                    if (y != null)
                    // TODO check if order is correct
                    {
                        return sj1;
                    }
                }

                // remove top vertex and continue iterating
                sj1 = ss.Pop();
            }

            throw new GeomException("LocateSj removed all vertices in stack");
        }

        /// <summary>
        /// Transforms the vertices on the stack to the final visibility polygon.
        /// Reverses the pre-process steps, namely the translation and rotation.
        /// Also removes one of endpoints if duplicate
        /// </summary>
        /// <param name="pre_s"></param>
        /// <param name="vs"></param>
        /// <param name="z"></param>
        /// <param name="initAngle"></param>
        /// <returns> Final visibility polygon. </returns>
        private static Polygon2D Postprocess(List<VertDispl> pre_s, VsRep vs, Vector2 z, float initAngle)
        {
            // reverse order of stack to establish CCW order of final visibility polygon
            pre_s.Reverse();

            if (vs.zIsVertex)
                pre_s.Add(new VertDispl(new PolarPoint2D(Vector2.zero), 0));

            // convert VertDispl to PolarPoint2D
            var rotatedPol = pre_s.Select(v => v.p).ToList();

            // rotates points back to original position before the rotation in preprocess()
            foreach (var curr in rotatedPol)
            {
                curr.RotateClockWise(-initAngle);
            }

            // convert PolarPoint2D to Vector2
            // shifts points back to their position before the shift in preprocess()
            var shiftedPol = rotatedPol.Select(v => v.Cartesian + z).ToList();

            Debug.Assert(shiftedPol.Count > 0);

            // check if first and last vertex are the same
            if (MathUtil.EqualsEps(shiftedPol.First(), shiftedPol.Last(), MathUtil.EPS * 10))
            {
                shiftedPol.RemoveAt(shiftedPol.Count - 1);
            }

            // make polygon clockwise
            // to conform to clockwise standard in project
            shiftedPol.Reverse();

            return new Polygon2D(shiftedPol);
        }

        /// <summary>
        /// Computes the angular displacement of each polar vertex with respect to starting vertex.
        /// As discussed in paper, the angular displacement of vertex is the previous displacement with 
        /// either + angle or - angle based on whether the current vertex constitutes a left or right turn.
        /// (or no addition/subtraction if the angle is exactly PI)
        /// </summary>
        /// <param name="v"></param>
        /// <returns>A list of vertex-displacement pairs. </returns>
        private static List<VertDispl> ComputeAngularDisplacements(List<PolarPoint2D> v)
        {
            // used to store the result, vertices with their corresponding angular
            // displacement
            var ret = new List<VertDispl>();

            for (var i = 0; i < v.Count; i++)
            {
                if (i == 0)
                {
                    ret.Add(new VertDispl(v[0], v[0].Theta));
                    continue;
                }

                var vi = v[i];
                var viprev = v[i - 1];
                var phi = vi.Theta;
                var rawAngle = Mathf.Abs(phi - viprev.Theta);

                Debug.Assert(rawAngle < MathUtil.PI2);

                var angle = Mathf.Min(rawAngle, MathUtil.PI2 - rawAngle);
                var sigma = MathUtil.Orient2D(Vector2.zero, viprev.Cartesian, vi.Cartesian);
                var alpha_vi = ret[i - 1].alpha + sigma * angle;

                Debug.Assert(MathUtil.LEQEps(Mathf.Abs(alpha_vi - ret[i - 1].alpha), MathUtil.PI));

                ret.Add(new VertDispl(vi, alpha_vi));
            }

            return ret;
        }

        /// <summary>
        /// Retrieve the original vertex, which should not be z and
        /// have the smallest positive angle compared to z.
        /// </summary>
        /// <param name="shiftedPol"></param>
        /// <param name="zIsVertex">whether z is a vertex</param>
        /// <returns> The polar point corresponding to the initial vertex. </returns>
        private static PolarPoint2D GetInitialVertex(Polygon2D shiftedPol, bool zIsVertex)
        {
            // if z is vertex then take vertex adjacent to z
            if (zIsVertex)
            {
                // find segment whose endpoint a is origin, pick the adjacent
                // endpoint b
                foreach (var curr in shiftedPol.Segments)
                {
                    if (MathUtil.EqualsEps(curr.Point1, Vector2.zero))
                    {
                        return new PolarPoint2D(curr.Point2);
                    }
                }

            }

            // if z is on an edge, return the vertex next to it
            foreach (var curr in shiftedPol.Segments)
            {
                if (curr.IsOnSegment(Vector2.zero))
                {
                    return new PolarPoint2D(curr.Point2);
                }
            }

            // used to store all visible (from z) vertices of the polygon
            var visible = new List<Vector2>();

            foreach (var v in shiftedPol.Vertices)
            {
                if (VisibleFromOrigin(shiftedPol, v))
                {
                    visible.Add(v);
                }
            }

            Debug.Assert(visible.Count != 0);

            var visiblePolar = visible.Select(x => new PolarPoint2D(x));

            // find visible vertex with smallest positive angle
            // if angles are equal, picks the closest one
            var closestVisibleVertex = visiblePolar.FirstOrDefault();
            foreach (var curr in visiblePolar)
            {
                if (MathUtil.LessEps(curr.Theta, closestVisibleVertex.Theta) ||
                    MathUtil.EqualsEps(curr.Theta, closestVisibleVertex.Theta) && curr.R < closestVisibleVertex.R)
                {
                    closestVisibleVertex = curr;
                }
            }

            return closestVisibleVertex;
        }

        /// <summary>
        /// Checks whether any of the polygons edges "block the view" from origin to point v.
        /// An edge e of the polygon blocks the view from origin to point v if and only if
	    /// e properly intersects(interiors intersect) the line segment formed by origin and v.
        /// </summary>
        /// <param name="pol">Polygon</param>
        /// <param name="v">Point whose visibility from origin we are checking.</param>
        /// <returns>true iff polygon does not block the view from origin to v.</returns>        
        public static bool VisibleFromOrigin(Polygon2D pol, Vector2 v)
        {
            var e = new LineSegment(Vector2.zero, v);

            foreach (var curr in pol.Segments)
            {
                var intersect = curr.Intersect(e);
                if (intersect.HasValue &&
                    !MathUtil.EqualsEps(intersect.Value, e.Point1, MathUtil.EPS * 10) &&
                    !MathUtil.EqualsEps(intersect.Value, e.Point2, MathUtil.EPS * 10))
                {
                    return false;   // edge curr of the polygon properly intersects e, hence v is not visible from origin
                }
            }

            // no edge of the polygon "blocks the view" (properly intersects e) from origin to v
            return true;
        }

        /// <summary>
        /// Stores a vertex as a polar point and an angular displacement.
        /// </summary>
        public class VertDispl
        {
            internal PolarPoint2D p;
            internal float alpha;   // angular displacement

            public VertDispl(PolarPoint2D p, float alpha)
            {
                this.p = p;
                this.alpha = alpha;
            }

            /// <summary>
            /// The direction vector of the angular displacement.
            /// </summary>
            public Vector2 Direction
            {
                get { return MathUtil.Rotate(new Vector2(1, 0), alpha); }
            }

            public override string ToString()
            {
                return "(" + p + ", " + alpha + ")";
            }
        }

        /// <summary>
        /// Representation structure for the vertices with angular displacements.
        /// Plus some additional data like whether z is a vertex in the polygon.
        /// </summary>
        public class VsRep
        {
            public bool zIsVertex;
            public int n;

            // contains vertices with angular displacements
            private readonly List<VertDispl> v;

            public VsRep(List<PolarPoint2D> vs, bool zIsVertex)
            {
                this.zIsVertex = zIsVertex;
                n = (zIsVertex) ? vs.Count - 1 : vs.Count;

                if (zIsVertex)
                {
                    v = ComputeAngularDisplacements(vs.GetRange(1, vs.Count));
                }
                else
                {
                    v = ComputeAngularDisplacements(vs);
                    //Debug.Assert(MathUtil.GEQEps(vs[vs.Count - 1].Theta, MathUtil.PI2));
                }
            }

            /// <summary>
            /// Gets the vertex+displacement at given position.
            /// </summary>
            /// <param name="i"></param>
            /// <returns></returns>
            public VertDispl Get(int i)
            {
                return v[i % n];
            }
        }

        public enum NextCall
        {
            ADVANCE,
            RETARD,
            SCAN,
            STOP
        }
    }
}
