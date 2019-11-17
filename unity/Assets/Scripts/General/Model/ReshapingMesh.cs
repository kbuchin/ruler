namespace General.Model
{
    using UnityEngine;

    /// <summary>
    /// Class that can modify a given mesh with given modifier variables
    /// </summary>
    public class ReshapingMesh : MonoBehaviour
    {
        // shape modifiers
        public float repeatDistance = 5f;
        public float widthmodifier = .3f;

        private MeshFilter m_meshFilter;
        private Renderer m_renderer;
        private MeshCollider m_collider;

        // Use this for initialization
        void Awake()
        {
            // get mesh renderer components
            m_meshFilter = GetComponent<MeshFilter>();
            m_collider = GetComponent<MeshCollider>();
            m_renderer = GetComponent<Renderer>();
        }

        public void CreateNewMesh(Vector3 a_start, Vector3 a_end)
        {
            if (a_start == a_end)
            {
                return;
            }

            var oldMesh = m_meshFilter.mesh;

            // initialize new mesh
            Mesh mesh = new Mesh();

            // get perpendicular vector
            var direction = a_start - a_end;
            var perp = new Vector3(direction.y, -direction.x, 0);
            perp.Normalize();
            perp *= widthmodifier;

            //create 4 corner vertices
            var newVertices = new Vector3[] { a_start - perp, a_start + perp, a_end - perp, a_end + perp };
            var newTriangles = new int[] { 0, 1, 2, 3, 2, 1 }; //quad
            var newUV = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };

            // set new mesh
            mesh.vertices = newVertices;
            mesh.uv = newUV;
            mesh.triangles = newTriangles;

            // update mesh filter with new mesh
            m_meshFilter.mesh = mesh;

            //duplicate material and scale
            var newMat = new Material(m_renderer.material)
            {
                mainTextureScale = new Vector2(direction.magnitude / repeatDistance, 1),
                mainTextureOffset = new Vector2(Random.Range(0f, 1f), 0f)
            };
            m_renderer.materials = new Material[] { newMat }; //also remove olde material by replacing material array

            //set the same mesh to the colider
            m_collider.sharedMesh = mesh;
        }
    }
}
