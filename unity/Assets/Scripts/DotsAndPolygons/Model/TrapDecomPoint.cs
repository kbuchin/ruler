﻿using System.Collections.Generic;
using UnityEngine;
using Util.Geometry;

namespace DotsAndPolygons
{
    public class TrapDecomPoint : ITrapDecomNode
    {
        public TrapDecomPoint(Vector2 splitPoint)
        {
            this.splitPoint = splitPoint;
        }

        public Vector2 splitPoint { get; set; }

        public ITrapDecomNode LeftChild { get; set; }

        public ITrapDecomNode RightChild { get; set; }

        public ITrapDecomNode query(DotsVertex queryPoint)
        {
            return queryPoint.Coordinates.x < splitPoint.x ? LeftChild.query(queryPoint) : RightChild.query(queryPoint);
        }

        public ITrapDecomNode Insert(LineSegment newNode)
        {
            throw new System.NotImplementedException();
        }

        public List<TrapFace> FindAllFaces()
        {
            List<TrapFace> leftChildFaces = LeftChild.FindAllFaces();
            List<TrapFace> rightChildFaces = RightChild.FindAllFaces();
            leftChildFaces.AddRange(rightChildFaces);
            return leftChildFaces;
        }
    }
}