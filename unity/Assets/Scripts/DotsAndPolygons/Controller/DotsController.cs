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
    using System.Collections;
    using System.IO;
    using System.Threading;
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

        protected int numberOfDots = 20; // TODO
        private float minX = -8.0f;
        private float maxX = 8.0f;
        private float minY = -3.5f;
        private float maxY = 3.5f;

        private HashSet<UnityTrapDecomLine> TrapDecomLines { get; set; } = new HashSet<UnityTrapDecomLine>();
        protected TrapDecomRoot root;

        public TrapFace frame;

        protected List<TrapFace> faces;
        protected List<GameObject> lines = new List<GameObject>();

        private List<PotentialMove>[] paths = new List<PotentialMove>[2]
        {
            new List<PotentialMove>(),
            new List<PotentialMove>()
        };

        public UnityDotsVertex FirstPoint { get; set; }
        public UnityDotsVertex SecondPoint { get; set; }


        public HashSet<UnityDotsVertex> Vertices { get; set; } = new HashSet<UnityDotsVertex>();
        protected HashSet<DotsHalfEdge> HalfEdges { get; set; } = new HashSet<DotsHalfEdge>();
        protected HashSet<UnityDotsEdge> Edges { get; set; } = new HashSet<UnityDotsEdge>();
        public HashSet<UnityDotsFace> Faces { get; set; } = new HashSet<UnityDotsFace>();
        public DotsPlayer CurrentPlayer { get; set; }
        public DotsPlayer Player1 { get; set; }
        public DotsPlayer Player2 { get; set; }

        public int CurrentPlayerValue => CurrentPlayer.Equals(Player1) ? 1 : 2;

        public abstract GameMode CurrentGamemode { get; }

        protected float TotalAreaP1
        {
            get => Player1.TotalArea;
            set => Player1.TotalArea = value;
        }

        protected float TotalAreaP2
        {
            get => Player2.TotalArea;
            set => Player2.TotalArea = value;
        }

        public List<GameObject> InstantObjects { get; private set; } = new List<GameObject>();

        public HashSet<LineSegment> Hull { get; set; }
        public float HullArea { get; set; }

        protected bool _showTrapDecomLines = false;

        public void SwitchPlayer()
        {
            CurrentPlayer = CurrentPlayer.Equals(Player1) ? Player2 : Player1;
            currentPlayerText.text = $"Go Player {CurrentPlayerValue}";
            currentPlayerText.gameObject.GetComponentInParent<Image>().color =
                CurrentPlayer == Player2 ? Color.blue : Color.red;

            if (CurrentPlayer.PlayerType != PlayerType.Player)
            {
                MoveForAiPlayer();
            }
        }

        protected void UpdatePath(DotsVertex a, DotsVertex b)
        {
            int nextPlayer = Convert.ToInt32(CurrentPlayer.PlayerNumber.Switch()) - 1;
            PotentialMove move = paths[nextPlayer].LastOrDefault();
            DotsVertex A = move?.A.Original ?? move?.A;
            DotsVertex B = move?.B.Original ?? move?.B;
            if (move != null && ((A.Equals(a) && B.Equals(b)) || (B.Equals(b) && A.Equals(a)))) {  
                paths[nextPlayer].Remove(move);
            }
        }

        private void MoveAiPlayerForThread()
        {
            int index = Convert.ToInt32(CurrentPlayer.PlayerNumber) - 1;
            List<PotentialMove> moves = (CurrentPlayer as AiPlayer).NextMove(
                Edges.Select(x => x.DotsEdge).ToHashSet(),
                HalfEdges,
                Faces.Select(x => x.DotsFace).ToHashSet(),
                Vertices.Select(x => x.dotsVertex).ToHashSet()
            );
            DotsVertex a = moves.Last().A.Original ?? moves.Last().A;
            DotsVertex b = moves.Last().B.Original ?? moves.Last().B;
            moves.Remove(moves.Last());
            paths[index] = moves;


            UnityMainThreadDispatcher.Instance().Enqueue(RunPostUpdate(DoMove, a, b));
        }

        private IEnumerator RunPostUpdate(Action<DotsVertex, DotsVertex> method, DotsVertex a, DotsVertex b)
        {
            // If RunOnMainThread() is called in a secondary thread,
            // this coroutine will start on the secondary thread
            // then yield until the end of the frame on the main thread
            yield return null;

            method(a, b);
        }

        public void MoveForAiPlayer()
        {
            if (CurrentPlayer.PlayerType == PlayerType.Player) return;
            int index = Convert.ToInt32(CurrentPlayer.PlayerNumber) - 1;
            List<PotentialMove> currentPath = paths[index];
            if (currentPath.Any() && currentPath.Last().PlayerNumber == CurrentPlayer.PlayerNumber)
            {
                DotsVertex a = currentPath.Last().A.Original ?? currentPath.Last().A;
                DotsVertex b = currentPath.Last().B.Original ?? currentPath.Last().B;
                currentPath.Remove(currentPath.Last());
                DoMove(a, b);
            }
            else
            {
                Thread instanceCaller = new Thread(new ThreadStart(MoveAiPlayerForThread));

                // Start the thread.
                instanceCaller.Start();
            }
        }

        public void DoMove(DotsVertex firstPoint, DotsVertex secondPoint)
        {
            AddVisualEdge(firstPoint, secondPoint);

            (DotsFace face1, DotsFace face2) = AddEdge(
                firstPoint,
                secondPoint,
                CurrentPlayerValue,
                HalfEdges,
                Vertices.Select(x => x.dotsVertex),
                CurrentGamemode,
                this,
                root
            );

            RemoveTrapDecomLines();
            ShowTrapDecomLines();
            UpdatePath(firstPoint, secondPoint);
            bool finished = CheckSolutionOfGameState();
            if (!finished && face1 == null && face2 == null)
            {
                SwitchPlayer();
            }
            else if (!finished && CurrentPlayer.PlayerType != PlayerType.Player)
            {
                MoveForAiPlayer();
            }
        }

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
            Player1 = Settings.Player1.CreatePlayer(PlayerNumber.Player1, CurrentGamemode);
            Player2 = Settings.Player2.CreatePlayer(PlayerNumber.Player2, CurrentGamemode);

            CurrentPlayer = Player1;

            HelperFunctions.print(
                $"Starting game with Player1 as {Player1.PlayerType} and Player2 as {Player2.PlayerType}");

            // get unity objects
            Vertices = new HashSet<UnityDotsVertex>();
            HalfEdges = new HashSet<DotsHalfEdge>();
            Edges = new HashSet<UnityDotsEdge>();
            Faces = new HashSet<UnityDotsFace>();
            // disable advance button
            advanceButton.Disable();

            InstantObjects = new List<GameObject>();
            InitLevel();


            Hull = ConvexHullHelper.ComputeHull(Vertices.Select(x => x.dotsVertex).ToList());
            HullArea = Triangulator.Triangulate(
                new Polygon2D(Hull.Select(it => it.Point1))
            ).Area;
            if (Player1.PlayerType == PlayerType.MinMaxAi)
            {
                ((MinMaxAi) Player1).TotalHullArea = HullArea;
                ((MinMaxAi)Player1).InitPairs(Vertices.Count);
            }

            if (Player2.PlayerType == PlayerType.MinMaxAi)
            {
                ((MinMaxAi) Player2).TotalHullArea = HullArea;
                ((MinMaxAi)Player2).InitPairs(Vertices.Count);
            }

            UpdateVisualArea();
            if (Player1.PlayerType != PlayerType.Player)
            {
                MoveForAiPlayer();
            }
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
            HashSet<Vector2> bestDots = new HashSet<Vector2>();
            for (int i = 0; i < 2; i++)
            {
                DotsPlacer dotsPlacer = new DotsPlacer(new Rect(minX, minY, maxX - minX, maxY - minY));
                dotsPlacer.AddNewPoints(numberOfDots);
                if (dotsPlacer.Dots.Count > bestDots.Count) bestDots = dotsPlacer.Dots;
            }

            HelperFunctions.print($"Number of placed dots: {bestDots.Count}");

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
            if (CurrentPlayer.Equals(Player1))
                p1Line.enabled = true;
            else
                p2Line.enabled = true;
        }

        public void SetDrawingLinePosition(int index, Vector2 position)
        {
            if (CurrentPlayer.Equals(Player1))
                p1Line.SetPosition(index, position);
            else
                p2Line.SetPosition(index, position);
        }

        public void AddVisualEdge(DotsVertex a_point1, DotsVertex a_point2)
        {
            var segment = new LineSegment(a_point1.Coordinates, a_point2.Coordinates);

            GameObject edgeMesh = Instantiate(
                CurrentPlayer.Equals(Player1) ? p1EdgeMeshPrefab : p2EdgeMeshPrefab,
                Vector3.forward,
                Quaternion.identity);
            edgeMesh.transform.parent = transform;
            InstantObjects.Add(edgeMesh);

            var edge = edgeMesh.GetComponent<UnityDotsEdge>();
            edge.DotsEdge.Segment = segment;
            Edges.Add(edge);

            var edgeMeshScript = edgeMesh.GetComponent<ReshapingMesh>();
            edgeMeshScript.CreateNewMesh(a_point1.Coordinates, a_point2.Coordinates);
        }

        public void CheckSolution()
        {
            // C# is awesome
        }

        // Enable advance button if "solution" is correct
        public abstract bool CheckSolutionOfGameState();

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
            foreach (UnityDotsVertex unityDotsVertex in Vertices)
            {
                unityDotsVertex.dotsVertex.InFace = true;
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
            HelperFunctions.print($"Current hull: {Hull}");
            HelperFunctions.print($"Current edges: {Edges}");
            IEnumerable<UnityDotsEdge> hullEdges = Edges.Where(edge => Hull.Any(hullEdge =>
                hullEdge.Point1.Equals(edge.DotsEdge.Segment.Point2) &&
                hullEdge.Point2.Equals(edge.DotsEdge.Segment.Point1)
                || hullEdge.Equals(edge.DotsEdge.Segment)));
            return Hull.Count == hullEdges.Count();
        }

        public static List<TrapFace> ExtractFaces(ITrapDecomNode current, List<TrapFace> result, int depth)
        {
            if (depth > 1000)
            {
                return new List<TrapFace>();
            }

            HelperFunctions.print(current.GetType());
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