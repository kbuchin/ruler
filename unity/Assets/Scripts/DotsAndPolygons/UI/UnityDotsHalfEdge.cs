using System;
using JetBrains.Annotations;
using UnityEngine;
using Util.Geometry;

namespace DotsAndPolygons
{
    public class UnityDotsHalfEdge : MonoBehaviour
    {
        public DotsHalfEdge DotsHalfEdge { get; set; }

        public DotsController GameController { get; set; }

        public UnityDotsHalfEdge Constructor(DotsHalfEdge halfEdge)
        {
            DotsHalfEdge = halfEdge;
            return this;
        }

        public UnityDotsHalfEdge Constructor(
            int player,
            DotsVertex origin,
            DotsHalfEdge twin,
            DotsHalfEdge prev = null,
            DotsHalfEdge next = null,
            string name = null)
        {
            DotsHalfEdge = new DotsHalfEdge().Constructor(player, origin, twin, prev,next,name);
            return this;
        }

        public override string ToString() => $"[{DotsHalfEdge.Origin.Coordinates} -> {DotsHalfEdge.Destination?.Coordinates}]";
        

    }
}