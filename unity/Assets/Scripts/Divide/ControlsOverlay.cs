using UnityEngine;
using System.Collections;

namespace Divide
{
    public class ControlsOverlay : MonoBehaviour
    {
        private SpriteRenderer m_sprite;
        private BoxCollider2D m_boxcolider;

        // Use this for initialization
        void Start()
        {
            m_sprite = GetComponent<SpriteRenderer>();
            m_boxcolider = GetComponent<BoxCollider2D>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Activate()
        {
            m_sprite.enabled = true;
            m_boxcolider.enabled = true;
        }

        void OnMouseUpAsButton()
        {
            m_sprite.enabled = false;
            m_boxcolider.enabled = false;
        }
    }
}