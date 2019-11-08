namespace Util.Algorithms.Polygon
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Geometry.Polygon;
    using Util.Math;

    /// <summary>
    /// Collection of algorithm related to cutting out polygons such that they no longer overlap.
    /// </summary>
    public static class Clipper
    {

        /// <summary>
        /// A implementation of the Weiler-Atherthon algorithm that cuts away the provided clippling area from the subject polygon
        /// Assumes the cutting polygon is not entirely inside subject polygon!
        /// </summary>
        /// <param name="a_subject"></param>
        /// <param name="a_clip"></param>
        /// <returns></returns>
        public static MultiPolygon2D CutOut(Polygon2D a_subject, Polygon2D a_clip)
        {
            if (!a_subject.IsClockwise())
            {
                a_subject = new Polygon2D(a_subject.Vertices.Reverse());
            }
            if (!a_clip.IsClockwise())
            {
                a_clip = new Polygon2D(a_clip.Vertices.Reverse());
            }

            var subjectList = WeilerAthertonList(a_subject, a_clip);
            var clipList = new LinkedList<Vector2>(WeilerAthertonList(a_clip, a_subject).Reverse());
            var intersectionList = WeilerAthertonIntersectionList(a_subject, a_clip);

            Debug.Assert(subjectList != null);

            if (intersectionList.Count == 0)
            {
                // either polygons is entirely inside the other, or they are disjoint.
                return new MultiPolygon2D(a_subject);
            }

            return WeilerAtherthonCutOut(subjectList, clipList, intersectionList);
        }

        /// <summary>
        /// Cuts a_cutPoly out of this polygon
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
        /// Creates a cutout given the relevant vector lists.
        /// Resulting in a polygon with holes
        /// </summary>
        /// <remarks>
        /// An eventual intersecting implementation of the Weiler-Atherthon algorithm should use selection on the startvertices.
        /// (i.e. taking only inbound edges, see also wikipedia)
        /// </remarks>
        /// <param name="a_subjectList"></param>
        /// <param name="a_clipList"></param>
        /// <param name="a_startVertexList">A list of points where we start tracing shapes. Double startpoints on the same shape will be removed. </param>
        /// <returns></returns>
        private static MultiPolygon2D WeilerAtherthonCutOut(LinkedList<Vector2> a_subjectList, LinkedList<Vector2> a_clipList, List<Vector2> a_startVertexList)
        {
            var result = new MultiPolygon2D();

            //PERF organise intersections better so i have to loop less over the lists
            while (a_startVertexList.Count > 0)
            {
                var start = a_startVertexList[0];
                var activeList = WAList.Subject;
                var singlePolyVertices = new List<Vector2>() { start };
                var startnode = a_subjectList.Find(start);

                if (startnode == null) //fallback
                {
                    foreach (var vertex in a_subjectList)
                    {
                        if (MathUtil.EqualsEps(vertex, start, MathUtil.EPS * 10)) //more liberal then Equals
                        {
                            startnode = a_subjectList.Find(vertex);
                        }
                    }
                }

                // incorrect input
                // fail softly
                if (startnode == null) return result;

                var workingvertex = startnode.Next ?? a_subjectList.First;

                while (workingvertex.Value != start)
                {
                    singlePolyVertices.Add(workingvertex.Value);

                    //check if workingvertex is an intersection, and in that case cross
                    LinkedListNode<Vector2> intersection;
                    if (activeList == WAList.Subject)
                    {
                        intersection = a_clipList.Find(workingvertex.Value);

                        //fallback
                        for (var node = a_clipList.First; node != null; node = node.Next)
                        {
                            if (MathUtil.EqualsEps(workingvertex.Value, node.Value))
                            {
                                intersection = node;
                                break;
                            }
                        }

                    }
                    else
                    {
                        intersection = a_subjectList.Find(workingvertex.Value);

                        //fallback
                        for (var node = a_subjectList.First; node != null; node = node.Next)
                        {
                            if (MathUtil.EqualsEps(workingvertex.Value, node.Value))
                            {
                                intersection = node;
                                break;
                            }
                        }
                    }

                    if (intersection != null)
                    {
                        //toggle activeList
                        activeList = activeList == WAList.Subject ? WAList.Clip : WAList.Subject;
                        workingvertex = intersection.Next;
                    }
                    //otherwise, advance in own list
                    else
                    {
                        workingvertex = workingvertex.Next;
                    }

                    // if workingvertex is null we called Next at the end of the list
                    if (workingvertex == null)
                    {
                        if (activeList == WAList.Subject)
                        {
                            workingvertex = a_subjectList.First;
                        }
                        else
                        {
                            Debug.Assert(activeList == WAList.Clip);
                            workingvertex = a_clipList.First;
                        }
                    }

                    if (singlePolyVertices.Count > 10000)
                    {
                        workingvertex.Value = start;
                        singlePolyVertices.Add(workingvertex.Value);
                        //throw new GeomException("nonterminating loop");
                    }

                }
                if (singlePolyVertices.Count > 2)  //only add if we find a nontrivial polygon
                {
                    var candidatePoly = new Polygon2D(singlePolyVertices);
                    if (candidatePoly.IsClockwise())
                    {
                        result.AddPolygon(candidatePoly);

                        //remove treated vertices
                        foreach (Vector2 vertex in singlePolyVertices)
                        {
                            a_startVertexList.Remove(vertex);
                        }
                    }
                    else
                    {
                        // we accidently found a hole (due to some colinearity issues)
                        //remove only starting vertex, the other vertex could yield a succes
                        a_startVertexList.Remove(start);
                    }
                }
                else
                {
                    //remove only starting vertex, the other vertex could yield a succes
                    a_startVertexList.Remove(start);
                }
            }

            return result;
        }


        /// <summary>
        /// Generates an in order list of all vertices of a_poly interlaced with it's intersections with a_intersectors in
        /// O(nm) with n the number of segments from the one polygon and m the number of segments of the other polygon
        /// </summary>
        /// <param name="a_poly"></param>
        /// <param name="a_intersector"></param>
        /// <returns></returns>
        private static LinkedList<Vector2> WeilerAthertonList(Polygon2D a_poly, Polygon2D a_intersector)
        {
            var intersectionList = new LinkedList<Vector2>();
            var intersectingSegments = a_intersector.Segments;

            foreach (var segment in a_poly.Segments)
            {
                intersectionList.AddLast(segment.Point1);
                var segIntersections = segment.Intersect(intersectingSegments);
                segIntersections.Sort(segment.ClosestToPoint1Comparer);
                foreach (var intersection in segIntersections) //no AddRange for LinkedList
                {
                    intersectionList.AddLast(intersection);
                }
            }

            if (intersectionList.Count <= 1) return intersectionList;

            //Clear subsequent (near) duplicates from intersectionlist
            var workingvertex = intersectionList.First;
            while (workingvertex != null)
            {
                var next = workingvertex.Next ?? intersectionList.First;
                if (MathUtil.EqualsEps(workingvertex.Value, next.Value))
                {
                    intersectionList.Remove(next);
                }
                else
                {
                    workingvertex = workingvertex.Next;
                }
            }

            return intersectionList;
        }



        /// <summary>
        /// Generates an in order list of all vertices of a_poly interlaced with it's intersections with a_intersectors
        /// O(nm) with n the number of segments from the one polygon and m the number of segments of the other polygon
        /// 
        /// NOTE: We retrurn all inersections since edge cases and collinear sitautions are hard to detect properly.
        /// Instead we later check if a given intersection was of th ight type by checking if the resulting polygon is clockwise 
        /// </summary>
        /// <param name="a_subject"></param>
        /// <param name="a_clip"></param>
        /// <returns>Those intersections where subject enters or exits the clipping polygon</returns>
        /// 
        private static List<Vector2> WeilerAthertonIntersectionList(Polygon2D a_subject, Polygon2D a_clip)
        {
            var intersectionList = new List<Vector2>();
            var intersectingSegments = a_subject.Segments;

            foreach (var segment in a_clip.Segments)
            {
                var segIntersections = segment.Intersect(intersectingSegments);
                segIntersections.Sort(segment.ClosestToPoint1Comparer);
                intersectionList.AddRange(segIntersections);
            }

            if (intersectionList.Count <= 1) return intersectionList;

            //Clear subsequent duplicates from intersectionlist
            for (var i = 0; i < intersectionList.Count; i++)
            {
                var next = (i + 1) % intersectionList.Count;
                if (Vector2.Distance(intersectionList[i], intersectionList[next]) < MathUtil.EPS)
                {
                    intersectionList.RemoveAt(next);

                    // decrement i to counteract next for-loop iteration
                    i--;
                }
            }

            return intersectionList;
        }

        /// <summary>
        /// List the types of Weiler-AthertonLists
        /// </summary>
        private enum WAList { Subject, Clip };
    }
}
