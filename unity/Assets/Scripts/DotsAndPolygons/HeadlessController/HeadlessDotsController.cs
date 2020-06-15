using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util.Algorithms.Triangulation;
using Util.Geometry;
using Util.Geometry.Polygon;

namespace DotsAndPolygons
{
    using System.Collections;
    using System.IO;
    using System.Threading;
    using static HelperFunctions;

    public abstract class HeadlessDotsController
    {
        public HeadlessDotsController(
            DotsPlayer player1,
            DotsPlayer player2,
            int numberOfDots = 20
        )
        {
            if (player1.PlayerType == PlayerType.Player || player2.PlayerType == PlayerType.Player)
            {
                throw new ArgumentException("Only AI players are allowed");
            }

            NumberOfDots = numberOfDots;
            Player1 = player1;
            Player2 = player2;
        }

        protected readonly int NumberOfDots;
        private const float MinX = -8.0f;
        private const float MaxX = 8.0f;
        private const float MinY = -3.5f;
        private const float MaxY = 3.5f;

        private readonly List<PotentialMove>[] paths = new List<PotentialMove>[2]
        {
            new List<PotentialMove>(),
            new List<PotentialMove>()
        };

        public HashSet<DotsVertex> Vertices { get; set; } = new HashSet<DotsVertex>();
        protected HashSet<DotsHalfEdge> HalfEdges { get; set; } = new HashSet<DotsHalfEdge>();
        protected HashSet<DotsEdge> Edges { get; set; } = new HashSet<DotsEdge>();
        public HashSet<DotsFace> Faces { get; set; } = new HashSet<DotsFace>();
        public DotsPlayer CurrentPlayer { get; set; }
        public DotsPlayer Player1 { get; }
        public DotsPlayer Player2 { get; }

        public int CurrentPlayerValue => CurrentPlayer.Equals(Player1) ? 1 : 2;

        public abstract GameMode CurrentGamemode { get; }

        public float TotalAreaP1
        {
            get => Player1.TotalArea;
            set => Player1.TotalArea = value;
        }

        public float TotalAreaP2
        {
            get => Player2.TotalArea;
            set => Player2.TotalArea = value;
        }

        public HashSet<LineSegment> Hull { get; set; }
        public float HullArea { get; set; }

        public bool Running { get; private set; } = false;
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);

        public void SwitchPlayer()
        {
            CurrentPlayer = CurrentPlayer.Equals(Player1) ? Player2 : Player1;

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
            if (move != null && ((A.Equals(a) && B.Equals(b)) || (B.Equals(b) && A.Equals(a))))
            {
                paths[nextPlayer].Remove(move);
            }
        }

        private void MoveAiPlayer()
        {
            int index = Convert.ToInt32(CurrentPlayer.PlayerNumber) - 1;
            List<PotentialMove> moves = (CurrentPlayer as AiPlayer).NextMove(
                Edges,
                HalfEdges,
                Faces,
                Vertices
            );
            DotsVertex a = moves.Last().A.Original ?? moves.Last().A;
            DotsVertex b = moves.Last().B.Original ?? moves.Last().B;
            moves.Remove(moves.Last());
            paths[index] = moves;

            DoMove(a, b);
        }

        public void MoveForAiPlayer()
        {
            if (CurrentPlayer.PlayerType == PlayerType.Player) return;
            int index = Convert.ToInt32(CurrentPlayer.PlayerNumber) - 1;
            List<PotentialMove> currentPath = paths[index];
            if (currentPath.Any() && currentPath.Last().playerNumber == CurrentPlayer.PlayerNumber)
            {
                DotsVertex a = currentPath.Last().A.Original ?? currentPath.Last().A;
                DotsVertex b = currentPath.Last().B.Original ?? currentPath.Last().B;
                currentPath.Remove(currentPath.Last());
                DoMove(a, b);
            }
            else
            {
                MoveAiPlayer();
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
                Vertices,
                CurrentGamemode
            );

            if (face1 != null)
            {
                Faces.Add(face1);
                AddToPlayerArea(CurrentPlayer, face1.AreaMinusInner);
            }

            if (face2 != null)
            {
                Faces.Add(face2);
                AddToPlayerArea(CurrentPlayer, face2.AreaMinusInner);
            }


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

        public void AddToPlayerArea(DotsPlayer player, float area)
        {
            if (player == Player1) TotalAreaP1 += Math.Abs(area);
            else TotalAreaP2 += Math.Abs(area);
        }

        // Start is called before the first frame update
        public void Start()
        {
            Running = true;
            CurrentPlayer = Player1;

            HelperFunctions.print(
                $"Starting game with Player1 as {Player1.PlayerType} and Player2 as {Player2.PlayerType}");

            // get unity objects
            Vertices = new HashSet<DotsVertex>();
            HalfEdges = new HashSet<DotsHalfEdge>();
            Edges = new HashSet<DotsEdge>();
            Faces = new HashSet<DotsFace>();

            InitLevel();

            Hull = ConvexHullHelper.ComputeHull(Vertices);
            HullArea = Triangulator.Triangulate(
                new Polygon2D(Hull.Select(it => it.Point1))
            ).Area;
            if (Player1.PlayerType == PlayerType.MinMaxAi)
            {
                ((MinMaxAi) Player1).TotalHullArea = HullArea;
                ((MinMaxAi) Player1).InitPairs(Vertices.Count);
            }

            if (Player2.PlayerType == PlayerType.MinMaxAi)
            {
                ((MinMaxAi) Player2).TotalHullArea = HullArea;
                ((MinMaxAi) Player2).InitPairs(Vertices.Count);
            }

            if (Player1.PlayerType != PlayerType.Player)
            {
                MoveForAiPlayer();
            }

            // wait until finished TODO
            while (Running)
            {
                _manualResetEvent.WaitOne();
            }
        }

        public void AddDotsInGeneralPosition()
        {
            // Take the best out of 2
            HashSet<Vector2> bestDots = new HashSet<Vector2>();
            for (int i = 0; i < 2; i++)
            {
                DotsPlacer dotsPlacer = new DotsPlacer(new Rect(MinX, MinY, MaxX - MinX, MaxY - MinY));
                dotsPlacer.AddNewPoints(NumberOfDots);
                if (dotsPlacer.Dots.Count > bestDots.Count) bestDots = dotsPlacer.Dots;
            }

            Vertices = bestDots.Select(it => new DotsVertex(it)).ToHashSet();

            HelperFunctions.print($"Number of placed dots: {bestDots.Count}");
        }

        public virtual void InitLevel()
        {
            // start level using randomly positioned dots in general position
            Clear();
        }

        public void Clear()
        {
            // clear level
            Vertices.Clear();
            HalfEdges.Clear();
            Faces.Clear();

            TotalAreaP1 = 0f;
            TotalAreaP2 = 0f;
        }

        public void AddVisualEdge(DotsVertex a_point1, DotsVertex a_point2)
        {
            LineSegment segment = new LineSegment(a_point1.Coordinates, a_point2.Coordinates);
            Edges.Add(new DotsEdge(segment));
        }

        // Enable advance button if "solution" is correct
        public abstract bool CheckSolutionOfGameState();

        public void FinishLevel()
        {
            HelperFunctions.print($"Game finished!", true);
            // disable all vertices
            foreach (DotsVertex unityDotsVertex in Vertices)
            {
                unityDotsVertex.InFace = true;
            }

            Running = false;
            _manualResetEvent.Set();
            _manualResetEvent.Reset();
        }

        // public void AdvanceLevel()
        // {
        //     // start a new level
        //     InitLevel();
        // }

        // Check whether all points are contained, aka the outer hull is created, so the game ends
        public bool CheckHull()
        {
            HelperFunctions.print($"Current hull: {Hull}");
            HelperFunctions.print($"Current edges: {Edges}");
            IEnumerable<DotsEdge> hullEdges = Edges.Where(edge => Hull.Any(hullEdge =>
                hullEdge.Point1.Equals(edge.Segment.Point2) &&
                hullEdge.Point2.Equals(edge.Segment.Point1)
                || hullEdge.Equals(edge.Segment)));
            return Hull.Count == hullEdges.Count();
        }
    }
}