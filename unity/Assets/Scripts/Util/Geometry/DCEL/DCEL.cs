namespace Util.Geometry.DCEL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Geometry.Graph;
    using Util.Geometry.Polygon;
    using Util.Math;

    public class DCEL
    {
        private readonly LinkedList<DCELVertex> m_Vertices;
        private readonly LinkedList<HalfEdge> m_Edges;
        private readonly LinkedList<Face> m_Faces;

        public ICollection<DCELVertex> Vertices { get { return m_Vertices; } }
        public ICollection<HalfEdge> Edges { get { return m_Edges; } }
        public ICollection<Face> Faces { get { return m_Faces; } }
        public ICollection<Face> InnerFaces { get { return m_Faces.Where(f => !f.IsOuter).ToList(); } }
        public Rect BoundingBox { get; internal set; }

        public int VertexCount { get { return m_Vertices.Count; } }
        public int EdgeCount { get { return m_Edges.Count; } }
        public int FaceCount { get { return m_Faces.Count; } }

        public Face OuterFace { get; private set; }

        /// <summary>
        /// Constructer to extend subclasses
        /// </summary>
        public DCEL()
        {
            m_Vertices = new LinkedList<DCELVertex>();
            m_Edges = new LinkedList<HalfEdge>();
            m_Faces = new LinkedList<Face>();
            OuterFace = new Face(null) { IsOuter = true };
            m_Faces.AddLast(OuterFace);
        }

        /// <summary>
        ///  Creates DCEL from arrangement of line segments
        /// </summary>
        public DCEL(IEnumerable<LineSegment> a_Segments) : this()
        {
            foreach (var segment in a_Segments)
            {
                //add line
                AddSegment(segment);

                Debug.Log("Outer: " + OuterFace.OuterComponent);
            }

            //debug stuff
            AssertWellformed();
        }

        /// <summary>
        ///  Creates DCEL from arrangement of line segments
        /// </summary>
        public DCEL(IEnumerable<Line> a_Lines) : this()
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
            if(OnEdge(a_Vertex, out a_Edge))
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
        /// <returns> The inserted Vertex
        ///If the requested insertion Vertex is on a endpoint we insert no vertex
        ///and instead return said endpoint
        ///</returns>
        public DCELVertex AddVertexInEdge(HalfEdge a_Edge, Vector2 a_Point)
        {
            if(!m_Edges.Contains(a_Edge))
            {
                throw new ArgumentException("Edge should already be part of DCEL");
            }
            if (a_Edge.From.Pos == a_Point)
            {
                throw new ArgumentException("Requested insertion in Edge on From.Pos");
            }
            if (a_Edge.To.Pos == a_Point)
            {
                throw new ArgumentException("Requested insertion in Edge on To.Pos");
            }
            if (!MathUtil.IsFinite(a_Point.x) || !MathUtil.IsFinite(a_Point.y))
            {
                throw new ArgumentException("Vertex should have a finite position");
            }

            var a_Vertex = new DCELVertex(a_Point, a_Edge.Twin);
            m_Vertices.AddLast(a_Vertex);
            var oldTo = a_Edge.To;
            a_Edge.To = a_Vertex;
            a_Edge.Twin.From = a_Vertex;

            var newedge = new HalfEdge(a_Vertex, oldTo);
            var newtwinedge = new HalfEdge(oldTo, a_Vertex);
            m_Edges.AddLast(newedge);
            m_Edges.AddLast(newtwinedge);

            Twin(newedge, newtwinedge);

            //fix pointers in the original cycle
            Chain(newedge, a_Edge.Next);
            Chain(a_Edge, newedge);

            //fix pointers in the twin cycle
            Chain(a_Edge.Twin.Prev, newtwinedge);
            Chain(newtwinedge, a_Edge.Twin);

            //set faces
            newedge.Face = newedge.Next.Face;
            newtwinedge.Face = newtwinedge.Next.Face;

            return a_Vertex;
        }

        /// <summary>
        /// Add edge between two new points.
        /// </summary>
        /// <param name="a_Point1"></param>
        /// <param name="a_Point2"></param>
        public void AddEdge(Vector2 a_Point1, Vector2 a_Point2)
        {
            AddSegment(new LineSegment(a_Point1, a_Point2));
        }

        /// <summary>
        /// Adds a line segment to the DCEL.
        /// Method that adds new vertices (if needed) and halfedges.
        /// </summary>
        /// <param name="segment"></param>
        public void AddSegment(LineSegment segment)
        {
            // check for intersections
            foreach (var e in m_Edges)
            {
                // ignore if edges has similar endpoints
                if (e.From.Pos == segment.Point1 || e.From.Pos == segment.Point2 ||
                    e.To.Pos == segment.Point1 || e.To.Pos == segment.Point2)
                    continue;

                Vector2? intersect;
                if (e.IntersectLine(segment, out intersect))
                {
                    // split line segment into two
                    AddSegment(new LineSegment(segment.Point1, (Vector2)intersect));
                    AddSegment(new LineSegment((Vector2)intersect, segment.Point2));
                    return;
                }
            }

            // find vertices at positions or add them if necessary
            DCELVertex v1, v2;
            if (!FindVertex(segment.Point1, out v1)) v1 = AddVertex(segment.Point1);
            if (!FindVertex(segment.Point2, out v2)) v2 = AddVertex(segment.Point2);

            AddEdge(v1, v2);
        }

        /// <summary>
        /// Adds a line to the dcel
        /// </summary>
        /// <param name="line"></param>
        public void AddLine(Line line)
        {
            // TODO
        }

        /// <summary>
        /// Adds an edge, consisting of two halfedges, between existing vertices in the DCEL.
        /// </summary>
        /// <remarks>
        /// VertVertices should be adjacent to a common face.
        /// </remarks>
        /// <param name="a_vertex1"></param>
        /// <param name="a_vertex2"></param>
        public void AddEdge(DCELVertex a_Vertex1, DCELVertex a_Vertex2)
        {
            if(!m_Vertices.Contains(a_Vertex1) || !m_Vertices.Contains(a_Vertex2))
            {
                throw new ArgumentException("Vertices should already be part of the DCEL");
            }

            Face face1 = GetSplittingFace(a_Vertex1, a_Vertex2);
            Face face2 = GetSplittingFace(a_Vertex2, a_Vertex1);

            if(face1 != face2)
            {
                Debug.Log("Edge between vertices not inside a common face");
            }
           
            AddEdgeInFace(a_Vertex1, a_Vertex2, face1);
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
        private Face AddEdgeInFace(DCELVertex a_Vertex1, DCELVertex a_Vertex2, Face a_Face)
        {
            var e1 = new HalfEdge(a_Vertex1, a_Vertex2);
            var e2 = new HalfEdge(a_Vertex2, a_Vertex1);

            Edges.Add(e1);
            Edges.Add(e2);

            e1.Face = a_Face;
            e2.Face = a_Face;

            Twin(e1, e2);

            // check whether a new face needs to be added
            var startedge = a_Face.OuterComponent;
            bool addNewFace;
            if (startedge == null)
            {
                a_Face.OuterComponent = e1;
                addNewFace = false;
            }
            else
            {
                addNewFace = OnCycle(startedge, a_Vertex1) && OnCycle(startedge, a_Vertex2);
            }

            if (!addNewFace && !a_Face.IsOuter)
            {
                throw new GeomException("Vertices not on given face");
            }

            // fix chaining after determining whether face needed to be added
            FixChaining(a_Vertex1, e1);
            FixChaining(a_Vertex2, e2);

            //update face reference (and add face)
            Face newface = null;
            if (addNewFace)
            {
                var innerEdge = IsCycleClockwise(e1) ? e1 : e2;

                newface = new Face(innerEdge);
                AddFace(newface);
                a_Face.OuterComponent = innerEdge.Twin; //Set the old face edge to be certainly correct (newedge side of the new edge)
                UpdateFaceInCycle(innerEdge, newface); //set the newface to be in the other part (newtwinedge side of the new edge)
                UpdateFaceInCycle(innerEdge.Twin, a_Face); //set the newface to be in the other part (newtwinedge side of the new edge)
            }

            return newface;
        }

        public Face AddFace(Face a_Face)
        {
            m_Faces.AddLast(a_Face);
            return a_Face;
        }

        public IEnumerable<HalfEdge> AdjacentEdges(DCELVertex a_Vertex1)
        {
            var edges = new List<HalfEdge>();
            foreach (var e in OutgoingEdges(a_Vertex1))
            {
                Edges.Add(e);
                Edges.Add(e.Twin);
            }

            return edges;
        }

        public IEnumerable<HalfEdge> OutgoingEdges(DCELVertex a_Vertex1)
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

        private static void Chain(HalfEdge a_First, HalfEdge a_Second)
        {
            Debug.Log("CHAIN: " + a_First + ", " + a_Second);
            a_First.Next = a_Second;
            a_Second.Prev = a_First;
        }

        private static void Twin(HalfEdge a_Edge1, HalfEdge a_Edge2)
        {
            a_Edge1.Twin = a_Edge2;
            a_Edge2.Twin = a_Edge1;
        }

        private static bool OnCycle(HalfEdge a_startedge, DCELVertex a_Vertex)
        {
            var workingedge = a_startedge;
            do
            {
                if (workingedge == null) throw new GeomException("Edge in cycle is null");
                if (workingedge.To == a_Vertex) return true;
                workingedge = workingedge.Next;
            } while (workingedge != a_startedge);
            return false;
        }

        private void FixChaining(DCELVertex a_Vertex, HalfEdge a_Edge)
        {
            List<HalfEdge> outedges = OutgoingEdges(a_Vertex).ToList();

            if (outedges.Count == 0)
            {
                //Chain(a_Edge, a_Edge.Twin);
                Chain(a_Edge.Twin, a_Edge);
                a_Vertex.Leaving = a_Edge;
                return;
            }

            outedges.Sort(EdgeAngleComparer);

            for (int i = 0; i < outedges.Count; i++)
            {
                var curEdge = outedges[i];
                var angle = MathUtil.Angle(curEdge.From.Pos, curEdge.From.Pos + new Vector2(1f, 0f), curEdge.To.Pos);
                var angle2 = MathUtil.Angle(a_Edge.From.Pos, a_Edge.From.Pos + new Vector2(1f, 0f), a_Edge.To.Pos);

                if (angle >= angle2)
                {
                    var prevEdge = outedges[MathUtil.PositiveMod(i - 1, outedges.Count)];
                    Chain(a_Edge.Twin, curEdge);
                    Chain(prevEdge.Twin, a_Edge);
                    return;
                }
            }

            Chain(a_Edge.Twin, outedges.First());
            Chain(outedges.Last().Twin, a_Edge);
        }

        private static bool IsCycleClockwise(HalfEdge a_startedge)
        {
            var workingedge = a_startedge;
            var poly = new Polygon2D();
            do
            {
                poly.AddVertex(workingedge.From.Pos);
                workingedge = workingedge.Next;
            } while (workingedge != a_startedge);
            return poly.IsClockwise();
        }

        private static void UpdateFaceInCycle(HalfEdge a_startedge, Face a_face)
        {
            var workingedge = a_startedge;
            do
            {
                workingedge.Face = a_face;
                workingedge = workingedge.Next;
            } while (workingedge != a_startedge);
        }

        private bool FindVertex(Vector2 a_Point, out DCELVertex a_Vertex)
        {
            foreach (var v in m_Vertices)
            {
                if (a_Point == v.Pos)
                {
                    a_Vertex = v;
                    return true;
                }
            }
            a_Vertex = null;
            return false;
        }

        private bool OnEdge(Vertex a_Vertex, out HalfEdge a_Edge)
        {
            foreach (var e in m_Edges)
            {
                if(e.Segment.IsOnSegment(a_Vertex.Pos))
                {
                    a_Edge = e;
                    return true;
                }
            }
            a_Edge = null;
            return false;
        }

        private Face GetSplittingFace(DCELVertex a_Vertex1, DCELVertex a_Vertex2)
        {
            List<HalfEdge> outedges = OutgoingEdges(a_Vertex1).ToList();

            if (outedges.Count == 0)
            {
                // a_Vertex1 leaving is null
                return GetContainingFace(a_Vertex1);
            }

            outedges.Sort(EdgeAngleComparer);

            for (int i = 0; i < outedges.Count; i++)
            {
                var curEdge = outedges[i];
                var angle = MathUtil.Angle(curEdge.From.Pos, curEdge.From.Pos + new Vector2(1f, 0f), curEdge.To.Pos);
                var angle2 = MathUtil.Angle(a_Vertex1.Pos, a_Vertex1.Pos + new Vector2(1f, 0f), a_Vertex2.Pos);

                if (angle >= angle2)
                {
                    return curEdge.Face;
                }
            }

            return outedges.First().Face;
        }

        private Face GetContainingFace(DCELVertex a_Vertex)
        {
            foreach (var f in m_Faces)
            {
                if (!f.IsOuter && f.Polygon.Contains(a_Vertex.Pos)) return f;
            }
            return OuterFace;
        }

        private static int EdgeAngleComparer(HalfEdge e1, HalfEdge e2)
        {
            var angle = MathUtil.Angle(e1.From.Pos, e1.From.Pos + new Vector2(1f, 0f), e1.To.Pos);
            var angle2 = MathUtil.Angle(e2.From.Pos, e2.From.Pos + new Vector2(1f, 0f), e2.To.Pos);

            return angle.CompareTo(angle2);
        }

        private void AssertWellformed()
        {
            //TODO add check for edges of length zero
            //TODO add check vertices to close

            Debug.Log(VertexCount + " " + (EdgeCount / 2) + " " + FaceCount);

            //euler formula check
            if (VertexCount - (EdgeCount / 2) + FaceCount != 2)
            { // divide by two for halfedges
                throw new GeomException("Malformed graph: Does not satisfy Euler charachteristic");
            }

            //prev-next check
            foreach (var e in m_Edges)
            {
                if (e.Prev.Next != e)
                {
                    throw new GeomException("Malformed graph: Prev/next error in" + e);
                }
                if (e.Next.Prev != e)
                {
                    throw new GeomException("Malformed graph: Next/prev error in" + e);
                }
            }

            //twin defined check
            foreach (var e in m_Edges)
            {
                if (e.Twin.Twin != e)
                {
                    throw new GeomException("Malformed graph: No or invalid twin in edge" + e);
                }
                if (e.Twin.From.Pos != e.To.Pos)
                {
                    throw new GeomException("Malformed graph: Invalid twin vertex" + e);
                }
                if (e.From.Pos != e.Twin.To.Pos)
                {
                    throw new GeomException("Malformed graph: Invalid twin vertex" + e);
                }
            }

            //cycle around a single face check
            foreach (var f in m_Faces)
            {
                var startedge = f.OuterComponent;
                var workingedge = startedge;
                do
                {
                    workingedge = workingedge.Next;
                    if (workingedge.Face != f)
                    {
                        if (!f.IsOuter) Debug.Log(f.Polygon);
                        throw new GeomException(
                            "Malformed graph: Unexpected face incident to edge" + workingedge
                        );
                    }
                } while (workingedge != startedge);
            }

            //control from from/to vertices of next edges
            foreach (var e in m_Edges)
            {
                if (e.Prev.To.Pos != e.From.Pos)
                {
                    throw new GeomException("Malformed graph: Prev.to/from error in" + e);
                }
                if (e.Next.From.Pos != e.To.Pos)
                {
                    throw new GeomException("Malformed graph: next.from/to error in" + e);
                }
            }
        }
    }
}
