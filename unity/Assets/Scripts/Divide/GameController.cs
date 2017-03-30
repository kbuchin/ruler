using UnityEngine;
using System.Collections.Generic;
using Algo;
using Algo.DCEL;
using Algo.Polygons;
using System.Linq;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Divide { 

    public class GameController : MonoBehaviour {
        public string m_nextlevel = "lv1";
        public int m_numberofswaps = 0;

        private List<Thing> m_selectedList;
        private Solution m_solution;
        private bool m_levelSolved;

        private GameObject[] m_archers;
        private GameObject[] m_swordsmen;
        private GameObject[] m_mages;
        private DCEL m_archerDcel;
        private DCEL m_swordsmenDcel;
        private DCEL m_mageDcel;
        private List<Vector2> m_archerspos;
        private List<Vector2> m_swordsmenpos;
        private List<Vector2> m_magespos;

        //Unity references
        private DivideLineDrawer m_lineDrawer;
        private GraphDrawer m_graphDrawer;
        private MouseLine m_mouseLine;
        private SpriteRenderer m_spriteRenderer;
        private BoxCollider2D m_colider;
        private Sprite m_sprGreatCut;
        private Sprite m_sprGoodCut;
        private Sprite m_sprTooBad;
        private Text m_swapText;
        private bool m_restartLevel;

        void Awake () {
            m_selectedList = new List<Thing>();
            m_archers = GameObject.FindGameObjectsWithTag(Tags.Archer);
            m_swordsmen= GameObject.FindGameObjectsWithTag(Tags.Swordsman);
            m_mages = GameObject.FindGameObjectsWithTag(Tags.Mage);
            m_mouseLine = FindObjectOfType<MouseLine>();
            m_lineDrawer = GameObject.FindObjectOfType<DivideLineDrawer>();
            m_graphDrawer = GameObject.FindObjectOfType<GraphDrawer>();
            m_spriteRenderer = GetComponent<SpriteRenderer>();
            m_colider = GetComponent<BoxCollider2D>();
            m_swapText = GameObject.FindGameObjectWithTag(Tags.Text).GetComponent<Text>();
            m_swapText.text = "Swaps:  " + m_numberofswaps;
        }

        void Start()
        {
            m_sprGreatCut = Resources.Load<Sprite>("greatCut");
            m_sprTooBad = Resources.Load<Sprite>("tooBad");
            m_sprGoodCut = Resources.Load<Sprite>("goodCut");
            
            UpdateArmyPos();
            FindSolution();
        }

        private void UpdateArmyPos()
        {
            //Find position of different soldiers
            m_archerspos = new List<Vector2>();
            m_swordsmenpos = new List<Vector2>();
            m_magespos = new List<Vector2>();
            foreach (GameObject archer in m_archers)
            {
                m_archerspos.Add(archer.transform.position);  //Automaticly casts to Vector2
            }
            foreach (GameObject swordsman in m_swordsmen)
            {
                m_swordsmenpos.Add(swordsman.transform.position);  //Automaticly casts to Vector2
            }
            foreach (GameObject mage in m_mages)
            {
                m_magespos.Add(mage.transform.position);  //Automaticly casts to Vector2
            }
        }

        internal void processSolution(Line line)
        {
            //TODO some procesing + victory screen
            if (line.NumberOfPointsAbove(m_magespos) == m_magespos.Count/2 
                && line.NumberOfPointsAbove(m_archerspos) == m_archerspos.Count/2
                && line.NumberOfPointsAbove(m_swordsmenpos) == m_swordsmenpos.Count/2)
            {
                if (m_numberofswaps >= 0)
                {
                    m_spriteRenderer.sprite = m_sprGreatCut;
                    m_levelSolved = true;
                }
                else
                {
                    m_spriteRenderer.sprite = m_sprGoodCut;
                    m_restartLevel = true;
                }
            }
            else
            {
                m_spriteRenderer.sprite = m_sprTooBad;
            }
            //go to next leveltra
            m_spriteRenderer.enabled = true;
            m_colider.enabled = true;
        }

        void OnMouseUpAsButton()
        {
            if (m_levelSolved)
            {
                SceneManager.LoadScene(m_nextlevel);
            }
            if (m_restartLevel)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            m_colider.enabled = false;
            m_spriteRenderer.enabled = false;
            m_mouseLine.DisableLine();
            
        }

        void Update()
        {
            //handle input
            if (Input.GetKeyDown("q"))
            {
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
                if (m_graphDrawer.Graph == m_swordsmenDcel)
                {
                    m_graphDrawer.Graph = null;
                }
                else {
                    m_graphDrawer.Graph = m_swordsmenDcel;
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

        internal void thingClick(Thing thing, bool isSelected)
        {
            if (isSelected){
                m_selectedList.Add(thing);
            } else
            {
                m_selectedList.Remove(thing);
            }

            if (m_selectedList.Count == 2)
            {
                Swap(m_selectedList[0], m_selectedList[1]);
                m_selectedList.Clear();
            }
        }

        private void Swap(Thing thing1, Thing thing2)
        {
            var pos = thing1.transform.position;
            thing1.transform.position = thing2.transform.position;
            thing2.transform.position = pos;

            thing1.deselect();
            thing2.deselect();

            m_numberofswaps--;
            m_swapText.text = "Swaps:  " + m_numberofswaps;

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
            List<Line> archerlines = GeomDual.Dual(m_archerspos);
            List<Line> swordsmenlines = GeomDual.Dual(m_swordsmenpos);
            List<Line> magelines = GeomDual.Dual(m_magespos);

            List<Line> allLines = archerlines.Concat(swordsmenlines.Concat(magelines)).ToList<Line>();

            Rect bBox = BoundingBoxComputer.FromLines(allLines, 10, 10);
            m_archerDcel = new DCEL(archerlines, bBox);
            m_swordsmenDcel = new DCEL(swordsmenlines, bBox.Enlarge(1f));
            m_mageDcel = new DCEL(magelines, bBox.Enlarge(2f));

            List<Face> archerFeas = m_archerDcel.middleFaces();
            List<Face> swordsmenFeas = m_swordsmenDcel.middleFaces();
            List<Face> mageFeas = m_mageDcel.middleFaces();

            m_solution = new Solution(findCutlines(archerFeas), 
                findCutlines(swordsmenFeas), 
                findCutlines(mageFeas), 
                findPointsInRegions(archerFeas, swordsmenFeas, mageFeas));
            m_lineDrawer.NewSolution(m_solution);
        }

        private List<Line> findCutlines(List<Face> a_region)
        {
            return findCutlines(a_region.Select(f => f.Polygon));
        }

        private List<Line> findCutlines(IEnumerable<VertexSimplePolygon> a_region)
        {
            if(a_region.Count() <=0) //no valied faces are supplied
            {
                return new List<Line>();
            }
           //facebased approach
            var lines = new List<Line>();
            foreach (VertexSimplePolygon poly in a_region.Skip(1).Take(a_region.Count() - 2)) //Treat faces on the bounding box seperatly
            {
                var line = poly.LineOfGreatestMinimumSeperationInTheDual(false).Line;
                if (line == null)
                {
                    throw new AlgoException();
                }
                lines.Add(line);
            }

            //Solve boundingbox cases (Take only the line with the greatest seperation
            var firstBoundingboxPoly = a_region.ElementAt(0);
            var lastBoundingboxPoly = a_region.ElementAt(a_region.Count() - 1);
            var firstTuple = firstBoundingboxPoly.LineOfGreatestMinimumSeperationInTheDual(true);
            var lastTuple = lastBoundingboxPoly.LineOfGreatestMinimumSeperationInTheDual(true);
            if(firstTuple.Seperation > lastTuple.Seperation)
            {
                lines.Add(firstTuple.Line);
            }
            else
            {
                lines.Add(lastTuple.Line);
            }
            return lines;
        }

        private List<Line> findPointsInRegions(List<Face> a_region1, List<Face> a_region2, List<Face> a_region3)
        {
            //Assume each list of faces has an strict y-order (i.e. each aface is above the other)
            a_region1.Sort((f1, f2) => f1.BoundingBox().yMin.CompareTo(f2.BoundingBox().yMin));
            a_region2.Sort((f1, f2) => f1.BoundingBox().yMin.CompareTo(f2.BoundingBox().yMin));
            a_region3.Sort((f1, f2) => f1.BoundingBox().yMin.CompareTo(f2.BoundingBox().yMin));

            //assert this 
            for (int i = 0; i < a_region1.Count -1; i++)
            {
                if (! MathUtil.EqualsEps(a_region1[i].BoundingBox().yMax , a_region1[i + 1].BoundingBox().yMin))
                {
                    throw new AlgoException("List has no unique y-order");
                }
            }
            for (int i = 0; i < a_region2.Count - 1; i++)
            {
                if (!MathUtil.EqualsEps(a_region2[i].BoundingBox().yMax, a_region2[i + 1].BoundingBox().yMin))
                {
                    throw new AlgoException("List has no unique y-order " + a_region2[i].BoundingBox().yMax + " "+ a_region2[i + 1].BoundingBox().yMin);
                }
            }
            for (int i = 0; i < a_region3.Count - 1; i++)
            {
                if (!MathUtil.EqualsEps(a_region3[i].BoundingBox().yMax, a_region3[i + 1].BoundingBox().yMin))
                {
                    throw new AlgoException("List has no unique y-order" + a_region3[i].BoundingBox().yMax + " " + a_region3[i + 1].BoundingBox().yMin);
                }
            }

            List<VertexSimplePolygon> region1 = a_region1.Select(x => x.Polygon).ToList();
            var region2 = a_region2.Select(x => x.Polygon).ToList();
            var region3 = a_region3.Select(x => x.Polygon).ToList();


            var intermediateList = new List<VertexSimplePolygon>();
            //Intersect first two lists
            var list1index = 0;
            var list2index = 0;

            while (true)
            {
                //progress trough y coordinates
                var intersection = VertexSimplePolygon.IntersectConvex(region1[list1index], region2[list2index]);
                if(intersection != null) {
                    intermediateList.Add(intersection);
                }
                
                if (region2[list2index].BoundingBox.yMax < region1[list1index].BoundingBox.yMax)
                {
                    list2index++;
                }
                else
                {
                    list1index++;
                }

                if (list1index>= region1.Count || list2index >= region2.Count)
                {
                    break;
                }
            }

            List<VertexSimplePolygon> result = new List<VertexSimplePolygon>();
            //Intersect intermediate list and last list
            var intermediateIndex = 0;
            var list3index = 0;
            while (true)
            {
                //progress trough y coordinates
                var intersection = VertexSimplePolygon.IntersectConvex(intermediateList[intermediateIndex], region3[list3index]);
                if (intersection != null)
                {
                    result.Add(intersection);
                }

                if (region3[list3index].BoundingBox.yMax < intermediateList[intermediateIndex].BoundingBox.yMax)
                {
                    list3index++;
                }
                else
                {
                    intermediateIndex++;
                }

                if (intermediateIndex >= intermediateList.Count || list3index >= region3.Count)
                {
                    break;
                }
            }

            //Convert polygons to lines
            return findCutlines(result);
        }
    }
}