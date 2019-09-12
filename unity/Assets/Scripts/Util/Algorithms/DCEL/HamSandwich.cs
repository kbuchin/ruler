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
            return FindCutlines(a_region.Select(f => f.Polygon));
        }

        public static List<Line> FindCutlines(IEnumerable<Polygon2D> a_region)
        {
            if (a_region.Count() <= 0) // no valid faces are supplied
            {
                Debug.Log("no valid faces");
                return new List<Line>();
            }

            //facebased approach
            var lines = new List<Line>();
            foreach (var poly in a_region.Skip(1).Take(a_region.Count() - 2)) //Treat faces on the bounding box separately
            {
                var line = Seperator.LineOfGreatestMinimumSeperationInTheDual(poly, false).Line;
                if (line == null)
                {
                    throw new GeomException();
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
                    throw new GeomException("List has no unique y-order");
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

            while (list1index < region1.Count || list2index < region2.Count)
            {
                //progress trough y coordinates
                var intersection = Polygon2D.IntersectConvex(region1[list1index], region2[list2index]);
                if (intersection != null)
                {
                    intermediateList.Add(intersection);
                }

                if (BoundingBox.FromPolygon(region2[list2index]).yMax <
                    BoundingBox.FromPolygon(region1[list1index]).yMax)
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

            while (intermediateIndex < intermediateList.Count || list3index < region3.Count)
            {
                //progress trough y coordinates
                var intersection = Polygon2D.IntersectConvex(intermediateList[intermediateIndex], region3[list3index]);
                if (intersection != null)
                {
                    result.Add(intersection);
                }

                if (BoundingBox.FromPolygon(region3[list3index]).yMax <
                    BoundingBox.FromPolygon(intermediateList[intermediateIndex]).yMax)
                {
                    list3index++;
                }
                else
                {
                    intermediateIndex++;
                }
            }

            //Convert polygons to lines
            return FindCutlines(result);
        }
    }
}
