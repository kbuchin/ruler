using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Util.Geometry;
using Random = System.Random;

namespace DotsAndPolygons
{
    public class MinMaxAi : AiPlayer
    {
        private int MaxDepth = 2;

        public MinMaxAi(PlayerNumber player, HelperFunctions.GameMode mode) : base(player,
            PlayerType.MinMaxAi, mode)
        {
        }

        [CanBeNull]
        public ValueMove MinMaxMove(PlayerNumber player, IDotsVertex[] vertices, HashSet<IDotsEdge> edges,
            HashSet<IDotsHalfEdge> halfEdges, HashSet<IDotsFace> dotsFaces, int currentDepth = 0)
        {
            var bestValueMove = new ValueMove(float.MinValue, null, null);

            // first maximize area if able to create a face
            // then run MinMaxMove again for same player
            // does not increase depth
            // first calculate possible max area
            for (int i = 0; i < vertices.Length - 1; i++)
            {
                for (int j = i + 1; j < vertices.Length; j++)
                {
                    // MonoBehaviour.print($"Calculating maximal area move, iteration {i}, {j}");
                    IDotsVertex a = vertices[i];
                    IDotsVertex b = vertices[j];

                    if (HelperFunctions.EdgeIsPossible(a, b, edges, dotsFaces))
                    {
                        List<IDotsVertex> disabled = new List<IDotsVertex>();
                        (IDotsFace face1, IDotsFace face2) = HelperFunctions.AddEdge(a, b,
                            Convert.ToInt32(PlayerNumber),
                            halfEdges, vertices, GameMode, newlyDisabled: disabled);

                        if (face1 != null) dotsFaces.Add(face1);
                        if (face2 != null) dotsFaces.Add(face2);

                        var newEdge = new DotsEdge(new LineSegment(a.Coordinates, b.Coordinates));
                        edges.Add(newEdge);

                        float newValue;

                        if (face1 != null || face2 != null)
                        {
                            float faceArea = (face1?.AreaMinusInner ?? 0.0f) + (face2?.AreaMinusInner ?? 0.0f);
                            if (player == PlayerNumber)
                                newValue = faceArea;
                            else
                                newValue = -faceArea;

                            // GO DEEPER
                            if (currentDepth < MaxDepth)
                            {
                                ValueMove deeperMoveSamePlayer = MinMaxMove(player, vertices, edges, halfEdges,
                                    dotsFaces,
                                    currentDepth);
                                if (deeperMoveSamePlayer != null)
                                {
                                    newValue += deeperMoveSamePlayer.BestValue;
                                }
                            }
                        }
                        else
                        {
                            // GO DEEPER OTHER PLAYER
                            if (currentDepth < MaxDepth)
                            {
                                ValueMove deeperMoveOtherPlayer = MinMaxMove(player.Switch(), vertices, edges,
                                    halfEdges,
                                    dotsFaces, ++currentDepth);
                                newValue = deeperMoveOtherPlayer?.BestValue ?? bestValueMove.BestValue;
                            }
                            else
                            {
                                newValue = bestValueMove.BestValue;
                            }
                        }

                        if (bestValueMove.BestValue + newValue > bestValueMove.BestValue || bestValueMove.A == null || bestValueMove.B == null)
                        {
                            bestValueMove.BestValue += newValue;
                            bestValueMove.A = a;
                            bestValueMove.B = b;
                        }


                        CleanUp(halfEdges, a, b, face1, face2, disabled, dotsFaces);
                        edges.Remove(newEdge);
                    }
                }
            }

            if (bestValueMove.A == null || bestValueMove.B == null) return null;

            return bestValueMove;
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