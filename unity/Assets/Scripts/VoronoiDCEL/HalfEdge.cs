namespace VoronoiDCEL
{
    public sealed class HalfEdge<T>
    {
        private Vertex<T> m_Origin;
        private HalfEdge<T> m_Twin;
        private Face<T> m_IncidentFace;
        // the face to the left.
        private HalfEdge<T> m_Next;
        // next halfedge on the boundary of IncidentFace.
        private HalfEdge<T> m_Previous;
        // previous halfedge on the boundary of IncidentFace
        private Edge<T> m_ParentEdge;

        public Vertex<T> Origin
        {
            get { return m_Origin; }
            set { m_Origin = value; }
        }

        public HalfEdge<T> Previous
        {
            get { return m_Previous; }
            set { m_Previous = value; }
        }

        public HalfEdge<T> Next
        {
            get { return m_Next; }
            set { m_Next = value; }
        }

        public HalfEdge<T> Twin
        {
            get { return m_Twin; }
            set { m_Twin = value; }
        }

        public Face<T> IncidentFace
        {
            get { return m_IncidentFace; }
            set { m_IncidentFace = value; }
        }

        public Edge<T> ParentEdge
        {
            get { return m_ParentEdge; }
            set { m_ParentEdge = value; }
        }
    }
}

