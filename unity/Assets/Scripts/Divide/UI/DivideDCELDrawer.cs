namespace Divide
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using Util.Algorithms.Triangulation;
    using Util.Geometry.DCEL;

    /// <summary>
    /// Draws a DCEL made from a collection of intersecting lines.
    /// Draws the faces in the middle of the line yellow.
    /// Used in the Divide game for visualization.
    /// </summary>
    public class DivideDCELDrawer : MonoBehaviour
    {
        /// <summary>
        /// The DCEl to be drawn.
        /// Whenever updated, redraw the graph.
        /// </summary>
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

        /// <summary>
        /// Faces in between dual lines. 
        /// For drawing of DCEL construction with dual lines.
        /// </summary>
        public List<Face> MiddleFaces { get; set; }

        /// <summary>
        /// Enable text boxes to display additioanl information
        /// </summary>
        /// <param name="a_enable"></param>
        private void EnableText(bool a_enable)
        {
            m_topText.GetComponent<Text>().enabled = a_enable;
            m_bottomText.GetComponent<Text>().enabled = a_enable;
            m_leftText.GetComponent<Text>().enabled = a_enable;
            m_rightText.GetComponent<Text>().enabled = a_enable;
        }

        private readonly Color FaceColor = Color.yellow;
        private readonly Color EdgeColor = new Color(.25f, .25f, .25f);     // dark grey
        private readonly Color VertexColor = Color.black;
        private readonly float VertexRadius = 0.2f;

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

        // Use this for initialization
        void Awake()
        {
            m_MyTransform = this.gameObject.transform;

            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            m_LineMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            // Turn on alpha blending
            m_LineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m_LineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            // Turn backface culling off
            m_LineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

            // Turn off depth writes
            m_LineMaterial.SetInt("_ZWrite", 0);
        }

        /// <summary>
        /// Fit the graph to the given screen
        /// </summary>
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

        /// <summary>
        /// Draw the edges of the DCEL.
        /// </summary>
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

        /// <summary>
        /// Draws the vertices of the DCEL.
        /// </summary>
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

        /// <summary>
        /// Draws the faces in between the lines.
        /// </summary>
        private void DrawMiddleFaces()
        {
            var triangles = Triangulator.Triangulate(MiddleFaces).Triangles;

            GL.Begin(GL.TRIANGLES);
            GL.Color(FaceColor);

            foreach (var triangle in triangles)
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

                // draw graph components
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