using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Util.Geometry;
using Random = System.Random;

namespace DotsAndPolygons
{
    public class MinMaxAi : AiPlayer
    {
        private volatile int _maxDepth;
        public float TotalHullArea { get; set; }

        private volatile float _threshold;

        private volatile MoveCollection[] _resultMoves;

        private volatile int _numberOfThreads;

        private ManualResetEvent _manualResetEvent = new ManualResetEvent(false);

        private List<(int, int)> _pairs = new List<(int, int)>();

        public MinMaxAi(
            PlayerNumber player,
            HelperFunctions.GameMode mode,
            int maxDepth = 3,
            float threshold = 0.5f,
            int numberOfThreads = 1
        ) : base(player, PlayerType.MinMaxAi, mode)
        {
            _maxDepth = maxDepth;
            _threshold = threshold;
            _numberOfThreads = numberOfThreads;
        }

        public void InitPairs(int numberOfDots)
        {
            for (int i = 0; i < numberOfDots; i++)
            {
                for (int j = i + 1; j < numberOfDots; j++)
                {
                    _pairs.Add((i, j));
                }
            }
        }

        private MoveCollection MinMaxMove(PlayerNumber player, Dcel dcel,
            int start, int end, float alpha = float.MinValue, float beta = float.MaxValue,
            int currentDepth = 0)
        {
            if (currentDepth > 0)
            {
                start = 0;
                end = _pairs.Count;
            }

            float value = dcel.DotsFaces.Sum(x =>
                x.Player == Convert.ToInt32(PlayerNumber) ? x.AreaMinusInner : -x.AreaMinusInner);
            float otherPlayerArea = dcel.DotsFaces.Sum(x =>
                x.Player == Convert.ToInt32(PlayerNumber.Switch()) ? x.AreaMinusInner : 0.0f);
            MoveCollection moveCollection = new MoveCollection(value);
            ValueMove gameStateMove = new ValueMove(0.0f, null, null);
            moveCollection.Value = player.Equals(PlayerNumber) ? float.MinValue : float.MaxValue;
            if (currentDepth >= _maxDepth)
            {
                return new MoveCollection(value);
            }

            bool movePossible = false;
            List<(int, int)> pairsToEvaluate = _pairs.Skip(start).Take(end - start).ToList();
            foreach ((int i, int j) in pairsToEvaluate)
            {
                DotsVertex a = dcel.Vertices[i];
                DotsVertex b = dcel.Vertices[j];
                if (currentDepth == 0)
                {
                    alpha = float.MinValue;
                    beta = float.MaxValue;
                }

                if (!HelperFunctions.EdgeIsPossible(a, b, dcel.Edges, dcel.DotsFaces)) continue;
                movePossible = true;
                
                if (otherPlayerArea > _threshold * TotalHullArea)
                {
                    UpdateGameStateMove(player, gameStateMove, a, b, value, moveCollection,
                        new MoveCollection(value));
                }
                
                List<DotsVertex> disabled = new List<DotsVertex>();
                (DotsFace face1, DotsFace face2) = HelperFunctions.AddEdge(a, b,
                    Convert.ToInt32(player),
                    dcel.HalfEdges, dcel.Vertices, GameMode, newlyDisabled: disabled);
                if (face1 != null) dcel.DotsFaces.Add(face1);
                if (face2 != null) dcel.DotsFaces.Add(face2);
                float faceArea = (face1?.AreaMinusInner ?? 0.0f) + (face2?.AreaMinusInner ?? 0.0f);

                PlayerNumber nextPlayer = faceArea > 0.0f ? player : player.Switch();
                int newDepth = currentDepth + 1;
                DotsEdge newEdge = new DotsEdge(new LineSegment(a.Coordinates, b.Coordinates));
                dcel.Edges.Add(newEdge);


                if (player.Equals(PlayerNumber))
                {
                    MoveCollection deeperMoveSamePlayer =
                        MinMaxMove(player: nextPlayer, dcel: dcel, start: start, end: end, alpha: alpha, beta: beta, currentDepth: newDepth);
                    if (moveCollection.Value < deeperMoveSamePlayer.Value)
                    {
                        UpdateGameStateMove(player, gameStateMove, a, b, deeperMoveSamePlayer.Value, moveCollection,
                            deeperMoveSamePlayer);

                        alpha = Math.Max(alpha, deeperMoveSamePlayer.Value);
                        if (alpha >= beta)
                        {
                            CleanUp(dcel.HalfEdges, a, b, face1, face2, disabled, dcel.DotsFaces);
                            dcel.Edges.Remove(newEdge);
                            goto EndLoop;
                        }
                    }
                }
                else
                {
                    MoveCollection deeperMoveSamePlayer =
                        MinMaxMove(player: nextPlayer, dcel: dcel, start: start, end: end, alpha: alpha, beta: beta, currentDepth: newDepth);
                    if (moveCollection.Value > deeperMoveSamePlayer.Value)
                    {
                        UpdateGameStateMove(player, gameStateMove, a, b, deeperMoveSamePlayer.Value, moveCollection,
                            deeperMoveSamePlayer);

                        beta = Math.Min(beta, deeperMoveSamePlayer.Value);
                        if (beta <= alpha)
                        {
                            CleanUp(dcel.HalfEdges, a, b, face1, face2, disabled, dcel.DotsFaces);
                            dcel.Edges.Remove(newEdge);
                            goto EndLoop;
                        }
                    }
                }

                CleanUp(dcel.HalfEdges, a, b, face1, face2, disabled, dcel.DotsFaces);
                dcel.Edges.Remove(newEdge);
            }

            EndLoop: ;
            moveCollection.PotentialMoves.Add(gameStateMove);
            if (!movePossible && currentDepth == 0)
            {
                value = float.MinValue;
            }

            return movePossible ? moveCollection : new MoveCollection(value);
        }

        private void StartThread(PlayerNumber player, Dcel dcel, int start, int end, int threadId)
        {
            HelperFunctions.print($"Thread id: {threadId} is starting");
            MoveCollection result = MinMaxMove(player: player, dcel: dcel, start: start, end: end);
            HelperFunctions.print($"Thread id: {threadId} is finished");
            _resultMoves[threadId] = result;
            _manualResetEvent.Set();
            _manualResetEvent.Reset();
        }

        private static void UpdateGameStateMove(PlayerNumber player, ValueMove gameStateMove, DotsVertex a, DotsVertex b,
            float value, MoveCollection path, MoveCollection deeperPath)
        {
            gameStateMove.A = a;
            gameStateMove.B = b;
            gameStateMove.PlayerNumber = player;
            gameStateMove.BestValue = value;
            path.Value = value;
            path.PotentialMoves = deeperPath.PotentialMoves;
        }

        private static void CleanUp(HashSet<DotsHalfEdge> halfEdges, DotsVertex a, DotsVertex b,
            DotsFace created1, DotsFace created2, List<DotsVertex> disabled, HashSet<DotsFace> dotsFaces)
        {
            DotsHalfEdge toRemove = a.LeavingHalfEdges()
                .FirstOrDefault(it => it.Destination.Equals(b));
            disabled.ForEach(it => it.InFace = false);
            if (created1 != null) dotsFaces.Remove(created1);
            if (created2 != null) dotsFaces.Remove(created2);
            created1?.OuterComponentHalfEdges.ForEach(it => it.IncidentFace = null);
            created2?.OuterComponentHalfEdges.ForEach(it => it.IncidentFace = null);
            HelperFunctions.RemoveFromDCEL(toRemove);
            halfEdges.Remove(toRemove);
            halfEdges.Remove(toRemove?.Twin);
        }

        public override List<PotentialMove> NextMove(
            HashSet<DotsEdge> edges,
            HashSet<DotsHalfEdge> halfEdges,
            HashSet<DotsFace> faces,
            HashSet<DotsVertex> vertices,
            bool multiThreaded = false
        )
        {
            HelperFunctions.print("Calculating next minimal move for MinMaxAI player");
            Dcel dcel = new Dcel(vertices.ToArray(), edges, halfEdges, faces);
            Random random = new Random();
            _pairs = _pairs.OrderBy(_ => random.Next()).ToList();
            _resultMoves = new MoveCollection[_numberOfThreads];
            int remainder = _pairs.Count % _numberOfThreads;
            int rangeSize = Convert.ToInt32(_pairs.Count / _numberOfThreads);
            int prevEnd = 0;
            if(multiThreaded)
            {
                for (int i = 0; i < _numberOfThreads; i++)
                {
                    int start = prevEnd;
                    int threadId = i;
                    int end = remainder-- > 0 ? start + rangeSize + 1 : start + rangeSize;
                    prevEnd = end;
                    Dcel cloned = dcel.Clone();
                    Thread t = new Thread(() => StartThread(PlayerNumber, cloned, start, end, threadId));
                    t.Start();
                }
                while (_resultMoves.Any(x => x == null))
                {
                    _manualResetEvent.WaitOne();
                    HelperFunctions.print($"Resultmoves: {string.Join(", ", _resultMoves as object[])}");
                }
                MoveCollection returner = _resultMoves.OrderByDescending(x => x.Value).FirstOrDefault();
                HelperFunctions.print($"PotentialMove: {returner}");
                return returner.PotentialMoves;
            }
            else
            {
                return MinMaxMove(PlayerNumber, dcel, 0, _pairs.Count).PotentialMoves;
            }
        }
    }
}