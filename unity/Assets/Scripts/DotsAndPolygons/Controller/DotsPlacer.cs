using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Util.Geometry.Polygon;
using IntPoint = ClipperLib.IntPoint;

namespace DotsAndPolygons
{
    using ClipperLib;
    using Util.Geometry;
    using Path = List<IntPoint>;
    using Paths = List<List<IntPoint>>;

    public class DotsPlacer
    {
        /**
         * General position was achieved when:
         * `ShortestPointDistance(coords, dots) >= 1f`
         * and `!Colinear(a, b, c, .14f)` for all points a,b,c in the dots
         * The last one means that `Line(a, b).DistanceToPoint(c)` greater than .14f for all a,b,c.
         * This means we can create rectangles with width 2 * .14f between all dots and subtract that from the available space.
         * The length of those rectangles probably needs to be abs(a - b) + 2 * .14f so we get:
         * 
         * ___________________________________________________
         * |                                                 |
         * |-- .14 -- *                         * -- .14 --  |
         * |_________________________________________________|
         *
         * I think if we take the union (somehow) of all these rectangles we get all the non-allowed area.
         * If we then take the complement of that (with the _bounds as the outer, well, bounds), we get the allowed area.
         * Not sure if we can use Polygon2D for that, that doesn't support much.
         * http://csharphelper.com/blog/2016/01/find-a-polygon-union-in-c/ this finds polygon union but doesn't support holes
         * http://www.cs.man.ac.uk/~toby/alan/software/gpc.html is great! but in c
         * http://www.angusj.com/delphi/clipper.php this might work, docs: http://www.angusj.com/delphi/clipper/documentation/Docs/Overview/_Body.htm
         */
        private Rect _bounds;

        private HashSet<Polygon2D> _availableArea;

        public HashSet<Vector2> PlacedDots;

        private HashSet<Polygon2D> _nonGeneralPositionAreas;

        private static float minDistance = 0.5f;


        public DotsPlacer(Rect bounds)
        {
            _bounds = bounds;
            _availableArea = new HashSet<Polygon2D>
            {
                new Polygon2D(new[]
                {
                    bounds.min,
                    bounds.min + new Vector2(bounds.width, 0),
                    bounds.max,
                    bounds.min + new Vector2(0, bounds.height)
                })
            };
            PlacedDots = new HashSet<Vector2>();
        }

        

        private static Vector2 GeneratePointOnCountour(PolyNode input)
        {
            int randomIndex = HelperFunctions.GenerateRandomInt(0, input.Contour.Count);
            IntPoint first = input.Contour[randomIndex];
            IntPoint second = input.Contour[(randomIndex+1) % input.Contour.Count];
            long randomX = HelperFunctions.GenerateRandomLong(first.X, second.X);
            long randomY = HelperFunctions.GenerateRandomLong(first.Y, second.Y);
            return new Vector2(randomX.toFloatForClipper(), randomY.toFloatForClipper());
        }

        private static Vector2 GeneratePoint(PolyNode intermediate)
        {
            MonoBehaviour.print(intermediate.ToString(""));
            if(!intermediate.Childs.Any())
            {                
                if(intermediate.IsHole)
                {
                    
                    Vector2 returner = GeneratePointOnCountour(intermediate.Parent);
                    return returner;
                }
                else
                {
                    Vector2 returner = GeneratePointOnCountour(intermediate);
                    return returner;
                }

            }

            return GeneratePoint(intermediate.Childs.DrawRandomItem());
        }

        /** attempt */
        public static HashSet<Vector2> GeneratePoints(Rect bounds, int amount)
        {
            Path boundingBox = new List<Vector2>
            {
                bounds.min,
                bounds.min + new Vector2(bounds.width, 0),
                bounds.max,
                bounds.min + new Vector2(0, bounds.height)
            }.Select(coords =>
                new IntPoint(coords.x.toLongForClipper(), coords.y.toLongForClipper())
            ).ToList();

            // generate first point and rectangle
            HashSet<Vector2> points = new HashSet<Vector2>();
            float firstX = HelperFunctions.GenerateRandomFloat(bounds.xMin, bounds.xMax);
            float firstY = HelperFunctions.GenerateRandomFloat(bounds.yMin, bounds.yMax);
            Vector2 firstPoint = new Vector2(firstX, firstY);
            points.Add(firstPoint);

            // initialize first rectangle for clipper
            Path firstRect = new Path();
            firstRect.Add(new IntPoint(firstX - (minDistance / 2), firstY + (minDistance / 2)));
            firstRect.Add(new IntPoint(firstX + (minDistance / 2), firstY + (minDistance / 2)));
            firstRect.Add(new IntPoint(firstX + (minDistance / 2), firstY - (minDistance / 2)));
            firstRect.Add(new IntPoint(firstX - (minDistance / 2), firstY - (minDistance / 2)));

            // add the first rectangle as unavailable area
            Paths nonAvailablearea = new Paths();
            nonAvailablearea.Add(firstRect);

            // calculate initial unavailable area
            Clipper clipper = new Clipper();
            clipper.AddPaths(nonAvailablearea, PolyType.ptClip, true);
            clipper.AddPath(boundingBox, PolyType.ptSubject, true);
            PolyTree availableArea = new PolyTree();
            MonoBehaviour.print(((PolyNode)availableArea).ToString(""));
            clipper.Execute(ClipType.ctDifference, availableArea, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
            
            for (int i = 1; i < amount; i++)
            {
                PolyNode random = availableArea.Childs.DrawRandomItem();
                Vector2 newPoint = GeneratePoint(random);
                foreach(Vector2 vector in points)
                {
                    // generate rectangle
                    Path path = generateUnavailableRectangle(bounds, newPoint, vector);

                    // first generate the clipper rectangle by taking the intersection with the bounding box and the newly unavailable rectangle
                    clipper.Clear();
                    clipper.AddPath(path, PolyType.ptClip, true);
                    clipper.AddPath(boundingBox, PolyType.ptSubject, true);
                    var newUnavailableArea = new Paths();
                    clipper.Execute(ClipType.ctIntersection, newUnavailableArea, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);


                    // second add the new unavailable area to the old unavailable area by taking the union
                    clipper.Clear();
                    clipper.AddPaths(nonAvailablearea, PolyType.ptClip, true);
                    clipper.AddPaths(newUnavailableArea, PolyType.ptSubject, true);
                    clipper.Execute(ClipType.ctUnion, nonAvailablearea, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

                    // finally take the difference with the all the unavilable area and the bounding box (i.e. all the available area)
                    clipper.Clear();
                    clipper.AddPaths(nonAvailablearea, PolyType.ptClip, true);
                    clipper.AddPath(boundingBox, PolyType.ptSubject, true);
                    availableArea = new PolyTree();
                    clipper.Execute(ClipType.ctDifference, availableArea, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
                }
                points.Add(newPoint);

            }

            return points;
        }

        private static Path generateUnavailableRectangle(Rect bounds, Vector2 newPoint, Vector2 vector)
        {
            LineSegment lineSegment = new LineSegment(vector, newPoint);

            LineSegment firstLineSegment = new LineSegment(vector * lineSegment.RightNormal().normalized * minDistance, newPoint * lineSegment.RightNormal().normalized * minDistance);
            LineSegment secondLineSegment = new LineSegment(vector * -lineSegment.RightNormal().normalized * minDistance, newPoint * -lineSegment.RightNormal().normalized * minDistance);
            LineSegment upper;
            LineSegment lower;
            if (firstLineSegment.IsAbove(secondLineSegment))
            {
                upper = firstLineSegment;
                lower = secondLineSegment;
            }
            else
            {
                upper = secondLineSegment;
                lower = firstLineSegment;
            }

            Vector2 direction = (upper.Point1 + upper.Point2).normalized;
            Vector2 otherDirection = (upper.Point2 + upper.Point1).normalized;

            float diagnalLength = bounds.DiagonalLength();
            Vector2 upperLeft = upper.Point1 * diagnalLength * otherDirection;
            Vector2 upperRight = upper.Point2 * diagnalLength * direction;
            Vector2 lowerRight = lower.Point2 * diagnalLength * direction;
            Vector2 lowerLeft = lower.Point1 * diagnalLength * otherDirection;

            Path path = new Path {
                        new IntPoint(upperLeft.x.toLongForClipper(), upperRight.y.toLongForClipper()),
                        new IntPoint(upperRight.x.toLongForClipper(), upperRight.y.toLongForClipper()),
                        new IntPoint(lowerRight.x.toLongForClipper(), lowerRight.y.toLongForClipper()),
                        new IntPoint(lowerLeft.x.toLongForClipper(), lowerLeft.y.toLongForClipper())
                    };
            return path;
        }

        public void AddNewPoints(int amount)
        {
            for (var i = 0; i < amount; i++) AddNewPoint();
        }

        public void AddNewPoint()
        {
        }
    }
}