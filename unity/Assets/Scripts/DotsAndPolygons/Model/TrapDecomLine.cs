﻿using Util.Geometry;
using UnityEngine;
using System.Collections.Generic;

namespace DotsAndPolygons
{
    public class TrapDecomLine : ITrapDecomNode
    {
        private IDotsVertex LeftPoint;

        private IDotsVertex RightPoint;

        //Below the line
        public ITrapDecomNode LeftChild { get; set; }

        // Above the line
        public ITrapDecomNode RightChild { get; set; }

        public TrapDecomLine(LineSegment segment)
        {
            LineSegment splitline = segment.Point1.x < segment.Point2.x
                ? segment
                : new LineSegment(new Vector2
                    {
                        x = segment.Point2.x,
                        y = segment.Point2.y
                    },
                    new Vector2
                    {
                        x = segment.Point1.x,
                        y = segment.Point1.y
                    });
            LeftPoint = new DotsVertex
            (
                splitline.Point1
            );
            RightPoint = new DotsVertex
            (
                splitline.Point2
            );
        }

        public ITrapDecomNode query(IDotsVertex queryPoint)
        {
            return ConvexHullHelper.makesRightTurn(LeftPoint, RightPoint, queryPoint)
                ? LeftChild.query(queryPoint)
                : RightChild.query(queryPoint);
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