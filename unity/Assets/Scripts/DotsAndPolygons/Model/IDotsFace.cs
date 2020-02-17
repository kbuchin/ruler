using System.Collections.Generic;
using UnityEngine;

namespace DotsAndPolygons
{
    public interface IDotsFace
    {
        // A half-edge of the outer cycle
        IDotsHalfEdge OuterComponent { get; set; }
        
        // Get list of all outer cycle half edges
        IEnumerable<IDotsHalfEdge> OuterComponentHalfEdges { get; }
        
        // Get list of all outer cycle vertices
        IEnumerable<IDotsVertex> OuterComponentVertices { get; }
        
        // List of half-edges for the inner cycles bounding the face
        List<IDotsHalfEdge> InnerComponents { get; set; }
        
        // Integer representing which player this face belongs to
        int Player { get; set; }

        // The area of this face
        float Area { get; set; }
        
        // The area of this face minus inner components
        float AreaMinusInner { get; }

        // Can be used both in the UnityDotsFace and in DotsFace
        void Constructor(
            IDotsHalfEdge outerComponent,
            List<IDotsHalfEdge> innerComponents = null,
            List<Vector2> testVertices = null);
        
        string ToString();
    }
}