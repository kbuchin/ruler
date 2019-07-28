using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using General;
using Util.Geometry.DCEL;
using System;

public class GraphDrawer : MonoBehaviour
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
        m_toptext.GetComponent<Text>().enabled = a_enable;
        m_bottext.GetComponent<Text>().enabled = a_enable;
        m_lefttext.GetComponent<Text>().enabled = a_enable;
        m_righttext.GetComponent<Text>().enabled = a_enable;
    }

    public Color FaceColor = Color.yellow;
    public float Pointradius = 2f;

    public Color m_color = Color.red;

    private Transform m_MyTransform;
    private Material m_LineMaterial;
    private DCEL m_graph;
    private GameObject m_toptext;
    private GameObject m_bottext;
    private GameObject m_lefttext;
    private GameObject m_righttext;

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

        m_toptext = GameObject.Find("TopText");
        m_lefttext = GameObject.Find("LeftText");
        m_righttext = GameObject.Find("RightText");
        m_bottext = GameObject.Find("BotText");

    }

    private void FitToScreen()
    {
        var bbox = Graph.BoundingBox;
        //Everytihng is a unexplained constant
        var xscale = 500 / (Graph.BoundingBox.width * 65);
        var yscale = 375 / (Graph.BoundingBox.height * 65);

        var center = new Vector2(bbox.x + bbox.width / 2, bbox.y + bbox.height / 2);

        center.Scale(new Vector3(-xscale, -yscale,1));
        transform.localPosition = center;
        transform.localScale = new Vector2(xscale, yscale);

        //Set correct boundary text's
        m_toptext.GetComponent<Text>().text = "Y: " + bbox.yMax;
        m_bottext.GetComponent<Text>().text = "Y: " + bbox.yMin;
        m_lefttext.GetComponent<Text>().text = "X: " + bbox.xMin;
        m_righttext.GetComponent<Text>().text = "X: " + bbox.xMax;

    }

    private void DrawEdges()
    {
        GL.Begin(GL.LINES);

        foreach (HalfEdge edge in m_graph.Edges)
        {
            GL.Color(m_color);
            GL.Vertex3(edge.From.Pos.x, edge.From.Pos.y, 0);
            GL.Vertex3(edge.To.Pos.x, edge.To.Pos.y, 0);
        }
        GL.End();
    }

    private void DrawVertices()
    {
        GL.Begin(GL.LINES);
        GL.Color(Color.black);

        foreach (Vertex vertex in m_graph.Vertices)
        {
            float step = (2 * Mathf.PI / 200);
            for (float a = 0; a < (2* Mathf.PI + step); a += step)
            {
                //midpoint of the circle.
                GL.Vertex3( vertex.Pos.x , vertex.Pos.y, 0);
                GL.Vertex3(
                    (Mathf.Cos(a) * Pointradius / transform.localScale.x) + vertex.Pos.x,  
                    (Mathf.Sin(a) * Pointradius / transform.localScale.y) + vertex.Pos.y, 
                    0);
            }
        }
        GL.End();
    }

    /*
    private void DrawMidlleFaces()
    {
        foreach(Face face in m_graph.middleFaces())
        {
            GL.Begin(GL.TRIANGLES);
            GL.Color(FaceColor);
            var vertices = face.OuterVertices();
            for(int i=1; i< vertices.Count -1; i++)
            {
                GL.Vertex(vertices[0].Pos);
                GL.Vertex(vertices[i].Pos);
                GL.Vertex(vertices[i+1].Pos);
            }
            GL.End();
        }
    }
    */

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

            //Backgorund
            GL.Begin(GL.QUADS);
            GL.Color(new Color(0, 0, 0, .3f));
            var bbox = m_graph.BoundingBox;
            GL.Vertex3(bbox.xMin, bbox.yMin, 0);
            GL.Vertex3(bbox.xMin, bbox.yMax, 0);
            GL.Vertex3(bbox.xMax, bbox.yMax, 0);
            GL.Vertex3(bbox.xMax, bbox.yMin, 0);
            GL.End();

            DrawMidlleFaces();
            DrawEdges();
            DrawVertices();

            GL.PopMatrix();
        }
    }

}
