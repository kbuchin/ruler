using System;
using System.Collections.Generic;
using System.Linq;
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

        public MinMaxAi(PlayerNumber player, HelperFunctions.GameMode mode) : base(player,
            PlayerType.MinMaxAi, mode)
        {
        }

        public ValueMove MinMaxMove(PlayerNumber player, Dcel dCEL,
            float alfa = float.MinValue, float beta = float.MaxValue, int currentDepth = 0)
        {
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
            for (int i = 0; i < dCEL.Vertices.Length - 1; i++)
            {
                for (int j = i + 1; j < dCEL.Vertices.Length; j++)
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
                                MinMaxMove(nextPlayer, dCEL, alfa, beta, newDepth);
                            if (gameStateMove.BestValue < deeperMoveSamePlayer.BestValue)
                            {
                                UpdateGameStateMove(player, gameStateMove, a, b, deeperMoveSamePlayer.BestValue, deeperMoveSamePlayer.Path);
                                alfa = Math.Max(alfa, gameStateMove.BestValue);
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
                            ValueMove deeperMoveSamePlayer =
                                MinMaxMove(nextPlayer, dCEL, alfa, beta, newDepth);
                            if (gameStateMove.BestValue > deeperMoveSamePlayer.BestValue)
                            {
                                UpdateGameStateMove(player, gameStateMove, a, b, deeperMoveSamePlayer.BestValue, deeperMoveSamePlayer.Path);
                                beta = Math.Min(beta, gameStateMove.BestValue);
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
            }

            EndLoop:;
            gameStateMove.Path.Add(gameStateMove);
            return movePossible ? gameStateMove : new ValueMove(value, null, null);
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
            ValueMove potentialMove = MinMaxMove(PlayerNumber, dCEL);
            HelperFunctions.print($"PotentialMove: {potentialMove}", debug: true);

            return potentialMove.Path.Select(x => (PotentialMove) x ).ToList();
        }
    }
}