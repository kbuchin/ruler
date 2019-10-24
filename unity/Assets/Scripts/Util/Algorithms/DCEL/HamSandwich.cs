namespace Util.Algorithms.DCEL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Algorithms.Polygon;
    using Util.Geometry;
    using Util.Geometry.DCEL;
    using Util.Geometry.Polygon;
    using Util.Math;

    public static class HamSandwich
    {
        public static List<Line> FindCutlines(IEnumerable<Face> a_region)
        {
            return FindCutlines(a_region.Select(f => f.Polygon.Outside));
        }

        public static List<Line> FindCutlines(IEnumerable<Polygon2D> a_region)
        {
            if (a_region.Count() <= 0) // no valid faces are supplied
            {
                return new List<Line>();
            }

            //facebased approach
            var lines = new List<Line>();
            foreach (var poly in a_region.Skip(1).Take(a_region.Count() - 2)) //Treat faces on the bounding box separately
            {
                var line = Seperator.LineOfGreatestMinimumSeperationInTheDual(poly, false).Line;
                if (line == null)
                {
                    throw new GeomException("Polygon should have a seperation line");
                }
                lines.Add(line);
            }

            // Solve bounding box cases (Take only the line with the greatest separation)
            var firstBoundingboxPoly = a_region.ElementAt(0);
            var lastBoundingboxPoly = a_region.ElementAt(a_region.Count() - 1);
            var firstTuple = Seperator.LineOfGreatestMinimumSeperationInTheDual(firstBoundingboxPoly, true);
            var lastTuple = Seperator.LineOfGreatestMinimumSeperationInTheDual(lastBoundingboxPoly, true);
            if (firstTuple.Seperation > lastTuple.Seperation)
            {
                lines.Add(firstTuple.Line);
            }
            else
            {
                lines.Add(lastTuple.Line);
            }
            foreach (var l in lines) { Debug.Log(l); }
            return lines;
        }

        public static List<Line> FindCutlines(IEnumerable<Face> a_region1, IEnumerable<Face> a_region2, IEnumerable<Face> a_region3)
        {
            return FindCutlines(a_region1.ToList(), a_region2.ToList(), a_region3.ToList());
        }

        public static List<Line> FindCutlines(List<Face> a_region1, List<Face> a_region2, List<Face> a_region3)
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

                if (BoundingBoxComputer.FromPolygon(region2[list2index]).yMax <
                    BoundingBoxComputer.FromPolygon(region1[list1index]).yMax)
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

                if (BoundingBoxComputer.FromPolygon(region3[list3index]).yMax <
                    BoundingBoxComputer.FromPolygon(intermediateList[intermediateIndex]).yMax)
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
            foreach (var poly in result) { Debug.Log(poly); }
            return FindCutlines(result);
        }

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
                while (!isEdgeLeadingToTopMostVertex(workingedge))
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

        private static bool isEdgeLeadingToTopMostVertex(HalfEdge edge)
        {
            var epsilon = 0.0005f;
            if (edge.From.Pos.y - epsilon <= edge.To.Pos.y && edge.Next.From.Pos.y >= edge.Next.To.Pos.y - epsilon)
            {
                return true;
            }
            return false;
        }
    }
}
