namespace Divide.Controller
{
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using Util.Geometry.DCEL;
    using Util.Geometry.Duality;
    using Drawing;
    using Divide.Model;
    using Divide.UI;
    using General.Menu;
    using Util.Algorithms.DCEL;
    using General.Controller;
    using Util.Math;

    /// <summary>
    /// Main controller for the divide game.
    /// Handles the game update loop, as well as level initialization and advancement.
    /// </summary>
    public class DivideController : MonoBehaviour, IController
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

        // holds current level index
        private int m_levelCounter = 0;

        // hold number of swaps still allowed
        private int m_numberOfSwaps;

        private DivideSoldier selectedSoldier;
        private DivideSolution m_solution;

        // holds the game objects for the soldiers
        private List<GameObject> m_spearmen = new List<GameObject>();
        private List<GameObject> m_archers = new List<GameObject>();
        private List<GameObject> m_mages = new List<GameObject>();

        // dcel for the dual lines related to soldiers
        private DCEL m_archerDcel, m_spearmenDcel,  m_mageDcel;

        //Unity references
        private DivideLineDrawer m_lineDrawer;
        private DCELDrawer m_graphDrawer;
        private DivideLine m_mouseLine;

        private bool m_levelSolved;
        private bool m_restartLevel;

        // Use this for initialization
        void Start()
        {
            m_mouseLine = FindObjectOfType<DivideLine>();
            m_lineDrawer = FindObjectOfType<DivideLineDrawer>();
            m_graphDrawer = FindObjectOfType<DCELDrawer>();

            m_victoryOverlay.Callback = AdvanceLevel;

            InitLevel();
        }

        // Update is called once per frame
        void Update()
        {
            //handle input key presses
            if (Input.GetKeyDown("q"))
            {
                m_lineDrawer.ToggleArchers();
            }
            if (Input.GetKeyDown("w"))
            {
                m_lineDrawer.ToggleSpearmen();
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
                // toggle archer dcel 
                m_graphDrawer.Graph = m_graphDrawer.Graph == m_archerDcel ? null : m_archerDcel;
            }
            if (Input.GetKeyDown("s"))
            {
                // toggle spearmen dcel
                m_graphDrawer.Graph = m_graphDrawer.Graph == m_spearmenDcel ? null : m_spearmenDcel;
            }
            if (Input.GetKeyDown("d"))
            {
                // toggle mage dcel
                m_graphDrawer.Graph = m_graphDrawer.Graph == m_mageDcel ? null : m_mageDcel;
            }
            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                // scale graph drawer down
                var newscale = m_graphDrawer.transform.localScale;
                newscale.Scale(new Vector3(1 / 1.2f, 1 / 1.2f, 1));
                m_graphDrawer.transform.localScale = newscale;
            }
            if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.Equals))
            {
                // scale graph drawer up
                var newscale = m_graphDrawer.transform.localScale;
                newscale.Scale(new Vector3(1.2f, 1.2f, 1));
                m_graphDrawer.transform.localScale = newscale;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Keypad4))
            {
                // move graph drawer to left
                m_graphDrawer.transform.localPosition += new Vector3(-0.5f, 0f, 0f); ;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Keypad6))
            {
                // move graph drawer to right
                m_graphDrawer.transform.localPosition += new Vector3(0.5f, 0f, 0f); ;
            }
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Keypad8))
            {
                // move graph drawer up
                m_graphDrawer.transform.localPosition += new Vector3(0f, 0.5f, 0f); ;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                // move graph drawer down
                m_graphDrawer.transform.localPosition += new Vector3(0f, -0.5f, 0f);
            }
        }

        public void InitLevel()
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
            FindSolution();
        }

        public void CheckSolution()
        {
            // obtain line and point data of game objects
            var line = m_mouseLine.Line;
            var archersPos = m_mages.Select(x => (Vector2)x.transform.position);
            var spearmenPos = m_mages.Select(x => (Vector2)x.transform.position);
            var magesPos = m_mages.Select(x => (Vector2)x.transform.position);

            if (line.NumberOfPointsAbove(archersPos) == m_archers.Count / 2
                && line.NumberOfPointsAbove(spearmenPos) == m_spearmen.Count / 2
                && line.NumberOfPointsAbove(magesPos) == m_mages.Count / 2)
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

        public void AdvanceLevel()
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

        /// <summary>
        /// Handles a user click on the given soldier.
        /// Performs a swap if appropriate.
        /// </summary>
        /// <param name="soldier"></param>
        internal void HandleSoldierClick(DivideSoldier soldier)
        {
            if (selectedSoldier == null)
            {
                // select the clicked soldier 
                selectedSoldier = soldier;
            }
            else if (selectedSoldier == soldier)
            {
                // deselect soldier that was clicked on twice
                soldier.Deselect();
                selectedSoldier = null;
            }
            else
            {
                // swap the previous selected soldier with current clicked soldier
                Swap(selectedSoldier, soldier);
                selectedSoldier = null;
            }
        }

        /// <summary>
        /// Perform a swap of two divide soldiers. 
        /// </summary>
        /// <param name="soldier1"></param>
        /// <param name="soldier2"></param>
        private void Swap(DivideSoldier soldier1, DivideSoldier soldier2)
        {
            // swap positions of soldiers
            var pos = soldier1.transform.position;
            soldier1.transform.position = soldier2.transform.position;
            soldier2.transform.position = pos;

            // deselect the soldiers in the swap
            soldier1.Deselect();
            soldier2.Deselect();

            // decrease number of swaps
            m_numberOfSwaps--;
            UpdateSwapText();

            // update solutions shown in drawer
            FindSolution();
        }

        /// <summary>
        /// Finds a number of solutions for the cut problem. Both per type of soldier and for all soldiers.
        /// 
        /// NOTE: only works if the x coords of all things are all positive or all negative
        /// </summary>
        public void FindSolution()
        {
            // obtain dual lines for game objects
            var archerlines = PointLineDual.Dual(m_archers.Select(x => (Vector2)x.transform.position));
            var swordsmenlines = PointLineDual.Dual(m_spearmen.Select(x => (Vector2)x.transform.position));
            var magelines = PointLineDual.Dual(m_mages.Select(x => (Vector2)x.transform.position));

            // add lines together
            var allLines = archerlines.Concat(swordsmenlines.Concat(magelines));

            // calculate bounding box around line intersections with some margin
            var bBox = BoundingBoxComputer.FromLines(allLines, 10f);

            // calculate dcel for line inside given bounding box
            m_archerDcel = new DCEL(archerlines, bBox);
            m_spearmenDcel = new DCEL(swordsmenlines, bBox);
            m_mageDcel = new DCEL(magelines, bBox);

            // find faces in the middle of the lines vertically
            var archerFaces = HamSandwich.MiddleFaces(m_archerDcel);
            var swordsmenFaces = HamSandwich.MiddleFaces(m_spearmenDcel);
            var mageFaces = HamSandwich.MiddleFaces(m_mageDcel);

            // obtain cut lines for the dcel middle faces and final possible cutlines
            m_solution = new DivideSolution(HamSandwich.FindCutlinesInDual(archerFaces),
                HamSandwich.FindCutlinesInDual(swordsmenFaces),
                HamSandwich.FindCutlinesInDual(mageFaces),
                HamSandwich.FindCutlinesInDual(archerFaces, swordsmenFaces, mageFaces));

            // update solution to the drawer
            m_lineDrawer.Solution = m_solution;
        }

        /// <summary>
        /// Set the text field with the maximum number of allowed swaps.
        /// </summary>
        private void UpdateSwapText()
        {
            m_swapText.text = "Swaps:  " + m_numberOfSwaps;
        }
    }
}