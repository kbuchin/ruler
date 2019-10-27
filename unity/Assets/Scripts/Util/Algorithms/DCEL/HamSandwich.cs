namespace Util.Algorithms.DCEL
{
    using System;
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
        public static List<Line> FindCutlines(IEnumerable<Vector2> a_points)
        {
            // obtain dual lines for points
            var lines = PointLineDual.Dual(a_points);

            // calculate bounding box around line intersections with some margin
            var bBox = BoundingBoxComputer.FromLines(lines, 10f);

            // calculate dcel for line inside given bounding box
            var m_dcel = new DCEL(lines, bBox);

            // find faces in the middle of the lines vertically and calculate cut lines
            var faces = MiddleFaces(m_dcel);
            return FindCutlinesInDual(faces);
        }

        /// <summary>
        /// Find cut lines through each face of a dcel.
        /// </summary>
        /// <param name="a_region"></param>
        /// <returns></returns>
        public static List<Line> FindCutlinesInDual(IEnumerable<Face> a_region)
        {
            return FindCutlinesInDual(a_region.Select(f => f.Polygon.Outside));
        }

        /// <summary>
        /// Find cut lines through each polygon.
        /// A polygon represents an area bounded by lines in the dual plane.
        /// The cut line will be a point in the dual plane, being the line of greatest seperation in the real plane.
        /// </summary>
        /// <param name="a_region"></param>
        /// <returns></returns>
        public static List<Line> FindCutlinesInDual(IEnumerable<Polygon2D> a_region)
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
            var archerFaces = MiddleFaces(dcel1);
            var swordsmenFaces = MiddleFaces(dcel2);
            var mageFaces = MiddleFaces(dcel3);

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
            //Assume each list of faces has an strict y-order (i.e. each aface is above the other)
            a_region1.Sort((f1, f2) => f1.BoundingBox().yMin.CompareTo(f2.BoundingBox().yMin));
            a_region2.Sort((f1, f2) => f1.BoundingBox().yMin.CompareTo(f2.BoundingBox().yMin));
            a_region3.Sort((f1, f2) => f1.BoundingBox().yMin.CompareTo(f2.BoundingBox().yMin));

            //assert this 
            for (int i = 0; i < a_region1.Count - 1; i++)
            {
                if (!MathUtil.EqualsEps(a_region1[i].BoundingBox().yMax, a_region1[i + 1].BoundingBox().yMin))
                {
                    throw new GeomException("List has no unique y-order " + a_region1[i].BoundingBox().yMax + " " + a_region1[i + 1].BoundingBox().yMin);
                }
            }
            for (int i = 0; i < a_region2.Count - 1; i++)
            {
                if (!MathUtil.EqualsEps(a_region2[i].BoundingBox().yMax, a_region2[i + 1].BoundingBox().yMin))
                {
                    throw new GeomException("List has no unique y-order " + a_region2[i].BoundingBox().yMax + " " + a_region2[i + 1].BoundingBox().yMin);
                }
            }
            for (int i = 0; i < a_region3.Count - 1; i++)
            {
                if (!MathUtil.EqualsEps(a_region3[i].BoundingBox().yMax, a_region3[i + 1].BoundingBox().yMin))
                {
                    throw new GeomException("List has no unique y-order" + a_region3[i].BoundingBox().yMax + " " + a_region3[i + 1].BoundingBox().yMin);
                }
            }

            var region1 = a_region1.Select(x => x.Polygon).ToList();
            var region2 = a_region2.Select(x => x.Polygon).ToList();
            var region3 = a_region3.Select(x => x.Polygon).ToList();

            var intermediateList = new List<Polygon2D>();
            //Intersect first two lists
            var list1index = 0;
            var list2index = 0;

            do
            {
                //progress trough y coordinates
                var intersection = Polygon2D.IntersectConvex(region1[list1index].Outside, region2[list2index].Outside);
                if (intersection != null)
                {
                    intermediateList.Add(intersection);
                }

                if (region2[list2index].BoundingBox().yMax < region1[list1index].BoundingBox().yMax)
                {
                    list2index++;
                }
                else
                {
                    list1index++;
                }
            }
            while (list1index < region1.Count && list2index < region2.Count);

            var result = new List<Polygon2D>();
            //Intersect intermediate list and last list
            var intermediateIndex = 0;
            var list3index = 0;

            do
            {
                //progress trough y coordinates
                var intersection = Polygon2D.IntersectConvex(intermediateList[intermediateIndex], region3[list3index].Outside);
                if (intersection != null)
                {
                    result.Add(intersection);
                }

                if (region3[list3index].BoundingBox().yMax < intermediateList[intermediateIndex].BoundingBox().yMax)
                {
                    list3index++;
                }
                else
                {
                    intermediateIndex++;
                }
            }
            while (intermediateIndex < intermediateList.Count && list3index < region3.Count);

            //Convert polygons to lines
            return FindCutlinesInDual(result);
        }

        /// <summary>
        /// Finds faces that are vertically right in middle of the dual lines in the given dcel.
        /// </summary>
        /// <param name="m_dcel"></param>
        /// <returns></returns>
        public static List<Face> MiddleFaces(DCEL m_dcel)
        {
            if (m_dcel == null) return new List<Face>();

            //returns the faces in which the dual point may lie to represent a cut
            // cutting a given army in two equal parts in the primal plane.
            var workingedge = m_dcel.OuterFace.InnerComponents[0];
            var bbox = m_dcel.InitBoundingBox.Value;

            var lineIntersectionEdges = new List<HalfEdge>();                //Will contain edges whose From is an intersection with a line

            while (!(MathUtil.EqualsEps(workingedge.From.Pos.x, bbox.xMin) && workingedge.From.Pos.y > 0 && workingedge.To.Pos.y < 0))
            {
                workingedge = workingedge.Next;
            }
            //only one edge satisfies the above conditions the edge on the left boundray first crossing the origin line.

            workingedge = workingedge.Next; //workingedge is now the first edge with both from.y and to.y <0

            while (workingedge.From.Pos.y < 0)
            {
                if (MathUtil.EqualsEps(workingedge.From.Pos.y, bbox.yMin) && (MathUtil.EqualsEps(workingedge.From.Pos.x, bbox.xMin) || MathUtil.EqualsEps(workingedge.From.Pos.x, bbox.xMax)))
                {
                    //From is a corner. Do not add to prevent duplicity
                }
                else
                {
                    lineIntersectionEdges.Add(workingedge);
                }
                workingedge = workingedge.Next;
            }

            if (lineIntersectionEdges.Count % 2 == 1)
            {
                Debug.LogError("Unexpected odd number of lineIntersectionedges  " + lineIntersectionEdges.Count);
            }

            //TODO Assumption, feasibleFaces are arrenged in a vertical manner!

            var middleEdge = lineIntersectionEdges[(lineIntersectionEdges.Count / 2) - 1];
            var startingFace = middleEdge.Twin.Face;
            var midllefaces = new List<Face>
            {
                startingFace
            };

            //itrate trough the faces until we hit the outer face again
            workingedge = middleEdge.Twin;
            while (true)
            {
                var dbstartedge = workingedge;
                while (!IsEdgeLeadingToTopMostVertex(workingedge))
                {
                    workingedge = workingedge.Next;
                    Debug.Assert((workingedge != dbstartedge), "OMG returned to starting Edge");
                }
                if (workingedge.Twin.Face == m_dcel.OuterFace)
                {
                    //hit the left or right side
                    break;
                }
                workingedge = workingedge.Twin.Prev.Twin;  // Goes from  *\/ to \/*
                if (workingedge.Face == m_dcel.OuterFace)
                {
                    break;
                }
                else
                {
                    midllefaces.Add(workingedge.Face);
                    if (midllefaces.Count > 100)
                    {
                        throw new System.Exception("Unexpected large amount of feasible faces");
                    }
                }
            }
            return midllefaces;
        }

        private static bool IsEdgeLeadingToTopMostVertex(HalfEdge edge)
        {
            return MathUtil.LEQEps(edge.From.Pos.y, edge.To.Pos.y) && 
                MathUtil.GEQEps(edge.Next.From.Pos.y, edge.Next.To.Pos.y);
        }
    }
}
