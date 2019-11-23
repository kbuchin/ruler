namespace General.Model
{
    using UnityEngine;
    using Util.Algorithms.Triangulation;
    using Util.Geometry.Polygon;

    /// <summary>
    /// Calculates a (triangular) mesh for a given polygon and gives this to mesh filter and collider components.
    /// Attach to a game object that contains MeshFilter and Renderer (and potentially a MeshCollider)
    /// </summary>
    public class Polygon2DMesh : MonoBehaviour
    {
        /// <summary>
        /// Polygon for the current mesh.
        /// Setting the Polygon will automatically update the Mesh.
        /// </summary>
        public Polygon2D Polygon
        {
            get { return m_polygon; }
            set { m_polygon = value; UpdateMesh(); }
        }

        protected Polygon2D m_polygon;

        // scale factor for the texture
        protected float m_scale;

        // relevant mesh renderer objects
        private MeshFilter m_meshFilter;
        private Renderer m_renderer;
        private MeshCollider m_collider;


        public Polygon2DMesh(float scale)
        {
            m_scale = scale;
        }

        public Polygon2DMesh()
        { }

        // Use this for initialization
        public void Awake()
        {
            // get mesh renderer components
            // should be attached to game object with this script
            m_meshFilter = GetComponent<MeshFilter>();
            m_collider = GetComponent<MeshCollider>();
            m_renderer = GetComponent<Renderer>();
        }

        /// <summary>
        /// Updates the mesh to represent the polygon
        /// </summary>
        protected void UpdateMesh()
        {
            if (m_polygon == null || m_polygon.Vertices.Count < 3)
            {
                m_meshFilter.mesh = new Mesh();
                return;
            }

            var oldMesh = m_meshFilter.mesh;

            // create triangulation
            var tri = Triangulator.Triangulate(m_polygon.RemoveDanglingEdges(), false);

            // create mesh from triangulation
            var mesh = tri.CreateMesh();
            m_meshFilter.mesh = mesh;

            //duplicate Material and scale
            var size = Mathf.Max(mesh.bounds.size.x, mesh.bounds.size.y);
            var newMat = new Material(m_renderer.material)
            {
                mainTextureScale = new Vector2(size / m_scale, size / m_scale)
            };
            m_renderer.materials = new Material[] { newMat }; //also remove olde material by replacing material array

            // set the same mesh to the colider
            if (m_collider != null)
            {
                m_collider.sharedMesh = mesh;
            }
        }
    }
}
