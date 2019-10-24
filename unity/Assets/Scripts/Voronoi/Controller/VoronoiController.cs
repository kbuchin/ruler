namespace Voronoi.Controller
{
    using UnityEngine;
    using System.Collections.Generic;
    using System;
    using Voronoi.UI;
    using Util.Geometry.DCEL;
    using Util.Algorithms.Triangulation;
    using Util.Geometry.Triangulation;
    using UnityEngine.SceneManagement;
    using Util.Geometry.Polygon;
    using Util.Algorithms.Graph;
    using System.Linq;

    public sealed class VoronoiController : MonoBehaviour
    {        
        public GameObject m_Player1Prefab;
        public GameObject m_Player2Prefab;

        public bool m_withLookAtOnPlacement = true;
        public int m_turns;
        public string m_p1Victory;
        public string m_p2Victory;

        public VoronoiGUIManager m_GUIManager;
        public MeshFilter m_meshFilter;

        private int m_halfTurnsTaken = 0;
        private bool player1Turn = true;
        private float[] m_playerArea;

        private Triangulation m_delaunay;
        private FishManager m_fishManager;

        private Polygon2D m_meshRect;
        private DCEL m_DCEL;

        private readonly Dictionary<Vector2, EOwnership> m_ownership = new Dictionary<Vector2, EOwnership>();

        private struct MeshDescription
        {
            public Vector3[] vertices;
            public int[][] triangles;
        }

        private enum EOwnership
        {
            UNOWNED,
            PLAYER1,
            PLAYER2
        }

        private void Start()
        {
            m_delaunay = Delaunay.Create();
            foreach (var vertex in m_delaunay.Vertices)
            {
                // add auxiliary vertices as unowned
                m_ownership.Add(vertex, EOwnership.UNOWNED);
            }

            m_fishManager = new FishManager();

            // create polygon of rectangle window for intersection with voronoi
            float z = (m_meshFilter.transform.position - Camera.main.transform.position).magnitude;
            var bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, z));
            var topRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, z));
            m_meshRect = new Polygon2D (
                new List<Vector2>() {
                    new Vector2(bottomLeft.x, bottomLeft.z),
                    new Vector2(bottomLeft.x, topRight.z),
                    new Vector2(topRight.x, topRight.z),
                    new Vector2(topRight.x, bottomLeft.z)
                });

            VoronoiDrawer.CreateLineMaterial();
        }

        private void UpdateVoronoi()
        {
            m_DCEL = Voronoi.Create(m_delaunay);

            m_playerArea = new float[2] { 0, 0 };

            UpdateMesh(m_delaunay);

            m_GUIManager.SetPlayerAreaOwned(m_playerArea[0], m_playerArea[1]);
        }

        private void UpdateMesh(Triangulation m_Delaunay)
        {
            if (m_meshFilter.mesh == null)
            {
                m_meshFilter.mesh = new Mesh
                {
                    subMeshCount = 2
                };
                m_meshFilter.mesh.MarkDynamic();
            }
            else
            {
                m_meshFilter.mesh.Clear();
                m_meshFilter.mesh.subMeshCount = 2;
            }

            // build vertices and triangle list
            var vertices = new List<Vector3>();
            var triangles = new List<int>[2] {
                new List<int>(),
                new List<int>()
            };

            // iterate over vertices and create triangles accordingly
            foreach (var inputNode in m_delaunay.Vertices)
            {
                bool unowned = m_ownership[inputNode] == EOwnership.UNOWNED;
                int playerIndex = m_ownership[inputNode] == EOwnership.PLAYER1 ? 0 : -1;
                playerIndex = m_ownership[inputNode] == EOwnership.PLAYER2 ? 1 : playerIndex;

                var face = m_DCEL.GetContainingFace(inputNode);

                // cant triangulate outer face
                if (face.IsOuter)
                {
                    continue;
                }

                // triangulate face polygon
                var triangulation = Triangulator.Triangulate(face.Polygon.Outside);

                // add triangles to correct list
                foreach (var triangle in triangulation.Triangles)
                {
                    int curCount = vertices.Count;

                    vertices.Add(new Vector3(triangle.P0.x, 0, triangle.P0.y));
                    vertices.Add(new Vector3(triangle.P1.x, 0, triangle.P1.y));
                    vertices.Add(new Vector3(triangle.P2.x, 0, triangle.P2.y));

                    if (unowned)
                    {
                        triangles[0].Add(curCount);
                        triangles[0].Add(curCount + 1);
                        triangles[0].Add(curCount + 2);
                        triangles[1].Add(curCount);
                        triangles[1].Add(curCount + 1);
                        triangles[1].Add(curCount + 2);
                    }
                    else
                    {
                        triangles[playerIndex].Add(curCount);
                        triangles[playerIndex].Add(curCount + 1);
                        triangles[playerIndex].Add(curCount + 2);
                        triangles[playerIndex].Add(curCount);
                        triangles[playerIndex].Add(curCount + 2);
                        triangles[playerIndex].Add(curCount + 1);
                    }
                }
                    
                if (!unowned)
                {
                    m_playerArea[playerIndex] += Polygon2D.IntersectConvex(m_meshRect, face.Polygon.Outside).Area;
                }
            }

            // update mesh
            m_meshFilter.mesh.vertices = vertices.ToArray();
            m_meshFilter.mesh.SetTriangles(triangles[0], 0);
            m_meshFilter.mesh.SetTriangles(triangles[1], 1);
            m_meshFilter.mesh.RecalculateBounds();

            // set correct uv
            var newUVs = new List<Vector2>();
            foreach (var vertex in vertices)
            {
                newUVs.Add(new Vector2(vertex.x, vertex.z));
            }
            m_meshFilter.mesh.uv = newUVs.ToArray();
        }

        private void OnRenderObject()
        {
            GL.PushMatrix();

            // Set transformation matrix for drawing to
            // match our transform
            GL.MultMatrix(transform.localToWorldMatrix);

            VoronoiDrawer.Draw(m_delaunay);

            GL.PopMatrix();
        }

        private void Update()
        {
            if (Input.GetKeyDown("c"))
            {
                VoronoiDrawer.CircleOn = !VoronoiDrawer.CircleOn;
            }

            if (Input.GetKeyDown("e"))
            {
                VoronoiDrawer.EdgesOn = !VoronoiDrawer.EdgesOn;
            }

            if (Input.GetKeyDown("v"))
            {
                VoronoiDrawer.VoronoiOn = !VoronoiDrawer.VoronoiOn;
            }

            if (Input.GetMouseButtonDown(0))
            {
                ProcessTurn();
            }
        }

        private void ProcessTurn()
        {
            if(m_halfTurnsTaken == 0)
            {
                m_GUIManager.OnStartClicked();
            }

            if (m_halfTurnsTaken >= 2 * m_turns)
            {
                if (m_playerArea[0] > m_playerArea[1])
                {
                    SceneManager.LoadScene(m_p1Victory);
                }
                else
                {
                    SceneManager.LoadScene(m_p2Victory);
                }
            }
            else
            {
                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                pos.y = 0;

                // add randomness to achieve general positions

                var me = new Vector2(pos.x, pos.z);

                if (m_ownership.ContainsKey(me))
                {
                    Debug.Log("Cannot click on existing vertex");
                    return;
                }
                m_ownership.Add(me, player1Turn ? EOwnership.PLAYER1 : EOwnership.PLAYER2);

                Delaunay.AddVertex(m_delaunay, me);

                GameObject onClickObject;
                if (player1Turn)
                {
                    onClickObject = Instantiate(m_Player1Prefab, pos, Quaternion.identity) as GameObject;
                }
                else
                {
                    onClickObject = Instantiate(m_Player2Prefab, pos, Quaternion.identity) as GameObject;
                }

                if (onClickObject == null)
                {
                    Debug.LogError("Couldn't instantiate m_PlayerPrefab!");
                }
                else
                {
                    onClickObject.transform.parent = this.gameObject.transform;
                    m_fishManager.AddFish(onClickObject.transform, player1Turn, m_withLookAtOnPlacement);
                }

                UpdateVoronoi();

                player1Turn = !player1Turn;
                if (player1Turn)
                {
                    m_GUIManager.OnBlueTurnStart();
                }
                else
                {
                    m_GUIManager.OnRedTurnStart();
                }

                //Update turn counter
                m_halfTurnsTaken += 1;

                if (m_halfTurnsTaken >= 2 * m_turns)
                {
                    m_GUIManager.OnLastMove();
                }
            }
        }
    }
}