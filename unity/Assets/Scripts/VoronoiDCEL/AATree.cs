namespace VoronoiDCEL
{
    using System;
    using System.Collections.Generic;

    public class AATree<T> where T: IComparable<T>, IEquatable<T>
    {
        // Sentinel.
        protected Node m_Bottom;
        protected Node m_Tree;
        protected int m_Size = 0;

        protected internal class TraversalHistory
        {
            public TraversalHistory(Node a_Node, Node a_Parent, ECHILDSIDE a_ChildSide)
            {
                node = a_Node;
                parentNode = a_Parent;
                side = a_ChildSide;
            }

            public Node node;
            public Node parentNode;

            public enum ECHILDSIDE
            {
                LEFT,
                RIGHT,
                ISROOT
            }

            public ECHILDSIDE side;
        }

        protected enum COMPARISON_TYPE
        {
            INSERT,
            DELETE,
            FIND
        }

        public int Size
        {
            get { return m_Size; }
        }

        protected internal class Node
        {
            public Node Left
            {
                get;
                set;
            }

            public Node Right
            {
                get;
                set;
            }

            public int Level
            {
                get;
                set;
            }

            public T Data
            {
                get;
                set;
            }

            public Node(T a_Data, Node a_Left, Node a_Right, int a_Level)
            {
                Data = a_Data;
                if (EqualityComparer<T>.Default.Equals(a_Data, default(T)))
                {
                    Left = this;
                    Right = this;
                    if (a_Level != 0)
                    {
                        throw new Exception("Cannot use default value for node other than Bottom!");
                    }
                }
                else
                {
                    Left = a_Left;
                    Right = a_Right;
                    if (a_Level != 1)
                    {
                        throw new Exception("Cannot create node with level other than 1!");
                    }
                }
                Level = a_Level;
            }
        }

        public AATree()
        {
            m_Bottom = new Node(default(T), null, null, 0);
            m_Tree = m_Bottom;
        }

        public bool Contains(T data)
        {
            return FindNodes(data).Length > 0;
        }

        public int ComputeSize()
        {
            return ComputeSize(m_Tree);
        }

        public bool FindMin(out T out_MinValue)
        {
            Node min = FindMinNode();
            if (min != m_Bottom)
            {
                out_MinValue = min.Data;
                return true;
            }
            out_MinValue = default(T);
            return false;
        }

        public bool FindMax(out T out_MaxValue)
        {
            Node max = FindMaxNode();
            if (max != m_Bottom)
            {
                out_MaxValue = max.Data;
                return true;
            }
            out_MaxValue = default(T);
            return false;
        }

        protected Node FindMinNode()
        {
            Node min = m_Tree;
            while (min.Left != m_Bottom)
            {
                min = min.Left;
            }
            return min;
        }

        protected Node FindMaxNode()
        {
            Node max = m_Tree;
            while (max.Right != m_Bottom)
            {
                max = max.Right;
            }
            return max;
        }

        public bool DeleteMax()
        {
            T max;
            return FindMax(out max) && Delete(max);
        }

        public bool DeleteMin()
        {
            T min;
            return FindMin(out min) && Delete(min);
        }

        public bool VerifyLevels()
        {
            return VerifyLevels(m_Tree, m_Tree.Level);
        }


        public T[] FindNodes(T data)
        {
            List<T> nodes = new List<T>();
            FindNodes(data, m_Tree, nodes);
            return nodes.ToArray();
        }

        public bool VerifyBST(T minValue, T maxValue)
        {
            return VerifyBST(m_Tree, minValue, maxValue, COMPARISON_TYPE.INSERT) &&
            VerifyBST(m_Tree, minValue, maxValue, COMPARISON_TYPE.DELETE) &&
            VerifyBST(m_Tree, minValue, maxValue, COMPARISON_TYPE.FIND);
        }

        public bool VerifyOrder()
        {
            return VerifyOrder(m_Tree, COMPARISON_TYPE.INSERT) &&
            VerifyOrder(m_Tree, COMPARISON_TYPE.DELETE) &&
            VerifyOrder(m_Tree, COMPARISON_TYPE.FIND);
        }

        private Node Skew(Node t, Node parent, TraversalHistory.ECHILDSIDE a_Side)
        {
            if (t.Left.Level == t.Level)
            {
                // Rotate right.
                Node oldLeft = t.Left;
                Node newLeft = oldLeft.Right;
                t.Left = newLeft;
                oldLeft.Right = t;
                if (a_Side == TraversalHistory.ECHILDSIDE.LEFT)
                {
                    parent.Left = oldLeft;
                }
                else if (a_Side == TraversalHistory.ECHILDSIDE.RIGHT)
                {
                    parent.Right = oldLeft;
                }
                else
                {
                    m_Tree = oldLeft;
                }
                return oldLeft;
            }
            return t;
        }

        private Node Split(Node t, Node parent, TraversalHistory.ECHILDSIDE a_Side)
        {
            if (t.Right.Right.Level == t.Level && t.Right != m_Bottom)
            {
                // Rotate left.
                Node oldRight = t.Right;
                Node newRight = oldRight.Left;
                t.Right = newRight;
                oldRight.Left = t;
                if (a_Side == TraversalHistory.ECHILDSIDE.LEFT)
                {
                    parent.Left = oldRight;
                }
                else if (a_Side == TraversalHistory.ECHILDSIDE.RIGHT)
                {
                    parent.Right = oldRight;
                }
                else
                {
                    m_Tree = oldRight;
                }
                ++oldRight.Level;
                return oldRight;
            }
            return t;
        }

        public bool Insert(T data)
        {
            if (!(data is ValueType) && EqualityComparer<T>.Default.Equals(data, default(T)))
            {
                return false;
            }
            else if (m_Tree == m_Bottom)
            {
                m_Tree = CreateNode(data);
                return true;
            }
            else
            {
                Node currentNode = m_Tree;
                Node parent = null;
                int comparisonResult = 0;
                Stack<TraversalHistory> nodeStack = new Stack<TraversalHistory>((int)Math.Ceiling(Math.Log(m_Size + 1, 2)) + 1);
                nodeStack.Push(new TraversalHistory(currentNode, parent, TraversalHistory.ECHILDSIDE.ISROOT));
                while (currentNode != m_Bottom)
                {
                    parent = currentNode;
                    comparisonResult = CompareTo(data, currentNode.Data, COMPARISON_TYPE.INSERT);
                    TraversalHistory histEntry;
                    if (comparisonResult < 0)
                    {
                        currentNode = currentNode.Left;
                        histEntry = new TraversalHistory(currentNode, parent, TraversalHistory.ECHILDSIDE.LEFT);
                        nodeStack.Push(histEntry);
                    }
                    else if (comparisonResult > 0)
                    {
                        currentNode = currentNode.Right;
                        histEntry = new TraversalHistory(currentNode, parent, TraversalHistory.ECHILDSIDE.RIGHT);
                        nodeStack.Push(histEntry);
                    }
                    else
                    {
                        break;
                    }
                }
                bool didInsert = false;
                if (nodeStack.Pop().node == m_Bottom) // This node must be m_Bottom.
                {
                    if (comparisonResult < 0)
                    {
                        parent.Left = CreateNode(data);
                        didInsert = true;
                    }
                    else if (comparisonResult > 0)
                    {
                        parent.Right = CreateNode(data);
                        didInsert = true;
                    }
                }

                while (nodeStack.Count != 0)
                {
                    TraversalHistory t = nodeStack.Pop();
                    Node n = t.node;
                    n = Skew(n, t.parentNode, t.side);
                    Split(n, t.parentNode, t.side);
                }
                return didInsert;
            }
        }

        private Node CreateNode(T data)
        {
            Node n = new Node(data, m_Bottom, m_Bottom, 1);
            ++m_Size;
            return n;
        }

        public bool Delete(T data)
        {
            if (m_Tree == m_Bottom || EqualityComparer<T>.Default.Equals(data, default(T)))
            {
                return false;
            }
            else
            {
                Node currentNode = m_Tree;
                Node parent = null;
                Node deleted = m_Bottom;
                Stack<TraversalHistory> nodeStack = new Stack<TraversalHistory>((int)Math.Ceiling(Math.Log(m_Size + 1, 2)) + 1);
                nodeStack.Push(new TraversalHistory(currentNode, parent, TraversalHistory.ECHILDSIDE.ISROOT));
                while (currentNode != m_Bottom)
                {
                    parent = currentNode;
                    TraversalHistory hist;
                    int comparisonResult = CompareTo(data, currentNode.Data, COMPARISON_TYPE.DELETE);
                    if (comparisonResult < 0)
                    {
                        currentNode = currentNode.Left;
                        hist = new TraversalHistory(currentNode, parent, TraversalHistory.ECHILDSIDE.LEFT);
                    }
                    else
                    {
                        deleted = currentNode;
                        currentNode = currentNode.Right;
                        hist = new TraversalHistory(currentNode, parent, TraversalHistory.ECHILDSIDE.RIGHT);
                    }
                    nodeStack.Push(hist);
                }

                bool didDelete = false;
                if (deleted != m_Bottom && CompareTo(data, deleted.Data, COMPARISON_TYPE.DELETE) == 0)
                {
                    if (nodeStack.Pop().node != m_Bottom) // Pop since the last entry is m_Bottom
                    {
                        throw new Exception("First node in traversal history was not Bottom!");
                    }
                    TraversalHistory lastHist = nodeStack.Pop();
                    Node last = lastHist.node; // This is the node that is leftmost of the node that we want to delete.
                    if (last.Left != m_Bottom)
                    {
                        throw new Exception("Last has a left child that is not Bottom!");
                    }
                    deleted.Data = last.Data;
                    Node copy = last.Right;
                    if (copy != m_Bottom)
                    {
                        last.Data = copy.Data;
                        last.Left = copy.Left;
                        last.Right = copy.Right;
                        last.Level = copy.Level;
                        // Destroy the node
                        copy.Left = null;
                        copy.Right = null;
                        copy.Data = default(T);
                    }
                    else
                    {
                        if (lastHist.side == TraversalHistory.ECHILDSIDE.LEFT)
                        {
                            lastHist.parentNode.Left = m_Bottom;
                        }
                        else if (lastHist.side == TraversalHistory.ECHILDSIDE.RIGHT)
                        {
                            lastHist.parentNode.Right = m_Bottom;
                        }
                        else
                        {
                            m_Tree = m_Bottom;
                        }
                    }
                    --m_Size;
                    didDelete = true;
                }

                while (nodeStack.Count != 0)
                {
                    TraversalHistory t = nodeStack.Pop();
                    Node n = t.node;
                    if (n.Left.Level < n.Level - 1 || n.Right.Level < n.Level - 1)
                    {
                        --n.Level;
                        if (n.Right.Level > n.Level)
                        {
                            n.Right.Level = n.Level;
                        }
                        n = Skew(n, t.parentNode, t.side);
                        n.Right = Skew(n.Right, n, TraversalHistory.ECHILDSIDE.RIGHT);
                        n.Right.Right = Skew(n.Right.Right, n.Right, TraversalHistory.ECHILDSIDE.RIGHT);
                        n = Split(n, t.parentNode, t.side);
                        n.Right = Split(n.Right, n, TraversalHistory.ECHILDSIDE.RIGHT);
                    }
                }
                return didDelete;
            }
        }

        public bool FindNextBiggest(T data, out T out_NextBiggest)
        {
            return FindNextBiggestOrSmallest(data, m_Tree, true, out out_NextBiggest);
        }

        public bool FindNextSmallest(T data, out T out_NextSmallest)
        {
            return FindNextBiggestOrSmallest(data, m_Tree, false, out out_NextSmallest);
        }

        private bool FindNextBiggestOrSmallest(T data, Node t, bool a_Bigger, out T out_NextBiggest)
        {
            if (t == m_Bottom || EqualityComparer<T>.Default.Equals(data, default(T)))
            {
                out_NextBiggest = default(T);
                return false;
            }
            else
            {
                Node currentNode = t;
                Node target = m_Bottom;
                Node lastSwitch = m_Bottom;
                while (true)
                {
                    int comparisonResult = CompareTo(data, currentNode.Data, COMPARISON_TYPE.FIND);
                    Node nextNode;
                    if (a_Bigger)
                    {
                        if (comparisonResult < 0)
                        {
                            nextNode = currentNode.Left;
                            lastSwitch = currentNode;
                        }
                        else
                        {
                            nextNode = currentNode.Right;
                            target = currentNode;
                        }
                    }
                    else
                    {
                        if (comparisonResult <= 0)
                        {
                            nextNode = currentNode.Left;
                            target = currentNode;
                        }
                        else
                        {
                            nextNode = currentNode.Right;
                            lastSwitch = currentNode;
                        }
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
                    
                if (target != m_Bottom && CompareTo(data, target.Data, COMPARISON_TYPE.FIND) == 0)
                {
                    if (currentNode != target && currentNode != m_Bottom)
                    {
                        out_NextBiggest = currentNode.Data;
                        return true;
                    }
                    else if (lastSwitch != m_Bottom)
                    {
                        out_NextBiggest = lastSwitch.Data;
                        return true;
                    }
                }
                out_NextBiggest = default(T);
                return false;
            }
        }

        private void FindNodes(T data, Node t, List<T> list)
        {
            if (t == m_Bottom)
            {
                return;
            }
            else if (IsEqual(t.Data, data, COMPARISON_TYPE.FIND))
            {
                list.Add(t.Data);
                FindNodes(data, t.Left, list);
                FindNodes(data, t.Right, list);
            }
            else if (CompareTo(data, t.Data, COMPARISON_TYPE.FIND) < 0)
            {
                FindNodes(data, t.Left, list);
            }
            else
            {
                FindNodes(data, t.Right, list);
            }
        }

        private int ComputeSize(Node t)
        {
            if (t == m_Bottom)
            {
                return 0;
            }
            else
            {
                int result = 1;
                result += ComputeSize(t.Left);
                result += ComputeSize(t.Right);
                return result;
            }
        }

        private bool VerifyLevels(Node t, int parentLevel)
        {
            if (t == m_Bottom && parentLevel >= 0)
            {
                return true;
            }
            else if (t != m_Bottom && t.Level <= parentLevel)
            {
                return VerifyLevels(t.Left, t.Level - 1) && VerifyLevels(t.Right, t.Level);
            }
            else
            {
                return false;
            }
        }

        private bool VerifyBST(Node t, T minKey, T maxKey, COMPARISON_TYPE a_ComparisonType)
        {
            if (t == m_Bottom)
            {
                return true;
            }
            else if (CompareTo(minKey, t.Data, a_ComparisonType) > 0 ||
                     CompareTo(maxKey, t.Data, a_ComparisonType) < 0)
            {
                return false;
            }
            else
            {
                return VerifyBST(t.Left, minKey, t.Data, a_ComparisonType) &&
                VerifyBST(t.Right, t.Data, maxKey, a_ComparisonType);
            }
        }

        private bool VerifyOrder(Node t, COMPARISON_TYPE a_ComparisonType)
        {
            try
            {
                if (t == m_Bottom)
                {
                    return true;
                }
                else if ((t.Left == m_Bottom || CompareTo(t.Data, t.Left.Data, a_ComparisonType) > 0) &&
                         (t.Right == m_Bottom || CompareTo(t.Data, t.Right.Data, a_ComparisonType) <= 0))
                {
                    return VerifyOrder(t.Left, a_ComparisonType) &&
                    VerifyOrder(t.Right, a_ComparisonType);
                }
                else
                {
                    return false;
                }
            }
            catch (NotImplementedException)
            {
                // If a particular comparison type is not implemented, just assume that was done
                // for a good reason and return true. (Otherwise the user's application will not work anyway)
                return true;
            }
        }

        protected virtual int CompareTo(T a, T b, COMPARISON_TYPE a_ComparisonType)
        {
            return a.CompareTo(b);
        }

        protected virtual bool IsEqual(T a, T b, COMPARISON_TYPE a_ComparisonType)
        {
            return a.Equals(b);
        }
    }
}