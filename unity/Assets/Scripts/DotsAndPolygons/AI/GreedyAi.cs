using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util.Geometry;

namespace DotsAndPolygons
{
    public class GreedyAi : AiPlayer
    {
        public GreedyAi(PlayerNumber player, HelperFunctions.GameMode mode) : base(player, PlayerType.GreedyAi, mode)
        {
            // todo fix check if ai can make a face
        }

        public PotentialMove MinimalMove(int start, int length, IDotsVertex[] vertices, IEnumerable<IDotsEdge> edges,
            IEnumerable<IDotsHalfEdge> halfEdges, HashSet<IDotsFace> dotsFaces)
        {
            int end = start + length;
            float minimalWeight = float.MaxValue;
            float maximalArea = 0.0f;
            IDotsVertex minA = null;
            IDotsVertex minB = null;
            IDotsVertex maxAreaA = null;
            IDotsVertex maxAreaB = null;
            bool claimPossible = false;
            PotentialMove result;
            for (int i = start; i < end - 1; i++)
            {
                for (int j = i + 1; j < end; j++)
                {
                    MonoBehaviour.print($"Calculating minimal move, iteration {i}, {j}");
                    IDotsVertex a = vertices[i];
                    IDotsVertex b = vertices[j];
                    if (a.Equals(b)) continue;

                    if (HelperFunctions.EdgeIsPossible(a, b, edges, dotsFaces))
                    {
                        List<IDotsVertex> newVertices = vertices.Select(it => it.Clone()).ToList();
                        IDotsVertex newA = newVertices[i];
                        IDotsVertex newB = newVertices[j];

                        var newDotsFaces = new HashSet<IDotsFace>(dotsFaces); // TODO maybe clone
                        var newHalfEdges = new HashSet<IDotsHalfEdge>(halfEdges.Select(it => it.Clone()));

                        float area = HelperFunctions.AddEdge(newA, newB, Convert.ToInt32(PlayerNumber),
                            newHalfEdges, newVertices, GameMode, dotsFaces: newDotsFaces);
                        if (area > maximalArea)
                        {
                            maximalArea = area;
                            maxAreaA = a;
                            maxAreaB = b;
                            claimPossible = true;
                        }

                        if (!claimPossible)
                        {
                            var newEdges = new HashSet<IDotsEdge>(edges)
                            {
                                new DotsEdge(new LineSegment(newA.Coordinates, newB.Coordinates))
                            };

                            float weight = CalculateWeight(newA, newB, newVertices, newEdges, newHalfEdges,
                                newDotsFaces);
                            if (weight < minimalWeight)
                            {
                                minA = a;
                                minB = b;
                                minimalWeight = weight;
                            }
                        }
                    }
                }
            }

            result = claimPossible
                ? new AreaMove(maximalArea, maxAreaA, maxAreaB)
                : (PotentialMove) new WeightMove(minimalWeight, minA, minB);
            return result;
        }

        private float CalculateWeight(IDotsVertex dotsVertex1, IDotsVertex dotsVertex2, List<IDotsVertex> dots,
            IEnumerable<IDotsEdge> edges, HashSet<IDotsHalfEdge> halfEdges, HashSet<IDotsFace> dotsFaces)
        {
            float maximalArea = 0.0f;

            for (var i = 0; i < dots.Count; i++)
            {
                IEnumerable<int> dotsVertices = new List<IDotsVertex> {dotsVertex1, dotsVertex2}
                    .Select(it => dots.IndexOf(it));
                foreach (int j in dotsVertices)
                {
                    IDotsVertex a = dots[j];
                    IDotsVertex b = dots[i];
                    if (b.Equals(dotsVertex1) || b.Equals(dotsVertex2)) continue;

                    if (HelperFunctions.EdgeIsPossible(a, b, edges, dotsFaces))
                    {
                        List<IDotsVertex> newDots = dots.Select(it => it.Clone()).ToList();
                        IDotsVertex newA = newDots[j];
                        IDotsVertex newB = newDots[i];

                        float area = HelperFunctions.AddEdge(
                            newA,
                            newB,
                            Convert.ToInt32(PlayerNumber),
                            new HashSet<IDotsHalfEdge>(
                                halfEdges.Select(it => it.Clone())
                            ),
                            newDots,
                            GameMode
                        );
                        if (area > maximalArea)
                        {
                            maximalArea = area;
                        }
                    }
                }
            }

            return maximalArea;
        }

        public override (IDotsVertex, IDotsVertex) NextMove(IEnumerable<IDotsEdge> edges,
            IEnumerable<IDotsHalfEdge> halfEdges,
            HashSet<IDotsFace> faces, IEnumerable<IDotsVertex> vertices)
        {
            IDotsVertex[] verticesArray = vertices.ToArray();

            MonoBehaviour.print("Calculating next minimal move for greedy player");
            PotentialMove potentialMove = MinimalMove(0, verticesArray.Length, verticesArray, edges, halfEdges, faces);

            MonoBehaviour.print($"PotentialMove: {potentialMove}");

            return (potentialMove.A, potentialMove.B);
        }
    }
}