namespace TheHeist
{
    using System.Collections.Generic;
    using UnityEngine;
    using System.Collections;
    using System;
    using Util.Math;
    using Util.Geometry.Polygon;
    using System.Linq;
    using Util.Geometry;

    public class TheHeistAlgorithmV2 : MonoBehaviour
    {

        // position of the player,guard and objects.
        //[SerializeField]
        //Vector2 playerPos;
        //[SerializeField]
        //Vector2 guardPos;
        //[SerializeField]
        //Vector2 guardOrientation;
        [SerializeField]
        float coneWidth;
        [SerializeField]
        float coneLength;
        [SerializeField]
        static List<Line> CurrentLevel;

        List<Line> final;

        enum COLOR { RED, BLACK };
        enum DIRECTION { LEFT, RIGHT };
        enum COMPARE { SMALLER, EQUAL, GREATER, UNKOWN }

        class Line
        {
            public Line(Vector2 start, Vector2 end)
            {
                this.start = start;
                this.end = end;
                this.middle = ((end - start) / 2) + end;

                this.segment = new LineSegment(start, end);
            }

            public LineSegment segment;

            public void translate(Vector2 translation)
            {
                start += translation;
                end += translation;
            }
            public Vector2 start, end, middle;
            public float startDistance, endDistance, shortestDistance, longestDistance, middleDistance;
            public float startInterval, endInterval;
        }

        class Interval : IInterval
        {
            public bool isLeftEndpoint;
            public Endpoint endpoint;
            Line correspondingLine;
            //TODO keep track of intervals in which the endpoint lies.

            public Interval(bool isLeftEndpoint, Endpoint endpoint, Line correspondingLine)
            {
                this.isLeftEndpoint = isLeftEndpoint;
                this.endpoint = endpoint;
                this.correspondingLine = correspondingLine;
            }
            public COMPARE compare(Interval other)
            {
                if (other == null)
                    throw new Exception("Other interval to compare is null");

                if (this.endpoint.pos > other.endpoint.pos)
                    return COMPARE.SMALLER;
                else if (this.endpoint == other.endpoint)
                    return COMPARE.EQUAL;
                else if (this.endpoint.pos < other.endpoint.pos)
                    return COMPARE.GREATER;
                else
                    return COMPARE.UNKOWN;
            }
        }

        class Endpoint
        {
            public float pos;
            public bool isLeft;

            public Endpoint(float pos, bool isLeft)
            {
                this.pos = pos;
                this.isLeft = isLeft;
            }
        }

        interface IInterval
        {
            COMPARE compare(Interval other);
        }

        class Node
        {
            //comparable data in our case a interval(eindpoint) 
            public IInterval data;

            //node info
            public Node left;
            public Node right;
            public Node parent;
            public COLOR color = COLOR.BLACK;

            //constructors
            public Node(IInterval data) : this(data, null, null) { }
            public Node(IInterval data, Node left, Node right)
            {
                this.data = data;
                this.left = left;
                this.right = right;
            }
        }

        class RedBlackTree
        {
            protected Node root;
            protected Node currentNode;
            public RedBlackTree()
            {
                root = new Node(null);
            }
            //protected int Compare (IInterval item, Node node)
            //{
            //    return item.compare(node.data);
            //}

            //public void Insert(Interval data)
            //{
            //    if (root != null)
            //    {
            //        if (root.data != null)
            //        {
            //            currentNode = root;
            //            while(true)
            //            {
            //                int compareResult = Compare(data, currentNode);
            //                if (compareResult < 0 && currentNode.left != null)
            //                    currentNode = currentNode.left;
            //                else if (compareResult >= 0 && currentNode.right != null)
            //                    currentNode = currentNode.right;
            //                else if (compareResult < 0 && currentNode.left == null)
            //                {
            //                    currentNode.left = new Node(data);
            //                    currentNode = currentNode.left;
            //                    UpdateRedBlack(currentNode);
            //                }
            //                else if (compareResult >= 0 && currentNode.right == null)
            //                {
            //                    currentNode.right = new Node(data);
            //                    currentNode = currentNode.right;
            //                    UpdateRedBlack(currentNode);
            //                }
            //            }
            //        }
            //        else
            //        {
            //            root.data = data;
            //        }
            //    }
            //    else
            //        throw new ArgumentException("Root is null init three first propably");


            //}

            //public void UpdateRedBlack(Node node)
            //{
            //    while(node != root && node.parent.color == COLOR.RED)
            //    {
            //        //inserted to much at the left??
            //        if(node.parent == node.parent.parent.left)
            //        {
            //            Node temp = node.parent.parent.right;
            //            if(temp != null && node.color == COLOR.RED)
            //            {
            //                node.parent.color = COLOR.BLACK;
            //                node.color = COLOR.BLACK;
            //                node.parent.parent.color = COLOR.RED;
            //                node = node.parent.parent;
            //            }
            //            else 
            //            {
            //                if (node == node.parent.right)
            //                {
            //                    node = node.parent;
            //                    LeftRotate(node);
            //                }
            //                node.parent.color = COLOR.BLACK;
            //                node.parent.parent.color = COLOR.RED;
            //                RightRotate(node.parent.parent);
            //            }

            //        }
            //        else //inserted to much at the right??
            //        {
            //            Node temp2 = node.parent.parent.left;
            //            if(temp2 != null && temp2.color == COLOR.BLACK)
            //            {
            //                node.parent.color = COLOR.RED;
            //                temp2.color = COLOR.RED;
            //                node.parent.parent.color = COLOR.BLACK;
            //                node = node.parent.parent;
            //            }
            //            else 
            //            {
            //                if (node == node.parent.left)
            //                {
            //                    node = node.parent;
            //                    RightRotate(node);
            //                }
            //                node.parent.color = COLOR.BLACK;
            //                node.parent.parent.color = COLOR.RED;
            //                LeftRotate(node.parent.parent);
            //            }
            //        }
            //    }
            //}
            //private void LeftRotate(Node X)
            //{
            //    Node Y = X.right; // set Y
            //    X.right = Y.left;//turn Y's left subtree into X's right subtree
            //    if (Y.left != null)
            //    {
            //        Y.left.parent = X;
            //    }
            //    if (Y != null)
            //    {
            //        Y.parent = X.parent;//link X's parent to Y
            //    }
            //    if (X.parent == null)
            //    {
            //        root = Y;
            //    }
            //    if (X == X.parent.left)
            //    {
            //        X.parent.left = Y;
            //    }
            //    else
            //    {
            //        X.parent.right = Y;
            //    }
            //    Y.left = X; //put X on Y's left
            //    if (X != null)
            //    {
            //        X.parent = Y;
            //    }

            //}

            //private void RightRotate(Node Y)
            //{
            //    // right rotate is simply mirror code from left rotate
            //    Node X = Y.left;
            //    Y.left = X.right;
            //    if (X.right != null)
            //    {
            //        X.right.parent = Y;
            //    }
            //    if (X != null)
            //    {
            //        X.parent = Y.parent;
            //    }
            //    if (Y.parent == null)
            //    {
            //        root = X;
            //    }
            //    if (Y == Y.parent.right)
            //    {
            //        Y.parent.right = X;
            //    }
            //    if (Y == Y.parent.left)
            //    {
            //        Y.parent.left = X;
            //    }

            //    X.right = Y;//put Y on X's right
            //    if (Y != null)
            //    {
            //        Y.parent = X;
            //    }
            //}


            //public Node Search(IComparable data)
            //{
            //    currentNode = root;
            //    while(true)
            //    {
            //        int compareResult = Compare(data, currentNode);

            //        if (compareResult < 0 && currentNode.left != null)
            //            currentNode = currentNode.left;
            //        else if (compareResult > 0 && currentNode.right != null)
            //            currentNode = currentNode.right;
            //        else if (compareResult == 0)
            //            return currentNode;
            //        else return null;
            //    }
            //}


        }



        public bool checkVisibility(Vector2 playerPos, Vector2 guardPos, Vector2 guardOrientation)
        {
            //this.playerPos = playerPos;
            //this.guardPos = guardPos;
            //this.guardOrientation = guardOrientation;

            List<Line> sortedShortestDistance = new List<Line>();
            List<Line> sortedLongestDistance = new List<Line>();
            List<Line> sortedMiddletDistance = new List<Line>();
            List<Line> sortedStartInterval = new List<Line>();
            List<Line> sortedEndInterval = new List<Line>();

            //Get the lines sorted on distances and angles after translating the current level by the guard pos.
            calculateDistanceAndAngle(CurrentLevel, guardPos, out sortedShortestDistance, out sortedLongestDistance, out sortedMiddletDistance, out sortedStartInterval, out sortedEndInterval);
            //Create bst for endpoints
            //RedBlackTree tree = new RedBlackTree();
            //foreach(Line l in sortedStartInterval)
            //{
            //    tree.Insert(new Interval(true, l.startInterval, l));
            //    tree.Insert(new Interval(false, l.endInterval, l));

            //}
            return true;
        }

        void diskSweep()
        {
            //Important to have the lvl and the guard pos set!!
            //List<Line> linesWithDistance = calculateDistance(CurrentLevel, guardPos);
            //Queue<HeistEvent> status = getEvents(linesWithDistance);
            //return status;
        }

        void getEvents(List<Line> sortedLines)
        {
            //List<HeistEvent> events = new List<HeistEvent>();
            //foreach(Line l in sortedLines)
            //{
            //    events.Add(new HeistEvent(l, true));
            //    events.Add(new HeistEvent(l, false));
            //}

            ////sort on event with smallest distance
            //events.Sort((x, y) => x.distance.CompareTo(y.distance));
            //Queue<HeistEvent> status = new Queue<HeistEvent>();
            //foreach(HeistEvent he in events)
            //{
            //    status.Enqueue(he);
            //}
            //return status;
        }

        void calculateDistanceAndAngle(List<Line> currentLevel, Vector2 guardPos, out List<Line> sortedShortestDistance, out List<Line> sortedLongestDistance
                , out List<Line> sortedMiddleDistance, out List<Line> sortedStartInterval, out List<Line> sortedEndInterval)
        {
            //Translate level
            List<Line> tempLines = new List<Line>(currentLevel);
            foreach (Line l in tempLines)
                l.translate(guardPos * -1);

            //calculate angle for each line and make sure they are between 0 and 360
            List<Line> linesToBeAdded = new List<Line>();
            foreach (Line l in tempLines)
            {
                float startAngle = Vector2.SignedAngle(Vector2.up, l.start);
                float endAngle = Vector2.SignedAngle(Vector2.up, l.end);

                if (startAngle < 0)
                    startAngle += 360;
                if (endAngle < 0)
                    endAngle += 360;

                //Get the rotational interval from the line
                if (startAngle < endAngle)
                {
                    l.startInterval = startAngle;
                    l.endInterval = endAngle;
                }
                else
                {
                    l.startInterval = endAngle;
                    l.endInterval = startAngle;
                }

                //flip the interval if nessesary
                if (l.endInterval > l.startInterval + 180)
                {
                    float tempStartInterval = l.startInterval;
                    l.startInterval = l.endInterval;
                    l.endInterval = tempStartInterval;
                }

                //Split a line in two when the interval contains 0
                if (l.startInterval > l.endInterval)
                {
                    //split the line in two lines
                    Line newLine = new Line(l.start, l.end);
                    newLine.startInterval = 0;
                    newLine.endInterval = l.endInterval;
                    linesToBeAdded.Add(newLine);
                    //update original 
                    l.endInterval = 360;
                }
            }

            //add the aditional intervals of the lines
            tempLines.AddRange(linesToBeAdded);

            //get distance form the start, end and middle points and keep track of the shorest one.
            foreach (Line l in tempLines)
            {
                l.startDistance = l.start.magnitude;
                l.endDistance = l.end.magnitude;
                l.middleDistance = l.middle.magnitude;

                if (l.startDistance < l.endDistance)
                {
                    l.shortestDistance = l.startDistance;
                    l.longestDistance = l.endDistance;
                }
                else
                {
                    l.shortestDistance = l.endDistance;
                    l.longestDistance = l.startDistance;
                }
            }

            //create and sort lists
            sortedShortestDistance = new List<Line>(tempLines);
            sortedLongestDistance = new List<Line>(tempLines);
            sortedMiddleDistance = new List<Line>(tempLines);
            sortedStartInterval = new List<Line>(tempLines);
            sortedEndInterval = new List<Line>(tempLines);
            //we assume this sort is in nLog(n)
            sortedShortestDistance.Sort((x, y) => x.shortestDistance.CompareTo(y.shortestDistance));
            sortedLongestDistance.Sort((x, y) => x.longestDistance.CompareTo(y.longestDistance));
            sortedMiddleDistance.Sort((x, y) => x.middleDistance.CompareTo(y.middleDistance));
            sortedStartInterval.Sort((x, y) => x.startInterval.CompareTo(y.startInterval));
            sortedEndInterval.Sort((x, y) => x.endInterval.CompareTo(y.endInterval));
        }

       

            //constructor for this class
            public Polygon2D Vision(Polygon2DWithHoles polygon, Vector2 guardPos, Vector2 playerPos, Vector2 guardOrientation)
        {
            List<Line> lines = new List<Line>();
            var lineSegments = polygon.Segments;
            foreach(var line in lines)
            {
                lines.Add(line);
            }
            CurrentLevel = lines;

            checkVisibility(guardPos, playerPos, guardOrientation);

            List<Vector2> polyPoints = new List<Vector2>();
            //foreach(var line in final)
            //{
            //    //polyPoints.Add(line.start);
            //}
            polyPoints.Add(guardPos);

            Vector2 coneLine1 = Rotate(guardOrientation, 30);
            Vector2 coneLine2 = Rotate(guardOrientation, -30);

            Ray2D ray = new Ray2D(guardPos, coneLine1);
            Ray2D ray2 = new Ray2D(guardPos, coneLine2);

            bool coneStarted = false;

            foreach(var seg in polygon.Outside.Segments)
            {
                var intersection = seg.Intersect(ray);
                var intersection2 = seg.Intersect(ray2);
                if (intersection.HasValue)
                {
                    coneStarted = true;
                    polyPoints.Add(intersection.Value);
                }
                if (intersection2.HasValue)
                {
                    polyPoints.Add(intersection2.Value);
                    coneStarted = false;
                }
                if (coneStarted)
                {
                    polyPoints.Add(seg.Point2);
                }
            }

            return new Polygon2D(polyPoints);
        }

        public static Vector2 Rotate(Vector2 v, float degrees)
        {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = v.x;
            float ty = v.y;
            v.x = (cos * tx) - (sin * ty);
            v.y = (sin * tx) + (cos * ty);
            return v;
        }


        // Use this for initialization
        void Start()
        {

            //List<Line> lines = new List<Line>();
            //lines.Add(new Line(new Vector2(1f, 1f), new Vector2(3f, 1f)));
            //lines.Add(new Line(new Vector2(4f, 1f), new Vector2(5f, 1f)));
            //lines.Add(new Line(new Vector2(-1f, 1f), new Vector2(-3f, 1f)));
            //lines.Add(new Line(new Vector2(-4f, 1f), new Vector2(-6f, 1f)));
            //lines.Add(new Line(new Vector2(-1f, 1f), new Vector2(1f, 1f)));
            //CurrentLevel = lines;

            //checkVisibility(Vector2.zero, Vector2.zero, Vector2.zero);
        }

    }
}
