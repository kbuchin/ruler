namespace Util.Geometry.Polygon
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    
    public interface IPolygon2D
    {
        ICollection<Vector2> Vertices { get; }

        ICollection<LineSegment> Segments { get; }

        Vector2? Next(Vector2 pos);
        Vector2? Prev(Vector2 pos);

        void AddVertex(Vector2 pos);
        void AddVertexFirst(Vector2 pos);
        void AddVertexAfter(Vector2 after, Vector2 pos);

        void RemoveVertex(Vector2 pos);
        void RemoveFirst();
        void RemoveLast();

        void Clear();

        float Area();
        bool Contains(Vector2 pos);
        bool IsConvex();
        bool IsSimple();

        bool IsClockwise();
    }
}
