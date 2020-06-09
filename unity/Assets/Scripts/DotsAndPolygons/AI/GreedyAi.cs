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

        public PotentialMove MinimalMove(int start, int length, DotsVertex[] vertices, HashSet<DotsEdge> edges,
            HashSet<DotsHalfEdge> halfEdges, HashSet<DotsFace> dotsFaces)
        {
            int end = start + length;
            float minimalWeight = float.MaxValue;
            float maximalArea = 0.0f;
            DotsVertex minA = null;
            DotsVertex minB = null;
            DotsVertex maxAreaA = null;
            DotsVertex maxAreaB = null;
            // bool claimPossible = false;
            PotentialMove result;

            // first calculate possible max area
            for (int i = start; i < end - 1; i++)
            {
                for (int j = i + 1; j < end; j++)
                {
                    // MonoBehaviour.print($"Calculating maximal area move, iteration {i}, {j}");
                    DotsVertex a = vertices[i];
                    DotsVertex b = vertices[j];

                    if (HelperFunctions.EdgeIsPossible(a, b, edges, dotsFaces))
                    {
                        List<DotsVertex> disabled = new List<DotsVertex>();
                        (DotsFace face1, DotsFace face2) = HelperFunctions.AddEdge(a, b,
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
                        DotsVertex a = vertices[i];
                        DotsVertex b = vertices[j];

                        if (HelperFunctions.EdgeIsPossible(a, b, edges, dotsFaces))
                        {
                            List<DotsVertex> disabled = new List<DotsVertex>();
                            (DotsFace face1, DotsFace face2) = HelperFunctions.AddEdge(a, b,
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

        private float CalculateWeight(DotsVertex dotsVertex1, DotsVertex dotsVertex2, DotsVertex[] dots,
            IEnumerable<DotsEdge> edges, HashSet<DotsHalfEdge> halfEdges, HashSet<DotsFace> dotsFaces)
        {
            float maximalArea = 0.0f;

            foreach (DotsVertex a in dots)
            {
                var dotsVertices = new List<DotsVertex> {dotsVertex1, dotsVertex2};
                foreach (DotsVertex b in dotsVertices)
                {
                    if (a.Equals(dotsVertex1) || a.Equals(dotsVertex2)) continue;

                    if (HelperFunctions.EdgeIsPossible(a, b, edges, dotsFaces))
                    {
                        List<DotsVertex> disabled = new List<DotsVertex>();
                        (DotsFace face1, DotsFace face2) = HelperFunctions.AddEdge(
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

        private static void CleanUp(HashSet<DotsHalfEdge> halfEdges, DotsVertex a, DotsVertex b,
            DotsFace created1, DotsFace created2, List<DotsVertex> disabled)
        {
            DotsHalfEdge toRemove = a.LeavingHalfEdges()
                .FirstOrDefault(it => it.Destination.Equals(b));
            disabled.ForEach(it => it.InFace = false);
            created1?.OuterComponentHalfEdges.ForEach(it => it.IncidentFace = null);
            created2?.OuterComponentHalfEdges.ForEach(it => it.IncidentFace = null);
            HelperFunctions.RemoveFromDCEL(toRemove);
            halfEdges.Remove(toRemove);
            halfEdges.Remove(toRemove.Twin);
        }

        public override List<PotentialMove> NextMove(
            HashSet<DotsEdge> edges,
            HashSet<DotsHalfEdge> halfEdges,
            HashSet<DotsFace> faces, 
            HashSet<DotsVertex> vertices)
        {
            HelperFunctions.print("Calculating next minimal move for greedy player");
            PotentialMove potentialMove = MinimalMove(0, vertices.Count, vertices.ToArray(), edges, halfEdges, faces);

            HelperFunctions.print($"PotentialMove: {potentialMove}");
            List<PotentialMove> path = new List<PotentialMove>();
            path.Add(potentialMove);
            return path;
        }
    }
}