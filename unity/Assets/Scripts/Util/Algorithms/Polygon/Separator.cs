namespace Util.Algorithms.Polygon
{
    using System;
    using System.Linq;
    using UnityEngine;
    using Util.Geometry;
    using Util.Geometry.Duality;
    using Util.Geometry.Polygon;

    /// <summary>
    /// Collection of algorithms related to seperation.
    /// </summary>
    public static class Separator
    {
        /// <summary>
        /// Return the Line that gives the greatest distance from all points in the dual. We calculate true seperation and not vertical seperation
        /// No error checking occurs for bounding box faces/polygons
        /// </summary>
        /// <returns>Line with greatest distance to the points encoded by the polygon boundary </returns>
        public static LineSeparation LineOfGreatestMinimumSeparationInTheDual(Polygon2D polygon, bool a_isBoundingBoxFace)
        {
            // copy for safety
            var poly = new Polygon2D(polygon.Vertices);

            if (!poly.IsConvex())
            {
                throw new GeomException("Only support computing lines of greatest seperation for convex polygons");
            }

            if (!poly.IsClockwise())
            {
                poly = new Polygon2D(poly.Vertices.Reverse());
            }

            var upperhull = ConvexHull.ComputeUpperHull(poly).ToList();
            var lowerhull = ConvexHull.ComputeLowerHull(poly).ToList();

            if (a_isBoundingBoxFace)
            {
                //Check if the boundingboxface has only 2 real neighbouring lines(in the dual) and return the bisector (in the primal) in this case 
                var reallines = poly.Segments
                    .Select(seg => seg.Line)
                    .Where(line => !line.IsHorizontal && !line.IsVertical)
                    .ToList();

                if (reallines.Count < 2)
                {
                    throw new GeomException("Found impossibly low amount of real lines");
                }
                else if (reallines.Count == 2)
                {
                    //The two reallines are two points in the primal plane
                    // get intersection
                    // Assumes general positions of initial points
                    var averagepoint = (PointLineDual.Dual(reallines[0]) + PointLineDual.Dual(reallines[1])) / 2;
                    var lineTroughBothPoints = PointLineDual.Dual(reallines[0].Intersect(reallines[1]).Value);
                    var perpLineSlope = -1 / lineTroughBothPoints.Slope;
                    var perpPoint = averagepoint + Vector2.right + Vector2.up * perpLineSlope;

                    return new LineSeparation(new Line(averagepoint, perpPoint), 0);

                    //we choose separtion 0 because a line with three constraints in the outer boundingboxface is more important?
                    //TODO this seems untrue, explictly calculate seperation (Wrt to all soldier??)
                }
                //Otherwise we return the regular bisector
            }


            //zero is the starting corner, 1 is the first interesting point
            var upperhullIterator = 0;
            var lowerhullIterator = 0;
            var candidatePoint = new Vector2(0, 0); //dummy value
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
                if (new FloatInterval(currentx, nextx).Contains(candidatex))
                {
                    var candidateheigth = baseheigth + heightchange * candidatex;
                    testCandidatePoint(candidatex, candidateheigth);
                }

                //save variables for next iteration
                currentheight = nextheight;
                currentx = nextx;
            }

            return new LineSeparation(PointLineDual.Dual(candidatePoint), currentSeparation);
        }
    }

    /// <summary>
    /// Struct for storing line-separation tuple
    /// </summary>
    public struct LineSeparation
    {
        /// <summary>
        /// Minimum separation of the given line and points
        /// </summary>
        public float Separation { get; private set; }

        public Line Line { get; private set; }

        public LineSeparation(Line line, float currentSeparation)
        {
            Line = line;
            Separation = currentSeparation;
        }
    }
}
