using System;
using System.Collections.Generic;
using System.Linq;
using General.Controller;
using General.Menu;
using General.Model;
using UnityEngine;
using UnityEngine.UI;
using Util.Algorithms.Triangulation;
using Util.Geometry;
using Util.Geometry.Polygon;

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
        [SerializeField] public bool AiEnabled;
        [SerializeField] public bool p1Ai;

        protected int numberOfDots = 20;
        private float minX = -8.0f;
        private float maxX = 8.0f;
        private float minY = -3.5f;
        private float maxY = 3.5f;

        private HashSet<UnityTrapDecomLine> TrapDecomLines { get; set; } = new HashSet<UnityTrapDecomLine>();
        protected TrapDecomRoot root;

        public TrapFace frame;

        protected List<TrapFace> faces;
        protected List<GameObject> lines = new List<GameObject>();

        public UnityDotsVertex FirstPoint { get; set; }
        public UnityDotsVertex SecondPoint { get; set; }


        public HashSet<IDotsVertex> Vertices { get; set; } = new HashSet<IDotsVertex>();
        protected HashSet<IDotsHalfEdge> HalfEdges { get; set; } = new HashSet<IDotsHalfEdge>();
        protected HashSet<IDotsEdge> Edges { get; set; } = new HashSet<IDotsEdge>();
        public HashSet<IDotsFace> Faces { get; set; } = new HashSet<IDotsFace>();
        public int CurrentPlayer { get; set; } = 1;

        protected DotsPlayer player1 { get; set; } 
        protected DotsPlayer player2 { get; set; } 

        protected float TotalAreaP1
        {
            get => player1.TotalArea;
            set => player1.TotalArea = value;
        }

        protected float TotalAreaP2
        {
            get => player2.TotalArea;
            set => player2.TotalArea = value;
        }

        public List<GameObject> InstantObjects { get; private set; } = new List<GameObject>();

        public HashSet<LineSegment> Hull { get; set; }
        public float HullArea { get; set; }

        protected bool _showTrapDecomLines = false;

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
            // Assign players
            // todo fix game mode parameter
            player1 = new DotsPlayer(PlayerNumber.Player1, Settings.Player1, GameMode.GameMode1);
            player2 = new DotsPlayer(PlayerNumber.Player2, Settings.Player2, GameMode.GameMode1);
            
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

        protected void ShowTrapDecomLines()
        {
            if (!_showTrapDecomLines) return;
            faces = root.FindAllFaces();
            
            foreach (TrapFace face in faces)
            {
                GameObject upper = UnityTrapDecomLine.CreateUnityTrapDecomLine(face.Upper.Segment, this);
                if (upper != null)
                    lines.Add(upper);

                GameObject downer = UnityTrapDecomLine.CreateUnityTrapDecomLine(face.Downer.Segment, this);
                if (downer != null)
                    lines.Add(downer);

                GameObject left = UnityTrapDecomLine.CreateUnityTrapDecomLine(face.Left, this);
                if (left != null)
                    lines.Add(left);

                GameObject right = UnityTrapDecomLine.CreateUnityTrapDecomLine(face.Right, this);
                if (right != null)
                    lines.Add(right);
            }
        }

        protected void RemoveTrapDecomLines()
        {
            foreach (GameObject line in lines)
            {
                Destroy(line);
            }

            lines.Clear();
        }

        public void AddDotsInGeneralPosition()
        {
            // Take the best out of 2
            var bestDots = new HashSet<Vector2>();
            for (var i = 0; i < 2; i++)
            {
                var dotsPlacer = new DotsPlacer(new Rect(minX, minY, maxX - minX, maxY - minY));
                dotsPlacer.AddNewPoints(numberOfDots);
                if (dotsPlacer.Dots.Count > bestDots.Count) bestDots = dotsPlacer.Dots;
            }

            print($"Number of placed dots: {bestDots.Count}");

            foreach (Vector2 dot in bestDots)
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

            root = new TrapDecomRoot(frame);
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
                TotalAreaP1 < TotalAreaP2 ? Color.blue : Color.red;

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