using System.Collections.Generic;

namespace DotsAndPolygons
{
    public interface ITrapDecomNode
    {
        ITrapDecomNode query(DotsVertex queryPoint);

        ITrapDecomNode LeftChild { get; set; }

        ITrapDecomNode RightChild { get; set; }

        List<TrapFace> FindAllFaces();
    }
}