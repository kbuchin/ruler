using System.Collections.Generic;
using System;
using UnityEngine;

namespace Algo.DCEL
{

    public class DCEL
    {
        public List<Vector2> Vertices { get{ return m_vertices; } }
        public List<Halfedge> Edges { get { return m_edges; } }
        public List<Face> Faces { get { return m_faces; } }
        public Rect BoundingBox { get { return m_boundingBox.Value; } }


        private List<Vector2> m_vertices;
        private List<Halfedge> m_edges;
        private List<Face> m_faces;
        private List<Face> m_midllefaces;
        private Rect? m_boundingBox;

        public Face outerface { get; private set; }

        /// <summary>
        /// Creates DCEL from bounding box (only outer border)
        /// </summary>
        public DCEL(Rect a_bBox )
        {
            var topleft = new Vector2(a_bBox.xMin, a_bBox.yMax);
            var topright = new Vector2(a_bBox.xMax, a_bBox.yMax);
            var downleft = new Vector2(a_bBox.xMin, a_bBox.yMin);
            var downright = new Vector2(a_bBox.xMax, a_bBox.yMin);

            var vertices = new List<Vector2>() { topleft, topright, downright, downleft };
            createFullClockwiseCylceFromVertices(vertices);

            m_boundingBox = a_bBox;
        }

        /// <summary>
        ///  Creates DCEL from arrangement of lines
        /// </summary>
        public DCEL(List<Line> a_lines, Rect a_bBox) : this(a_bBox)
        {
            for (var i = 0; i < a_lines.Count; i++)
            {
                //add line
                addLine(a_lines[i]);

                //debug stuff
                AssertWellformed();
            }
        }

        /// <summary>
        /// Constructer to extend subclasses
        /// </summary>
        protected DCEL()
        {

        }

        /// <summary>
        /// Adds a line to a DCEL with a bounding box
        /// </summary>
        /// <param name="line"></param>
        private void addLine(Line line)
        {
            //first find intersections on the outer face
            var startedge = outerface.OuterComponent;
            var intersections = new List<PositionOnEdge>();
            var workingedge = startedge;
            do
            {
                var intersection = workingedge.intersectLine(line);
                if (intersection != null)
                {
                    intersections.Add(intersection);
                }
                workingedge = workingedge.Next;
            } while (workingedge != startedge);


            //TODO Choiche here actually does'n matter (It is more like top and bottom anyway)
            PositionOnEdge leftintersection;
            PositionOnEdge rightintersection;
            if (intersections.Count < 2) //We can find more intersections if the line croses a corner
            {
                throw new AlgoException("Didn't find the right amount of interections on the outer face");
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


            intersections = new List<PositionOnEdge>();
            intersections.Add(leftintersection);
            var faces = new List<Face>();
            workingedge = leftintersection.Edge.Twin;



            while (workingedge.Face != outerface)
            {
                PositionOnEdge intersection = null;
                while (intersection == null)
                {
                    workingedge = workingedge.Next;
                    intersection = workingedge.intersectLine(line);
                };

                workingedge = workingedge.Twin; //move workingedge to right position to contiue search

                //then add more
                intersections.Add(intersection);
                faces.Add(intersection.Edge.Face);
            }

            if (intersections.Count != faces.Count + 1)
            {
                throw new System.Exception("Unexpected number of vertices/faces");
            }

            var vertices = new List<Vector2>();
            for (var i = 0; i < intersections.Count; i++)
            {
                vertices.Add(DCELHelper.insertVertexInEdge(intersections[i], this));
            }

            for (var i = 0; i < intersections.Count - 1; i++)
            {
                DCELHelper.addEdgeInFace(vertices[i], vertices[i + 1], faces[i], this);
            }
        }

        /// <summary>
        /// Creates a full cyle (i.e inner and outer face and twinning of vertices). This only works in a empty dcel
        /// </summary>
        protected void createFullClockwiseCylceFromVertices(List<Vector2> a_vertices)
        {
            var interiorEdges = ClockwiseCycle(a_vertices);
            a_vertices.Reverse();
            var outerEdges = ClockwiseCycle(a_vertices);

            //now twin them
            var workingedge = interiorEdges[interiorEdges.Count - 1];
            DCELHelper.Twin(workingedge, outerEdges[outerEdges.Count - 1]);

            workingedge = workingedge.Next;
            while (workingedge != interiorEdges[interiorEdges.Count - 1])
            {
                var twintobe = workingedge.Prev.Twin.Prev;
                DCELHelper.Twin(workingedge, twintobe);

                workingedge = workingedge.Next;
            }


            m_faces = new List<Face>(2);
            m_faces.Add(outerEdges[0].Face);
            m_faces.Add(interiorEdges[0].Face);
            outerface = outerEdges[0].Face;


            outerEdges.AddRange(interiorEdges);
            m_edges = outerEdges;

            m_vertices = new List<Vector2>(a_vertices);
        }


        private List<Halfedge> ClockwiseCycle(List<Vector2> a_vertices)
        {
            //returns all Halfedges in the cycle, the newly created interior face is then easaliy found
            //all properties are set except for
            var edges = new List<Halfedge>();

            for (var i = 0; i < a_vertices.Count - 1; i++)
            {
                edges.Add(new Halfedge(a_vertices[i], a_vertices[i + 1]));
            }
            edges.Add(new Halfedge(a_vertices[a_vertices.Count - 1], a_vertices[0]));

            Face face = new Face(edges[0]);

            //set aditional properties
            for (var i = 0; i < edges.Count - 1; i++)
            {
                DCELHelper.Chain(edges[i], edges[i + 1]);
                edges[i].Face = face;
            }
            DCELHelper.Chain(edges[edges.Count - 1], edges[0]);
            edges[edges.Count - 1].Face = face;


            return edges;
        }

        private bool AssertWellformed()
        {
            var isCorrect = true;
            var i=0;

            //TODO add check for edges of length zero
            //TODO add check vertices to close


            //euler formula check
            //Debug.Log("Faces " + m_faces.Count + "HalfEdges" + m_edges.Count + "vertices" + m_vertices.Count);
            if (m_vertices.Count - (m_edges.Count / 2) + m_faces.Count != 2)
            { // divide by two for halfedges
                Debug.LogError("Does not satisfy Euler charachteristic");
                isCorrect = false;
            }

            //prev-next check
            for (i = 0; i < m_edges.Count; i++)
            {
                if (m_edges[i].Prev.Next != m_edges[i])
                {
                    Debug.LogError("Prev/next error in" + i);
                    isCorrect = false;
                }
                if (m_edges[i].Next.Prev != m_edges[i])
                {
                    Debug.LogError("nect/prev error in" + i);
                    isCorrect = false;
                }
            }

            //twin defined check
            for (i = 0; i < m_edges.Count; i++)
            {
                if (m_edges[i].Twin.Twin != m_edges[i])
                {
                    Debug.LogError("no or invalid twin in edge" + i);
                    isCorrect = false;
                }
                if (m_edges[i].Twin.Fromvertex != m_edges[i].Tovertex)
                {
                    Debug.LogError("invalid twin vertex" + i);
                    isCorrect = false;
                }
                if (m_edges[i].Fromvertex != m_edges[i].Twin.Tovertex)
                {
                    Debug.LogError("invalid twin vertex" + i);
                    isCorrect = false;
                }
            }

            //cycle around a single face check
            for (i = 0; i < m_faces.Count; i++)
            {
                var startedge = m_faces[i].OuterComponent;
                var workingedge = startedge;
                var edgelist = new List<int>();
                edgelist.Add(m_edges.IndexOf(workingedge));
                do
                {
                    workingedge = workingedge.Next;
                    if (workingedge.Face != m_faces[i])
                    {
                        Debug.LogError("Unexpected face incident to edge" + m_edges.IndexOf(workingedge));
                        isCorrect = false;
                    }
                    else {
                        edgelist.Add(m_edges.IndexOf(workingedge));
                    }
                } while (workingedge != startedge);
            }


            //control from from/to vertices of next edges
            for (i = 0; i < m_edges.Count; i++)
            {
                if (m_edges[i].Prev.Tovertex != m_edges[i].Fromvertex)
                {
                    Debug.LogError("Prev.to/from error in" + i);
                  isCorrect = false;
                }
                if (m_edges[i].Next.Fromvertex != m_edges[i].Tovertex)
                {
                    Debug.LogError("next.from/to error in" + i);
                    isCorrect = false;
                }
            }

            if (!isCorrect)
            {
                throw new System.Exception("Malformed graph");
            }
            return isCorrect;
         }

        private bool isEdgeLeadingToTopMostVertex(Halfedge edge)
        {
            var epsilon = 0.0005f;
            if (edge.Fromvertex.y - epsilon <= edge.Tovertex.y && edge.Next.Fromvertex.y >= edge.Next.Tovertex.y - epsilon)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        ///  Returns a list of k/2 level faces counted from the middleleft point 
        ///  NOTE! We assume we travel from below and upwards
        /// </summary>
        /// <returns></returns>
        public List<Face> middleFaces()
        {
            if (m_midllefaces == null)
            {
                //returns the faces in which the dual point may lie to represent a cut
                // cutting a given army in two equal parts in the primal plane.
                var workingedge = outerface.OuterComponent;
                var bbox = m_boundingBox.Value;

                var lineIntersectionEdges = new List<Halfedge>();                //Will contain edges whose fromvertex is an intersection with a line

                while (!(MathUtil.EqualsEps(workingedge.Fromvertex.x , bbox.xMin) && workingedge.Fromvertex.y>0 && workingedge.Tovertex.y <0 ))
                {
                    workingedge = workingedge.Next;
                }
                //only one edge satisfies the above conditions the edge on the left boundray first crossing the origin line.

                workingedge = workingedge.Next; //workingedge is now the first edge with both from.y and to.y <0

                while (workingedge.Fromvertex.y < 0)
                {
                    if (MathUtil.EqualsEps(workingedge.Fromvertex.y, bbox.yMin) && (MathUtil.EqualsEps(workingedge.Fromvertex.x, bbox.xMin) || MathUtil.EqualsEps(workingedge.Fromvertex.x, bbox.xMax)))
                    {
                        //fromvertex is a corner. Do not add to prevent duplicity
                    }
                    else { 
                        lineIntersectionEdges.Add(workingedge);
                    }
                    workingedge = workingedge.Next;
                }

                if (lineIntersectionEdges.Count % 2 == 1)
                {
                    Debug.LogError("Unexpected odd number of lineIntersectionedges  " + lineIntersectionEdges.Count);
                }

                //TODO Assumption, feasibleFaces are arrenged in a vertical manner!

                var middleedge = lineIntersectionEdges[(lineIntersectionEdges.Count / 2 ) -1];
                var startingFace = middleedge.Twin.Face;
                var midllefaces = new List<Face>();
                midllefaces.Add(startingFace);

                //itrate trough the faces until we hit the outer face again
                workingedge = middleedge.Twin;
                while (true)
                {
                    var dbstartedge = workingedge;
                    while (!isEdgeLeadingToTopMostVertex(workingedge))
                    {
                        workingedge = workingedge.Next;
                        Debug.Assert((workingedge != dbstartedge), "OMG returned to starting Edge");
                    }
                    if (workingedge.Twin.Face == outerface)
                    {
                        //hit the left or right side
                        break;
                    }
                    workingedge = workingedge.Twin.Prev.Twin;  // Goes from  *\/ to \/*
                    if (workingedge.Face == outerface)
                    {
                        break;
                    }
                    else {
                        midllefaces.Add(workingedge.Face);
                        if (midllefaces.Count > 100)
                        {
                            throw new System.Exception("Unexpected large amount of feasible faces");
                        }
                    }
                }
                m_midllefaces = midllefaces;
            }
            return m_midllefaces;

        }

    }
}


