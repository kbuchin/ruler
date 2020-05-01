using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Util.Geometry;
using Util.Geometry.Polygon;
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
        private static float minDistance = .6f;

        private static Vector2 GeneratePointOnCountour(PolyNode input)
        {
            int randomIndex = HelperFunctions.GenerateRandomInt(0, input.Contour.Count);
            IntPoint first = input.Contour[randomIndex];
            IntPoint second = input.Contour[(randomIndex + 1) % input.Contour.Count];
            long randomX = HelperFunctions.GenerateRandomLong(first.X, second.X);
            long randomY = HelperFunctions.GenerateRandomLong(first.Y, second.Y);
            return new Vector2(randomX.toFloatForClipper(), randomY.toFloatForClipper());
        }

        private static Vector2 GeneratePointFloat(PolyNode intermediate)
        {
            while (true)
            {
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

                intermediate = intermediate.Childs.DrawRandomItem();
            }
        }

        /** attempt */
        public static HashSet<Vector2> GeneratePoints(Rect bounds, int amount, DotsController dotsController)
        {
            Path boundingBox = new List<Vector2>
            {
                new Vector2(bounds.xMin, bounds.yMin),
                new Vector2(bounds.xMax, bounds.yMin),
                new Vector2(bounds.xMax, bounds.yMax),
                new Vector2(bounds.xMin, bounds.yMax)
            }.Select(coords =>
                new IntPoint(coords.x.toLongForClipper(), coords.y.toLongForClipper())
            ).ToList();

            // generate first point and rectangle
            var pointFloats = new HashSet<Vector2>();
            float firstX = HelperFunctions.GenerateRandomFloat(bounds.xMin, bounds.xMax);
            float firstY = HelperFunctions.GenerateRandomFloat(bounds.yMin, bounds.yMax);
            var firstPoint = new Vector2(firstX, firstY);
            pointFloats.Add(firstPoint);

            // initialize first rectangle for clipper
            var firstRect = new Path
            {
                new IntPoint((firstX - minDistance).toLongForClipper(),
                    (firstY - minDistance).toLongForClipper()),
                new IntPoint((firstX + minDistance).toLongForClipper(),
                    (firstY - minDistance).toLongForClipper()),
                new IntPoint((firstX + minDistance).toLongForClipper(),
                    (firstY + minDistance).toLongForClipper()),
                new IntPoint((firstX - minDistance).toLongForClipper(),
                    (firstY + minDistance).toLongForClipper())
            };

            // add the first rectangle as unavailable area
            var unavailableArea = new Paths {firstRect};

            // nonAvailableArea.ForEach(it => PrintFace(dotsController, it, false));
            //
            // foreach (Path newPath in nonAvailableArea)
            // {
            //     for (var j = 0; j < newPath.Count; j++)
            //     {
            //         IntPoint point1 = newPath[j];
            //         IntPoint point2 = newPath[(j + 1) % newPath.Count];
            //         var seg = new LineSegment(
            //             new Vector2(point1.X.toFloatForClipper(), point1.Y.toFloatForClipper()),
            //             new Vector2(point2.X.toFloatForClipper(), point2.Y.toFloatForClipper())
            //         );
            //         UnityTrapDecomLine.CreateUnityTrapDecomLine(seg, dotsController);
            //     }
            // }

            // calculate initial unavailable area
            var clipper = new Clipper();
            clipper.AddPaths(unavailableArea, PolyType.ptClip, true);
            clipper.AddPath(boundingBox, PolyType.ptSubject, true);
            var availableArea = new PolyTree();
            clipper.Execute(ClipType.ctDifference, availableArea, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

            // TODO REMOVE
            amount = 6;
            for (var i = 1; i < amount; i++)
            {
                MonoBehaviour.print(availableArea.ToString(""));
                if (!availableArea.Childs.Any()) break; // There is no room anymore
                PolyNode random = availableArea.Childs.DrawRandomItem();
                Vector2 newPoint = GeneratePointFloat(random);
                foreach (Vector2 pointFloat in pointFloats)
                {
                    // generate rectangle
                    Path path = generateUnavailableRectangle(bounds, newPoint, pointFloat, dotsController);

                    // for (var j = 0; j < path.Count; j++)
                    // {
                    //     IntPoint point1 = path[j];
                    //     IntPoint point2 = path[(j + 1) % path.Count];
                    //     var seg = new LineSegment(
                    //         new Vector2(point1.X.toFloatForClipper(), point1.Y.toFloatForClipper()),
                    //         new Vector2(point2.X.toFloatForClipper(), point2.Y.toFloatForClipper())
                    //     );
                    //     UnityTrapDecomLine.CreateUnityTrapDecomLine(seg, dotsController);
                    // }

                    // first generate the clipper rectangle by taking the intersection with the bounding box and the newly unavailable rectangle
                    clipper.Clear();
                    clipper.AddPath(path, PolyType.ptClip, true);
                    clipper.AddPath(boundingBox, PolyType.ptSubject, true);
                    var newUnavailableArea = new Paths();
                    clipper.Execute(ClipType.ctIntersection, newUnavailableArea, PolyFillType.pftEvenOdd,
                        PolyFillType.pftEvenOdd);


                    // newUnavailableArea.ForEach(it => PrintFace(dotsController, it, false));
                    // foreach (Path newPath in newUnavailableArea)
                    // {
                    //     for (var j = 0; j < newPath.Count; j++)
                    //     {
                    //         IntPoint point1 = newPath[j];
                    //         IntPoint point2 = newPath[(j + 1) % newPath.Count];
                    //         var seg = new LineSegment(
                    //             new Vector2(point1.X.toFloatForClipper(), point1.Y.toFloatForClipper()),
                    //             new Vector2(point2.X.toFloatForClipper(), point2.Y.toFloatForClipper())
                    //         );
                    //         UnityTrapDecomLine.CreateUnityTrapDecomLine(seg, dotsController);
                    //     }
                    // }

                    // second add the new unavailable area to the old unavailable area by taking the union
                    clipper.Clear();
                    clipper.AddPaths(unavailableArea, PolyType.ptClip, true);
                    clipper.AddPaths(newUnavailableArea, PolyType.ptSubject, true);
                    clipper.Execute(ClipType.ctUnion, unavailableArea, PolyFillType.pftEvenOdd,
                        PolyFillType.pftEvenOdd);

                    // finally take the difference with the all the unavailable area and the bounding box (i.e. all the available area)
                    clipper.Clear();
                    clipper.AddPaths(unavailableArea, PolyType.ptClip, true);
                    clipper.AddPath(boundingBox, PolyType.ptSubject, true);
                    availableArea = new PolyTree();
                    clipper.Execute(ClipType.ctDifference, availableArea, PolyFillType.pftEvenOdd,
                        PolyFillType.pftEvenOdd);
                }

                pointFloats.Add(newPoint);
            }

            // TODO remove
            MonoBehaviour.print(availableArea.ToString(""));

            PrintFace(dotsController, availableArea);

            // foreach (Path newPath in availableArea)
            // {
            //     for (var j = 0; j < newPath.Count; j++)
            //     {
            //         IntPoint point1 = newPath[j];
            //         IntPoint point2 = newPath[(j + 1) % newPath.Count];
            //         var seg = new LineSegment(
            //             new Vector2(point1.X.toFloatForClipper(), point1.Y.toFloatForClipper()),
            //             new Vector2(point2.X.toFloatForClipper(), point2.Y.toFloatForClipper())
            //         );
            //         UnityTrapDecomLine.CreateUnityTrapDecomLine(seg, dotsController);
            //     }
            // }

            return pointFloats;
        }

        private static Path generateUnavailableRectangle(Rect bounds, Vector2 point1, Vector2 point2,
            DotsController dotsController)
        {
            var lineSegment = new LineSegment(point1, point2);

            // TODO remove
            // UnityTrapDecomLine.CreateUnityTrapDecomLine(lineSegment, dotsController);


            Vector2 a = point1 + minDistance * lineSegment.RightNormal().normalized;
            Vector2 b = point2 + minDistance * lineSegment.RightNormal().normalized;

            var firstLineSegment = new LineSegment(
                a.x < b.x ? a : b,
                a.x < b.x ? b : a
            );

            // TODO remove
            // UnityTrapDecomLine.CreateUnityTrapDecomLine(firstLineSegment, dotsController);

            Vector2 c = point1 + minDistance * -lineSegment.RightNormal().normalized;
            Vector2 d = point2 + minDistance * -lineSegment.RightNormal().normalized;

            var secondLineSegment = new LineSegment(
                c.x < d.x ? c : d,
                c.x < d.x ? d : c
            );

            // TODO remove
            // UnityTrapDecomLine.CreateUnityTrapDecomLine(secondLineSegment, dotsController);

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

            // MonoBehaviour.print(
            //     $"upper left: {upperLeft}, upper right: {upperRight}, lower right: {lowerRight}, lower left: {lowerLeft}");

            // TODO remove
            // UnityTrapDecomLine.CreateUnityTrapDecomLine(new LineSegment(upperLeft, upperRight), dotsController);
            // UnityTrapDecomLine.CreateUnityTrapDecomLine(new LineSegment(lowerLeft, lowerRight), dotsController);

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

            face.Constructor(halfEdges.First());
        }
    }
}