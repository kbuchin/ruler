using System;
using UnityEngine;

namespace DotsAndPolygons
{
    [Serializable]
    public class DotsVertex
    {
        private SerializableVector2 serializableVector;
        public Vector2 Coordinates
        {
            get
            {
                return serializableVector.Vector2;
            }
            set
            {
                serializableVector = new SerializableVector2(value);
            }
        }
        public DotsHalfEdge IncidentEdge { get; set; } = null;
        public bool InFace { get; set; } = false;

        public bool OnHull { get; set; } = false;

        public DotsVertex(Vector2 coordinates, DotsHalfEdge incidentEdge = null, bool inFace = false, bool onHull = false)
        {
            Coordinates = coordinates;
            IncidentEdge = incidentEdge;
            InFace = inFace;
            OnHull = onHull;
        }

        public DotsVertex(DotsVertex vertex)
        {
            Coordinates = vertex.Coordinates;
            IncidentEdge = vertex.IncidentEdge;
            InFace = vertex.InFace;
            OnHull = vertex.OnHull;
        }
        
        public override string ToString() => $"Coordinates: {Coordinates}, IncidentEdge: {IncidentEdge?.ToString()}, InFace: {InFace}";

        public DotsVertex Clone() => new DotsVertex(Coordinates, IncidentEdge.Clone(), InFace, OnHull);
    }
}