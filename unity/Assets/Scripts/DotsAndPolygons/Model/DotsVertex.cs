﻿using UnityEngine;

namespace DotsAndPolygons
{
    public class DotsVertex: IDotsVertex
    {
        public Vector2 Coordinates { get; set; }
        public IDotsHalfEdge IncidentEdge { get; set; } = null;
        public bool InFace { get; set; } = false;

        public bool OnHull { get; set; } = false;

        public DotsVertex(Vector2 coordinates, IDotsHalfEdge incidentEdge = null, bool inFace = false, bool onHull = false)
        {
            Coordinates = coordinates;
            IncidentEdge = incidentEdge;
            InFace = inFace;
            OnHull = onHull;
        }
        
        public override string ToString() => $"Coordinates: {Coordinates}, IncidentEdge: {IncidentEdge?.ToString()}, InFace: {InFace}";

        public IDotsVertex Clone() => new DotsVertex(Coordinates, IncidentEdge, InFace, OnHull);
    }
}