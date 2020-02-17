namespace DotsAndPolygons
{
    using System.Collections.Generic;
    using System.Linq;
    using Util.Geometry;

    public static class ConvexHullHelper
    {
        public static HashSet<LineSegment> ComputeHull(IEnumerable<IDotsVertex> dotsVertices)
        {
            IDotsVertex[] sortedAsc = dotsVertices.OrderBy(vertex => vertex.Coordinates.x).ToArray();
            IDotsVertex[] sortedDesc = dotsVertices.OrderByDescending(vertex => vertex.Coordinates.x).ToArray();
            var lUppper = new List<IDotsVertex>();
            var lLower = new List<IDotsVertex>();
            lUppper.Add(sortedAsc[0]);
            lUppper.Add(sortedAsc[1]);
            lLower.Add(sortedDesc[0]);
            lLower.Add(sortedDesc[1]);
            for (int i = 2; i < sortedDesc.Length; i++)
            {
                lUppper.Add(sortedAsc[i]);

                // upper hull
                while (lUppper.Count > 2 &&
                    !makesRightTurn(lUppper[lUppper.Count - 3], lUppper[lUppper.Count - 2], lUppper.Last()))
                {
                    lUppper.Remove(lUppper[lUppper.Count - 2]);
                }

                lLower.Add(sortedDesc[i]);

                //lower hull
                while (lLower.Count > 2 &&
                    !makesRightTurn(lLower[lLower.Count - 3], lLower[lLower.Count - 2], lLower.Last()))
                {
                    lLower.Remove(lLower[lLower.Count - 2]);
                }
            }
            lLower.Remove(lLower.First());
            lLower.Remove(lLower.Last());
            lUppper.AddRange(lLower);

            return CreateHullSet(lUppper);
        }

        private static HashSet<LineSegment> CreateHullSet(List<IDotsVertex> hull)
        {
            foreach (IDotsVertex dotsVertex in hull)
            {
                dotsVertex.OnHull = true;
            }
            IDotsVertex[] convexHull = hull.ToArray();
            var returner = new HashSet<LineSegment>();
            for (var i = 0; i < convexHull.Length; i++)
            {
                LineSegment edge = i + 1 < convexHull.Length ? new LineSegment(convexHull[i].Coordinates, convexHull[i + 1].Coordinates) : 
                    new LineSegment(convexHull[i].Coordinates, convexHull[0].Coordinates);

                returner.Add(edge);
            }
            return returner;
        }

        private static float calculateCrossProduct(IDotsVertex vertex1, IDotsVertex vertex2, IDotsVertex vertex3)
        {
            float bX = vertex2.Coordinates.x;
            float bY = vertex2.Coordinates.y;
            float pX = vertex3.Coordinates.x;
            float pY = vertex3.Coordinates.y;
            float aX = vertex1.Coordinates.x;
            float aY = vertex1.Coordinates.y;
            bX -= aX;
            bY -= aY;
            pX -= aX;
            pY -= aY;

            float returner = bX * pY - bY * pX;
            return returner;
        }

        public static bool makesRightTurn(IDotsVertex vertex1, IDotsVertex vertex2, IDotsVertex vertex3)
        {
            return calculateCrossProduct(vertex1, vertex2, vertex3) < 0;
        }
    }
}
