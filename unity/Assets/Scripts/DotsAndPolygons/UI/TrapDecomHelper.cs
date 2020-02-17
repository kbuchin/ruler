using System;
using DotsAndPolygons;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Util.Geometry;
using static DotsAndPolygons.HelperFunctions;

public class TrapDecomHelper
{
    /// <summary>
    /// Extracts the left point of a line
    /// </summary>
    /// <param name="line"></param>
    /// <returns>The point that is the left of the line</returns>
    public static Vector2 ExtractLeft(LineSegment line)
    {
        return line.Point1.x < line.Point2.x ? line.Point1 : line.Point2;
    }

    /// <summary>
    /// Extracts the right point of a line
    /// </summary>
    /// <param name="line"></param>
    /// <returns>The point that is th right point of the line</returns>
    public static Vector2 ExtractRight(LineSegment line)
    {
        return line.Point2.x < line.Point1.x ? line.Point1 : line.Point2;
    }

    private static void setNeighboursOtherCases(TrapFace currentOldLeftFace, TrapFace currentOldRightFace,
        TrapFace newFace, IReadOnlyCollection<TrapFace> trapFaces)
    {
        // setting left neighbours
        foreach (TrapFace leftNeighOldFace in currentOldLeftFace.LeftNeighbours)
        {
            FloatInterval interval = leftNeighOldFace.Right.YInterval.Intersect(newFace.Left.YInterval);
            int updater =
                leftNeighOldFace.RightNeighBours.FindIndex(rightNeigh => rightNeigh.Equals(currentOldLeftFace));
            if (updater != -1 && Math.Abs(leftNeighOldFace.Rightpoint.x - newFace.Leftpoint.x) < TOLERANCE &&
                interval != null && Math.Abs(interval.Min - interval.Max) > TOLERANCE)
            {
                leftNeighOldFace.RightNeighBours[updater] = newFace;
                newFace.LeftNeighbours.Add(leftNeighOldFace);
            }
        }

        // setting right neighbours
        foreach (TrapFace rightNeighOldFace in currentOldRightFace.RightNeighBours)
        {
            FloatInterval interval = rightNeighOldFace.Left.YInterval.Intersect(newFace.Right.YInterval);
            int updater =
                rightNeighOldFace.LeftNeighbours.FindIndex(leftNeigh => leftNeigh.Equals(currentOldRightFace));
            if (updater != -1 && Math.Abs(rightNeighOldFace.Leftpoint.x - newFace.Rightpoint.x) < TOLERANCE &&
                interval != null && Math.Abs(interval.Min - interval.Max) > TOLERANCE)
            {
                rightNeighOldFace.LeftNeighbours[updater] = newFace;
                newFace.RightNeighBours.Add(rightNeighOldFace);
            }
        }

        if (trapFaces.Any())
        {
            if (!trapFaces.Last().RightNeighBours.Contains(newFace))
                trapFaces.Last().RightNeighBours.Add(newFace);

            if (!newFace.LeftNeighbours.Contains(trapFaces.Last()))
                newFace.LeftNeighbours.Add(trapFaces.Last());
        }
    }

    static DotsVertex movedEndPoint(Vector2 left, Vector2 right, bool isLeftPoint)
    {
        Vector2 diffVec;
        if (isLeftPoint)
            diffVec = right - left;
        else
            diffVec = left - right;

        diffVec = diffVec.normalized;
        diffVec.Scale(new Vector2(0.01f, 0.01f));

        if (isLeftPoint)
            return new DotsVertex(left + diffVec);
        else
            return new DotsVertex(right + diffVec);
    }

    private static void setNeigboursBaseCase(TrapFace neighbour, TrapFace newFace, bool left)
    {
        if (left)
        {
            newFace.LeftNeighbours = neighbour.LeftNeighbours;
            foreach (TrapFace neigh in neighbour.LeftNeighbours)
            {
                int k = neigh.RightNeighBours.IndexOf(neighbour);
                if (k != -1)
                {
                    neigh.RightNeighBours[k] = newFace;
                }
            }
        }
        else
        {
            foreach (TrapFace neigh in neighbour.RightNeighBours)
            {
                int k = neigh.LeftNeighbours.IndexOf(neighbour);
                if (k != -1)
                {
                    neigh.LeftNeighbours[k] = newFace;
                }
            }
        }
    }

    private static TrapFace ExtractFace(Vector2 orientationHelper1, Vector2 orientationHelper2, LineSegment upperline,
        LineSegment lowerline, IDotsEdge upperDotsEdge, IDotsEdge lowerDotsEdge)
    {
        var leftLowerPoint = new Vector2(orientationHelper1.x, lowerline.Y(orientationHelper1.x));
        var leftUpperPoint = new Vector2(orientationHelper1.x, upperline.Y(orientationHelper1.x));
        var rightUpperPoint = new Vector2(orientationHelper2.x, upperline.Y(orientationHelper2.x));
        var rightLowerPoint = new Vector2(orientationHelper2.x, lowerline.Y(orientationHelper2.x));

        var leftLine = new LineSegment(leftLowerPoint, leftUpperPoint);

        var upperLine = new LineSegmentWithDotsEdge(leftUpperPoint, rightUpperPoint, upperDotsEdge);
        var rightLine = new LineSegment(rightUpperPoint, rightLowerPoint);
        var downerLine = new LineSegmentWithDotsEdge(leftLowerPoint, rightLowerPoint, lowerDotsEdge);

        return new TrapFace(upperLine, downerLine, leftLine, rightLine, orientationHelper1, orientationHelper2);
    }

    private static int UpdateLowerUpper(int current, float xA, float xB)
    {
        return xA >= xB ? current + 1 : current;
    }

    /// <summary>
    /// Insert new edge inside the trapzoidal decomposition tree.
    /// </summary>
    /// <param name="root">The current root node of the tree</param>
    /// <param name="newEdge">The new edge that needs to be inserted</param>
    /// <returns></returns>
    public static List<TrapFace> Insert(ITrapDecomNode root, IDotsEdge newEdge)
    {
        // Inititialize helper variables for inserting

        // Linesegment that corresponds to the new line that is inserted i.e. the new edge drawn by the player
        LineSegment newline = newEdge.Segment;

        Vector2 left = ExtractLeft(newline);
        Vector2 right = ExtractRight(newline);

        // The current faces where the left and right point of the new line lie in
        TrapFace faceLeft = (TrapFace) root.query(movedEndPoint(left, right, true));
        TrapFace faceRight = (TrapFace) root.query(movedEndPoint(left, right, false));

        // find all right neighbours of faceLeft these faces need to be split up
        List<ITrapDecomNode> neighbours = new List<ITrapDecomNode>();
        neighbours = ExtractNeighbours(faceLeft, faceRight, new List<ITrapDecomNode>(), newline);

        Vector2 bottomleft = left;
        Vector2 upperleft = left;

        // upperfaces and lowerfaces lists that contains all the new faces that are create above and below the new line respectively.
        // if a new face crosses the new line it is added to both lists
        List<TrapFace> upperFaces = new List<TrapFace>();
        List<TrapFace> lowerFaces = new List<TrapFace>();

        TrapFace upperLeftFace = faceLeft;
        TrapFace bottomLeftFace = faceLeft;

        var i = 0;
        foreach (TrapFace neighbour in neighbours.Cast<TrapFace>())
        {
            // case 1: draw left face for when new startpoint lies in a face. (So not equal)
            if (neighbour.Equals(faceLeft) && !left.Equals(neighbour.Leftpoint))
            {
                TrapFace newLeftFace = ExtractFace(neighbour.Leftpoint, left, neighbour.Upper.Segment,
                    neighbour.Downer.Segment, null, null);
                setNeigboursBaseCase(neighbour, newLeftFace, true);
                lowerFaces.Add(newLeftFace);
                upperFaces.Add(newLeftFace);
                upperleft = left;
                bottomleft = left;
            }

            // Case 2: rightpoint of newline fits inside face (so not equal)
            if (neighbour.Equals(faceRight))
            {
                TrapFace newUpperFace = ExtractFace(upperleft, right, neighbour.Upper.Segment, newline,
                    neighbour.Upper.DotsEdge, newEdge);
                setNeighboursOtherCases(upperLeftFace, neighbour, newUpperFace, upperFaces);
                upperFaces.Add(newUpperFace);

                TrapFace newDownFace = ExtractFace(bottomleft, right, newline, neighbour.Downer.Segment, newEdge,
                    neighbour.Downer.DotsEdge);
                setNeighboursOtherCases(bottomLeftFace, neighbour, newDownFace, lowerFaces);
                lowerFaces.Add(newDownFace);

                if (!right.x.Equals(neighbour.Rightpoint.x))
                {
                    TrapFace newUpperF = ExtractFace(right, neighbour.Rightpoint, neighbour.Upper.Segment,
                        neighbour.Downer.Segment, null, null);
                    newUpperF.LeftNeighbours.Add(upperFaces.Last());
                    newUpperF.LeftNeighbours.Add(lowerFaces.Last());
                    upperFaces.Last().RightNeighBours.Add(newUpperF);
                    lowerFaces.Last().RightNeighBours.Insert(0, newUpperF);
                    newUpperF.RightNeighBours = neighbour.RightNeighBours;
                    setNeigboursBaseCase(neighbour, newUpperF, false);
                    lowerFaces.Add(newUpperF);
                    upperFaces.Add(newUpperF);
                }
            }
            else
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (HelperFunctions.TurnDirection(left, right, neighbour.Rightpoint) == 1.0f)
                {
                    TrapFace newFace = ExtractFace(bottomleft, neighbour.Rightpoint, newline, neighbour.Downer.Segment,
                        newEdge, neighbour.Downer.DotsEdge);
                    setNeighboursOtherCases(bottomLeftFace, neighbour, newFace, lowerFaces);
                    lowerFaces.Add(newFace);
                    bottomleft = neighbour.Rightpoint;
                    bottomLeftFace = (TrapFace) (i + 1 < neighbours.Count ? neighbours[i + 1] : faceRight);
                }
                else
                {
                    TrapFace newFace = ExtractFace(upperleft, neighbour.Rightpoint, neighbour.Upper.Segment, newline,
                        neighbour.Upper.DotsEdge, newEdge);
                    // ExtractUpperNeighbour(upperleft, newEdge, neighbour);
                    setNeighboursOtherCases(upperLeftFace, neighbour, newFace, upperFaces);
                    upperFaces.Add(newFace);
                    upperleft = neighbour.Rightpoint;
                    upperLeftFace = (TrapFace) (i + 1 < neighbours.Count ? neighbours[i + 1] : faceRight);
                }
            }

            i++;
        }


        // Refactoring so far so good
        // Update data structure
        var i_lower = 0;
        var i_upper = 0;


        if (faceLeft.Equals(faceRight) && upperFaces.Count == 3)
        {
            // root is bounding box
            ITrapDecomNode leftpointnode = new TrapDecomPoint(left);
            upperFaces[0].AddParent(leftpointnode);
            leftpointnode.LeftChild = upperFaces[0];
            ITrapDecomNode rightpointnode = new TrapDecomPoint(right);
            upperFaces[2].AddParent(rightpointnode);
            rightpointnode.RightChild = upperFaces[2];
            leftpointnode.RightChild = rightpointnode;
            ITrapDecomNode newlineNode = new TrapDecomLine(newline);
            newlineNode.LeftChild = lowerFaces[1];
            newlineNode.RightChild = upperFaces[1];
            upperFaces[1].AddParent(newlineNode);
            lowerFaces[1].AddParent(newlineNode);
            rightpointnode.LeftChild = newlineNode;
            ((TrapFace) neighbours[0]).Update(leftpointnode);
        }
        else
        {
            var start = 0;
            // leftface left of leftpoint of new line
            if (upperFaces[i_upper].Equals(lowerFaces[i_lower]))
            {
                start = 1;

                ITrapDecomNode point = new TrapDecomPoint(left);
                point.LeftChild = upperFaces[i_upper];
                upperFaces[i_upper].AddParent(point);
                ITrapDecomNode newlineNode = new TrapDecomLine(newline);
                point.RightChild = newlineNode;

                i_upper++;
                i_lower++;

                newlineNode.RightChild = upperFaces[i_upper];
                newlineNode.LeftChild = lowerFaces[i_lower];
                upperFaces[i_upper].AddParent(newlineNode);
                lowerFaces[i_lower].AddParent(newlineNode);

                ((TrapFace) neighbours[0]).Update(point);

                i_upper = UpdateLowerUpper(i_upper, ((TrapFace) neighbours[0]).Rightpoint.x,
                    upperFaces[i_upper].Rightpoint.x);
                i_lower = UpdateLowerUpper(i_lower, ((TrapFace) neighbours[0]).Rightpoint.x,
                    lowerFaces[i_lower].Rightpoint.x);
            }
            else
            {
                start = 0;
            }

            // all the in between
            for (int j = start; j < neighbours.Count - 1; j++)
            {
                ITrapDecomNode newlineNode = new TrapDecomLine(newline);

                var neigh = (TrapFace) neighbours[j];

                newlineNode.RightChild = upperFaces[i_upper];
                newlineNode.LeftChild = lowerFaces[i_lower];

                upperFaces[i_upper].AddParent(newlineNode);
                lowerFaces[i_lower].AddParent(newlineNode);

                ((TrapFace) neighbours[j]).Update(newlineNode);

                i_upper = UpdateLowerUpper(i_upper, neigh.Rightpoint.x, upperFaces[i_upper].Rightpoint.x);
                i_lower = UpdateLowerUpper(i_lower, neigh.Rightpoint.x, lowerFaces[i_lower].Rightpoint.x);
            }

            // right point lies in face and face lies right of right point
            if (upperFaces.Last().Equals(lowerFaces.Last()))
            {
                ITrapDecomNode point = new TrapDecomPoint(right);
                point.RightChild = upperFaces.Last();
                ((TrapFace) point.RightChild).AddParent(point);

                ITrapDecomNode newlineNode = new TrapDecomLine(newline);
                point.LeftChild = newlineNode;


                newlineNode.RightChild = upperFaces[i_upper];
                newlineNode.LeftChild = lowerFaces[i_lower];
                upperFaces[i_upper].AddParent(newlineNode);
                lowerFaces[i_lower].AddParent(newlineNode);

                ((TrapFace) neighbours[neighbours.Count - 1]).Update(point);
            }
            else
            {
                if (i_upper < upperFaces.Count)
                {
                    ITrapDecomNode newlineNode = new TrapDecomLine(newline);

                    newlineNode.RightChild = upperFaces[i_upper];
                    newlineNode.LeftChild = lowerFaces[i_lower];

                    upperFaces[i_upper].AddParent(newlineNode);
                    lowerFaces[i_lower].AddParent(newlineNode);

                    ((TrapFace) neighbours[neighbours.Count - 1]).Update(newlineNode);
                }
            }
        }

        upperFaces.AddRange(lowerFaces);
        return upperFaces;
    }

    public static List<ITrapDecomNode> ExtractNeighbours(TrapFace start, TrapFace end, List<ITrapDecomNode> result,
        LineSegment newLine)
    {
        if (start.Equals(end))
        {
            result.Add(end);
            return result;
        }

        TrapFace neighbour = start.RightNeighBours.FirstOrDefault(face => face.LineSegments.Any(
            line => HelperFunctions.InterSEGting(newLine, line)));
        result.Add(start);
        return ExtractNeighbours(neighbour, end, result, newLine);
    }
}