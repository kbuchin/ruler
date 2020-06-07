namespace DotsAndPolygons
{
    using UnityEngine;
    using Util.Geometry;

    public class UnityDotsEdge : MonoBehaviour
    {
        DotsEdge DotsEdge { get; set; }
        
        private DotsController _mGameController;

        private void Awake()
        {
            _mGameController = FindObjectOfType<DotsController>();
            DotsEdge = new DotsEdge(null);
            DotsEdge.Player = _mGameController.CurrentPlayerValue;
        }
        
        public override string ToString() => $"{this.DotsEdge.Segment}, Player: {this.DotsEdge.Player}";
    }
}