namespace DotsAndPolygons
{
    using UnityEngine;

    // Vertex
    public class UnityDotsVertex : MonoBehaviour, IDotsVertex
    {
        // Coordinates of this vertex in the game
        public Vector2 Coordinates { get; set; }

        // Some half-edge leaving this vertex
        public IDotsHalfEdge IncidentEdge { get; set; }

        // Reference to the main game class
        public DotsController mController;

        public bool InFace { get; set; } = false;

        public bool OnHull { get; set; } = false;

        private void Awake()
        {
            Vector3 position = transform.position;
            Coordinates = new Vector2(position.x, position.y);
            mController = FindObjectOfType<DotsController>();
            mController.Vertices.Add(this);
        }

        private void OnMouseDown()
        {
            if (InFace || mController.CurrentPlayer.PlayerType != PlayerType.Player) return;
            mController.EnableDrawingLine();
            mController.FirstPoint = this;
            mController.SetDrawingLinePosition(0, Coordinates);
        }

        public void OnMouseEnter()
        {
            if (mController.FirstPoint == null) return;
            mController.SecondPoint = this;
            mController.SetDrawingLinePosition(1, Coordinates);
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
            $"Coordinates: {Coordinates}, IncidentEdge: {IncidentEdge?.ToString()}, InFace: {InFace}";

        public IDotsVertex Clone() => new DotsVertex(Coordinates, IncidentEdge, InFace, OnHull);
    }
}