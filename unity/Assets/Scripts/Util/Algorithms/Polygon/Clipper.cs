namespace Util.Algorithms.Polygon
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Geometry;
    using Util.Geometry.Polygon;
    using Util.Math;

    /// <summary>
    /// Collection of algorithm related to cutting out polygons such that they no longer overlap.
    /// </summary>
    public static class Clipper
    {
        /// <summary>
        /// Cuts a_cutPoly out of this polygon.
        /// </summary>
        /// <param name="a_cutPoly"></param>
        public static MultiPolygon2D CutOut(MultiPolygon2D a_subject, Polygon2D a_cutPoly)
        {
            var result = new List<Polygon2D>();

            foreach (var poly in a_subject.Polygons)
            {
                result.AddRange(CutOut(poly, a_cutPoly).Polygons);
            }

            return new MultiPolygon2D(result);
        }

        /// <summary>
        /// A implementation of the Weiler-Atherthon algorithm that cuts away the provided clippling area from the subject polygon.
        /// Assumes the cutting polygon is not entirely inside subject polygon!
        /// </summary>
        /// <param name="a_subject"></param>
        /// <param name="a_clip"></param>
        /// <returns></returns>
        public static MultiPolygon2D CutOut(Polygon2D a_subject, Polygon2D a_clip)
        {
            // copy and maybe reverse
            if (a_subject.IsClockwise())
                a_subject = new Polygon2D(a_subject.Vertices);
            else 
                a_subject = new Polygon2D(a_subject.Vertices.Reverse());

            if (a_clip.IsClockwise()) 
                a_clip = new Polygon2D(a_clip.Vertices);
            else 
                a_clip = new Polygon2D(a_clip.Vertices.Reverse());


            var WA = new WeilerAtherton(a_subject, a_clip);

            //return new MultiPolygon2D();
            return WeilerAthertonCutOut(WA);
        }

        /// <summary>
        /// Creates a cutout given the relevant weiler atherton object containing subject and clipping lists
        /// Can result in multiple polygons.
        /// Assumes the cutting polygon is not entirely inside subject polygon!
        /// </summary>
        /// <param name="WA"></param>
        /// <returns></returns>
        private static MultiPolygon2D WeilerAthertonCutOut(WeilerAtherton WA)
        {
            var multiPoly = new MultiPolygon2D();

            // check if polygons have intersections
            if (WA.EntryIntersections.Count == 0)
            {
                multiPoly.AddPolygon(new Polygon2D(WA.Subject.Vertices));
            }

            while (WA.EntryIntersections.Count > 0)
            {
                var vertices = new List<Vector2>();
                var visited = new HashSet<Vector2>();

                // remove an entry intersection point
                var startPoint = WA.EntryIntersections.LastOrDefault();
                WA.EntryIntersections.Remove(startPoint);

                LinkedListNode<WAPoint> node;
                try
                {
                    node = WA.ClipNode[startPoint];
                } 
                catch (KeyNotFoundException e)
                {
                    Debug.Log("entry point not found");
                    continue;
                }

                WAPoint last = null;

                while (!visited.Contains(node.Value.Pos))
                {
                    // remove entry intersection from starting list
                    WA.EntryIntersections.Remove(node.Value);

                    // traverse clip polygon in counter-clockwise order until exit vertex found
                    while (node.Value.Type != WAList.Exit && !visited.Contains(node.Value.Pos))
                    {
                        // check for duplicates
                        if (last == null || !MathUtil.EqualsEps(node.Value.Pos, last.Pos))
                        {
                            vertices.Add(node.Value.Pos);
                            visited.Add(node.Value.Pos);
                        }
                        last = node.Value;
                        node = node.Previous ?? WA.ClipList.Last;
                    }

                    try
                    {
                        node = WA.SubjectNode[node.Value];
                    }
                    catch (KeyNotFoundException e)
                    {
                        Debug.Log("exit point not found");
                        break;
                    }

                    // traverse subject polygon in clockwise order until new entr vertex found
                    while (node.Value.Type != WAList.Entry && !visited.Contains(node.Value.Pos))
                    {
                        if (last == null || !MathUtil.EqualsEps(node.Value.Pos, last.Pos))
                        {
                            vertices.Add(node.Value.Pos);
                            visited.Add(node.Value.Pos);
                        }
                        last = node.Value;
                        node = node.Next ?? WA.SubjectList.First;
                    }

                    node = WA.ClipNode[node.Value];
                }

                // add new polygon from the vertices
                multiPoly.AddPolygon(new Polygon2D(vertices));
            }

            return multiPoly;
        }

        private class WeilerAtherton
        {
            public Polygon2D Subject;
            public Polygon2D Clip;

            public readonly LinkedList<WAPoint> SubjectList;
            public readonly LinkedList<WAPoint> ClipList;
            public readonly Dictionary<WAPoint, LinkedListNode<WAPoint>> SubjectNode;
            public readonly Dictionary<WAPoint, LinkedListNode<WAPoint>> ClipNode;
            public readonly HashSet<WAPoint> EntryIntersections;

            public WeilerAtherton(Polygon2D a_subject, Polygon2D a_clip)
            {
                // initialize variables
                Subject = a_subject;
                Clip = a_clip;
                SubjectList = new LinkedList<WAPoint>();
                ClipList = new LinkedList<WAPoint>();
                SubjectNode = new Dictionary<WAPoint, LinkedListNode<WAPoint>>();
                ClipNode = new Dictionary<WAPoint, LinkedListNode<WAPoint>>();
                EntryIntersections = new HashSet<WAPoint>();

                // store intersections for clipping line segment
                // saves recomputation of intersections and entry-exit categorization
                var intersectSegmentsClip = new Dictionary<LineSegment, List<WAPoint>>();
                foreach (var seg in a_clip.Segments) intersectSegmentsClip.Add(seg, new List<WAPoint>());

                WAPoint last, current;
                bool inside1, inside2;

                // find all intersections and create subject list
                foreach (var seg1 in a_subject.Segments)
                {
                    // retrieve intersection list
                    var vertices = new List<WAPoint>();
                    foreach (var seg2 in a_clip.Segments)
                    {
                        var intersect = seg1.Intersect(seg2);
                        if (intersect.HasValue)
                        {
                            // store intersection point
                            var point = new WAPoint(intersect.Value, WAList.Vertex);
                            vertices.Add(point);
                            intersectSegmentsClip[seg2].Add(point);
                        }
                    }

                    // sort intersections on distance to start vertex
                    vertices.Sort(new ClosestToPointComparer(seg1.Point1));

                    // add first vertex to point list
                    vertices.Insert(0, new WAPoint(seg1.Point1, WAList.Vertex));

                    //Debug.Log(seg1.Point1 + " HELLO");

                    foreach (var vertex in vertices)
                    {
                        last = SubjectList.LastOrDefault();

                        // remove duplicate last nodes of subject list
                        // removes last rather than current since last could also be point1
                        while (last != null && MathUtil.EqualsEps(last.Pos, vertex.Pos))
                        {
                            vertex.Type = last.Type;
                            last.Type = WAList.Ignore;
                            SubjectList.RemoveLast();
                            EntryIntersections.Remove(last);
                            last = SubjectList.LastOrDefault();
                        }

                        // add intersections to subject list
                        var node = SubjectList.AddLast(vertex);
                        SubjectNode.Add(vertex, node);

                        // dont compare with itself
                        if (SubjectList.Count == 1) continue;

                        // do containment check for setting entry/exit last vertex
                        inside1 = a_clip.ContainsInside(last.Pos);
                        inside2 = a_clip.ContainsInside(vertex.Pos);
                        if (inside1 && !inside2)
                        {
                            last.Type = WAList.Exit;
                        }
                        else if (!inside1 && inside2)
                        {
                            vertex.Type = WAList.Entry;
                            EntryIntersections.Add(vertex);
                        }
                    }                
                }

                last = SubjectList.LastOrDefault();
                current = SubjectList.FirstOrDefault();

                // ignore last node at duplicate points
                while (current != last && MathUtil.EqualsEps(current.Pos, last.Pos))
                {
                    current.Type = WAList.Ignore;
                    SubjectList.RemoveFirst();
                    current = SubjectList.FirstOrDefault();
                }

                // check for first and last entry/exit
                inside1 = a_clip.ContainsInside(last.Pos);
                inside2 = a_clip.ContainsInside(current.Pos);
                if (inside1 && !inside2)
                {
                    last.Type = WAList.Exit;
                }
                else if (!inside1 && inside2)
                {
                    current.Type = WAList.Entry;
                    EntryIntersections.Add(current);
                }

                // create clip list and intersections
                foreach (var seg in a_clip.Segments)
                {
                    var vertices = intersectSegmentsClip[seg];

                    // sort intersections on distance to start vertex
                    intersectSegmentsClip[seg].Sort(new ClosestToPointComparer(seg.Point1));

                    vertices.Insert(0, new WAPoint(seg.Point1, WAList.Vertex));

                    // loop over intersections
                    foreach (var vertex in vertices)
                    {
                        if (vertex.Type == WAList.Ignore) continue;

                        // add intersection to clipping list
                        var node = ClipList.AddLast(vertex);
                        ClipNode.Add(vertex, node);
                    }
                }
            }
        }

        /// <summary>
        /// Compares points based on distance to another point.
        /// Used for sorting intersection points on distance to start vertex.
        /// </summary>
        private class ClosestToPointComparer : IComparer<WAPoint>
        {
            private Vector2 m_point;

            public ClosestToPointComparer(Vector2 a_point)
            {
                m_point = a_point;
            }

            public int Compare(WAPoint x, WAPoint y)
            {
                var dist_1 = Vector2.Distance(m_point, x.Pos);
                var dist_2 = Vector2.Distance(m_point, y.Pos);

                return dist_1.CompareTo(dist_2);
            }
        }

        /// <summary>
        /// Stores a vertex with a category (Vertex, Entry, Exit).
        /// </summary>
        private class WAPoint
        {
            public Vector2 Pos;
            public WAList Type;

            public WAPoint(Vector2 pos, WAList type)
            {
                Pos = new Vector2(pos.x, pos.y);
                Type = type;
            }
        }

        /// <summary>
        /// List the types of Weiler-Atherton vertices.
        /// Either vertex of original polygon, entry intersection, or exit intersection.
        /// </summary>
        private enum WAList { Vertex, Entry, Exit, Ignore};
    }
}
