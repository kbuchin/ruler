namespace KingsTaxes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using Util.Geometry.Graph;

    public class Settlement : MonoBehaviour
    {
        public Vertex Vertex { get; private set; }
        public Vector2 Pos { get { return Vertex.Pos; } }


        private KingsTaxesController m_controller;

        void Awake()
        {
            Vertex = new Vertex(transform.position);
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
