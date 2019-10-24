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

    public static class Visibility
    {
        public static Polygon2D Vision(Polygon2D polygon, Vector2 z)
        {
            if (polygon.VertexCount < 3)
            {
                return null;
            }

            // make polygon counter clockwise
            if (polygon.IsClockwise())
            {
                polygon = new Polygon2D(polygon.Vertices.Reverse());
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
                s = Scan(vs, s0, 0, null, -1);  // -1 is CounterClockWise

            Debug.Assert(MathUtil.EqualsEps(s[s.Count - 1].p.Cartesian, v0.p.Cartesian));

            return Postprocess(s, vs, z, initAngle);
        }

        private static VsRep Preprocess(Polygon2D pol, Vector2 z, out float initAngle)
        {
            pol.ShiftToOrigin(z);

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

            // if z is on boundary?? then [v0, v1, ..., vk, z] -> [z, v0, v1, ...,
            // vk]
            if (zIsVertex)
            {
                // removes z from list (z is origin because we shifted the polygon)
                var temp = l.Remove(new PolarPoint2D(0, 0));
                Debug.Assert(temp);

                l.Insert(0, new PolarPoint2D(0, 0));
            }

            // rotate all points of the shifted polygon clockwise such that v0 lies
            // on the x axis
            foreach (var curr in l)
            {
                if (!curr.IsOrigin())
                {
                    curr.RotateClockWise(v0.Theta);
                }
            }

            Debug.Assert(l[0].Theta == 0);

            initAngle = v0.Theta;
            return new VsRep(l, zIsVertex);
        }

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
                                v.Get(i + 1).p.Cartesian) == -1)
                { // -1 is RightTurn
                    return Scan(v, s, i, null, -1); // -1 is CounterClockwise
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

                return Scan(v, s, iprev, v0, 1); // 1 is ClockWise
            }
        }

        public static List<VertDispl> Retard(VsRep v, Stack<VertDispl> sOld, int iprev)
        {
            var sj1 = sOld.Peek();
            var ssTail = new List<VertDispl>(sOld).Skip(1).ToList();

            VertDispl sjNext;
            var s = LocateSj(v.Get(iprev), v.Get(iprev + 1), sj1, ssTail, out sjNext);

            var sj = s.Peek();

            if (sj.alpha < v.Get(iprev + 1).alpha)
            {
                int i = iprev + 1;

                // TODO .map correctly translated?
                var vi = v.Get(i);
                var p = (new LineSegment(sj.p.Cartesian, sjNext.p.Cartesian)).Intersect(vi.p.Ray);

                if (p == null)
                {
                    Debug.Log(new LineSegment(sj.p.Cartesian, sjNext.p.Cartesian));
                    Debug.Log(vi.p.Ray);
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
                    MathUtil.Orient2D(v.Get(i - 1).p.Cartesian, vi.p.Cartesian, v.Get(i + 1).p.Cartesian) == -1)
                { // -1 is RighTurn
                    return Advance(v, s, i);
                }
                else if (MathUtil.GreaterEps(v.Get(i + 1).alpha, vi.alpha) && 
                    MathUtil.Orient2D(v.Get(i - 1).p.Cartesian, vi.p.Cartesian, v.Get(i + 1).p.Cartesian) == 1)
                {  // 1 is LeftTurn
                    s.Pop();
                    return Scan(v, s, i, vi, 1);  // 1 is ClockWise
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
                    MathUtil.Orient2D(v.Get(iprev).p.Cartesian, v.Get(iprev + 1).p.Cartesian, v.Get(iprev + 2).p.Cartesian) == -1)
                {  // -1 is RightTurn

                    s.Push(v.Get(iprev + 1));
                    return Advance(v, s, iprev + 1);

                }
                else
                {
                    VertDispl w = IntersectWithWindow(v.Get(iprev), v.Get(iprev + 1), sj, sjNext);

                    Debug.Assert(w != null);
                    return Scan(v, s, iprev, w, -1); // -1 is CounterClockWise
                }
            }
        }

        public static List<VertDispl> Scan(VsRep v, Stack<VertDispl> s, int iprev, VertDispl windowEnd, int ccw)
        {
            var i = iprev + 1;

            if (i + 1 == v.n) return new List<VertDispl>(s);

            if (ccw == -1 &&        // -1 is CounterClockWise
                MathUtil.GreaterEps(v.Get(i + 1).alpha, s.Peek().alpha) &&
                MathUtil.GEQEps(s.Peek().alpha, v.Get(i).alpha))
            {

                VertDispl intersec = IntersectWithWindow(v.Get(i), v.Get(i + 1), s.Peek(), windowEnd);

                if (intersec != null && !(windowEnd != null && MathUtil.EqualsEps(intersec.p.Cartesian, windowEnd.p.Cartesian)))
                {
                    s.Push(intersec);
                    return Advance(v, s, i);
                }
                else
                {
                    return Scan(v, s, i, windowEnd, ccw);
                }
            }
            else if (ccw == 1 &&        // 1 is ClockWise
                     MathUtil.LEQEps(v.Get(i + 1).alpha, s.Peek().alpha) &&
                     s.Peek().alpha < v.Get(i).alpha)
            {
                if (IntersectWithWindow(v.Get(i), v.Get(i + 1), s.Peek(), windowEnd) != null)
                {
                    return Retard(v, s, i);
                }
                else
                {
                    return Scan(v, s, i, windowEnd, ccw);
                }
            }
            else
            {
                return Scan(v, s, i, windowEnd, ccw);
            }

        }

        public static VertDispl IntersectWithWindow(VertDispl a, VertDispl b, VertDispl orig, VertDispl endpoint)
        {
            LineSegment s1 = new LineSegment(a.p, b.p);

            Vector2? res;
            if (endpoint != null)
            {
                LineSegment s2 = new LineSegment(orig.p, endpoint.p);
                res = s1.Intersect(s2);

                if (res == null) { Debug.Log(s1); Debug.Log(s2); }
            }
            else
            {
                var ray = new Ray2D(orig.p.Cartesian, orig.Direction);
                res = s1.Intersect(ray);


                if (res == null) { Debug.Log(s1); Debug.Log(ray); }
            }

            Debug.Assert(res != null);

            return DisplacementInBetween(new PolarPoint2D(res.Value), a, b);
        }


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

        public static Stack<VertDispl> LocateSj(VertDispl vi, VertDispl vi1, VertDispl sj1,
                List<VertDispl> ss, out VertDispl outSj)
        {

            VertDispl sj = ss[0];
            var sTail = ss.Skip(1).ToList();

            if (MathUtil.LessEps(sj.alpha, vi1.alpha) && MathUtil.LEQEps(vi1.alpha, sj1.alpha))
            {
                //			Collections.reverse(ss); 

                // TODO check if order is correct
                outSj = sj1;
                return new Stack<VertDispl>(ss);
            }

            var y = (new LineSegment(vi.p.Cartesian, vi1.p.Cartesian)).Intersect(new LineSegment(sj.p.Cartesian, sj1.p.Cartesian));

            if (y != null &&
                MathUtil.LEQEps(vi1.alpha, sj.alpha) &&
                MathUtil.LEQEps(sj.alpha, sj1.alpha) &&
                !MathUtil.EqualsEps(y.Value, sj.p.Cartesian) &&
                !MathUtil.EqualsEps(y.Value, sj1.p.Cartesian))
            {
                // TODO check if order is correct
                outSj = sj1;
                return new Stack<VertDispl>(ss);
            }

            return LocateSj(vi, vi1, sj, sTail, out outSj);
        }


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

            return new Polygon2D(shiftedPol);
        }

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
                        return new PolarPoint2D(curr.Point2);
                }

            }

            // if z is on an edge, return the vertex next to it
            foreach (var curr in shiftedPol.Segments)
            {
                if (curr.IsOnSegment(Vector2.zero))
                    return new PolarPoint2D(curr.Point2);
            }

            // used to store all visible (from z) vertices of the polygon
            var visible = new List<Vector2>();

            foreach (var v in shiftedPol.Vertices)
            {
                if (VisibleFromOrigin(shiftedPol, v))
                    visible.Add(v);
            }

            Debug.Assert(visible.Count != 0);

            var visiblePolar = visible.Select(x => new PolarPoint2D(x));

            var closestVisibleVertex = visiblePolar.First();

            foreach (var curr in visiblePolar)
            {
                if (curr.R < closestVisibleVertex.R && (curr.R > 0 || MathUtil.EqualsEps(curr.R, 0)))
                    closestVisibleVertex = curr;
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
        /// <returns>true iff.polygon does not block the view from origin to v.</returns>        
        public static bool VisibleFromOrigin(Polygon2D pol, Vector2 v)
        {
            var e = new LineSegment(Vector2.zero, v);

            foreach (var curr in pol.Segments)
            {
                if (curr.IntersectProper(e) != null)
                    return false;   // edge curr of the polygon properly intersects e, hence v is not visible from origin
            }

            // no edge of the polygon "blocks the view" (properly intersects e) from origin to v
            return true;
        }

        public class VertDispl
        {
            internal PolarPoint2D p;
            internal float alpha;   // angular displacement

            public VertDispl(PolarPoint2D p, float alpha)
            {
                this.p = p;
                this.alpha = alpha;
            }

            public Vector2 Direction
            {
                get { return MathUtil.Rotate(new Vector2(1, 0), alpha); }
            }

            public override string ToString()
            {
                return "(" + p + ", " + alpha + ")";
            }
        }

        public class VsRep
        {
            internal List<VertDispl> v;
            internal bool zIsVertex;
            internal int n;

            public VsRep(List<PolarPoint2D> vs, bool zIsVertex)
            {
                this.zIsVertex = zIsVertex;
                n = (zIsVertex) ? vs.Count - 1 : vs.Count;

                if (zIsVertex)
                    v = ComputeAngularDisplacements(vs.GetRange(1, vs.Count));
                else
                    v = ComputeAngularDisplacements(vs);
            }

            public VertDispl Get(int i)
            {
                return v[i];
            }
        }
    }
}
