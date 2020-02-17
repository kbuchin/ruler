namespace DotsAndPolygons
{
    using UnityEngine;
    using Util.Geometry;

    public class UnityDotsEdge : MonoBehaviour, IDotsEdge
    {
        public LineSegment Segment { get; set; }

        public IDotsHalfEdge RightPointingHalfEdge { get; set; } = null;
        public IDotsHalfEdge LeftPointingHalfEdge { get; set; } = null;
        
        private DotsController _mGameController;

        public int Player { get; set; }

        private void Awake()
        {
            _mGameController = FindObjectOfType<DotsController>();
            Player = _mGameController.CurrentPlayer;
        }
        
        public override string ToString() => $"{Segment}, Player: {Player}";
    }
}