namespace KingsTaxes
{
    using UnityEngine;
    using Util.Geometry.Graph;

    public class ReshapingMesh : MonoBehaviour {

        public float repeatDistance = 5f;
        public float widthmodifier = .3f;

        private MeshFilter m_meshFilter;
        private Renderer m_renderer;
        private MeshCollider m_collider;

        void Awake()
        {
            m_meshFilter = GetComponent<MeshFilter>();
            m_collider = GetComponent<MeshCollider>();
            m_renderer = GetComponent<Renderer>();
        }

        internal void CreateNewMesh(Vector3 a_start, Vector3 a_end)
        {
            var oldMesh = m_meshFilter.mesh;

            Mesh mesh = new Mesh();
            var direction = a_start - a_end;
            var perp = new Vector3(direction.y, -direction.x, 0);
            perp.Normalize();
            perp = perp * widthmodifier;

            //create 4 corner vertices
            var newVertices = new Vector3[] { a_start- perp, a_start + perp, a_end- perp, a_end + perp};
            var newTriangles = new int[] { 0, 1, 2, 3, 2, 1}; //quad
            var newUV = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };

            mesh.vertices = newVertices;
            mesh.uv = newUV;
            mesh.triangles = newTriangles;

            m_meshFilter.mesh = mesh;

            //duplicateMaterial and scale
            var newMat = new Material(m_renderer.material);
            newMat.mainTextureScale = new Vector2(direction.magnitude / repeatDistance, 1);
            newMat.mainTextureOffset = new Vector2(Random.Range(0f,1f), 0f);
            m_renderer.materials = new Material[] { newMat }; //also remove olde material by replacing material array

            //set the same mesh to the colider
            m_collider.sharedMesh = mesh;
        }
    }

}
