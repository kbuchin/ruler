namespace TheHeist
{
    using General.Model;

    /// <summary>
    /// Represents the level island (2D polygon).
    /// Handles user clicks on the polygon
    /// </summary>
    public class TheHeistIsland : Polygon2DWithHolesMesh
    {
        private TheHeistController m_controller;

        public TheHeistIsland()
        {
            m_scale = 9.65f;
        }

        // Use this for initialization
        public new void Awake()
        {
            base.Awake();
            m_controller = FindObjectOfType<TheHeistController>();
        }

        void OnMouseUpAsButton()
        {
            // call the relevant controller method
            m_controller.HandleIslandClick();
        }
    }
}
