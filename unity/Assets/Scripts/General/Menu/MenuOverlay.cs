namespace General.Menu
{
    using UnityEngine;

    public delegate void CallbackMethod();

    /// <summary>
    /// Menu overlay that upon activation shows a sprite, which disactivates upon clicking.
    /// Can set a callback method which is called after a click.
    /// </summary>
    public class MenuOverlay : MonoBehaviour
    {
        private SpriteRenderer m_sprite;
        private BoxCollider2D m_boxcollider;

        public CallbackMethod Callback { private get; set; }

        // Use this for initialization
        void Start()
        {
            // get 
            m_sprite = GetComponent<SpriteRenderer>();
            m_boxcollider = GetComponent<BoxCollider2D>();
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

        /// <summary>
        /// Activates the overlay sprite and box collider
        /// </summary>
        public void Activate()
        {
            m_sprite.enabled = true;
            m_boxcollider.enabled = true;
        }

        /// <summary>
        /// Sets the sprite pointer in the sprite renderer.
        /// </summary>
        /// <param name="sprite"></param>
        public void SetSprite(Sprite sprite)
        {
            m_sprite.sprite = sprite;
        }
    }
}