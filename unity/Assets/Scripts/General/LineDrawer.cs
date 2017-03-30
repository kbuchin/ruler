using UnityEngine;
using System.Collections.Generic;
using Algo;
using General;
using Divide;
using System;

public class LineDrawer : MonoBehaviour
{
    private List<ColoredLines> m_lines;
    private Material m_LineMaterial;

    void Awake()
    {
        m_lines = new List<ColoredLines>();

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

    private void DrawLines()
    {
        GL.Begin(GL.LINES);
        foreach (ColoredLines colored_lines in m_lines)
        {
            GL.Color(colored_lines.Color);
            foreach (Line line in colored_lines.Lines)
            {
                GL.Vertex3(0, line.Y(0), 0);
                GL.Vertex3(16, line.Y(16), 0);
            }
        }
        GL.End();
    }

    private void OnRenderObject()
    {
        // Apply the line material
        m_LineMaterial.SetPass(0);
        DrawLines();
    }

    internal void AddLines(List<Line> a_lines, Color a_color)
    {
        m_lines.Add(new ColoredLines(a_color, a_lines));
    }

    internal void ClearLines()
    {
        m_lines.Clear();
    }
}
