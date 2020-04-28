using System;
using System.Collections.Generic;
using UnityEngine;
using Util.Geometry.Polygon;

namespace DotsAndPolygons
{
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
         * |  *                                          *   |
         * |_________________________________________________|
         *
         * I think if we take the union (somehow) of all these rectangles we get all the non-allowed area.
         * If we then take the complement of that (with the _bounds as the outer, well, bounds), we get the allowed area.
         * Not sure if we can use Polygon2D for that, that doesn't support much.
         * http://csharphelper.com/blog/2016/01/find-a-polygon-union-in-c/ this finds polygon union but doesn't support holes
         * http://www.cs.man.ac.uk/~toby/alan/software/gpc.html is great! but in c
         * http://www.angusj.com/delphi/clipper.php this might work
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

        public void AddNewPoints(int amount)
        {
            for (var i = 0; i < amount; i++) AddNewPoint();
        }

        public void AddNewPoint()
        {
            
        }
    }
}