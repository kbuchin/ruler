namespace Util.Geometry.Polygon
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using UnityEngine;
    using System;
    using Util.Math;
    using Util.Geometry.Duality;

    /// <summary>
    /// A simple polygon, that is, one without holes. The polygon should be imutable, instead we return new polygons if something changes.
    /// We represent the polygon internally as a list of vertices.
    /// </summary>
    public class Polygon2D : IPolygon2D
    {
        private List<Vector2> m_vertices;
        private bool m_convex;
        private Rect? m_bBox = null;
        private Vector2? m_vertexMean = null;
        private Vector2? m_leftMostVertex = null;
        private List<Polygon2D> m_triangulization = null;
        private static readonly float m_eps = 0.005f;

        public ReadOnlyCollection<Vector2> Vertices { get { return m_vertices.AsReadOnly(); } }

        public int VertexCount { get { return m_vertices.Count; } }

        public Vector2 VertexMean
        {
            get
            {
                if (!m_vertexMean.HasValue)
                {
                    var sum = new Vector2(0, 0);
                    foreach (Vector2 vertex in m_vertices)
                    {
                        sum += vertex;
                    }
                    m_vertexMean = sum / m_vertices.Count;
                }
                return m_vertexMean.Value;
            }
        }

        public Vector2 LeftMostVertex
        {
            get
            {
                if (m_leftMostVertex.HasValue)
                {
                    return m_leftMostVertex.Value;
                }
                else
                {
                    m_leftMostVertex = FindLeftMostVertex();
                    return m_leftMostVertex.Value;
                }
            }
            private set
            {
                m_leftMostVertex = value;
            }
        }

        //TODO change to Polygon2DWithHoles
        public List<Polygon2D> Triangulation
        {
            get
            {
                if (m_triangulization == null)
                {
                    m_triangulization = Triangulate();
                }
                return m_triangulization;
            }
        }

        private Vector2 FindLeftMostVertex()
        {
            //init
            var minVertex = Vertices[0];
            var minX = Vertices[0].x;

            foreach (var v in Vertices)
            {
                if (v.x < minX)
                {
                    minX = v.x;
                    minVertex = v;
                }
            }

            return minVertex;
        }

        /// <summary>
        /// Constructs a clockwise polygon with the given vertices. They are assumed to be in clokwise order.
        /// </summary>
        /// <param name="a_vertices"></param>
        public Polygon2D(IEnumerable<Vector2> a_vertices)
        {
            m_vertices = a_vertices.ToList();
            if (m_vertices.Count() < 3)
            {
                throw new GeomException("Creating polygon of less then three vertices");
            }
            m_convex = isConvex();
        }


        /// <summary>
        /// Computes the area spanned by this polygon
        /// </summary>
        /// The theory behind this method is documented in the docs folder.
        /// <returns></returns>
        public float Area()
        {
            //Take the origin as arbitrary point P

            //add up signed areas allong the edges of the polygon
            var areasum = 0f;
            foreach (LineSegment seg in Segments())
            {
                var v1 = seg.Point1;
                var v2 = seg.Point2;
                areasum += v1.x * v2.y - v2.x * v1.y;
            }

            return Math.Abs(areasum) / 2;
        }

        /// <summary>
        /// Returns a list of all the line segments forming the polygon oriented in a counterclockwise manner.
        /// </summary>
        /// <returns></returns>
        public List<LineSegment> Segments()
        {
            var result = new List<LineSegment>(Vertices.Count);
            for (int i = 0; i < Vertices.Count - 1; i++)
            {
                result.Add(new LineSegment(Vertices[i], Vertices[i + 1]));
            }
            result.Add(new LineSegment(Vertices.Last(), Vertices.First()));
            return result;
        }

        /// <summary>
        /// returns a list of all the tangent lines
        /// </summary>
        /// <returns></returns>
        public List<Line> Lines()
        {
            var result = new List<Line>(Vertices.Count);
            for (int i = 0; i < Vertices.Count - 1; i++)
            {
                result.Add(new Line(Vertices[i], Vertices[i + 1]));
            }
            result.Add(new Line(Vertices.Last(), Vertices.First()));
            return result;
        }

        /// <summary>
        /// Tests wheter this polygon is clockwise and convex by verifying that each tripple of points constitues a right turn
        /// </summary>
        public bool isConvex()
        {
            if (VertexCount < 3)
            {
                throw new GeomException("Being convex is illdefined for polygons of 2 or less vertices");
            }

            Line line;
            bool test;
            for (var i = 0; i < m_vertices.Count - 2; i++)
            {
                line = new Line(m_vertices[i], m_vertices[i + 1]);
                test = line.PointRightOfLine(m_vertices[i + 2]);
                if (test == false)
                {
                    return false;
                }
            }

            var n = m_vertices.Count;
            line = new Line(m_vertices[n - 2], m_vertices[n - 1]);
            test = line.PointRightOfLine(m_vertices[0]);
            if (test == false)
            {
                return false;
            }
            line = new Line(m_vertices[n - 1], m_vertices[0]);
            test = line.PointRightOfLine(m_vertices[1]);
            if (test == false)
            {
                return false;
            }

            return true;
        }

        public bool Contains(Vector2 a_pos)
        {
            if (Area() == 0) //catch case of "flat" triangle
            {
                return false;
            }
            if (isConvex())
            {
                LineSegment segment;
                for (var i = 0; i < m_vertices.Count - 1; i++)
                {
                    segment = new LineSegment(m_vertices[i], m_vertices[i + 1]);
                    if (!segment.IsRightOf(a_pos))
                    {
                        return false;
                    }
                }
                segment = new LineSegment(m_vertices[m_vertices.Count - 1], m_vertices[0]);
                if (!segment.IsRightOf(a_pos))
                {
                    return false;
                }

                return true;
            }
            else
            {
                Debug.Assert(VertexCount > 3);
                foreach (var triangle in Triangulation)
                {
                    if (triangle.Contains(a_pos))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Triangulates this polygon using the two ears theorhem. This is O(n^2).
        /// </summary>
        /// NB: Also see poygontriangulation.pdf in the docs folder
        /// <returns>A list of clockwise triangles whose disjoint union is this polygon</returns>
        private List<Polygon2D> Triangulate()
        {
            //PERF we can do this faster as a sweepline algorithm
            if (VertexCount == 3)
            {
                if (this.isConvex())
                {
                    return new List<Polygon2D>() { this };
                }
                else
                {
                    var resultvertices = m_vertices.Where(v => true).Reverse();
                    var result = new Polygon2D(resultvertices);
                    Debug.Assert(result.isConvex());
                    return new List<Polygon2D>() { result };
                }
            }

            var triangles = new List<Polygon2D>();

            //Find leftmost vertex
            var leftVertex = LeftMostVertex;
            var index = Vertices.IndexOf(leftVertex);
            var previndex = (int)MathUtil.PositiveMod(index - 1, VertexCount);
            var nextindex = (int)MathUtil.PositiveMod(index + 1, VertexCount);

            //Create triangle with diagonal
            var triangleVertices = new List<Vector2>() { m_vertices[previndex], m_vertices[index], m_vertices[nextindex] };
            var candidateTriangle = new Polygon2D(triangleVertices);

            //check for other vertices inside the candidate triangle
            Line baseline = new Line(m_vertices[previndex], m_vertices[nextindex]);
            float distance = 0;
            int diagonalIndex = -1;

            for (int i = 0; i < Vertices.Count; i++)
            {
                var v = Vertices[i];
                if (triangleVertices.Contains(v) == false)
                {
                    if (candidateTriangle.Contains(v))
                    {
                        if (baseline.DistanceToPoint(v) > distance)
                        {
                            distance = baseline.DistanceToPoint(v);
                            diagonalIndex = i;
                        }
                    }
                }
            }


            //Do Recursive call
            if (diagonalIndex == -1) //didn't change
            {
                if (candidateTriangle.isConvex())
                {
                    triangles.Add(candidateTriangle);
                }
                else
                {
                    var resultvertices = triangleVertices.Where(v => true).Reverse();
                    triangles.Add(new Polygon2D(resultvertices));
                }
                var recursionList = Vertices.ToList();
                recursionList.Remove(leftVertex);
                triangles.AddRange(new Polygon2D(recursionList).Triangulate());
            }
            else
            {
                IEnumerable<Vector2> poly1List, poly2List;
                if (diagonalIndex < index)
                {
                    poly1List = Vertices.Skip(diagonalIndex).Take(index - diagonalIndex + 1);
                    poly2List = Vertices.Take(diagonalIndex + 1).Union(Vertices.Skip(index));
                }
                else
                {
                    poly1List = Vertices.Skip(index).Take(diagonalIndex - index + 1);
                    poly2List = Vertices.Take(index + 1).Union(Vertices.Skip(diagonalIndex));
                }
                triangles.AddRange(new Polygon2D(poly1List).Triangulate());
                triangles.AddRange(new Polygon2D(poly2List).Triangulate());
            }

            //retun result
            return triangles;
        }

        /// <summary>
        /// Dirty method O(n^2)
        /// </summary>
        /// <param name="a_poly1"></param>
        /// <param name="a_poly2"></param>
        /// <returns></returns>
        public static Polygon2D IntersectConvex(Polygon2D a_poly1, Polygon2D a_poly2)
        {
            if (!(a_poly1.m_convex))
            {
                throw new GeomException("Method not defined for nonconvex polygons" + a_poly1);
            }
            if (!(a_poly2.m_convex))
            {
                throw new GeomException("Method not defined for nonconvex polygons" + a_poly2);
            }

            var resultVertices = new List<Vector2>();

            foreach (Vector2 vertex in a_poly1.Vertices)
            {
                if (a_poly2.Contains(vertex))
                {
                    resultVertices.Add(vertex);
                }
            }

            foreach (Vector2 vertex in a_poly2.Vertices)
            {
                if (a_poly1.Contains(vertex))
                {
                    resultVertices.Add(vertex);
                }
            }

            foreach (LineSegment seg1 in a_poly1.Segments())
            {
                foreach (LineSegment seg2 in a_poly2.Segments())
                {
                    var intersection = seg1.Intersect(seg2);
                    if (intersection.HasValue)
                    {
                        resultVertices.Add(intersection.Value);
                    }
                }
            }
            if (resultVertices.Count >= 3)
            {
                return ConvexPolygonFromPoints(resultVertices);
            }
            return null;

        }


        /// <summary>
        /// A implementation of the Weiler-Atherthon algorithm that cuts away the provided clippling area from the subject polygon
        /// </summary>
        /// <param name="a_subject"></param>
        /// <param name="a_clip"></param>
        /// <returns></returns>
        public static Polygon2DWithHoles CutOut(Polygon2D a_subject, Polygon2D a_clip)
        {
            var subjectList = WeilerAthertonList(a_subject, a_clip);
            var clipList = new LinkedList<Vector2>(WeilerAthertonList(a_clip, a_subject).Reverse());
            var intersectionList = WeilerAthertonIntersectionList(a_subject, a_clip);

            Debug.Assert(subjectList != null);

            if (intersectionList.Count == 0)
            {
                //either polygons is entirly inside the other, or they are disjoint.
                //We can't have vision polygons entirly inside each other.
                return new Polygon2DWithHoles(a_subject);
            }


            return WeilerAtherthonCutOut(subjectList, clipList, intersectionList);
        }

        /// <summary>
        /// A implementation of the Weiler-Atherthon algorithm that cuts away the provided clippling area from this polygon
        /// </summary>
        /// <param name="a_clip"></param>
        /// <returns>A new polygon that equals to this with a_clip cut away</returns>
        public Polygon2DWithHoles CutOut(Polygon2D a_clip)
        {
            return CutOut(this, a_clip);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a_subjectList"></param>
        /// <param name="a_clipList"></param>
        /// <param name="a_startVertexList">A list of points where we start tracing shapes. Double startpoints on the same shape will be removed. </param>
        /// <returns></returns>
        /// 
        ///NOTE: An eventual interesecting implementation of the Weiler-Atherthon algorithm should use selection on the startvertices. (i.e. taking only inbound edges, see also wikipedia)
        private static Polygon2DWithHoles WeilerAtherthonCutOut(LinkedList<Vector2> a_subjectList, LinkedList<Vector2> a_clipList, List<Vector2> a_startVertexList)
        {
            throw new NotSupportedException();
            /*
            var result = new Polygon2DWithHoles();

            //PERF organise intersections better so i have to loop less over the lists
            while (a_startVertexList.Count > 0)
            {
                Vector2 start = a_startVertexList[0];
                var activeList = WAList.Subject;
                var singlePolyVertices = new List<Vector2>() { start };
                LinkedListNode<Vector2> startnode = a_subjectList.Find(start);

                if (startnode == null) //fallback
                {
                    foreach (Vector2 vertex in a_subjectList)
                    {
                        if (Vector2.Distance(vertex, start) < 2 * m_eps) //more liberal then Equals
                        {
                            startnode = a_subjectList.Find(vertex);
                        }
                    }
                }

                Debug.Assert(startnode != null, startnode);
                LinkedListNode<Vector2> workingvertex = startnode.Next;

                if (workingvertex == null) { workingvertex = a_subjectList.First; }

                while (workingvertex.Value != start)
                {
                    singlePolyVertices.Add(workingvertex.Value);

                    //check if workingvertex is an intersection, and in that case cross
                    LinkedListNode<Vector2> intersection;
                    if (activeList == WAList.Subject)
                    {
                        intersection = a_clipList.Find(workingvertex.Value);
                        //fallback
                        var iterationvertex = a_clipList.First;
                        while (iterationvertex != null)
                        {
                            if (Vector2.Distance(workingvertex.Value, iterationvertex.Value) < m_eps)
                            {
                                intersection = iterationvertex;
                                break;
                            }
                            iterationvertex = iterationvertex.Next;
                        }

                    }
                    else
                    {
                        intersection = a_subjectList.Find(workingvertex.Value);

                        //fallback
                        var iterationvertex = a_subjectList.First;
                        while (iterationvertex != null)
                        {
                            if (Vector2.Distance(workingvertex.Value, iterationvertex.Value) < m_eps)
                            {
                                intersection = iterationvertex;
                                break;
                            }
                            iterationvertex = iterationvertex.Next;
                        }
                    }

                    if (intersection != null)
                    {
                        //toggle activeList
                        if (activeList == WAList.Subject) { activeList = WAList.Clip; } else { activeList = WAList.Subject; }
                        workingvertex = intersection.Next;
                    }
                    //otherwise, advance in own list
                    else
                    {
                        workingvertex = workingvertex.Next;
                    }
                    // if workingvertex is null we called Next at the end of the list
                    if (workingvertex == null)
                    {
                        if (activeList == WAList.Subject)
                        {
                            workingvertex = a_subjectList.First;
                        }
                        else
                        {
                            Debug.Assert(activeList == WAList.Clip);
                            workingvertex = a_clipList.First;
                        }
                    }

                    if (singlePolyVertices.Count > 10000)
                    {
                        workingvertex.Value = start;
                        singlePolyVertices.Add(workingvertex.Value);
                        //throw new GeomException("nonterminating loop");
                    }

                }
                if (singlePolyVertices.Count > 2)  //only add if we find a nontrivial polygon
                {
                    var candidatePoly = new Polygon2D(singlePolyVertices);
                    if (candidatePoly.isClockwise())
                    {
                        result.Add(candidatePoly);
                        //remove treated vertices
                        foreach (Vector2 vertex in singlePolyVertices)
                        {
                            a_startVertexList.Remove(vertex);
                        }
                    }
                    else
                    {
                        // we accidently found a hole (due to some colinearity issues)
                        //remove only starting vertex, the other vertex could yield a succes
                        a_startVertexList.Remove(start);
                    }

                }
                else
                {
                    //remove only starting vertex, the other vertex could yield a succes
                    a_startVertexList.Remove(start);
                }
            }

            return result;
            */
        }

        /// <summary>
        /// Determines wheter or not this polygon is clockwise using the Shoelace formula (in O(n))
        /// </summary>
        /// <returns></returns>
        private bool isClockwise()
        {
            var sum = 0f;
            foreach (LineSegment seg in Segments())
            {
                sum += (seg.Point2.x - seg.Point1.x) * (seg.Point2.y + seg.Point1.y);
            }

            if (sum > 0)
            {
                return true;
            }
            //Debug.Assert(sum != 0);
            return false;
        }


        /// <summary>
        /// Generates an in order list of all vertices of a_poly interlaced with it's intersections with a_intersectors in
        /// O(nm) with n the number of segments from the one polygon and m the number of segments of the other polygon
        /// </summary>
        /// <param name="a_poly"></param>
        /// <param name="a_intersector"></param>
        /// <returns></returns>
        private static LinkedList<Vector2> WeilerAthertonList(Polygon2D a_poly, Polygon2D a_intersector)
        {
            var intersectionList = new LinkedList<Vector2>();
            var intersectingSegments = a_intersector.Segments();

            foreach (LineSegment segment in a_poly.Segments())
            {
                intersectionList.AddLast(segment.Point1);
                foreach (Vector2 intersection in segment.IntersectionWithSegments(intersectingSegments)) //no AddRange for LinkedList
                {
                    //Debug.Log(segment.Line.DistanceToPoint(intersection));
                    intersectionList.AddLast(intersection);
                }
            }

            //Clear subsequent (near) duplicates from intersectionlist
            LinkedListNode<Vector2> workingvertex = intersectionList.First;
            while (workingvertex.Next != null)
            {
                if (Vector2.Distance(workingvertex.Value, workingvertex.Next.Value) < m_eps)
                {
                    intersectionList.Remove(workingvertex.Next);
                }
                else
                {
                    workingvertex = workingvertex.Next;
                }
            }
            if (Vector2.Distance(intersectionList.First.Value, intersectionList.Last.Value) < m_eps && intersectionList.Count != 1)
            {
                intersectionList.RemoveLast();
            }

            return intersectionList;
        }



        /// <summary>
        /// Generates an in order list of all vertices of a_poly interlaced with it's intersections with a_intersectors
        /// O(nm) with n the number of segments from the one polygon and m the number of segments of the other polygon
        /// 
        /// NOTE: We retrurn all inersections since edge cases and collinear sitautions are hard to detect properly.
        /// Instead we later check if a given intersection was of th ight type by checking if the resulting polygon is clockwise 
        /// </summary>
        /// <param name="a_subject"></param>
        /// <param name="a_clip"></param>
        /// <returns>Those intersections where subject enters or exits the clipping polygon</returns>
        /// 
        private static List<Vector2> WeilerAthertonIntersectionList(Polygon2D a_subject, Polygon2D a_clip)
        {
            var intersectionList = new List<Vector2>();
            var intersectingSegments = a_subject.Segments();

            foreach (LineSegment segment in a_clip.Segments())
            {
                intersectionList.AddRange(segment.IntersectionWithSegments(intersectingSegments));
            }


            if (intersectionList.Count == 0)
            {
                return intersectionList;
            }

            //Clear subsequent duplicates from intersectionlist
            var i = 0;
            while (intersectionList.Count > i + 1)
            {
                if (Vector2.Distance(intersectionList[i], intersectionList[i + 1]) < m_eps)
                {
                    intersectionList.RemoveAt(i + 1);
                }
                else
                {
                    i++;
                }
            }

            if (Vector2.Distance(intersectionList[0], intersectionList[intersectionList.Count - 1]) < m_eps && intersectionList.Count != 1)
            {
                intersectionList.RemoveAt(0);
            }


            return intersectionList;
        }

        /// <summary>
        /// Does a simple graham scan
        /// </summary>
        /// <param name="a_points"></param>
        public static Polygon2D ConvexPolygonFromPoints(List<Vector2> a_points)
        {
            if (a_points.Count <= 2)
            {
                throw new GeomException("Too little points provided");
            }

            var tuple = ComputeUpperAndLowerHull(a_points);
            var upperhull = tuple.Upperhull;
            var lowerhull = tuple.Lowerhull;

            //STITCH AND RETURN
            lowerhull.Reverse();
            return new Polygon2D(upperhull.Concat(lowerhull.GetRange(1, lowerhull.Count - 2)));
        }

        public Rect BoundingBox
        {
            get
            {
                if (m_bBox.HasValue)
                {
                    return m_bBox.Value;
                }
                var bBox = new Rect(m_vertices[0], Vector2.zero);
                foreach (var point in m_vertices.Skip(1))
                {
                    if (point.x < bBox.xMin)
                    {
                        bBox.xMin = point.x;
                    }
                    else if (point.x > bBox.xMax)
                    {
                        bBox.xMax = point.x;
                    }

                    if (point.y < bBox.yMin)
                    {
                        bBox.yMin = point.y;
                    }
                    else if (point.y > bBox.yMax)
                    {
                        bBox.yMax = point.y;
                    }
                }

                m_bBox = bBox;
                return bBox;
            }
        }

        public bool BoundingBoxIntersects(Rect a_otherRect)
        {
            return BoundingBox.Overlaps(a_otherRect);
        }

        /// <summary>
        /// Metod returning wheter threesubsequent points form a right turn. 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        private static bool IsRightTurn(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            var line = new Line(p1, p3);
            return !line.PointRightOfLine(p2);
        }

        public override string ToString()
        {
            var str = "Face: {";
            foreach (var vertex in Vertices)
            {
                str += vertex + ", ";
            }
            return str + "}";
        }

        /// <summary>
        /// Return the Line that gives the greatest distance from all points in the dual. We calculate true seperation and not vertical seperation
        /// No error cheking occurs for boundingbox faces/polygons
        /// </summary>
        /// <returns>Line with greatest distance to the points encoded by the polygon boundary </returns>
        public LineSeperationTuple LineOfGreatestMinimumSeperationInTheDual(bool a_isBoundingBoxFace)
        {
            if (!isConvex())
            {
                throw new GeomException();
            }

            var tuple = ComputeUpperAndLowerHull(Vertices.ToList());
            var upperhull = tuple.Upperhull;
            var lowerhull = tuple.Lowerhull;

            if (a_isBoundingBoxFace)
            {
                //Check if the boundingboxface has only 2 real neighbouring lines(in the dual) and return the bisector (in the primal) in this case 
                var reallines = new List<Line>();
                foreach (var line in Lines())
                {
                    if (!(float.IsInfinity(line.Slope) || MathUtil.EqualsEps(line.Slope, 0f)))
                    {
                        reallines.Add(line);
                    }
                }

                if (reallines.Count < 2)
                {
                    throw new GeomException("Found impossibly low amount of real lines");
                }
                else if (reallines.Count == 2)
                {
                    //The two reallines are two points in the primal plane
                    var averagepoint = (reallines[0].Dual() + reallines[1].Dual()) / 2;
                    var lineTroughBothPoints = reallines[0].Intersect(reallines[1]).Dual();
                    var perpLineSlope = -1 / lineTroughBothPoints.Slope;
                    var perpPoint = averagepoint + Vector2.right + Vector2.up * perpLineSlope;
                    return new LineSeperationTuple(new Line(averagepoint, perpPoint), 0);

                    //we choose separtion 0 because a line with three constraints in the outer boundingboxface is more importnat?
                    //TODO this seems untrue, explictly calculate seperation (Wrt to all soldier??)
                }
                //Otherwise we return the regular bisector
            }


            //zero is the starting corner, 1 is the first intresting point
            var upperhullIterator = 0;
            var lowerhullIterator = 0;
            Vector2 candidatePoint = new Vector2(0, 0); //dummy value
            var currentSeparation = 0f;

            float currentx = upperhull[0].x;
            float nextx;
            float currentheight = 0;
            float nextheight;

            LineSegment upperSegment = null;
            LineSegment lowerSegment = null;

            Action<float, float> testCandidatePoint = delegate (float testx, float testheight)
            {
                var trueSeperation = Mathf.Sin(Mathf.PI / 2 - Mathf.Abs(Mathf.Atan(testx))) * testheight; //Atan(x) is the angle a line of slope x makes
                if (trueSeperation > currentSeparation)
                {
                    candidatePoint = new Vector2(testx, (upperSegment.Y(testx) + lowerSegment.Y(testx)) / 2);
                    currentSeparation = trueSeperation;
                    //Debug.Log("Candidate accepted with seperation:" + trueSeperation );
                    return;
                }
                //Debug.Log("Candidate FAILED with seperation:" + trueSeperation);
            };

            //initialize segments
            lowerSegment = new LineSegment(lowerhull[0], lowerhull[1]);
            upperSegment = new LineSegment(upperhull[0], upperhull[1]);


            //Break when one of the two lists is completly traversed
            while (upperhullIterator < upperhull.Count - 1 && lowerhullIterator < lowerhull.Count - 1)
            {
                //The part between currentx(exclusive) and nextx(inclusive) is under scrutiny
                //we advance the segment that ends on the smallest x
                if (upperhull[upperhullIterator].x < lowerhull[lowerhullIterator].x)
                {
                    upperhullIterator++;
                    upperSegment = new LineSegment(upperhull[upperhullIterator - 1], upperhull[upperhullIterator]);
                }
                else
                {
                    lowerhullIterator++;
                    lowerSegment = new LineSegment(lowerhull[lowerhullIterator - 1], lowerhull[lowerhullIterator]);
                }

                if (lowerSegment.IsVertical || upperSegment.IsVertical)
                {
                    continue; //skip this iteration 
                }

                nextx = Mathf.Min(upperSegment.XInterval.Max, lowerSegment.XInterval.Max);


                nextheight = upperSegment.Y(nextx) - lowerSegment.Y(nextx);
                if (nextheight < 0)
                {
                    throw new GeomException();
                }
                testCandidatePoint(nextx, nextheight);

                //also points inbetween vertices
                float heightchange = (nextheight - currentheight) / (nextx - currentx);
                float baseheigth = nextheight - nextx * heightchange;

                float candidatex = heightchange / baseheigth;
                if (currentx < candidatex && candidatex < nextx)
                {
                    var candidateheigth = baseheigth + heightchange * candidatex;
                    testCandidatePoint(candidatex, candidateheigth);
                }


                //save variables for next iteration
                currentheight = nextheight;
                currentx = nextx;

            }

            return new LineSeperationTuple(PointLineDual.Dual(candidatePoint), currentSeparation);
        }

        private static UpperLowerHullTuple ComputeUpperAndLowerHull(List<Vector2> a_vertices)
        {
            //Sort vertices on x-coordinate
            var sortedVertices = a_vertices.OrderBy(v => v.x).ToList();

            //UPPER HULL
            //add first two points
            var upperhull = new List<Vector2>();
            upperhull.Add(sortedVertices[0]);
            upperhull.Add(sortedVertices[1]);

            //add point and check for removal
            foreach (Vector2 point in sortedVertices.Skip(2))
            {
                upperhull.Add(point);
                var n = upperhull.Count;
                while (n > 2 && !IsRightTurn(upperhull[n - 3], upperhull[n - 2], upperhull[n - 1]))
                {
                    upperhull.RemoveAt(n - 2);
                    n = upperhull.Count;
                }
            }

            //LOWER HULL
            //add first two points
            var lowerhull = new List<Vector2>();
            lowerhull.Add(sortedVertices[0]);
            lowerhull.Add(sortedVertices[1]);

            //add point and check for removal
            foreach (Vector2 point in sortedVertices.Skip(2))
            {
                lowerhull.Add(point);
                var n = lowerhull.Count;
                while (n > 2 && IsRightTurn(lowerhull[n - 3], lowerhull[n - 2], lowerhull[n - 1]))
                {
                    lowerhull.RemoveAt(n - 2);
                    n = lowerhull.Count;

                }
            }

            return new UpperLowerHullTuple(upperhull, lowerhull);
        }

        public bool isSimple()
        {
            return true; // TODO
        }

        /// <summary>
        /// A tuple containg both upper and lower hull
        /// </summary>
        private class UpperLowerHullTuple
        {
            private List<Vector2> m_lowerhull;
            private List<Vector2> m_upperhull;

            internal UpperLowerHullTuple(List<Vector2> upperhull, List<Vector2> lowerhull)
            {
                m_upperhull = upperhull;
                m_lowerhull = lowerhull;
            }

            public List<Vector2> Lowerhull
            {
                get
                {
                    return m_lowerhull;
                }
            }

            public List<Vector2> Upperhull
            {
                get
                {
                    return m_upperhull;
                }
            }
        }

        /// <summary>
        /// List the types of Weiler-AthertonLists
        /// </summary>
        private enum WAList { Subject, Clip };
    }

    public class LineSeperationTuple
    {
        private float m_seperation;
        private Line m_line;

        public float Seperation
        {
            get
            {
                return m_seperation;
            }
        }

        public Line Line
        {
            get
            {
                return m_line;
            }
        }

        public LineSeperationTuple(Line line, float currentSeparation)
        {
            m_line = line;
            m_seperation = currentSeparation;
        }
    }
}