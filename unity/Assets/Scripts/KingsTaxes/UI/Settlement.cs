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


        private RoadBuilder m_roadBuilder;

        void Awake()
        {
            Vertex = new Vertex(transform.position);
        }

        void Start()
        {
            m_roadBuilder = GameObject.FindObjectOfType<RoadBuilder>();
            if (m_roadBuilder == null) throw new InvalidOperationException("Road builder cannot be found");
        }

        void OnMouseDown()
        {
            m_roadBuilder.MouseDown(this);
        }

        void OnMouseEnter()
        {
            m_roadBuilder.MouseEnter(this);
        }

        void OnMouseExit()
        {
            m_roadBuilder.MouseExit(this);
        }

    }
}
