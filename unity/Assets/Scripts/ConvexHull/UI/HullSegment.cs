namespace ConvexHull
{
    using UnityEngine;
    using Util.Geometry;

    public class HullSegment : MonoBehaviour
    {
        public LineSegment Segment { get; set; }

        private HullController m_gameController;

        void Awake()
        {
            m_gameController = FindObjectOfType<HullController>();
        }

        void OnMouseUpAsButton()
        {
            // destroy the road object
            m_gameController.RemoveSegment(this);
            Destroy(gameObject);
        }
    }
}
