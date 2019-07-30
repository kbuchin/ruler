namespace Divide {
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using Util.Geometry.DCEL;
    using General.Drawing;
    using Util.Geometry;
    using Util.Geometry.Duality;
    using Util.Algorithms;
    using Util.Geometry.Polygon;
    using Util.Algorithms.Polygon;
    using Util.Math;

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
        private DCELDrawer m_graphDrawer;
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
            m_graphDrawer = GameObject.FindObjectOfType<DCELDrawer>();
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

            thing1.Deselect();
            thing2.Deselect();

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
            var archerlines = PointLineDual.Dual(m_archerspos);
            var swordsmenlines = PointLineDual.Dual(m_swordsmenpos);
            var magelines = PointLineDual.Dual(m_magespos);

            var allLines = archerlines.Concat(swordsmenlines.Concat(magelines)).ToList<Line>();

            m_archerDcel = new DCEL(archerlines);
            m_swordsmenDcel = new DCEL(swordsmenlines);
            m_mageDcel = new DCEL(magelines);

            var archerFaces = m_archerDcel.Faces;
            var swordsmenFaces = m_swordsmenDcel.Faces;
            var mageFaces = m_mageDcel.Faces;

            m_solution = new Solution(FindCutlines(archerFaces), 
                FindCutlines(swordsmenFaces), 
                FindCutlines(mageFaces), 
                findPointsInRegions(archerFaces.ToList(), swordsmenFaces.ToList(), mageFaces.ToList()));
            m_lineDrawer.NewSolution(m_solution);
        }

        private List<Line> FindCutlines(ICollection<Face> a_region)
        {
            return FindCutlines(a_region.Select(f => f.Polygon));
        }

        private List<Line> FindCutlines(IEnumerable<Polygon2D> a_region)
        {
            if(a_region.Count() <=0) //no valied faces are supplied
            {
                return new List<Line>();
            }
           //facebased approach
            var lines = new List<Line>();
            foreach (var poly in a_region.Skip(1).Take(a_region.Count() - 2)) //Treat faces on the bounding box seperatly
            {
                var line = Seperator.LineOfGreatestMinimumSeperationInTheDual(poly, false).Line;
                if (line == null)
                {
                    throw new GeomException();
                }
                lines.Add(line);
            }

            //Solve boundingbox cases (Take only the line with the greatest seperation
            var firstBoundingboxPoly = a_region.ElementAt(0);
            var lastBoundingboxPoly = a_region.ElementAt(a_region.Count() - 1);
            var firstTuple = Seperator.LineOfGreatestMinimumSeperationInTheDual(firstBoundingboxPoly, true);
            var lastTuple = Seperator.LineOfGreatestMinimumSeperationInTheDual(lastBoundingboxPoly, true);
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
                    throw new GeomException("List has no unique y-order");
                }
            }
            for (int i = 0; i < a_region2.Count - 1; i++)
            {
                if (!MathUtil.EqualsEps(a_region2[i].BoundingBox().yMax, a_region2[i + 1].BoundingBox().yMin))
                {
                    throw new GeomException("List has no unique y-order " + a_region2[i].BoundingBox().yMax + " "+ a_region2[i + 1].BoundingBox().yMin);
                }
            }
            for (int i = 0; i < a_region3.Count - 1; i++)
            {
                if (!MathUtil.EqualsEps(a_region3[i].BoundingBox().yMax, a_region3[i + 1].BoundingBox().yMin))
                {
                    throw new GeomException("List has no unique y-order" + a_region3[i].BoundingBox().yMax + " " + a_region3[i + 1].BoundingBox().yMin);
                }
            }

            var region1 = a_region1.Select(x => x.Polygon).ToList();
            var region2 = a_region2.Select(x => x.Polygon).ToList();
            var region3 = a_region3.Select(x => x.Polygon).ToList();


            var intermediateList = new List<Polygon2D>();
            //Intersect first two lists
            var list1index = 0;
            var list2index = 0;

            while (true)
            {
                //progress trough y coordinates
                var intersection = Polygon2D.IntersectConvex(region1[list1index], region2[list2index]);
                if(intersection != null) {
                    intermediateList.Add(intersection);
                }
                
                if (BoundingBox.FromPolygon(region2[list2index]).yMax < 
                    BoundingBox.FromPolygon(region1[list1index]).yMax)
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

            var result = new List<Polygon2D>();
            //Intersect intermediate list and last list
            var intermediateIndex = 0;
            var list3index = 0;
            while (true)
            {
                //progress trough y coordinates
                var intersection = Polygon2D.IntersectConvex(intermediateList[intermediateIndex], region3[list3index]);
                if (intersection != null)
                {
                    result.Add(intersection);
                }

                if (BoundingBox.FromPolygon(region3[list3index]).yMax <
                    BoundingBox.FromPolygon(intermediateList[intermediateIndex]).yMax)
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
            return FindCutlines(result);
        }
    }
}