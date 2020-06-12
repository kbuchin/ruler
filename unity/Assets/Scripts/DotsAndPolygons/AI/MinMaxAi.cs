using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using Util.Geometry;
using Util.Geometry.DCEL;
using Random = System.Random;

namespace DotsAndPolygons
{
    public class MinMaxAi : AiPlayer
    {
        private int MaxDepth = 5;

        private List<ValueMove> currentPath = new List<ValueMove>();
        public float TotalHullArea { get; set; }

        private float threshold = 0.33f;

        private volatile float _alfa = float.MinValue;

        private volatile float _beta = float.MaxValue;

        private static int usingAlfa = 0;

        private static int usingBeta = 0;

        private volatile int[] threadDepth;

        private volatile ValueMove[] resultMoves;

        private int numberOfThreads = 4;

        private ManualResetEvent manualResetEvent = new ManualResetEvent(false);

        public MinMaxAi(PlayerNumber player, HelperFunctions.GameMode mode) : base(player,
            PlayerType.MinMaxAi, mode)
        {
        }

        private ValueMove MinMaxMove(PlayerNumber player, Dcel dCEL,
            int start, int end, int threadId, int currentDepth = 0)
        {
            if(threadDepth.Distinct().Count() != 1)
            {
                threadDepth[threadId] = currentDepth;
                if(threadDepth.Distinct().Count() != 1)
                {
                    manualResetEvent.Set();
                    manualResetEvent.Reset();
                }
                else
                {
                    manualResetEvent.WaitOne();
                }
            }
            
            
            if (currentDepth > 0)
            {
                start = 0;
                end = dCEL.Vertices.Length;
            }
            float value = dCEL.DotsFaces.Sum(x =>
                x.Player == Convert.ToInt32(this.PlayerNumber) ? x.AreaMinusInner : -x.AreaMinusInner);
            float otherPlayerArea = dCEL.DotsFaces.Sum(x => x.Player == Convert.ToInt32(this.PlayerNumber.Switch()) ? x.AreaMinusInner : 0.0f);
            if (currentDepth >= MaxDepth)
            {
                return new ValueMove(value, null, null);
            }

            bool movePossible = false;
            var gameStateMove = new ValueMove(0.0f, null, null);
            gameStateMove.BestValue = player.Equals(this.PlayerNumber) ? float.MinValue : float.MaxValue;
            for (int i = start; i < end - 1; i++)
            {
                for (int j = i + 1; j < end; j++)
                {
                    DotsVertex a = dCEL.Vertices[i];
                    DotsVertex b = dCEL.Vertices[j];
                    if (HelperFunctions.EdgeIsPossible(a, b, dCEL.Edges, dCEL.DotsFaces))
                    {
                        movePossible = true;
                        if(otherPlayerArea > threshold * TotalHullArea)
                        {
                            UpdateGameStateMove(player, gameStateMove, a, b, value, new List<ValueMove>());
                            goto EndLoop;
                        }
                        List<DotsVertex> disabled = new List<DotsVertex>();
                        (DotsFace face1, DotsFace face2) = HelperFunctions.AddEdge(a, b,
                            Convert.ToInt32(player),
                            dCEL.HalfEdges, dCEL.Vertices, GameMode, newlyDisabled: disabled);
                        if (face1 != null) dCEL.DotsFaces.Add(face1);
                        if (face2 != null) dCEL.DotsFaces.Add(face2);
                        float faceArea = (face1?.AreaMinusInner ?? 0.0f) + (face2?.AreaMinusInner ?? 0.0f);

                        PlayerNumber nextPlayer = faceArea > 0.0f ? player : player.Switch();
                        int newDepth = currentDepth + 1; 
                        var newEdge = new DotsEdge(new LineSegment(a.Coordinates, b.Coordinates));
                        dCEL.Edges.Add(newEdge);


                        if (player.Equals(this.PlayerNumber))
                        {
                            ValueMove deeperMoveSamePlayer =
                                MinMaxMove(nextPlayer, dCEL, start, end, threadId, newDepth);
                            if (gameStateMove.BestValue < deeperMoveSamePlayer.BestValue)
                            {
                                UpdateGameStateMove(player, gameStateMove, a, b, deeperMoveSamePlayer.BestValue, deeperMoveSamePlayer.Path);

                                bool alfaLteBeta = UpdateAlfaBeta(gameStateMove.BestValue, float.MaxValue);
                                
                                if (!alfaLteBeta)
                                {
                                    CleanUp(dCEL.HalfEdges, a, b, face1, face2, disabled, dCEL.DotsFaces);
                                    dCEL.Edges.Remove(newEdge);
                                    goto EndLoop;
                                }
                            }
                        }
                        else
                        {
                            ValueMove deeperMoveSamePlayer =
                                MinMaxMove(nextPlayer, dCEL, start, end, threadId, newDepth);
                            if (gameStateMove.BestValue > deeperMoveSamePlayer.BestValue)
                            {
                                UpdateGameStateMove(player, gameStateMove, a, b, deeperMoveSamePlayer.BestValue, deeperMoveSamePlayer.Path);
                                bool alfaLteBeta = UpdateAlfaBeta(float.MinValue, gameStateMove.BestValue);
                                if (alfaLteBeta)
                                {
                                    CleanUp(dCEL.HalfEdges, a, b, face1, face2, disabled, dCEL.DotsFaces);
                                    dCEL.Edges.Remove(newEdge);
                                    goto EndLoop;
                                }
                            }
                        }

                        CleanUp(dCEL.HalfEdges, a, b, face1, face2, disabled, dCEL.DotsFaces);
                        dCEL.Edges.Remove(newEdge);
                    }
                }
            }

            EndLoop:;
            gameStateMove.Path.Add(gameStateMove);
            return movePossible ? gameStateMove : new ValueMove(value, null, null);
        }

        private void StartThread(PlayerNumber player, Dcel dCEL,
            int start, int end, int threadId)
        {
            HelperFunctions.print($"Thread id: {threadId} is starting", debug: true);
            ValueMove result = MinMaxMove(player, dCEL, start, end, threadId);
            HelperFunctions.print($"Thread id: {threadId} is finished", debug: true);
            resultMoves[threadId] = result;
        }

        

        private bool UpdateAlfaBeta(float alfaNew, float betaNew)
        {
            int count = 0;
            int max = 1000;
            while (count < max)
            {
                if (0 == Interlocked.Exchange(ref usingAlfa, 1) && 0 == Interlocked.Exchange(ref usingBeta, 1))
                {
                    //HelperFunctions.print($"Aquired alfa beta lock", debug: true);
                    _alfa = !alfaNew.Equals(float.MinValue) ? Math.Max(_alfa, alfaNew) : _alfa;
                    _beta = !betaNew.Equals(float.MaxValue) ? Math.Min(_beta, alfaNew) : _beta;
                    bool returner = _alfa <= _beta;
                    Interlocked.Exchange(ref usingAlfa, 0);
                    Interlocked.Exchange(ref usingBeta, 0);
                    //HelperFunctions.print($"released alfa beta lock", debug: true);
                    return returner;
                }
                count++;
                Thread.Sleep(1);
                
            }
            HelperFunctions.print($"Unable to aquire lock", debug: true);
            Thread.CurrentThread.Interrupt();
            return false;
        }

        private static void UpdateGameStateMove(PlayerNumber player, ValueMove gameStateMove, DotsVertex a, DotsVertex b, 
            float value, List<ValueMove> path)
        {
            gameStateMove.A = a;
            gameStateMove.B = b;
            gameStateMove.playerNumber = player;
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
            Dcel dCEL = new Dcel(vertices.ToArray(), edges, halfEdges, faces);
            int rangeSize = Convert.ToInt32(vertices.Count / numberOfThreads);
            this.resultMoves = new ValueMove[numberOfThreads];
            this.threadDepth = new int[numberOfThreads];
            _alfa = float.MinValue;
            _beta = float.MaxValue;
            for (int i = 0; i < numberOfThreads; i++)
            {
                int start = i * rangeSize;
                int threadId = i;
                int end = (i + 1) * rangeSize;
                if (end > vertices.Count() - 1)
                {
                    end = vertices.Count();
                }
                var t = new Thread(() => StartThread(this.PlayerNumber, dCEL.Clone(), start, end, threadId));
                t.Start();
                
            }

            while(resultMoves.Any(x => x == null))
            {
                Thread.Sleep(1);
            }
            float best = resultMoves.Select(x => x.BestValue).Max();
            ValueMove returner = resultMoves.FirstOrDefault(x => x.BestValue == best && x.A != null && x.B != null);
            //ValueMove potentialMove = MinMaxMove(PlayerNumber, dCEL);
            HelperFunctions.print($"PotentialMove: {returner}", debug: true);

            return returner.Path.Select(x => (PotentialMove) x ).ToList();
        }
    }
}