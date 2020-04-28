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

        private Dictionary<Tuple<Vector2, Vector2>, Polygon2D> _nonGeneralPositionAreas;


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

        /** attempt */
        public static PolyTree GetGeneralPositionArea(List<List<Vector2>> polygons, Rect bounds)
        {
            Paths paths = polygons.Select(it =>
                it.Select(coords =>
                    new IntPoint(coords.x.toLongForClipper(), coords.y.toLongForClipper())
                ).ToList()
            ).ToList();

            var clipper = new Clipper();
            var union = new Paths();
            foreach (Path path in paths)
            {
                clipper.AddPaths(union, PolyType.ptSubject, true);
                clipper.AddPath(path, PolyType.ptClip, true);

                var result = new Paths();
                clipper.Execute(ClipType.ctUnion, result, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

                union.AddRange(result);
                clipper.Clear();
            }

            clipper.Clear();

            Path boundingBox = new List<Vector2>
            {
                bounds.min,
                bounds.min + new Vector2(bounds.width, 0),
                bounds.max,
                bounds.min + new Vector2(0, bounds.height)
            }.Select(coords =>
                new IntPoint(coords.x.toLongForClipper(), coords.y.toLongForClipper())
            ).ToList();

            clipper.AddPaths(union, PolyType.ptClip, true);
            clipper.AddPath(boundingBox, PolyType.ptSubject, true);

            var secondSolution = new PolyTree();
            clipper.Execute(ClipType.ctDifference, secondSolution, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

            return secondSolution;
            //     secondSolution.Select(it =>
            //     it.Select(coords =>
            //         new Vector2(coords.X.toFloatForClipper(), coords.Y.toFloatForClipper())
            //     ).ToList()
            // ).ToList();
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