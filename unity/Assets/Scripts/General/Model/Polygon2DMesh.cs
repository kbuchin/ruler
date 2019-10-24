namespace General.Model
{
    using System.Linq;
    using UnityEngine;
    using Util.Algorithms.Triangulation;
    using Util.Geometry.Polygon;

    public class Polygon2DMesh : MonoBehaviour
    {
        private Polygon2D m_polygon;

        private MeshFilter m_meshFilter;
        private Renderer m_renderer;
        private MeshCollider m_collider;

        protected float m_scale;

        /// <summary>
        /// Seting the Polygon will automatically update the Mesh
        /// </summary>
        public Polygon2D Polygon
        {
            get
            {
                return m_polygon;
            }
            set
            {
                m_polygon = value;
                UpdateMesh();
            }
        }

        protected Polygon2DMesh(float scale)
        {
            m_scale = scale;
        }

        protected Polygon2DMesh()
        { }

        protected void Awake()
        {
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
                Debug.Log("Polygon is not correctly set");
                return;
            }

            var oldMesh = m_meshFilter.mesh;

            //create triangulation
            var tri = Triangulator.Triangulate(Polygon2D.RemoveDanglingEdges(m_polygon));

            var mesh = tri.CreateMesh();
            m_meshFilter.mesh = mesh;

            //duplicateMaterial and scale
            var size = Mathf.Max(mesh.bounds.size.x, mesh.bounds.size.y);
            var newMat = new Material(m_renderer.material)
            {
                mainTextureScale = new Vector2(size / m_scale, size / m_scale)
            };
            m_renderer.materials = new Material[] { newMat }; //also remove olde material by replacing material array

            //set the same mesh to the colider
            if (m_collider != null)
            {
                m_collider.sharedMesh = mesh;
            }
        }
    }
}
