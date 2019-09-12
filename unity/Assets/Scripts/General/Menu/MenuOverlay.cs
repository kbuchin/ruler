namespace General.Menu
{
    using UnityEngine;

    public delegate void CallbackMethod();

    public class MenuOverlay : MonoBehaviour
    {
        private SpriteRenderer m_sprite;
        private BoxCollider2D m_boxcollider;

        public CallbackMethod Callback { private get; set; }

        // Use this for initialization
        void Start()
        {
            m_sprite = GetComponent<SpriteRenderer>();
            m_boxcollider = GetComponent<BoxCollider2D>();
        }

        public void Activate()
        {
            // enable overlay sprite and box collider
            m_sprite.enabled = true;
            m_boxcollider.enabled = true;
        }

        public void SetSprite(Sprite sprite)
        {
            m_sprite.sprite = sprite;
        }

        void OnMouseUpAsButton()
        {
            // disable overlay
            m_sprite.enabled = false;
            m_boxcollider.enabled = false;

            // perform callback if set
            if (Callback != null)
            {
                Callback();
            }
        }
    }
}