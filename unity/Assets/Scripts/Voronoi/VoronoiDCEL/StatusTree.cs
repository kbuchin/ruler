using System.Collections.Generic;
using System;

namespace VoronoiDCEL
{
    public sealed class StatusData<T> : IComparable<StatusData<T>>, IEquatable<StatusData<T>>
    {
        private readonly Edge<T> m_Edge;
        private readonly Vertex<T> m_SweeplineIntersectionPoint;
        private readonly Vertex<T> m_PointP;

        public Edge<T> Edge
        {
            get { return m_Edge; }
        }

        public Vertex<T> SweeplineIntersectionPoint
        {
            get { return m_SweeplineIntersectionPoint; }
        }

        public Vertex<T> PointP
        {
            get { return m_PointP; }
        }

        public StatusData(Edge<T> a_Edge)
        {
            m_Edge = a_Edge;
            m_SweeplineIntersectionPoint = Vertex<T>.Zero;
            m_PointP = Vertex<T>.Zero;
        }

        public StatusData(Edge<T> a_Edge, Vertex<T> a_PointP)
        {
            m_Edge = a_Edge;
            m_PointP = a_PointP;
            double sweeplineHeight = a_PointP.Y;
            if (!DCEL<T>.IntersectLines(a_Edge.UpperEndpoint, a_Edge.LowerEndpoint, new Vertex<T>(Math.Min(a_Edge.UpperEndpoint.X, a_Edge.LowerEndpoint.X) - 1, sweeplineHeight),
                    new Vertex<T>(Math.Max(a_Edge.UpperEndpoint.X, a_Edge.LowerEndpoint.X) + 1, sweeplineHeight), out m_SweeplineIntersectionPoint))
            {
                m_SweeplineIntersectionPoint = a_Edge.UpperEndpoint;
            }
        }

        public override bool Equals(object obj)
        {
            StatusData<T> statusData = obj as StatusData<T>;
            if (statusData != null)
            {
                return statusData.m_Edge == m_Edge;
            }
            else
            {
                return false;
            }
        }

        public bool Equals(StatusData<T> a_StatusData)
        {
            if (a_StatusData != null)
            {
                return a_StatusData.m_Edge == m_Edge;
            }
            else
            {
                return false;
            }
        }

        public int CompareTo(StatusData<T> a_StatusData)
        {
            return a_StatusData.m_Edge.UpperEndpoint.CompareTo(m_Edge) * -1;
        }

        public override int GetHashCode()
        {
            return m_Edge.GetHashCode();
        }
    }

    public sealed class StatusTree<T> : AATree<StatusData<T>>
    {
        public bool Insert(Edge<T> a_Edge, Vertex<T> a_PointP)
        {
            StatusData<T> statusData = new StatusData<T>(a_Edge, a_PointP);
            return Insert(statusData);
        }

        public bool Delete(Edge<T> a_Edge, Vertex<T> a_PointP)
        {
            StatusData<T> statusData = new StatusData<T>(a_Edge, a_PointP);
            return Delete(statusData);
        }

        public bool FindNextBiggest(Edge<T> a_Edge, Vertex<T> a_PointP, out Edge<T> out_NextBiggest)
        {
            StatusData<T> statusData = new StatusData<T>(a_Edge, a_PointP);
            StatusData<T> nextBiggest;
            if (FindNextBiggest(statusData, out nextBiggest))
            {
                out_NextBiggest = nextBiggest.Edge;
                return true;
            }
            out_NextBiggest = null;
            return false;
        }

        public bool FindNextSmallest(Edge<T> a_Edge, Vertex<T> a_PointP, out Edge<T> out_NextSmallest)
        {
            StatusData<T> statusData = new StatusData<T>(a_Edge, a_PointP);
            StatusData<T> nextSmallest;
            if (FindNextSmallest(statusData, out nextSmallest))
            {
                out_NextSmallest = nextSmallest.Edge;
                return true;
            }
            out_NextSmallest = null;
            return false;
        }

        public Edge<T>[] FindNodes(Vertex<T> v)
        {
            List<StatusData<T>> nodes = new List<StatusData<T>>();
            FindNodes(v, m_Tree, nodes);
            int nodeCount = nodes.Count;
            Edge<T>[] edges = new Edge<T>[nodeCount];
            for (int i = 0; i < nodeCount; ++i)
            {
                edges[i] = nodes[i].Edge;
            }
            return edges;
        }

        private void FindNodes(Vertex<T> v, Node t, List<StatusData<T>> list)
        {
            if (t == m_Bottom)
            {
                return;
            }
            else if (IsEqual(v, t.Data))
            {
                list.Add(t.Data);
                FindNodes(v, t.Left, list);
                FindNodes(v, t.Right, list);
            }
            else if (CompareTo(v, t.Data) < 0)
            {
                FindNodes(v, t.Left, list);
            }
            else
            {
                FindNodes(v, t.Right, list);
            }
        }

        protected override int CompareTo(StatusData<T> a, StatusData<T> b, COMPARISON_TYPE a_ComparisonType)
        {
            // If the edge we're comparing with is the one we're trying to delete or find, then return 0.
            if (a.Edge.LowerEndpoint.Equals(b.Edge.LowerEndpoint) &&
                a.Edge.UpperEndpoint.Equals(b.Edge.UpperEndpoint))
            {
                return 0;
            }

            // Is the edge we're inserting/deleting horizontal?
            if (a.Edge.IsHorizontal)
            {
                // Is the edge we're comparing with also horizontal?
                if (b.Edge.IsHorizontal)
                {
                    // First handle the non-overlapping cases.
                    if (a.Edge.LowerEndpoint.X <= b.Edge.UpperEndpoint.X)
                    {
                        return -1;
                    }
                    else if (b.Edge.LowerEndpoint.X <= a.Edge.UpperEndpoint.X)
                    {
                        return 1;
                    }
                    else
                    {
                        UnityEngine.Debug.Log("Found overlapping horizontal edges!");
                        UnityEngine.Debug.Log(a.Edge.ToString());
                        UnityEngine.Debug.Log(b.Edge.ToString());
                        UnityEngine.Debug.Log(a.SweeplineIntersectionPoint.ToString());
                        UnityEngine.Debug.Log(a_ComparisonType.ToString());
                        // Edges are overlapping.
                        if (Math.Abs(a.Edge.UpperEndpoint.X - b.Edge.UpperEndpoint.X) < Math.Exp(-9))
                        {
                            // With coinciding upper endpoints, let the lower endpoints decide.
                            return a.Edge.LowerEndpoint.X <= b.Edge.LowerEndpoint.X ? -1 : 1;
                        }
                        else
                        {
                            // Else just let the upper endpoints decide.
                            return a.Edge.UpperEndpoint.X < b.Edge.UpperEndpoint.X ? -1 : 1;
                        }
                    }
                }
                else
                {
                    // Edge b is not horizontal, but a is, so compare using a's lower endpoint.
                    // If the lower endpoint is to the left or on b then the whole of a is to the left.
                    // But if the lower endpoint is to the right of b, then either a is intersecting b
                    // or the whole of a is on the right of b. Horizontal segments always come last among segments
                    // that are intersecting p, so in both cases we can return that it's to the right.
                    if (a_ComparisonType != COMPARISON_TYPE.DELETE && a.PointP.CompareTo(b.Edge) == 0 && !b.Edge.LowerEndpoint.Equals(a.PointP) && !b.Edge.UpperEndpoint.Equals(a.PointP))
                    {
                        return 1;
                    }
                    if (a_ComparisonType == COMPARISON_TYPE.DELETE && a.Edge.LowerEndpoint.Equals(a.PointP) && a.Edge.LowerEndpoint.CompareTo(b.Edge) > 0)
                    {
                        return 1;
                    }
                    return a.Edge.UpperEndpoint.CompareTo(b.Edge) < 0 ? -1 : 1;
                }
            }
            // a is not horizontal. Is the edge we're comparing with horizontal?
            else if (b.Edge.IsHorizontal)
            {
                // If the point on a that is intersecting with the sweep line (a.Vertex) is
                // to the left of b's lower endpoint, then either we are on the left or intersecting.
                // In both cases we return left, because horizontal segments always come last.
                if (a_ComparisonType != COMPARISON_TYPE.DELETE && a.PointP.CompareTo(a.Edge) == 0 && !a.Edge.LowerEndpoint.Equals(a.PointP) && !a.Edge.UpperEndpoint.Equals(a.PointP))
                {
                    return -1;
                }
                return a.SweeplineIntersectionPoint.X <= b.Edge.UpperEndpoint.X ? -1 : 1;
            }
            else
            {
                // Both a and b are not horizontal. The easy case! Use a.Vertex (the point of a intersecting the sweep line)
                // and look on what side of b it is.
                int side = a.SweeplineIntersectionPoint.CompareTo(b.Edge);
                if (0 == side)
                {
                    // If we're deleting and the point p is a's lower endpoint, then we cannot use a's
                    // lower endpoint to resolve this case, so we must use the upper endpoint.
                    if (a.SweeplineIntersectionPoint.Equals(a.Edge.LowerEndpoint))
                    {
                        side = a.Edge.UpperEndpoint.CompareTo(b.Edge);
                    }
                    else
                    {
                        // a.Vertex was on b, so use a's lower endpoint to check on what side it is.
                        side = a.Edge.LowerEndpoint.CompareTo(b.Edge);
                    }
                    if (0 == side)
                    {
                        // a's lower endpoint is also on b, this should not occur in a planar subdivision!
                        UnityEngine.Debug.Log("Found overlapping edges!");
                        UnityEngine.Debug.Log(a.Edge.ToString());
                        UnityEngine.Debug.Log(b.Edge.ToString());
                        UnityEngine.Debug.Log(a.SweeplineIntersectionPoint.ToString());
                        UnityEngine.Debug.Log(a_ComparisonType.ToString());
                    }
                }
                return side;
            }
        }

        protected override bool IsEqual(StatusData<T> a, StatusData<T> b, COMPARISON_TYPE a_ComparisonType)
        {
            if (a_ComparisonType == COMPARISON_TYPE.INSERT)
            {
                return a.Edge.UpperEndpoint.OnLine(b.Edge) && a.Edge.LowerEndpoint.OnLine(b.Edge);
            }
            else if (a_ComparisonType == COMPARISON_TYPE.DELETE)
            {
                return a.Edge.Equals(b.Edge);
            }
            else
            {
                return a.Equals(b);
            }
        }

        private static bool IsEqual(Vertex<T> v, StatusData<T> b)
        {
            return b.Edge.UpperEndpoint == v || b.Edge.LowerEndpoint == v || v.OnLine(b.Edge);
        }

        private static int CompareTo(Vertex<T> v, StatusData<T> b)
        {
            return v.CompareTo(b.Edge);
        }

        public bool FindNeighboursOfPoint(Vertex<T> a_Point, out Edge<T> out_LeftNeighbour, out Edge<T> out_RightNeighbour)
        {
            if (m_Tree == m_Bottom || a_Point == null)
            {
                out_LeftNeighbour = null;
                out_RightNeighbour = null;
                return false;
            }
            else
            {
                Node currentNode = m_Tree;
                Node lastRight = m_Bottom;
                Node lastLeft = m_Bottom;
                while (true)
                {
                    int comparisonResult = a_Point.CompareTo(currentNode.Data.Edge);
                    Node nextNode;
                    if (comparisonResult < 0)
                    {
                        nextNode = currentNode.Left;
                        lastLeft = currentNode;
                    }
                    else
                    {
                        nextNode = currentNode.Right;
                        lastRight = currentNode;
                    }
                    if (nextNode != m_Bottom)
                    {
                        currentNode = nextNode;
                    }
                    else
                    {
                        break;
                    }
                }

                out_RightNeighbour = lastLeft != m_Bottom ? lastLeft.Data.Edge : null;
                out_LeftNeighbour = lastRight != m_Bottom ? lastRight.Data.Edge : null;
                return lastLeft != m_Bottom && lastRight != m_Bottom;
            }
        }

        public bool FindLeftMostSegmentInSet(HashSet<Edge<T>> a_Set, out Edge<T> out_Leftmost)
        {
            return FindLeftMostSegmentInSet(a_Set, m_Tree, out out_Leftmost);
        }

        public bool FindRightMostSegmentInSet(HashSet<Edge<T>> a_Set, out Edge<T> out_Rightmost)
        {
            return FindRightMostSegmentInSet(a_Set, m_Tree, out out_Rightmost);
        }

        private bool FindLeftMostSegmentInSet(HashSet<Edge<T>> a_Set, Node t, out Edge<T> out_LeftMost)
        {
            if (t == m_Bottom)
            {
                out_LeftMost = null;
                return false;
            }
            else
            {
                out_LeftMost = null;
                // Perform a DFS.
                int setCount = a_Set.Count;
                Stack<Node> nodes = new Stack<Node>(m_Size);
                Stack<int> nodesLevel = new Stack<int>(m_Size);
                nodes.Push(t);
                nodesLevel.Push(0);
                int lowestLevel = -1;
                while (nodes.Count != 0 && setCount != 0)
                {
                    int curLevel = nodesLevel.Pop();
                    if (curLevel <= lowestLevel && out_LeftMost != null)
                    {
                        return true;
                    }
                    Node curNode = nodes.Pop();
                    if (a_Set.Contains(curNode.Data.Edge))
                    {
                        --setCount;
                        if (out_LeftMost == null)
                        {
                            if (curNode.Left == m_Bottom || setCount == 0)
                            {
                                out_LeftMost = curNode.Data.Edge;
                                return true;
                            }
                            else
                            {
                                out_LeftMost = curNode.Data.Edge;
                                lowestLevel = curLevel;
                                nodes.Push(curNode.Left);
                                nodesLevel.Push(curLevel + 1);
                            }
                        }
                        else
                        {
                            out_LeftMost = curNode.Data.Edge;
                            lowestLevel = curLevel;
                            if (curNode.Left != m_Bottom)
                            {
                                nodes.Push(curNode.Left);
                                nodesLevel.Push(curLevel + 1);
                            }
                        }
                    }
                    else
                    {
                        if (curNode.Right != m_Bottom)
                        {
                            nodes.Push(curNode.Right);
                            nodesLevel.Push(curLevel + 1);
                        }
                        if (curNode.Left != m_Bottom)
                        {
                            nodes.Push(curNode.Left);
                            nodesLevel.Push(curLevel + 1);
                        }
                    }
                }
                return out_LeftMost != null;
            }
        }

        private bool FindRightMostSegmentInSet(HashSet<Edge<T>> a_Set, Node t, out Edge<T> out_RightMost)
        {
            if (t == m_Bottom)
            {
                out_RightMost = null;
                return false;
            }
            else
            {
                out_RightMost = null;
                // Perform a DFS.
                int setCount = a_Set.Count;
                Stack<Node> nodes = new Stack<Node>(m_Size);
                Stack<int> nodesLevel = new Stack<int>(m_Size);
                nodes.Push(t);
                nodesLevel.Push(0);
                int lowestLevel = -1;
                while (nodes.Count != 0 && setCount != 0)
                {
                    int curLevel = nodesLevel.Pop();
                    if (curLevel <= lowestLevel && out_RightMost != null)
                    {
                        return true;
                    }
                    Node curNode = nodes.Pop();
                    if (a_Set.Contains(curNode.Data.Edge))
                    {
                        --setCount;
                        if (out_RightMost == null)
                        {
                            if (curNode.Right == m_Bottom || setCount == 0)
                            {
                                out_RightMost = curNode.Data.Edge;
                                return true;
                            }
                            else
                            {
                                out_RightMost = curNode.Data.Edge;
                                lowestLevel = curLevel;
                                nodes.Push(curNode.Right);
                                nodesLevel.Push(curLevel + 1);
                            }
                        }
                        else
                        {
                            out_RightMost = curNode.Data.Edge;
                            lowestLevel = curLevel;
                            if (curNode.Right != m_Bottom)
                            {
                                nodes.Push(curNode.Right);
                                nodesLevel.Push(curLevel + 1);
                            }
                        }
                    }
                    else
                    {
                        if (curNode.Left != m_Bottom)
                        {
                            nodes.Push(curNode.Left);
                            nodesLevel.Push(curLevel + 1);
                        }
                        if (curNode.Right != m_Bottom)
                        {
                            nodes.Push(curNode.Right);
                            nodesLevel.Push(curLevel + 1);
                        }
                    }
                }
                return out_RightMost != null;
            }
        }
    }
}
