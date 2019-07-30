using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using KingsTaxes;
using Util.Math;
using Util.Geometry.Polygon;
using Util.Algorithms.Polygon;

namespace ArtGallery {
    public class ArtGalleryController : MonoBehaviour
    {
        [SerializeField]
        private string m_nextlevel = "ag";

        [SerializeField]
        private List<Vector2> m_levelPoints = new List<Vector2>() { new Vector2(2, 2), new Vector2(2, 0), new Vector2(0, 0) }; //TODO fake triangle

        [SerializeField]
        private int m_maxNumberOfLighthouses = 2;

        [SerializeField]
        private GameObject m_lighthousePrefab;


        private float m_eps = Mathf.Epsilon * 10;
        private Polygon2DWithHoles m_level;
        private IslandMesh m_levelMesh;
        private List<LightHouse> m_lighthouses = new List<LightHouse>();
        private LightHouse m_selectedLighthouse;
        private DisableButtonContainer m_advanceButton;


        internal Polygon2DWithHoles LevelPolygon { get { return m_level;} }


        public void Start()
        {
            m_level = new Polygon2DWithHoles(new Polygon2D( m_levelPoints));
            m_levelMesh = GetComponentInChildren<IslandMesh>();
            m_levelMesh.Polygon = m_level.Outside;
            UpdateLighthouseText();

            m_advanceButton = GameObject.FindGameObjectWithTag(Tags.AdvanceButtonContainer).GetComponent<DisableButtonContainer>();
            m_advanceButton.Disable();
        }

        internal void SelectLighthouse(LightHouse a_select)
        {
            m_selectedLighthouse = a_select;
        }

        public void Update()
        {
            if (m_selectedLighthouse != null)
            {
                var worldlocation = Camera.main.ScreenPointToRay(Input.mousePosition).origin;
                worldlocation.z = -2f;

                m_selectedLighthouse.Pos = worldlocation;

                if (Input.GetMouseButtonUp(0))
                {
                    //check whether lighthouse is over the island
                    if(m_level.Contains(m_selectedLighthouse.Pos))
                    {
                    }
                    //if it's not destroy it
                    else
                    {
                        m_lighthouses.Remove(m_selectedLighthouse);
                        Destroy(m_selectedLighthouse.gameObject);
                        UpdateLighthouseText();              
                    }

                    m_selectedLighthouse = null;
                    CheckSolution();
                }
            }
        }

        internal void IslandClick()
        {
            if (m_selectedLighthouse == null && m_lighthouses.Count < m_maxNumberOfLighthouses)
            {
                var worldlocation = Camera.main.ScreenPointToRay(Input.mousePosition).origin;
                worldlocation.z = -2f;

                GameObject go = Instantiate(m_lighthousePrefab, worldlocation, Quaternion.identity) as GameObject;

                m_lighthouses.Add(go.GetComponent<LightHouse>());
                UpdateLighthouseText();

                CheckSolution();
            }
        }

        private void CheckSolution()
        {
            if (m_lighthouses.Count > 0)
            {
                var visiblePolygon = new MultiPolygon2D(m_lighthouses[0].VisionArea);
                foreach ( LightHouse lighthouse in m_lighthouses.Skip(1)){
                    Clipper.CutOut(visiblePolygon, lighthouse.VisionArea);
                    visiblePolygon.AddPolygon(lighthouse.VisionArea);
                }
                var ratio = visiblePolygon.Area() / LevelPolygon.Area();
                Debug.Log(ratio + "part is visible");
                if(ratio > 1 - m_eps)
                {
                    m_advanceButton.Enable();
                }

                Visibility.Vision(LevelPolygon, m_lighthouses[0].Pos);
            }
        }

        /// <summary>
        /// Advances to the next level
        /// </summary>
        public void AdvanceLevel()
        {
            SceneManager.LoadScene(m_nextlevel);
        }

        private void UpdateLighthouseText()
        {
            GetComponentInChildren<Text>().text = "Torches left: " + (m_maxNumberOfLighthouses - m_lighthouses.Count);
        }
    }
}