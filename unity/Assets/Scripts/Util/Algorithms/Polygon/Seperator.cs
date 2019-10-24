namespace Util.Algorithms.Polygon
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry;
    using Util.Geometry.Duality;
    using Util.Geometry.Polygon;
    using Util.Math;

    public static class Seperator
    {
        /// <summary>
        /// Return the Line that gives the greatest distance from all points in the dual. We calculate true seperation and not vertical seperation
        /// No error checking occurs for bounding box faces/polygons
        /// </summary>
        /// <returns>Line with greatest distance to the points encoded by the polygon boundary </returns>
        public static LineSeperationTuple LineOfGreatestMinimumSeperationInTheDual(Polygon2D polygon, bool a_isBoundingBoxFace)
        {
            if (!polygon.IsConvex())
            {
                throw new GeomException("Only support computing lines of greatest seperation for convex polygons");
            }
            
            if (!polygon.IsClockwise())
            {
                polygon = new Polygon2D(polygon.Vertices.Reverse());
            }

            var upperhull = ConvexHull.ComputeUpperHull(polygon).ToList();
            var lowerhull = ConvexHull.ComputeLowerHull(polygon).ToList();

            if (a_isBoundingBoxFace)
            {
                //Check if the boundingboxface has only 2 real neighbouring lines(in the dual) and return the bisector (in the primal) in this case 
                var reallines = polygon.Segments.Where(seg => 
                        !(float.IsInfinity(seg.Line.Slope) || MathUtil.EqualsEps(seg.Line.Slope, 0f)))
                    .Select(seg => seg.Line)
                    .ToList();

                if (reallines.Count < 2)
                {
                    Debug.Log(polygon);
                    throw new GeomException("Found impossibly low amount of real lines");
                }
                else if (reallines.Count == 2)
                {
                    //The two reallines are two points in the primal plane
                    var averagepoint = (PointLineDual.Dual(reallines[0]) + PointLineDual.Dual(reallines[1])) / 2;
                    var lineTroughBothPoints = PointLineDual.Dual((Vector2)reallines[0].Intersect(reallines[1]));
                    var perpLineSlope = -1 / lineTroughBothPoints.Slope;
                    var perpPoint = averagepoint + Vector2.right + Vector2.up * perpLineSlope;
                    return new LineSeperationTuple(new Line(averagepoint, perpPoint), 0);

                    //we choose separtion 0 because a line with three constraints in the outer boundingboxface is more important?
                    //TODO this seems untrue, explictly calculate seperation (Wrt to all soldier??)
                }
                //Otherwise we return the regular bisector
            }


            //zero is the starting corner, 1 is the first intresting point
            var upperhullIterator = 0;
            var lowerhullIterator = 0;
            Vector2 candidatePoint = new Vector2(0, 0); //dummy value
            var currentSeparation = 0f;

            float currentx = upperhull[0].x;
            float nextx;
            float currentheight = 0;
            float nextheight;

            LineSegment upperSegment = null;
            LineSegment lowerSegment = null;

            Action<float, float> testCandidatePoint = delegate (float testx, float testheight)
            {
                var trueSeperation = Mathf.Sin(Mathf.PI / 2 - Mathf.Abs(Mathf.Atan(testx))) * testheight; //Atan(x) is the angle a line of slope x makes
                if (trueSeperation > currentSeparation)
                {
                    candidatePoint = new Vector2(testx, (upperSegment.Y(testx) + lowerSegment.Y(testx)) / 2);
                    currentSeparation = trueSeperation;
                    return;
                }
            };

            //initialize segments
            lowerSegment = new LineSegment(lowerhull[0], lowerhull[1]);
            upperSegment = new LineSegment(upperhull[0], upperhull[1]);

            //Break when one of the two lists is completly traversed
            while (upperhullIterator < upperhull.Count - 1 && lowerhullIterator < lowerhull.Count - 1)
            {
                //The part between currentx(exclusive) and nextx(inclusive) is under scrutiny
                //we advance the segment that ends on the smallest x
                if (upperhull[upperhullIterator].x < lowerhull[lowerhullIterator].x)
                {
                    upperhullIterator++;
                    upperSegment = new LineSegment(upperhull[upperhullIterator - 1], upperhull[upperhullIterator]);
                }
                else
                {
                    lowerhullIterator++;
                    lowerSegment = new LineSegment(lowerhull[lowerhullIterator - 1], lowerhull[lowerhullIterator]);
                }

                if (lowerSegment.IsVertical || upperSegment.IsVertical)
                {
                    continue; //skip this iteration 
                }

                nextx = Mathf.Min(upperSegment.XInterval.Max, lowerSegment.XInterval.Max);

                nextheight = upperSegment.Y(nextx) - lowerSegment.Y(nextx);
                if (nextheight < 0)
                {
                    throw new GeomException();
                }
                testCandidatePoint(nextx, nextheight);

                //also points inbetween vertices
                float heightchange = (nextheight - currentheight) / (nextx - currentx);
                float baseheigth = nextheight - nextx * heightchange;

                float candidatex = heightchange / baseheigth;
                if (currentx < candidatex && candidatex < nextx)
                {
                    var candidateheigth = baseheigth + heightchange * candidatex;
                    testCandidatePoint(candidatex, candidateheigth);
                }

                //save variables for next iteration
                currentheight = nextheight;
                currentx = nextx;
            }

            return new LineSeperationTuple(PointLineDual.Dual(candidatePoint), currentSeparation);
        }
    }

    public struct LineSeperationTuple
    {
        public float Seperation { get; private set; }

        public Line Line { get; private set; }

        public LineSeperationTuple(Line line, float currentSeparation)
        {
            Line = line;
            Seperation = currentSeparation;
        }
    }
}
