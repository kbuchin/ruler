namespace Util.Geometry.Polygon
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Polygon interface that defines vertices, segments, adding/removing vertices
    /// and some auxiliary methods.
    /// </summary>
    public interface IPolygon2D : IEquatable<IPolygon2D>
    {
        /// <summary>
        /// Collection of vertices that define polygon
        /// </summary>
        ICollection<Vector2> Vertices { get; }

        /// <summary>
        /// Collection of line segments between vertices.
        /// </summary>
        ICollection<LineSegment> Segments { get; }

        int VertexCount { get; }

        /// <summary>
        /// Area of the given polygon
        /// </summary>
        float Area { get; }

        /// <summary>
        /// Find the next vertex after the given point.
        /// Gives first point if multiple apply.
        /// </summary>
        /// <remarks>
        /// Typically slow implementation in O(n), so not recommended.
        /// </remarks>
        /// <param name="pos"></param>
        /// <returns></returns>
        Vector2? Next(Vector2 pos);

        /// <summary>
        /// Finds the previous vertex after given point.
        /// Gives first such point if multiple apply.
        /// </summary>
        /// <remarks>
        /// Typically slow implementation in O(n), so not recommended.
        /// </remarks>
        /// <param name="pos"></param>
        /// <returns></returns>
        Vector2? Prev(Vector2 pos);

        /// <summary>
        /// Add a new vertex at the end of the polygon vertex list.
        /// </summary>
        /// <param name="pos"></param>
        void AddVertex(Vector2 pos);

        /// <summary>
        /// Add the vertex to the start of the polygon's vertex list
        /// </summary>
        /// <param name="pos"></param>
        void AddVertexFirst(Vector2 pos);

        /// <summary>
        /// Add a vertex after the specified point.
        /// Will find the first matching point and add vertex after.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="after"></param>
        void AddVertexAfter(Vector2 pos, Vector2 after);

        /// <summary>
        /// Removes given point from the polygon.
        /// Will remove first such points if exists.
        /// </summary>
        /// <param name="pos"></param>
        void RemoveVertex(Vector2 pos);

        /// <summary>
        /// Remove first point from the polygon.
        /// </summary>
        void RemoveFirst();

        /// <summary>
        /// Remove last point from the polygon.
        /// </summary>
        void RemoveLast();

        /// <summary>
        /// Clears all points of the polygon.
        /// </summary>
        void Clear();

        /// <summary>
        /// Checks whether the point is contained strictly inside the polygon.
        /// Use OnBoundary if also want to check for points on boundary of polygon.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        bool ContainsInside(Vector2 pos);

        /// <summary>
        /// Check whether a point lies on the boundary of the polygon.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        bool OnBoundary(Vector2 pos);

        /// <summary>
        /// Checks whether the point is a vertex in the polygon.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        bool ContainsVertex(Vector2 pos);

        /// <summary>
        /// Check whether the polygon is convex.
        /// </summary>
        /// <returns></returns>
        bool IsConvex();

        /// <summary>
        /// Check whether the polygon is simple (no intersections).
        /// </summary>
        /// <returns></returns>
        bool IsSimple();

        /// <summary>
        /// Check whether the vertices are in clockwise order.
        /// </summary>
        /// <returns></returns>
        bool IsClockwise();

        /// <summary>
        /// Reverse the polygon, to toglle CW or CCW order.
        /// </summary>
        void Reverse();

        /// <summary>
        /// Compute bounding box of the polygon.
        /// </summary>
        /// <param name="margin"></param>
        /// <returns></returns>
        Rect BoundingBox(float margin = 0f);
    }
}
