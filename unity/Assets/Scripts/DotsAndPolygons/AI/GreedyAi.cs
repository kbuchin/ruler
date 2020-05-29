using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util.Geometry;
using Random = System.Random;

namespace DotsAndPolygons
{
    public class GreedyAi : AiPlayer
    {
        public GreedyAi(PlayerNumber player, HelperFunctions.GameMode mode) : base(player, PlayerType.GreedyAi, mode) {}

        public PotentialMove MinimalMove(int start, int length, IDotsVertex[] vertices, HashSet<IDotsEdge> edges,
            HashSet<IDotsHalfEdge> halfEdges, HashSet<IDotsFace> dotsFaces)
        {
            int end = start + length;
            float minimalWeight = float.MaxValue;
            float maximalArea = 0.0f;
            IDotsVertex minA = null;
            IDotsVertex minB = null;
            IDotsVertex maxAreaA = null;
            IDotsVertex maxAreaB = null;
            // bool claimPossible = false;
            PotentialMove result;

            // first calculate possible max area
            for (int i = start; i < end - 1; i++)
            {
                for (int j = i + 1; j < end; j++)
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

                        var newEdge = new DotsEdge(new LineSegment(a.Coordinates, b.Coordinates));
                        edges.Add(newEdge);

                        if (face1 != null || face2 != null)
                        {
                            float newMaximalArea = (face1?.AreaMinusInner ?? 0.0f) + (face2?.AreaMinusInner ?? 0.0f);
                            if (newMaximalArea > maximalArea)
                            {
                                maximalArea = newMaximalArea;
                                maxAreaA = a;
                                maxAreaB = b;
                            }
                        }

                        CleanUp(halfEdges, a, b, face1, face2, disabled);
                        edges.Remove(newEdge);
                    }
                }
            }


            // second calculate the least area given to other player
            if (maximalArea < HelperFunctions.BIETJE)
            {
                // shuffle vertices
                var rnd = new Random();
                vertices = vertices.OrderBy(_ => rnd.Next()).ToArray();
                
                for (int i = start; i < end - 1; i++)
                {
                    for (int j = i + 1; j < end; j++)
                    {
                        // MonoBehaviour.print($"Calculating minimal move, iteration {i}, {j}");
                        IDotsVertex a = vertices[i];
                        IDotsVertex b = vertices[j];

                        if (HelperFunctions.EdgeIsPossible(a, b, edges, dotsFaces))
                        {
                            List<IDotsVertex> disabled = new List<IDotsVertex>();
                            (IDotsFace face1, IDotsFace face2) = HelperFunctions.AddEdge(a, b,
                                Convert.ToInt32(PlayerNumber),
                                halfEdges, vertices, GameMode, newlyDisabled: disabled);

                            var newEdge = new DotsEdge(new LineSegment(a.Coordinates, b.Coordinates));
                            edges.Add(newEdge);


                            float weight = CalculateWeight(a, b, vertices, edges, halfEdges,
                                dotsFaces);
                            if (weight < minimalWeight)
                            {
                                minA = a;
                                minB = b;
                                minimalWeight = weight;
                            }

                            CleanUp(halfEdges, a, b, face1, face2, disabled);
                            edges.Remove(newEdge);

                            // weights are randomized, so return first edge with 0 weight found (for speed)
                            if (weight < HelperFunctions.BIETJE)
                            {
                                return new WeightMove(minimalWeight, minA, minB);
                            }
                        }
                    }
                }
            }

            result = maximalArea > 0.0
                ? new AreaMove(maximalArea, maxAreaA, maxAreaB)
                : (PotentialMove) new WeightMove(minimalWeight, minA, minB);
            return result;
        }

        private float CalculateWeight(IDotsVertex dotsVertex1, IDotsVertex dotsVertex2, IDotsVertex[] dots,
            IEnumerable<IDotsEdge> edges, HashSet<IDotsHalfEdge> halfEdges, HashSet<IDotsFace> dotsFaces)
        {
            float maximalArea = 0.0f;

            foreach (IDotsVertex a in dots)
            {
                var dotsVertices = new List<IDotsVertex> {dotsVertex1, dotsVertex2};
                foreach (IDotsVertex b in dotsVertices)
                {
                    if (a.Equals(dotsVertex1) || a.Equals(dotsVertex2)) continue;

                    if (HelperFunctions.EdgeIsPossible(a, b, edges, dotsFaces))
                    {
                        List<IDotsVertex> disabled = new List<IDotsVertex>();
                        (IDotsFace face1, IDotsFace face2) = HelperFunctions.AddEdge(
                            a,
                            b,
                            Convert.ToInt32(PlayerNumber),
                            halfEdges,
                            dots,
                            GameMode,
                            newlyDisabled: disabled
                        );
                        float area = face1?.AreaMinusInner ?? 0.0f + face2?.AreaMinusInner ?? 0.0f;
                        if (area > maximalArea)
                        {
                            maximalArea = area;
                        }

                        CleanUp(halfEdges, a, b, face1, face2, disabled);
                    }
                }
            }

            return maximalArea;
        }

        private static void CleanUp(HashSet<IDotsHalfEdge> halfEdges, IDotsVertex a, IDotsVertex b,
            IDotsFace created1, IDotsFace created2, List<IDotsVertex> disabled)
        {
            IDotsHalfEdge toRemove = a.LeavingHalfEdges()
                .FirstOrDefault(it => it.Destination.Equals(b));
            disabled.ForEach(it => it.InFace = false);
            created1?.OuterComponentHalfEdges.ForEach(it => it.IncidentFace = null);
            created2?.OuterComponentHalfEdges.ForEach(it => it.IncidentFace = null);
            HelperFunctions.RemoveFromDCEL(toRemove);
            halfEdges.Remove(toRemove);
            halfEdges.Remove(toRemove.Twin);
        }

        public override (IDotsVertex, IDotsVertex) NextMove(HashSet<IDotsEdge> edges,
            HashSet<IDotsHalfEdge> halfEdges,
            HashSet<IDotsFace> faces, IEnumerable<IDotsVertex> vertices)
        {
            IDotsVertex[] verticesArray = vertices.ToArray();

            HelperFunctions.print("Calculating next minimal move for greedy player");
            PotentialMove potentialMove = MinimalMove(0, verticesArray.Length, verticesArray, edges, halfEdges, faces);

            HelperFunctions.print($"PotentialMove: {potentialMove}");

            return (potentialMove.A, potentialMove.B);
        }
    }
}