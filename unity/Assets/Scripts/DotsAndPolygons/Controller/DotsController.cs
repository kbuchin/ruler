using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using ClipperLib;
using General.Controller;
using General.Menu;
using General.Model;
using UnityEngine;
using UnityEngine.UI;
using Util.Algorithms.Triangulation;
using Util.Geometry;
using Util.Geometry.Polygon;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace DotsAndPolygons
{
    using static HelperFunctions;

    public abstract class DotsController : MonoBehaviour, IController
    {
        [SerializeField] public GameObject trapDecompEdgeMeshPrefab;
        [SerializeField] public LineRenderer p1Line;
        [SerializeField] public LineRenderer p2Line;
        [SerializeField] public GameObject p1EdgeMeshPrefab;
        [SerializeField] public GameObject p2EdgeMeshPrefab;
        [SerializeField] public GameObject dotPrefab;
        [SerializeField] public ButtonContainer advanceButton;
        [SerializeField] public Material p1FaceMaterial;
        [SerializeField] public Material p2FaceMaterial;
        [SerializeField] public GameObject facePrefab;
        [SerializeField] public Text p1TextArea;
        [SerializeField] public Text p2TextArea;
        [SerializeField] public Text currentPlayerText;
        [SerializeField] public GameObject p1WonBackgroundPrefab;
        [SerializeField] public GameObject p2WonBackgroundPrefab;


        [SerializeField] public int numberOfDots = 20;
        [SerializeField] public float minX = -7f;
        [SerializeField] public float maxX = 7f;
        [SerializeField] public float minY = -3f;
        [SerializeField] public float maxY = 3f;

        public HashSet<UnityTrapDecomLine> TrapDecomLines { get; set; } = new HashSet<UnityTrapDecomLine>();
        public TrapDecomRoot root;

        public TrapFace frame;

        public List<TrapFace> faces = new List<TrapFace>();
        public List<GameObject> lines = new List<GameObject>();

        public UnityDotsVertex FirstPoint { get; set; }
        public UnityDotsVertex SecondPoint { get; set; }


        public HashSet<IDotsVertex> Vertices { get; set; } = new HashSet<IDotsVertex>();
        public HashSet<IDotsHalfEdge> HalfEdges { get; set; } = new HashSet<IDotsHalfEdge>();
        public HashSet<IDotsEdge> Edges { get; set; } = new HashSet<IDotsEdge>();
        public HashSet<IDotsFace> Faces { get; set; } = new HashSet<IDotsFace>();
        public int CurrentPlayer { get; set; } = 1;
        public float TotalAreaP1 { get; set; } = 0;
        public float TotalAreaP2 { get; set; } = 0;
        public List<GameObject> InstantObjects { get; private set; } = new List<GameObject>();

        public HashSet<LineSegment> Hull { get; set; }
        public float HullArea { get; set; }

        public void AddToPlayerArea(int player, float area)
        {
            if (player == 1) TotalAreaP1 += Math.Abs(area);
            else TotalAreaP2 += Math.Abs(area);
            UpdateVisualArea();
        }

        public void UpdateVisualArea()
        {
            p1TextArea.text = $"P1 Area: {Math.Round(value: TotalAreaP1, digits: 4)}";
            p2TextArea.text = $"P2 Area: {Math.Round(value: TotalAreaP2, digits: 4)}";
        }

        // Start is called before the first frame update
        protected void Start()
        {
            frame = new TrapFace(
                new LineSegmentWithDotsEdge(
                    new Vector2(minX, maxY),
                    new Vector2(maxX, maxY),
                    null),
                new LineSegmentWithDotsEdge(
                    new Vector2(minX, minY),
                    new Vector2(maxX, minY),
                    null),
                new LineSegment(
                    new Vector2(minX, minY),
                    new Vector2(minX, maxY)),
                new LineSegment(
                    new Vector2(maxX, minY),
                    new Vector2(maxX, maxY)),
                new Vector2(minX, 0),
                new Vector2(maxX, 0)
            );

            // get unity objects
            Vertices = new HashSet<IDotsVertex>();
            foreach (UnityDotsVertex vertex in FindObjectsOfType<UnityDotsVertex>()) Vertices.Add(vertex);
            HalfEdges = new HashSet<IDotsHalfEdge>();
            Edges = new HashSet<IDotsEdge>();
            Faces = new HashSet<IDotsFace>();
            // disable advance button
            advanceButton.Disable();

            InstantObjects = new List<GameObject>();
            InitLevel();

            Hull = ConvexHullHelper.ComputeHull(Vertices.ToList());
            HullArea = Triangulator.Triangulate(
                new Polygon2D(Hull.Select(it => it.Point1))
            ).Area;

            UpdateVisualArea();
        }

        // Define mouse clicking behavior etc
        public abstract void Update();

        public void AddDotsInGeneralPosition()
        {
            Rect bounds = new Rect(minX, minY, (maxX - maxY), (maxY - minY));
            var dots = DotsPlacer.GeneratePoints(bounds, numberOfDots);
            foreach(Vector2 dot in dots)
            {
                GameObject gameDot = Instantiate(dotPrefab, new Vector3(dot.x, dot.y, 0), Quaternion.identity);
                gameDot.transform.parent = transform;
                InstantObjects.Add(gameDot);
            }
        }

        public virtual void InitLevel()
        {
            // start level using randomly positioned dots in general position
            Clear();

            advanceButton.Disable();
        }

        public void Clear()
        {
            // clear level
            // Jolan
            Vertices.Clear();
            HalfEdges.Clear();
            Faces.Clear();

            TotalAreaP1 = 0f;
            TotalAreaP2 = 0f;
            currentPlayerText.text = "Go Player 1!";

            // destroy game objects created in level
            foreach (GameObject obj in InstantObjects)
            {
                // destroy immediate
                // since controller will search for existing objects afterwards
                DestroyImmediate(obj);
            }
        }

        public void EnableDrawingLine()
        {
            if (CurrentPlayer == 1)
                p1Line.enabled = true;
            else
                p2Line.enabled = true;
        }

        public void SetDrawingLinePosition(int index, Vector2 position)
        {
            if (CurrentPlayer == 1)
                p1Line.SetPosition(index, position);
            else
                p2Line.SetPosition(index, position);
        }

        public void AddVisualEdge(UnityDotsVertex a_point1, UnityDotsVertex a_point2)
        {
            var segment = new LineSegment(a_point1.Coordinates, a_point2.Coordinates);

            GameObject edgeMesh = Instantiate(
                CurrentPlayer == 1 ? p1EdgeMeshPrefab : p2EdgeMeshPrefab,
                Vector3.forward,
                Quaternion.identity);
            edgeMesh.transform.parent = transform;
            InstantObjects.Add(edgeMesh);

            var edge = edgeMesh.GetComponent<UnityDotsEdge>();
            edge.Segment = segment;
            Edges.Add(edge);

            var edgeMeshScript = edgeMesh.GetComponent<ReshapingMesh>();
            edgeMeshScript.CreateNewMesh(FirstPoint.Coordinates, SecondPoint.Coordinates);
        }

        // Enable advance button if "solution" is correct
        public abstract void CheckSolution();

        public void FinishLevel()
        {
            advanceButton.Enable();
            currentPlayerText.text = $"Player {(TotalAreaP1 > TotalAreaP2 ? "1" : "2")} Wins!!";
            currentPlayerText.gameObject.GetComponentInParent<Image>().color =
                TotalAreaP1 > TotalAreaP2 ? Color.blue : Color.red;

            GameObject background =
                Instantiate(TotalAreaP1 > TotalAreaP2 ? p1WonBackgroundPrefab : p2WonBackgroundPrefab);
            InstantObjects.Add(background);

            // disable all vertices
            foreach (IDotsVertex dotsVertex in Vertices)
            {
                dotsVertex.InFace = true;
            }
        }

        public void AdvanceLevel()
        {
            // start a new level
            InitLevel();
        }

        // Check whether all points are contained, aka the outer hull is created, so the game ends
        public bool CheckHull()
        {
            print($"Current hull: {Hull}");
            print($"Current edges: {Edges}");
            IEnumerable<IDotsEdge> hullEdges = Edges.Where(edge => Hull.Any(hullEdge =>
                hullEdge.Point1.Equals(edge.Segment.Point2) && hullEdge.Point2.Equals(edge.Segment.Point1)
                || hullEdge.Equals(edge.Segment)));
            return Hull.Count == hullEdges.Count();
        }

        public static List<TrapFace> ExtractFaces(ITrapDecomNode current, List<TrapFace> result, int depth)
        {
            if (depth > 1000)
            {
                return new List<TrapFace>();
            }

            print(current.GetType());
            if (current.GetType() == typeof(TrapFace))
            {
                return new List<TrapFace> {(TrapFace) current};
            }

            if (current.GetType() == typeof(TrapDecomLine) || current.GetType() == typeof(TrapDecomPoint))
            {
                result.AddRange(ExtractFaces(current.LeftChild, new List<TrapFace>(), depth + 1));
                result.AddRange(ExtractFaces(current.RightChild, new List<TrapFace>(), depth + 1));
            }

            return result;
        }
    }
}