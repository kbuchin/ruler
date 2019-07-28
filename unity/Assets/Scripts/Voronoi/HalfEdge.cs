namespace Voronoi
{
    public sealed class HalfEdge
    {
        private Vertex m_Origin;
        public Triangle Triangle;
        public HalfEdge Twin;
        public HalfEdge Next;
        public HalfEdge Prev;

        public HalfEdge(Vertex a_Vertex)
        {
            // Using new here prevents reference equality. Is it really necessary to copy?
            m_Origin = new Vertex(a_Vertex.X, a_Vertex.Y, a_Vertex.Ownership);
            Triangle = null;
            Twin = null;
            Next = null;
            Prev = null;
        }

        public Vertex Origin
        {
            get { return m_Origin; }
            // Using new here prevents reference equality. Is it really necessary to copy?
            set { m_Origin = new Vertex(value.X, value.Y, value.Ownership); }
        }
    }

}
