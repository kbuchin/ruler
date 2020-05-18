using System;
using JetBrains.Annotations;
using UnityEngine;
using Util.Geometry;

namespace DotsAndPolygons
{
    public class UnityDotsHalfEdge : MonoBehaviour, IDotsHalfEdge
    {
        public IDotsVertex Origin { get; set; }
        public string Name { get; set; }
        public IDotsVertex Destination { get; }
        public IDotsFace IncidentFace { get; set; }
        public IDotsHalfEdge Twin { get; set; }
        public IDotsHalfEdge Prev { get; set; }
        public IDotsHalfEdge Next { get; set; }
        public int Player { get; set; }

        public DotsController GameController { get; set; }

        public LineSegment Segment => Origin != null && Destination != null
            ? new LineSegment(Origin.Coordinates, Destination.Coordinates)
            : null;

        public IDotsHalfEdge Constructor(IDotsHalfEdge halfEdge, [CanBeNull] DotsController mGameController) =>
            Constructor(
                mGameController ? mGameController : halfEdge.GameController,
                halfEdge.Player,
                halfEdge.Origin,
                halfEdge.Twin,
                halfEdge.Prev,
                halfEdge.Next,
                halfEdge.Name
            );

        public IDotsHalfEdge Constructor(
            DotsController mGameController,
            int player,
            IDotsVertex origin,
            IDotsHalfEdge twin,
            IDotsHalfEdge prev = null,
            IDotsHalfEdge next = null,
            string name = null)
        {
            if (mGameController == null)
                throw new ArgumentException("mGameController cannot be null");
            Player = player;
            Origin = origin;
            Twin = twin;
            Prev = prev;
            Next = next;
            Name = name;
            GameController = mGameController;

            return this;
        }

        public override string ToString() => $"[{Origin.Coordinates} -> {Destination?.Coordinates}]";
        
        public IDotsHalfEdge Clone() => new DotsHalfEdge().Constructor(GameController, Player, Origin, Twin, Prev, Next, Name);
    }
}