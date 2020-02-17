namespace DotsAndPolygons
{
    using UnityEngine;

    public interface IDotsVertex
    {
        // Coordinates of this vertex in the game
        Vector2 Coordinates { get; set; }

        // Some half-edge leaving this vertex
        IDotsHalfEdge IncidentEdge { get; set; }

        // Whether this vertex lies in a face, should default to false
        bool InFace { get; set; }
        
        // Whether this vertex lies on the hull, so it can never be disabled
        bool OnHull { get; set; }
        
        string ToString();
    }
}