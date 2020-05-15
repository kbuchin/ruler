using DotsAndPolygons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.TestTools.Utils;
using Util.Geometry;

namespace Assets.Scripts.DotsAndPolygons.AI
{
    public class GreedyAi : AiPlayer
    {
        public GreedyAi(PlayerNumber player, HelperFunctions.GameMode mode) : base(player, PlayerType.GreedyAi, mode)
        {
            // todo fix check if ai can make a face
        }

        public PotentialMove MinimalMove(int start, int length, 
            IDotsVertex[] vertices, IEnumerable<IDotsEdge> edges, IEnumerable<IDotsHalfEdge> halfEdges, IEnumerable<IDotsFace> dotsFaces)
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
            for(int i = start; i < end - 1; i++)
            {
                for(int j = i + 1; j < end; j++)
                {
                    IDotsVertex a = vertices[i];
                    IDotsVertex b = vertices[j];
                    if (HelperFunctions.EdgeIsPossible(a,b,edges, dotsFaces))
                    {
                        float area = HelperFunctions.AddEdge(a, b, Convert.ToInt32(this.PlayerNumber), 
                                new HashSet<IDotsHalfEdge>(halfEdges), vertices.ToList(), this.GameMode);
                        if(area > maximalArea)
                        {
                            maximalArea = area;
                            maxAreaA = a;
                            maxAreaB = b;
                            claimPossible = true;
                            
                        }
                        if(!claimPossible)
                        {
                            float weight = CalculateWeight(a, b, vertices);
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
            result = claimPossible ? new AreaMove(maximalArea, maxAreaA, maxAreaB) : (PotentialMove)new WeightMove(minimalWeight, minA, minB);
            return result;
        }

        private float CalculateWeight(IDotsVertex dotsVertex1, IDotsVertex dotsVertex2, IDotsVertex[] dots)
        {
            for(int i  = 0; i < dots.Length; i++)
            {
                
            }
            return 10.0f;
        }

        public (IDotsVertex, IDotsVertex) NextMove(IEnumerable<IDotsEdge> edges, IEnumerable<IDotsVertex> vertices) 
        {
            IDotsVertex[] verticesArray = vertices.ToArray();

            return (null, null);
        }

        
    }
}
