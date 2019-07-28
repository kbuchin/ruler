namespace Util.Geometry.DCEL
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry.Graph;
    using Util.Math;

    public struct PositionOnEdge
    {
        public Vector2 Pos;
        public HalfEdge Edge;

        public PositionOnEdge(Vector2 p, HalfEdge e)
        {
            Pos = p;
            Edge = e;
        }
    }

    public class DCEL
    {
        private readonly LinkedList<Vertex> m_Vertices;
        private readonly LinkedList<HalfEdge> m_Edges;
        private readonly LinkedList<Face> m_Faces;

        public ICollection<Vertex> Vertices { get { return m_Vertices; } }
        public ICollection<HalfEdge> Edges { get { return m_Edges; } }
        public ICollection<Face> Faces { get { return m_Faces; } }
        public Rect BoundingBox { get; internal set; }

        public Face OuterFace { get; private set; }

        /// <summary>
        /// Creates DCEL from bounding box (only outer border)
        /// </summary>
        public DCEL(Rect a_bBox)
        {
            var topleft = new Vector2(a_bBox.xMin, a_bBox.yMax);
            var topright = new Vector2(a_bBox.xMax, a_bBox.yMax);
            var downleft = new Vector2(a_bBox.xMin, a_bBox.yMin);
            var downright = new Vector2(a_bBox.xMax, a_bBox.yMin);

            var vertices = new List<Vertex>() {
                new Vertex(topleft),
                new Vertex(topright),
                new Vertex(downright),
                new Vertex(downleft)
            };
            CreateFullClockwiseCycleFromVertices(vertices);

            BoundingBox = a_bBox;
        }

        /// <summary>
        ///  Creates DCEL from arrangement of lines
        /// </summary>
        public DCEL(IEnumerable<Line> a_lines, Rect a_bBox) : this(a_bBox)
        {
            foreach (var line in a_lines)
            {
                //add line
                AddLine(line);

                //debug stuff
                AssertWellformed();
            }
        }

        /// <summary>
        /// Constructer to extend subclasses
        /// </summary>
        public DCEL()
        { }

        /// <summary>
        /// Adds a line to a DCEL with a bounding box
        /// </summary>
        /// <param name="line"></param>
        public void AddLine(Line line)
        {
            //first find intersections on the outer face
            var startedge = OuterFace.OuterComponent;
            var intersections = new List<PositionOnEdge>();
            var workingedge = startedge;
            do
            {
                var intersection = workingedge.IntersectLine(line);
                if (intersection != null)
                {
                    intersections.Add(new PositionOnEdge((Vector2)intersection, workingedge));
                }
                workingedge = workingedge.Next;
            } while (workingedge != startedge);


            //TODO Choice here actually doesn't matter (It is more like top and bottom anyway)
            PositionOnEdge leftintersection;
            PositionOnEdge rightintersection;
            if (intersections.Count < 2) //We can find more intersections if the line croses a corner
            {
                throw new GeomException("Didn't find the right amount of interections on the outer face");
            }
            if (intersections[0].Pos.x < intersections[1].Pos.x)
            {
                leftintersection = intersections[0];
                rightintersection = intersections[1];
            }
            else
            {
                leftintersection = intersections[1];
                rightintersection = intersections[0];
            }


            intersections = new List<PositionOnEdge>
            {
                leftintersection
            };
            var faces = new List<Face>();
            workingedge = leftintersection.Edge.Twin;

            while (workingedge.Face != OuterFace)
            {
                Vector2? intersection = null;
                while (intersection == null)
                {
                    workingedge = workingedge.Next;
                    intersection = workingedge.IntersectLine(line);
                };

                intersections.Add(new PositionOnEdge((Vector2)intersection, workingedge));
                faces.Add(workingedge.Face);

                workingedge = workingedge.Twin; //move workingedge to right position to contiue search
            }

            if (intersections.Count != faces.Count + 1)
            {
                throw new GeomException("Unexpected number of vertices/faces");
            }

            var vertices = new List<Vertex>();
            foreach (var intersect in intersections)
            {
                vertices.Add(AddVertexInEdge(intersect.Edge, new Vertex(intersect.Pos)));
            }

            for (var i = 0; i < intersections.Count - 1; i++)
            {
                AddEdgeInFace(vertices[i], vertices[i + 1], faces[i]);
            }
        }

        /// <summary>
        /// Creates a full cyle (i.e inner and outer face and twinning of vertices). This only works in a empty dcel
        /// </summary>
        protected void CreateFullClockwiseCycleFromVertices(List<Vertex> a_vertices)
        {
            var interiorEdges = ClockwiseCycle(a_vertices);
            a_vertices.Reverse();
            var outerEdges = ClockwiseCycle(a_vertices);

            //now twin them
            var workingedge = interiorEdges.Last.Value;
            Twin(workingedge, outerEdges.Last.Value);

            workingedge = workingedge.Next;
            while (workingedge != interiorEdges.Last.Value)
            {
                var twintobe = workingedge.Prev.Twin.Prev;
                Twin(workingedge, twintobe);

                workingedge = workingedge.Next;
            }

            m_Faces.Clear();
            m_Faces.AddLast(outerEdges.First.Value.Face);
            m_Faces.AddLast(interiorEdges.First.Value.Face);
            OuterFace = outerEdges.First.Value.Face;

            m_Edges.Clear();
            foreach (var e in outerEdges) m_Edges.AddLast(e);
            foreach (var e in interiorEdges) m_Edges.AddLast(e);

            m_Vertices.Clear();
            foreach (var v in a_vertices) m_Vertices.AddLast(v);
        }


        private LinkedList<HalfEdge> ClockwiseCycle(List<Vertex> a_vertices)
        {
            //returns all Halfedges in the cycle, the newly created interior face is then easaliy found
            //all properties are set except for
            var edges = new LinkedList<HalfEdge>();

            for (var i = 0; i < a_vertices.Count - 1; i++)
            {
                edges.AddLast(new HalfEdge(a_vertices[i], a_vertices[i + 1]));
            }
            edges.AddLast(new HalfEdge(a_vertices[a_vertices.Count - 1], a_vertices[0]));

            var face = new Face(edges.First.Value);

            //set aditional properties
            var it = edges.GetEnumerator();
            while(it.MoveNext())
            {
                Chain(it.Current, it.Current.Next);
                it.Current.Face = face;
            }
            Chain(edges.Last.Value, edges.First.Value);
            edges.Last.Value.Face = face;

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

        public static void Chain(HalfEdge a_first, HalfEdge a_second)
        {
            a_first.Next = a_second;
            a_second.Prev = a_first;
        }

        public static void Twin(HalfEdge a_edge1, HalfEdge a_edge2)
        {
            a_edge1.Twin = a_edge2;
            a_edge2.Twin = a_edge1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a_Edge"></param>
        /// <param name="a_Vertex"></param>
        /// <param name="a_dcel"></param>
        /// <returns> The inserted Vertex
        ///If the requested insertion Vertex is on a endpoint we insert no vertex
        ///and instead return said endpoint
        ///</returns>
        public Vertex AddVertexInEdge(HalfEdge a_Edge, Vertex a_Vertex)
        {
            if (a_Edge.From.Pos == a_Vertex.Pos)
            {
                Debug.Log("Requested insertion in Edge on From.Pos");
                return a_Edge.From;
            }

            if (a_Edge.To.Pos == a_Vertex.Pos)
            {
                Debug.Log("Requested insertion in Edge on To.Pos");
                return a_Edge.To;
            }
            if (!MathUtil.isFinite(a_Vertex.Pos.x) || !MathUtil.isFinite(a_Vertex.Pos.y)) 
            {
                throw new GeomException("Vertex should have a finite position");
            }

            Vertices.Add(a_Vertex);
            var oldTo = a_Edge.To;
            a_Edge.To = a_Vertex;
            a_Edge.Twin.From = a_Vertex;

            var newedge = new HalfEdge(a_Vertex, oldTo);
            var newtwinedge = new HalfEdge(oldTo, a_Vertex);
            Edges.Add(newedge);
            Edges.Add(newtwinedge);

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
        /// Creates a edge from two provided vertices already in the face boundary. 
        /// The face is spilt into two. One of the two faces will stay the old face, 
        /// the other face gets a newly generated face object
        /// </summary>
        /// <param name="a_vertex1"></param>
        /// <param name="a_vertex2"></param>
        /// <param name="a_face"></param>
        /// <param name="a_dcel"></param>
        /// <returns> The new Face </returns>
        public Face AddEdgeInFace(Vertex a_vertex1, Vertex a_vertex2, Face a_face)
        {
            var startedge = a_face.OuterComponent;
            var workingedge = startedge;

            HalfEdge to1 = null, from1 = null, to2 = null, from2 = null;
            do
            {
                if (workingedge.To.Pos == a_vertex1.Pos)
                {
                    to1 = workingedge;
                    from1 = to1.Next;
                }
                if (workingedge.To.Pos == a_vertex2.Pos)
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
            try
            {
                newedge = new HalfEdge(a_vertex1, a_vertex2);
                Edges.Add(newedge);
                Chain(to1, newedge);
                Chain(newedge, from2);

                newtwinedge = new HalfEdge(a_vertex2, a_vertex1);
                Edges.Add(newtwinedge);
                Chain(to2, newtwinedge);
                Chain(newtwinedge, from1);
            }
            catch (Exception)
            {
                throw new GeomException("Failure inserting edge");
            }
            Twin(newedge, newtwinedge);

            //update face reference (and add face)
            var newface = new Face(newtwinedge);
            Faces.Add(newface);
            newedge.Face = a_face;
            a_face.OuterComponent = newedge; //Set the old face to be certainly one part  (newedge side of the new edge)
            UpdateFaceInCycle(newtwinedge, newface); //set the newface to be in the other part (newtwinedge side of the new edge)

            /*
            if (Faces.Count > 100)
            {
                throw new System.Exception("More faces then expected");
            }
            */
            return newface;
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
    }
}
