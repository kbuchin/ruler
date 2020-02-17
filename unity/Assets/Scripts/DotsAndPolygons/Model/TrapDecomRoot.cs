using System.Collections.Generic;


namespace DotsAndPolygons
{
    public class TrapDecomRoot : ITrapDecomNode
    {
        
        public TrapDecomRoot(TrapFace frame)
        {
            LeftChild = frame;
            frame.AddParent(this);
        }
        public ITrapDecomNode query(IDotsVertex queryPoint) => LeftChild.query(queryPoint);

        public List<TrapFace> FindAllFaces() => LeftChild.FindAllFaces();

        public ITrapDecomNode LeftChild { get; set; }
        public ITrapDecomNode RightChild { get; set; }
    }
}