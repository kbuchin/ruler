namespace DotsAndPolygons
{
    using UnityEngine;

    // Vertex
    public class UnityDotsVertex : MonoBehaviour
    {
        public DotsVertex dotsVertex { get; set; }

        // Reference to the main game class
        public DotsController mController;

        private void Awake()
        {
            Vector3 position = transform.position;
            
            Vector2 coordinates = new Vector2(position.x, position.y);
            dotsVertex = new DotsVertex(coordinates);
            mController = FindObjectOfType<DotsController>();
            mController.Vertices.Add(this);
        }

        private void OnMouseDown()
        {
            if (dotsVertex.InFace || mController.CurrentPlayer.PlayerType != PlayerType.Player) return;
            mController.EnableDrawingLine();
            mController.FirstPoint = this;
            mController.SetDrawingLinePosition(0, dotsVertex.Coordinates);
        }

        public void OnMouseEnter()
        {
            if (mController.FirstPoint == null) return;
            mController.SecondPoint = this;
            mController.SetDrawingLinePosition(1, dotsVertex.Coordinates);
        }

        public void OnMouseExit()
        {
            if (this != mController.SecondPoint) return;
            mController.SecondPoint = null;
            if (Camera.main == null) return;
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition + 10 * Vector3.forward);
            mController.SetDrawingLinePosition(1, pos);
        }

        public override string ToString() =>
            $"Coordinates: {dotsVertex.Coordinates}, IncidentEdge: {dotsVertex.IncidentEdge?.ToString()}, InFace: {dotsVertex.InFace}";

    }
}