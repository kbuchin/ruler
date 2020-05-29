using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util.Algorithms.Triangulation;
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
        private const float MinDistance = .25f;

        private readonly Clipper _clipper = new Clipper();
        private Rect _bounds;

        // private readonly Path _boundingBox;

        public HashSet<Vector2> Dots { get; } = new HashSet<Vector2>();

        // private readonly Paths _unavailableArea = new Paths();
        private Paths _availableArea;

        public DotsPlacer(Rect bounds)
        {
            _bounds = bounds;

            // use bounding box to create the initial available area
            Path boundingBox = bounds.ToPathForClipper();

            _availableArea = new Paths
            {
                boundingBox
            };

            // generate first point
            float firstX = HelperFunctions.GenerateRandomFloat(bounds.xMin, bounds.xMax);
            float firstY = HelperFunctions.GenerateRandomFloat(bounds.yMin, bounds.yMax);
            var firstPoint = new Vector2(firstX, firstY);
            Dots.Add(firstPoint);

            // calculate initial unavailable area
            CreateUnavailableRectangles(firstPoint)
                .Select(it => it.ToPathForClipper())
                .ForEach(RemoveFromAvailableArea);

            RemoveFromAvailableArea(
                CreateUnavailableSquare(firstPoint)
                    .ToPathForClipper()
            );
        }

        public void AddNewPoint()
        {
            var availableArea = _availableArea.ToPolyTree(_clipper);
            if (!availableArea.Childs.Any()) return; // There is no room anymore
            Vector2? generatedPoint = GeneratePointFloat(availableArea);
            if (generatedPoint == null) return;
            Vector2 newPoint = generatedPoint.Value;
            foreach (Vector2 pointFloat in Dots)
            {
                // generate rectangle
                GenerateUnavailableCups(_bounds, newPoint, pointFloat)
                    .ForEach(RemoveFromAvailableArea);

                // rectangles to prevent same x/y coordinates
                CreateUnavailableRectangles(newPoint)
                    .Select(it => it.ToPathForClipper())
                    .ForEach(RemoveFromAvailableArea);

                RemoveFromAvailableArea(
                    CreateUnavailableSquare(newPoint)
                        .ToPathForClipper()
                );
            }

            Dots.Add(newPoint);
        }

        public void AddNewPoints(int amount)
        {
            for (var i = 0; i < amount - 1; i++) AddNewPoint();
        }

        public void PrintAvailableArea(DotsController dotsController, HashSet<GameObject> faces)
        {
            PrintFace(dotsController, _availableArea.ToPolyTree(_clipper), faces);
        }


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

        // TODO can be non-recursive-like 
        private Vector2? GeneratePointFloat(PolyNode intermediate)
        {
            while (true)
            {
                // PrintFace(dotsController, intermediate);
                //MonoBehaviour.print(intermediate.ToString(""));
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

                    for (var i = 0; i < areaInt; i++) largeSet.Add(index);
                }

                if (!largeSet.Any()) return null;

                intermediate = intermediate.Childs[largeSet.DrawRandomItem()];
            }
        }

        private List<Rect> CreateUnavailableRectangles(Vector2 point)
        {
            var firstHorizontalRect = new Rect(_bounds.x, point.y - MinDistance / 8f, _bounds.width, MinDistance / 4f);
            var firstVerticalRect = new Rect(point.x - MinDistance / 8f, _bounds.y, MinDistance / 4f, _bounds.height);
            return new List<Rect> {firstHorizontalRect, firstVerticalRect};
        }

        private Rect CreateUnavailableSquare(Vector2 point)
        {
            return new Rect(point.x - MinDistance * 2f, point.y - MinDistance * 2f, MinDistance * 4f,
                MinDistance * 4f);
        }

        private void RemoveFromAvailableArea(Path unavailableArea)
        {
            _clipper.Clear();
            _clipper.AddPaths(_availableArea, PolyType.ptSubject, true);
            _clipper.AddPath(unavailableArea, PolyType.ptClip, true);
            _clipper.Execute(ClipType.ctDifference, _availableArea, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
        }

        private static (Vector2, Vector2) SortLeftRight(Vector2 vec1, Vector2 vec2) =>
            (vec1.x < vec2.x ? vec1 : vec2, vec1.x < vec2.x ? vec2 : vec1);

        private static (Vector2, Vector2) SortUpDown(Vector2 vec1, Vector2 vec2) =>
            (vec1.y > vec2.y ? vec1 : vec2, vec1.y > vec2.y ? vec2 : vec1);


        /**
         * This generates cups of unavailable area according to the picture shown here: [https://imgur.com/a/wvcx3fO]
         */
        private Paths GenerateUnavailableCups(Rect bounds, Vector2 point1, Vector2 point2)
        {
            // Make sure point1 is left of point2
            (point1, point2) = SortLeftRight(point1, point2);

            // Minimum length needed to make sure the cup always extends to outside the bounds
            float diagonalLength = bounds.DiagonalLength();

            // line between two points
            var lineSegment = new LineSegment(point1, point2);

            // Creating point a1, b2
            (Vector2 a1, Vector2 b1) = SortUpDown(
                point2 + MinDistance * .75f * lineSegment.RightNormal().normalized,
                point2 + MinDistance * .75f * -lineSegment.RightNormal().normalized
            );

            // Draw line segments from point1 through a1 and b1 respectively
            Vector2 directionFirstLineSegment1 = new LineSegment(point1, a1).Orientation().normalized;
            var firstLineSegment1 = new LineSegment(
                point1,
                a1 + diagonalLength * directionFirstLineSegment1
            );

            Vector2 directionSecondLineSegment1 = new LineSegment(point1, b1).Orientation().normalized;
            var secondLineSegment1 = new LineSegment(
                point1,
                b1 + diagonalLength * directionSecondLineSegment1
            );


            // Add points in clockwise manner to a new path
            var path1 = new Path
            {
                new IntPoint(
                    point1.x.toLongForClipper(),
                    point1.y.toLongForClipper()
                ),
                new IntPoint(
                    firstLineSegment1.Point2.x.toLongForClipper(),
                    firstLineSegment1.Point2.y.toLongForClipper()
                ),
                new IntPoint(
                    secondLineSegment1.Point2.x.toLongForClipper(),
                    secondLineSegment1.Point2.y.toLongForClipper()
                )
            };

            // Creating point a2, b2
            (Vector2 a2, Vector2 b2) = SortUpDown(
                point1 + MinDistance * .75f * lineSegment.RightNormal().normalized,
                point1 + MinDistance * .75f * -lineSegment.RightNormal().normalized
            );

            // Draw line segments from point2 through a2 and b2 respectively
            Vector2 directionFirstLineSegment2 = new LineSegment(point2, a2).Orientation().normalized;
            var firstLineSegment2 = new LineSegment(
                point2,
                a2 + diagonalLength * directionFirstLineSegment2
            );

            Vector2 directionSecondLineSegment2 = new LineSegment(point2, b2).Orientation().normalized;
            var secondLineSegment2 = new LineSegment(
                point2,
                b2 + diagonalLength * directionSecondLineSegment2
            );

            // Add points in clockwise manner to a new path
            var path2 = new Path
            {
                new IntPoint(
                    point2.x.toLongForClipper(),
                    point2.y.toLongForClipper()
                ),
                new IntPoint(
                    firstLineSegment2.Point2.x.toLongForClipper(),
                    firstLineSegment2.Point2.y.toLongForClipper()
                ),
                new IntPoint(
                    secondLineSegment2.Point2.x.toLongForClipper(),
                    secondLineSegment2.Point2.y.toLongForClipper()
                )
            };

            return new Paths
            {
                path1, path2
            };
        }

        private void PrintFace(DotsController dotsController, PolyNode polyNode, HashSet<GameObject> faces)
        {
            if (polyNode.Contour.Any()) PrintFace(dotsController, polyNode.Contour, polyNode.IsHole, faces);
            foreach (PolyNode child in polyNode.Childs)
            {
                PrintFace(dotsController, child, faces);
            }
        }

        private void PrintFace(DotsController dotsController, Path path, bool isHole, HashSet<GameObject> faces)
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

            faces.Add(faceObject);
        }
    }
}