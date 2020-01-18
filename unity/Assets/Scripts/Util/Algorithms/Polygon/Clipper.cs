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
            if (WA.Entry.Count == 0)
            {
                if (WA.Subject.Vertices.ToList().Exists(p => !(WA.Clip.ContainsInside(p) || WA.Clip.OnBoundary(p))))
                    multiPoly.AddPolygon(new Polygon2D(WA.Subject.Vertices));
            }

            while (WA.Entry.Count > 0)
            {
                var vertices = new List<Vector2>();
                var visited = new HashSet<LinkedListNode<WAPoint>>();

                // remove an entry intersection point
                var startPoint = WA.Entry.LastOrDefault();
                WA.Entry.Remove(startPoint);

                LinkedListNode<WAPoint> node;
                try
                {
                    node = WA.ClipNode[startPoint];
                } 
                catch (KeyNotFoundException e)
                {
                    Debug.Log("entry point not found: " + e);
                    continue;
                }

                WAPoint last = null;

                while (!visited.Contains(node))
                {
                    // remove entry intersection from starting list
                    WA.Entry.Remove(node.Value);

                    // traverse clip polygon in counter-clockwise order until exit vertex found
                    while (node.Value.Type != WAList.Exit && !visited.Contains(node))
                    {
                        // check for duplicates
                        if (last == null || !MathUtil.EqualsEps(node.Value.Pos, last.Pos))
                        {
                            vertices.Add(node.Value.Pos);
                            visited.Add(node);
                        }
                        last = node.Value;
                        node = node.Previous ?? WA.ClipList.Last;
                    }

                    // might contains entry but no exit vertex
                    // should not occur, return no polygon
                    if (visited.Contains(node)) { vertices.Clear(); break; }

                    try
                    {
                        node = WA.SubjectNode[node.Value];
                    }
                    catch (KeyNotFoundException e)
                    {
                        Debug.Log("exit point not found: " + e.StackTrace);
                        break;
                    }

                    // traverse subject polygon in clockwise order until new entr vertex found
                    while (node.Value.Type != WAList.Entry && !visited.Contains(node))
                    {
                        if (last == null || !MathUtil.EqualsEps(node.Value.Pos, last.Pos))
                        {
                            vertices.Add(node.Value.Pos);
                            visited.Add(node);
                        }
                        last = node.Value;
                        node = node.Next ?? WA.SubjectList.First;
                    }

                    if (visited.Contains(node)) break;

                    try
                    {
                        node = WA.ClipNode[node.Value];
                    }
                    catch (KeyNotFoundException e)
                    {
                        Debug.Log("entry point not found: " + e);
                        break;
                    }
                }

                // add new polygon from the vertices
                if (vertices.Count > 2) 
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
            public readonly HashSet<WAPoint> Entry;
            public readonly HashSet<WAPoint> Exit;

            public WeilerAtherton(Polygon2D a_subject, Polygon2D a_clip)
            {
                // initialize variables
                Subject = a_subject;
                Clip = a_clip;
                SubjectList = new LinkedList<WAPoint>(Subject.Vertices.Select(v => new WAPoint(v, WAList.Vertex)));
                ClipList = new LinkedList<WAPoint>(Clip.Vertices.Select(v => new WAPoint(v, WAList.Vertex)));
                SubjectNode = new Dictionary<WAPoint, LinkedListNode<WAPoint>>();
                ClipNode = new Dictionary<WAPoint, LinkedListNode<WAPoint>>();
                Entry = new HashSet<WAPoint>();
                Exit = new HashSet<WAPoint>();

                // find all intersections and create subject and clip list
                for (var node1 = SubjectList.First; node1 != null; node1 = node1.Next)
                {
                    // calculate subject segment
                    var next1 = node1.Next ?? SubjectList.First;
                    var seg1 = new LineSegment(node1.Value.Pos, next1.Value.Pos);

                    // obtain intersection lists
                    var intersections = new List<Vector2>();
                    var intersectNodes = new Dictionary<Vector2, LinkedListNode<WAPoint>>();
                    for (var node2 = ClipList.Last; node2 != null; node2 = node2.Previous)
                    {
                        var prev2 = node2.Previous ?? ClipList.Last;
                        var seg2 = new LineSegment(node2.Value.Pos, prev2.Value.Pos);

                        var intersect = seg1.Intersect(seg2);
                        if (intersect.HasValue && !intersectNodes.ContainsKey(intersect.Value))
                        {
                            // store intersection point
                            intersectNodes.Add(intersect.Value, node2);
                            intersections.Add(intersect.Value);
                        }
                    }

                    // sort intersections on distance to start vertex
                    intersections.Sort(new ClosestToPointComparer(seg1.Point1));

                    // insert intersections into subject/clip lists
                    foreach (var vertex in intersections)
                    {
                        var point = new WAPoint(vertex, WAList.Ignore);

                        var newNode1 = SubjectList.AddAfter(node1, point);
                        SubjectNode.Add(point, newNode1);

                        var newNode2 = ClipList.AddBefore(intersectNodes[vertex], point);
                        ClipNode.Add(point, newNode2);

                        // increment node1 since we added a vertex
                        node1 = newNode1;
                    }
                }

                // remove duplicates of subject
                for (var node = SubjectList.First; node != null;)
                {
                    var next = node.Next ?? SubjectList.First;
                    while (MathUtil.EqualsEps(node.Value.Pos, next.Value.Pos, MathUtil.EPS * 10))
                    {
                        if (next.Value.Type != WAList.Vertex)
                        {
                            if (ClipNode.ContainsKey(node.Value)) ClipList.Remove(ClipNode[node.Value]);
                            SubjectList.Remove(node);
                            node = next;
                        }
                        else
                        {
                            SubjectList.Remove(next);
                        }
                        if (node == null) break;
                        next = node.Next ?? SubjectList.First;
                    }
                    if (node != null) node = node.Next;
                }

                // remove duplicates of clip
                for (var node = ClipList.Last; node != null;)
                {
                    var prev = node.Previous ?? ClipList.Last;
                    while (MathUtil.EqualsEps(node.Value.Pos, prev.Value.Pos, MathUtil.EPS * 10))
                    {
                        if (prev.Value.Type != WAList.Vertex)
                        {
                            if (SubjectNode.ContainsKey(node.Value)) SubjectList.Remove(SubjectNode[node.Value]);
                            ClipList.Remove(node);
                            node = prev;
                        }
                        else
                        {
                            ClipList.Remove(prev);
                        }
                        if (node == null) break;
                        prev = node.Previous ?? ClipList.Last;
                    }
                    if (node != null) node = node.Previous;
                }

                // set entry/exit types correctly
                for (var node = SubjectList.First; node != null; node = node.Next)
                {
                    if (node.Value.Type == WAList.Vertex) continue; 

                    var prev = node.Previous ?? SubjectList.Last;
                    var next = node.Next ?? SubjectList.First;

                    var inside1 = Clip.ContainsInside(prev.Value.Pos) || Clip.OnBoundary(prev.Value.Pos);
                    var inside2 = Clip.ContainsInside(next.Value.Pos) || Clip.OnBoundary(next.Value.Pos);

                    if (!inside1 && inside2)
                    {
                        node.Value.Type = WAList.Entry;
                        Entry.Add(node.Value);
                    }
                    else if (inside1 && !inside2)
                    {
                        node.Value.Type = WAList.Exit;
                        Exit.Add(node.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Compares points based on distance to another point.
        /// Used for sorting intersection points on distance to start vertex.
        /// </summary>
        private class ClosestToPointComparer : IComparer<Vector2>
        {
            private Vector2 m_point;

            public ClosestToPointComparer(Vector2 a_point)
            {
                m_point = a_point;
            }

            public int Compare(Vector2 x, Vector2 y)
            {
                var dist_1 = Vector2.Distance(m_point, x);
                var dist_2 = Vector2.Distance(m_point, y);

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
