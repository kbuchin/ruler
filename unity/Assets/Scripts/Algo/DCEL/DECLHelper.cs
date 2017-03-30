using System;
using System.Collections.Generic;
using UnityEngine;
namespace Algo.DCEL {
    public class DCELHelper
    {

        public static void Chain(Halfedge a_first, Halfedge a_second)
        {
            a_first.Next = a_second;
            a_second.Prev = a_first;
        }

        public static void Twin(Halfedge a_edge1, Halfedge a_edge2)
        {
            a_edge1.Twin = a_edge2;
            a_edge2.Twin = a_edge1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a_edge"></param>
        /// <param name="a_pos"></param>
        /// <param name="a_dcel"></param>
        /// <returns> The inserted Vertex
        ///If the requested insertion Position is on a endpoint we insert no vertex
        ///and instead return said endpoint
        ///</returns>
        internal static Vector2 InsertVertexInEdge(Halfedge a_edge, Vector2 a_pos, DCEL a_dcel)
        {
            if (a_edge.Fromvertex == a_pos)
            {
                Debug.Log("Requested insertion in Edge on fromvertex");
                return a_edge.Fromvertex;
            }

            if (a_edge.Tovertex == a_pos)
            {
                Debug.Log("Requested insertion in Edge on tovertex");
                return a_edge.Tovertex;
            }
            if (float.IsNaN(a_pos.x) || float.IsNaN(a_pos.y))
            {
                throw new AlgoException("Unexpected NaN");
            }


            var vertex = new Vector2(a_pos.x, a_pos.y);
            a_dcel.Vertices.Add(vertex);
            var oldToVertex = a_edge.Tovertex;
            a_edge.Tovertex = vertex;
            a_edge.Twin.Fromvertex = vertex;

            var newedge = new Halfedge(vertex, oldToVertex);
            var newtwinedge = new Halfedge(oldToVertex, vertex);
            a_dcel.Edges.Add(newedge);
            a_dcel.Edges.Add(newtwinedge);

            Twin(newedge, newtwinedge);

            //fix pointers in the original cycle
            Chain(newedge, a_edge.Next);
            Chain(a_edge, newedge);


            //fix pointers in the twin cycle
            Chain(a_edge.Twin.Prev, newtwinedge);
            Chain(newtwinedge, a_edge.Twin);

            //set faces
            newedge.Face = newedge.Next.Face;
            newtwinedge.Face = newtwinedge.Next.Face;

            return vertex;
        }

        /// <summary>
        /// Convience method
        /// </summary>
        /// <param name="a_intersection"></param>
        /// <param name="a_dcel"></param>
        /// <returns></returns>
        internal static Vector2 insertVertexInEdge(PositionOnEdge a_intersection, DCEL a_dcel)
        {
            var edge = a_intersection.Edge;
            var pos = a_intersection.Pos;

            return InsertVertexInEdge(edge, pos, a_dcel);
        }

        /// <summary>
        /// Creates a edge from two provided vertices already in the face boundary. 
        /// The face is spilt into two. One of the two faces will stay the old face, the other face gets a newly generated face object
        /// </summary>
        /// <param name="a_vertex1"></param>
        /// <param name="a_vertex2"></param>
        /// <param name="a_face"></param>
        /// <param name="a_dcel"></param>
        /// <returns> The new Face </returns>
        internal static Face addEdgeInFace(Vector2 a_vertex1, Vector2 a_vertex2, Face a_face, DCEL a_dcel)
        {
            var startedge = a_face.OuterComponent;
            var workingedge = startedge;

            Halfedge to1 = null, from1 = null, to2 = null, from2 = null;
            do
            {
                if (workingedge.Tovertex == a_vertex1)
                {
                    to1 = workingedge;
                    from1 = to1.Next;
                }
                if (workingedge.Tovertex == a_vertex2)
                {
                    to2 = workingedge;
                    from2 = to2.Next;
                }
                workingedge = workingedge.Next;
            } while (workingedge != startedge);

            if (to1 == null || from1 == null || to2 == null || from2 == null)
            {
                //TODO how can this happen?
                throw new System.Exception("Vertices do appear to not lie on the boundary of the provided face");
            }

            Halfedge newedge = null, newtwinedge = null;
            try
            {
                newedge = new Halfedge(a_vertex1, a_vertex2);
                a_dcel.Edges.Add(newedge);
                Chain(to1, newedge);
                Chain(newedge, from2);

                newtwinedge = new Halfedge(a_vertex2, a_vertex1);
                a_dcel.Edges.Add(newtwinedge);
                Chain(to2, newtwinedge);
                Chain(newtwinedge, from1);
            }
            catch (Exception)
            {
                Debug.Log("error(vertex1, vertex2, face, dcel)");
                throw new System.Exception("Failure inserting edge");
            }
            Twin(newedge, newtwinedge);

            //update face reference (and add face)
            var newface = new Face(newtwinedge);
            a_dcel.Faces.Add(newface);
            newedge.Face = a_face;
            a_face.OuterComponent = newedge; //Set the old face to be certianly one part  (newedge side of the new edge)
            updateFaceInCycle(newtwinedge, newface); //set the newface to be in the other part (newtwinedge side of the new edge)

            if (a_dcel.Faces.Count > 100)    
            {
                throw new System.Exception("More faces then expeted");
            }
            return newface;
        }

        private static void updateFaceInCycle(Halfedge a_startedge, Face a_face)
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
