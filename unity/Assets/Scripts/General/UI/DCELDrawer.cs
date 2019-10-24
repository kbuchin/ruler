namespace Drawing
{
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections.Generic;
    using General;
    using Util.Geometry.DCEL;
    using System;
    using Util.Algorithms.Triangulation;
    using Util.Algorithms.DCEL;

    public class DCELDrawer : MonoBehaviour
    {
        public DCEL Graph
        {
            get
            {
                return m_graph;
            }
            set
            {
                m_graph = value;
                if (value == null)
                {
                    EnableText(false);
                }
                else
                {
                    EnableText(true);
                    FitToScreen();
                }
            }
        }

        private void EnableText(bool a_enable)
        {
            m_topText.GetComponent<Text>().enabled = a_enable;
            m_bottomText.GetComponent<Text>().enabled = a_enable;
            m_leftText.GetComponent<Text>().enabled = a_enable;
            m_rightText.GetComponent<Text>().enabled = a_enable;
        }

        private Color FaceColor = Color.yellow;
        private Color EdgeColor = Color.grey;
        private Color VertexColor = Color.black;
        private float VertexRadius = 0.15f;

        private Transform m_MyTransform;
        private Material m_LineMaterial;
        private DCEL m_graph;

        [Header("Text Fields")]
        [SerializeField]
        private GameObject m_topText;
        [SerializeField]
        private GameObject m_bottomText;
        [SerializeField]
        private GameObject m_leftText;
        [SerializeField]
        private GameObject m_rightText;

        void Awake()
        {
            m_MyTransform = this.gameObject.transform;

            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            m_LineMaterial = new Material(shader);
            m_LineMaterial.hideFlags = HideFlags.HideAndDontSave;

            // Turn on alpha blending
            m_LineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m_LineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            // Turn backface culling off
            m_LineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

            // Turn off depth writes
            m_LineMaterial.SetInt("_ZWrite", 0);
        }

        private void FitToScreen()
        {
            var bbox = Graph.BoundingBox;

            // Some unexplained constants
            var xscale = 500 / (Graph.BoundingBox.width * 65);
            var yscale = 375 / (Graph.BoundingBox.height * 65);

            var center = new Vector2(bbox.x + bbox.width / 2, bbox.y + bbox.height / 2);

            center.Scale(new Vector3(-xscale, -yscale, 1));
            transform.localPosition = center;
            transform.localScale = new Vector2(xscale, yscale);

            //Set correct boundary text's
            m_topText.GetComponent<Text>().text = "Y: " + bbox.yMax;
            m_bottomText.GetComponent<Text>().text = "Y: " + bbox.yMin;
            m_leftText.GetComponent<Text>().text = "X: " + bbox.xMin;
            m_rightText.GetComponent<Text>().text = "X: " + bbox.xMax;

        }

        private void DrawEdges()
        {
            GL.Begin(GL.LINES);

            foreach (HalfEdge edge in m_graph.Edges)
            {
                GL.Color(EdgeColor);
                GL.Vertex3(edge.From.Pos.x, edge.From.Pos.y, 0);
                GL.Vertex3(edge.To.Pos.x, edge.To.Pos.y, 0);
            }
            GL.End();
        }

        private void DrawVertices()
        {
            foreach (var vertex in m_graph.Vertices)
            {
                GL.Begin(GL.TRIANGLE_STRIP);
                GL.Color(VertexColor);

                float step = (2 * Mathf.PI / 200);
                for (float a = 0; a < (2 * Mathf.PI + step); a += step)
                {
                    //midpoint of the circle.
                    GL.Vertex3(
                        Mathf.Cos(a) * VertexRadius + vertex.Pos.x,
                        Mathf.Sin(a) * VertexRadius + vertex.Pos.y,
                        0f);

                    GL.Vertex(vertex.Pos);
                }
                GL.End();
            }
        }
        
        private void DrawMiddleFaces()
        {
            GL.Begin(GL.TRIANGLES);
            GL.Color(FaceColor);
            foreach (var triangle in Triangulator.Triangulate(HamSandwich.MiddleFaces(m_graph)).Triangles)
            {
                GL.Vertex(triangle.P0);
                GL.Vertex(triangle.P1);
                GL.Vertex(triangle.P2);
            }
            GL.End();
        }

        private void OnRenderObject()
        {
            if (m_graph != null)
            {
                // Apply the line material
                m_LineMaterial.SetPass(0);

                GL.PushMatrix();
                // Set transformation matrix for drawing to
                // match our transform
                GL.MultMatrix(m_MyTransform.localToWorldMatrix);

                DrawMiddleFaces();
                DrawEdges();
                DrawVertices();

                //Background
                if (m_graph.InitBoundingBox != null)
                {
                    var bbox = (Rect)m_graph.InitBoundingBox;

                    GL.Begin(GL.QUADS);
                    GL.Color(new Color(0, 0, 0, .3f));
                    GL.Vertex3(bbox.xMin, bbox.yMin, 0);
                    GL.Vertex3(bbox.xMin, bbox.yMax, 0);
                    GL.Vertex3(bbox.xMax, bbox.yMax, 0);
                    GL.Vertex3(bbox.xMax, bbox.yMin, 0);
                    GL.End();
                }
                
                GL.PopMatrix();
            }
        }
    }
}