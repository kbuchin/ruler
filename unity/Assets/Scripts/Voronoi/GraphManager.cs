namespace Voronoi
{
    using UnityEngine;
    using System.Collections.Generic;
    using System;
    using Util.Geometry.DCEL;
    using MNMatrix = MathNet.Numerics.LinearAlgebra.Matrix<float>;
    using Util.Algorithms.Triangulation;
    using Util.Geometry.Graph;
    using Util.Geometry.Triangulation;
    using Util.Math;

    public sealed class GraphManager : MonoBehaviour
    {
        /*
        
        public GameObject m_Player1Prefab;
        public GameObject m_Player2Prefab;
        public bool m_WithLookAtOnPlacement = true;
        public int m_turns;
        public string m_p1Victory;
        public string m_p2Victory;
        private int m_halfTurnsTaken = 0;
        private Triangulation m_Delaunay;
        private bool m_CircleOn = false;
        private bool m_EdgesOn = false;
        private bool m_VoronoiOn = false;
        private bool m_InvalidEdgesOn = false;
        private MeshFilter m_MeshFilter;
        private bool player1Turn = true;
        private Transform m_MyTransform;
        public GUIManager m_GUIManager;
        private FishManager m_FishManager;
        private Rect m_MeshRect;
        private List<Vector2> m_ClippingEdges = new List<Vector2>();
        private DCEL m_DCEL;
        //private DCEL.Intersection[] m_DCELIntersections;
        private float[] m_playerArea;

        [Flags]
        private enum ERectangleSide
        {
            NONE = 0,
            LEFT = 1,
            TOP = 2,
            RIGHT = 4,
            BOTTOM = 8
        };

        private class MeshDescription
        {
            public Vector3[] vertices;
            public int[][] triangles;
            public float[] playerArea;
        }

        private struct InvalidEdge
        {
            public Vertex m_InvalidVertex;
            public Vector2 m_IntersectingPoint;

            public InvalidEdge(Vertex a_InvalidVertex, Vector2 a_IntersectingPoint)
            {
                m_InvalidVertex = a_InvalidVertex;
                m_IntersectingPoint = a_IntersectingPoint;
            }
        }

        void Awake()
        {
            m_MyTransform = this.gameObject.transform;
            m_FishManager = new FishManager();
            GameObject rendererObject = GameObject.Find("VoronoiMesh");
            m_MeshFilter = rendererObject.GetComponent<MeshFilter>();
            float z = (m_MeshFilter.transform.position - Camera.main.transform.position).magnitude;
            Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, z));
            Vector3 topRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, z));
            m_MeshRect = new Rect(bottomLeft.x, bottomLeft.z, topRight.x - bottomLeft.x, topRight.z - bottomLeft.z);
        }

        private void Start()
        {
            m_Delaunay = Delaunay.Create();
        }

        private void DrawEdges()
        {
            GL.Color(Color.green);
            GL.Begin(GL.LINES);

            foreach (var halfEdge in m_Delaunay.Edges)
            {
                GL.Vertex3(halfEdge.Start.X, 0, halfEdge.Start.Y);
                GL.Vertex3(halfEdge.End.X, 0, halfEdge.End.Y);
            }
            GL.End();
        }

        private DCEL GetScreenDCEL()
        {
            DCEL screen = new DCEL();
            Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
            Vector3 topRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
            Vector3 bottomRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0));
            Vector3 topLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0));
            screen.AddEdge(new Vector2(bottomLeft.x, bottomLeft.z), new Vector2(topLeft.x, topLeft.z));
            screen.AddEdge(new Vector2(topLeft.x, topLeft.z), new Vector2(topRight.x, topRight.z));
            screen.AddEdge(new Vector2(topRight.x, topRight.z), new Vector2(bottomRight.x, bottomRight.z));
            screen.AddEdge(new Vector2(bottomRight.x, bottomRight.z), new Vector2(bottomLeft.x, bottomLeft.z));
            return screen;
        }

        private void UpdateVoronoiMesh()
        {
            m_DCEL = CreateVoronoiDiagram();
            //m_DCELIntersections = null;
            DCEL screen = GetScreenDCEL();
            DCEL overlay = new DCEL(m_DCEL, screen);
            overlay.FindIntersections2(out m_DCELIntersections);
            m_DCEL = DCEL.MapOverlay(overlay);
            MeshDescription newDescription = TriangulateVoronoi();
            m_playerArea = newDescription.playerArea;
            m_GUIManager.SetPlayerAreaOwned(newDescription.playerArea[0], newDescription.playerArea[1]);
            Mesh mesh = m_MeshFilter.mesh;
            if (mesh == null)
            {
                mesh = new Mesh();
                m_MeshFilter.mesh = mesh;
            }
            mesh.subMeshCount = 2;
            mesh.MarkDynamic();
            mesh.vertices = newDescription.vertices;
            mesh.SetTriangles(newDescription.triangles[0], 0);
            mesh.SetTriangles(newDescription.triangles[1], 1);
            mesh.RecalculateBounds();

            Vector2[] newUVs = new Vector2[newDescription.vertices.Length];
            for (int i = 0; i < newDescription.vertices.Length; ++i)
            {
                Vector3 vertex = newDescription.vertices[i];
                newUVs[i] = new Vector2(vertex.x, vertex.z);
            }

            mesh.uv = newUVs;
            ;
        }

        private void DrawCircles()
        {
            float radius = 0;
            GL.Begin(GL.LINES);

            foreach (Triangle triangle in m_Delaunay.Triangles)
            {
                //GL.Color(triangle.Color);
                radius = Convert.ToSingle(Math.Sqrt(triangle.Circumcenter));
                float heading = 0;
                const float extra = (360 / 100);
                for (int a = 0; a < (360 + extra); a += 360 / 100)
                {
                    //the circle.
                    GL.Vertex3((Mathf.Cos(heading) * radius) + triangle.Circumcenter.x, 0, (Mathf.Sin(heading) * radius) + triangle.Circumcenter.y);
                    heading = a * Mathf.PI / 180;
                    GL.Vertex3((Mathf.Cos(heading) * radius) + triangle.Circumcenter.x, 0, (Mathf.Sin(heading) * radius) + triangle.Circumcenter.y);

                    //midpoint of the circle.
                    GL.Vertex3((Mathf.Cos(heading) * 0.1f) + triangle.Circumcenter.x, 0, (Mathf.Sin(heading) * 0.1f) + triangle.Circumcenter.y);
                    GL.Vertex3((Mathf.Cos(heading) * 0.2f) + triangle.Circumcenter.x, 0, (Mathf.Sin(heading) * 0.2f) + triangle.Circumcenter.y);
                }
            }
            GL.End();
        }

        private void DrawVoronoi()
        {
            GL.Begin(GL.LINES);
            foreach (var halfEdge in m_Delaunay.Edges)
            {
                if (halfEdge.Twin == null)
                {
                    continue;
                }

                Triangle t1 = halfEdge.T;
                Triangle t2 = halfEdge.Twin.T;

                if (t1 != null && t2 != null)
                {
                    var v1 = t1.Circumcenter;
                    var v2 = t2.Circumcenter;

                    GL.Vertex3(v1.x, 0, v1.y);
                    GL.Vertex3(v2.x, 0, v2.y);
                }
            }
            GL.End();
        }

        private static bool IntersectLines(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 o_Intersection)
        {
            var numerator = MathUtil.Orient2D(c, d, a);
            if ((numerator * MathUtil.Orient2D(c, d, b) <= 0) && 
                (MathUtil.Orient2D(a, b, c) * MathUtil.Orient2D(a, b, d) <= 0))
            {
                float[,] denominatorArray = new float[,]
                {
                    { b.x - a.x, b.y - a.y },
                    { d.x - c.x, d.y - c.y }
                };
			
                var denominatorMatrix = MNMatrix.Build.DenseOfArray(denominatorArray);
                var denominator = denominatorMatrix.Determinant();

                if (Mathf.Abs(denominator) <= float.Epsilon)
                { // ab and cd are parallel or equal
                    o_Intersection = Vector2.zero;
                    return false;
                }
                else
                { // can optionally check if p is very close to b, c, or d and then flip so that a is nearest p.
                    var alpha = numerator / denominator;
                    var direction = b - a;
                    direction.Scale(new Vector2((float)alpha, (float)alpha));
                    o_Intersection = a + direction;
                    return true;
                }
            }
            else
            {
                o_Intersection = Vector2.zero;
                return false;
            }
        }

        private static bool IntersectLineWithRectangle(Vector2 a_From, Vector2 a_To, Rect a_Rectangle, int a_MaxIntersections, out Vector2[] o_Intersections,
                                                       out ERectangleSide o_Sides)
        {
            bool intersected = false;
            o_Sides = ERectangleSide.NONE;
            Vector2 intersection;
            List<Vector2> intersectionsList = new List<Vector2>(a_MaxIntersections);

            if (IntersectLines(a_From, a_To, new Vector2(a_Rectangle.xMin, a_Rectangle.yMin),
                    new Vector2(a_Rectangle.xMin, a_Rectangle.yMax), out intersection))
            {
                intersectionsList.Add(intersection);
                o_Sides = ERectangleSide.LEFT;
                intersected = true;
                if (intersectionsList.Count == a_MaxIntersections)
                {
                    o_Intersections = intersectionsList.ToArray();
                    return true;
                }
            }
            if (IntersectLines(a_From, a_To, new Vector2(a_Rectangle.xMin, a_Rectangle.yMax),
                    new Vector2(a_Rectangle.xMax, a_Rectangle.yMax), out intersection))
            {
                intersectionsList.Add(intersection);
                o_Sides = o_Sides & ERectangleSide.TOP;
                intersected = true;
                if (intersectionsList.Count == a_MaxIntersections)
                {
                    o_Intersections = intersectionsList.ToArray();
                    return true;
                }
            }
            if (IntersectLines(a_From, a_To, new Vector2(a_Rectangle.xMax, a_Rectangle.yMax),
                    new Vector2(a_Rectangle.xMax, a_Rectangle.yMin), out intersection))
            {
                intersectionsList.Add(intersection);
                o_Sides = o_Sides & ERectangleSide.RIGHT;
                intersected = true;
                if (intersectionsList.Count == a_MaxIntersections)
                {
                    o_Intersections = intersectionsList.ToArray();
                    return true;
                }
            }
            if (IntersectLines(a_From, a_To, new Vector2(a_Rectangle.xMax, a_Rectangle.yMin),
                    new Vector2(a_Rectangle.xMin, a_Rectangle.yMin), out intersection))
            {
                intersectionsList.Add(intersection);
                o_Sides = o_Sides & ERectangleSide.BOTTOM;
                intersected = true;
            }
            o_Intersections = intersectionsList.ToArray();
            return intersected;
        }

        private void ReplaceVoronoiVertex(Vertex a_InvalidVertex, Vertex a_ReplacingVertex, Dictionary<Vertex, HashSet<Vertex>> a_VoronoiEdges,
                                          Dictionary<Vertex, HashSet<Vertex>> a_VoronoiToInternal,
                                          Dictionary<Vertex, HashSet<Vertex>> a_InternalEdges)
        {
            HashSet<Vertex> adjacentVoronoiVertices;
            if (a_VoronoiEdges.TryGetValue(a_InvalidVertex, out adjacentVoronoiVertices))
            {
                if (a_ReplacingVertex != null)
                {
                    a_VoronoiEdges[a_ReplacingVertex] = adjacentVoronoiVertices;
                }
                a_VoronoiEdges.Remove(a_InvalidVertex);
                foreach (Vertex adjacentVertex in adjacentVoronoiVertices)
                {
                    HashSet<Vertex> adjacentOfAdjacent;
                    if (a_VoronoiEdges.TryGetValue(adjacentVertex, out adjacentOfAdjacent))
                    {
                        adjacentOfAdjacent.Remove(a_InvalidVertex);
                        if (a_ReplacingVertex != null)
                        {
                            adjacentOfAdjacent.Add(a_ReplacingVertex);
                        }
                    }
                }
            }
            HashSet<Vertex> inputVertices;
            if (a_VoronoiToInternal.TryGetValue(a_InvalidVertex, out inputVertices))
            {
                if (a_ReplacingVertex != null)
                {
                    a_VoronoiToInternal[a_ReplacingVertex] = inputVertices;
                }
                a_VoronoiToInternal.Remove(a_InvalidVertex);
                foreach (Vertex inputVertex in inputVertices)
                {
                    HashSet<Vertex> voronoiVertices;
                    if (a_InternalEdges.TryGetValue(inputVertex, out voronoiVertices))
                    {
                        voronoiVertices.Remove(a_InvalidVertex);
                        if (a_ReplacingVertex != null)
                        {
                            voronoiVertices.Add(a_ReplacingVertex);
                        }
                    }
                }
            }
        }

        private void FixInvalidVoronoiEdges(List<InvalidEdge> a_InvalidEdges, List<Vertex> a_VerticesToRemove,
                                            Dictionary<Vertex, HashSet<Vertex>> a_VoronoiEdges,
                                            Dictionary<Vertex, HashSet<Vertex>> a_InternalEdges,
                                            Dictionary<Vertex, HashSet<Vertex>> a_VoronoiToInternal)
        {
            foreach (InvalidEdge invalidEdge in a_InvalidEdges)
            {
                var replacingVertex = new Vertex(invalidEdge.m_IntersectingPoint);
                ReplaceVoronoiVertex(invalidEdge.m_InvalidVertex, replacingVertex, a_VoronoiEdges, a_VoronoiToInternal, a_InternalEdges);
            }
            foreach (Vertex invalidVertex in a_VerticesToRemove)
            {
                ReplaceVoronoiVertex(invalidVertex, null, a_VoronoiEdges, a_VoronoiToInternal, a_InternalEdges);
            }
        }

        private List<InvalidEdge> FindClippingVoronoiEdges(Dictionary<Vertex, HashSet<Vertex>> a_VoronoiEdges, List<Vertex> a_VerticesToRemove, List<Vector2> a_ClippingEdges)
        {
            List<InvalidEdge> invalidEdges = new List<InvalidEdge>();
            a_ClippingEdges.Clear();
            foreach (var voronoiVertex in a_VoronoiEdges.Keys)
            {
                Vector2 voronoiPos = new Vector2(voronoiVertex.Pos.x, voronoiVertex.Pos.y);
                if (m_MeshRect.Contains(voronoiPos) == false)
                {
                    HashSet<Vertex> adjacentVoronoiVertices = a_VoronoiEdges[voronoiVertex];
                    foreach (var adjacentVertex in adjacentVoronoiVertices)
                    {
                        Vector2 adjacentVoronoiPos = new Vector2(adjacentVertex.Pos.x, adjacentVertex.Pos.y);
                        // For an edge to be invalid, it either has 1 vertex outside the rectangle (clipping it)
                        // or both vertices are completely outside of the rectangle and not clipping it
                        // or both vertices are completely outside of the rectangle and the edge intersects the rectangle twice.
                        if (m_MeshRect.Contains(voronoiPos) == false)
                        {
                            // If the first vertex is outside and the second vertex is inside, we intersect the rectangle in 1 location.
                            if (m_MeshRect.Contains(adjacentVoronoiPos))
                            {
                                Vector2[] intersections;
                                ERectangleSide intersectedSides;
                                if (IntersectLineWithRectangle(voronoiPos, adjacentVoronoiPos, m_MeshRect, 1, out intersections,
                                        out intersectedSides))
                                {
                                    a_ClippingEdges.Add(voronoiPos);
                                    a_ClippingEdges.Add(intersections[0]);
                                    invalidEdges.Add(new InvalidEdge(voronoiVertex, intersections[0]));
                                }
                            }
                            else
                            {
                                // If the first vertex it outside and the second vertex too, it is possible we still intersect the
                                // rectangle in 2 places.
                                Vector2[] intersections;
                                ERectangleSide intersectedSides;
                                if (IntersectLineWithRectangle(voronoiPos, adjacentVoronoiPos, m_MeshRect, 2, out intersections,
                                        out intersectedSides))
                                {
                                    // The edge intersects the rectangle in 2 places, find the intersection on the rectangle that is
                                    // on "this side" of the rectangle, seen from the first vertex of the edge that we are processing.
                                    // The line segment part of the edge on the other side of the rectangle will be found later in the iteration.
                                    int index = 0;
                                    if ((intersectedSides & ERectangleSide.LEFT) != ERectangleSide.NONE)
                                    {
                                        if (voronoiPos.x < m_MeshRect.xMin)
                                        {
                                            a_ClippingEdges.Add(voronoiPos);
                                            a_ClippingEdges.Add(intersections[0]);
                                            continue;
                                        }
                                        index++;
                                    }
                                    if ((intersectedSides & ERectangleSide.TOP) != ERectangleSide.NONE)
                                    {
                                        if (voronoiPos.y > m_MeshRect.yMax)
                                        {
                                            a_ClippingEdges.Add(voronoiPos);
                                            a_ClippingEdges.Add(intersections[index]);
                                            continue;
                                        }
                                        index++;
                                    }
                                    if ((intersectedSides & ERectangleSide.RIGHT) != ERectangleSide.NONE)
                                    {
                                        if (voronoiPos.x > m_MeshRect.xMax)
                                        {
                                            a_ClippingEdges.Add(voronoiPos);
                                            a_ClippingEdges.Add(intersections[index]);
                                            continue;
                                        }
                                        index++;
                                    }
                                    if ((intersectedSides & ERectangleSide.BOTTOM) != ERectangleSide.NONE)
                                    {
                                        if (voronoiPos.y < m_MeshRect.yMin)
                                        {
                                            a_ClippingEdges.Add(voronoiPos);
                                            a_ClippingEdges.Add(intersections[index]);
                                        }
                                    }
                                }
                                else
                                {
                                    // Both vertices are outside of the rectangle and the edge does not intersect with the rectangle.
                                    a_ClippingEdges.Add(voronoiPos);
                                    a_ClippingEdges.Add(adjacentVoronoiPos);
                                    //a_VerticesToRemove.Add(voronoiVertex);
                                    //a_VerticesToRemove.Add(adjacentVertex);
                                }
                            }
                        }
                    }
                }
            }
            return invalidEdges;
        }


        private MeshDescription TriangulateVoronoi()
        {
            float[] playerArea = new float[2] { 0, 0 };
            Dictionary<Vertex, HashSet<Vertex>> internalEdges = new Dictionary<Vertex, HashSet<Vertex>>();
            Dictionary<Vertex, HashSet<Vertex>> voronoiEdges = new Dictionary<Vertex, HashSet<Vertex>>();
            Dictionary<Vertex, HashSet<Vertex>> voronoiToInternalEdges = new Dictionary<Vertex, HashSet<Vertex>>();
            foreach (HalfEdge halfEdge in m_Delaunay.HalfEdges)
            {
                ProcessHalfEdge(halfEdge, voronoiEdges, internalEdges, voronoiToInternalEdges);
            }
            List<Vertex> verticesToRemove = new List<Vertex>();
            //List<InvalidEdge> invalidEdges = FindClippingVoronoiEdges...
            FindClippingVoronoiEdges(voronoiEdges, verticesToRemove, m_ClippingEdges);
            //FixInvalidVoronoiEdges(invalidEdges, verticesToRemove, voronoiEdges, internalEdges, voronoiToInternalEdges);
            List<Vector3> vertices = new List<Vector3>();
            List<int>[] triangleLists = new List<int>[2];
            triangleLists[0] = new List<int>();
            triangleLists[1] = new List<int>();

            foreach (Vertex inputNode in internalEdges.Keys)
            {
                bool unowned = inputNode.Ownership == Vertex.EOwnership.UNOWNED;
                int playerIndex = inputNode.Ownership == Vertex.EOwnership.PLAYER1 ? 0 : -1;
                playerIndex = inputNode.Ownership == Vertex.EOwnership.PLAYER2 ? 1 : playerIndex;
                HashSet<Vertex> voronoiNodes = internalEdges[inputNode];
                foreach (Vertex voronoiNode in voronoiNodes)
                {
                    HashSet<Vertex> adjacentVoronoiNodes = voronoiEdges[voronoiNode];
                    HashSet<Vertex> intersection = new HashSet<Vertex>(adjacentVoronoiNodes, adjacentVoronoiNodes.Comparer);
                    intersection.IntersectWith(voronoiNodes);
                    foreach (Vertex adjacent in intersection)
                    {
                        int curCount = VertexCount;
                        vertices.Add(new Vector3(inputNode.X, 0, inputNode.Y));
                        vertices.Add(new Vector3(voronoiNode.X, 0, voronoiNode.Y));
                        vertices.Add(new Vector3(adjacent.X, 0, adjacent.Y));
                        if (unowned)
                        {
                            triangleLists[0].Add(curCount);
                            triangleLists[0].Add(curCount + 1);
                            triangleLists[0].Add(curCount + 2);
                            triangleLists[1].Add(curCount);
                            triangleLists[1].Add(curCount + 1);
                            triangleLists[1].Add(curCount + 2);
                        }
                        else
                        {
                            triangleLists[playerIndex].Add(curCount);
                            triangleLists[playerIndex].Add(curCount + 1);
                            triangleLists[playerIndex].Add(curCount + 2);

                            Vector2[] tr = new Vector2[3];
                            tr[0] = new Vector2(inputNode.X, inputNode.Y);
                            if (Orient2D(inputNode, voronoiNode, adjacent) < 0)
                            {
                                tr[1] = new Vector2(voronoiNode.X, voronoiNode.Y);
                                tr[2] = new Vector2(adjacent.X, adjacent.Y);
                            } else
                            {
                                tr[2] = new Vector2(voronoiNode.X, voronoiNode.Y);
                                tr[1] = new Vector2(adjacent.X, adjacent.Y);
                            }

                            Vector2[] rec = new Vector2[4];
                            Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
                            Vector3 topRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
                            rec[3] = new Vector2(topRight.x, bottomLeft.z);
                            rec[2] = new Vector2(topRight.x, topRight.z);
                            rec[1] = new Vector2(bottomLeft.x, topRight.z);
                            rec[0] = new Vector2(bottomLeft.x, bottomLeft.z);

                            float area = VertexSimplePolygon.IntersectConvex(new VertexSimplePolygon(tr), new VertexSimplePolygon(rec)).Area();
                            print(area);

                            playerArea[playerIndex] += area; // Mathf.Abs((Orient2D(inputNode, voronoiNode, adjacent) / 2));
                        }
                    }
                }
            }
            MeshDescription description = new MeshDescription();
            description.triangles = new int[2][];
            description.triangles[0] = triangleLists[0].ToArray();
            description.triangles[1] = triangleLists[1].ToArray();
            description.vertices = vertices.ToArray();
            description.playerArea = playerArea;
            return description;
            */
            /**GL.Begin(GL.LINES);
		    foreach (Vertex key in internalEdges.Keys)
		    {
			    HashSet<Vertex> vertices = internalEdges[key];
			    foreach (Vertex item in vertices)
			    {
				    GL.Vertex3(key.X, 0, key.Y);
				    GL.Vertex3(item.X, 0, item.Y);
			    }
		    }
		    foreach (Vertex key in voronoiEdges.Keys)
		    {
			    HashSet<Vertex> vertices = voronoiEdges[key];
			    foreach (Vertex item in vertices)
			    {
				    GL.Vertex3(key.X, 0, key.Y);
				    GL.Vertex3(item.X, 0, item.Y);
			    }
		    }
		    GL.End();**/ /*
        }

        private static void ProcessHalfEdge(HalfEdge a_H1, Dictionary<Vertex, HashSet<Vertex>> a_VoronoiEdges,
                                            Dictionary<Vertex, HashSet<Vertex>> a_InternalEdges,
                                            Dictionary<Vertex, HashSet<Vertex>> a_VoronoiToInternalEdges)
        {
            if (a_H1.Twin == null)
            {
                return;
            }

            var t1 = a_H1.T;
            Triangle t2 = a_H1.Twin.Triangle;

            if (t1 != null && t2 != null)
            {
                Vertex voronoiVertex = t1.Circumcenter;
                Vertex voronoiVertex2 = t2.Circumcenter;
                HashSet<Vertex> existingVoronoiEdges;

                if (a_VoronoiEdges.TryGetValue(voronoiVertex, out existingVoronoiEdges))
                {
                    existingVoronoiEdges.Add(voronoiVertex2);
                }
                else
                {
                    a_VoronoiEdges.Add(voronoiVertex, new HashSet<Vertex>{ voronoiVertex2 });
                }

                if (a_VoronoiEdges.TryGetValue(voronoiVertex2, out existingVoronoiEdges))
                {
                    existingVoronoiEdges.Add(voronoiVertex);
                }
                else
                {
                    a_VoronoiEdges.Add(voronoiVertex2, new HashSet<Vertex>{ voronoiVertex });
                }

                foreach (Vertex inputVertex in t1.Vertices)
                {
                    HashSet<Vertex> existingValue;
                    if (a_InternalEdges.TryGetValue(inputVertex, out existingValue))
                    {
                        existingValue.Add(voronoiVertex);
                    }
                    else
                    {
                        a_InternalEdges.Add(inputVertex, new HashSet<Vertex>{ voronoiVertex });
                    }
                }
                HashSet<Vertex> inputVertices = new HashSet<Vertex>(t1.Vertices);
                HashSet<Vertex> existingInputVertices;
                if (a_VoronoiToInternalEdges.TryGetValue(voronoiVertex, out existingInputVertices))
                {
                    existingInputVertices.UnionWith(inputVertices);
                }
                else
                {
                    a_VoronoiToInternalEdges.Add(voronoiVertex, inputVertices);
                }
                // Yes, yes, code duplication is bad.
                foreach (Vertex inputVertex in t2.Vertices)
                {
                    HashSet<Vertex> existingValue;
                    if (a_InternalEdges.TryGetValue(inputVertex, out existingValue))
                    {
                        existingValue.Add(voronoiVertex2);
                    }
                    else
                    {
                        a_InternalEdges.Add(inputVertex, new HashSet<Vertex>{ voronoiVertex2 });
                    }
                }
                inputVertices = new HashSet<Vertex>(t2.Vertices);
                existingInputVertices = null;
                if (a_VoronoiToInternalEdges.TryGetValue(voronoiVertex2, out existingInputVertices))
                {
                    existingInputVertices.UnionWith(inputVertices);
                }
                else
                {
                    a_VoronoiToInternalEdges.Add(voronoiVertex2, inputVertices);
                }
            }
        }

        private void OnRenderObject()
        {
            // Apply the line material
            m_Delaunay.LineMaterial.SetPass(0);

            GL.PushMatrix();
            // Set transformation matrix for drawing to
            // match our transform
            GL.MultMatrix(transform.localToWorldMatrix);

            if (m_EdgesOn)
            {
                DrawEdges();
            }

            if (m_CircleOn)
            {
                DrawCircles();
            }

            if (m_VoronoiOn)
            {
                //DrawVoronoi();
                if (m_DCEL != null)
                {
                    m_DCEL.Draw();
                    if (m_DCELIntersections != null)
                    {
                        foreach (DCEL.Intersection intersection in m_DCELIntersections)
                        {
                            GL.Begin(GL.QUADS);
                            GL.Color(Color.cyan);
                            const float size = 0.1f;
                            GL.Vertex(new Vector3((float)intersection.point.X - size, 0, (float)intersection.point.Y - size));
                            GL.Vertex(new Vector3((float)intersection.point.X - size, 0, (float)intersection.point.Y + size));
                            GL.Vertex(new Vector3((float)intersection.point.X + size, 0, (float)intersection.point.Y + size));
                            GL.Vertex(new Vector3((float)intersection.point.X + size, 0, (float)intersection.point.Y - size));
                            GL.End();
                        }
                    }
                }
            }

            //DrawMeshRect();

            if (m_InvalidEdgesOn)
            {
                DrawInvalidVoronoiEdges();
            }

            GL.PopMatrix();
        }

        private void DrawInvalidVoronoiEdges()
        {
            GL.Begin(GL.LINES);
            GL.Color(Color.red);
            for (int i = 0; i < m_ClippingEdges.Count; i += 2)
            {
                GL.Vertex3(m_ClippingEdges[i].x, 0, m_ClippingEdges[i].y);
                GL.Vertex3(m_ClippingEdges[i + 1].x, 0, m_ClippingEdges[i + 1].y);
            }
            GL.End();
        }

        private void DrawMeshRect()
        {
            GL.Begin(GL.LINES);

            GL.Vertex3(m_MeshRect.xMin, 0, m_MeshRect.yMin);
            GL.Vertex3(m_MeshRect.xMin, 0, m_MeshRect.yMax);

            GL.Vertex3(m_MeshRect.xMin, 0, m_MeshRect.yMax);
            GL.Vertex3(m_MeshRect.xMax, 0, m_MeshRect.yMax);

            GL.Vertex3(m_MeshRect.xMax, 0, m_MeshRect.yMax);
            GL.Vertex3(m_MeshRect.xMax, 0, m_MeshRect.yMin);

            GL.Vertex3(m_MeshRect.xMax, 0, m_MeshRect.yMin);
            GL.Vertex3(m_MeshRect.xMin, 0, m_MeshRect.yMin);

            GL.End();
        }

        private void Update()
        {
            if (Input.GetKeyDown("c"))
            {
                m_CircleOn = !m_CircleOn;
            }

            if (Input.GetKeyDown("e"))
            {
                m_EdgesOn = !m_EdgesOn;
            }

            if (Input.GetKeyDown("v"))
            {
                m_VoronoiOn = !m_VoronoiOn;
            }

            if (Input.GetKeyDown("i"))
            {
                m_InvalidEdgesOn = !m_InvalidEdgesOn;
            }

            if (Input.GetMouseButtonDown(0))
            {

                if (m_halfTurnsTaken >= 2 * m_turns)
                {
                    if (m_playerArea[0] > m_playerArea[1])
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene(m_p1Victory);
                    }
                    else
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene(m_p2Victory);
                    }
                }
                else
                {

                    Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    pos.y = 0;
                    Vertex me = new Vertex(pos.x, pos.z, player1Turn ? Vertex.EOwnership.PLAYER1 : Vertex.EOwnership.PLAYER2);
                    if (m_Delaunay.AddVertex(me))
                    {
                        GameObject onClickObject = null;
                        if (player1Turn)
                        {
                            onClickObject = GameObject.Instantiate(m_Player1Prefab, pos, Quaternion.identity) as GameObject;
                        }
                        else
                        {
                            onClickObject = GameObject.Instantiate(m_Player2Prefab, pos, Quaternion.identity) as GameObject;
                        }

                        if (onClickObject == null)
                        {
                            Debug.LogError("Couldn't instantiate m_PlayerPrefab!");
                        }
                        else
                        {
                            onClickObject.name = "onClickObject_" + me.Ownership.ToString();
                            onClickObject.transform.parent = m_MyTransform;
                            m_FishManager.AddFish(onClickObject.transform, player1Turn ? 1 : 2, m_WithLookAtOnPlacement);
                        }

                        UpdateVoronoiMesh();

                        player1Turn = !player1Turn;
                        if (player1Turn)
                        {
                            m_GUIManager.OnBlueTurnStart();
                        }
                        else
                        {
                            m_GUIManager.OnRedTurnStart();
                        }
                    }
                    //Update turn counter
                    m_halfTurnsTaken += 1;

                    if (m_halfTurnsTaken >= 2 * m_turns)
                    {
                        m_GUIManager.OnLastMove();
                    }
                }
            }
        }
        */
    }
}