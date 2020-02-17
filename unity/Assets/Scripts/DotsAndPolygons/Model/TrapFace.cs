using System.Collections.Generic;
using UnityEngine;
using Util.Geometry;

namespace DotsAndPolygons
{
    public class TrapFace : ITrapDecomNode
    {
        public ITrapDecomNode LeftChild { get; set; }
        public ITrapDecomNode RightChild { get; set; }
        public List<LineSegment> LineSegments { get; private set; }
        public LineSegmentWithDotsEdge Upper { get; private set; }
        public LineSegmentWithDotsEdge Downer { get; private set; }
        public LineSegment Left { get; private set; }
        public LineSegment Right { get; private set; }
        public List<TrapFace> RightNeighBours { get; set; }
        public List<TrapFace> LeftNeighbours { get; set; }
        public Vector2 Leftpoint { get; private set; }
        public Vector2 Rightpoint { get; private set; }
        public List<ITrapDecomNode> Parents { get; set; }

        public override string ToString() => $"Trapface line segments: [{string.Join(", ", LineSegments)}]";

        public TrapFace(LineSegmentWithDotsEdge upper, LineSegmentWithDotsEdge downer, LineSegment left,
            LineSegment right, Vector2 leftpoint, Vector2 rightpoint)
        {
            Upper = upper;
            Downer = downer;
            Left = left;
            Right = right;

            LineSegments = new List<LineSegment>
            {
                upper.Segment,
                downer.Segment,
                left,
                right
            };

            RightNeighBours = new List<TrapFace>();
            LeftNeighbours = new List<TrapFace>();
            Leftpoint = leftpoint;
            Rightpoint = rightpoint;

            Parents = new List<ITrapDecomNode>();
        }

        public ITrapDecomNode query(IDotsVertex queryPoint)
        {
            return this;
        }

        public void Update(ITrapDecomNode newNode)
        {
            foreach (ITrapDecomNode parent in Parents)
            {
                if (parent.LeftChild.Equals(this))
                {
                    parent.LeftChild = newNode;
                }
                else
                {
                    parent.RightChild = newNode;
                }
            }
        }

        public void AddParent(ITrapDecomNode newParent)
        {
            Parents.Add(newParent);
        }

        public List<TrapFace> FindAllFaces() => new List<TrapFace> {this};
    }
}