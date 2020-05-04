using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Util.Algorithms.Triangulation;
using Util.Geometry;
using Util.Geometry.Polygon;
using Util.Geometry.Triangulation;
using IntPoint = ClipperLib.IntPoint;
using Object = UnityEngine.Object;

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
        private const float MinDistance = .5f;

        private static Vector2 GeneratePointOnCountour(PolyNode input)
        {
            var indexSex = new List<int>();
            for (var i = 0; i < input.Contour.Count; i++)
            {
                IntPoint a = input.Contour[i];
                IntPoint b = input.Contour[(i + 1) % input.Contour.Count];

                var seg = new LineSegment(
                    new Vector2(a.X.toFloatForClipper(), a.Y.toFloatForClipper()),
                    new Vector2(b.X.toFloatForClipper(), b.Y.toFloatForClipper())
                );

                int length = Mathf.CeilToInt(seg.SqrMagnitude);

                for (var j = 0; j < length; j++) indexSex.Add(i);
            }

            int randomIndex = indexSex.DrawRandomItem();
            IntPoint first = input.Contour[randomIndex];
            IntPoint second = input.Contour[(randomIndex + 1) % input.Contour.Count];

            IntPoint left = first.X < second.X ? first : second;
            IntPoint right = first.X < second.X ? second : first;

            var segment = new LineSegment(
                new Vector2(left.X.toFloatForClipper(), left.Y.toFloatForClipper()),
                new Vector2(right.X.toFloatForClipper(), second.Y.toFloatForClipper())
            );

            float randomX = HelperFunctions.GenerateRandomFloat(segment.Point1.x, segment.Point2.x);
            float randomY = segment.IsVertical
                ? HelperFunctions.GenerateRandomFloat(segment.Point1.y, segment.Point2.y)
                : segment.Y(randomX);

            return new Vector2(randomX, randomY);
        }

        private static Vector2? GeneratePointFloat(DotsController dotsController, PolyNode intermediate)
        {
            while (true)
            {
                // PrintFace(dotsController, intermediate);
                MonoBehaviour.print(intermediate.ToString(""));
                if (!intermediate.Childs.Any())
                {
                    if (intermediate.IsHole)
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

                var largeSet = new List<int>();
                for (var index = 0; index < intermediate.Childs.Count; index++)
                {
                    PolyNode child = intermediate.Childs[index];
                    IEnumerable<Vector2> vertices = child.Contour.Select(it =>
                        new Vector2(it.X.toFloatForClipper(), it.Y.toFloatForClipper())
                    );
                    var polygon = new Polygon2D(vertices);
                    Triangulation triangulation = Triangulator.Triangulate(polygon);
                    float area = triangulation.Area;
                    area = area < .01f ? 0f : area;

                    int areaInt = Mathf.CeilToInt(area * 100f);

                    MonoBehaviour.print($"MAG WELLUS: {area}|||||{areaInt}");

                    for (var i = 0; i < areaInt; i++) largeSet.Add(index);
                }

                if (!largeSet.Any()) return null;

                intermediate = intermediate.Childs[largeSet.DrawRandomItem()];
            }
        }

        public static HashSet<Vector2> GeneratePoints(Rect bounds, int amount, DotsController dotsController)
        {
            var clipper = new Clipper();
            Path boundingBox = bounds.toPathForClipper();

            // generate first point and rectangle
            var pointFloats = new HashSet<Vector2>();
            float firstX = HelperFunctions.GenerateRandomFloat(bounds.xMin, bounds.xMax);
            float firstY = HelperFunctions.GenerateRandomFloat(bounds.yMin, bounds.yMax);
            var firstPoint = new Vector2(firstX, firstY);
            pointFloats.Add(firstPoint);

            // initialize first rectangle for clipper
            // var firstRect = new Path
            // {
            //     new IntPoint((firstX - minDistance).toLongForClipper(),
            //         (firstY - minDistance).toLongForClipper()),
            //     new IntPoint((firstX + minDistance).toLongForClipper(),
            //         (firstY - minDistance).toLongForClipper()),
            //     new IntPoint((firstX + minDistance).toLongForClipper(),
            //         (firstY + minDistance).toLongForClipper()),
            //     new IntPoint((firstX - minDistance).toLongForClipper(),
            //         (firstY + minDistance).toLongForClipper())
            // };
            var firstHorizontalRect = new Rect(bounds.x, firstPoint.y - MinDistance / 2f, bounds.width, MinDistance);
            var firstVerticalRect = new Rect(firstPoint.x - MinDistance / 2f, bounds.y, MinDistance, bounds.height);


            var unavailableArea = new Paths();
            AddToUnion(clipper, unavailableArea, firstHorizontalRect.toPathForClipper());
            AddToUnion(clipper, unavailableArea, firstVerticalRect.toPathForClipper());

            // calculate initial unavailable area

            PolyTree availableArea = Difference(clipper, unavailableArea, boundingBox);
            // TODO remove
            // amount = 6;

            Paths newUnavailableArea = null;
            
            for (var i = 1; i < amount; i++)
            {
                // MonoBehaviour.print(availableArea.ToString(""));
                if (!availableArea.Childs.Any()) break; // There is no room anymore
                Vector2? generatedPoint = GeneratePointFloat(dotsController, availableArea);
                if (generatedPoint == null) break;
                Vector2 newPoint = generatedPoint.Value;

                foreach (Vector2 pointFloat in pointFloats)
                {
                    // generate rectangle
                    Path path = generateUnavailableRectangle(bounds, newPoint, pointFloat, dotsController);
                    AddToUnion(clipper, unavailableArea, path);
                }

                // rectangles to prevent same x/y coordinates
                var horizontalRect = new Rect(bounds.x, newPoint.y - MinDistance / 2f, bounds.width, MinDistance);
                AddToUnion(clipper, unavailableArea, horizontalRect.toPathForClipper());

                var verticalRect = new Rect(newPoint.x - MinDistance / 2f, bounds.y, MinDistance, bounds.height);
                AddToUnion(clipper, unavailableArea, verticalRect.toPathForClipper());

                // first generate the clipper rectangle by taking the intersection with the bounding box and the newly unavailable rectangle
                clipper.Clear();
                clipper.AddPaths(unavailableArea, PolyType.ptClip, true);
                clipper.AddPath(boundingBox, PolyType.ptSubject, true);
                newUnavailableArea = new Paths();
                clipper.Execute(ClipType.ctIntersection, newUnavailableArea, PolyFillType.pftEvenOdd,
                    PolyFillType.pftEvenOdd);

                // finally take the difference with the all the unavailable area and the bounding box (i.e. all the available area)
                availableArea = Difference(clipper, newUnavailableArea, boundingBox);

                pointFloats.Add(newPoint);
            }
            
            // TODO printen
            // clipper.Clear();
            // var newUnavailableAreaTree = new PolyTree();
            // clipper.AddPaths(newUnavailableArea, PolyType.ptClip, true);
            // clipper.AddPaths(newUnavailableArea, PolyType.ptSubject, true);
            // clipper.Execute(ClipType.ctUnion, newUnavailableAreaTree, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
            //
            // PrintFace(dotsController, newUnavailableAreaTree);
            

            // clipper.Clear();
            // clipper.AddPaths(availableArea, PolyType.ptClip, true);


            return pointFloats;
        }

        private static PolyTree Difference(Clipper clipper, Paths newUnavailableArea, Path boundingBox)
        {
            PolyTree availableArea;
            clipper.Clear();
            clipper.AddPaths(newUnavailableArea, PolyType.ptClip, true);
            clipper.AddPath(boundingBox, PolyType.ptSubject, true);
            availableArea = new PolyTree();
            clipper.Execute(ClipType.ctDifference, availableArea, PolyFillType.pftEvenOdd,
                PolyFillType.pftEvenOdd);
            return availableArea;
        }

        private static void AddToUnion(Clipper clipper, Paths unavailableArea, Path path)
        {
            // second add the new unavailable area to the old unavailable area by taking the union
            clipper.Clear();
            clipper.AddPaths(unavailableArea, PolyType.ptClip, true);
            clipper.AddPath(path, PolyType.ptSubject, true);
            clipper.Execute(ClipType.ctUnion, unavailableArea, PolyFillType.pftEvenOdd,
                PolyFillType.pftEvenOdd);
        }

        private static Path generateUnavailableRectangle(Rect bounds, Vector2 point1, Vector2 point2,
            DotsController dotsController)
        {
            var lineSegment = new LineSegment(point1, point2);

            Vector2 a = point1 + MinDistance / 2f * lineSegment.RightNormal().normalized;
            Vector2 b = point2 + MinDistance / 2f * lineSegment.RightNormal().normalized;

            var firstLineSegment = new LineSegment(
                a.x < b.x ? a : b,
                a.x < b.x ? b : a
            );

            Vector2 c = point1 + MinDistance / 2f * -lineSegment.RightNormal().normalized;
            Vector2 d = point2 + MinDistance / 2f * -lineSegment.RightNormal().normalized;

            var secondLineSegment = new LineSegment(
                c.x < d.x ? c : d,
                c.x < d.x ? d : c
            );

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

            Vector2 direction = upper.Orientation().normalized;
            Vector2 otherDirection = -upper.Orientation().normalized;

            float diagonalLength = bounds.DiagonalLength();
            Vector2 upperLeft = upper.Point1 + diagonalLength * otherDirection;
            Vector2 upperRight = upper.Point2 + diagonalLength * direction;

            Vector2 lowerRight = lower.Point2 + diagonalLength * direction;
            Vector2 lowerLeft = lower.Point1 + diagonalLength * otherDirection;

            return new Path
            {
                new IntPoint(upperLeft.x.toLongForClipper(), upperLeft.y.toLongForClipper()),
                new IntPoint(upperRight.x.toLongForClipper(), upperRight.y.toLongForClipper()),
                new IntPoint(lowerRight.x.toLongForClipper(), lowerRight.y.toLongForClipper()),
                new IntPoint(lowerLeft.x.toLongForClipper(), lowerLeft.y.toLongForClipper())
            };
        }

        private static void PrintFace(DotsController dotsController, PolyNode polyNode)
        {
            if (polyNode.Contour.Any()) PrintFace(dotsController, polyNode.Contour, polyNode.IsHole);
            foreach (PolyNode child in polyNode.Childs)
            {
                PrintFace(dotsController, child);
            }
        }

        private static void PrintFace(DotsController dotsController, Path path, bool isHole)
        {
            for (var j = 0; j < path.Count; j++)
            {
                IntPoint point1 = path[j];
                IntPoint point2 = path[(j + 1) % path.Count];
                var seg = new LineSegment(
                    new Vector2(point1.X.toFloatForClipper(), point1.Y.toFloatForClipper()),
                    new Vector2(point2.X.toFloatForClipper(), point2.Y.toFloatForClipper())
                );
                UnityTrapDecomLine.CreateUnityTrapDecomLine(seg, dotsController);
            }

            GameObject faceObject = Object.Instantiate(
                dotsController.facePrefab,
                new Vector3(0, 0, 0),
                Quaternion.identity);
            faceObject.transform.parent = dotsController.transform;
            dotsController.InstantObjects.Add(faceObject);
            var face = faceObject.gameObject.GetComponent<UnityDotsFace>();

            List<DotsVertex> vertices = path
                .Select(it =>
                    new DotsVertex(
                        new Vector2(
                            it.X.toFloatForClipper(),
                            it.Y.toFloatForClipper())
                    )
                ).ToList();

            List<DotsHalfEdge> halfEdges = vertices.Select(vertex =>
                new DotsHalfEdge
                {
                    GameController = dotsController,
                    Player = isHole ? 1 : 2,
                    Origin = vertex
                }).ToList();
            for (var i = 0; i < halfEdges.Count; i++)
            {
                halfEdges[i].Next = halfEdges[(i + 1) % halfEdges.Count];
            }

            face.Player = isHole ? 1 : 2;
            
            face.Constructor(halfEdges.First());
        }
    }
}