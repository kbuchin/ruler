namespace General.Mesh
{
    using System.Linq;
    using UnityEngine;
    using Util.Algorithms.Triangulation;
    using Util.Geometry.Polygon;

    class Polygon2DMesh : MonoBehaviour
    {
        private IPolygon2D m_polygon;

        private MeshFilter m_meshFilter;
        private Renderer m_renderer;
        private MeshCollider m_collider;
        protected float m_scale;

        /// <summary>
        /// Seting the Polygon will automatically update the Mesh
        /// </summary>
        public IPolygon2D Polygon { get { return m_polygon; } set { m_polygon = value; UpdateMesh(); } }

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
            var oldMesh = m_meshFilter.mesh;

            //create triangulation
            var newPoints = m_polygon.Vertices.ToList();
            var newVertices = newPoints.Select<Vector2, Vector3>(p => p).ToArray(); //use automatic casting
            var tri = Triangulator.Triangulate(m_polygon);

            var mesh = tri.CreateMesh();

            //duplicateMaterial and scale
            var newMat = new Material(m_renderer.material);
            newMat.mainTextureScale = new Vector2(mesh.bounds.size.x / m_scale, mesh.bounds.size.y / m_scale);
            m_renderer.materials = new Material[] { newMat }; //also remove olde material by replacing material array

            //set the same mesh to the colider
            if (m_collider != null)
            {
                m_collider.sharedMesh = mesh;
            }
        }
    }
}
