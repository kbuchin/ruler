namespace Util.Algorithms.DCEL
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Algorithms.Polygon;
    using Util.Geometry;
    using Util.Geometry.DCEL;
    using Util.Geometry.Duality;
    using Util.Geometry.Polygon;
    using Util.Math;

    /// <summary>
    /// Collection of algorithms for finding cut lines through a collection of points.
    /// Uses point-line duality to find the middle regions between the lines in the dual.
    /// Finds line of greatest separation.
    /// </summary>
    public static class HamSandwich
    {
        public static List<Line> FindCutLines(IEnumerable<Vector2> a_points)
        {
            // obtain dual lines for points
            var lines = PointLineDual.Dual(a_points);

            // calculate bounding box around line intersections with some margin
            var bBox = BoundingBoxComputer.FromLines(lines, 10f);

            // calculate dcel for line inside given bounding box
            var m_dcel = new DCEL(lines, bBox);

            // find faces in the middle of the lines vertically and calculate cut lines
            var faces = MiddleFaces(m_dcel, lines);
            return FindCutlinesInDual(faces);
        }

        /// <summary>
        /// Find cut lines through each face of a dcel.
        /// </summary>
        /// <param name="a_region"></param>
        /// <returns></returns>
        public static List<Line> FindCutlinesInDual(IEnumerable<Face> a_region)
        {
            return FindCutLinesInDual(a_region.Select(f => f.Polygon.Outside));
        }

        /// <summary>
        /// Find cut lines through each polygon.
        /// A polygon represents an area bounded by lines in the dual plane.
        /// The cut line will be a point in the dual plane, being the line of greatest seperation in the real plane.
        /// </summary>
        /// <param name="a_region"></param>
        /// <returns></returns>
        public static List<Line> FindCutLinesInDual(IEnumerable<Polygon2D> a_region)
        {
            if (a_region.Count() <= 0) // no valid faces are supplied
            {
                return new List<Line>();
            }

            //facebased approach
            var lines = new List<Line>();
            foreach (var poly in a_region.Skip(1).Take(a_region.Count() - 2)) //Treat faces on the bounding box separately
            {
                var line = Separator.LineOfGreatestMinimumSeparationInTheDual(poly, false).Line;
                if (line == null)
                {
                    throw new GeomException("Polygon should have a seperation line");
                }
                lines.Add(line);
            }

            // Solve bounding box cases (Take only the line with the greatest separation)
            var firstBoundingboxPoly = a_region.ElementAt(0);
            var lastBoundingboxPoly = a_region.ElementAt(a_region.Count() - 1);
            var firstTuple = Separator.LineOfGreatestMinimumSeparationInTheDual(firstBoundingboxPoly, true);
            var lastTuple = Separator.LineOfGreatestMinimumSeparationInTheDual(lastBoundingboxPoly, true);
            if (firstTuple.Separation > lastTuple.Separation)
            {
                lines.Add(firstTuple.Line);
            }
            else
            {
                lines.Add(lastTuple.Line);
            }

            return lines;
        }

        /// <summary>
        /// Find cut lines that separates all point sets equally.
        /// Generates dcel of dual lines and generates cut lines through intersection of middle faces.
        /// </summary>
        /// <param name="a_points1"></param>
        /// <param name="a_points2"></param>
        /// <param name="a_points3"></param>
        /// <returns></returns>
        public static List<Line> FindCutLines(IEnumerable<Vector2> a_points1, IEnumerable<Vector2> a_points2,
            IEnumerable<Vector2> a_points3)
        {
            // obtain dual lines for game objects
            var lines1 = PointLineDual.Dual(a_points1);
            var lines2 = PointLineDual.Dual(a_points2);
            var lines3 = PointLineDual.Dual(a_points3);

            // add lines together
            var allLines = lines1.Concat(lines2.Concat(lines3));

            // calculate bounding box around line intersections with some margin
            var bBox = BoundingBoxComputer.FromLines(allLines, 10f);

            // calculate dcel for line inside given bounding box
            var dcel1 = new DCEL(lines1, bBox);
            var dcel2 = new DCEL(lines2, bBox);
            var dcel3 = new DCEL(lines3, bBox);

            // find faces in the middle of the lines vertically
            var archerFaces = MiddleFaces(dcel1, lines1);
            var swordsmenFaces = MiddleFaces(dcel2, lines2);
            var mageFaces = MiddleFaces(dcel3, lines3);

            // obtain cut lines for the dcel middle faces
            return FindCutlinesInDual(archerFaces, swordsmenFaces, mageFaces);
        }

        /// <summary>
        /// Find cut line through the intersection of dcel faces.
        /// Each dcel face is bounded by lines in the dual plane, representing the points to separate equally.
        /// </summary>
        /// <param name="a_region1"></param>
        /// <param name="a_region2"></param>
        /// <param name="a_region3"></param>
        /// <returns></returns>
        public static List<Line> FindCutlinesInDual(List<Face> a_region1, List<Face> a_region2, List<Face> a_region3)
        {
            a_region1.Sort((f1, f2) => f1.BoundingBox().xMin.CompareTo(f2.BoundingBox().xMin));
            a_region2.Sort((f1, f2) => f1.BoundingBox().xMin.CompareTo(f2.BoundingBox().xMin));
            a_region3.Sort((f1, f2) => f1.BoundingBox().xMin.CompareTo(f2.BoundingBox().xMin));

            var region1 = a_region1.Select(x => x.Polygon.Outside).ToList();
            var region2 = a_region2.Select(x => x.Polygon.Outside).ToList();
            var region3 = a_region3.Select(x => x.Polygon.Outside).ToList();

            var intermediateList = new List<Polygon2D>();

            //Intersect first two lists
            var list1index = 0;
            var list2index = 0;

            while (list1index < region1.Count && list2index < region2.Count)
            {
                //progress trough y coordinates
                var intersection = Intersector.IntersectConvex(region1[list1index], region2[list2index]);
                if (intersection != null)
                {
                    intermediateList.Add(intersection);
                }

                if (region2[list2index].BoundingBox().xMax < region1[list1index].BoundingBox().xMax)
                {
                    list2index++;
                }
                else
                {
                    list1index++;
                }
            }

            var result = new List<Polygon2D>();
            //Intersect intermediate list and last list
            var intermediateIndex = 0;
            var list3index = 0;

            while (intermediateIndex < intermediateList.Count && list3index < region3.Count)
            {
                //progress trough y coordinates
                var intersection = Intersector.IntersectConvex(intermediateList[intermediateIndex], region3[list3index]);
                if (intersection != null)
                {
                    result.Add(intersection);
                }

                if (region3[list3index].BoundingBox().xMax < intermediateList[intermediateIndex].BoundingBox().xMax)
                {
                    list3index++;
                }
                else
                {
                    intermediateIndex++;
                }
            }

            //Convert polygons to lines
            return FindCutLinesInDual(result);
        }

        /// <summary>
        /// Finds faces that are vertically right in middle of the dual lines in the given dcel.
        /// </summary>
        /// <remarks>
        /// Output may be unexpected for vertical lines.
        /// Assumes the dcel was constructed using the given dual lines, 
        /// with a bounding box with a slight margin (to avoid vertices with too many intersecting lines)
        /// </remarks>
        /// <param name="m_dcel"></param>
        /// <returns></returns>
        public static List<Face> MiddleFaces(DCEL m_dcel, IEnumerable<Line> m_dualLines)
        {
            if (m_dcel == null) return new List<Face>();

            var workingEdge = LeftMostMiddleHalfEdge(m_dcel, m_dualLines);

            var middleFaces = new List<Face>();
            // iterate trough the faces until we hit the outer face again
            while (workingEdge.Face != m_dcel.OuterFace)
            {
                // get edge of face on the right

                if (middleFaces.Contains(workingEdge.Face))
                {
                    throw new GeomException("Middle face added twice");
                }
                middleFaces.Add(workingEdge.Face);

                // loop until finding edge to rightmost vertex of face
                var dbstartedge = workingEdge;
                while (!IsEdgeLeadingToRightMostVertex(workingEdge))
                {
                    workingEdge = workingEdge.Next;
                    Debug.Assert((workingEdge != dbstartedge), "Returned to starting Edge");
                }

                if (workingEdge.Twin.Face == m_dcel.OuterFace) break;

                workingEdge = workingEdge.Twin.Prev.Twin;  // Goes from  *\ / to \/*
            }

            return middleFaces;
        }

        /// <summary>
        /// Returns a halfedge of the leftmost middle face.
        /// </summary>
        /// <param name="m_dcel"></param>
        /// <param name="m_dualLines"></param>
        /// <returns></returns>
        private static HalfEdge LeftMostMiddleHalfEdge(DCEL m_dcel, IEnumerable<Line> m_dualLines)
        {
            var bbox = m_dcel.InitBoundingBox.Value;

            // get all intersections with left line of bounding box
            var leftLine = new Line(new Vector2(bbox.xMin, bbox.yMin), new Vector2(bbox.xMin, bbox.yMax));
            var intersections = m_dualLines
                .Where(l => !l.IsVertical)
                .Select(l => leftLine.Intersect(l).Value)
                .ToList();

            // sort on y value
            intersections.Sort((v1, v2) => v1.y.CompareTo(v2.y));

            // find edge between middle intersections
            var middleIntersectBot = intersections[intersections.Count / 2 - 1];
            var middleIntersectTop = intersections[intersections.Count / 2];
            var edgeInterval = new FloatInterval(middleIntersectBot.y, middleIntersectTop.y);

            // get corresponding halfedge in dcel
            HalfEdge workingEdge;
            if (bbox.yMin < edgeInterval.Min && bbox.yMax > edgeInterval.Max)
            {
                // both intersections contained on left segment of bounding box

                workingEdge = DCEL.Cycle(m_dcel.OuterFace.InnerComponents[0])
                   .FirstOrDefault(e => e.From.Pos.x == bbox.xMin &&
                       edgeInterval.ContainsEpsilon(e.From.Pos.y) &&
                       edgeInterval.ContainsEpsilon(e.To.Pos.y));
            }
            else if (bbox.yMin >= edgeInterval.Min)
            {
                // bottom intersection lies below left segment

                // find halfedge pointing to bottom left corner
                workingEdge = DCEL.Cycle(m_dcel.OuterFace.InnerComponents[0])
                   .FirstOrDefault(e => MathUtil.Equals(e.From.Pos, new Vector2(bbox.xMin, bbox.yMin)));

                // traverse outer cycle until we find line with the corresponding bottom intersection
                while (workingEdge.Twin.Prev.Segment.Line.IsVertical ||
                    !MathUtil.EqualsEps(workingEdge.Twin.Prev.Segment.Line.Intersect(leftLine).Value, middleIntersectBot))
                {
                    workingEdge = workingEdge.Next;
                }
            }
            else
            {
                // top intersection lies above left segment

                // find halfedge pointing away from top left corner
                workingEdge = DCEL.Cycle(m_dcel.OuterFace.InnerComponents[0])
                  .FirstOrDefault(e => MathUtil.Equals(e.To.Pos, new Vector2(bbox.xMin, bbox.yMax)));

                // traverse outer cycle until we find line with the corresponding top intersection
                while (workingEdge.Twin.Next.Segment.Line.IsVertical ||
                    !MathUtil.EqualsEps(workingEdge.Twin.Next.Segment.Line.Intersect(leftLine).Value, middleIntersectTop))
                {
                    workingEdge = workingEdge.Prev;
                }
            }

            // middle edge should always exist
            if (workingEdge == null)
            {
                throw new GeomException("working edge not found");
            }

            // return twin (edge of middle face, not outer face)
            return workingEdge.Twin;
        }

        /// <summary>
        /// Check whether given edge leads to the rightmost vertex.
        /// Edge is unique if there are no vertical edges.
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        private static bool IsEdgeLeadingToRightMostVertex(HalfEdge edge)
        {
            return MathUtil.LEQEps(edge.From.Pos.x, edge.To.Pos.x) &&
                MathUtil.GEQEps(edge.Next.From.Pos.x, edge.Next.To.Pos.x);
        }
    }
}
