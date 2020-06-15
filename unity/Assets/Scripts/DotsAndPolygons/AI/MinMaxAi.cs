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
        private int MaxDepth = 6;
        public float TotalHullArea { get; set; }

        private float threshold = 0.5f;

        private volatile MoveCollection[] resultMoves;

        private int numberOfThreads = 2;

        private ManualResetEvent manualResetEvent = new ManualResetEvent(false);

        private List<(int, int)> pairs = new List<(int, int)>();

        public MinMaxAi(PlayerNumber player, HelperFunctions.GameMode mode) : base(player,
            PlayerType.MinMaxAi, mode)
        {
            
        }

        public void InitPairs(int numberOfDots)
        {
            for (int i = 0; i < numberOfDots; i++)
            {
                for (int j = i + 1; j < numberOfDots; j++)
                {
                    pairs.Add((i, j));
                }
            }
        }

        private MoveCollection MinMaxMove(PlayerNumber player, Dcel dCEL,
            int start, int end, int threadId, float alfa = float.MinValue, float beta = float.MaxValue, int currentDepth = 0)
        {
            if (currentDepth > 0)
            {
                start = 0;
                end = pairs.Count;
            }
            float value = dCEL.DotsFaces.Sum(x =>
                x.Player == Convert.ToInt32(this.PlayerNumber) ? x.AreaMinusInner : -x.AreaMinusInner);
            float otherPlayerArea = dCEL.DotsFaces.Sum(x => x.Player == Convert.ToInt32(this.PlayerNumber.Switch()) ? x.AreaMinusInner : 0.0f);
            MoveCollection moveCollection = new MoveCollection(value);
            var gameStateMove = new ValueMove(0.0f, null, null);
            moveCollection.Value = player.Equals(this.PlayerNumber) ? float.MinValue : float.MaxValue;
            if (currentDepth >= MaxDepth)
            {
                return new MoveCollection(value);
            }

            bool movePossible = false;
            List<(int, int)> pairstoEvaluate = pairs.Skip(start).Take(end - start).ToList();
            foreach ((int i, int j) in pairstoEvaluate)
            {
                DotsVertex a = dCEL.Vertices[i];
                DotsVertex b = dCEL.Vertices[j];
                if(currentDepth == 0)
                {
                    alfa = float.MinValue;
                    beta = float.MaxValue;
                }
                if (HelperFunctions.EdgeIsPossible(a, b, dCEL.Edges, dCEL.DotsFaces))
                {
                    if(otherPlayerArea > threshold * TotalHullArea)
                    {
                        UpdateGameStateMove(player, gameStateMove, a, b, value, moveCollection, new MoveCollection(value));
                    }
                    movePossible = true;
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
                        MoveCollection deeperMoveSamePlayer =
                            MinMaxMove(nextPlayer, dCEL, start, end, threadId, alfa, beta, newDepth);
                        if (moveCollection.Value < deeperMoveSamePlayer.Value)
                        {
                            UpdateGameStateMove(player, gameStateMove, a, b, deeperMoveSamePlayer.Value, moveCollection, deeperMoveSamePlayer);
                                
                            alfa = Math.Max(alfa, deeperMoveSamePlayer.Value);
                                
                            if (alfa >= beta)
                            {
                                CleanUp(dCEL.HalfEdges, a, b, face1, face2, disabled, dCEL.DotsFaces);
                                dCEL.Edges.Remove(newEdge);
                                goto EndLoop;
                            }
                        }
                    }
                    else
                    {
                        MoveCollection deeperMoveSamePlayer =
                            MinMaxMove(nextPlayer, dCEL, start, end, threadId, alfa, beta, newDepth);
                        if (moveCollection.Value > deeperMoveSamePlayer.Value)
                        {
                            UpdateGameStateMove(player, gameStateMove, a, b, deeperMoveSamePlayer.Value, moveCollection, deeperMoveSamePlayer);
                                
                            beta = Math.Min(beta, deeperMoveSamePlayer.Value);
                            if (beta <= alfa)
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

            EndLoop:;
            moveCollection.PotentialMoves.Add(gameStateMove);
            if(!movePossible && currentDepth == 0)
            {
                value = float.MinValue;
            }
            return movePossible ? moveCollection : new MoveCollection(value);
        }

        private void StartThread(PlayerNumber player, Dcel dCEL,
            int start, int end, int threadId)
        {
            HelperFunctions.print($"Thread id: {threadId} is starting");
            MoveCollection result = MinMaxMove(player, dCEL, start, end, threadId);
            HelperFunctions.print($"Thread id: {threadId} is finished");
            resultMoves[threadId] = result;
            manualResetEvent.Set();
            manualResetEvent.Reset();
        }

        private static void UpdateGameStateMove(PlayerNumber player, ValueMove gameStateMove, DotsVertex a, DotsVertex b, 
            float value, MoveCollection path, MoveCollection deeperPath)
        {
            gameStateMove.A = a;
            gameStateMove.B = b;
            gameStateMove.playerNumber = player;
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
            HashSet<DotsVertex> vertices)
        {
            Timer.StartTimer();
            HelperFunctions.print("Calculating next minimal move for MinMaxAI player");
            Dcel dCEL = new Dcel(vertices.ToArray(), edges, halfEdges, faces);
            Random random = new Random();
            pairs = pairs.OrderBy(_ => random.Next()).ToList();
            this.resultMoves = new MoveCollection[numberOfThreads];
            int remainder = pairs.Count % numberOfThreads;
            int rangeSize = Convert.ToInt32(pairs.Count / numberOfThreads);
            int prevEnd = 0;
            for (int i = 0; i < numberOfThreads; i++)
            {
                int start = prevEnd;
                int threadId = i;
                int end = remainder-- > 0 ? start + rangeSize + 1 : start + rangeSize;
                prevEnd = end;
                Dcel cloned = dCEL.Clone();
                var t = new Thread(() => StartThread(this.PlayerNumber, cloned, start, end, threadId));
                t.Start();
                
            }
            while(resultMoves.Any(x => x == null))
            {
                manualResetEvent.WaitOne();
            }
            MoveCollection returner = resultMoves.OrderByDescending(x => x.Value).FirstOrDefault();
            HelperFunctions.print($"PotentialMove: {returner}", debug: true);
            Timer.StopTimer();
            return returner.PotentialMoves;
        }
    }
}