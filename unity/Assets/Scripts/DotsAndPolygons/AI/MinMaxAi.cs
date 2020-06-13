using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Util.Geometry;

namespace DotsAndPolygons
{
    public class MinMaxAi : AiPlayer
    {
        private const int MaxDepth = 5;

        // private List<ValueMove> currentPath = new List<ValueMove>(); // todo not used?
        public float TotalHullArea { get; set; }

        private const float Threshold = 0.33f;

        private volatile float _alpha = float.MinValue;

        private volatile float _beta = float.MaxValue;

        private static int _usingAlpha = 0;

        private static int _usingBeta = 0;

        private volatile int[] _threadDepth;

        private volatile ValueMove[] _resultMoves;

        private const int NumberOfThreads = 4;

        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);

        public MinMaxAi(PlayerNumber player, HelperFunctions.GameMode mode) : base(player,
            PlayerType.MinMaxAi, mode)
        {
        }

        private ValueMove MinMaxMove(PlayerNumber player, Dcel dcel,
            int start, int end, int threadId, int currentDepth = 0)
        {
            if (_threadDepth.Distinct().Count() != 1)
            {
                _threadDepth[threadId] = currentDepth;
                if (_threadDepth.Distinct().Count() != 1)
                {
                    _manualResetEvent.Set();
                    _manualResetEvent.Reset();
                }
                else
                {
                    _manualResetEvent.WaitOne();
                }
            }


            if (currentDepth > 0)
            {
                start = 0;
                end = dcel.Vertices.Length;
            }

            float value = dcel.DotsFaces.Sum(x =>
                x.Player == Convert.ToInt32(PlayerNumber) ? x.AreaMinusInner : -x.AreaMinusInner);
            float otherPlayerArea = dcel.DotsFaces.Sum(x =>
                x.Player == Convert.ToInt32(PlayerNumber.Switch()) ? x.AreaMinusInner : 0.0f);
            if (currentDepth >= MaxDepth)
            {
                return new ValueMove(value, null, null);
            }

            bool movePossible = false;
            ValueMove gameStateMove = new ValueMove(0.0f, null, null)
            {
                BestValue = player.Equals(PlayerNumber) ? float.MinValue : float.MaxValue
            };
            for (int i = start; i < end - 1; i++)
            {
                for (int j = i + 1; j < end; j++)
                {
                    DotsVertex a = dcel.Vertices[i];
                    DotsVertex b = dcel.Vertices[j];
                    if (HelperFunctions.EdgeIsPossible(a, b, dcel.Edges, dcel.DotsFaces))
                    {
                        movePossible = true;
                        if (otherPlayerArea > Threshold * TotalHullArea)
                        {
                            UpdateGameStateMove(player, gameStateMove, a, b, value, new List<ValueMove>());
                            goto EndLoop;
                        }

                        var disabled = new List<DotsVertex>();
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
                            ValueMove deeperMoveSamePlayer =
                                MinMaxMove(nextPlayer, dcel, start, end, threadId, newDepth);
                            if (gameStateMove.BestValue < deeperMoveSamePlayer.BestValue)
                            {
                                UpdateGameStateMove(player, gameStateMove, a, b, deeperMoveSamePlayer.BestValue,
                                    deeperMoveSamePlayer.Path);

                                bool alphaLteBeta = UpdateAlphaBeta(gameStateMove.BestValue, float.MaxValue);

                                if (!alphaLteBeta)
                                {
                                    CleanUp(dcel.HalfEdges, a, b, face1, face2, disabled, dcel.DotsFaces);
                                    dcel.Edges.Remove(newEdge);
                                    goto EndLoop;
                                }
                            }
                        }
                        else
                        {
                            ValueMove deeperMoveSamePlayer =
                                MinMaxMove(nextPlayer, dcel, start, end, threadId, newDepth);
                            if (gameStateMove.BestValue > deeperMoveSamePlayer.BestValue)
                            {
                                UpdateGameStateMove(player, gameStateMove, a, b, deeperMoveSamePlayer.BestValue,
                                    deeperMoveSamePlayer.Path);
                                bool alphaLteBeta = UpdateAlphaBeta(float.MinValue, gameStateMove.BestValue);
                                if (alphaLteBeta)
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
                }
            }

            EndLoop: ;
            gameStateMove.Path.Add(gameStateMove);
            return movePossible ? gameStateMove : new ValueMove(value, null, null);
        }

        private void StartThread(PlayerNumber player, Dcel dcel,
            int start, int end, int threadId)
        {
            HelperFunctions.print($"Thread id: {threadId} is starting", debug: true);
            ValueMove result = MinMaxMove(player, dcel, start, end, threadId);
            HelperFunctions.print($"Thread id: {threadId} is finished", debug: true);
            _resultMoves[threadId] = result;
        }


        private bool UpdateAlphaBeta(float alphaNew, float betaNew)
        {
            int count = 0;
            const int max = 1000;
            while (count < max)
            {
                if (0 == Interlocked.Exchange(ref _usingAlpha, 1) && 0 == Interlocked.Exchange(ref _usingBeta, 1))
                {
                    //HelperFunctions.print($"Acquired alpha beta lock", debug: true);
                    _alpha = !alphaNew.Equals(float.MinValue) ? Math.Max(_alpha, alphaNew) : _alpha;
                    _beta = !betaNew.Equals(float.MaxValue) ? Math.Min(_beta, alphaNew) : _beta;
                    bool returner = _alpha <= _beta;
                    Interlocked.Exchange(ref _usingAlpha, 0);
                    Interlocked.Exchange(ref _usingBeta, 0);
                    //HelperFunctions.print($"released alpha beta lock", debug: true);
                    return returner;
                }

                count++;
                Thread.Sleep(1);
            }

            HelperFunctions.print($"Unable to acquire lock", debug: true);
            Thread.CurrentThread.Interrupt();
            return false;
        }

        private static void UpdateGameStateMove(PlayerNumber player, ValueMove gameStateMove, DotsVertex a,
            DotsVertex b,
            float value, List<ValueMove> path)
        {
            gameStateMove.A = a;
            gameStateMove.B = b;
            gameStateMove.PlayerNumber = player;
            gameStateMove.BestValue = value;
            gameStateMove.Path = path;
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
            HashSet<DotsVertex> vertices)
        {
            HelperFunctions.print("Calculating next minimal move for MinMaxAI player");
            Dcel dcel = new Dcel(vertices.ToArray(), edges, halfEdges, faces);
            int rangeSize = Convert.ToInt32(vertices.Count / NumberOfThreads);
            _resultMoves = new ValueMove[NumberOfThreads];
            _threadDepth = new int[NumberOfThreads];
            _alpha = float.MinValue;
            _beta = float.MaxValue;
            for (int i = 0; i < NumberOfThreads; i++)
            {
                int start = i * rangeSize;
                int threadId = i;
                int end = (i + 1) * rangeSize;
                if (end > vertices.Count - 1)
                {
                    end = vertices.Count;
                }

                Thread t = new Thread(() => StartThread(PlayerNumber, dcel.Clone(), start, end, threadId));
                t.Start();
            }

            while (_resultMoves.Any(x => x == null))
            {
                Thread.Sleep(1);
            }

            float best = _resultMoves.Select(x => x.BestValue).Max();
            ValueMove returner = _resultMoves.FirstOrDefault(x =>
                Math.Abs(x.BestValue - best) < HelperFunctions.BIETJE && x.A != null && x.B != null);
            //ValueMove potentialMove = MinMaxMove(PlayerNumber, dCEL);
            HelperFunctions.print($"PotentialMove: {returner}", debug: true);

            return returner.Path.Select(x => (PotentialMove) x).ToList();
        }
    }
}