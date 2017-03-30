using Algo.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KingsTaxes
{
    public class Settlement : MonoBehaviour
    {
        public Vertex Vertex { get { return m_vertex; } }
        public Vector2 Pos { get { return m_vertex.Pos; } }


        private KingsTaxesController m_controller;
        private Vertex m_vertex;

        void Awake()
        {
            m_vertex = new Vertex(transform.position);
        }

        void Start()
        {
            var inbetween = GameObject.FindGameObjectWithTag(Tags.GameController);
            m_controller = inbetween.GetComponent<KingsTaxesController>();
        }

        void OnMouseDown()
        {
            m_controller.MouseDown(this);
        }

        void OnMouseEnter()
        {
            m_controller.MouseEnter(this);
        }

        void OnMouseExit()
        {
            m_controller.MouseExit(this);
        }

    }
}
