using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Util.Algorithms.Polygon;
using Util.Geometry.Polygon;
using Util.Math;

namespace ArtGallery
{
    public static class NaiveLighthouseToLighthouseVisibility
    {

        public static List<Vector2> VisibleVertices(
            Polygon2D polygon,
            Vector2 vertex)
        {
            List<Vector2> result = new List<Vector2>();
            // calculate new visibility polygon
            var vision = Visibility.Vision(polygon, vertex);

            if (vision == null)
            {
                throw new Exception("Vision polygon cannot be null");
            }

            //check if the level polygon and the visibility polygon are given 
            // in the same direction. If not, reverse the vision polygon to
            // match the direction of the level polygon
            if (polygon.IsClockwise() != vision.IsClockwise())
            {
                vision.Reverse();
            }

            for (int i = 0; i < polygon.VertexCount; i++)
            {
                for (int j = 0; j < vision.VertexCount; j++)
                {
                    var pVertex = polygon.Vertices.ElementAt(i);
                    var vVertex = vision.Vertices.ElementAt(j);
                    bool areEqual = pVertex.EqualsEps(vVertex);

                    if (areEqual)
                    {
                        result.Add(pVertex);

                        break;
                    }
                }
            }
            
           

            return result;
        }

        public static List<List<Vector2>> VisibleVertices(
            Polygon2D polygon,
            List<Vector2> vertices)
        {
            List<List<Vector2>> result = new List<List<Vector2>>();

            foreach (Vector2 vertex in vertices)
            {
                result.Add(VisibleVertices(polygon, vertex));    
            }

            return result;
        }
    }
}
