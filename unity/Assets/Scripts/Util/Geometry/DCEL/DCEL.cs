namespace Util.Geometry.DCEL
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry.Graph;
    using Util.Math;

    public class DCEL
    {
        private readonly LinkedList<DCELVertex> m_Vertices;
        private readonly LinkedList<HalfEdge> m_Edges;
        private readonly LinkedList<Face> m_Faces;

        public ICollection<DCELVertex> Vertices { get { return m_Vertices; } }
        public ICollection<HalfEdge> Edges { get { return m_Edges; } }
        public ICollection<Face> Faces { get { return m_Faces; } }
        public Rect BoundingBox { get; internal set; }

        public Face OuterFace { get; private set; }

        /// <summary>
        /// Constructer to extend subclasses
        /// </summary>
        public DCEL()
        {
            m_Vertices = new LinkedList<DCELVertex>();
            m_Edges = new LinkedList<HalfEdge>();
            m_Faces = new LinkedList<Face>();
            OuterFace = new Face(null);
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
            if (!MathUtil.isFinite(a_Point.x) || !MathUtil.isFinite(a_Point.y))
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
            if (!FindVertex(segment.Point1, out v2)) v2 = AddVertex(segment.Point2);

            var face = GetCommonFace(v1, v2);
            if (face != null)
            {
                AddEdgeInFace(v1, v2, face);
            }
            else
            {
                // we assume one or both of the vertices lies inside a face

                face = GetContainingFace(v1);

                var e1 = new HalfEdge(v1, v2);
                var e2 = new HalfEdge(v2, v1);
                m_Edges.AddLast(e1);
                m_Edges.AddLast(e2);
                e1.Face = face;
                e2.Face = face;

                Twin(e1, e2);

                // chain edges adjacent to v1
                if(v1.Leaving == null)
                {
                    v1.Leaving = e1;
                    Chain(e2, e1);
                }
                else
                {
                    // search for adjacent edges with same adjacent face
                    foreach (var e in AdjacentEdges(v1))
                    {
                        if(e.Face == face)
                        {
                            if (e.To == v1) Chain(e, e1);
                            else Chain(e2, e);
                        }
                    }
                }

                // chain edges adjacent to v2
                if(v2.Leaving == null)
                {
                    v2.Leaving = e2;
                    Chain(e1, e2);
                }
                else
                {
                    // search for adjacent edges with same adjacent face
                    foreach (var e in AdjacentEdges(v2))
                    {
                        if (e.Face == face)
                        {
                            if (e.To == v2) Chain(e, e2);
                            else Chain(e1, e);
                        }
                    }
                }
            }
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

            Face a_Face = GetCommonFace(a_Vertex1, a_Vertex2);
            if (a_Face == null)
            {
                throw new ArgumentException("Vertices should have a common adjacent face");
            }

            AddEdgeInFace(a_Vertex1, a_Vertex2, a_Face);
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
            var startedge = a_Face.OuterComponent;
            var workingedge = startedge;

            HalfEdge to1 = null, from1 = null, to2 = null, from2 = null;
            do
            {
                if (workingedge.To.Pos == a_Vertex1.Pos)
                {
                    to1 = workingedge;
                    from1 = to1.Next;
                }
                if (workingedge.To.Pos == a_Vertex2.Pos)
                {
                    to2 = workingedge;
                    from2 = to2.Next;
                }
                workingedge = workingedge.Next;
            } while (workingedge != startedge);

            if (to1 == null || from1 == null || to2 == null || from2 == null)
            {
                //TODO how can this happen?
                throw new GeomException(
                    "Vertices do appear to not lie on the boundary of the provided face"
                );
            }

            HalfEdge newedge, newtwinedge;

            newedge = new HalfEdge(a_Vertex1, a_Vertex2);
            Edges.Add(newedge);
            Chain(to1, newedge);
            Chain(newedge, from2);

            newtwinedge = new HalfEdge(a_Vertex2, a_Vertex1);
            Edges.Add(newtwinedge);
            Chain(to2, newtwinedge);
            Chain(newtwinedge, from1);

            Twin(newedge, newtwinedge);

            //update face reference (and add face)
            var newface = new Face(newtwinedge);
            Faces.Add(newface);
            newedge.Face = a_Face;
            a_Face.OuterComponent = newedge; //Set the old face to be certainly one part  (newedge side of the new edge)
            UpdateFaceInCycle(newtwinedge, newface); //set the newface to be in the other part (newtwinedge side of the new edge)

            /*
            if (Faces.Count > 100)
            {
                throw new System.Exception("More faces then expected");
            }
            */
            return newface;
        }

        public Face AddFace(Face a_Face)
        {
            m_Faces.AddLast(a_Face);
            return a_Face;
        }

        public IEnumerable<HalfEdge> AdjacentEdges(DCELVertex a_Vertex1)
        {
            if (a_Vertex1.Leaving == null) return null;

            var edges = new List<HalfEdge>();
            var e = a_Vertex1.Leaving;

            do
            {
                edges.Add(e);
                edges.Add(e.Twin);
                e = e.Twin.Next;
            } while (e != a_Vertex1.Leaving);

            return edges;
        }

        private void AssertWellformed()
        {
            //TODO add check for edges of length zero
            //TODO add check vertices to close


            //euler formula check
            //Debug.Log("Faces " + Faces.Count + "HalfEdges" + Edges.Count + "vertices" + Vertices.Count);
            if (Vertices.Count - (Edges.Count / 2) + Faces.Count != 2)
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
                    throw new GeomException("Malformed graph: nect/prev error in" + e);
                }
            }

            //twin defined check
            foreach (var e in m_Edges)
            {
                if (e.Twin.Twin != e)
                {
                    throw new GeomException("Malformed graph: no or invalid twin in edge" + e);
                }
                if (e.Twin.From.Pos != e.To.Pos)
                {
                    throw new GeomException("Malformed graph: invalid twin vertex" + e);
                }
                if (e.From.Pos != e.Twin.To.Pos)
                {
                    throw new GeomException("Malformed graph: invalid twin vertex" + e);
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

        private static void Chain(HalfEdge a_First, HalfEdge a_Second)
        {
            a_First.Next = a_Second;
            a_Second.Prev = a_First;
        }

        private static void Twin(HalfEdge a_Edge1, HalfEdge a_Edge2)
        {
            a_Edge1.Twin = a_Edge2;
            a_Edge2.Twin = a_Edge1;
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

        private Face GetCommonFace(DCELVertex a_Vertex1, DCELVertex a_Vertex2)
        {
            foreach (var e1 in AdjacentEdges(a_Vertex1))
            {
                foreach (var e2 in AdjacentEdges(a_Vertex2))
                {
                    if (e1.Face == e2.Face) return e1.Face;
                }
            }

            return null;
        }

        private Face GetContainingFace(DCELVertex a_Vertex)
        {
            foreach (var f in m_Faces)
            {
                if (!f.IsOuter && f.Polygon.Contains(a_Vertex.Pos)) return f;
            }
            return OuterFace;
        }
    }
}
