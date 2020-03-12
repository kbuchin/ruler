namespace General.Model
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Algorithms.Triangulation;
    using Util.Geometry.Polygon;
    using Util.Geometry.Triangulation;

    /// <summary>
    /// Calculates a (triangular) mesh for a given polygon and gives this to mesh filter and collider components.
    /// Attach to a game object that contains MeshFilter and Renderer (and potentially a MeshCollider)
    /// 
    /// This method does not calculate the actual triangulation of the polygon with holes,
    /// instead it simply draws black "hole" meshes over the polygon mesh.
    /// </summary>
    public class Polygon2DWithHolesMesh : MonoBehaviour
    {
        /// <summary>
        /// Polygon for the current mesh.
        /// Setting the Polygon will automatically update the Mesh.
        /// </summary>
        public Polygon2DWithHoles Polygon
        {
            get { return m_polygon; }
            set { m_polygon = value; UpdateMesh(); }
        }

        protected Polygon2DWithHoles m_polygon;

        // scale factor for the texture
        protected float m_scale;

        // relevant mesh renderer objects
        private MeshFilter m_meshFilter;
        private Renderer m_renderer;
        private MeshCollider m_collider;
        private GameObject m_object;


        public Polygon2DWithHolesMesh(float scale = 1f)
        {
            m_scale = scale;
        }

        // Use this for initialization
        public void Awake()
        {
            // get mesh renderer components
            // should be attached to game object with this script
            m_meshFilter = GetComponent<MeshFilter>();
            m_collider = GetComponent<MeshCollider>();
            m_renderer = GetComponent<Renderer>();
            m_object = m_renderer.gameObject;
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
            var tri = Triangulator.Triangulate(m_polygon.Outside, false);

            // create mesh from triangulation
            var mesh = tri.CreateMesh();
            m_meshFilter.mesh = mesh;

            //duplicate Material and scale
            var size = Mathf.Max(mesh.bounds.size.x, mesh.bounds.size.y);
            var newMats = new Material(m_renderer.materials[0])
            {
                mainTextureScale = new Vector2(size / m_scale, size / m_scale)
            };

            m_renderer.materials = new Material[] { newMats };

            // set the same mesh to the colider
            if (m_collider != null)
            {
                m_collider.sharedMesh = mesh;
            }

            // destroy old children
            foreach (Transform child in m_object.transform)
            {
                Destroy(child.gameObject);
            }

            // add hole meshes
            foreach (var hole in m_polygon.Holes)
            {
                var holeObject = Instantiate(Resources.Load("HoleMesh")) as GameObject;
                holeObject.transform.parent = m_object.transform;
                holeObject.transform.position = new Vector3(0, 0, m_object.transform.position.z - 0.1f);

                var holeMesh = Triangulator.Triangulate(hole, false).CreateMesh();

                holeObject.GetComponent<MeshFilter>().mesh = holeMesh;
                holeObject.GetComponent<MeshCollider>().sharedMesh = holeMesh;
            }
        }
    }
}
