namespace Util.Geometry.DCEL
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Geometry.Polygon;
    using Util.Math;

    /// <summary>
    /// Implementation of a standard DCEL.
    /// 
    /// Each edge is represented as two halfedges, which are connected in cycles to create explicit faces.
    /// The inner faces are stored in clockwise order (CW), while the outer face is counter-clockwise (CCW).
    /// 
    /// Each halfedge stores a pointer to its next and previous edges and its adjacent face.
    /// The vertices store one outgoing halfedge for easy iteration.
    /// The face stores one halfedge of the outer boundary (unless outer face)
    /// and a list of halfedges of innercomponents, one halfedge for each inner faces
    /// </summary>
    /// <remarks>
    /// TODO: Add implementation for removing vertices/edges.
    /// </remarks>
    public class DCEL
    {
        private readonly LinkedList<DCELVertex> m_Vertices = new LinkedList<DCELVertex>();
        private readonly LinkedList<HalfEdge> m_Edges = new LinkedList<HalfEdge>();
        private readonly LinkedList<Face> m_Faces = new LinkedList<Face>();

        // store bounding box and its edges, possibly used for initialization DCEL
        public readonly Rect? InitBoundingBox;

        public ICollection<DCELVertex> Vertices { get { return m_Vertices; } }
        public ICollection<HalfEdge> Edges { get { return m_Edges; } }
        public ICollection<Face> Faces { get { return m_Faces; } }
        public ICollection<Face> InnerFaces { get { return m_Faces.Where(f => !f.IsOuter).ToList(); } }

        public Rect BoundingBox
        {
            get { return BoundingBoxComputer.FromPoints(Vertices.Select(v => v.Pos)); }
        }

        public int VertexCount { get { return m_Vertices.Count; } }
        public int EdgeCount { get { return (int)(m_Edges.Count / 2f); } }
        public int HalfEdgeCount { get { return m_Edges.Count; } }
        public int FaceCount { get { return m_Faces.Count; } }

        /// <summary>
        /// Stores the outer face explicitly.
        /// </summary>
        public Face OuterFace { get; private set; }

        /// <summary>
        /// Constructer to extend subclasses
        /// </summary>
        public DCEL()
        {
            OuterFace = new Face(null) { IsOuter = true };
            m_Faces.AddLast(OuterFace);
        }

        /// <summary>
        /// Creates a dcel and initializes it with a bounding rectangle.
        /// Useful whenever you want certain faces to be bounded, e.g. in Voronoi diagram.
        /// </summary>
        /// <param name="a_bBox"></param>
        public DCEL(Rect a_bBox) : this()
        {
            if (MathUtil.EqualsEps(a_bBox.width, 0f) || MathUtil.EqualsEps(a_bBox.height, 0f))
            {
                throw new GeomException("Bounding box is invalid");
            }

            // calculate four bounding vertices
            var topleft = new Vector2(a_bBox.xMin, a_bBox.yMax);
            var topright = new Vector2(a_bBox.xMax, a_bBox.yMax);
            var downright = new Vector2(a_bBox.xMax, a_bBox.yMin);
            var downleft = new Vector2(a_bBox.xMin, a_bBox.yMin);

            var v1 = AddVertex(topleft);
            var v2 = AddVertex(topright);
            var v3 = AddVertex(downright);
            var v4 = AddVertex(downleft);

            AddEdge(v1, v2);
            AddEdge(v2, v3);
            AddEdge(v3, v4);
            AddEdge(v4, v1);

            InitBoundingBox = a_bBox;

            //debug stuff
            AssertWellformed();
        }

        /// <summary>
        ///  Creates DCEL from arrangement of line segments
        /// </summary>
        public DCEL(IEnumerable<LineSegment> a_Segments, Rect a_bBox) : this(a_bBox)
        {
            foreach (var segment in a_Segments)
            {
                //add line
                AddSegment(segment);
            }

            //debug stuff
            AssertWellformed();
        }

        /// <summary>
        ///  Creates DCEL from arrangement of line segments
        /// </summary>
        public DCEL(IEnumerable<Line> a_Lines, Rect a_bBox) : this(a_bBox)
        {
            foreach (var line in a_Lines)
            {
                //add line
                AddLine(line);
            }

            //debug stuff
            AssertWellformed();
        }

        /// <summary>
        /// Create a new DCELVertex at the specified position.
        /// </summary>
        /// <param name="a_Point"></param>
        /// <returns>the added vertex</returns>
        public DCELVertex AddVertex(Vector2 a_Point)
        {
            return AddVertex(new DCELVertex(a_Point));
        }

        /// <summary>
        /// Adds a given DCELVertex to the DCEL.
        /// </summary>
        /// <param name="a_Vertex"></param>
        /// <returns>The added vertex</returns>
        public DCELVertex AddVertex(DCELVertex a_Vertex)
        {
            HalfEdge a_Edge;
            if (OnEdge(a_Vertex, out a_Edge))
            {
                return AddVertexInEdge(a_Edge, a_Vertex.Pos);
            }
            else
            {
                m_Vertices.AddLast(a_Vertex);
                return a_Vertex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a_Edge"></param>
        /// <param name="a_Vertex"></param>
        /// <returns> The inserted Vertex.
        /// If the requested insertion Vertex is on a endpoint we insert no vertex
        /// and instead return said endpoint
        ///</returns>
        public DCELVertex AddVertexInEdge(HalfEdge a_Edge, Vector2 a_Point)
        {
            if (!m_Edges.Contains(a_Edge))
            {
                throw new GeomException("Edge should already be part of DCEL");
            }
            if (!a_Edge.Segment.IsOnSegment(a_Point))
            {
                throw new GeomException("Point should lie on edge");
            }
            if (MathUtil.EqualsEps(a_Edge.From.Pos, a_Point))
            {
                return a_Edge.From;
                //throw new GeomException("Requested insertion in Edge on From.Pos");
            }
            if (MathUtil.EqualsEps(a_Edge.To.Pos, a_Point))
            {
                return a_Edge.To;
                //throw new GeomException("Requested insertion in Edge on To.Pos");
            }

            // create vertex with outgoing edge
            var a_Vertex = new DCELVertex(a_Point, a_Edge.Twin);
            m_Vertices.AddLast(a_Vertex);

            // update old edge pointers
            var oldTo = a_Edge.To;
            a_Edge.To = a_Vertex;
            a_Edge.Twin.From = a_Vertex;

            // create new halfedges
            var newedge = new HalfEdge(a_Vertex, oldTo);
            var newtwinedge = new HalfEdge(oldTo, a_Vertex);
            m_Edges.AddLast(newedge);
            m_Edges.AddLast(newtwinedge);
            Twin(newedge, newtwinedge);

            //fix next/prev pointers in the original cycle
            Chain(newedge, a_Edge.Next);
            Chain(a_Edge, newedge);

            //fix pointers in the twin cycle
            Chain(a_Edge.Twin.Prev, newtwinedge);
            Chain(newtwinedge, a_Edge.Twin);

            //set faces
            newedge.Face = a_Edge.Face;
            newtwinedge.Face = a_Edge.Twin.Face;

            return a_Vertex;
        }

        /// <summary>
        /// Add edge between two new points.
        /// </summary>
        /// <param name="a_Point1"></param>
        /// <param name="a_Point2"></param>
        /// <returns> One of the newly added edges. </returns>
        public HalfEdge AddEdge(Vector2 a_Point1, Vector2 a_Point2)
        {
            return AddSegment(new LineSegment(a_Point1, a_Point2));
        }

        /// <summary>
        /// Adds a line segment to the DCEL.
        /// Method that adds new vertices (if needed) and halfedges.
        /// </summary>
        /// <param name="segment"></param>
        /// <returns> One of the new halfedges. </returns>
        public HalfEdge AddSegment(LineSegment segment)
        {
            var segmentVertices = new HashSet<DCELVertex>
            {
                // add endpoints
                AddVertex(segment.Point1),
                AddVertex(segment.Point2)
            };

            // avoid concurrent modification
            var intersectingEdges = new List<HalfEdge>();

            // store intersections unique
            // needed since we already added endpoint vertices
            var intersectingVertices = new HashSet<Vector2>();

            foreach (var edge in m_Edges)
            {
                // find proper intersection (not through vertex)
                var intersection = edge.Segment.Intersect(segment);
                if (intersection.HasValue)
                {
                    // check for inproper intersections with edge
                    if (MathUtil.EqualsEps(edge.From.Pos, intersection.Value))
                    {
                        segmentVertices.Add(edge.From);
                    }
                    else if (MathUtil.EqualsEps(edge.To.Pos, intersection.Value))
                    {
                        segmentVertices.Add(edge.To);
                    }
                    else if (!intersectingVertices.Contains(intersection.Value))
                    {
                        intersectingEdges.Add(edge);
                        intersectingVertices.Add(intersection.Value);
                    }
                }
            }

            // add new vertices for proper intersections
            foreach (var edge in intersectingEdges)
            {
                segmentVertices.Add(AddVertexInEdge(edge, edge.Segment.Intersect(segment).Value));
            }

            // resolve intersections in x order
            var orderedIntersections = segmentVertices
                .OrderBy(x => x.Pos.x)
                .ThenByDescending(x => x.Pos.y)    // in case of vertical lines
                .ToList();

            HalfEdge ret = null;
            for (var i = 0; i < orderedIntersections.Count - 1; i++)
            {
                ret = AddEdge(orderedIntersections[i], orderedIntersections[i + 1]);
            }
            return ret;
        }

        /// <summary>
        /// Adds a line to the dcel.
        /// </summary>
        /// <param name="line"> One of the newly added edges. </param>
        public HalfEdge AddLine(Line line)
        {
            // find intersections of line with dcel
            // use set to disregard multiple same intersections
            // (e.g. inproper intersections at a vertex)
            var intersections = new HashSet<DCELVertex>();

            // avoid concurrent modification
            var intersectingEdges = new List<HalfEdge>();

            foreach (var edge in m_Edges)
            {
                // find proper intersection (not through vertex)
                var intersection = edge.Segment.Intersect(line);
                if (intersection.HasValue)
                {
                    // check for inproper intersections
                    if (MathUtil.EqualsEps(edge.From.Pos, intersection.Value))
                    {
                        intersections.Add(edge.From);
                    }
                    else if (MathUtil.EqualsEps(edge.To.Pos, intersection.Value))
                    {
                        intersections.Add(edge.To);
                    }
                    else
                    {
                        intersectingEdges.Add(edge);
                    }
                }
            }

            // add new vertices for proper intersections
            foreach (var edge in intersectingEdges)
            {
                intersections.Add(AddVertexInEdge(edge, edge.Segment.Intersect(line).Value));
            }

            // resolve intersections in x order
            var orderedIntersections = intersections
                .OrderBy(v => v.Pos.x)
                .ThenByDescending(x => x.Pos.y)    // in case of vertical lines
                .ToList();

            HalfEdge ret = null;
            for (var i = 0; i < orderedIntersections.Count - 1; i++)
            {
                ret = AddEdge(orderedIntersections[i], orderedIntersections[i + 1]);
            }
            return ret;
        }

        /// <summary>
        /// Adds an edge, consisting of two halfedges, between existing vertices in the DCEL.
        /// </summary>
        /// <remarks>
        /// Vertices should be adjacent to a common face.
        /// </remarks>
        /// <param name="a_vertex1"></param>
        /// <param name="a_vertex2"></param>
        /// <returns> One of the newly added edges </returns>
        public HalfEdge AddEdge(DCELVertex a_Vertex1, DCELVertex a_Vertex2)
        {
            if (!m_Vertices.Contains(a_Vertex1) || !m_Vertices.Contains(a_Vertex2))
            {
                throw new GeomException("Vertices should already be part of the DCEL");
            }

            // create edges
            var e1 = new HalfEdge(a_Vertex1, a_Vertex2);
            var e2 = new HalfEdge(a_Vertex2, a_Vertex1);
            m_Edges.AddLast(e1);
            m_Edges.AddLast(e2);

            bool newFace = false;
            Face face1, face2;

            // check if both vertices already part of face
            // or disconnected inside a face
            if (OutgoingEdges(a_Vertex1).Count() != 0 && OutgoingEdges(a_Vertex2).Count() != 0)
            {
                // get faces split by new edge from each vertex
                face1 = GetSplittingFace(a_Vertex1, a_Vertex2.Pos);
                face2 = GetSplittingFace(a_Vertex2, a_Vertex1.Pos);

                // check if new edge will create additional face
                var newInnerFace = face1.InnerComponents.Exists(e => OnCycle(e, a_Vertex1) && OnCycle(e, a_Vertex2));
                var outerFaceSplit = !face1.IsOuter &&
                    OnCycle(face1.OuterComponent, a_Vertex1) &&
                    OnCycle(face1.OuterComponent, a_Vertex2);
                newFace = newInnerFace || outerFaceSplit;
            }
            else if (OutgoingEdges(a_Vertex2).Count() != 0)
            {
                face1 = GetContainingFace(a_Vertex1.Pos);
                face2 = GetSplittingFace(a_Vertex2, a_Vertex1.Pos);
            }
            else if (OutgoingEdges(a_Vertex1).Count() != 0)
            {
                face1 = GetSplittingFace(a_Vertex1, a_Vertex2.Pos);
                face2 = GetContainingFace(a_Vertex2.Pos);
            }
            else
            {
                face1 = GetContainingFace(a_Vertex1.Pos);
                face2 = GetContainingFace(a_Vertex2.Pos);

                // new inner component inside face
                face1.InnerComponents.Add(e1);
            }

            if (face1 != face2)
            {
                throw new GeomException("Vertices do not lie in the same face:\n"
                    + a_Vertex1 + "\n" + a_Vertex2 + "\n" + face1 + "\n" + face2);
            }

            // fix edge pointers
            e1.Face = face1;
            e2.Face = face1;
            Twin(e1, e2);
            AddEdgeInVertexChain(a_Vertex1, e1);
            AddEdgeInVertexChain(a_Vertex2, e2);

            if (newFace)
            {
                face2 = SplitFace(e1, e2, face1);
            }
            else
            {
                face2 = null;
            }

            // check whether inner component has become part of outer component
            // or inner components have merged
            FixInnerComponents(e1, e2, face1, face2);

            return e1;
        }

        /// <summary>
        /// Creates a edge from two provided vertices already in the face boundary. 
        /// The face is split into two. One of the two faces will stay the old face, 
        /// the other face gets a newly generated face object
        /// </summary>
        /// <param name="a_Vertex1"></param>
        /// <param name="a_Vertex2"></param>
        /// <param name="a_Face"></param>
        /// <param name="a_dcel"></param>
        /// <returns> The new Face </returns>
        private Face SplitFace(HalfEdge e1, HalfEdge e2, Face a_Face)
        {
            var innerEdge = IsCycleClockwise(e1) ? e1 : e2;

            var newface = new Face(innerEdge);
            AddFace(newface);

            if (IsCycleClockwise(innerEdge.Twin))
            {
                // edges form two new face from one
                // replace old outercomponent of a_Face
                a_Face.OuterComponent = innerEdge.Twin;
            }

            //set the newface to be in the other part
            UpdateFaceInCycle(innerEdge, newface);
            UpdateFaceInCycle(innerEdge.Twin, a_Face);

            return newface;
        }

        /// <summary>
        /// Adds a new face to the collection.
        /// </summary>
        /// <remarks>
        /// Assumes its edges are already added to the DCEL.
        /// Only use to store the new face in the face list.
        /// </remarks>
        /// <param name="a_Face"></param>
        /// <returns>the newly added face</returns>
        private Face AddFace(Face a_Face)
        {
            m_Faces.AddLast(a_Face);
            return a_Face;
        }

        /// <summary>
        /// Find all half edges adjacent to the given vertex, ingoing as well as outgoing.
        /// </summary>
        /// <param name="a_Vertex1"></param>
        /// <returns></returns>
        public List<HalfEdge> AdjacentEdges(DCELVertex a_Vertex1)
        {
            var edges = new List<HalfEdge>();
            foreach (var e in OutgoingEdges(a_Vertex1))
            {
                edges.Add(e);
                edges.Add(e.Twin);
            }

            return edges;
        }


        /// <summary>
        /// Finds a vertex with the given location, or null otherwise.
        /// </summary>
        /// <remarks>
        /// Slow method O(n), not recommended.
        /// </remarks>
        /// <param name="a_Point"></param>
        /// <param name="a_Vertex"></param>
        /// <returns></returns>
        public bool FindVertex(Vector2 a_Point, out DCELVertex a_Vertex)
        {
            a_Vertex = m_Vertices.FirstOrDefault(v => a_Point.Equals(v.Pos));
            return a_Vertex != null;
        }

        /// <summary>
        /// Returns all edges that are outgoing from the given vertex.
        /// </summary>
        /// <param name="a_Vertex1"></param>
        /// <returns></returns>
        public List<HalfEdge> OutgoingEdges(DCELVertex a_Vertex1)
        {
            if (a_Vertex1.Leaving == null) return new List<HalfEdge>();

            var edges = new List<HalfEdge>();
            var e = a_Vertex1.Leaving;
            do
            {
                edges.Add(e);
                e = e.Twin.Next;
            } while (e != a_Vertex1.Leaving);

            return edges;
        }

        /// <summary>
        /// Chains the two edges together in sequence.
        /// Sets prev/next pointers.
        /// </summary>
        /// <param name="a_First"></param>
        /// <param name="a_Second"></param>
        private static void Chain(HalfEdge a_First, HalfEdge a_Second)
        {
            a_First.Next = a_Second;
            a_Second.Prev = a_First;
        }

        /// <summary>
        /// Sets the two edges as each others twins.
        /// </summary>
        /// <param name="a_Edge1"></param>
        /// <param name="a_Edge2"></param>
        private static void Twin(HalfEdge a_Edge1, HalfEdge a_Edge2)
        {
            a_Edge1.Twin = a_Edge2;
            a_Edge2.Twin = a_Edge1;
        }

        /// <summary>
        /// Checks whether the given vertex lies on the edge cycle specified by the halfedge.
        /// </summary>
        /// <param name="a_startedge"></param>
        /// <param name="a_Vertex"></param>
        /// <returns></returns>
        private static bool OnCycle(HalfEdge a_startedge, DCELVertex a_Vertex)
        {
            return Cycle(a_startedge).ToList().Exists(e => e.To == a_Vertex);
        }

        /// <summary>
        /// Fix the edge chaining from the given vertex and insert the half edge.
        /// Updates the next/prev data from the adjacent edges of a_Vertex
        /// to include the new edge.
        /// </summary>
        /// <param name="a_Vertex"></param>
        /// <param name="a_Edge"></param>
        private void AddEdgeInVertexChain(DCELVertex a_Vertex, HalfEdge a_Edge)
        {
            List<HalfEdge> outedges = OutgoingEdges(a_Vertex).ToList();

            if (outedges.Count == 0)
            {
                // Add initial edge to vertex
                Chain(a_Edge.Twin, a_Edge);
                a_Vertex.Leaving = a_Edge;
                return;
            }

            outedges.Sort(EdgeAngleComparer);

            // loop over edges in order of angle
            for (int i = 0; i < outedges.Count; i++)
            {
                var curEdge = outedges[i];
                var angle = MathUtil.Angle(curEdge.From.Pos, curEdge.From.Pos + new Vector2(1f, 0f), curEdge.To.Pos);
                var angle2 = MathUtil.Angle(a_Edge.From.Pos, a_Edge.From.Pos + new Vector2(1f, 0f), a_Edge.To.Pos);

                if (angle >= angle2)
                {
                    // chain new edge correctly
                    var prevEdge = outedges[MathUtil.PositiveMod(i - 1, outedges.Count)];
                    Chain(a_Edge.Twin, curEdge);
                    Chain(prevEdge.Twin, a_Edge);
                    return;
                }
            }

            // new edge between first and last edge
            Chain(a_Edge.Twin, outedges.FirstOrDefault());
            Chain(outedges.Last().Twin, a_Edge);
        }

        /// <summary>
        /// Fix the inner component of the oldface, after adding two halfedges.
        /// 
        /// Will redistribute inner components whenever a face was split into.
        /// Additionally, removes inner components that have been joined to other components.
        /// </summary>
        /// <param name="e1"></param>
        /// <param name="e2"></param>
        /// <param name="oldFace"></param>
        /// <param name="newFace"></param>
        private void FixInnerComponents(HalfEdge e1, HalfEdge e2, Face oldFace, Face newFace)
        {
            // remove all inner components that were affected by adding halfedges
            oldFace.InnerComponents.RemoveAll(e => e.Face != oldFace || Cycle(e).Contains(e1) || Cycle(e).Contains(e2));

            // re-add one of halfedges if both not part of outer component
            if (oldFace.IsOuter || !(Cycle(oldFace.OuterComponent).Contains(e1) | Cycle(oldFace.OuterComponent).Contains(e2)))
            {
                oldFace.InnerComponents.Add(e1.Face == oldFace ? e1 : e2);
            }

            // redistribute inner components that are now contained inside new face
            if (newFace != null)
            {
                var componentsToSwap = oldFace.InnerComponents.Where(e => newFace.Contains(e.From.Pos) &&
                        !(Cycle(e).Contains(e1) || Cycle(e).Contains(e2)))
                    .ToList();
                foreach (var halfedge in componentsToSwap)
                {
                    if (halfedge.Segment.Intersect(e1.Segment) != null)
                    {
                        throw new GeomException("inner component intersects with new edge: " + halfedge.Segment +
                            "\n" + e1.Segment + "\n" + newFace.Polygon.Outside);
                    }

                    // swap inner components
                    oldFace.InnerComponents.Remove(halfedge);
                    newFace.InnerComponents.Add(halfedge);

                    // update face pointers
                    UpdateFaceInCycle(halfedge, newFace);
                }
            }

        }

        /// <summary>
        /// Check whether the cycle specified by a_startedge is clockwise.
        /// </summary>
        /// <param name="a_startedge"></param>
        /// <returns></returns>
        private static bool IsCycleClockwise(HalfEdge a_startedge)
        {
            return new Polygon2D(Cycle(a_startedge).Select(e => e.From.Pos)).IsClockwise();
        }

        /// <summary>
        /// Updats the face pointers in the cycle starting from given halfedge to the new face.
        /// </summary>
        /// <param name="a_startedge"></param>
        /// <param name="a_face"></param>
        private static void UpdateFaceInCycle(HalfEdge a_startedge, Face a_face)
        {
            foreach (var e in Cycle(a_startedge))
            {
                e.Face = a_face;
            }
        }

        /// <summary>
        /// Checks whether the given vertex lies on the cycle
        /// </summary>
        /// <param name="a_Vertex"></param>
        /// <param name="a_Edge"></param>
        /// <returns></returns>
        private bool OnEdge(DCELVertex a_Vertex, out HalfEdge a_Edge)
        {
            a_Edge = m_Edges.FirstOrDefault(e => e.Segment.IsOnSegment(a_Vertex.Pos));
            return a_Edge != null;
        }

        /// <summary>
        /// Given a initial vertex and a new point, finds the face that is split by a new edge in this direction
        /// </summary>
        /// <param name="a_vertex"></param>
        /// <param name="a_point"></param>
        /// <returns></returns>
        private Face GetSplittingFace(DCELVertex a_vertex, Vector2 a_point)
        {
            List<HalfEdge> outedges = OutgoingEdges(a_vertex).ToList();

            if (outedges.Count == 0)
            {
                // a_Vertex1 leaving is null
                throw new GeomException("Vertex should be connected to a face boundary");
            }

            outedges.Sort(EdgeAngleComparer);

            foreach (var curEdge in outedges)
            {
                var angle = MathUtil.Angle(a_vertex.Pos, a_vertex.Pos + new Vector2(1f, 0f), curEdge.To.Pos);
                var angle2 = MathUtil.Angle(a_vertex.Pos, a_vertex.Pos + new Vector2(1f, 0f), a_point);

                if (angle >= angle2)
                {
                    return curEdge.Face;
                }
            }

            return outedges.FirstOrDefault().Face;
        }

        /// <summary>
        /// Finds the face that contains the given point
        /// </summary>
        /// <param name="a_point"></param>
        /// <returns></returns>
        public Face GetContainingFace(Vector2 a_point)
        {
            // find if contained in any inner face
            foreach (var f in InnerFaces)
            {
                if (f.Polygon.ContainsInside(a_point))
                    return f;
            }

            // else point in outer face
            return OuterFace;
        }

        /// <summary>
        /// Returns an enumerable of all edges on the cycle starting at the given edge.
        /// </summary>
        /// <param name="a_Edge"></param>
        /// <returns></returns>
        public static List<HalfEdge> Cycle(HalfEdge a_Edge)
        {
            if (a_Edge == null) return new List<HalfEdge>();

            var edges = new List<HalfEdge>() { a_Edge };
            var curedge = a_Edge.Next;
            while (!curedge.Equals(a_Edge))
            {
                edges.Add(curedge);
                curedge = curedge.Next;
            }
            return edges;
        }

        /// <summary>
        /// Compares to halfedges based on angle.
        /// </summary>
        /// <param name="e1"></param>
        /// <param name="e2"></param>
        /// <returns></returns>
        private static int EdgeAngleComparer(HalfEdge e1, HalfEdge e2)
        {
            var angle = MathUtil.Angle(e1.From.Pos, e1.From.Pos + new Vector2(1f, 0f), e1.To.Pos);
            var angle2 = MathUtil.Angle(e2.From.Pos, e2.From.Pos + new Vector2(1f, 0f), e2.To.Pos);

            return angle.CompareTo(angle2);
        }

        /// <summary>
        /// Debug method that checks whether various conditions and assumptions hold.
        /// </summary>
        private void AssertWellformed()
        {
            // check initial state
            if (VertexCount == 0)
            {
                if (EdgeCount != 0 || FaceCount != 1)
                {
                    throw new GeomException("Malformed graph: Should have no edges and one face");
                }
                return;
            }

            //prev-next check
            foreach (var e in m_Edges)
            {
                if (e.Prev.Next != e)
                {
                    throw new GeomException("Malformed graph: Prev/next error in, " + e);
                }
                if (e.Next.Prev != e)
                {
                    throw new GeomException("Malformed graph: Next/prev error in, " + e);
                }
                if (MathUtil.EqualsEps(e.Magnitude, 0f))
                {
                    throw new GeomException("Malformed graph: Edge of length zero, " + e);
                }
            }

            //control from from/to vertices of next edges
            foreach (var e in m_Edges)
            {
                if (e.Prev.To.Pos != e.From.Pos)
                {
                    throw new GeomException("Malformed graph: Prev.to/from error in, " + e);
                }
                if (e.Next.From.Pos != e.To.Pos)
                {
                    throw new GeomException("Malformed graph: next.from/to error in, " + e);
                }
            }

            // zero length edge check
            foreach (var e in m_Edges)
            {
                if (MathUtil.EqualsEps(e.Magnitude, 0f))
                {
                    throw new GeomException("Malformed graph: Edge of length zero, " + e);
                }
            }

            //twin defined check
            foreach (var e in m_Edges)
            {
                if (e.Twin.Twin != e)
                {
                    throw new GeomException("Malformed graph: No or invalid twin in edge, " + e);
                }
                if (e.Twin.From.Pos != e.To.Pos)
                {
                    throw new GeomException("Malformed graph: Invalid twin vertex, " + e);
                }
                if (e.From.Pos != e.Twin.To.Pos)
                {
                    throw new GeomException("Malformed graph: Invalid twin vertex, " + e);
                }
            }

            // check edge face 
            foreach (var e in m_Edges)
            {
                if (!(e.Face.OuterHalfEdges.Contains(e) || e.Face.InnerHalfEdges.Contains(e)))
                {
                    throw new GeomException("Malformed graph: edge face field mismatch with face components, " + e);
                }
            }

            // check for single outer face
            foreach (var f in m_Faces)
            {
                if (f.IsOuter && f != OuterFace)
                {
                    throw new GeomException("Malformed graph: More than one outer face, " + f);
                }
            }

            //cycle around outer face check
            foreach (var f in m_Faces)
            {
                foreach (var e in f.OuterHalfEdges)
                {
                    if (e.Face != f)
                    {
                        throw new GeomException("Malformed graph: Unexpected face incident to edge, " + e);
                    }
                }
            }

            // cycle around inner components check
            foreach (var f in m_Faces)
            {
                foreach (var e in f.InnerHalfEdges)
                {
                    if (e.Face != f)
                    {
                        throw new GeomException("Malformed graph: Unexpected face incident to edge, " + e);
                    }
                }
            }
        }
    }
}
