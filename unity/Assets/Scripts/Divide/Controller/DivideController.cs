namespace Divide.Controller
{
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using Util.Geometry.DCEL;
    using Util.Geometry;
    using Util.Geometry.Duality;
    using Drawing;
    using Divide.Model;
    using Divide.UI;
    using General.Menu;
    using Util.Algorithms.DCEL;

    public class DivideController : MonoBehaviour
    {
        [Header("Levels")]
        [SerializeField]
        private List<DivideLevel> m_levels;

        [Header("Menu")]
        [SerializeField]
        private string m_victoryScreen = "agVictory";
        [SerializeField]
        private Text m_swapText;
        [SerializeField]
        private MenuOverlay m_victoryOverlay;

        [Header("Soldier Prefabs")]
        [SerializeField]
        private GameObject m_spearmenPrefab;
        [SerializeField]
        private GameObject m_archerPrefab;
        [SerializeField]
        private GameObject m_magePrefab;

        [Header("Cut Sprites")]
        [SerializeField]
        private Sprite m_sprGreatCut;
        [SerializeField]
        private Sprite m_sprGoodCut;
        [SerializeField]
        private Sprite m_sprTooBad;

        private int m_levelCounter = 0;
        private int m_numberOfSwaps;

        private DivideSoldier selectedSoldier;
        private DivideSolution m_solution;

        private List<GameObject> m_spearmen = new List<GameObject>();
        private List<GameObject> m_archers = new List<GameObject>();
        private List<GameObject> m_mages = new List<GameObject>();

        private DCEL m_archerDcel, m_spearmenDcel,  m_mageDcel;

        private List<Vector2> m_spearmenPos = new List<Vector2>();
        private List<Vector2> m_archersPos = new List<Vector2>();
        private List<Vector2> m_magesPos = new List<Vector2>();

        //Unity references
        private DivideLineDrawer m_lineDrawer;
        private DCELDrawer m_graphDrawer;
        private DivideLine m_mouseLine;

        private bool m_levelSolved;
        private bool m_restartLevel;

        void Start() {
            m_mouseLine = FindObjectOfType<DivideLine>();
            m_lineDrawer = FindObjectOfType<DivideLineDrawer>();
            m_graphDrawer = FindObjectOfType<DCELDrawer>();

            m_victoryOverlay.Callback = NextLevel;

            InitLevel();
        }

        private void InitLevel()
        {
            if (m_levelCounter >= m_levels.Count)
            {
                SceneManager.LoadScene(m_victoryScreen);
                return;
            }

            // clear old level
            foreach (var spearmen in m_spearmen) Destroy(spearmen);
            foreach (var archer in m_archers) Destroy(archer);
            foreach (var mage in m_mages) Destroy(mage);

            m_spearmen.Clear();
            m_archers.Clear();
            m_mages.Clear();

            m_levelSolved = false;
            m_restartLevel = false;

            var level = m_levels[m_levelCounter];

            m_numberOfSwaps = level.NumberOfSwaps;

            // add the different soldiers to scene
            foreach (var spearmen in level.Spearmen)
            {
                var pos = new Vector3(spearmen.x, spearmen.y, -1);
                var obj = Instantiate(m_spearmenPrefab, pos, Quaternion.identity) as GameObject;
                m_spearmen.Add(obj);
            }
            foreach (var archer in level.Archers)
            {
                var pos = new Vector3(archer.x, archer.y, -1);
                var obj = Instantiate(m_archerPrefab, pos, Quaternion.identity) as GameObject;
                m_archers.Add(obj);
            }
            foreach (var mage in level.Mages)
            {
                var pos = new Vector3(mage.x, mage.y, -1);
                var obj = Instantiate(m_magePrefab, pos, Quaternion.identity) as GameObject;
                m_mages.Add(obj);
            }

            UpdateSwapText();
            UpdateArmyPos();
            FindSolution();
        }

        private void UpdateArmyPos()
        {
            //Find position of different soldiers
            m_archersPos = m_archers.Select(x => (Vector2)x.transform.position).ToList();
            m_spearmenPos = m_spearmen.Select(x => (Vector2)x.transform.position).ToList();
            m_magesPos = m_mages.Select(x => (Vector2)x.transform.position).ToList();
        }

        internal void ProcessSolution(Line line)
        {
            if (line.NumberOfPointsAbove(m_magesPos) == m_magesPos.Count / 2 
                && line.NumberOfPointsAbove(m_archersPos) == m_archersPos.Count / 2
                && line.NumberOfPointsAbove(m_spearmenPos) == m_spearmenPos.Count / 2)
            {
                if (m_numberOfSwaps >= 0)
                {
                    m_victoryOverlay.SetSprite(m_sprGreatCut);
                    m_levelSolved = true;
                }
                else
                {
                    m_victoryOverlay.SetSprite(m_sprGoodCut);
                    m_restartLevel = true;
                }
            }
            else
            {
                m_victoryOverlay.SetSprite(m_sprTooBad);
            }

            m_victoryOverlay.Activate();
        }

        public void NextLevel()
        {
            m_mouseLine.DisableLine();

            if (m_levelSolved)
            {
                // load next level
                m_levelCounter++;
                InitLevel();
            }

            if (m_restartLevel)
            {
                // reload current level
                InitLevel();
            }
        }

        void Update()
        {
            //handle input
            if (Input.GetKeyDown("q"))
            {
                Debug.Log("hello");
                m_lineDrawer.ToggleArchers();
            }
            if (Input.GetKeyDown("w"))
            {
                m_lineDrawer.ToggleSoldier();
            }
            if (Input.GetKeyDown("e"))
            {
                m_lineDrawer.ToggleMages();
            }
            if (Input.GetKeyDown("r"))
            {
                m_lineDrawer.ToggleAll();
            }
            if (Input.GetKeyDown("a"))
            {
                if (m_graphDrawer.Graph == m_archerDcel)
                {
                    m_graphDrawer.Graph = null;
                }
                else {
                    m_graphDrawer.Graph = m_archerDcel;
                }
            }
            if (Input.GetKeyDown("s"))
            {
                if (m_graphDrawer.Graph == m_spearmenDcel)
                {
                    m_graphDrawer.Graph = null;
                }
                else {
                    m_graphDrawer.Graph = m_spearmenDcel;
                }
            }
            if (Input.GetKeyDown("d"))
            {
                if (m_graphDrawer.Graph == m_mageDcel)
                {
                    m_graphDrawer.Graph = null;
                }
                else {
                    m_graphDrawer.Graph = m_mageDcel;
                }
            }
            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                var newscale = m_graphDrawer.transform.localScale;
                newscale.Scale(new Vector3(1/1.2f, 1/1.2f, 1));
                m_graphDrawer.transform.localScale = newscale;
            }
            if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus)|| Input.GetKeyDown(KeyCode.Equals))
            {
                var newscale = m_graphDrawer.transform.localScale;
                newscale.Scale(new Vector3(1.2f,  1.2f, 1));
                m_graphDrawer.transform.localScale = newscale;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Keypad4))
            {
                var pos = m_graphDrawer.transform.localPosition;
                pos.Set(pos.x - 0.5f, pos.y, pos.z);
                m_graphDrawer.transform.localPosition = pos;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Keypad6))
            {
                var pos = m_graphDrawer.transform.localPosition;
                pos.Set(pos.x + 0.5f, pos.y, pos.z);
                m_graphDrawer.transform.localPosition = pos;
            }
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Keypad8))
            {
                var pos = m_graphDrawer.transform.localPosition;
                pos.Set(pos.x , pos.y+ 0.5f, pos.z);
                m_graphDrawer.transform.localPosition = pos;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                var pos = m_graphDrawer.transform.localPosition;
                pos.Set(pos.x , pos.y -0.5f, pos.z);
                m_graphDrawer.transform.localPosition = pos;
            }
        }

        internal void SoldierClick(DivideSoldier soldier)
        {
            if (selectedSoldier == soldier){
                soldier.Deselect();
                selectedSoldier = null;
            }
            else if (selectedSoldier != null)
            {
                Swap(selectedSoldier, soldier);
                selectedSoldier = null;
            }
            else
            {
                selectedSoldier = soldier;
            }
        }

        private void Swap(DivideSoldier soldier1, DivideSoldier soldier2)
        {
            var pos = soldier1.transform.position;
            soldier1.transform.position = soldier2.transform.position;
            soldier2.transform.position = pos;

            soldier1.Deselect();
            soldier2.Deselect();

            m_numberOfSwaps--;

            UpdateSwapText();

            UpdateArmyPos();
            FindSolution();
        }

        /// <summary>
        /// Finds a number of solutions for the cut problem. Both per type of soldier and for all soldiers.
        /// 
        /// NOTE: only works if the x coords of all things are all positive or all negative
        /// </summary>
        private void FindSolution()
        {
            var archerlines = PointLineDual.Dual(m_archersPos);
            var swordsmenlines = PointLineDual.Dual(m_spearmenPos);
            var magelines = PointLineDual.Dual(m_magesPos);

            //var allLines = archerlines.Concat(swordsmenlines.Concat(magelines)).ToList();

            m_archerDcel = new DCEL(archerlines);
            m_spearmenDcel = new DCEL(swordsmenlines);
            m_mageDcel = new DCEL(magelines);

            var archerFaces = m_archerDcel.InnerFaces;
            var swordsmenFaces = m_spearmenDcel.InnerFaces;
            var mageFaces = m_mageDcel.InnerFaces;

            m_solution = new DivideSolution(HamSandwich.FindCutlines(archerFaces),
                HamSandwich.FindCutlines(swordsmenFaces),
                HamSandwich.FindCutlines(mageFaces),
                HamSandwich.FindCutlines(archerFaces, swordsmenFaces, mageFaces));

            m_lineDrawer.NewSolution(m_solution);
        }

        private void UpdateSwapText()
        {
            m_swapText.text = "Swaps:  " + m_numberOfSwaps;
        }
    }
}