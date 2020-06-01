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
        private int MaxDepth = 4;

        public MinMaxAi(PlayerNumber player, HelperFunctions.GameMode mode) : base(player,
            PlayerType.MinMaxAi, mode)
        {
        }

        [CanBeNull]
        public ValueMove MinMaxMove(PlayerNumber player, IDotsVertex[] vertices, HashSet<IDotsEdge> edges,
            HashSet<IDotsHalfEdge> halfEdges, HashSet<IDotsFace> dotsFaces, float alfa = float.MinValue, float beta = float.MaxValue, int currentDepth = 0)
        {
            float value = dotsFaces.Sum(x => x.Player == Convert.ToInt32(this.PlayerNumber) ? x.AreaMinusInner : -x.AreaMinusInner);
            if (currentDepth >= MaxDepth)
            {
                HelperFunctions.print("print move: " + value);
                return new ValueMove(value, null, null);
            }
            bool movePossible = false;
            var gameStateMove = new ValueMove(0.0f, null, null);
            gameStateMove.BestValue = player.Equals(this.PlayerNumber) ? float.MinValue : float.MaxValue;
            for (int i = 0; i < vertices.Length - 1; i++)
            {
                for (int j = i + 1; j < vertices.Length; j++)
                {
                    IDotsVertex a = vertices[i];
                    IDotsVertex b = vertices[j];
                    if (HelperFunctions.EdgeIsPossible(a, b, edges, dotsFaces))
                    {
                        movePossible = true;
                        List<IDotsVertex> disabled = new List<IDotsVertex>();
                        (IDotsFace face1, IDotsFace face2) = HelperFunctions.AddEdge(a, b,
                            Convert.ToInt32(player),
                            halfEdges, vertices, GameMode, newlyDisabled: disabled);
                        if (face1 != null) dotsFaces.Add(face1);
                        if (face2 != null) dotsFaces.Add(face2);
                        float faceArea = (face1?.AreaMinusInner ?? 0.0f) + (face2?.AreaMinusInner ?? 0.0f);
                        PlayerNumber nextPlayer = faceArea > 0.0f ? player : player.Switch();
                        int newDepth = currentDepth + 1; //player != nextPlayer ? currentDepth + 1 : currentDepth;
                        var newEdge = new DotsEdge(new LineSegment(a.Coordinates, b.Coordinates));
                        edges.Add(newEdge);
                        

                        if (player.Equals(this.PlayerNumber))
                        {
                            //gameStateMove.BestValue += faceArea;
                            ValueMove deeperMoveSamePlayer = MinMaxMove(nextPlayer, vertices, edges, halfEdges,
                                    dotsFaces, alfa,beta, newDepth);
                            if (gameStateMove.BestValue < deeperMoveSamePlayer.BestValue)
                            {
                                gameStateMove.A = a;
                                gameStateMove.B = b;
                                gameStateMove.BestValue = deeperMoveSamePlayer.BestValue;
                                alfa = Math.Max(alfa, gameStateMove.BestValue);
                                if(alfa >= beta)
                                {
                                    CleanUp(halfEdges, a, b, face1, face2, disabled, dotsFaces);
                                    edges.Remove(newEdge);
                                    goto EndLoop;
                                }
                            }
                        }
                        else
                        {
                            //gameStateMove.BestValue -= faceArea;
                            ValueMove deeperMoveSamePlayer = MinMaxMove(nextPlayer, vertices, edges, halfEdges,
                                    dotsFaces, alfa, beta, newDepth);
                            if (gameStateMove.BestValue > deeperMoveSamePlayer.BestValue)
                            {
                                gameStateMove.A = a;
                                gameStateMove.B = b;
                                gameStateMove.BestValue = deeperMoveSamePlayer.BestValue;
                                beta = Math.Min(beta, gameStateMove.BestValue);
                                if (beta <= alfa)
                                {
                                    CleanUp(halfEdges, a, b, face1, face2, disabled, dotsFaces);
                                    edges.Remove(newEdge);
                                    goto EndLoop;
                                }
                            }
                        }
                        
                        CleanUp(halfEdges, a, b, face1, face2, disabled, dotsFaces);
                        edges.Remove(newEdge);
                    }                
                }
            }
            EndLoop:;
            if(!movePossible)
            {
                HelperFunctions.print("print move: " + value + " ");
            }
            return movePossible ? gameStateMove : new ValueMove(value, null, null);
        }


        private static void CleanUp(HashSet<IDotsHalfEdge> halfEdges, IDotsVertex a, IDotsVertex b,
            IDotsFace created1, IDotsFace created2, List<IDotsVertex> disabled, HashSet<IDotsFace> dotsFaces)
        {
            IDotsHalfEdge toRemove = a.LeavingHalfEdges()
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

        public override (IDotsVertex, IDotsVertex) NextMove(HashSet<IDotsEdge> edges,
            HashSet<IDotsHalfEdge> halfEdges,
            HashSet<IDotsFace> faces, IEnumerable<IDotsVertex> vertices)
        {
            IDotsVertex[] verticesArray = vertices.ToArray();

            HelperFunctions.print("Calculating next minimal move for MinMaxAI player");
            PotentialMove potentialMove = MinMaxMove(PlayerNumber, verticesArray, edges, halfEdges, faces);

            HelperFunctions.print($"PotentialMove: {potentialMove}", debug: true);

            return (potentialMove.A, potentialMove.B);
        }
    }
}