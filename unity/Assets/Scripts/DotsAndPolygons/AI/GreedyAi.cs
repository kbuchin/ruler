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
            PotentialMove result = null;
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
                        var newDotsFaces = new HashSet<IDotsFace>(dotsFaces);
                        var newHalfEdges = new HashSet<IDotsHalfEdge>(halfEdges);
                        float area = HelperFunctions.AddEdge(a, b, Convert.ToInt32(this.PlayerNumber),
                            newHalfEdges, vertices.ToList(), this.GameMode, dotsFaces: newDotsFaces);
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
                                new DotsEdge(new LineSegment(a.Coordinates, b.Coordinates))
                            };

                            float weight = CalculateWeight(a, b, vertices, newEdges, newHalfEdges, newDotsFaces);
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

        private float CalculateWeight(IDotsVertex dotsVertex1, IDotsVertex dotsVertex2, IDotsVertex[] dots,
            IEnumerable<IDotsEdge> edges, HashSet<IDotsHalfEdge> halfEdges, HashSet<IDotsFace> dotsFaces)
        {
            float maximalArea = 0.0f;

            foreach (IDotsVertex b in dots)
            {
                foreach (IDotsVertex a in new List<IDotsVertex> {dotsVertex1, dotsVertex2})
                {
                    if (b.Equals(dotsVertex1) || b.Equals(dotsVertex2)) continue;
                    
                    if (HelperFunctions.EdgeIsPossible(a, b, edges, dotsFaces))
                    {
                        float area = HelperFunctions.AddEdge(a, b, Convert.ToInt32(this.PlayerNumber),
                            new HashSet<IDotsHalfEdge>(halfEdges), dots.ToList(), this.GameMode);
                        if (area > maximalArea)
                        {
                            maximalArea = area;
                        }
                    }
                }
            }

            return maximalArea;
        }

        public override (IDotsVertex, IDotsVertex) NextMove(IEnumerable<IDotsEdge> edges, IEnumerable<IDotsHalfEdge> halfEdges,
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