using System;
using System.Collections.Generic;
using UnityEngine;
using MNMatrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace VoronoiDCEL
{
    public sealed class DCEL<T>
    {
        private static int m_NextUniqueID = 0;

        private List<Vertex<T>> m_Vertices;
        private List<Edge<T>> m_Edges;
        private readonly List<HalfEdge<T>> m_HalfEdges;
        private List<Face<T>> m_Faces;
        private readonly int m_UniqueID;

        public List<Vertex<T>> Vertices { get { return m_Vertices; } }

        public List<Edge<T>> Edges { get { return m_Edges; } }

        public List<HalfEdge<T>> HalfEdges { get { return m_HalfEdges; } }

        public List<Face<T>> Faces { get { return m_Faces; } }

        public delegate void IntersectionPointAction(Intersection a_Intersection,DCEL<T> a_DCEL);

        public DCEL()
        {
            m_Vertices = new List<Vertex<T>>();
            m_Edges = new List<Edge<T>>();
            m_HalfEdges = new List<HalfEdge<T>>();
            m_Faces = new List<Face<T>>();
            m_UniqueID = m_NextUniqueID++;
        }

        public DCEL(DCEL<T> A, DCEL<T> B)
        {
            m_Vertices = new List<Vertex<T>>(A.Vertices.Count + B.Vertices.Count);
            m_Vertices.AddRange(A.Vertices);
            m_Vertices.AddRange(B.Vertices);
            m_Edges = new List<Edge<T>>(A.Edges.Count + B.Edges.Count);
            m_Edges.AddRange(A.Edges);
            m_Edges.AddRange(B.Edges);
            m_HalfEdges = new List<HalfEdge<T>>(A.HalfEdges.Count + B.HalfEdges.Count);
            m_HalfEdges.AddRange(A.HalfEdges);
            m_HalfEdges.AddRange(B.HalfEdges);
            m_Faces = new List<Face<T>>(A.Faces.Count + B.Faces.Count);
            m_Faces.AddRange(A.Faces);
            m_Faces.AddRange(B.Faces);
            m_UniqueID = m_NextUniqueID++;
        }

        public void Initialize()
        {
            ConnectHalfEdges(m_HalfEdges);
            m_Faces = CreateFaces(m_HalfEdges);
        }

        public void AddEdge(double a_x, double a_y, double b_x, double b_y)
        {
            double epsilon = Math.Exp(-9);

            HalfEdge<T> h1 = new HalfEdge<T>();
            HalfEdge<T> h2 = new HalfEdge<T>();
            Edge<T> e = new Edge<T>(h1, h2, m_UniqueID);
            h1.ParentEdge = e;
            h2.ParentEdge = e;

            h1.Twin = h2;
            h2.Twin = h1;
            Vertex<T> v1 = null;
            Vertex<T> v2 = null;
            foreach (Vertex<T> v in m_Vertices)
            {
                if (Math.Abs(v.X - a_x) < epsilon && Math.Abs(v.Y - a_y) < epsilon)
                {
                    v1 = v;
                }
                else if (Math.Abs(v.X - b_x) < epsilon && Math.Abs(v.Y - b_y) < epsilon)
                {
                    v2 = v;
                }
                if (v1 != null && v2 != null)
                {
                    foreach (HalfEdge<T> h in v1.IncidentEdges)
                    {
                        if (h.Twin.Origin == v2)
                        {
                            return;
                        }
                    }
                    break;
                }
            }
            if (v1 == null)
            {
                v1 = new Vertex<T>(a_x, a_y);
                m_Vertices.Add(v1);
            }
            if (v2 == null)
            {
                v2 = new Vertex<T>(b_x, b_y);
                m_Vertices.Add(v2);
            }
            h1.Origin = v1;
            h2.Origin = v2;
            v1.IncidentEdges.Add(h1);
            v2.IncidentEdges.Add(h2);
            m_HalfEdges.Add(h1);
            m_HalfEdges.Add(h2);
            m_Edges.Add(e);
        }

        private static void ConnectHalfEdges(List<HalfEdge<T>> a_HalfEdges)
        {
            foreach (HalfEdge<T> h in a_HalfEdges)
            {
                Vector3 edgeDirection = new Vector3((float)(h.Twin.Origin.X - h.Origin.X), (float)(h.Twin.Origin.Y - h.Origin.Y), 0);
                edgeDirection.Normalize();
                const float turnSize = 0;
                HalfEdge<T> mostLeftTurn = null;
                foreach (HalfEdge<T> h2 in h.Twin.Origin.IncidentEdges)
                {
                    if (h2 != h.Twin)
                    {
                        Vector3 nextEdgeDirection = new Vector3((float)(h2.Twin.Origin.X - h2.Origin.X), (float)(h2.Twin.Origin.Y - h2.Origin.Y), 0);
                        nextEdgeDirection.Normalize();
                        float turn = Vector3.Cross(edgeDirection, nextEdgeDirection).z;
                        if (turn <= 0)
                        {
                            float size = Mathf.Abs(Vector3.Dot(edgeDirection, nextEdgeDirection) - 1);
                            if (size >= turnSize)
                            {
                                mostLeftTurn = h2;
                            }
                        }
                    }
                }
                if (mostLeftTurn != null)
                {
                    h.Next = mostLeftTurn;
                    mostLeftTurn.Previous = h;
                }
            }
        }

        private static List<Face<T>> CreateFaces(List<HalfEdge<T>> a_HalfEdges)
        {
            List<Face<T>> faces = new List<Face<T>>();
            List<HalfEdge<T>> faceEdges = new List<HalfEdge<T>>();
            foreach (HalfEdge<T> h in a_HalfEdges)
            {
                if (h.IncidentFace == null)
                {
                    faceEdges.Add(h);
                    HalfEdge<T> curEdge = h.Next;
                    while (curEdge != h && curEdge != null)
                    {
                        faceEdges.Add(curEdge);
                        curEdge = curEdge.Next;
                    }
                    if (curEdge == h)
                    {
                        Face<T> f = new Face<T>();
                        f.StartingEdge = h;
                        foreach (HalfEdge<T> newFaceEdge in faceEdges)
                        {
                            newFaceEdge.IncidentFace = f;
                        }
                        faces.Add(f);
                    }
                    faceEdges.Clear();
                }
            }
            return faces;
        }

        public Vertex<T> AddVertexOnEdge(double a_x, double a_y, Edge<T> a_Edge)
        {
            m_Edges.Remove(a_Edge);

            Vertex<T> x = new Vertex<T>(a_x, a_y);
            m_Vertices.Add(x);
            HalfEdge<T> h1 = new HalfEdge<T>();
            HalfEdge<T> h2 = new HalfEdge<T>();

            h1.Origin = a_Edge.Half1.Origin;
            h2.Origin = x;
            x.IncidentEdges.Add(h2);

            h1.Next = h2;
            h2.Next = a_Edge.Half1.Next;

            h2.Previous = h1;
            h1.Previous = a_Edge.Half1.Previous;

            a_Edge.Half1.Next.Previous = h2;
            a_Edge.Half1.Previous.Next = h1;

            h1.IncidentFace = a_Edge.Half1.IncidentFace;
            h2.IncidentFace = a_Edge.Half1.IncidentFace;

            a_Edge.Half1.Origin.IncidentEdges.Remove(a_Edge.Half1);
            if (a_Edge.Half1.IncidentFace != null && a_Edge.Half1.IncidentFace.StartingEdge == a_Edge.Half1)
            {
                a_Edge.Half1.IncidentFace.StartingEdge = h1;
            }
            m_HalfEdges.Remove(a_Edge.Half1);
            m_HalfEdges.Add(h1);
            m_HalfEdges.Add(h2);

            // Now the second halfedge.
            HalfEdge<T> h3 = new HalfEdge<T>();
            HalfEdge<T> h4 = new HalfEdge<T>();

            h3.Origin = a_Edge.Half2.Origin;
            h4.Origin = x;
            x.IncidentEdges.Add(h4);

            h3.Next = h4;
            h4.Next = a_Edge.Half2.Next;

            h4.Previous = h3;
            h3.Previous = a_Edge.Half2.Previous;

            a_Edge.Half2.Next.Previous = h4;
            a_Edge.Half2.Previous.Next = h3;

            h3.IncidentFace = a_Edge.Half2.IncidentFace;
            h4.IncidentFace = a_Edge.Half2.IncidentFace;

            a_Edge.Half2.Origin.IncidentEdges.Remove(a_Edge.Half2);
            if (a_Edge.Half2.IncidentFace != null && a_Edge.Half2.IncidentFace.StartingEdge == a_Edge.Half2)
            {
                a_Edge.Half2.IncidentFace.StartingEdge = h3;
            }
            m_HalfEdges.Remove(a_Edge.Half2);
            m_HalfEdges.Add(h3);
            m_HalfEdges.Add(h4);

            // Connect twins.
            h1.Twin = h4;
            h4.Twin = h1;
            h2.Twin = h3;
            h3.Twin = h2;

            // Create edges.
            Edge<T> e1 = new Edge<T>(h1, h4, m_UniqueID);
            Edge<T> e2 = new Edge<T>(h2, h3, m_UniqueID);
            h1.ParentEdge = e1;
            h4.ParentEdge = e1;
            h2.ParentEdge = e2;
            h3.ParentEdge = e2;
            m_Edges.Add(e1);
            m_Edges.Add(e2);
            return x;
        }

        public void AddVertexInsideFace(double a_x, double a_y, HalfEdge<T> a_h)
        {
            // Precondition: the open line segment (v, u), where u = target(h) = origin(twin(h))
            // and where v = (a_x, a_y) lies completely in f = face(h).

            Vertex<T> v = new Vertex<T>(a_x, a_y);
            HalfEdge<T> h1 = new HalfEdge<T>();
            HalfEdge<T> h2 = new HalfEdge<T>();
            Edge<T> e = new Edge<T>(h1, h2, m_UniqueID);
            h1.ParentEdge = e;
            h2.ParentEdge = e;

            v.IncidentEdges.Add(h2);
            h1.Twin = h2;
            h2.Twin = h1;
            h2.Origin = v;
            h1.Origin = a_h.Twin.Origin;

            h1.IncidentFace = a_h.IncidentFace;
            h2.IncidentFace = a_h.IncidentFace;

            h1.Next = h2;
            h2.Next = a_h.Next;

            h1.Previous = a_h;
            h2.Previous = h1;

            a_h.Next = h1;
            h2.Next.Previous = h2;

            m_Vertices.Add(v);
            m_HalfEdges.Add(h1);
            m_HalfEdges.Add(h2);
            m_Edges.Add(e);
        }

        public void SplitFace(HalfEdge<T> a_h, Vertex<T> a_v)
        {
            // Precondition: v is incident to f = face(h) but not adjacent to 
            // u = target(h) = origin(twin(h)). And the open line segment (v, u) lies
            // completely in f.

            Vertex<T> u = a_h.Twin.Origin;
            Face<T> f = a_h.IncidentFace;
            Face<T> f1 = new Face<T>();
            Face<T> f2 = new Face<T>();
            HalfEdge<T> h1 = new HalfEdge<T>();
            HalfEdge<T> h2 = new HalfEdge<T>();
            Edge<T> e = new Edge<T>(h1, h2, m_UniqueID);
            h1.ParentEdge = e;
            h2.ParentEdge = e;

            f1.StartingEdge = h1;
            f2.StartingEdge = h2;

            h1.Twin = h2;
            h2.Twin = h1;

            h2.Origin = a_v;
            h1.Origin = u;

            h2.Next = a_h.Next;
            h2.Next.Previous = h2;

            h1.Previous = a_h;
            a_h.Next = h1;

            HalfEdge<T> i = h2;
            while (true)
            {
                i.IncidentFace = f2;
                if (i.Twin.Origin == a_v)
                {
                    break;
                }
                else
                {
                    i = i.Next;
                }
            }

            h1.Next = i.Next;
            h1.Next.Previous = h1;
            i.Next = h2;
            h2.Previous = i;
            i = h1;

            while (i.Twin.Origin != u)
            {
                i.IncidentFace = f1;
                i = i.Next;
            }

            m_Faces.Remove(f);
            m_Faces.Add(f1);
            m_Faces.Add(f2);

            m_HalfEdges.Add(h1);
            m_HalfEdges.Add(h2);
            m_Edges.Add(e);
        }

        public static DCEL<T> MapOverlay(DCEL<T> A, DCEL<T> B)
        {
            DCEL<T> overlay = new DCEL<T>(A, B);
            return MapOverlay(overlay);
        }

        public static DCEL<T> MapOverlay(DCEL<T> overlay)
        {
            Intersection[] intersections;
            overlay.FindIntersections2(out intersections, HandleMapOverlayEvent);
            // Todo: continue implementing the map overlay algorithm.
            //List<Face<T>> overlayFaces = CreateFaces(overlay.HalfEdges);
            return overlay;
        }

        private static void HandleMapOverlayEvent(Intersection a_Intersection, DCEL<T> a_DCEL)
        {
            HashSet<Edge<T>> edges = new HashSet<Edge<T>>(a_Intersection.upperEndpointEdges);
            edges.UnionWith(a_Intersection.containingEdges);
            edges.UnionWith(a_Intersection.lowerEndpointEdges);
            HashSet<Edge<T>>.Enumerator enumerator = edges.GetEnumerator();
            enumerator.MoveNext();
            int firstID = enumerator.Current.DCEL_ID;
            bool bothDCELInvolved = false;
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.DCEL_ID != firstID)
                {
                    bothDCELInvolved = true;
                    break;
                }
            }
            if (bothDCELInvolved)
            {
                Debug.Log("Found intersection between DCELs.");
                if (a_Intersection.containingEdges.Length == 1)
                {
                    a_DCEL.UpdateMapOverlayDCEL(a_Intersection.containingEdges[0], a_Intersection.point);
                    Debug.Log("Handled intersection between edge and vertex");
                }
                else if (a_Intersection.containingEdges.Length == 2)
                {
                    Vertex<T> v = a_DCEL.AddVertexOnEdge(a_Intersection.point.X, a_Intersection.point.Y, a_Intersection.containingEdges[0]);
                    a_DCEL.UpdateMapOverlayDCEL(a_Intersection.containingEdges[1], v);
                    Debug.Log("Handled intersection between two edges");
                }
                else
                {
                    Debug.Log("Unhandled intersection case!");
                }
            }
        }

        private void UpdateMapOverlayDCEL(Edge<T> e, Vertex<T> v) // e is of s1 and v is of s2.
        {
            // Delete e
            m_Edges.Remove(e);

            // Create two new half-edge records with v as the origin.
            HalfEdge<T> h1 = new HalfEdge<T>();
            HalfEdge<T> h2 = new HalfEdge<T>();
            h1.Origin = v;
            h2.Origin = v;

            // The two existing half-edges for e keep the endpoints of e as their origin
            HalfEdge<T> original1 = e.Half1;
            HalfEdge<T> original2 = e.Half2;

            // Pair up the existing half-edges with the new half-edges by setting their Twin pointers.
            h1.Twin = original1;
            original1.Twin = h1;
            h2.Twin = original2;
            original2.Twin = h2;

            // So e' is represented by one new and one existing half-edge, and the same holds for e''.
            Edge<T> e1 = new Edge<T>(original1, h1, m_UniqueID);
            Edge<T> e2 = new Edge<T>(original2, h2, m_UniqueID);

            // The Next() pointers of the two new half-edges each copy the Next() pointer of the old half-edge that is not its twin.
            h1.Next = original2.Next;
            h2.Next = original1.Next;

            // The half-edges to which these pointers point must also update their Prev() pointer and set it to the new half-edges.
            original2.Previous = h1;
            original1.Previous = h2;

            // We must set the Next() and Prev() pointers of the four half-edges representing e' and e'' and of the four half-edges incident from s2 to v.
            HalfEdge<T> clockWiseEdge;
            HalfEdge<T> counterClockwiseEdge;

            GetNextPrevEdges(h1, v.IncidentEdges, out clockWiseEdge, out counterClockwiseEdge);
            h1.Previous = counterClockwiseEdge.Twin;
            counterClockwiseEdge.Twin.Next = h1;
            original1.Next = clockWiseEdge;
            clockWiseEdge.Previous = original1;

            GetNextPrevEdges(h2, v.IncidentEdges, out clockWiseEdge, out counterClockwiseEdge);
            h2.Previous = counterClockwiseEdge.Twin;
            counterClockwiseEdge.Twin.Next = h2;
            original2.Next = clockWiseEdge;
            clockWiseEdge.Previous = original2;

            m_HalfEdges.Add(h1);
            m_HalfEdges.Add(h2);
            m_Edges.Add(e1);
            m_Edges.Add(e2);
        }

        private static void GetNextPrevEdges(HalfEdge<T> a_HalfEdge, List<HalfEdge<T>> a_IncidentEdges, out HalfEdge<T> out_ClockWiseEdge, out HalfEdge<T> out_CounterClockwiseEdge)
        {
            Vector3 a = new Vector3((float)a_HalfEdge.Twin.Origin.X - (float)a_HalfEdge.Origin.X, (float)a_HalfEdge.Twin.Origin.Y - (float)a_HalfEdge.Origin.Y, 0);
            a.Normalize();
            List<CyclicEdgeOrderElement> cyclicOrder = new List<CyclicEdgeOrderElement>(a_IncidentEdges.Count + 1);
            CyclicEdgeOrderElement e = new CyclicEdgeOrderElement(0, a_HalfEdge);
            cyclicOrder.Add(e);
            foreach (HalfEdge<T> h in a_IncidentEdges)
            {
                Vector3 b = new Vector3((float)h.Twin.Origin.X - (float)a_HalfEdge.Origin.X, (float)h.Twin.Origin.Y - (float)a_HalfEdge.Origin.Y, 0);
                b.Normalize();
                float signedArea = Vector3.Cross(a, b).z;
                cyclicOrder.Add(new CyclicEdgeOrderElement(signedArea, h));
            }
            if (cyclicOrder.Count < 3)
            {
                throw new Exception("Could not find next and prev edges: too few edges");
            }
            cyclicOrder.Sort();
            int index = cyclicOrder.FindIndex(elem => elem == e);
            if (index == -1)
            {
                throw new Exception("Could not find reference halfedge in cyclic order of incident edges");
            }
            else if (index == 0)
            {
                out_ClockWiseEdge = cyclicOrder[1].halfEdge;
                out_CounterClockwiseEdge = cyclicOrder[cyclicOrder.Count - 1].halfEdge;
            }
            else if (index == cyclicOrder.Count - 1)
            {
                out_ClockWiseEdge = cyclicOrder[0].halfEdge;
                out_CounterClockwiseEdge = cyclicOrder[cyclicOrder.Count - 2].halfEdge; 
            }
            else
            {
                out_ClockWiseEdge = cyclicOrder[index + 1].halfEdge;
                out_CounterClockwiseEdge = cyclicOrder[index - 1].halfEdge;
            }
        }

        private sealed class CyclicEdgeOrderElement : IComparable<CyclicEdgeOrderElement>
        {
            public double area;
            public HalfEdge<T> halfEdge;

            public CyclicEdgeOrderElement(double a_Area, HalfEdge<T> a_HalfEdge)
            {
                area = a_Area;
                halfEdge = a_HalfEdge;
            }

            public int CompareTo(CyclicEdgeOrderElement other)
            {
                double diff = area - other.area;
                if (diff < 0)
                {
                    return -1;
                }
                else if (diff > 0)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        public bool FindIntersections(out Intersection[] out_Intersections, IntersectionPointAction a_IntersectionPointAction = null)
        {
            AATree<Vertex<T>> eventQueue = new AATree<Vertex<T>>();
            List<Intersection> intersections = new List<Intersection>();
            foreach (Edge<T> e in m_Edges)
            {
                eventQueue.Insert(e.UpperEndpoint);
                eventQueue.Insert(e.LowerEndpoint);
            }
            StatusTree<T> status = new StatusTree<T>();
            while (eventQueue.Size != 0)
            {
                Vertex<T> p;
                if (eventQueue.FindMax(out p))
                {
                    eventQueue.Delete(p);
                    HandleEventPoint(p, status, eventQueue, intersections, this, a_IntersectionPointAction);
                }
                else
                {
                    throw new Exception("Couldn't find max node in non-empty tree!");
                }
            }
            if (intersections.Count > 0)
            {
                out_Intersections = intersections.ToArray();
                return true;
            }
            out_Intersections = null;
            return false;
        }

        // The "quick and dirty" O(n^2) intersection method.
        public bool FindIntersections2(out Intersection[] out_Intersections, IntersectionPointAction a_IntersectionPointAction = null)
        {
            List<Intersection> intersections = new List<Intersection>();
            Vertex<T> intersectionPoint;
            Edge<T> a, b;
            List<Edge<T>> edges = new List<Edge<T>>(m_Edges);
            for (int i = 0; i < edges.Count; i++)
            {
                a = edges[i];
                for (int k = i; k < edges.Count; k++)
                {
                    b = edges[k];
                    if (a.DCEL_ID == b.DCEL_ID)
                    {
                        continue;
                    }
                    if (a.UpperEndpoint.Equals(b.UpperEndpoint))
                    {
                        Intersection intersection = new Intersection();
                        intersection.point = a.UpperEndpoint;
                        intersection.containingEdges = new Edge<T>[0];
                        intersection.lowerEndpointEdges = new Edge<T>[0];
                        intersection.upperEndpointEdges = new [] { a, b };
                        intersections.Add(intersection);
                        if (a_IntersectionPointAction != null)
                        {
                            a_IntersectionPointAction(intersection, this);
                        }
                    }
                    else if (a.LowerEndpoint.Equals(b.LowerEndpoint))
                    {
                        Intersection intersection = new Intersection();
                        intersection.point = a.LowerEndpoint;
                        intersection.containingEdges = new Edge<T>[0];
                        intersection.lowerEndpointEdges = new [] { a, b };
                        intersection.upperEndpointEdges = new Edge<T>[0];
                        intersections.Add(intersection);
                        if (a_IntersectionPointAction != null)
                        {
                            a_IntersectionPointAction(intersection, this);
                        }
                    }
                    else if (a.LowerEndpoint.Equals(b.UpperEndpoint))
                    {
                        Intersection intersection = new Intersection();
                        intersection.point = a.LowerEndpoint;
                        intersection.containingEdges = new Edge<T>[0];
                        intersection.lowerEndpointEdges = new [] { a };
                        intersection.upperEndpointEdges = new [] { b };
                        intersections.Add(intersection);
                        if (a_IntersectionPointAction != null)
                        {
                            a_IntersectionPointAction(intersection, this);
                        }
                    }
                    else if (a.UpperEndpoint.Equals(b.LowerEndpoint))
                    {
                        Intersection intersection = new Intersection();
                        intersection.point = a.UpperEndpoint;
                        intersection.containingEdges = new Edge<T>[0];
                        intersection.lowerEndpointEdges = new [] { b };
                        intersection.upperEndpointEdges = new [] { a };
                        intersections.Add(intersection);
                        if (a_IntersectionPointAction != null)
                        {
                            a_IntersectionPointAction(intersection, this);
                        }
                    }
                    else if (IntersectLines(a.Half1.Origin, a.Half2.Origin, b.Half1.Origin, b.Half2.Origin, out intersectionPoint))
                    {
                        if (a.UpperEndpoint.Equals(intersectionPoint))
                        {
                            Intersection intersection = new Intersection();
                            intersection.point = a.UpperEndpoint;
                            intersection.containingEdges = new [] { b };
                            intersection.lowerEndpointEdges = new Edge<T>[0];
                            intersection.upperEndpointEdges = new [] { a };
                            intersections.Add(intersection);
                            if (a_IntersectionPointAction != null)
                            {
                                a_IntersectionPointAction(intersection, this);
                            }
                        }
                        else if (a.LowerEndpoint.Equals(intersectionPoint))
                        {
                            Intersection intersection = new Intersection();
                            intersection.point = a.LowerEndpoint;
                            intersection.containingEdges = new [] { b };
                            intersection.lowerEndpointEdges = new [] { a };
                            intersection.upperEndpointEdges = new Edge<T>[0];
                            intersections.Add(intersection);
                            if (a_IntersectionPointAction != null)
                            {
                                a_IntersectionPointAction(intersection, this);
                            }
                        }
                        else if (b.UpperEndpoint.Equals(intersectionPoint))
                        {
                            Intersection intersection = new Intersection();
                            intersection.point = b.UpperEndpoint;
                            intersection.containingEdges = new [] { a };
                            intersection.lowerEndpointEdges = new Edge<T>[0];
                            intersection.upperEndpointEdges = new [] { b };
                            intersections.Add(intersection);
                            if (a_IntersectionPointAction != null)
                            {
                                a_IntersectionPointAction(intersection, this);
                            }
                        }
                        else if (b.LowerEndpoint.Equals(intersectionPoint))
                        {
                            Intersection intersection = new Intersection();
                            intersection.point = b.LowerEndpoint;
                            intersection.containingEdges = new [] { a };
                            intersection.lowerEndpointEdges = new [] { b };
                            intersection.upperEndpointEdges = new Edge<T>[0];
                            intersections.Add(intersection);
                            if (a_IntersectionPointAction != null)
                            {
                                a_IntersectionPointAction(intersection, this);
                            }
                        }
                        else
                        {
                            Intersection intersection = new Intersection();
                            intersection.point = intersectionPoint;
                            intersection.containingEdges = new [] { a, b };
                            intersection.lowerEndpointEdges = new Edge<T>[0];
                            intersection.upperEndpointEdges = new Edge<T>[0];
                            intersections.Add(intersection);
                            if (a_IntersectionPointAction != null)
                            {
                                a_IntersectionPointAction(intersection, this);
                            }
                        }
                    }
                }
            }
            if (intersections.Count > 0)
            {
                out_Intersections = intersections.ToArray();
                return true;
            }
            out_Intersections = null;
            return false;
        }

        public sealed class Intersection
        {
            public Vertex<T> point;
            public Edge<T>[] upperEndpointEdges;
            public Edge<T>[] lowerEndpointEdges;
            public Edge<T>[] containingEdges;
        }

        private static void HandleEventPoint(Vertex<T> a_Point, StatusTree<T> a_Status,
                                             AATree<Vertex<T>> a_EventQueue, List<Intersection> intersections, DCEL<T> a_DCEL, IntersectionPointAction a_IntersectionPointAction = null)
        {
            HashSet<Edge<T>> upperEndpointEdges = new HashSet<Edge<T>>(); // U(p)
            HashSet<Edge<T>> lowerEndpointEdges = new HashSet<Edge<T>>(); // L(p)
            foreach (HalfEdge<T> h in a_Point.IncidentEdges)
            {
                if (h.ParentEdge.UpperEndpoint == a_Point)
                {
                    upperEndpointEdges.Add(h.ParentEdge);
                }
                if (h.ParentEdge.LowerEndpoint == a_Point)
                {
                    lowerEndpointEdges.Add(h.ParentEdge);
                }
            }
            HashSet<Edge<T>> containingEdges = new HashSet<Edge<T>>(a_Status.FindNodes(a_Point));
            containingEdges.ExceptWith(upperEndpointEdges);
            containingEdges.ExceptWith(lowerEndpointEdges);
            HashSet<Edge<T>> union = new HashSet<Edge<T>>(lowerEndpointEdges);
            union.UnionWith(upperEndpointEdges);
            union.UnionWith(containingEdges);
            if (union.Count > 1)
            {
                Intersection intersection = new Intersection();
                intersection.point = a_Point;
                intersection.upperEndpointEdges = new Edge<T>[upperEndpointEdges.Count];
                upperEndpointEdges.CopyTo(intersection.upperEndpointEdges);
                intersection.lowerEndpointEdges = new Edge<T>[lowerEndpointEdges.Count];
                lowerEndpointEdges.CopyTo(intersection.lowerEndpointEdges);
                intersection.containingEdges = new Edge<T>[containingEdges.Count];
                containingEdges.CopyTo(intersection.containingEdges);
                intersections.Add(intersection);
                if (a_IntersectionPointAction != null)
                {
                    a_IntersectionPointAction(intersection, a_DCEL);
                }
            }
            union = new HashSet<Edge<T>>(lowerEndpointEdges);
            union.UnionWith(containingEdges);
            foreach (Edge<T> e in union)
            {
                if (!a_Status.Delete(e, a_Point))
                {
                    Debug.Log("Could not delete lower endpoint or containing edge from status!");
                }
            }
            union = new HashSet<Edge<T>>(upperEndpointEdges);
            union.UnionWith(containingEdges);
            foreach (Edge<T> e in union)
            {
                if (!a_Status.Insert(e, a_Point))
                {
                    Debug.Log("Could not insert upper endpoint or containing edge into status!");
                }
            }
            if (union.Count == 0)
            {
                Edge<T> leftNeighbour;
                Edge<T> rightNeighbour;
                if (a_Status.FindNeighboursOfPoint(a_Point, out leftNeighbour, out rightNeighbour))
                {
                    FindNewEvent(leftNeighbour, rightNeighbour, a_Point, a_EventQueue);
                }
            }
            else
            {
                Edge<T> leftMost;
                if (a_Status.FindLeftMostSegmentInSet(union, out leftMost))
                {
                    Edge<T> leftNeighbour;
                    if (a_Status.FindNextSmallest(leftMost, a_Point, out leftNeighbour))
                    {
                        FindNewEvent(leftNeighbour, leftMost, a_Point, a_EventQueue);
                    }
                }
                else
                {
                    throw new Exception("Leftmost segment not found in status, but must exist!");
                }
                Edge<T> rightMost;
                if (a_Status.FindRightMostSegmentInSet(union, out rightMost))
                {
                    Edge<T> rightNeighbour;
                    if (a_Status.FindNextBiggest(rightMost, a_Point, out rightNeighbour))
                    {
                        FindNewEvent(rightMost, rightNeighbour, a_Point, a_EventQueue);
                    }
                }
                else
                {
                    throw new Exception("Rightmost segment not found in status, but must exist!");
                }
            }
        }

        public static void FindNewEvent(Edge<T> a, Edge<T> b, Vertex<T> point, AATree<Vertex<T>> eventQueue)
        {
            Vertex<T> intersection;
            if (IntersectLines(a.UpperEndpoint, a.LowerEndpoint, b.UpperEndpoint, b.LowerEndpoint, out intersection))
            {
                if (intersection.Y < point.Y || (Math.Abs(intersection.Y - point.Y) < Math.Exp(-9) && intersection.X > point.X))
                {
                    eventQueue.Insert(intersection);
                }
            }
        }

        public static bool IntersectLines(Vertex<T> a, Vertex<T> b, Vertex<T> c, Vertex<T> d, out Vertex<T> o_Intersection)
        {
            double numerator = Vertex<T>.Orient2D(c, d, a);
            if ((numerator * Vertex<T>.Orient2D(c, d, b) <= 0) && (Vertex<T>.Orient2D(a, b, c) * Vertex<T>.Orient2D(a, b, d) <= 0))
            {
                double[,] denominatorArray =
                    {
                        { b.X - a.X, b.Y - a.Y },
                        { d.X - c.X, d.Y - c.Y }
                    };

                MNMatrix denominatorMatrix = MNMatrix.Build.DenseOfArray(denominatorArray);
                double denominator = denominatorMatrix.Determinant();

                if (Math.Abs(denominator) < Math.Exp(-9))
                { // ab and cd are parallel or equal
                    o_Intersection = Vertex<T>.Zero;
                    return false;
                }
                else
                { // can optionally check if p is very close to b, c, or d and then flip so that a is nearest p.
                    double alpha = numerator / denominator;
                    double directionX = (b.X - a.X) * alpha;
                    double directionY = (b.Y - a.Y) * alpha;
                    o_Intersection = new Vertex<T>(a.X + directionX, a.Y + directionY);
                    return true;
                }
            }
            else
            {
                o_Intersection = Vertex<T>.Zero;
                return false;
            }
        }

        public void Draw()
        {
            GL.Begin(GL.LINES);
            GL.Color(Color.red);
            /*foreach (HalfEdge h in m_HalfEdges)
            {
                GL.Vertex3((float)(h.Origin.X), 0, (float)(h.Origin.Y));
                GL.Vertex3((float)(h.Twin.Origin.X), 0, (float)(h.Twin.Origin.Y));
            }*/
            foreach (Edge<T> e in m_Edges)
            {
                GL.Vertex3((float)(e.Half1.Origin.X), 0, (float)(e.Half1.Origin.Y));
                GL.Vertex3((float)(e.Half2.Origin.X), 0, (float)(e.Half2.Origin.Y));
            }
            GL.End();
        }
    }
}
