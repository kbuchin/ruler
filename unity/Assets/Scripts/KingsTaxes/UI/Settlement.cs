namespace KingsTaxes
{
    using System;
    using UnityEngine;
    using Util.Geometry.Graph;

    /// <summary>
    /// Data storage class. 
    /// Holds position and corresponding vertex of a settlement in the game.
    /// </summary>
    public class Settlement : MonoBehaviour
    {
        /// <summary>
        /// Vertex in graph corresponding to this settlement
        /// </summary>
        public Vertex Vertex { get; private set; }

        /// <summary>
        /// Position of the settlement. 
        /// Not necessary, but used as shorthand.
        /// </summary>
        public Vector2 Pos { get { return Vertex.Pos; } }

        private RoadBuilder m_roadBuilder;

        // Use this for initialization
        void Awake()
        {
            // create vertex at settlement position
            Vertex = new Vertex(transform.position);

            // find road builder class to call upon user interaction
            m_roadBuilder = FindObjectOfType<RoadBuilder>();
            if (m_roadBuilder == null)
            {
                throw new InvalidProgramException("Road builder cannot be found");
            }
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
