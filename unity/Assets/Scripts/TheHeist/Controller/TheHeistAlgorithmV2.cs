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

            public bool equal(Line other)
            {
                if (this.start == other.start && this.end == other.end)
                {
                    return true;
                }
                else
                    return false;
            }
        }

        class Interval
        {
            public bool isLeftEndpoint;
            public Intervalpoint intervalpoint;
            public float startInterval, endInterval;
            public Line correspondingLine;
            public List<Interval> intervals = new List<Interval>();

            //TODO keep track of intervals in which the endpoint lies.

            public Interval(Intervalpoint endpoint, Line correspondingLine)
            {
                this.startInterval = correspondingLine.startInterval;
                this.endInterval = correspondingLine.endInterval;
                this.intervalpoint = endpoint;
                this.correspondingLine = correspondingLine;
            }
            public COMPARE compare(Interval other)
            {
                Interval interval = other as Interval;

                if (interval == null)
                    throw new Exception("Other interval to compare is null");

                if (this.intervalpoint.angle > interval.intervalpoint.angle)
                    return COMPARE.SMALLER;
                else if (this.intervalpoint.angle == interval.intervalpoint.angle)
                    return COMPARE.EQUAL;
                else if (this.intervalpoint.angle < interval.intervalpoint.angle)
                    return COMPARE.GREATER;
                else
                    return COMPARE.UNKOWN;
            }

            public string print()
            {
                if (intervalpoint != null)
                    return "pos: " + intervalpoint.angle + "is left: " + isLeftEndpoint;
                else
                    return "No endpoint avaiable";
            }
        }

        class Intervalpoint
        {
            public float angle;
            public bool isLeft;

            public Intervalpoint(float angle, bool isLeft)
            {
                this.angle = angle;
                this.isLeft = isLeft;
            }
        }

        //interface IInterval
        //{
        //    COMPARE compare(Interval other);
        //}

        class Node
        {
            //comparable data in our case a interval(eindpoint) 
            public Interval data;
            public List<Interval> isInIntervals = new List<Interval>();
            public List<Interval> isOutIntervals = new List<Interval>();
            //node info
            public Node left;
            public Node right;
            public Node parent;
            public bool isLeftChild;
            public COLOR color = COLOR.BLACK;

            //creates a node with empty children
            public Node(Interval data)
            {
                setData(data);
            }

            //sets the data and create empty children because the node is already a leaf.
            public void setData(Interval data)
            {
                this.data = data;
                color = COLOR.RED;
                left = new Node();
                right = new Node();
                left.setParent(this);
                right.setParent(this);
                left.isLeftChild = true;
                right.isLeftChild = false;
                if (data != null)
                    this.isInIntervals = data.intervals;
            }

            public void setParent(Node parent)
            {
                this.parent = parent;
            }

            public void setRight(Node rightchild)
            {
                right = rightchild;
                right.setParent(this);
            }

            public void setLeft(Node leftchild)
            {
                left = leftchild;
                left.setParent(this);
            }

            public string print()
            {
                string msg = "";
                if (data != null)
                {
                    if (color == COLOR.RED)
                    {
                        msg = "Red - " + data.print();
                    }
                    else
                    {
                        msg = "Black - " + data.print();
                    }
                }
                else
                    msg = "No interval available";
                return msg;
            }

            public Node()
            {

            }
        }

        class RedBlackTree
        {
            public Node root;

            public RedBlackTree()
            {
                root = new Node(null);
            }

            public void insertInterval(Interval interval)
            {
                insertNode(interval, ref root);

            }


            public void print()
            {
                printNode(ref root);
            }

            private void printNode(ref Node currentNode)
            {
                string msg;
                if (currentNode != null && currentNode.left != null && currentNode.right != null)
                {
                    msg = "current: " + currentNode.print() + " left: " + currentNode.left.print() + " right: " + currentNode.right.print();
                }
                else
                {
                    msg = "leaf";
                }

                Debug.Log(msg);
                if (currentNode.left != null)
                    printNode(ref currentNode.left);
                if (currentNode.right != null)
                    printNode(ref currentNode.right);
            }

            public void Query(float angle, ref Node currentNode)
            {
                if (angle < currentNode.data.intervalpoint.angle)
                {

                }
            }


            public void UpdateNode(float angle, bool isLeft, ref Node currentNode, List<Interval> inIntervals, List<Interval> outIntervals)
            {
                if (currentNode.data.intervalpoint.angle == angle && currentNode.data.intervalpoint.isLeft == isLeft)
                {
                    // update node when found
                    inIntervals = ListSubstractor(inIntervals, outIntervals);

                    currentNode.isInIntervals = inIntervals;
                    currentNode.isOutIntervals = outIntervals;
                }
                else
                {
                    inIntervals = ListExclusiveCombine(inIntervals, currentNode.isInIntervals);
                    outIntervals = ListExclusiveCombine(outIntervals, currentNode.isOutIntervals);
                    // update all nodes in path
                    if (angle <= currentNode.data.intervalpoint.angle)
                    {
                        if (currentNode.left.data != null)
                           UpdateNode(angle, isLeft, ref currentNode.left, inIntervals, outIntervals);
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (currentNode.right.data != null)
                            UpdateNode(angle, isLeft, ref currentNode.right, inIntervals, outIntervals);
                        else
                        {
                            return;
                        }
                    }
                }
            }

            public Interval Search(float angle, bool isLeft, ref Node currentNode)
            {
                if (currentNode.data.intervalpoint.angle == angle && currentNode.data.intervalpoint.isLeft == isLeft)
                {
                    return currentNode.data;
                } else
                {
                    if (angle <= currentNode.data.intervalpoint.angle)
                    { 
                        if (currentNode.left.data != null)
                            return Search(angle, isLeft, ref currentNode.left);
                        else
                        {
                            return currentNode.data;
                        }
                    }
                    else 
                    {
                        if (currentNode.right.data != null)
                            return Search(angle, isLeft, ref currentNode.right);
                        else
                        {
                            return currentNode.data;
                        }
                    }
                }
            }
            public List<Interval> ListSubstractor(List<Interval> inList, List<Interval> outList)
            {
                List<Interval> finalList = new List<Interval>();
                foreach (Interval inter in inList)
                {
                    bool found = false;
                    foreach (Interval inter2 in outList)
                    {
                        if (inter.correspondingLine.equal(inter2.correspondingLine))
                            found = true;
                    }
                    if (!found)
                        finalList.Add(inter);
                }
                return finalList;
            }
            /// <summary>
            /// Combines two lists of intervals excluding duplicates
            /// </summary>
            /// <param name="list1"></param>
            /// <param name="list2"></param>
            /// <returns></returns>
            public List<Interval> ListExclusiveCombine(List<Interval> list1, List<Interval> list2)
            {
                List<Interval> finalList = list1;
                foreach (Interval inter in list2)
                {
                    bool found = false;
                    foreach (Interval inter2 in list1)
                    {
                        if (inter.intervalpoint.angle == inter2.intervalpoint.angle && inter.intervalpoint.isLeft == inter2.intervalpoint.isLeft)
                        {
                            found = true;
                        }

                    }
                    if (!found)
                    {
                        finalList.Add(inter);
                    }
                }
                return finalList;
            }


            private void insertNode(Interval interval, ref Node currentNode)
            {
                if (currentNode.data == null)
                {
                    currentNode.setData(interval);
                    return;
                }
                else
                {
                    ////keep track of known intervals
                    //List<Interval> knownIntervals = new List<Interval>();
                    //foreach (Interval i in currentNode.intervals)
                    //{
                    //    bool alreadyKnown = false;
                    //    foreach (Interval ii in interval.intervals)
                    //    {
                    //        if (ii.correspondingLine.equal(i.correspondingLine))
                    //        {
                    //            alreadyKnown = true;
                    //        }
                    //    }
                    //    if (alreadyKnown == false)
                    //        knownIntervals.Add(i);
                    //}

                    //foreach (Interval I in knownIntervals)
                    //{
                    //    interval.intervals.Add(I);
                    //}



                    COMPARE result = currentNode.data.compare(interval);
                    switch (result)
                    {
                        case COMPARE.SMALLER:
                            {
                                if (interval.intervalpoint.isLeft)
                                {
                                    currentNode.isInIntervals.Add(interval); //add interval that current is in
                                } else
                                {
                                    currentNode.isOutIntervals.Add(interval);
                                }

                                currentNode.isInIntervals = ListSubstractor(currentNode.isInIntervals, currentNode.isOutIntervals);

                                insertNode(interval, ref currentNode.left);
                            }
                            break;
                        case COMPARE.EQUAL:
                            {
                                if (interval.intervalpoint.isLeft)
                                {
                                    currentNode.isInIntervals.Add(interval); //add interval that current is in
                                }
                                else
                                {
                                    currentNode.isOutIntervals.Add(interval);
                                }

                                currentNode.isInIntervals = ListSubstractor(currentNode.isInIntervals, currentNode.isOutIntervals);

                                insertNode(interval, ref currentNode.left);
                            }
                            break;
                        case COMPARE.GREATER:
                            {
                                if (!interval.intervalpoint.isLeft)
                                {
                                    currentNode.isInIntervals.Add(interval); //add interval that current is in
                                }
                                else
                                {
                                    currentNode.isOutIntervals.Add(interval);
                                }
                                
                                currentNode.isInIntervals = ListSubstractor(currentNode.isInIntervals, currentNode.isOutIntervals);

                                insertNode(interval, ref currentNode.right);
                            }
                            break;
                        default:
                            {
                                throw new Exception("Can't compare interval data");
                            }
                            break;
                    }
                }

            }
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

                RedBlackTree tree = new RedBlackTree();
                foreach (Line l in sortedMiddletDistance)
                {
                    tree.insertInterval(new Interval(new Intervalpoint(l.startInterval, true), l));
                    tree.UpdateNode(l.startInterval, true, ref tree.root, new List<Interval>(), new List<Interval>());
                    tree.insertInterval(new Interval(new Intervalpoint(l.endInterval, false), l));
                    tree.UpdateNode(l.startInterval, false, ref tree.root, new List<Interval>(), new List<Interval>());
                }

                //tree.print();

                return true;
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
            //CurrentLevel = lines;
            CurrentLevel = new List<Line>();
            CurrentLevel.Add(new Line(new Vector2(432, 590), new Vector2(432, 530)));
            CurrentLevel.Add(new Line(new Vector2(420, 560), new Vector2(420, 500)));
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
