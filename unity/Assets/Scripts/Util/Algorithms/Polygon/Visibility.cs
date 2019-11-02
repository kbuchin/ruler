namespace Util.Algorithms.Polygon
{
    using System;
    using Util.Geometry;
    using Util.Geometry.Polygon;
    using System.Linq;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Math;
    using Util.Geometry.Core;

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

            if (!polygon.Contains(z))
            {
                throw new ArgumentException("Visibility point must be inside polygon");
            }

            // list v, satisfies assumptions made in paper (section 2, paragraph 1
            // and 2).
            float initAngle;
            var vs = Preprocess(polygon, z, out initAngle);

            var v0 = vs.Get(0);

            var s0 = new Stack<VertDispl>();
            s0.Push(v0);

            Debug.Assert(vs.n > 1);

            List<VertDispl> s; // used to store the vertices of the visibility
                               // polygon
            if (MathUtil.GEQEps(vs.Get(1).alpha, v0.alpha))
                s = Advance(vs, s0, 0);
            else
                s = Scan(vs, s0, 0, null, true);  // CounterClockWise

            Debug.Assert(MathUtil.EqualsEps(s[s.Count - 1].p.Cartesian, v0.p.Cartesian));

            var poly = Postprocess(s, vs, z, initAngle);

            return poly;
        }

        /// <summary>
        /// Preprocess polygon such that z is at origin and vertices are stored as polar points with angular displacements.
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
                // v0 = vn
                l.Add(new PolarPoint2D(v0.R, v0.Theta + 2 * MathUtil.PI));
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
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <param name="s"></param>
        /// <param name="iprev"></param>
        /// <returns></returns>
        public static List<VertDispl> Advance(VsRep v, Stack<VertDispl> s, int iprev)
        {
            var n = v.n - 1;

            Debug.Assert(iprev + 1 <= n);

            if (MathUtil.LEQEps(v.Get(iprev + 1).alpha, MathUtil.PI2))
            {
                int i = iprev + 1;
                s.Push(v.Get(i));

                // TODO check order of returned list
                if (i == n)
                {
                    return new List<VertDispl>(s);
                }

                if (MathUtil.LessEps(v.Get(i + 1).alpha, v.Get(i).alpha)
                   && MathUtil.Orient2D(v.Get(i - 1).p.Cartesian, v.Get(i).p.Cartesian,
                         v.Get(i + 1).p.Cartesian) < 0)
                { // -1 is RightTurn
                    return Scan(v, s, i, null, true); // CounterClockwise
                }
                else if (MathUtil.LessEps(v.Get(i + 1).alpha, v.Get(i).alpha)
                      && MathUtil.Orient2D(v.Get(i - 1).p.Cartesian, v.Get(i).p.Cartesian,
                              v.Get(i + 1).p.Cartesian) == 1)
                { // 1 is LeftTurn
                    return Retard(v, s, i);
                }
                else
                {
                    return Advance(v, s, i);
                }
            }
            else
            {
                var v0 = v.Get(0);

                if (s.Peek().alpha < MathUtil.PI2)
                {
                    var isect = (new LineSegment(v.Get(iprev).p.Cartesian, v.Get(iprev + 1).p.Cartesian)).Intersect(v0.p.Ray);

                    if (isect == null)
                    {
                        Debug.Log(new LineSegment(v.Get(iprev).p.Cartesian, v.Get(iprev + 1).p.Cartesian));
                        Debug.Log(v0.p.Ray);
                    }

                    Debug.Assert(isect != null);

                    var st = DisplacementInBetween(new PolarPoint2D(isect.Value), v.Get(iprev), v.Get(iprev + 1));
                    s.Push(st);
                }

                return Scan(v, s, iprev, v0, false); // ClockWise
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <param name="sOld"></param>
        /// <param name="iprev"></param>
        /// <returns></returns>
        public static List<VertDispl> Retard(VsRep v, Stack<VertDispl> s, int iprev)
        {
            var sjNext = LocateSj(v.Get(iprev), v.Get(iprev + 1), s);

            var sj = s.Peek();

            if (sj.alpha < v.Get(iprev + 1).alpha)
            {
                int i = iprev + 1;

                var vi = v.Get(i);
                var p = (new LineSegment(sj.p.Cartesian, sjNext.p.Cartesian)).Intersect(vi.p.Ray);

                if (p == null)
                {
                    Debug.Log(new LineSegment(sj.p.Cartesian, sjNext.p.Cartesian));
                    Debug.Log(vi.p.Ray);
                    Debug.Log(vi.p.Cartesian);
                }
                Debug.Assert(p != null);

                var st1 = DisplacementInBetween(new PolarPoint2D(p.Value), sj, sjNext);

                if (st1 != null)
                    s.Push(st1);

                s.Push(vi);

                // paper does i == v.n
                if (i == v.n - 1)
                {
                    // TODO order of returned list correct? (check stack to list conversion)
                    return new List<VertDispl>(s);
                }
                else if (MathUtil.GEQEps(v.Get(i + 1).alpha, vi.alpha) && 
                    MathUtil.Orient2D(v.Get(i - 1).p.Cartesian, vi.p.Cartesian, v.Get(i + 1).p.Cartesian) < 0)
                { // -1 is RighTurn
                    return Advance(v, s, i);
                }
                else if (MathUtil.GreaterEps(v.Get(i + 1).alpha, vi.alpha) && 
                    MathUtil.Orient2D(v.Get(i - 1).p.Cartesian, vi.p.Cartesian, v.Get(i + 1).p.Cartesian) > 0)
                {  // 1 is LeftTurn
                    s.Pop();
                    return Scan(v, s, i, vi, false);  //ClockWise
                }
                else
                {
                    s.Pop();
                    return Retard(v, s, i);
                }
            }
            else
            {
                if (MathUtil.EqualsEps(v.Get(iprev + 1).alpha, sj.alpha) &&
                    MathUtil.GreaterEps(v.Get(iprev + 2).alpha, v.Get(iprev + 1).alpha) &&
                    MathUtil.Orient2D(v.Get(iprev).p.Cartesian, v.Get(iprev + 1).p.Cartesian, v.Get(iprev + 2).p.Cartesian) < 0)
                {  // -1 is RightTurn
                    s.Push(v.Get(iprev + 1));
                    return Advance(v, s, iprev + 1);
                }
                else
                {
                    VertDispl w = IntersectWithWindow(v.Get(iprev), v.Get(iprev + 1), sj, sjNext);

                    Debug.Assert(w != null);
                    return Scan(v, s, iprev, w, true); // CounterClockWise
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <param name="s"></param>
        /// <param name="iprev"></param>
        /// <param name="windowEnd"></param>
        /// <param name="ccw"></param>
        /// <returns></returns>
        public static List<VertDispl> Scan(VsRep v, Stack<VertDispl> s, int iprev, VertDispl windowEnd, bool ccw)
        {
            for (int i = iprev + 1; i < v.n; i++)
            {
                if (ccw &&        // CounterClockWise
                    MathUtil.GreaterEps(v.Get(i + 1).alpha, s.Peek().alpha) &&
                    MathUtil.GEQEps(s.Peek().alpha, v.Get(i).alpha))
                {
                    VertDispl intersec = IntersectWithWindow(v.Get(i), v.Get(i + 1), s.Peek(), windowEnd);

                    if (intersec != null && !(windowEnd != null && MathUtil.EqualsEps(intersec.p.Cartesian, windowEnd.p.Cartesian)))
                    {
                        s.Push(intersec);
                        return Advance(v, s, i);
                    }
                }
                else if (!ccw &&        // ClockWise
                    MathUtil.LEQEps(v.Get(i + 1).alpha, s.Peek().alpha) &&
                    s.Peek().alpha < v.Get(i).alpha)
                {
                    if (IntersectWithWindow(v.Get(i), v.Get(i + 1), s.Peek(), windowEnd) != null)
                    {
                        return Retard(v, s, i);
                    }
                }
            }

            throw new GeomException("Scan called for i >= n, error in preprocess");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="orig"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public static VertDispl IntersectWithWindow(VertDispl a, VertDispl b, VertDispl orig, VertDispl endpoint)
        {
            LineSegment s1 = new LineSegment(a.p, b.p);

            Vector2? res;
            if (endpoint != null)
            {
                LineSegment s2 = new LineSegment(orig.p, endpoint.p);
                res = s1.Intersect(s2);
            }
            else
            {
                var ray = new Ray2D(orig.p.Cartesian, orig.Direction);
                res = s1.Intersect(ray);
            }

            if (res == null) return null;

            return DisplacementInBetween(new PolarPoint2D(res.Value), a, b);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
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

            Debug.Assert(MathUtil.LEQEps(bot, temp) && MathUtil.LEQEps(temp, top));
            return new VertDispl(s, temp);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vi"></param>
        /// <param name="vi1"></param>
        /// <param name="ss"></param>
        /// <param name="outSj"></param>
        /// <returns></returns>
        public static VertDispl LocateSj(VertDispl vi, VertDispl vi1, Stack<VertDispl> ss)
        {
            var sj1 = ss.Pop();

            while (ss.Count > 0)
            {
                VertDispl sj = ss.Peek();

                if (MathUtil.LessEps(sj.alpha, vi1.alpha) && MathUtil.LEQEps(vi1.alpha, sj1.alpha))
                {
                    // TODO check if order is correct
                    return sj1;
                }

                var y = (new LineSegment(vi.p.Cartesian, vi1.p.Cartesian)).Intersect(new LineSegment(sj.p.Cartesian, sj1.p.Cartesian));

                if (y != null &&
                    MathUtil.LEQEps(vi1.alpha, sj.alpha) &&
                    MathUtil.EqualsEps(sj.alpha, sj1.alpha))// &&
                    //!MathUtil.EqualsEps(y.Value, sj.p.Cartesian) &&
                    //!MathUtil.EqualsEps(y.Value, sj1.p.Cartesian))
                {
                    // TODO check if order is correct
                    return sj1;
                }

                // remove top vertex and continue iterating
                sj1 = ss.Pop();
            }
            return sj1;
            //throw new GeomException("Retard called while no vertices in stack");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pre_s"></param>
        /// <param name="vs"></param>
        /// <param name="z"></param>
        /// <param name="initAngle"></param>
        /// <returns></returns>
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
            if (MathUtil.EqualsEps(shiftedPol.First(), shiftedPol.Last()))
            {
                shiftedPol.RemoveAt(shiftedPol.Count - 1);
            }

            // make polygon clockwise
            // to conform to clockwise standard in project
            shiftedPol.Reverse();

            return new Polygon2D(shiftedPol);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
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

                Debug.Assert(Mathf.Abs(alpha_vi - ret[i - 1].alpha) < MathUtil.PI);

                ret.Add(new VertDispl(vi, alpha_vi));
            }

            return ret;
        }

        /// <summary>
        /// Retrieve the original vertex, which should not be z and have the smallest positive angle compared to z.
        /// </summary>
        /// <param name="shiftedPol"></param>
        /// <param name="zIsVertex">whether z is a vertex</param>
        /// <returns>the polar point corresponding to the initial vertex</returns>
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
            var closestVisibleVertex = visiblePolar.FirstOrDefault();
            foreach (var curr in visiblePolar)
            {
                if (curr.Theta < closestVisibleVertex.Theta && MathUtil.GEQEps(curr.Theta, 0))
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
                if (curr.IntersectProper(e) != null)
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
                    v = ComputeAngularDisplacements(vs.GetRange(1, vs.Count));
                else
                    v = ComputeAngularDisplacements(vs);
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
    }
}
