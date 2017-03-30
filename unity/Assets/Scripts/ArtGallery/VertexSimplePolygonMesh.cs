using Algo;
using Algo.Polygons;
using System.Linq;
using UnityEngine;


namespace ArtGallery
{
    class VertexSimplePolygonMesh : MonoBehaviour
    {
        private VertexSimplePolygon m_polygon;

        private MeshFilter m_meshFilter;
        private Renderer m_renderer;
        private MeshCollider m_collider;
        protected float m_scale;

        /// <summary>
        /// Seting the Polygon will automatically update the Mesh
        /// </summary>
        public VertexSimplePolygon Polygon { get { return m_polygon; } set { m_polygon = value; UpdateMesh(); } }

        protected VertexSimplePolygonMesh(float scale)
        {
            m_scale = scale;
        }

        protected VertexSimplePolygonMesh()
        {

        }

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

            Mesh mesh = new Mesh();

            //create vertices
            var newPoints = m_polygon.Vertices.ToArray();
            var newVertices = newPoints.Select<Vector2, Vector3>(p => p).ToArray(); //use automatic casting
            var tri = new Triangulator(newPoints);
            var newTriangles = tri.Triangulate();

            //Calculate UV's
            var bbox = BoundingBoxComputer.FromVector2(newPoints.ToList());
            var newUV = newPoints.Select<Vector2, Vector2>(p => Rect.PointToNormalized(bbox, p)).ToArray();

            mesh.vertices = newVertices;
            mesh.uv = newUV;
            mesh.triangles = newTriangles;
            m_meshFilter.mesh = mesh;

            //duplicateMaterial and scale
            var newMat = new Material(m_renderer.material);
            var size = Mathf.Max(bbox.width, bbox.height);
            newMat.mainTextureScale = new Vector2(size/m_scale, size/m_scale);
            m_renderer.materials = new Material[] { newMat }; //also remove olde material by replacing material array

            //set the same mesh to the colider
            if(m_collider != null)
            {
                m_collider.sharedMesh = mesh;
            }
        }
    }
}
