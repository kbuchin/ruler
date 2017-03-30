using UnityEngine;

namespace Voronoi
{
    public static class Utility
    {
        public static Vertex Midpoint(Vertex v1, Vertex v2)
        {
            float mx = (v1.X + v2.X) / 2;
            float my = (v1.Y + v2.Y) / 2;
            return new Vertex(mx, my);
        }

        public static float Slope(Vertex v1, Vertex v2)
        {
            float value = (v2.Y - v1.Y) / (v2.X - v1.X);
            return value;
        }

        public static float SquaredDistance(Vertex v1, Vertex v2)
        {
            return Mathf.Pow(v1.X - v2.X, 2) + Mathf.Pow(v1.Y - v2.Y, 2);
        }

        public static float Distance(Vertex v1, Vertex v2)
        {
            return Mathf.Sqrt(SquaredDistance(v1, v2));
        }
    }
}
